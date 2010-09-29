using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Neith.Crawler.Util;

namespace Neith.Crawler.Sites.Neith
{
    public static class NeithXFN
    {
        public static IObservable<bool> ReadTypes()
        {
            return "http://spreadsheets.google.com/pub?key=0AlnLTLNQTaTJdGFZb1c2RTFuV01fUnBxbThNaGpWUXc&single=true&gid=0&output=csv"
                .RxGetUpdateWebResponseStream()
                .Select(st => {
                    if (st == null) return false;
                    using (st) {
                        foreach (var item in CsvUtil.ReadCsv(st, Encoding.UTF8)) {
                            Debug.WriteLine("[NeithXFN::ReadTypes]" + string.Join(",", item));
                        }
                    }
                    return true;
                });
        }

    }
}
