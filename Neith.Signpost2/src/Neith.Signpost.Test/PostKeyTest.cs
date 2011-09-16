using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using Neith.Interop.Win32;

namespace Neith.Signpost.Test
{
    using NUnit.Framework;
    public class PostKeyTest : AssertionHelper
    {
        //[Test]
        public void TestMethod1()
        {
            'F'.SendShift();
            var text = "ゆにこーどしけん\r\n";
            Console.Clear();
            var oRead = Observable.Start(() => Console.ReadLine(), Scheduler.NewThread);
            var oInput = Observable.Start(() => text.SendInput(), Scheduler.NewThread);
            using (oInput.Subscribe()) {
                var rc = oRead.First();
                Expect(rc, Is.SameAs(text));
            }
        }
    }
}