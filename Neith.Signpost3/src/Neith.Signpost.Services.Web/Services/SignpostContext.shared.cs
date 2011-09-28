using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Signpost.Services
{
    public partial class SignpostContext
    {
        public const string BaseUrlString = "DomainServices/";
        public const string ServiceNameString = "Neith-Signpost-Services-SignpostService.svc";

        public static Uri ServiceUrl
        {
            get
            {
                var uri = new Uri(BaseUrlString + ServiceNameString, UriKind.Relative);
                return uri;
            }
        }
    }
}