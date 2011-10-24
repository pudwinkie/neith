//#undef DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Neith.Util.Input
{
    internal static  class API
    {
        #region 定数定義
        internal const int MOUSEEVENTF_MOVE = 0x0001;
        internal const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        internal const int MOUSEEVENTF_LEFTUP = 0x0004;
        internal const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        internal const int MOUSEEVENTF_RIGHTUP = 0x0010;
        internal const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        internal const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        internal const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        internal const int MOUSEEVENTF_WHEEL = 0x0800;

        internal const int WHEEL_DELTA = 120;

        internal const int INPUT_MOUSE = 0;
        internal const int INPUT_KEYBOARD = 1;
        internal const int INPUT_HARDWARE = 2;

        internal const int MAPVK_VK_TO_VSC = 0x00;
        internal const int MAPVK_VSC_TO_VK = 0x01;
        internal const int MAPVK_VK_TO_CHAR = 0x02;
        internal const int MAPVK_VSC_TO_VK_EX = 0x03;
        internal const int MAPVK_VK_TO_VSC_EX = 0x04;

        ///<summary>メッセージコードを表す。</summary>
        internal enum WM : int
        {
            ///<summary>キーが押された。</summary>
            KEYDOWN = 0x0100,

            ///<summary>キーが放された。</summary>
            KEYUP = 0x0101,

            ///<summary>システムキーが押された。</summary>
            SYSKEYDOWN = 0x104,

            ///<summary>システムキーが放された。</summary>
            SYSKEYUP = 0x105,
        }

        #endregion
        #region 構造体
        [StructLayout(LayoutKind.Explicit)]
        internal struct Input
        {
            [FieldOffset(0)]
            public int type;
            [FieldOffset(4)]
            public MouseInput mi;
            [FieldOffset(4)]
            public KeybdInput ki;
            [FieldOffset(4)]
            public HardwareInput hi;
        }

        internal struct MouseInput
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public int dwExtraInfo;
            public MouseInput(int dx, int dy, int mouseData, int dwFlags, int time, int dwExtraInfo)
            {
                this.dx = dx;
                this.dy = dy;
                this.mouseData = mouseData;
                this.dwFlags = dwFlags;
                this.time = time;
                this.dwExtraInfo = dwExtraInfo;
            }
        }

        internal struct KeybdInput
        {
            public VK wVk;
            public ushort wScan;
            public KEYEVENTF dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        internal struct HardwareInput
        {
            public int uMsg;
            public ushort wParamL;
            public ushort wParamH;
            public HardwareInput(int uMsg, ushort wParamL, ushort wParamH)
            {
                this.uMsg = uMsg;
                this.wParamL = wParamL;
                this.wParamH = wParamH;
            }

        }


        #endregion
        #region API呼び出し(KEY送信)
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int SendInput(int nInputs, Input[] pInputs, int cbSize);


        [DllImport("user32.dll")]
        internal static extern IntPtr GetMessageExtraInfo();

        [DllImport("user32.dll")]
        internal static extern void keybd_event(byte bVk, byte bScan, int dwFlags, IntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        internal static extern int MapVirtualKeyEx(int uCode, int uMapType, IntPtr dwhkl);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        internal static extern uint SendMessage(IntPtr window, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();


        #endregion
        #region API呼び出し(KEY HOOK)
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr SetWindowsHookEx(int hookType, KeyboardHookDelegate hookDelegate, IntPtr hInstance, uint threadId);
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int CallNextHookEx(IntPtr hook, int code, WM message, ref KeyboardState state);
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool UnhookWindowsHookEx(IntPtr hook);

        internal delegate int KeyboardHookDelegate(int code, WM message, ref KeyboardState state);

        #endregion
        #region Input構造体の作成

        /// <summary>
        /// キー入力の送信
        /// </summary>
        /// <param name="vk"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        internal static int SendInput(VK vk, KEYEVENTF flags)
        {
            var inputs = new Input[1];
            var input = InputKeyboard(vk, flags);
            Debug.WriteLine(string.Format(
              "  KEYBOARD: VK[{0}] SCAN[0x{1:X2}] FLAG[{2}]",
              input.ki.wVk, input.ki.wScan, input.ki.dwFlags));
            inputs[0] = input;
            return SendInput(inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
        }


        /// <summary>
        /// キーコードよりInput構造体を生成します。
        /// </summary>
        /// <param name="vk"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        internal static Input InputKeyboard(VK vk, KEYEVENTF flags)
        {
            var layout = GetKeyboardLayout(0);
            var rc = new Input();
            rc.type = INPUT_KEYBOARD;
            rc.ki.wVk = vk;
            rc.ki.wScan = (ushort)MapVirtualKeyEx((int)vk, MAPVK_VK_TO_VSC, layout);
            rc.ki.dwFlags = flags;
            //rc.ki.dwExtraInfo = GetMessageExtraInfo();
            rc.ki.dwExtraInfo = IntPtr.Zero;
            rc.ki.time = 0;
            return rc;
        }

        #endregion
    }
}
