using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Threading.Tasks;
using System.ServiceModel;

namespace Neith.Signpost.Services
{
    public class Channels
    {
        private static ISignpostChannel signpostCH = null;
        private static readonly Task<ISignpostChannel> initTask = CreateSignpostChannelAsync();

        public static async Task<ISignpostChannel> GetSignpostChannelAsync()
        {
            if (signpostCH == null) {
                signpostCH = await TaskEx.RunEx(() => initTask);
            }
            return signpostCH;
        }

        private static async Task<ISignpostChannel> CreateSignpostChannelAsync()
        {
            var fact = new ChannelFactory<ISignpostChannel>(new BasicHttpBinding(), new EndpointAddress(Const.SignpostServiceUrlString));
            var ch = fact.CreateChannel();
            await ch.OpenAsync();
            return ch;
        }

    }

    public interface ISignpostChannel : ISignpostContext, IClientChannel
    {
    }
}
