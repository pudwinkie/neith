using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Neith.Interop.Win32
{
    public static class KeyboardAction
    {
        public static void PostDown(IntPtr hWnd, Key key)
        {
            var vKey = KeyInterop.VirtualKeyFromKey(key);
            NativeMethods.PostMessage(hWnd, WindowMessage.KeyDown, (IntPtr)vKey, IntPtr.Zero);
        }

        public static void PostUp(IntPtr hWnd, Key key)
        {
            var vKey = KeyInterop.VirtualKeyFromKey(key);
            NativeMethods.PostMessage(hWnd, WindowMessage.KeyUp, (IntPtr)vKey, IntPtr.Zero);
        }


    }
}
