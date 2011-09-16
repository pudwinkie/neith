using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Neith.Signpost.Proxy
{
    /// <summary>
    /// Signpostアプリ間の基本サービス。
    /// </summary>
    public interface ISignpostBaseService
    {

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
        /// <summary>
        /// Alt + ? を実行。
        /// </summary>
        /// <param name="name">サービス名</param>
        /// <param name="c">組み合わせ文字。アルファベットの場合は大文字を使う。</param>
        [WebGet]
        void SendAlt(char c);

        /// <summary>
        /// Ctrl + ? を実行。
        /// </summary>
        /// <param name="name">サービス名</param>
        /// <param name="c">組み合わせ文字。アルファベットの場合は大文字を使う。</param>
        [WebGet]
        void SendCtrl(char c);

        /// <summary>
        /// Shift + ? を実行。
        /// </summary>
        /// <param name="name">サービス名</param>
        /// <param name="c">組み合わせ文字。アルファベットの場合は大文字を使う。</param>
        [WebGet]
        void SendShift(char c);

        /// <summary>
        /// コントロール系キー + ?を実行。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="c"></param>
        /// <param name="ctrlKey"></param>
        [WebGet]
        void SendCtrlInput(char c, params Key[] ctrlKey);




    }




    [ServiceContract(
        Namespace = "http://signpost.vbel.net/servicemodel",
        CallbackContract = typeof(ISignpostService))]
    public interface ISignpostService : ISignpostBaseService, ISignpostSubscriptionService, ISignpostActionService
    {
    }

    public interface ISignpostServiceChannel : ISignpostService, IClientChannel
    {
    }
}
