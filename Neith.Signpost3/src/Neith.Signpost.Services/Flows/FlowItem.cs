using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Neith.Signpost.Services.Flows
{
    public class FlowItem<T> : IFlowItem<T>
    {
        /// <summary>要求者。</summary>
        public string Sender { get; set; }

        /// <summary>要求ID。</summary>
        public long SendID { get; private set; }

        /// <summary>要求日時。</summary>
        public DateTime SendTime { get; private set; }

        /// <summary>ターゲット。誰に対する要求なのか。</summary>
        public string Target { get; set; }

        /// <summary>要求名。何をして欲しいのか。</summary>
        public string Request { get; set; }

        /// <summary>要求先。誰にして欲しいのか。</summary>
        public string To { get; set; }

        /// <summary>優先度。高いほど先に実行される。</summary>
        public double Rate { get; set; }

        /// <summary>ターゲットのオリジナル情報。</summary>
        public T Value { get; set; }

        /// <summary>ターゲットのオリジナル情報。</summary>
        object IFlowItem.Value { get { return Value; } }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public FlowItem(T value)
        {
            SendID = FlowItemExtensions.GetSendID();
            SendTime = DateTime.Now;
            Value = value;
        }
    }

    public static class FlowItemExtensions
    {
        internal static long GetSendID()
        {
            return Interlocked.Increment(ref _GetSendID);
        }
        private static long _GetSendID;


    }

}
