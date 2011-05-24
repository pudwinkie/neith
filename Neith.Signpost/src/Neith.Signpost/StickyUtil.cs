using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using Neith.Logger.Model;
using System.Reactive.Linq;
using Blue.Windows;

namespace Neith.Signpost
{
    /// <summary>
    /// ウィンドウくっつき制御
    /// </summary>
    public static class StickyUtil
    {
        static StickyUtil()
        {
            if (Debugger.IsAttached) {
                var p = Process.GetProcessesByName("devenv")
                    .Where(a => a.MainWindowHandle != IntPtr.Zero)
                    .FirstOrDefault();
                if (p != null) StickyWindowWPF.Register(p.MainWindowHandle);
            }
        }

        /// <summary>
        /// ロガーが注目しているウィンドウにくっつくようにする。
        /// </summary>
        /// <param name="rxArgs"></param>
        /// <returns></returns>
        public static IDisposable RxLogReceive(this IObservable<NeithLogEventArgs> rxArgs)
        {
            var lastHWnd = IntPtr.Zero;
            return rxArgs
                .Subscribe(a =>
                {
                    var hwnd = a.Log.HWnd;
                    if (hwnd == IntPtr.Zero) return;
                    if (hwnd == lastHWnd) return;
                    if (lastHWnd != IntPtr.Zero) {
                        StickyWindowWPF.Unregister(lastHWnd);
                    }
                    StickyWindowWPF.Register(hwnd);
                    lastHWnd = hwnd;
                });
        }
    }
}