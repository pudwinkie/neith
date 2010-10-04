using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace System.IO
{
    public static class RxIO
    {
        public static IObservable<string> ToContents(this IObservable<Stream> rxSt)
        {
            return rxSt
                .Select(st => {
                    using (st)
                    using (var reader = new StreamReader(st, Encoding.UTF8)) {
                        return reader.ReadToEnd();
                    }
                });
        }

    }
}
