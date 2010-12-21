using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Neith.Logger.XIV;

namespace Neith.Logger.Test
{
    using NUnit.Framework;

    [TestFixture]
    public class XIVTest
    {
        [Test]
        public void ReadTest()
        {
            Debug.WriteLine("XIVTest.ReadTest::20秒間試行開始");
            using (var collecter = new XIVCollecter())
            using (var task = collecter
                .RxCollect()
                .Subscribe(a => Debug.WriteLine(a))) {
                System.Threading.Thread.Sleep(60 * 1000);
            }
            Debug.WriteLine("XIVTest.ReadTest::完了");
        }
    }
}
