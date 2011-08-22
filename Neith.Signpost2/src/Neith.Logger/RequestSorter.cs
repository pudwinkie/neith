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

            Tasks.Add(rxMessage.Subscribe(Receive));
            Tasks.Add(rxRegester.Subscribe(Receive));
            Tasks.Add(rxNotification.Subscribe(Receive));
            Tasks.Add(rxSubscriber.Subscribe(Receive));
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
        internal IDisposable Register(IObservable<RegisterItem> rx)
        {
            return rx.Subscribe(a => rxRegester.OnNext(a));
        }

        /// <summary>
        /// SubscriberItemの配信を登録します。
        /// </summary>
        /// <param name="rx"></param>
        /// <returns></returns>
        internal IDisposable Register(IObservable<SubscriberItem> rx)
        {
            return rx.Subscribe(a => rxSubscriber.OnNext(a));
        }

        /// <summary>
        /// NotificationItemの配信を登録します。
        /// </summary>
        /// <param name="rx"></param>
        /// <returns></returns>
        internal IDisposable Register(IObservable<NotificationItem> rx)
        {
            return rx.Subscribe(a => rxNotification.OnNext(a));
        }

        #endregion
        #region 受信処理

        /// <summary>
        /// リクエストの処理
        /// </summary>
        /// <param name="item"></param>
        private void Receive(MessageItem item)
        {
            var request = item.Request;
            try {
                switch (request.Directive) {
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
            catch (GrowlException gEx) {
                rxResponse.OnNext(new ResponseItem(item, gEx.ErrorCode, gEx.Message, gEx.AdditionalInfo));
            }
            catch (Exception ex) {
                rxResponse.OnNext(new ResponseItem(item, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message));
            }
        }

        /// <summary>
        /// アプリケーション登録処理
        /// </summary>
        /// <param name="item"></param>
        private void Receive(RegisterItem item)
        {

        }

        /// <summary>
        /// 購買登録処理
        /// </summary>
        /// <param name="item"></param>
        private void Receive(SubscriberItem item)
        {

        }

        /// <summary>
        /// 通知処理
        /// </summary>
        /// <param name="item"></param>
        private void Receive(NotificationItem item)
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
