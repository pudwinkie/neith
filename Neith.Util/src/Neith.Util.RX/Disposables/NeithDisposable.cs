using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Reactive.Disposables
{
    /// <summary>Disposable拡張</summary>
    public static class NeithDisposable
    {
        /// <summary>
        /// CompositeDisposableに要素を登録します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="disp"></param>
        /// <returns></returns>
        public static T Add<T>(this T src, CompositeDisposable disp)
            where T : IDisposable
        {
            disp.Add(src);
            return src;
        }

        /// <summary>
        /// CompositeDisposableにActionを登録します。
        /// </summary>
        /// <param name="disp"></param>
        /// <param name="act"></param>
        public static void Add(this CompositeDisposable disp, Action act)
        {
            disp.Add(Disposable.Create(act));
        }

        /// <summary>
        /// Enter,Leaveアクションを指定したIDisposableを作成します。
        /// </summary>
        /// <param name="enter"></param>
        /// <param name="leave"></param>
        /// <returns></returns>
        public static IDisposable Create(Action enter, Action leave)
        {
            enter();
            return Disposable.Create(leave);
        }

    }
}
