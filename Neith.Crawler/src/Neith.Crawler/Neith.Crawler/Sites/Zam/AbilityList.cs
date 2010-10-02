using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Diagnostics;
using System.Text;

namespace Neith.Crawler.Sites.Zam
{
    public static class AbilityList
    {
        public static IObservable<bool> Task()
        {
            return @"http://ffxiv.zam.com/ja/abilitylist.html"
                .RxGetUpdateWebResponseStream()
                .Select(st =>
                {
                    using (st)
                    using (var reader = new StreamReader(st, Encoding.UTF8))
                    {
                        return XElement.Load(reader);
                    }
                })
                .Select(doc =>
                {
                    Debug.WriteLine(doc.ToString());
                    return true;
                })
                .TakeLast(0);
        }


    }
}
