using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using ReactiveUI.Xaml;

namespace Neith.Logger.Test.Study.ReactiveUI
{
    using NUnit.Framework;

    [TestFixture]
    public class ReactiveCommandTest
    {
        [Test]
        public void Test1()
        {
            try {
                NewMethod();
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
                throw;
            }
        }

        private static void NewMethod()
        {
            var rc = "";

            var cmd = ReactiveCommand.Create(a => a is int, scheduler: Scheduler.CurrentThread);
            cmd.Where(a => ((int)a) % 2 == 0)
               .Subscribe(a => { rc = string.Format("{0} is Even", a); });
            cmd.Where(a => ((int)a) % 2 != 0)
               .Subscribe(a => { rc = string.Format("{0} is Odd", a); });

            cmd.Execute(2);
            rc.AreEqual("2 is Even");
        }
    }
}
