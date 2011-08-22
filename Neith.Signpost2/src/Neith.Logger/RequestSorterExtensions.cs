using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Neith.Logger.Model;
using Neith.Growl.Daemon;
using Neith.Growl.Connector;

namespace Neith.Logger
{
    public static class RequestSorterExtensions
    {
        /// <summary>
        /// MessageItemの配信を登録します。
        /// </summary>
        /// <param name="rx"></param>
        /// <param name="receiver"></param>
        /// <returns></returns>
        public static IDisposable Subscribe(this IObservable<MessageItem> rx, RequestSorter receiver)
        {
            return receiver.Register(rx);
        }

        /// <summary>
        /// RegisterItemの配信を登録します。
        /// </summary>
        /// <param name="rx"></param>
        /// <param name="receiver"></param>
        /// <returns></returns>
        public static IDisposable Subscribe(IObservable<RegisterItem> rx, RequestSorter receiver)
        {
            return receiver.Register(rx);
        }

        /// <summary>
        /// SubscriberItemの配信を登録します。
        /// </summary>
        /// <param name="rx"></param>
        /// <param name="receiver"></param>
        /// <returns></returns>
        public static IDisposable Subscribe(IObservable<SubscriberItem> rx, RequestSorter receiver)
        {
            return receiver.Register(rx);
        }

        /// <summary>
        /// NotificationItemの配信を登録します。
        /// </summary>
        /// <param name="rx"></param>
        /// <param name="receiver"></param>
        /// <returns></returns>
        public static IDisposable Subscribe(IObservable<NotificationItem> rx, RequestSorter receiver)
        {
            return receiver.Register(rx);
        }

    }
}
