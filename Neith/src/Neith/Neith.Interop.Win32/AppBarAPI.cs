﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace Neith.Interop
{
    internal static class AppBarAPI
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public int Top { get { return top; } }
            public int Left { get { return left; } }
            public int Width { get { return right - left; } }
            public int Height { get { return bottom - top; } }

            public Rect RectWPF
            {
                get { return new Rect(Top, Left, Width, Height); }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public RECT rc;
            public IntPtr lParam;
        }

        public enum ABMsg : int
        {
            ABM_NEW = 0,
            ABM_REMOVE,
            ABM_QUERYPOS,
            ABM_SETPOS,
            ABM_GETSTATE,
            ABM_GETTASKBARPOS,
            ABM_ACTIVATE,
            ABM_GETAUTOHIDEBAR,
            ABM_SETAUTOHIDEBAR,
            ABM_WINDOWPOSCHANGED,
            ABM_SETSTATE
        }
        public enum ABNotify : int
        {
            ABN_STATECHANGE = 0,
            ABN_POSCHANGED,
            ABN_FULLSCREENAPP,
            ABN_WINDOWARRANGE
        }
        public enum ABEdge : int
        {
            ABE_LEFT = 0,
            ABE_TOP,
            ABE_RIGHT,
            ABE_BOTTOM
        }

        [DllImport("SHELL32", CallingConvention = CallingConvention.StdCall)]
        public static extern uint SHAppBarMessage(int dwMessage, ref APPBARDATA pData);
        [DllImport("USER32")]
        public static extern int GetSystemMetrics(int Index);
        [DllImport("User32.dll", ExactSpelling = true,
            CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern bool MoveWindow
            (IntPtr hWnd, int x, int y, int cx, int cy, bool repaint);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int RegisterWindowMessage(string msg);


        public static Rect GetTaskbarRect()
        {
            var bar=new APPBARDATA();
            bar.cbSize = Marshal.SizeOf(bar);
            SHAppBarMessage((int)ABMsg.ABM_GETTASKBARPOS, ref bar);
            return bar.rc.RectWPF;
        }

    }
}
