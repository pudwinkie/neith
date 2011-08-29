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
using System.Collections.Generic;
using System.Windows.Data;
using System.Xml.Linq;

using X = NetflixApp.Model.ODataXML;

namespace NetflixApp.Model
{
    public static class ODataXML
    {
        public static XName Name(string x) { return XName.Get(x, "http://www.w3.org/2005/Atom"); }
        public static XName dName(string x) { return XName.Get(x, "http://schemas.microsoft.com/ado/2007/08/dataservices"); }
        public static XName mName(string x) { return XName.Get(x, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"); }
    }
}
