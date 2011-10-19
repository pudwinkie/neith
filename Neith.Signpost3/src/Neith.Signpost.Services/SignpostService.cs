﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Neith.Threading.Tasks;

namespace Neith.Signpost.Services
{
    /// <summary>
    /// Signpostサービス。
    /// </summary>
    public class SignpostService : ISignpostContext
    {
        #region サーバ時刻取得
        public static  DateTimeOffset GetServerTime()
        {
            var now = DateTimeOffset.Now;
            Debug.WriteLine("[SignpostService::GetServerTime] " + now);
            return now;
        }
        private static readonly Func<DateTimeOffset> ApmGetServerTime = GetServerTime;


        public IAsyncResult BeginGetServerTime(AsyncCallback callback, object state)
        {
            return ApmGetServerTime.BeginInvoke(callback, state);
        }
        public DateTimeOffset EndGetServerTime(IAsyncResult result)
        {
            return ApmGetServerTime.EndInvoke(result);
        }


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
        //private static readonly IApm<string, TimeSpan> ApmSendKeys = NeithTaskEx.ToApm<string, TimeSpan>(Neith.Util.Input.SendKeyInput.SendKeysAsync);
        private static readonly IApm<string, TimeSpan> ApmSendKeys = NeithTaskEx.ToApm<string, TimeSpan>(Neith.Util.Input.SendKeysWinform.SendWaitExAsync);

        public IAsyncResult BeginSendKeys(string command, AsyncCallback callback, object state)
        {
            Debug.WriteLine("[SignpostService::BeginSendKeys] command=" + command);
            return ApmSendKeys.BeginInvoke(command, callback, state);
        }

        public TimeSpan EndSendKeys(IAsyncResult result)
        {
            return ApmSendKeys.EndInvoke(result);
        }

        #endregion
    }
}
