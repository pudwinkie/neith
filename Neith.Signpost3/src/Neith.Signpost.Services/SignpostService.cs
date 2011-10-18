using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Neith.Signpost.Services
{
    /// <summary>
    /// Signpostサービス。
    /// </summary>
    public class SignpostService : ISignpostContext
    {
        #region サーバ時刻取得
        public static DateTimeOffset GetServerTime()
        {
            var now = DateTimeOffset.Now;
            Debug.WriteLine("[SignpostService::GetServerTime] " + now);
            return now;
        }

        public IAsyncResult BeginGetServerTime(AsyncCallback callback, object state)
        {
            return GetServerTimeFunc.BeginInvoke(callback, state);
        }
        public DateTimeOffset EndGetServerTime(IAsyncResult result)
        {
            return GetServerTimeFunc.EndInvoke(result);
        }
        private delegate DateTimeOffset DelegateGetServerTime();
        private readonly DelegateGetServerTime GetServerTimeFunc = new DelegateGetServerTime(GetServerTime);


        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null) {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue) {
                composite.StringValue += "Suffix";
            }
            return composite;
        }


        #endregion
        #region キーイベントを発行



        public IAsyncResult BeginSendKeys(string command, AsyncCallback callback, object state)
        {
            var t1 = Neith.Util.Input.SendKeyInput.SendKeysAsync(command);
            Task<TimeSpan> t2 = new Task<TimeSpan>(a => t1.Result, state);
            t1.ContinueWith(a => t2.Start());
            t2.ContinueWith(a => callback(t2));
            return t2;
        }

        public TimeSpan EndSendKeys(IAsyncResult result)
        {
            var task = result as Task<TimeSpan>;
            return task.Result;
        }

        #endregion
    }
}
