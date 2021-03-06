﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Neith.Logger.Model;
using Neith.Logger.XIV;

namespace Neith.Logger.Test
{
    using NUnit.Framework;

    [TestFixture]
    public class XIVTest
    {
        //[Test]
        public void ReadTest()
        {
            Debug.WriteLine("XIVTest.ReadTest::20秒間試行開始");
            using (var collecter = new XIVCollecter())
            using (var task = Observable
                .FromEventPattern<NeithLogEventArgs>(collecter, "Collect")
                .Subscribe(a => Debug.WriteLine(a.EventArgs))) {
                System.Threading.Thread.Sleep(300 * 1000);
            }
            Debug.WriteLine("XIVTest.ReadTest::完了");
        }
        [Test]
        public void ReadDummyTest()
        {
            Debug.WriteLine("XIVTest.ReadDummyTest::5秒間試行開始");
            using (var collecter = new DummyXIVCollecter())
            using (var task = Observable
                .FromEventPattern<NeithLogEventArgs>(collecter, "Collect")
                .Subscribe(a => Debug.WriteLine(a.EventArgs))) {
                System.Threading.Thread.Sleep(5 * 1000);
            }
            Debug.WriteLine("XIVTest.ReadTest::完了");
        }
    }
}
