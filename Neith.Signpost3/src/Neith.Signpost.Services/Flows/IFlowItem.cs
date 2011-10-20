using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Signpost.Services.Flows
{
    public interface IFlowItem
    {
        /// <summary>要求者。</summary>
        string Sender { get; }

        /// <summary>要求ID。</summary>
        long SendID { get; }

        /// <summary>要求日時。</summary>
        DateTime SendTime { get; }

        /// <summary>ターゲット。誰に対する要求なのか。</summary>
        string Target { get; }

        /// <summary>要求名。何をして欲しいのか。</summary>
        string Request { get; }

        /// <summary>要求先。誰にして欲しいのか。</summary>
        string To { get; }

        /// <summary>優先度。高いほど先に実行される。</summary>
        double Rate { get; }

        /// <summary>ターゲットのオリジナル情報。</summary>
        object Value { get; }
    }

    public interface IFlowItem<T> : IFlowItem
    {
        /// <summary>ターゲットのオリジナル情報。</summary>
        new T Value { get; }
    }

}
