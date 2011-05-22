using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Neith.Logger.Test.Study.WPF
{
    using NUnit.Framework;

    //[TestFixture]
    public class VTreeViewTestWindowTest
    {
        [Test]
        public void Test1()
        {
            // VisualStudioがあればスナップ対象にする
            var q1 = from p in Process.GetProcesses()
                     let Name = p.ProcessName
                     let IsWindow = (p.MainWindowHandle != IntPtr.Zero)
                     let WinName = p.MainWindowTitle
                     let HWND = p.MainWindowHandle
                     where HWND != IntPtr.Zero && Name == "devenv"
                     select new { Process = p, Name, IsWindow, WinName, HWND };
            var vs = q1.FirstOrDefault();

            try {
                if (vs != null) { Blue.Windows.StickyWindowWPF.Register(vs.HWND); }

                // ウィンドウの起動
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
            finally {
                if (vs != null) { Blue.Windows.StickyWindowWPF.Unregister(vs.HWND); }
            }
        }
    }
}