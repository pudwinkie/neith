using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using Neith.Logger.Model;
using Neith.Growl.Daemon;
using Neith.Growl.Connector;

namespace Neith.Logger
{
    /// <summary>
    /// リクエストの収集・振り分けエンジン。
    /// </summary>
    public class RequestSorter : IDisposable, IObservable<ResponseItem>
    {
        #region 開放
        private readonly CompositeDisposable Tasks = new CompositeDisposable();
        /// <summary>解放処理</summary>
        public void Dispose()
        {
            Tasks.Dispose();
        }


        #endregion
        #region フィールド
        private readonly Subject<MessageItem> rxMessage;
        private readonly Subject<RegisterItem> rxRegester;
        private readonly Subject<SubscriberItem> rxSubscriber;
        private readonly Subject<NotificationItem> rxNotification;
        private readonly Subject<ResponseItem> rxResponse;


        #endregion
        #region 初期化

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public RequestSorter()
        {
            var sc = Scheduler.TaskPool;
            rxMessage = new Subject<MessageItem>().Add(Tasks);
            rxRegester = new Subject<RegisterItem>().Add(Tasks);
            rxSubscriber = new Subject<SubscriberItem>().Add(Tasks);
            rxNotification = new Subject<NotificationItem>().Add(Tasks);
            rxResponse = new Subject<ResponseItem>().Add(Tasks);

            AddTask(rxMessage, ActReceive);
            AddTask(rxRegester, ActRegister);
            AddTask(rxNotification, ActNotify);
            Tasks.Add(rxSubscriber.Subscribe(ActSubscribe));
        }

        private void AddTask<T>(IObservable<T> rx, Action<T> act) where T : MessageItem
        {
            Tasks.Add(rx.Subscribe(a => ActOrCatch(a, act)));
        }

        /// <summary>
        /// リクエストの例外フィルタ
        /// </summary>
        /// <param name="item"></param>
        private void ActOrCatch<T>(T item, Action<T> act) where T : MessageItem
        {
            try {
                act(item);
            }
            catch (GrowlException gEx) {
                rxResponse.OnNext(ResponseItem.Create(item, gEx));
            }
            catch (Exception ex) {
                rxResponse.OnNext(ResponseItem.Create(item, ex));
            }
        }

        #endregion
        #region 配信登録

        /// <summary>
        /// MessageItemの配信を登録します。
        /// </summary>
        /// <param name="rx"></param>
        /// <returns></returns>
        internal IDisposable Register(IObservable<MessageItem> rx)
        {
            return rx.Subscribe(a => rxMessage.OnNext(a));
        }

        /// <summary>
        /// RegisterItemの配信を登録します。
        /// </summary>
        /// <param name="rx"></param>
        /// <returns></returns>
        internal IDisposable CreateSubscrive(IObservable<RegisterItem> rx)
        {
            return rx.Subscribe(a => rxRegester.OnNext(a));
        }

        /// <summary>
        /// SubscriberItemの配信を登録します。
        /// </summary>
        /// <param name="rx"></param>
        /// <returns></returns>
        internal IDisposable CreateSubscrive(IObservable<SubscriberItem> rx)
        {
            return rx.Subscribe(a => rxSubscriber.OnNext(a));
        }

        /// <summary>
        /// NotificationItemの配信を登録します。
        /// </summary>
        /// <param name="rx"></param>
        /// <returns></returns>
        internal IDisposable CreateSubscrive(IObservable<NotificationItem> rx)
        {
            return rx.Subscribe(a => rxNotification.OnNext(a));
        }

        #endregion
        #region 受信処理

        /// <summary>
        /// リクエストの処理
        /// </summary>
        /// <param name="item"></param>
        private void ActReceive(MessageItem item)
        {
            switch (item.Request.Directive) {
                case RequestType.REGISTER:
                    rxRegester.OnNext(RegisterItem.Create(item));
                    return;
                case RequestType.NOTIFY:
                    rxNotification.OnNext(NotificationItem.Create(item));
                    return;
                case RequestType.SUBSCRIBE:
                    rxSubscriber.OnNext(SubscriberItem.Create(item));
                    return;
            }
        }

        /// <summary>
        /// アプリケーション登録処理
        /// </summary>
        /// <param name="item"></param>
        private void ActRegister(RegisterItem item)
        {

        }

        /// <summary>
        /// 購買登録処理
        /// </summary>
        /// <param name="item"></param>
        private void ActSubscribe(SubscriberItem item)
        {

        }

        /// <summary>
        /// 通知処理
        /// </summary>
        /// <param name="item"></param>
        private void ActNotify(NotificationItem item)
        {

        }



        #endregion

        #region IObservable<ResponseItem> メンバー

        /// <summary>
        /// レスポンスの発行。
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        public IDisposable Subscribe(IObserver<ResponseItem> observer)
        {
            return rxResponse.Subscribe(observer);
        }

        #endregion
    }
}
