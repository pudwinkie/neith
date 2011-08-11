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
    /// <summary>
    /// リクエストの収集・振り分けエンジン。
    /// </summary>
    public class RequestReceiver :IDisposable
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
        private Subject<MessageItem> rxMessage;
        private Subject<RegisterItem> rxRegester;
        private Subject<SubscriberItem> rxSubscriber;
        private Subject<NotificationItem> rxNotification;


        #endregion
        #region 初期化

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public RequestReceiver()
        {
            rxMessage = new Subject<MessageItem>().Add(Tasks);
            rxRegester = new Subject<RegisterItem>().Add(Tasks);
            rxSubscriber = new Subject<SubscriberItem>().Add(Tasks);
            rxNotification = new Subject<NotificationItem>().Add(Tasks);
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
        public IDisposable Subscribe(IObservable<MessageItem> rx)
        {
            return rx.Subscribe(a => rxMessage.OnNext(a));
        }

        /// <summary>
        /// RegisterItemの配信を登録します。
        /// </summary>
        /// <param name="rx"></param>
        /// <returns></returns>
        public IDisposable Subscribe(IObservable<RegisterItem> rx)
        {
            return rx.Subscribe(a => rxRegester.OnNext(a));
        }

        /// <summary>
        /// SubscriberItemの配信を登録します。
        /// </summary>
        /// <param name="rx"></param>
        /// <returns></returns>
        public IDisposable Subscribe(IObservable<SubscriberItem> rx)
        {
            return rx.Subscribe(a => rxSubscriber.OnNext(a));
        }

        /// <summary>
        /// NotificationItemの配信を登録します。
        /// </summary>
        /// <param name="rx"></param>
        /// <returns></returns>
        public IDisposable Subscribe(IObservable<NotificationItem> rx)
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
#if false
            var request = item.Request;

            try {
                IResponse response = null;
                switch (request.Directive) {
                    case RequestType.REGISTER:
                        var application = Application.FromHeaders(request.Headers);
                        var notificationTypes = new List<INotificationType>();
                        for (int i = 0; i < request.NotificationsToBeRegistered.Count; i++) {
                            var headers = request.NotificationsToBeRegistered[i];
                            notificationTypes.Add(NotificationType.FromHeaders(headers));
                        }
                        response = this.OnRegisterReceived(application, notificationTypes, mh.RequestInfo);
                        break;
                    case RequestType.NOTIFY:
                        var notification = NeithNotificationModel.FromHeaders(request.Headers);
                        mh.CallbackInfo.NotificationID = notification.ID;
                        response = this.OnNotifyReceived(notification, mh.CallbackInfo, mh.RequestInfo);
                        break;
                    case RequestType.SUBSCRIBE:
                        var subscriber = Subscriber.FromHeaders(request.Headers);
                        subscriber.IPAddress = mh.Socket.RemoteAddress.ToString();
                        subscriber.Key = new SubscriberKey(request.Key, subscriber.ID, request.Key.HashAlgorithm, request.Key.EncryptionAlgorithm);
                        response = this.OnSubscribeReceived(subscriber, mh.RequestInfo);
                        break;
                }


                var responseType = ResponseType.ERROR;
                if (response != null && response.IsOK) {
                    responseType = ResponseType.OK;
                    response.InResponseTo = request.Directive.ToString();
                }

                // no response
                if (response == null)
                    response = new Response(ErrorCode.INTERNAL_SERVER_ERROR, ErrorDescription.INTERNAL_SERVER_ERROR);

                AddServerHeaders(response);
                var mb = new MessageBuilder(responseType);
                var responseHeaders = response.ToHeaders();
                foreach (var header in responseHeaders) {
                    mb.AddHeader(header);
                }
                // return any application-specific data headers that were received
                RequestData rd = RequestData.FromHeaders(request.Headers);
                AddRequestData(mb, rd);

                bool requestComplete = !mh.CallbackInfo.ShouldKeepConnectionOpen();
                mh.WriteResponse(mb, requestComplete);
            }
            catch (GrowlException gEx) {
                mh.WriteError(gEx.ErrorCode, gEx.Message, gEx.AdditionalInfo);
            }
            catch (Exception ex) {
                mh.WriteError(ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
#endif
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
    }
}
