using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace Neith.Logger.Test.Study.WPF
{
    using NUnit.Framework;

    [TestFixture]
    public class VTreeViewTestWindowTest
    {
        [Test]
        public void Test1()
        {
            var mylock = new object();
            DispatcherScheduler sc = null;
            var t = new Thread(() =>
            {
                sc = new DispatcherScheduler(Dispatcher.CurrentDispatcher);
                lock (mylock) Monitor.PulseAll(mylock);
                Dispatcher.Run();
            });
            t.SetApartmentState(ApartmentState.STA);
            t.IsBackground = true;
            lock (mylock) {
                t.Start();
                Monitor.Wait(mylock);
            }
            Observable.Start(() =>
            {
                var win = new VTreeViewTestWindow();
                win.ShowDialog();
                sc.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
                return Unit.Default;
            }, sc);
            t.Join();
        }
    }
}
