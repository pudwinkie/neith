using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows;

namespace Neith.Logger.Test
{
    public static class API
    {
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public int X { get { return left; } }
            public int Y { get { return top; } }
            public int Width { get { return right - left; } }
            public int Height { get { return bottom - top; } }
            public Int32Rect ToInt32Rect()
            {
                return new Int32Rect(X, Y, Width, Height);
            }
        }

        [DllImport("User32.Dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowRect(
               IntPtr hWnd,      // ウィンドウのハンドル
               out RECT rect   // ウィンドウの座標値
               );

        public static Int32Rect GetWindowRect(this Process process)
        {
            var rect = new RECT();
            if (GetWindowRect(process.MainWindowHandle, out rect) == 0) {
                var hResult = Marshal.GetHRForLastWin32Error();
                Marshal.ThrowExceptionForHR(hResult);
            }
            return rect.ToInt32Rect();
        }

    }
}
