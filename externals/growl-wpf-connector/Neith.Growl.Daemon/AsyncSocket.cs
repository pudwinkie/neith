using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Net.Security;
using System.Text;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Threading;

namespace Neith.Growl.Daemon
{
    /// <summary>
    /// The AsyncSocket class allows for asynchronous socket activity,
    /// and has usefull methods that allow for controlled reading of a certain length,
    /// or until a specified terminator.
    /// It also has the ability to timeout asynchronous operations, and has several useful events.
    /// </summary>
    public class AsyncSocket
    {
        /// <summary></summary>
        public delegate void SocketDidAccept(AsyncSocket sender, AsyncSocket newSocket);

        /// <summary></summary>
        public delegate bool SocketWillConnect(AsyncSocket sender, Socket socket);

        /// <summary></summary>
        public delegate void SocketDidConnect(AsyncSocket sender, IPAddress address, UInt16 port);

        /// <summary></summary>
        public delegate void SocketDidRead(AsyncSocket sender, byte[] data, long tag);

        /// <summary></summary>
        public delegate void SocketDidReadPartial(AsyncSocket sender, int partialLength, long tag);

        /// <summary></summary>
        public delegate void SocketDidWrite(AsyncSocket sender, long tag);

        /// <summary></summary>
        public delegate void SocketDidWritePartial(AsyncSocket sender, int partialLength, long tag);

        /// <summary></summary>
        public delegate void SocketWillClose(AsyncSocket sender, Exception e);

        /// <summary></summary>
        public delegate void SocketDidClose(AsyncSocket sender);
        // GROWL
        /// <summary></summary>
        public delegate bool SocketDidReadTimeout(AsyncSocket sender);


        /// <summary></summary>
        public event SocketDidAccept DidAccept;

        /// <summary></summary>
        public event SocketWillConnect WillConnect;

        /// <summary></summary>
        public event SocketDidConnect DidConnect;

        /// <summary></summary>
        public event SocketDidRead DidRead;

        /// <summary></summary>
        public event SocketDidReadPartial DidReadPartial;

        /// <summary></summary>
        public event SocketDidWrite DidWrite;

        /// <summary></summary>
        public event SocketDidWritePartial DidWritePartial;

        /// <summary></summary>
        public event SocketWillClose WillClose;

        /// <summary></summary>
        public event SocketDidClose DidClose;
        
        // GROWL
        /// <summary></summary>
        public event SocketDidReadTimeout DidReadTimeout;

        private Socket socket4;
        private Socket socket6;
        private Stream stream;
        private NetworkStream socketStream;
        private SslStream secureSocketStream;

        private const int INIT_READQUEUE_CAPACITY = 5;
        private const int INIT_WRITEQUEUE_CAPACITY = 5;
        private const int INIT_EVENTQUEUE_CAPACITY = 5;

        private const int CONNECTION_QUEUE_CAPACITY = 10;

        private const int READ_CHUNKSIZE = (1024 * 16);
        private const int READALL_CHUNKSIZE = (1024 * 32);
        private const int WRITE_CHUNKSIZE = (1024 * 32);

        private volatile byte flags;
        private const byte kDidPassConnectMethod = 1 << 0;  // If set, disconnection results in delegate call
        private const byte kPauseReads = 1 << 1;  // If set, reads are not dequeued until further notice
        private const byte kPauseWrites = 1 << 2;  // If set, writes are not dequeued until further notice
        private const byte kForbidReadsWrites = 1 << 3;  // If set, no new reads or writes are allowed
        private const byte kCloseAfterReads = 1 << 4;  // If set, disconnect after no more reads are queued
        private const byte kCloseAfterWrites = 1 << 5;  // If set, disconnect after no more writes are queued
        private const byte kClosingWithError = 1 << 6;  // If set, socket is being closed due to an error
        private const byte kClosed = 1 << 7;  // If set, socket is considered closed

        private Queue readQueue;
        private Queue writeQueue;

        private AsyncReadPacket currentRead;
        private AsyncWritePacket currentWrite;

        private System.Threading.Timer connectTimer;
        private System.Threading.Timer readTimer;
        private System.Threading.Timer writeTimer;

        private MutableData readOverflow;

        // We use a seperate lock object instead of locking on 'this'.
        // This is necessary to avoid a tricky deadlock situation.
        // The generated methods that handle += and -= calls to events actually lock on 'this'.
        // So the following is possible:
        // - We invoke one of our OnEventHandler methods from within a lock(this) block.
        // - There is a SynchronizedObject set, and we invoke callbacks on it.
        // - A registered delegate receives the callback on a seperate thread.
        // - The registered delegate then attempts to add a delegate to one of our events.
        // - Deadlock!
        // - The += method is blocking until we finish our lock(this) block.
        // - We won't finish our lock(this) block until the delegate methods complete. 
        private Object lockObj = new Object();

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Utility Classes
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The AsyncReadPacket encompasses the instructions for a read.
        /// The content of a read packet allows the code to determine if we're:
        /// reading to a certain length, reading to a certain separator, or simply reading the first chunk of data.
        /// </summary>
        private class AsyncReadPacket
        {
            public MutableData buffer;
            public int bytesDone;
            public int bytesProcessing;
            public int timeout;
            public int maxLength;
            public long tag;
            public bool readAllAvailableData;
            public bool fixedLengthRead;
            public byte[] term;

            public AsyncReadPacket(MutableData buffer,
                                           int timeout,
                                           int maxLength,
                                          long tag,
                                          bool readAllAvailableData,
                                          bool fixedLengthRead,
                                        byte[] term)
            {
                this.buffer = buffer;
                this.bytesDone = 0;
                this.bytesProcessing = 0;
                this.timeout = timeout;
                this.maxLength = maxLength;
                this.tag = tag;
                this.readAllAvailableData = readAllAvailableData;
                this.fixedLengthRead = fixedLengthRead;
                this.term = term;
            }
        }

        /// <summary>
        /// The AsyncWritePacket encompasses the instructions for a write.
        /// </summary>
        private class AsyncWritePacket
        {
            public byte[] buffer;
            public int offset;
            public int length;
            public int bytesDone;
            public int bytesProcessing;
            public int timeout;
            public long tag;

            public AsyncWritePacket(byte[] buffer,
                                     int offset,
                                     int length,
                                     int timeout,
                                    long tag)
            {
                this.buffer = buffer;
                this.offset = offset;
                this.length = length;
                this.bytesDone = 0;
                this.bytesProcessing = 0;
                this.timeout = timeout;
                this.tag = tag;
            }
        }

        /// <summary>
        /// Encompasses special instructions for interruptions in the read/write queues.
        /// This class my be altered to support more than just TLS in the future.
        /// </summary>
        private class AsyncSpecialPacket
        {
            public bool startTLS;

            public AsyncSpecialPacket(bool startTLS)
            {
                this.startTLS = startTLS;
            }
        }

        private class ConnectParameters
        {
            public String host;
            public UInt16 port;

