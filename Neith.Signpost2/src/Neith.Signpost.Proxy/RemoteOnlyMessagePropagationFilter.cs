using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Diagnostics;

namespace Neith.Signpost.Proxy
{
    /// <summary>
    /// 発信元がローカルの場合に、ローカルは受信せずリモートだけに送信するフィルタ。
    /// </summary>
    public class RemoteOnlyMessagePropagationFilter : PeerMessagePropagationFilter
    {

        public RemoteOnlyMessagePropagationFilter()
        {

        }

        public override PeerMessagePropagation ShouldMessagePropagate(Message message, PeerMessageOrigination origination)
        {
            return origination == PeerMessageOrigination.Local
                ? PeerMessagePropagation.Remote
                : PeerMessagePropagation.LocalAndRemote;
        }

    }
}