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
        public static async Task<DateTimeOffset> GetServerTimeAsync(this ISignpostChannel THIS)
        {
            var task = new Task<DateTimeOffset>(THIS.GetServerTime);
            return await task;
        }

    }
}