            public ConnectParameters(String host, UInt16 port)
            {
                this.host = host;
                this.port = port;
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Setup
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public AsyncSocket()
        {
            // Initialize an empty set of flags
            // During execution, various flags are set to allow us to track what's been done
            // and what needs to be done.
            flags = 0;

            // Initialize queues (thread safe)
            readQueue = Queue.Synchronized(new Queue(INIT_READQUEUE_CAPACITY));
            writeQueue = Queue.Synchronized(new Queue(INIT_WRITEQUEUE_CAPACITY));
        }

        private Object mTag;
        /// <summary>
        /// Gets or sets the object that contains data about the socket.
        /// <remarks>
        ///		Any type derived from the Object class can be assigned to this property.
        ///		A common use for the Tag property is to store data that is closely associated with the socket.
        /// </remarks>
        /// </summary>
        public Object Tag
        {
            get { return mTag; }
            set { mTag = value; }
        }

        private Dispatcher synchronizingObject = null;
        /// <summary>
        /// Set the <see cref="System.ComponentModel.ISynchronizeInvoke">ISynchronizeInvoke</see>
        /// object to use as the invoke object. When returning results from asynchronous calls,
        /// the Invoke method on this object will be called to pass the results back
        /// in a thread safe manner.
        /// </summary>
        /// <remarks>
        /// If using in conjunction with a form, it is highly recommended
        /// that you pass your main <see cref="Dispatcher">form</see> (window) in.
        /// </remarks>
        /// <remarks>
        /// You should configure your invoke options before you start reading/writing.
        /// It's recommended you don't change your invoke options in the middle of reading/writing.
        /// </remarks>
        public Dispatcher SynchronizingObject
        {
            get { return synchronizingObject; }
            set { synchronizingObject = value; }
        }

        /// <summary>
        /// ���pDispatcher�B
        /// </summary>
        public Dispatcher EventDispatcher
        {
            get
            {
                if (_EventDispatcher == null) {
                    if (SynchronizingObject != null) _EventDispatcher = SynchronizingObject;
                    else if (allowApplicationForms) _EventDispatcher = GetApplicationDispatcher();
                } return _EventDispatcher;
            }
        }
        private Dispatcher _EventDispatcher = null;

        /// <summary>
        /// ���p�X�P�W���[���B
        /// </summary>
        public IScheduler EventScheduler
        {
            get
            {
                if (_EventScheduler == null) {
                    if (EventDispatcher != null) _EventScheduler = new DispatcherScheduler(EventDispatcher);
                    else if (allowMultithreadedCallbacks) _EventScheduler = Scheduler.TaskPool;
                } return _EventScheduler;
            }
        }
        private IScheduler _EventScheduler = null;

        private bool allowApplicationForms = true;
        /// <summary>
        /// Allows the application to attempt to post async replies over the
        /// application "main loop" by using the message queue of the first available
        /// open form (window). This is retrieved through
        /// 
        /// Note: This is true by default.
        /// </summary>
        /// <remarks>
        /// You should configure your invoke options before you start reading/writing.
        /// It's recommended you don't change your invoke options in the middle of reading/writing.
        /// </remarks>
        public bool AllowApplicationForms
        {
            get { return allowApplicationForms; }
            set { allowApplicationForms = value; }
        }

        private bool allowMultithreadedCallbacks = false;
        /// <summary>
        /// If set to true, <see cref="AllowApplicationForms">AllowApplicationForms</see>
        /// is set to false and <see cref="SynchronizingObject">SynchronizingObject</see> is set
        /// to null. Any time an asynchronous method needs to invoke a delegate method
        /// it will run the method in its own thread.
        /// </summary>
        /// <remarks>
        /// If set to true, you will have to handle any synchronization needed.
        /// If your application uses Windows.Forms or any other non-thread safe
        /// library, then you will have to do your own invoking.
        /// </remarks>
        /// <remarks>
        /// You should configure your invoke options before you start reading/writing.
        /// It's recommended you don't change your invoke options in the middle of reading/writing.
        /// </remarks>
        public bool AllowMultithreadedCallbacks
        {
            get { return allowMultithreadedCallbacks; }
            set
            {
                allowMultithreadedCallbacks = value;
                if (allowMultithreadedCallbacks)
                {
                    allowApplicationForms = false;
                    synchronizingObject = null;
                }
            }
        }

        /// <summary>
        /// �A�N�V�����𓯊����s���܂��B
        /// </summary>
        /// <param name="act"></param>
        /// <returns></returns>
        protected void InvokeAction(Action act)
        {
            if (EventDispatcher != null) EventDispatcher.Invoke(new Action(act));
            else if (EventScheduler != null) {
                var ll = new object();
                Action task = () =>
                {
                    lock (ll) {
                        try { act(); }
                        finally { Monitor.Pulse(ll); }
                    }
                };
                lock (ll) {
                    EventScheduler.Schedule(task);
                    Monitor.Wait(ll);
                }
            }
        }

        /// <summary>
        /// �֐��𓯊����s���܂��B
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        protected T InvokeFunc<T>(Func<T> func)
        {
            T rc = default(T);
            if (EventDispatcher != null) {
                Action task = () => { rc = func(); };
                EventDispatcher.Invoke(new Action(task));
            }
            else if (EventScheduler != null) {
                var ll = new object();
                Action task = () =>
                {
                    lock (ll) {
                        try { rc = func(); }
                        finally { Monitor.Pulse(ll); }
                    }
                };
                lock (ll) {
                    EventScheduler.Schedule(task);
                    Monitor.Wait(ll);
                }
            }
            return rc;
        }

        /// <summary>
        /// �A�N�V������񓯊����s���܂��B
        /// </summary>
        /// <param name="act"></param>
        protected IDisposable BeginInvokeAction(Action act)
        {
            if (EventScheduler == null) return null;
            return EventScheduler.Schedule(act);
        }

        // What is going on with the event handler methods below?
        // 
        // The asynchronous nature of this class means that we're very multithreaded.
        // But the client may not be. The client may be using a SynchronizingObject,
        // or has requested we use application forms for invoking.
        //
        // A problem arises from this situation:
        // If a client calls the Disconnect method, then he/she does NOT
        // expect to receive any other delegate methods after the
        // call to Diconnect completes.
        // 
        // Primitive invoking from a background thread will not solve this problem.
        // So what we do instead is invoke into the same thread as the client,
        // then check to make sure the socket hasn't been closed,
        // and then execute the delegate method.

        /// <summary></summary>
        protected virtual void OnSocketDidAccept(AsyncSocket newSocket)
        {
            if (DidAccept == null) return;
            InvokeAction(() => DoDidAccept(newSocket));
        }

        /// <summary></summary>
        protected virtual bool OnSocketWillConnect(Socket socket)
        {
            if (WillConnect == null) return true;
            return InvokeFunc(() => DoWillConnect(socket));
        }

        /// <summary></summary>
        protected virtual void OnSocketDidConnect(IPAddress address, UInt16 port)
        {
            if (DidConnect == null) return;
            BeginInvokeAction(() => DoDidConnect(address, port));
        }

        /// <summary></summary>
        protected virtual void OnSocketDidRead(byte[] data, long tag)
        {
            if (DidRead == null) return;
            BeginInvokeAction(() => DoDidRead(data, tag));
        }

        /// <summary></summary>
        protected virtual void OnSocketDidReadPartial(int partialLength, long tag)
        {
            if (DidReadPartial == null) return;
            BeginInvokeAction(() => DoDidReadPartial(partialLength, tag));
        }

        /// <summary></summary>
        protected virtual void OnSocketDidWrite(long tag)
        {
            if (DidWrite == null) return;
            BeginInvokeAction(() => DoDidWrite( tag));
        }

        /// <summary></summary>
        protected virtual void OnSocketDidWritePartial(int partialLength, long tag)
        {
            if (DidWritePartial == null) return;
            BeginInvokeAction(() => DoDidWritePartial(partialLength, tag));
        }

        /// <summary></summary>
        protected virtual void OnSocketWillClose(Exception e)
        {
            if (WillClose == null) return;
            InvokeAction(() => DoWillClose(e));
        }

        // GROWL
        private delegate bool DoDidReadTimeoutDelegate(AsyncSocket socket);
        /// <summary></summary>
        protected virtual bool OnSocketDidReadTimeout()
        {
            if (DidReadTimeout == null) return false;
            return InvokeFunc(() => DidReadTimeout(this));
        }

        /// <summary></summary>
        protected virtual void OnSocketDidClose()
        {
            if (DidClose == null) return;
            InvokeAction(() => DoDidClose());
        }

        private delegate void DoDidAcceptDelegate(AsyncSocket newSocket);
        private void DoDidAccept(AsyncSocket newSocket)
        {
            // Threading Notes:
            // This method is called when using a SynchronizingObject or AppForms,
            // so method is executed on the same thread that the delegate is using.
            // Thus, the kClosed flag prevents any callbacks after the delegate calls the close method.

            if ((flags & kClosed) != 0) return;

            try
            {
                if (DidAccept != null)
                {
                    DidAccept(this, newSocket);
                }
            }
            catch { }
        }

        private delegate bool DoWillConnectDelegate(Socket socket);
        private bool DoWillConnect(Socket socket)
        {
            // Threading Notes:
            // This method is called when using a SynchronizingObject or AppForms,
            // so method is executed on the same thread that the delegate is using.
            // Thus, the kClosed flag prevents any callbacks after the delegate calls the close method.

            if ((flags & kClosed) != 0) return false;

            try
            {
                if (WillConnect != null)
                {
                    return WillConnect(this, socket);
                }
            }
            catch { }

            return true;
        }

        private delegate void DoDidConnectDelegate(IPAddress address, UInt16 port);
        private void DoDidConnect(IPAddress address, UInt16 port)
        {
            // Threading Notes:
            // This method is called when using a SynchronizingObject or AppForms,
            // so method is executed on the same thread that the delegate is using.
            // Thus, the kClosed flag prevents any callbacks after the delegate calls the close method.

            if ((flags & kClosed) != 0) return;

            try
            {
                if (DidConnect != null)
                {
                    DidConnect(this, address, port);
                }
            }
            catch { }
        }

        private delegate void DoDidReadDelegate(byte[] data, long tag);
        private void DoDidRead(byte[] data, long tag)
        {
            // Threading Notes:
            // This method is called when using a SynchronizingObject or AppForms,
            // so method is executed on the same thread that the delegate is using.
            // Thus, the kClosed flag prevents any callbacks after the delegate calls the close method.

            if ((flags & kClosed) != 0) return;

            try
            {
                if (DidRead != null)
                {
                    DidRead(this, data, tag);
                }
            }
            catch { }
        }

        private delegate void DoDidReadPartialDelegate(int partialLength, long tag);
        private void DoDidReadPartial(int partialLength, long tag)
        {
            // Threading Notes:
            // This method is called when using a SynchronizingObject or AppForms,
            // so method is executed on the same thread that the delegate is using.
            // Thus, the kClosed flag prevents any callbacks after the delegate calls the close method.

            if ((flags & kClosed) != 0) return;

            try
            {
                if (DidReadPartial != null)
                {
                    DidReadPartial(this, partialLength, tag);
                }
            }
            catch { }
        }

        private delegate void DoDidWriteDelegate(long tag);
        private void DoDidWrite(long tag)
        {
            // Threading Notes:
            // This method is called when using a SynchronizingObject or AppForms,
            // so method is executed on the same thread that the delegate is using.
            // Thus, the kClosed flag prevents any callbacks after the delegate calls the close method.

            if ((flags & kClosed) != 0) return;

            try
            {
                if (DidWrite != null)
                {
                    DidWrite(this, tag);
                }
            }
            catch { }
        }

        private delegate void DoDidWritePartialDelegate(int partialLength, long tag);
        private void DoDidWritePartial(int partialLength, long tag)
        {
            // Threading Notes:
            // This method is called when using a SynchronizingObject or AppForms,
            // so method is executed on the same thread that the delegate is using.
            // Thus, the kClosed flag prevents any callbacks after the delegate calls the close method.

            if ((flags & kClosed) != 0) return;

            try
            {
                if (DidWritePartial != null)
                {
                    DidWritePartial(this, partialLength, tag);
                }
            }
            catch { }
        }

        private delegate void DoWillCloseDelegate(Exception e);
        private void DoWillClose(Exception e)
        {
            // Threading Notes:
            // This method is called when using a SynchronizingObject or AppForms,
            // so method is executed on the same thread that the delegate is using.
            // Thus, the kClosed flag prevents any callbacks after the delegate calls the close method.

            if ((flags & kClosed) != 0) return;

            try
            {
                if (WillClose != null)
                {
                    WillClose(this, e);
                }
            }
            catch { }
        }

        private delegate void DoDidCloseDelegate();
        private void DoDidClose()
        {
            // Threading Notes:
            // This method is called when using a SynchronizingObject or AppForms,
            // so method is executed on the same thread that the delegate is using.
            // Thus, the kClosed flag prevents any callbacks after the delegate calls the close method.

            try
            {
                if (DidClose != null)
                {
                    DidClose(this);
                }
            }
            catch { }
        }


        /// <summary>
        /// Returns a form that can be used to invoke an event.
        /// </summary>
        private Dispatcher GetApplicationDispatcher()
        {
            return System.Windows.Application.Current.Dispatcher;
        }

        /// <summary>
        /// Allows invoke options to be inherited from another AsyncSocket.
        /// This is usefull when accepting connections.
        /// </summary>
        /// <param name="fromSocket">
        ///		AsyncSocket object to copy invoke options from.
        ///	</param>
        protected void InheritInvokeOptions(AsyncSocket fromSocket)
        {
            // We set the MultiThreadedCallback property first,
            // as it has the potential to affect the other properties.
            AllowMultithreadedCallbacks = fromSocket.AllowMultithreadedCallbacks;

            AllowApplicationForms = fromSocket.AllowApplicationForms;
            SynchronizingObject = fromSocket.SynchronizingObject;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Progress
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary></summary>
        public float ProgressOfCurrentRead()
        {
            long tag;
            int bytesDone;
            int total;

            float result = ProgressOfCurrentRead(out tag, out bytesDone, out total);
            return result;
        }

        /// <summary></summary>
        public float ProgressOfCurrentRead(out long tag, out int bytesDone, out int total)
        {
            // First get a reference to the current read.
            // We do this because the currentRead pointer could be changed in a separate thread.
            // And locking should not be done in this method
            // because it's public, and could potentially cause deadlock.
            AsyncReadPacket thisRead = null;
            Interlocked.Exchange(ref thisRead, currentRead);

            // Check to make sure we're actually reading something right now
            if (thisRead == null)
            {
                tag = ((long)0);
                bytesDone = 0;
                total = 0;
                return float.NaN;
            }

            // It's only possible to know the progress of our read if we're reading to a certain length
            // If we're reading to data, we of course have no idea when the data will arrive
            // If we're reading to timeout, then we have no idea when the next chunk of data will arrive.
            bool hasTotal = thisRead.fixedLengthRead;

            tag = thisRead.tag;
            bytesDone = thisRead.bytesDone;
            total = hasTotal ? thisRead.buffer.Length : 0;

            if (total > 0)
                return (((float)bytesDone) / ((float)total));
            else
                return 1.0f;
        }

        /// <summary></summary>
        public float ProgressOfCurrentWrite()
        {
            long tag;
            int bytesDone;
            int total;

            float result = ProgressOfCurrentWrite(out tag, out bytesDone, out total);
            return result;
        }

        /// <summary></summary>
        public float ProgressOfCurrentWrite(out long tag, out int bytesDone, out int total)
        {
            // First get a reference to the current write.
            // We do this because the currentWrite pointer could be changed in a separate thread.
            // And locking should not be done in this method
            // because it's public, and could potentially cause deadlock.
            AsyncWritePacket thisWrite = null;
            Interlocked.Exchange(ref thisWrite, currentWrite);

            // Check to make sure we're actually writing something right now
            if (thisWrite == null)
            {
                tag = ((long)0);
                bytesDone = 0;
                total = 0;
                return float.NaN;
            }

            tag = thisWrite.tag;
            bytesDone = thisWrite.bytesDone;
            total = thisWrite.buffer.Length;

            return (((float)bytesDone) / ((float)total));
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Accepting
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Tells the socket to begin accepting connections on the given port.
        /// The socket will listen on all interfaces.
        /// Be sure to register to receive DidAccept events.
        /// </summary>
        /// <param name="port">
        ///		The port to accept connections on. Pass 0 to allow the OS to pick any available port.
        /// </param>
        /// <returns>
        ///		True if the socket was able to begin listening for connections on the given address and port.
        ///		False otherwise.  If false consult the error parameter for more information.
        /// </returns>
        public bool Accept(UInt16 port)
        {
            Exception error;
            return Accept(null, port, out error);
        }

        /// <summary>
        /// Tells the socket to begin accepting connections on the given port.
        /// The socket will listen on all interfaces.
        /// Be sure to register to receive DidAccept events.
        /// </summary>
        /// <param name="port">
        ///		The port to accept connections on. Pass 0 to allow the OS to pick any available port.
        /// </param>
        /// <param name="error">
        ///		If this method returns false, the error will contain the reason for it's failure.
        /// </param>
        /// <returns>
        ///		True if the socket was able to begin listening for connections on the given address and port.
        ///		False otherwise.  If false consult the error parameter for more information.
        /// </returns>
        public bool Accept(UInt16 port, out Exception error)
        {
            return Accept(null, port, out error);
        }

        /// <summary>
        /// Tells the socket to begin accepting connections on the given address and port.
        /// Be sure to register to receive DidAccept events.
        /// </summary>
        /// <param name="hostaddr">
        ///		A string that contains an IP address in dotted-quad notation for IPv4
        ///		or in colon-hexadecimal notation for IPv6.
        ///		For convenience, you may also pass the strings "loopback" or "localhost".
        /// </param>
        /// <param name="port">
        ///		The port to accept connections on. Pass 0 to allow the OS to pick any available port.
        /// </param>
        /// <returns>
        ///		True if the socket was able to begin listening for connections on the given address and port.
        ///		False otherwise.  If false consult the error parameter for more information.
        /// </returns>
        public bool Accept(String hostaddr, UInt16 port)
        {
            Exception error;
            return Accept(hostaddr, port, out error);
        }

        /// <summary>
        /// Tells the socket to begin accepting connections on the given address and port.
        /// Be sure to register to receive DidAccept events.
        /// </summary>
        /// <param name="hostaddr">
        ///		A string that contains an IP address in dotted-quad notation for IPv4
        ///		or in colon-hexadecimal notation for IPv6.
        ///		For convenience, you may also pass the strings "loopback" or "localhost".
        ///		Pass null to listen on all interfaces.
        /// </param>
        /// <param name="port">
        ///		The port to accept connections on. Pass 0 to allow the OS to pick any available port.
        /// </param>
        /// <param name="error">
        ///		If this method returns false, the error will contain the reason for it's failure.
        /// </param>
        /// <returns>
        ///		True if the socket was able to begin listening for connections on the given address and port.
        ///		False otherwise.  If false consult the error parameter for more information.
        /// </returns>
        public bool Accept(String hostaddr, UInt16 port, out Exception error)
        {
            error = null;

            // Make sure we're not closed
            if ((flags & kClosed) != 0)
            {
                String msg = "Socket is closed.";
                error = new Exception(msg);
                return false;
            }

            // Make sure we're not already listening for connections, or already connected
            if ((flags & kDidPassConnectMethod) != 0)
            {
                String msg = "Attempting to connect while connected or accepting connections.";
                error = new Exception(msg);
                return false;
            }

            // Extract proper IPAddress(es) from the given hostaddr
            IPAddress address4 = null;
            IPAddress address6 = null;

            if (hostaddr == null)
            {
                address4 = IPAddress.Any;
                address6 = IPAddress.IPv6Any;
            }
            else
            {
                if (hostaddr.Equals("loopback") || hostaddr.Equals("localhost"))
                {
                    address4 = IPAddress.Loopback;
                    address6 = IPAddress.IPv6Loopback;
                }
                else
                {
                    try
                    {
                        IPAddress addr = IPAddress.Parse(hostaddr);
                        if (addr.AddressFamily == AddressFamily.InterNetwork)
                        {
                            address4 = addr;
                            address6 = null;
                        }
                        else if (addr.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            address4 = null;
                            address6 = addr;
                        }
                    }
                    catch (Exception e)
                    {
                        error = e;
                        return false;
                    }

                    if ((address4 == null) && (address6 == null))
                    {
                        String msg = String.Format("hostaddr ({0}) is not a valid IPv4 or IPv6 address", hostaddr);
                        error = new Exception(msg);
                        return false;
                    }
                }
            }

            // Watch out for versions of XP that don't support IPv6
            if (!Socket.OSSupportsIPv6)
            {
                if (address4 == null)
                {
                    error = new Exception("Requesting IPv6, but OS does not support it.");
                    return false;
                }
                address6 = null;
            }

            // Attention: Lock within public method.
            // Note: Should be fine since we can only get this far if the socket is null.
            lock (lockObj)
            {
                try
                {
                    // Initialize socket(s)
                    if (address4 != null)
                    {
                        // Initialize socket
                        socket4 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                        // Always reuse address
                        socket4.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

                        // Bind the socket to the proper address/port
                        socket4.Bind(new IPEndPoint(address4, port));

                        // Start listening (using the preset max pending connection queue size)
                        socket4.Listen(CONNECTION_QUEUE_CAPACITY);

                        // Start accepting connections
                        socket4.BeginAccept(new AsyncCallback(socket_DidAccept), socket4);
                    }
                    if (address6 != null)
                    {
                        // Initialize socket
                        socket6 = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);

                        // Always reuse address
                        socket6.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

                        // Bind the socket to the proper address/port
                        socket6.Bind(new IPEndPoint(address6, port));

                        // Start listening (using the preset max pending connection queue size)
                        socket6.Listen(CONNECTION_QUEUE_CAPACITY);

                        // Start accepting connections
                        socket6.BeginAccept(new AsyncCallback(socket_DidAccept), socket6);
                    }
                }
                catch (Exception e)
                {
                    error = e;
                    if (socket4 != null)
                    {
                        socket4.Close();
                        socket4 = null;
                    }
                    if (socket6 != null)
                    {
                        socket6.Close();
                        socket6 = null;
                    }
                    return false;
                }

                flags |= kDidPassConnectMethod;
            }

            return true;
        }

        /// <summary>
        /// Description forthcoming
        /// </summary>
        /// <param name="iar"></param>
        private void socket_DidAccept(IAsyncResult iar)
        {
            lock (lockObj)
            {
                if ((flags & kClosed) > 0) return;

                try
                {
                    Socket socket = (Socket)iar.AsyncState;

                    Socket newSocket = socket.EndAccept(iar);
                    AsyncSocket newAsyncSocket = new AsyncSocket();

                    newAsyncSocket.InheritInvokeOptions(this);
                    newAsyncSocket.PreConfigure(newSocket);

                    OnSocketDidAccept(newAsyncSocket);

                    newAsyncSocket.PostConfigure();

                    // And listen for more connections
                    socket.BeginAccept(new AsyncCallback(socket_DidAccept), socket);
                }
                catch (Exception e)
                {
                    CloseWithException(e);
                }
            }
        }

        /// <summary>
        /// Called to configure an AsyncSocket after an accept has occured.
        /// This is called before OnSocketDidAccept.
        /// </summary>
        /// <param name="socket">
        ///		The newly accepted socket.
        /// </param>
        private void PreConfigure(Socket socket)
        {
            // Store socket
            if (socket.AddressFamily == AddressFamily.InterNetwork)
            {
                this.socket4 = socket;
            }
            else
            {
                this.socket6 = socket;
            }

            // Create NetworkStream from new socket
            socketStream = new NetworkStream(socket);
            stream = socketStream;
            flags |= kDidPassConnectMethod;
        }

        /// <summary>
        /// Called to configure an AsyncSocket after an accept has occured.
        /// This is called after OnSocketDidAccept.
        /// </summary>
        private void PostConfigure()
        {
            // Notify the delegate
            OnSocketDidConnect(RemoteAddress, RemotePort);

            // Immediately deal with any already-queued requests.
            // Notice that we delay the call to allow execution in socket_DidAccept().
            ThreadPool.QueueUserWorkItem(new WaitCallback(MaybeDequeueRead));
            ThreadPool.QueueUserWorkItem(new WaitCallback(MaybeDequeueWrite));
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Connecting
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Begins an asynchronous connection attempt to the specified host and port.
        /// Returns false if the connection attempt immediately fails.
        /// If this method succeeds, the delegate will be informed of the
        /// connection success/failure via the proper delegate methods.
        /// </summary>
        /// <param name="host">
        ///		The host name or IP address to connect to.
        ///		E.g. "deusty.com" or "70.85.193.226" or "2002:cd9:3ea8:0:88c8:b211:b605:ab59"
        /// </param>
        /// <param name="port">
        ///		The port to connect to (eg. 80)
        /// </param>
        /// <returns>
        /// 	True if the socket was able to begin attempting to connect to the given host and port.
        ///		False otherwise.
        /// </returns>
        public bool Connect(String host, UInt16 port)
        {
            Exception error;
            return Connect(host, port, out error);
        }

        /// <summary>
        /// Begins an asynchronous connection attempt to the specified host and port.
        /// Returns false if the connection attempt immediately fails.
        /// If this method succeeds, the delegate will be informed of the
        /// connection success/failure via the proper delegate methods.
        /// </summary>
        /// <param name="host">
        ///		The host name or IP address to connect to.
        ///		E.g. "deusty.com" or "70.85.193.226" or "2002:cd9:3ea8:0:88c8:b211:b605:ab59"
        /// </param>
        /// <param name="port">
        ///		The port to connect to (eg. 80)
        /// </param>
        /// <param name="timeout">
        ///		Timeout in milliseconds. Specify a negative value if no timeout is desired.
        /// </param>
        /// <returns>
        ///		True if the socket was able to begin attempting to connect to the given host and port.
        ///		False otherwise.
        ///	</returns>
        public bool Connect(String host, UInt16 port, int timeout)
        {
            Exception error;
            return Connect(host, port, timeout, out error);
        }

        /// <summary>
        /// Begins an asynchronous connection attempt to the specified host and port.
        /// Returns false if the connection attempt immediately failed, in which case the error parameter will be set.
        /// If this method succeeds, the delegate will be informed of the
        /// connection success/failure via the proper delegate methods.
        /// </summary>
        /// <param name="host">
        ///		The host name or IP address to connect to.
        ///		E.g. "deusty.com" or "70.85.193.226" or "2002:cd9:3ea8:0:88c8:b211:b605:ab59"
        /// </param>
        /// <param name="port">
        ///		The port to connect to (eg. 80)
        /// </param>
        /// <param name="error">
        ///		If this method returns false, the error will contain the reason for it's failure.
        /// </param>
        /// <returns>
        /// 	True if the socket was able to begin attempting to connect to the given host and port.
        ///		False otherwise.  If false consult the error parameter for more information.
        /// </returns>
        public bool Connect(String host, UInt16 port, out Exception error)
        {
            return Connect(host, port, -1, out error);
        }

        /// <summary>
        /// Begins an asynchronous connection attempt to the specified host and port.
        /// Returns false if the connection attempt immediately failed, in which case the error parameter will be set.
        /// If this method succeeds, the delegate will be informed of the
        /// connection success/failure via the proper delegate methods.
        /// </summary>
        /// <param name="host">
        ///		The host name or IP address to connect to.
        ///		E.g. "deusty.com" or "70.85.193.226" or "2002:cd9:3ea8:0:88c8:b211:b605:ab59"
        /// </param>
        /// <param name="port">
        ///		The port to connect to (eg. 80)
        /// </param>
        /// <param name="timeout">
        ///		Timeout in milliseconds. Specify a negative value if no timeout is desired.
        /// </param>
        /// <param name="error">
        ///		If this method returns false, the error will contain the reason for it's failure.
        /// </param>
        /// <returns>
        ///		True if the socket was able to begin attempting to connect to the given host and port.
        ///		False otherwise.  If false consult the error parameter for more information.
        ///	</returns>
        public bool Connect(String host, UInt16 port, int timeout, out Exception error)
        {
            error = null;

            // Make sure we're not closed
            if ((flags & kClosed) != 0)
            {
                String msg = "Socket is closed.";
                error = new Exception(msg);
                return false;
            }

            // Make sure we're not already connected, or listening for connections
            if ((flags & kDidPassConnectMethod) > 0)
            {
                String e = "Attempting to connect while connected or accepting connections.";
                error = new Exception(e);
                return false;
            }

            // Attention: Lock within public method.
            // Note: Should be fine since we can only get this far if the socket is null.
            lock (lockObj)
            {
                try
                {
                    // We're about to start resolving the host name asynchronously.
                    ConnectParameters parameters = new ConnectParameters(host, port);

                    // Start time-out timer
                    if (timeout >= 0)
                    {
                        connectTimer = new System.Threading.Timer(new TimerCallback(socket_DidNotConnect),
                                                                  parameters,
                                                                  timeout,
                                                                  Timeout.Infinite);
                    }

                    // Start resolving the host
                    Dns.BeginGetHostAddresses(host, new AsyncCallback(Dns_DidResolve), parameters);
                }
                catch (Exception e)
                {
                    error = e;
                    return false;
                }

                flags |= kDidPassConnectMethod;
            }

            return true;
        }

        /// <summary>
        /// Callback method when dns has resolved the host (or was unable to resolve it).
        /// 
        /// This method is thread safe.
        /// </summary>
        /// <param name="iar">
        ///		The state of the IAsyncResult refers to the ConnectRequest object
        ///		containing the parameters of the original call to the Connect() method.
        /// </param>
        private void Dns_DidResolve(IAsyncResult iar)
        {
            ConnectParameters parameters = (ConnectParameters)iar.AsyncState;

            lock (lockObj)
            {
                // Check to make sure the async socket hasn't been closed.
                if ((flags & kClosed) > 0)
                {
                    // We no longer need the result of the dns query.
                    // Properly end the async procedure, but ignore the result.
                    try
                    {
                        Dns.EndGetHostAddresses(iar);
                    }
                    catch { }

                    return;
                }

                IPAddress[] addresses = null;

                bool done = false;
                bool cancelled = false;

                try
                {
                    addresses = Dns.EndGetHostAddresses(iar);

                    for (int i = 0; i < addresses.Length && !done && !cancelled; i++)
                    {
                        IPAddress address = addresses[i];

                        if (address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            // Initialize a new socket
                            socket4 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                            // Allow delegate to configure the socket if needed
                            if (OnSocketWillConnect(socket4))
                            {
                                // Attempt to connect with the given information
                                socket4.BeginConnect(address, parameters.port, new AsyncCallback(socket_DidConnect), socket4);

                                // Stop looping through addresses
                                done = true;
                            }
                            else
                            {
                                cancelled = true;
                            }
                        }
                        else if (address.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            if (Socket.OSSupportsIPv6)
                            {
                                // Initialize a new socket
                                socket6 = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);

                                // Allow delegate to configure the socket if needed
                                if (OnSocketWillConnect(socket6))
                                {
                                    // Attempt to connect with the given information
                                    socket6.BeginConnect(address, parameters.port, new AsyncCallback(socket_DidConnect), socket6);

                                    // Stop looping through addresses
                                    done = true;
                                }
                                else
                                {
                                    cancelled = true;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    CloseWithException(e);
                }

                if ((addresses == null) || (addresses.Length == 0))
                {
                    String msg = String.Format("Unable to resolve host \"{0}\"", parameters.host);
                    CloseWithException(new Exception(msg));
                }

                if (cancelled)
                {
                    String msg = "Connection attempt cancelled in WillConnect delegate";
                    CloseWithException(new Exception(msg));
                }

                if (!done)
                {
                    String format = "Unable to resolve host \"{0}\" to valid IPv4 or IPv6 address";
                    String msg = String.Format(format, parameters.host);
                    CloseWithException(new Exception(msg));
                }
            }
        }

        /// <summary>
        /// Callback method when socket has connected (or was unable to connect).
        /// 
        /// This method is thread safe.
        /// </summary>
        /// <param name="iar">
        ///		The state of the IAsyncResult refers to the socket that called BeginConnect().
        /// </param>
        private void socket_DidConnect(IAsyncResult iar)
        {
            // We lock in this method to ensure that the SocketDidConnect delegate fires before
            // processing any reads or writes. ScheduledDequeue methods may be lurking.
            // Also this ensures the flags are properly updated prior to any other locked method executing.
            lock (lockObj)
            {
                if ((flags & kClosed) > 0) return;

                try
                {
                    Socket socket = (Socket)iar.AsyncState;

                    socket.EndConnect(iar);

                    socketStream = new NetworkStream(socket);
                    stream = socketStream;

                    // Notify the delegate
                    OnSocketDidConnect(RemoteAddress, RemotePort);

                    // Cancel the connect timer
                    if (connectTimer != null)
                    {
                        connectTimer.Dispose();
                        connectTimer = null;
                    }

                    // Immediately deal with any already-queued requests.
                    MaybeDequeueRead(null);
                    MaybeDequeueWrite(null);
                }
                catch (Exception e)
                {
                    CloseWithException(e);
                }
            }
        }

        /// <summary>
        /// Called after a connect timeout timer fires.
        /// This will fire on an available thread from the thread pool.
        /// 
        /// This method is thread safe.
        /// </summary>
        private void socket_DidNotConnect(object ignore)
        {
            lock (lockObj)
            {
                if ((flags & kClosed) > 0) return;

                // The timer may have fired in the middle of the socket_DidConnect method above.
                // In this case, the lock would have prevented both methods from running at the same time.
                // Check to make sure we still don't have a socketStream, because if we do then we've connected.
                if (socketStream == null)
                {
                    CloseWithException(GetConnectTimeoutException());
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Disconnecting
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Fires the WillDisconnect event, and then closes the socket.
        /// 
        /// This method is NOT thread safe, and should only be invoked via thread safe methods.
        /// </summary>
        /// <param name="e">
        /// 	The exception that occurred, to be sent to the client.
        /// </param>
        private void CloseWithException(Exception e)
        {
            flags |= kClosingWithError;

            if ((flags & kDidPassConnectMethod) > 0)
            {
                // Try to salvage what data we can
                RecoverUnreadData();

                // Let the delegate know, so it can try to recover if it likes.
                OnSocketWillClose(e);
            }

            Close(null);
        }

        /// <summary>
        /// This method extracts any unprocessed data, and makes it available to the client.
        /// 
        /// Called solely from CloseWithException, which is only called from thread safe methods.
        /// </summary>
        private void RecoverUnreadData()
        {
            if (currentRead != null)
            {
                // We never finished the current read.

                int bytesAvailable = currentRead.bytesDone + currentRead.bytesProcessing;

                if (readOverflow == null)
                {
                    readOverflow = new MutableData(currentRead.buffer, 0, bytesAvailable);
                }
                else
                {
                    // We need to move the data into the front of the read overflow
                    readOverflow.InsertData(0, currentRead.buffer, 0, bytesAvailable);
                }
            }
        }

        /// <summary>
        /// Clears the read and writes queues.
        /// Remember that the queues are synchronized/thread-safe.
        /// </summary>
        private void EmptyQueues()
        {
            if (currentRead != null) EndCurrentRead();
            if (currentWrite != null) EndCurrentWrite();

            readQueue.Clear();
            writeQueue.Clear();
        }

        /// <summary>
        /// Drops pending reads and writes, closes all sockets and stream, and notifies delegate if needed.
        /// </summary>
        private void Close(object ignore)
        {
            lock (lockObj)
            {
                EmptyQueues();

                if (secureSocketStream != null)
                {
                    secureSocketStream.Close();
                    secureSocketStream = null;
                }
                if (socketStream != null)
                {
                    socketStream.Close();
                    socketStream = null;
                }
                if (stream != null)
                {
                    // Stream is just a pointer to the real stream we're using
                    // I.e. it points to either socketStream of secureSocketStream
                    // Thus we don't close it
                    stream = null;
                }
                if (socket6 != null)
                {
                    socket6.Close();
                    socket6 = null;
                }
                if (socket4 != null)
                {
                    socket4.Close();
                    socket4 = null;
                }

                if (connectTimer != null)
                {
                    connectTimer.Dispose();
                    connectTimer = null;
                }

                // The readTimer and writeTimer are cleared in the EmptyQueues method above.

                // Clear flags to signal closed socket
                flags = (kForbidReadsWrites | kClosed);

                // Notify delegate that we're now disconnected
                OnSocketDidClose();

                /* GFW - change to always call OnSocketDidClose
                if ((flags & kDidPassConnectMethod) > 0)
                {
                    // Clear flags to signal closed socket
                    flags = (kForbidReadsWrites | kClosed);

                    // Notify delegate that we're now disconnected
                    OnSocketDidClose();
                }
                else
                {
                    // Clear flags to signal closed socket
                    flags = (kForbidReadsWrites | kClosed);
                }
                 * */
            }
        }

        /// <summary>
        /// Immediately stops all transfers, and releases any socket and stream resources.
        /// Any pending reads or writes are dropped.
        /// 
        /// If the socket is already closed, this method does nothing.
        /// 
        /// Note: The SocketDidClose method will be called.
        /// </summary>
        public void Close()
        {
            flags |= kClosed;

            ThreadPool.QueueUserWorkItem(new WaitCallback(Close), null);
        }

        /// <summary>
        /// Closes the socket after all pending reads have completed.
        /// After calling this, the read and write methods will do nothing.
        /// The socket will close even if there are still pending writes.
        /// </summary>
        public void CloseAfterReading()
        {
            flags |= (kForbidReadsWrites | kCloseAfterReads);

            // Queue a call to MaybeClose
            ThreadPool.QueueUserWorkItem(new WaitCallback(MaybeClose));
        }

        /// <summary>
        /// Closes after all pending writes have completed.
        /// After calling this, the read and write methods will do nothing.
        /// The socket will close even if there are still pending reads.
        /// </summary>
        public void CloseAfterWriting()
        {
            flags |= (kForbidReadsWrites | kCloseAfterWrites);

            // Queue a call to MaybeClose
            ThreadPool.QueueUserWorkItem(new WaitCallback(MaybeClose));
        }

        /// <summary>
        /// Closes after all pending reads and writes have completed.
        /// After calling this, the read and write methods will do nothing.
        /// </summary>
        public void CloseAfterReadingAndWriting()
        {
            flags |= (kForbidReadsWrites | kCloseAfterReads | kCloseAfterWrites);

            // Queue a call to MaybeClose
            ThreadPool.QueueUserWorkItem(new WaitCallback(MaybeClose));
        }

        /// <summary></summary>
        public void CloseImmediately()
        {
            Close(null);
        }

        private void MaybeClose(object ignore)
        {
            lock (lockObj)
            {
                if ((flags & kCloseAfterReads) > 0)
                {
                    if ((readQueue.Count == 0) && (currentRead == null))
                    {
                        if ((flags & kCloseAfterWrites) > 0)
                        {
                            if ((writeQueue.Count == 0) && (currentWrite == null))
                            {
                                Close(null);
                            }
                        }
                        else
                        {
                            Close(null);
                        }
                    }
                }
                else if ((flags & kCloseAfterWrites) > 0)
                {
                    if ((writeQueue.Count == 0) && (currentWrite == null))
                    {
                        Close(null);
                    }
                }
            }
        }

        /// <summary></summary>
        public void Shutdown(SocketShutdown how)
        {
            if (socket6 != null)
            {
                socket6.Shutdown(how);
            }
            if (socket4 != null)
            {
                socket4.Shutdown(how);
            }
        }

        /// <summary>
        /// In the event of an error, this method may be called during SocketWillClose
        /// to read any data that's left on the socket.
        /// </summary>
        public byte[] GetUnreadData()
        {
            // Ensure this method will only return data in the event of an error
            if ((flags & kClosingWithError) == 0) return null;

            if (readOverflow == null)
                return new byte[0];
            else
                return readOverflow.ByteArray;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Errors
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private Exception GetEndOfStreamException()
        {
            return new Exception("Socket reached end of stream.");
        }

        private Exception GetConnectTimeoutException()
        {
            return new Exception("Connect operation timed out.");
        }

        private Exception GetReadTimeoutException()
        {
            return new Exception("Read operation timed out.");
        }

        private Exception GetWriteTimeoutException()
        {
            return new Exception("Write operation timed out.");
        }

        private Exception GetReadMaxedOutException()
        {
            return new Exception("Read operation reached set maximum length");
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Diagnostics
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The Connected property gets the connection state of the Socket as of the last I/O operation.
        /// When it returns false, the Socket was either never connected, or is no longer connected.
        /// 
        /// Note that this functionallity matches normal Socket.Connected functionallity.
        /// </summary>
        public bool Connected
        {
            get
            {
                if (socket4 != null)
                    return socket4.Connected;
                else
                    return ((socket6 != null) && (socket6.Connected));
            }
        }

        /// <summary>
        /// Non-retarded method of Connected.
        /// Returns the logical answer to the question "Is this socket connected."
        /// </summary>
        public bool SmartConnected
        {
            get
            {
                if (socket4 != null)
                    return GetIsSmartConnected(socket4);
                else
                    return GetIsSmartConnected(socket6);
            }
        }

        private bool GetIsSmartConnected(Socket socket)
        {
            bool connected = false;
            if (socket != null && socket.Connected)
            {
                connected = socket.Connected;
                bool blockingState = socket.Blocking;
                try
                {
                    byte[] tmp = new byte[0];

                    socket.Blocking = false;
                    int x = socket.Send(tmp, 0, 0);
                    if (x == 0) connected = false;
                }
                catch (SocketException e)
                {
                    // 10035 == WSAEWOULDBLOCK
                    if (e.NativeErrorCode == 10035)
                    {
                        // Still Connected, but the Send would block
                    }
                    else
                    {
                        // Disconnected
                        connected = false;
                    }
                }
                finally
                {
                    socket.Blocking = blockingState;
                }
            }

            return connected;
        }

        /// <summary></summary>
        public IPAddress RemoteAddress
        {
            get
            {
                if (socket4 != null)
                    return GetRemoteAddress(socket4);
                else
                    return GetRemoteAddress(socket6);
            }
        }

        /// <summary></summary>
        public UInt16 RemotePort
        {
            get
            {
                if (socket4 != null)
                    return GetRemotePort(socket4);
                else
                    return GetRemotePort(socket6);
            }
        }

        /// <summary></summary>
        public IPAddress LocalAddress
        {
            get
            {
                if (socket4 != null)
                    return GetLocalAddress(socket4);
                else
                    return GetLocalAddress(socket6);
            }
        }

        /// <summary></summary>
        public UInt16 LocalPort
        {
            get
            {
                if (socket4 != null)
                    return GetLocalPort(socket4);
                else
                    return GetLocalPort(socket6);
            }
        }

        private IPAddress GetRemoteAddress(Socket socket)
        {
            if (socket != null && socket.Connected)
            {
                IPEndPoint ep = (IPEndPoint)socket.RemoteEndPoint;
                if (ep != null)
                    return ep.Address;
            }
            return null;
        }

        private UInt16 GetRemotePort(Socket socket)
        {
            if (socket != null && socket.Connected)
            {
                IPEndPoint ep = (IPEndPoint)socket.RemoteEndPoint;
                if (ep != null)
                    return (UInt16)ep.Port;
            }
            return 0;
        }

        private IPAddress GetLocalAddress(Socket socket)
        {
            if (socket != null)
            {
                IPEndPoint ep = (IPEndPoint)socket.LocalEndPoint;
                if (ep != null)
                    return ep.Address;
            }
            return null;
        }

        private UInt16 GetLocalPort(Socket socket)
        {
            if (socket != null)
            {
                IPEndPoint ep = (IPEndPoint)socket.LocalEndPoint;
                if (ep != null)
                    return (UInt16)ep.Port;
            }
            return 0;
        }

        /// <summary></summary>
        public int Available
        {
            get
            {
                if (socket4 != null)
                    return socket4.Available;
                else if (socket6 != null)
                    return socket6.Available;
                else
                    return 0;
            }
        }

        /// <summary></summary>
        public override string ToString()
        {
            // Todo: Add proper description for AsyncSocket
            return base.ToString();
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Reading
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reads the first available bytes on the socket.
        /// </summary>
        /// <param name="timeout">
        ///		Timeout in milliseconds. Specify negative value for no timeout.
        /// </param>
        /// <param name="tag">
        ///		Tag to identify read request.
        ///	</param>
        public void Read(int timeout, long tag)
        {
            if ((flags & kForbidReadsWrites) > 0) return;

            MutableData buffer = new MutableData(0);

            // readQueue is synchronized
            readQueue.Enqueue(new AsyncReadPacket(buffer, timeout, -1, tag, true, false, null));

            // Queue a call to maybeDequeueRead
            ThreadPool.QueueUserWorkItem(new WaitCallback(MaybeDequeueRead));
        }

        /// <summary>
        /// Reads a certain number of bytes, and calls the delegate method when those bytes have been read.
        /// If length is 0, this method does nothing and no delgate methods are called.
        /// </summary>
        /// <param name="length">
        ///		The number of bytes to read.
        /// </param>
        /// <param name="timeout">
        ///		Timeout in milliseconds. Specify negative value for no timeout.
        /// </param>
        /// <param name="tag">
        ///		Tag to identify read request.
        ///	</param>
        public void Read(int length, int timeout, long tag)
        {
            if (length <= 0) return;
            if ((flags & kForbidReadsWrites) > 0) return;

            MutableData buffer = new MutableData(length);

            // readQueue is synchronized
            readQueue.Enqueue(new AsyncReadPacket(buffer, timeout, -1, tag, false, true, null));

            // Queue a call to maybeDequeueRead
            ThreadPool.QueueUserWorkItem(new WaitCallback(MaybeDequeueRead));
        }

        /// <summary>
        /// Reads bytes up to and including the passed data paramter, which acts as a separator.
        /// The bytes and the separator are returned by the delegate method.
        /// 
        /// If you pass null or zero-length data as the separator, this method will do nothing.
        /// To read a line from the socket, use the line separator (e.g. CRLF for HTTP) as the data parameter.
        /// Note that this method is not character-set aware, so if a separator can occur naturally
        /// as part of the encoding for a character, the read will prematurely end.
        /// </summary>
        /// <param name="term">
        ///		The separator/delimeter to use.
        /// </param>
        /// <param name="timeout">
        ///		Timeout in milliseconds. Specify negative value for no timeout.
        /// </param>
        /// <param name="tag">
        ///		Tag to identify read request.
        /// </param>
        public void Read(byte[] term, int timeout, long tag)
        {
            if ((term == null) || (term.Length == 0)) return;
            if ((flags & kForbidReadsWrites) > 0) return;

            MutableData buffer = new MutableData(0);

            // readQueue is synchronized
            readQueue.Enqueue(new AsyncReadPacket(buffer, timeout, -1, tag, false, false, term));

            // Queue a call to MaybeDequeueRead
            ThreadPool.QueueUserWorkItem(new WaitCallback(MaybeDequeueRead));
        }

        /// <summary>
        /// Reads bytes up to and including the passed data parameter, which acts as a separator.
        /// The bytes and the separator are returned by the delegate method.
        /// 
        /// The amount of data read may not surpass the given maxLength (specified in bytes).
        /// If the max length is surpassed, it is treated the same as a timeout - the socket is closed.
        /// Pass -1 as maxLength if no length restriction is desired, or simply use the other Read method.
        /// 
        /// If you pass null or zero-length data as the separator, or if you pass a maxLength parameter that is
        /// less than the length of the data parameter, this method will do nothing.
        /// To read a line from the socket, use the line separator (e.g. CRLF for HTTP) as the data parameter.
        /// Not that this method is not character-set aware, so if a separator can occur naturally
        /// as part of the encoding for a character, the read will prematurely end.
        /// </summary>
        /// <param name="term">
        ///		The separator/delimeter to use.
        /// </param>
        /// <param name="timeout">
        ///		Timeout in milliseconds. Specify negative value for no timeout.
        /// </param>
        /// <param name="maxLength">
        ///		Max length of data to read (in bytes). Specify negative value for no max length.
        /// </param>
        /// <param name="tag">
        ///		Tag to identify read request.
        ///	</param>
        public void Read(byte[] term, int timeout, int maxLength, long tag)
        {
            if ((term == null) || (term.Length == 0)) return;
            if ((maxLength >= 0) && (maxLength < term.Length)) return;
            if ((flags & kForbidReadsWrites) > 0) return;

            MutableData buffer = new MutableData(0);

            // readQueue is synchronized
            readQueue.Enqueue(new AsyncReadPacket(buffer, timeout, maxLength, tag, false, false, term));

            // Queue a call to MaybeDequeueRead
            ThreadPool.QueueUserWorkItem(new WaitCallback(MaybeDequeueRead));
        }

        /// <summary>
        /// If possible, this method dequeues a read from the read queue and starts it.
        /// This is only possible if all of the following are true:
        ///  1) any previous read has completed
        ///  2) there's a read in the queue
        ///  3) and the stream is ready.
        /// 
        /// This method is thread safe.
        /// </summary>
        private void MaybeDequeueRead(object ignore)
        {
            lock (lockObj)
            {
                if ((flags & kClosed) > 0)
                {
                    readQueue.Clear();
                    return;
                }

                if ((currentRead == null) && (stream != null))
                {
                    if ((flags & kPauseReads) > 0)
                    {
                        // Don't do any reads yet.
                        // We're waiting for TLS negotiation to start and/or finish.
                    }
                    else if (readQueue.Count > 0)
                    {
                        // Get the next object in the read queue
                        Object nextRead = readQueue.Dequeue();

                        if (nextRead is AsyncSpecialPacket)
                        {
                            // Next read packet is a special instruction packet.
                            // Right now this can only mean a StartTLS instruction.
                            AsyncSpecialPacket specialRead = (AsyncSpecialPacket)nextRead;

                            // Update flags - this flag will be unset when TLS finishes
                            flags |= kPauseReads;
                        }
                        else
                        {
                            // Get the new current read AsyncReadPacket
                            currentRead = (AsyncReadPacket)nextRead;

                            // Start time-out timer
                            if (currentRead.timeout >= 0)
                            {
                                readTimer = new System.Threading.Timer(new TimerCallback(stream_DidNotRead),
                                                                       currentRead,
                                                                       currentRead.timeout,
                                                                       Timeout.Infinite);
                            }

                            // Do we have any overflow data that we've already read from the stream?
                            if (readOverflow != null)
                            {
                                // Start reading from the overflow
                                DoReadOverflow();
                            }
                            else
                            {
                                // Start reading from the stream
                                DoStartRead();
                            }
                        }
                    }
                    else if ((flags & kCloseAfterReads) > 0)
                    {
                        if ((flags & kCloseAfterWrites) > 0)
                        {
                            if ((writeQueue.Count == 0) && (currentWrite == null))
                            {
                                Close(null);
                            }
                        }
                        else
                        {
                            Close(null);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method fills the currentRead buffer with data from the readOverflow variable.
        /// After this is properly completed, DoFinishRead is called to process the bytes.
        /// 
        /// This method is called from MaybeDequeueRead().
        /// 
        /// The above method is thread safe, so this method is inherently thread safe.
        /// It is not explicitly thread safe though, and should not be called outside thread safe methods.
        /// </summary>
        private void DoReadOverflow()
        {
            Debug.Assert(currentRead.bytesDone == 0);
            Debug.Assert(readOverflow.Length > 0);

            try
            {
                if (currentRead.readAllAvailableData)
                {
                    // We're supposed to read what's available.
                    // What we have in the readOverflow is what we have available, so just use it.

                    currentRead.buffer = readOverflow;
                    currentRead.bytesProcessing = readOverflow.Length;

                    readOverflow = null;
                }
                else if (currentRead.fixedLengthRead)
                {
                    // We're reading a certain length of data.

                    if (currentRead.buffer.Length < readOverflow.Length)
                    {
                        byte[] src = readOverflow.ByteArray;
                        byte[] dst = currentRead.buffer.ByteArray;

                        Buffer.BlockCopy(src, 0, dst, 0, dst.Length);

                        currentRead.bytesProcessing = dst.Length;

                        readOverflow.TrimStart(dst.Length);

                        // Note that this is the only case in which the readOverflow isn't emptied.
                        // This is OK because the read is guaranteed to finish in DoFinishRead().
                    }
                    else
                    {
                        byte[] src = readOverflow.ByteArray;
                        byte[] dst = currentRead.buffer.ByteArray;

                        Buffer.BlockCopy(src, 0, dst, 0, src.Length);

                        currentRead.bytesProcessing = src.Length;

                        readOverflow = null;
                    }
                }
                else
                {
                    // We're reading up to a termination sequence
                    // So we can just set the currentRead buffer to the readOverflow
                    // and the DoStartRead method will automatically handle any further overflow.

                    currentRead.buffer = readOverflow;
                    currentRead.bytesProcessing = readOverflow.Length;

                    readOverflow = null;
                }

                // At this point we've filled a currentRead buffer with some data
                // And the currentRead.bytesProcessing is set to the amount of data we filled it with
                // It's now time to process the data.
                DoFinishRead();
            }
            catch (Exception e)
            {
                CloseWithException(e);
            }
        }

        /// <summary>
        /// This method is called when either:
        ///  A) a new read is taken from the read queue
        ///  B) or when data has just been read from the stream, and we need to read more.
        /// 
        /// More specifically, it is called from either:
        ///  A) MaybeDequeueRead()
        ///  B) DoFinishRead()
        /// 
        /// The above methods are thread safe, or inherently thread safe, so this method is inherently thread safe.
        /// It is not explicitly thread safe though, and should not be called outside thread safe methods.
        /// </summary>
        private void DoStartRead()
        {
            try
            {
                // Perform an AsyncRead to notify us of when data becomes available on the socket.

                // Determine how much to read
                int size;
                if (currentRead.readAllAvailableData)
                {
                    size = READALL_CHUNKSIZE;

                    // Ensure the buffer is big enough to fit all the data
                    if (currentRead.buffer.Length < (currentRead.bytesDone + size))
                    {
                        currentRead.buffer.SetLength(currentRead.bytesDone + size);
                    }
                }
                else if (currentRead.fixedLengthRead)
                {
                    // We're reading a fixed amount of data, into a fixed size buffer
                    // We'll read up to the chunksize amount

                    // The read method is supposed to return smaller chunks as they become available.
                    // However, it doesn't seem to always follow this rule in practice.
                    // 
                    // size = currentRead.buffer.Length - currentRead.bytesDone;

                    int left = currentRead.buffer.Length - currentRead.bytesDone;
                    size = Math.Min(left, READ_CHUNKSIZE);
                }
                else
                {
                    // We're reading up to a termination sequence
                    size = READ_CHUNKSIZE;

                    // Ensure the buffer is big enough to fit all the data
                    if (currentRead.buffer.Length < (currentRead.bytesDone + size))
                    {
                        currentRead.buffer.SetLength(currentRead.bytesDone + size);
                    }
                }

                // The following should be spelled out:
                // If the stream can immediately complete the requested opertion, then
                // it may not fork off a background thread, meaning
                // that it's possible for the stream_DidRead method to get called before the
                // stream.BeginRead method returns below.

                stream.BeginRead(currentRead.buffer.ByteArray,      // buffer to read data into
                                 currentRead.bytesDone,             // buffer offset
                                 size,                              // max amout of data to read
                                 new AsyncCallback(stream_DidRead), // callback method
                                 currentRead);                      // callback info
            }
            catch (Exception e)
            {
                CloseWithException(e);
            }
        }

        /// <summary>
        /// Called after we've read data from the stream.
        /// We now call DoBytesAvailable, which will read and process further available data via the stream.
        /// 
        /// This method is thread safe.
        /// </summary>
        /// <param name="iar">AsyncState is AsyncReadPacket.</param>
        private void stream_DidRead(IAsyncResult iar)
        {
            lock (lockObj)
            {
                if (iar.AsyncState == currentRead)
                {
                    try
                    {
                        currentRead.bytesProcessing = stream.EndRead(iar);

                        if (currentRead.bytesProcessing > 0)
                        {
                            DoFinishRead();
                        }
                        else
                        {
                            CloseWithException(GetEndOfStreamException());
                        }
                    }
                    catch (Exception e)
                    {
                        CloseWithException(e);
                    }
                }
            }
        }

        /// <summary>
        /// Called after a read timeout timer fires.
        /// This will generally fire on an available thread from the thread pool.
        /// 
        /// This method is thread safe.
        /// </summary>
        /// <param name="state">state is AsyncReadPacket.</param>
        private void stream_DidNotRead(object state)
        {
            lock (lockObj)
            {
                if (state == currentRead)
                {
                    EndCurrentRead();

                    // GROWL
                    bool cancelClose = this.OnSocketDidReadTimeout();
                    if(!cancelClose)
                        CloseWithException(GetReadTimeoutException());
                }
            }
        }

        /// <summary>
        /// This method is called when either:
        ///  A) a new read is taken from the read queue
        ///  B) or when data has just been read from the stream.
        /// 
        /// More specifically, it is called from either:
        ///  A) DoReadOverflow()
        ///  B) stream_DidRead()
        /// 
        /// The above methods are thread safe, so this method is inherently thread safe.
        /// It is not explicitly thread safe though, and should not be called outside thread safe methods.
        /// </summary>
        private void DoFinishRead()
        {
            Debug.Assert(currentRead != null);
            Debug.Assert(currentRead.bytesProcessing > 0);

            int totalBytesRead = 0;
            bool done = false;
            bool maxoutError = false;

            if (currentRead.readAllAvailableData)
            {
                // We're done because we read everything that was available (up to a max size).
                currentRead.bytesDone += currentRead.bytesProcessing;
                totalBytesRead = currentRead.bytesProcessing;
                currentRead.bytesProcessing = 0;

                done = true;
            }
            else if (currentRead.fixedLengthRead)
            {
                // We're reading up to a fixed size
                currentRead.bytesDone += currentRead.bytesProcessing;
                totalBytesRead = currentRead.bytesProcessing;
                currentRead.bytesProcessing = 0;

                done = currentRead.buffer.Length == currentRead.bytesDone;
            }
            else
            {
                // We're reading up to a terminator
                // So let's start searching for the termination sequence in the new data

                while (!done && !maxoutError && (currentRead.bytesProcessing > 0))
                {
                    currentRead.bytesDone++;
                    totalBytesRead++;
                    currentRead.bytesProcessing--;

                    bool match = currentRead.bytesDone >= currentRead.term.Length;
                    int offset = currentRead.bytesDone - currentRead.term.Length;

                    for (int i = 0; match && i < currentRead.term.Length; i++)
                    {
                        match = (currentRead.term[i] == currentRead.buffer[offset + i]);
                    }
                    done = match;

                    if (!done && (currentRead.maxLength >= 0) && (currentRead.bytesDone >= currentRead.maxLength))
                    {
                        maxoutError = true;
                    }
                }
            }

            // If there was any overflow data, extract it and save it.
            // This may occur if our read maxed out.
            // Or if we received Y bytes, but only needed X bytes to finish the read (X < Y).
            if (currentRead.bytesProcessing > 0)
            {
                readOverflow = new MutableData(currentRead.buffer, currentRead.bytesDone, currentRead.bytesProcessing);
                currentRead.bytesProcessing = 0;
            }

            if (done)
            {
                // Truncate any excess unused buffer space in the read packet
                currentRead.buffer.SetLength(currentRead.bytesDone);

                CompleteCurrentRead();
                ThreadPool.QueueUserWorkItem(new WaitCallback(MaybeDequeueRead));
            }
            else if (maxoutError)
            {
                CloseWithException(GetReadMaxedOutException());
            }
            else
            {
                // We're not done yet, but we have read in some bytes
                OnSocketDidReadPartial(totalBytesRead, currentRead.tag);

                // It appears that we've read all immediately available data on the socket
                // So begin asynchronously reading data again
                DoStartRead();
            }
        }

        /// <summary>
        /// Completes the current read by ending it, and then informing the delegate that it's complete.
        /// 
        /// This method is called from DoFinishRead, which is inherently thread safe.
        /// Therefore this method is also inherently thread safe.
        /// It is not explicitly thread safe though, and should not be called outside thread safe methods.
        /// </summary>
        private void CompleteCurrentRead()
        {
            // Save reference to currentRead
            AsyncReadPacket completedRead = currentRead;

            // End the current read (this will nullify the currentRead variable)
            EndCurrentRead();

            // Notify delegate if possible
            OnSocketDidRead(completedRead.buffer.ByteArray, completedRead.tag);
        }

        /// <summary>
        /// Ends the current read by disposing and nullifying the read timer,
        /// and then nullifying the current read.
        /// </summary>
        private void EndCurrentRead()
        {
            if (readTimer != null)
            {
                readTimer.Dispose();
                readTimer = null;
            }

            currentRead = null;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Writing
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Writes the specified data to the socket.
        /// </summary>
        /// <param name="data">
        ///		The data to send.
        /// </param>
        /// <param name="timeout">
        ///		Timeout in milliseconds. Specify a negative value if no timeout is desired.
        /// </param>
        /// <param name="tag">
        ///		A tag that can be used to track the write.
        ///		This tag will be returned in the callback methods.
        /// </param>
        public void Write(byte[] data, int timeout, long tag)
        {
            if ((data == null) || (data.Length == 0)) return;
            if ((flags & kForbidReadsWrites) > 0) return;

            // writeQueue is synchronized
            writeQueue.Enqueue(new AsyncWritePacket(data, 0, data.Length, timeout, tag));

            // Queue a call to MaybeDequeueWrite
            ThreadPool.QueueUserWorkItem(new WaitCallback(MaybeDequeueWrite));
        }

        /// <summary>
        /// Writes the specified data to the socket.
        /// </summary>
        /// <param name="data">
        ///		The buffer that contains the data to write.
        /// </param>
        /// <param name="offset">
        ///		The offset within the given data to start writing from.
        /// </param>
        /// <param name="length">
        ///		The amount of data (in bytes) to write from the given data, starting from the given offset.
        /// </param>
        /// <param name="timeout">
        ///		Timeout in milliseconds. Specify a negative value if no timeout is desired.
        /// </param>
        /// <param name="tag">
        ///		A tag that can be used to track the write.
        ///		This tag will be returned in the callback methods.
        ///	</param>
        public void Write(byte[] data, int offset, int length, int timeout, long tag)
        {
            if ((data == null) || (data.Length == 0)) return;
            if ((flags & kForbidReadsWrites) > 0) return;

            // writeQueue is synchronized
            writeQueue.Enqueue(new AsyncWritePacket(data, offset, length, timeout, tag));

            // Queue a call to MaybeDequeueWrite
            ThreadPool.QueueUserWorkItem(new WaitCallback(MaybeDequeueWrite));
        }

        /// <summary>
        /// If possible, this method dequeues a write from the write queue and starts it.
        /// This is only possible if all of the following are true:
        ///  1) any previous write has completed
        ///  2) there's a write in the queue
        ///  3) and the socket is connected.
        /// 
        /// This method is thread safe.
        /// </summary>
        private void MaybeDequeueWrite(object ignore)
        {
            lock (lockObj)
            {
                if ((flags & kClosed) > 0)
                {
                    writeQueue.Clear();
                    return;
                }

                if ((currentWrite == null) && (stream != null))
                {
                    if ((flags & kPauseWrites) > 0)
                    {
                        // Don't do any reads yet.
                        // We're waiting for TLS negotiation to start and/or finish.
                    }
                    else if (writeQueue.Count > 0)
                    {
                        // Get the next object in the read queue
                        Object nextWrite = writeQueue.Dequeue();

                        if (nextWrite is AsyncSpecialPacket)
                        {
                            // Next write packet is a special instruction packet.
                            // Right now this can only mean a StartTLS instruction.
                            AsyncSpecialPacket specialWrite = (AsyncSpecialPacket)nextWrite;

                            // Update flags - this flag will be unset when TLS finishes
                            flags |= kPauseWrites;
                        }
                        else
                        {
                            // Get the current write AsyncWritePacket
                            currentWrite = (AsyncWritePacket)nextWrite;

                            // Start time-out timer
                            if (currentWrite.timeout >= 0)
                            {
                                writeTimer = new System.Threading.Timer(new TimerCallback(stream_DidNotWrite),
                                                                        currentWrite,
                                                                        currentWrite.timeout,
                                                                        Timeout.Infinite);
                            }

                            try
                            {
                                DoSendBytes();
                            }
                            catch (Exception e)
                            {
                                CloseWithException(e);
                            }
                        }
                    }
                    else if ((flags & kCloseAfterWrites) > 0)
                    {
                        if ((flags & kCloseAfterReads) > 0)
                        {
                            if ((readQueue.Count == 0) && (currentRead == null))
                            {
                                Close(null);
                            }
                        }
                        else
                        {
                            Close(null);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method is called when either:
        ///  A) a new write is taken from the write queue
        ///  B) or when a previos write has finished.
        /// 
        /// More specifically, it is called from either:
        ///  A) MaybeDequeueWrite()
        ///  B) stream_DidWrite()
        /// 
        /// The above methods are thread safe, so this method is inherently thread safe.
        /// It is not explicitly thread safe though, and should not be called outside the above named methods.
        /// </summary>
        private void DoSendBytes()
        {
            int available = currentWrite.length - currentWrite.bytesDone;
            int size = (available < WRITE_CHUNKSIZE) ? available : WRITE_CHUNKSIZE;

            // The following should be spelled out:
            // If the stream can immediately complete the requested opertion, then
            // it may not fork off a background thread, meaning
            // that it's possible for the stream_DidWrite method to get called before the
            // stream.BeginWrite method returns below.

            currentWrite.bytesProcessing = size;

            stream.BeginWrite(currentWrite.buffer,                          // buffer to write from
                              currentWrite.offset + currentWrite.bytesDone, // buffer offset
                              size,                                         // amount of data to send
                              new AsyncCallback(stream_DidWrite),           // callback method
                              currentWrite);                                // callback info
        }

        /// <summary>
        /// Called when an asynchronous write has finished.
        /// This may just be a chunk of the data, and not the entire thing.
        /// 
        /// This method is thread safe.
        /// </summary>
        /// <param name="iar"></param>
        private void stream_DidWrite(IAsyncResult iar)
        {
            lock (lockObj)
            {
                if (iar.AsyncState == currentWrite)
                {
                    try
                    {
                        // Note: EndWrite is void
                        // Instead we must store and retrieve the amount of data we were trying to send
                        stream.EndWrite(iar);
                        currentWrite.bytesDone += currentWrite.bytesProcessing;

                        if (currentWrite.bytesDone == currentWrite.length)
                        {
                            CompleteCurrentWrite();
                            ThreadPool.QueueUserWorkItem(new WaitCallback(MaybeDequeueWrite));
                        }
                        else
                        {
                            // We're not done yet, but we have written out some bytes
                            OnSocketDidWritePartial(currentWrite.bytesProcessing, currentWrite.tag);

                            DoSendBytes();
                        }
                    }
                    catch (Exception e)
                    {
                        CloseWithException(e);
                    }
                }
            }
        }

        /// <summary>
        /// Called when a timeout occurs. (Called via thread timer).
        /// 
        /// This method is thread safe.
        /// </summary>
        /// <param name="state">
        /// 	The AsyncWritePacket that the timeout applies to.
        /// </param>
        private void stream_DidNotWrite(object state)
        {
            lock (lockObj)
            {
                if (state == currentWrite)
                {
                    EndCurrentWrite();
                    CloseWithException(GetWriteTimeoutException());
                }
            }
        }

        /// <summary>
        /// Completes the current write by ending it, and then informing the delegate that it's complete.
        /// 
        /// This method is called from stream_DidWrite, which is thread safe.
        /// Therefore this method is inherently thread safe.
        /// It is not explicitly thread safe though, and should not be called outside thread safe methods.
        /// </summary>
        private void CompleteCurrentWrite()
        {
            // Save reference to currentRead
            AsyncWritePacket completedWrite = currentWrite;

            // End the current write (this will nullify the currentWrite variable)
            EndCurrentWrite();

            // Notify delegate if possible
            OnSocketDidWrite(completedWrite.tag);
        }

        /// <summary></summary>
        public void EndCurrentWrite()
        {
            if (writeTimer != null)
            {
                writeTimer.Dispose();
                writeTimer = null;
            }

            currentWrite = null;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Static Methods
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary></summary>
        public static byte[] CRLFCRLFData
        {
            get { return Encoding.UTF8.GetBytes("\r\n\r\n"); }
        }

        /// <summary></summary>
        public static byte[] CRLFData
        {
            get { return Encoding.UTF8.GetBytes("\r\n"); }
        }

        /// <summary></summary>
        public static byte[] CRData
        {
            get { return Encoding.UTF8.GetBytes("\r"); }
        }

        /// <summary></summary>
        public static byte[] LFData
        {
            get { return Encoding.UTF8.GetBytes("\n"); }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
