using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using X = NetflixApp.Model.ODataXML;

namespace NetflixApp.Model
{
    public abstract class NetflixEntity
    {
        public string Title { get; set; }

        public virtual void LoadFromXML(XElement entry)
        {
            Title = entry.Element(X.Name("title")).Value;
        }
    }
}
