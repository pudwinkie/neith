using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Neith.Signpost.Services
{
    [ServiceContract]
    public interface IStaticContents
    {
        [WebGet(UriTemplate = "{a}.html")]
        Stream GetHtml(string a);

        [WebGet(UriTemplate = "{a}.js")]
        Stream GetJS(string a);

        [WebGet(UriTemplate = "ClientBin/{a}.xap")]
        Stream GetXap(string a);

    }
}