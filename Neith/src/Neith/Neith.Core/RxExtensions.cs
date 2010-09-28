using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neith.Threading;
using System.Windows.Threading;

namespace Neith
{
    /// <summary>
    /// ネイトの共有非同期関数を提供します。
    /// </summary>
    public static class RxExtensions
    {

        /// <summary>
        /// 以後のスケジューラを一般タスク用に切り替えます。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<TSource> ObserveOnTask<TSource>(this IObservable<TSource> source)
        {
            return source.ObserveOn(Schedulers.NormalScheduler);
        }

        /// <summary>
        /// 規定スケジューラを一般タスク用に設定します。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<TSource> SubscribeOnTask<TSource>(this IObservable<TSource> source)
        {
            return source.SubscribeOn(Schedulers.NormalScheduler);
        }




        /// <summary>
        /// 以後のスケジューラを長時間タスク用に切り替えます。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<TSource> ObserveOnLongRunning<TSource>(this IObservable<TSource> source)
        {
            return source.ObserveOn(Schedulers.LongRunningScheduler);
        }

        /// <summary>
        /// 規定スケジューラを長時間タスク用に切り替えます。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<TSource> SubscribeOnLongRunning<TSource>(this IObservable<TSource> source)
        {
            return source.SubscribeOn(Schedulers.LongRunningScheduler);
        }




        /// <summary>
        /// 以後のスケジューラをディスパッチャー：Normal優先度に切り替えます。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<TSource> ObserveOnDispatcher<TSource>(
            this IObservable<TSource> source, DispatcherPriority priority)
        {
            var context = new DispatcherPrioritySynchronizationContext(priority);
            return source.ObserveOn(context);
        }

        /// <summary>
        /// 規定スケジューラをディスパッチャー：Normal優先度に切り替えます。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<TSource> SubscribeOnDispatcher<TSource>(
            this IObservable<TSource> source, DispatcherPriority priority)
        {
            var context = new DispatcherPrioritySynchronizationContext(priority);
            return source.SubscribeOn(context);
        }



        /// <summary>
        /// 以後のスケジューラをディスパッチャー：Normal優先度に切り替えます。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<TSource> ObserveOnNormal<TSource>(this IObservable<TSource> source)
        {
            return source.ObserveOnDispatcher(DispatcherPriority.Normal);
        }

        /// <summary>
        /// 規定スケジューラをディスパッチャー：Normal優先度に切り替えます。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<TSource> SubscribeOnNormal<TSource>(this IObservable<TSource> source)
        {
            return source.SubscribeOnDispatcher(DispatcherPriority.Normal);
        }




        /// <summary>
        /// 以後のスケジューラをディスパッチャー：Render優先度に切り替えます。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<TSource> ObserveOnRender<TSource>(this IObservable<TSource> source)
        {
            return source.ObserveOnDispatcher(DispatcherPriority.Render);
        }

        /// <summary>
        /// 規定スケジューラをディスパッチャー：Render優先度に切り替えます。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<TSource> SubscribeOnRender<TSource>(this IObservable<TSource> source)
        {
            return source.SubscribeOnDispatcher(DispatcherPriority.Render);
        }




        /// <summary>
        /// 以後のスケジューラをディスパッチャー：Loaded優先度に切り替えます。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<TSource> ObserveOnLoaded<TSource>(this IObservable<TSource> source)
        {
            return source.ObserveOnDispatcher(DispatcherPriority.Loaded);
        }

        /// <summary>
        /// 規定スケジューラをディスパッチャー：Loaded優先度に切り替えます。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<TSource> SubscribeOnLoaded<TSource>(this IObservable<TSource> source)
        {
            return source.SubscribeOnDispatcher(DispatcherPriority.Loaded);
        }




        /// <summary>
        /// 以後のスケジューラをディスパッチャー：Background優先度に切り替えます。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<TSource> ObserveOnBackground<TSource>(this IObservable<TSource> source)
        {
            return source.ObserveOnDispatcher(DispatcherPriority.Background);
        }

        /// <summary>
        /// 規定スケジューラをディスパッチャー：Background優先度に切り替えます。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<TSource> SubscribeOnBackground<TSource>(this IObservable<TSource> source)
        {
            return source.SubscribeOnDispatcher(DispatcherPriority.Background);
        }




        /// <summary>
        /// 以後のスケジューラをディスパッチャー：ApplicationIdle優先度に切り替えます。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<TSource> ObserveOnIdle<TSource>(this IObservable<TSource> source)
        {
            return source.ObserveOnDispatcher(DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// 以後のスケジューラをディスパッチャー：ApplicationIdle優先度に切り替えます。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<TSource> SubscribeOnIdle<TSource>(this IObservable<TSource> source)
        {
            return source.SubscribeOnDispatcher(DispatcherPriority.ApplicationIdle);
        }


    }
}
