using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Neith.Crawler;
using System.Diagnostics;

namespace Neith.Crawler.Test
{
    using NUnit.Framework;
    //[TestFixture]
    public class WebAccessTest
    {
        [Test]
        public void WebGetTest()
        {
            var sync = new object();
            var text = "";
            Debug.WriteLine("WebGetTest[Start]");
            lock (sync) {
                using (var task =
                    "http://xfn.vbel.net/".RxGetWebContents()
                    .Finally(() =>
                    {
                        Debug.WriteLine("WebGetTest[Finally]");
                        lock (sync) Monitor.Pulse(sync);
                    })
                    .Subscribe(item =>
                    {
                        Debug.WriteLine("WebGetTest[Subscribe]");
                        text = item;
                    }
                    )) {
                    Debug.WriteLine("WebGetTest[Wait::Start]");
                    Monitor.Wait(sync);
                    Debug.WriteLine("WebGetTest[Wait::End]");
                }
            }
            Assert.IsFalse(string.IsNullOrEmpty(text));
            Debug.WriteLine("WebGetTest[result]\n" + text);
        }


        [Test]
        public void WebETagTest()
        {
            var sync = new object();
            var etag = "";
            lock (sync) {
                using (var task =
                    "http://xfn.vbel.net/".RxGetResponseHeader("ETag")
                    .Finally(() => { lock (sync)Monitor.Pulse(sync); })
                    .Subscribe(item => { etag = item; })
                    ) { Monitor.Wait(sync); }
            }
            Assert.IsFalse(string.IsNullOrEmpty(etag));
            Debug.WriteLine("ETag: " + etag);
        }


    }
}
