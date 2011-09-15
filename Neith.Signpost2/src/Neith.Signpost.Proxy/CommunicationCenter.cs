using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Neith.Signpost.Proxy
{
    public static class CommunicationCenter
    {
        /// <summary>
        /// Peerチャンネルと接続します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="sourceObject"></param>
        /// <param name="p2pUrl"></param>
        /// <param name="portNumber"></param>
        /// <returns></returns>
        public static U OpenPeerChannel<T, U>(T sourceObject,
            string p2pUrl,
            int portNumber) where U : IClientChannel
        {
            InstanceContext sourceContext = new InstanceContext(sourceObject);
            NetPeerTcpBinding binding = new NetPeerTcpBinding()
            {
                Port = portNumber,
                Name = p2pUrl + "@" + portNumber
            };

            binding.Security.Mode = SecurityMode.None;
            EndpointAddress address = new EndpointAddress(p2pUrl);
            DuplexChannelFactory<U> sourceFactory =
                new DuplexChannelFactory<U>(sourceContext, binding, address);
            U sourceProxy = (U)sourceFactory.CreateChannel();

            RemoteOnlyMessagePropagationFilter remoteOnlyFilter = new RemoteOnlyMessagePropagationFilter();

            PeerNode peerNode = ((IClientChannel)sourceProxy).GetProperty<PeerNode>();
            peerNode.MessagePropagationFilter = remoteOnlyFilter;


            sourceProxy.Open();
            return sourceProxy;
        }
    }
}
