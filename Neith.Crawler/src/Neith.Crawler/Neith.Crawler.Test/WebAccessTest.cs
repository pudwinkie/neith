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
    [TestFixture]
    public class WebAccessTest
    {
        [Test]
        public void WebGetTest()
        {
            var text = "";
            "http://xfn.vbel.net/"
                .RxGetWebContents()
                .Run(item => {
                    text = item;
                });
            Assert.IsFalse(string.IsNullOrEmpty(text));
            Debug.WriteLine("WebGetTest[result]\n" + text);
        }


        [Test]
        public void WebETagTest()
        {
            var etag = "";
            "http://xfn.vbel.net/"
                .RxGetResponseHeader("ETag")
                .Run(item => { etag = item; });
            Assert.IsFalse(string.IsNullOrEmpty(etag));
            Debug.WriteLine("ETag: " + etag);
        }


        [Test]
        public void ClowlGetTest()
        {
            var rc1 = "";
            "http://xfn.vbel.net/"
                .RxGetExWebContents()
                .Run(item => {
                    rc1 = item;
                });
            Assert.IsFalse(string.IsNullOrEmpty(rc1));

            var rc2 = "";
            "http://xfn.vbel.net/"
                .RxGetUpdateWebContents()
                .Run(item => {
                    rc2 = item;
                });
            Assert.IsNull(rc2);

        }


    }
}
