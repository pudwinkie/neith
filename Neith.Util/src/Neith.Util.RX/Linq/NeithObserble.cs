using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace Neith.Util.RX.Linq
{
    public static class NeithObserble
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public static IObservable<T> Weak<T>(this IObservable<T> src)
        {
            var sub = new Subject<T>();
            var wr = new WeakReference(sub);
            return src;
        }

        private class WeakSubject<T> : ISubject<T>
        {
            private


            #region IObserver<T> メンバー

            public void OnCompleted()
            {
                throw new NotImplementedException();
            }

            public void OnError(Exception error)
            {
                throw new NotImplementedException();
            }

            public void OnNext(T value)
            {
                throw new NotImplementedException();
            }

            #endregion

            #region IObservable<T> メンバー

            public IDisposable Subscribe(IObserver<T> observer)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

    }
}
