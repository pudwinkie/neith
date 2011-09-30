using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Signpost.Services
{
    public static class Const
    {
        public const int Port = 14080;
        public static readonly string BaseUrlString = string.Format("http://localhost:{0}/", Port);
        public const string SignpostServiceUrlString = "Signpost.svc";

        public static readonly Uri BaseUri = new Uri(BaseUrlString);
        public static readonly Uri ServiceUrl = new Uri(BaseUrlString + SignpostServiceUrlString);
        public static readonly Uri RelServiceUrl = new Uri(SignpostServiceUrlString,UriKind.Relative);

    }
}
