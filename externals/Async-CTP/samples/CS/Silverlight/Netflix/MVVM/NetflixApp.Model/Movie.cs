using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using X = NetflixApp.Model.ODataXML;

namespace NetflixApp.Model
{
    public class Movie : NetflixEntity
    {
        public Movie() { }

        public string Url { get; set; }
        public string BoxArtSmallUrl { get; set; }

        public override void LoadFromXML(System.Xml.Linq.XElement entry)
        {
            base.LoadFromXML(entry);

            var properties = entry.Element(X.mName("properties"));
            Url = properties.Element(X.dName("Url")).Value;
            BoxArtSmallUrl = properties.Element(X.dName("BoxArt")).Element(X.dName("SmallUrl")).Value;
        }
    }
}
