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
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetServerTime(AsyncCallback callback, object state);
        DateTimeOffset EndGetServerTime(IAsyncResult result);

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
