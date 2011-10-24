using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Neith.Signpost.Services
{
    /// <summary>
    /// Signpostサービスコンテキスト。
    /// </summary>
    [ServiceContract]
    public interface ISignpostContext
    {
#if SILVERLIGHT
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetDataUsingDataContract(CompositeType composite, AsyncCallback callback, object state);
        CompositeType EndGetDataUsingDataContract(IAsyncResult result);
#else
        [OperationContract]
        CompositeType GetDataUsingDataContract(CompositeType composite);
#endif
        /// <summary>
        /// サーバ時刻を取得します。
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetServerTime(AsyncCallback callback, object state);
        DateTimeOffset EndGetServerTime(IAsyncResult result);

        /// <summary>
        /// 指定された文字列のキーイベントを発行します。
        /// 特殊キー入力モードがあります。
        /// </summary>
        /// <remarks>
        /// 特殊キーモード
        ///  *x →[x:F1～F12(1～0,-,^)] FUNCキーを押して離す
        ///       1～0 F1～F10
        ///       -    F11
        ///       ^    F12
        ///  [x →[x:C,A,X] 特殊キーを押す
        ///       C CTRL
        ///       A ALT
        ///       X CTRL+ALT
        ///  ]  →特殊キーを離す
        ///  +  →[Shift+0]キーを押す
        ///  |  →[漢字]キーを押す
        ///  ~  →[Enter]キーを押す
        ///  @  →SendMessageを利用して前面窓に[Enter]キーを押す
        ///  _  →キー入力キューが無くなるまで待機し、その後約50ms待機する
        ///  :  →次のキーのとき、押して離す間に50ms待機する
        /// 特殊キーモードで利用しているキーを押したいとき
        /// * + 押したいキーでエスケープ
        /// </remarks>
        /// <param name="command">キーコマンド</param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginSendKeys(string command, AsyncCallback callback, object state);
        TimeSpan EndSendKeys(IAsyncResult result);

        /// <summary>
        /// ログ情報を取得します。最大2秒待機します。
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetLogs(AsyncCallback callback, object state);
        string[] EndGetLogs(IAsyncResult result);


    }

    // サービス操作に複合型を追加するには、以下のサンプルに示すようにデータ コントラクトを使用します。
    [DataContract]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }
}
