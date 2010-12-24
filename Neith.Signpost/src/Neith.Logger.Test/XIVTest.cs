﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Neith.Logger.XIV;
using Neith.Logger.Model;

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
            using (var task = Observable
                .FromEvent<LogEventArgs>(collecter, "Collect")
                .Subscribe(a => Debug.WriteLine(a))) {
                System.Threading.Thread.Sleep(20 * 1000);
            }
            Debug.WriteLine("XIVTest.ReadTest::完了");
        }
    }
}
