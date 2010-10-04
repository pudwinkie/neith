using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using System.Security;
using System.Security.Permissions;

namespace Neith.Threading
{
    public sealed class DispatcherPrioritySynchronizationContext : SynchronizationContext
    {
        public Dispatcher Dispatcher { get; private set; }
        public DispatcherPriority Priority { get; private set; }
        private SynchronizationContext waitContext;

        public DispatcherPrioritySynchronizationContext(Dispatcher dispatcher, DispatcherPriority priority)
        {
            waitContext = new DispatcherSynchronizationContext(dispatcher);
            Dispatcher = dispatcher;
            Priority = priority;
            base.SetWaitNotificationRequired();
        }

        public DispatcherPrioritySynchronizationContext(DispatcherPriority priority)
            : this(Dispatcher.CurrentDispatcher, priority)
        {
        }

        public override SynchronizationContext CreateCopy()
        {
            return this;
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            Dispatcher.BeginInvoke(Priority, d, state);
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            Dispatcher.Invoke(Priority, d, state);
        }

        [SecurityCritical, SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.ControlEvidence)]
        public override int Wait(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
        {
            return waitContext.Wait(waitHandles, waitAll, millisecondsTimeout);
        }

    }
}
