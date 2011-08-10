using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Neith.Util.Threading
{
    /// <summary>
    /// スレッドの起動待機を行うための処理クラスです。
    /// </summary>
    public sealed class WaitedThreadStartRunner
    {
        private object l = new object();
        private Thread thread;




        /// <summary>
        /// 新しく作成したスレッド。
        /// </summary>
        public Thread Thread { get { return thread; } }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="start"></param>
        public WaitedThreadStartRunner(ThreadStart start)
        {
            thread = new Thread(GetWaitThreadStart(start));
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="start"></param>
        /// <param name="maxStackSize"></param>
        public WaitedThreadStartRunner(ThreadStart start, int maxStackSize)
        {
            thread = new Thread(GetWaitThreadStart(start), maxStackSize);
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="start"></param>
        public WaitedThreadStartRunner(ParameterizedThreadStart start)
        {
            thread = new Thread(GetWaitThreadStart(start));
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="start"></param>
        /// <param name="maxStackSize"></param>
        public WaitedThreadStartRunner(ParameterizedThreadStart start, int maxStackSize)
        {
            thread = new Thread(GetWaitThreadStart(start), maxStackSize);
        }

        /// <summary>
        /// スレッドを開始し、スレッドが実際に開始するまで待機します。
        /// </summary>
        public void Start()
        {
            lock (l) {
                Thread.Start();
                Wait();
            }
        }

        /// <summary>
        /// スレッドを開始し、スレッドが実際に開始するまで待機します。
        /// </summary>
        /// <param name="parameter"></param>
        public void Start(object parameter)
        {
            lock (l) {
                Thread.Start(parameter);
                Wait();
            }
        }

        private ParameterizedThreadStart GetWaitThreadStart(ParameterizedThreadStart start)
        {
            return delegate(object arg) { Pulse(); start(arg); };
        }
        private ThreadStart GetWaitThreadStart(ThreadStart start)
        {
            return delegate() { Pulse(); start(); };
        }

        private void Wait()
        {
            Monitor.Wait(l);
        }

        private void Pulse()
        {
            Monitor.Pulse(l);
        }

        #region その他スレッドクラスのラッパー

        /// <summary>
        /// アパートメント状態を示す ApartmentState 値を返します。 
        /// </summary>
        /// <returns></returns>
        public ApartmentState GetApartmentState()
        {
            return Thread.GetApartmentState();
        }

        /// <summary>
        /// スレッドを開始する前にスレッドのアパートメント状態を設定します。
        /// </summary>
        /// <param name="state"></param>
        public void SetApartmentState(ApartmentState state)
        {
            Thread.SetApartmentState(state);
        }

        /// <summary>現在のスレッドの実行ステータスを示す値を取得します。 </summary>
        public bool IsAlive { get { return Thread.IsAlive; } }

        /// <summary>スレッドがバックグラウンド スレッドであるかどうかを示す値を取得または設定します。</summary>
        public bool IsBackground
        {
            get { return Thread.IsBackground; }
            set { Thread.IsBackground = value; }
        }






        #endregion

    }
}
