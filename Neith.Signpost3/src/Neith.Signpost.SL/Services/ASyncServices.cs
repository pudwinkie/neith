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

namespace Neith.Signpost.Services
{
    public static class ASyncServices
    {
        public static async Task<DateTimeOffset> GetServerTimeAsync(this ISignpostContext THIS)
        {
            return await Task.Factory.FromAsync(
                new Func<AsyncCallback, object, IAsyncResult>(THIS.BeginGetServerTime),
                new Func<IAsyncResult, DateTimeOffset>(THIS.EndGetServerTime), null);
        }

    }
}
