using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Neith.Interop.Win32
{
    public static class KeyboardAction
    {
        const int INPUT_KEYBOARD = 1;
        const int KEYEVENTF_EXTENDEDKEY = 0x1;
        const int KEYEVENTF_KEYUP = 0x2;
        const int KEYEVENTF_UNICODE = 0x4;

        const uint MAPVK_VK_TO_VSC = 0x00;
        const uint MAPVK_VSC_TO_VK = 0x01;
        const uint MAPVK_VK_TO_CHAR = 0x02;
        const uint MAPVK_VSC_TO_VK_EX = 0x03;
        const uint MAPVK_VK_TO_VSC_EX = 0x04;

        [DllImport("user32.dll")]
        private static extern int SendInput(int nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        private static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();

        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct INPUT
        {
            [FieldOffset(0)]
            public uint type;
            [FieldOffset(4)]
            public MOUSEINPUT mi;
            [FieldOffset(4)]
            public KEYBDINPUT ki;
            [FieldOffset(4)]
            public HARDWAREINPUT hi;
        }

        public static void SendInput(this string text)
        {
            var inputs = text.ToInput();
            SendInput(inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        // 拡張キーと制御コードのチェックはしない。
        private static INPUT[] ToInput(this string p)
        {
            int len = p.Length;
            INPUT[] inputs = new INPUT[len * 2];
            var extraInfo = GetMessageExtraInfo();
            for (int i = 0; i < len; i++) {
                int j = i * 2;
                inputs[j].type = INPUT_KEYBOARD;
                inputs[j].ki.dwFlags = KEYEVENTF_UNICODE;
                inputs[j].ki.wScan = p[i];
                inputs[j].ki.dwExtraInfo = extraInfo;

                int k = j + 1;
                inputs[k] = inputs[j];
                inputs[k].ki.dwFlags |= KEYEVENTF_KEYUP;
                inputs[k].ki.dwExtraInfo = extraInfo;
            }
            return inputs;
        }

        public static void SendAlt(this char c)
        {
            Key.LeftAlt.SendCtrlInput(c);
        }
        public static void SendCtrl(this char c)
        {
            Key.LeftCtrl.SendCtrlInput(c);
        }
        public static void SendShift(this char c)
        {
            Key.LeftShift.SendCtrlInput(c);
        }


        public static void SendCtrlInput(this Key ctrlKey, char c)
        {
            var ctrlVK = (ushort)KeyInterop.VirtualKeyFromKey(ctrlKey);
            var ctrlSC = (ushort)MapVirtualKeyEx(ctrlVK, MAPVK_VK_TO_VSC, GetKeyboardLayout(0));

            var charVK = (ushort)c;
            var charSC = (ushort)MapVirtualKey(charVK, 0);

            var inputs = new INPUT[4];
            var extraInfo = GetMessageExtraInfo();
            for (var i = 0; i < inputs.Length; i++) inputs[i].ki.dwExtraInfo = extraInfo;

            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].ki.wVk = ctrlVK;
            inputs[0].ki.wScan = ctrlSC;
            inputs[0].ki.dwFlags = KEYEVENTF_EXTENDEDKEY;

            inputs[1].type = INPUT_KEYBOARD;
            inputs[1].ki.wVk = charVK;
            inputs[1].ki.wScan = charSC;
            inputs[1].ki.dwFlags = KEYEVENTF_EXTENDEDKEY;

            inputs[2].type = INPUT_KEYBOARD;
            inputs[2].ki.wVk = charVK;
            inputs[2].ki.wScan = charSC;
            inputs[2].ki.dwFlags = KEYEVENTF_KEYUP;

            inputs[3].type = INPUT_KEYBOARD;
            inputs[3].ki.wVk = ctrlVK;
            inputs[3].ki.wScan = ctrlSC;
            inputs[3].ki.dwFlags = KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP;

            SendInput(inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

    }
}