using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Neith.Signpost.Proxy
{
    /// <summary>
    /// Signpostアプリ間の基本サービス。
    /// </summary>
    public interface ISignpostBaseService
    {
        /// <summary>
        /// サービス開始を通知します。
        /// </summary>
        /// <param name="name"></param>
        [OperationContract(IsOneWay = true)]
        void Join(string name);

        /// <summary>
        /// サービス終了を通知します。
        /// </summary>
        /// <param name="name"></param>
        [OperationContract(IsOneWay = true)]
        void Leave(string name);

        /// <summary>
        /// 全てのサービスに応答を要求します。
        /// </summary>
        /// <param name="name"></param>
        [OperationContract(IsOneWay = true)]
        void RequestEcho(string name);

        /// <summary>
        /// 応答します。
        /// </summary>
        /// <param name="name"></param>
        [OperationContract(IsOneWay = true)]
        void ResponseEcho(string name);

    }


    /// <summary>
    /// サーバ側状態の購読サービス。
    /// </summary>
    public interface ISignpostSubscriptionService
    {


    }


    /// <summary>
    /// サーバへのアクションサービス。
    /// </summary>
    public interface ISignpostActionService
    {


    }




    [ServiceContract(Namespace = "http://signpost.vbel.net/peerservice",
    CallbackContract = typeof(ISignpostService))]
    public interface ISignpostService : ISignpostBaseService, ISignpostSubscriptionService, ISignpostActionService
    {
    }

    public interface ISignpostServiceChannel : ISignpostService, IClientChannel
    {
    }
}
