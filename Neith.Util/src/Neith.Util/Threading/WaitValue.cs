using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Neith.Util.Threading
{
    /// <summary>
    /// スレッド間の同期を取りつつ値のやり取りを行います。
    /// 他のスレッドより値が設定されるまで待機し、値を取得します。
    /// また、設定した値がまだ取得されていない場合に待機します。
    /// このクラスは読み書きそれぞれ１スレッドの場合にのみ使用してください。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WaitValue<T> where T : class
    {
        private T v = null;
        private bool isSetValue = false;
        private bool isWait = false;


        /// <summary>
        /// 値。
        /// </summary>
        public T Value
        {
            get
            {
                lock (this) {
                    while (!isSetValue) {
                        if (isWait) throw new InvalidOperationException("同時に２スレッド以上が読み書きしようとしています。");
                        isWait = true;
                        Monitor.Wait(this);
                    }
                    T rc = v;
                    isSetValue = false;
                    if (isWait) {
                        Monitor.Pulse(this);
                        isWait = false;
                    }
                    return rc;
                }
            }
            set
            {
                lock (this) {
                    while (isSetValue) {
                        if (isWait) throw new InvalidOperationException("同時に２スレッド以上が読み書きしようとしています。");
                        isWait = true;
                        Monitor.Wait(this);
                    }
                    v = value;
                    isSetValue = false;
                    if (isWait) {
                        Monitor.Pulse(this);
                        isWait = false;
                    }
                }
            }
        }

    }
}
