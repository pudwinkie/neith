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
    public static class SendKeyInput
    {
        #region 定数定義
        const int MOUSEEVENTF_MOVE = 0x0001;
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const int MOUSEEVENTF_RIGHTUP = 0x0010;
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        const int MOUSEEVENTF_WHEEL = 0x0800;

        const int WHEEL_DELTA = 120;

        const int INPUT_MOUSE = 0;
        const int INPUT_KEYBOARD = 1;
        const int INPUT_HARDWARE = 2;

        [Flags]
        public enum KEYEVENTF : int
        {
            EXTENDEDKEY = 0x0001,
            KEYDOWN = 0x0000,
            KEYUP = 0x0002,
            UNICODE = 0x0004,
            SCANCODE = 0x0008,
        }

        public enum WM : int
        {
            KEYDOWN = 0x0100,
            KEYUP = 0x0101,
        }

        #region キーコード
        public enum VK : ushort
        {
            /*
             * Virtual Keys, Standard Set
             */
            LBUTTON = 0x01,
            RBUTTON = 0x02,
            CANCEL = 0x03,
            MBUTTON = 0x04,    /* NOT contiguous with L & RBUTTON */

            XBUTTON1 = 0x05,    /* NOT contiguous with L & RBUTTON */
            XBUTTON2 = 0x06,    /* NOT contiguous with L & RBUTTON */

            /*
             * 0x07 : unassigned
             */

            BACK = 0x08,
            TAB = 0x09,

            /*
             * 0x0A - 0x0B : reserved
             */

            CLEAR = 0x0C,
            RETURN = 0x0D,

            SHIFT = 0x10,
            CONTROL = 0x11,
            MENU = 0x12,
            PAUSE = 0x13,
            CAPITAL = 0x14,

            KANA = 0x15,
            HANGEUL = 0x15,  /* old name - should be here for compatibility */
            HANGUL = 0x15,
            JUNJA = 0x17,
            FINAL = 0x18,
            HANJA = 0x19,
            KANJI = 0x19,

            ESCAPE = 0x1B,

            CONVERT = 0x1C,
            NONCONVERT = 0x1D,
            ACCEPT = 0x1E,
            MODECHANGE = 0x1F,

            SPACE = 0x20,
            PRIOR = 0x21,
            NEXT = 0x22,
            END = 0x23,
            HOME = 0x24,
            LEFT = 0x25,
            UP = 0x26,
            RIGHT = 0x27,
            DOWN = 0x28,
            SELECT = 0x29,
            PRINT = 0x2A,
            EXECUTE = 0x2B,
            SNAPSHOT = 0x2C,
            INSERT = 0x2D,
            DELETE = 0x2E,
            HELP = 0x2F,

            _0 = 0x30 + 0,
            _1 = 0x30 + 1,
            _2 = 0x30 + 2,
            _3 = 0x30 + 3,
            _4 = 0x30 + 4,
            _5 = 0x30 + 5,
            _6 = 0x30 + 6,
            _7 = 0x30 + 7,
            _8 = 0x30 + 8,
            _9 = 0x30 + 9,

            A = 0x41 + 0,
            B = 0x41 + 1,
            C = 0x41 + 2,
            D = 0x41 + 3,
            E = 0x41 + 4,
            F = 0x41 + 5,
            G = 0x41 + 6,
            H = 0x41 + 7,
            I = 0x41 + 8,
            J = 0x41 + 9,
            K = 0x41 + 10,
            L = 0x41 + 11,
            M = 0x41 + 12,
            N = 0x41 + 13,
            O = 0x41 + 14,
            P = 0x41 + 15,
            Q = 0x41 + 16,
            R = 0x41 + 17,
            S = 0x41 + 18,
            T = 0x41 + 19,
            U = 0x41 + 20,
            V = 0x41 + 21,
            W = 0x41 + 22,
            X = 0x41 + 23,
            Y = 0x41 + 24,
            Z = 0x41 + 25,

            LWIN = 0x5B,
            RWIN = 0x5C,
            APPS = 0x5D,

            /*
             * 0x5E : reserved
             */

            SLEEP = 0x5F,

            NUMPAD0 = 0x60,
            NUMPAD1 = 0x61,
            NUMPAD2 = 0x62,
            NUMPAD3 = 0x63,
            NUMPAD4 = 0x64,
            NUMPAD5 = 0x65,
            NUMPAD6 = 0x66,
            NUMPAD7 = 0x67,
            NUMPAD8 = 0x68,
            NUMPAD9 = 0x69,
            MULTIPLY = 0x6A,
            ADD = 0x6B,
            SEPARATOR = 0x6C,
            SUBTRACT = 0x6D,
            DECIMAL = 0x6E,
            DIVIDE = 0x6F,
            F1 = 0x70,
            F2 = 0x71,
            F3 = 0x72,
            F4 = 0x73,
            F5 = 0x74,
            F6 = 0x75,
            F7 = 0x76,
            F8 = 0x77,
            F9 = 0x78,
            F10 = 0x79,
            F11 = 0x7A,
            F12 = 0x7B,
            F13 = 0x7C,
            F14 = 0x7D,
            F15 = 0x7E,
            F16 = 0x7F,
            F17 = 0x80,
            F18 = 0x81,
            F19 = 0x82,
            F20 = 0x83,
            F21 = 0x84,
            F22 = 0x85,
            F23 = 0x86,
            F24 = 0x87,

            /*
             * 0x88 - 0x8F : unassigned
             */

            NUMLOCK = 0x90,
            SCROLL = 0x91,

            /*
             * NEC PC-9800 kbd definitions
             */
            OEM_NEC_EQUAL = 0x92,   // '=' key on numpad

            /*
             * Fujitsu/OASYS kbd definitions
             */
            OEM_FJ_JISHO = 0x92,   // 'Dictionary' key
            OEM_FJ_MASSHOU = 0x93,   // 'Unregister word' key
            OEM_FJ_TOUROKU = 0x94,   // 'Register word' key
            OEM_FJ_LOYA = 0x95,   // 'Left OYAYUBI' key
            OEM_FJ_ROYA = 0x96,   // 'Right OYAYUBI' key

            /*
             * 0x97 - 0x9F : unassigned
             */

            /*
             * VK.L* & VK.R* - left and right Alt, Ctrl and Shift virtual keys.
             * Used only as parameters to GetAsyncKeyState() and GetKeyState().
             * No other API or message will distinguish left and right keys in this way.
             */
            LSHIFT = 0xA0,
            RSHIFT = 0xA1,
            LCONTROL = 0xA2,
            RCONTROL = 0xA3,
            LMENU = 0xA4,
            RMENU = 0xA5,

            BROWSER_BACK = 0xA6,
            BROWSER_FORWARD = 0xA7,
            BROWSER_REFRESH = 0xA8,
            BROWSER_STOP = 0xA9,
            BROWSER_SEARCH = 0xAA,
            BROWSER_FAVORITES = 0xAB,
            BROWSER_HOME = 0xAC,

            VOLUME_MUTE = 0xAD,
            VOLUME_DOWN = 0xAE,
            VOLUME_UP = 0xAF,
            MEDIA_NEXT_TRACK = 0xB0,
            MEDIA_PREV_TRACK = 0xB1,
            MEDIA_STOP = 0xB2,
            MEDIA_PLAY_PAUSE = 0xB3,
            LAUNCH_MAIL = 0xB4,
            LAUNCH_MEDIA_SELECT = 0xB5,
            LAUNCH_APP1 = 0xB6,
            LAUNCH_APP2 = 0xB7,


            /*
             * 0xB8 - 0xB9 : reserved
             */

            OEM_1 = 0xBA,   // ',:' for US
            OEM_PLUS = 0xBB,   // '+' any country
            OEM_COMMA = 0xBC,   // ',' any country
            OEM_MINUS = 0xBD,   // '-' any country
            OEM_PERIOD = 0xBE,   // '.' any country
            OEM_2 = 0xBF,   // '/?' for US
            OEM_3 = 0xC0,   // '`~' for US

            /*
             * 0xC1 - 0xD7 : reserved
             */

            /*
             * 0xD8 - 0xDA : unassigned
             */

            OEM_4 = 0xDB,  //  '[{' for US
            OEM_5 = 0xDC,  //  '\|' for US
            OEM_6 = 0xDD,  //  ']}' for US
            OEM_7 = 0xDE,  //  ''"' for US
            OEM_8 = 0xDF,

            /*
             * 0xE0 : reserved
             */

            /*
             * Various extended or enhanced keyboards
             */
            OEM_AX = 0xE1,  //  'AX' key on Japanese AX kbd
            OEM_102 = 0xE2,  //  "<>" or "\|" on RT 102-key kbd.
            ICO_HELP = 0xE3,  //  Help key on ICO
            ICO_00 = 0xE4,  //  00 key on ICO

            PROCESSKEY = 0xE5,

            ICO_CLEAR = 0xE6,


            PACKET = 0xE7,

            /*
             * 0xE8 : unassigned
             */

            /*
             * Nokia/Ericsson definitions
             */
            OEM_RESET = 0xE9,
            OEM_JUMP = 0xEA,
            OEM_PA1 = 0xEB,
            OEM_PA2 = 0xEC,
            OEM_PA3 = 0xED,
            OEM_WSCTRL = 0xEE,
            OEM_CUSEL = 0xEF,
            OEM_ATTN = 0xF0,
            OEM_FINISH = 0xF1,
            OEM_COPY = 0xF2,
            OEM_AUTO = 0xF3,
            OEM_ENLW = 0xF4,
            OEM_BACKTAB = 0xF5,

            ATTN = 0xF6,
            CRSEL = 0xF7,
            EXSEL = 0xF8,
            EREOF = 0xF9,
            PLAY = 0xFA,
            ZOOM = 0xFB,
            NONAME = 0xFC,
            PA1 = 0xFD,
            OEM_CLEAR = 0xFE,

            // ここからは内部処理用のオリジナルキーコード
            CMD_EX_ENTER_KEY = 0x4000 + 1,
        }
        #endregion
        #endregion
        #region 構造体
        [StructLayout(LayoutKind.Explicit)]
        struct Input
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

        struct MouseInput
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

        struct KeybdInput
        {
            public VK wVk;
            public ushort wScan;
            public KEYEVENTF dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        struct HardwareInput
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
        #region API呼び出し
        [DllImport("user32.dll", SetLastError = true)]
        static private extern int mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        static private extern int SendInput(int nInputs, Input[] pInputs, int cbSize);


        [DllImport("user32.dll")]
        static private extern IntPtr GetMessageExtraInfo();

        [DllImport("user32.dll")]
        static private extern void keybd_event(byte bVk, byte bScan, int dwFlags, IntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        static private extern int MapVirtualKey(int uCode, int uMapType);

        [DllImport("user32.dll")]
        static extern uint SendMessage(IntPtr window, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        #endregion
        #region APIの非同期呼び出し

        /// <summary>
        /// キー入力の送信。
        /// </summary>
        /// <param name="nInputs"></param>
        /// <param name="pInputs"></param>
        /// <param name="cbSize"></param>
        /// <returns></returns>
        static private Task<int> SendInputAsync(int nInputs, Input[] pInputs, int cbSize)
        {
            var func = new AsyncSendInputCaller(SendInput);
            return Task.Factory.FromAsync<int, Input[], int, int>(
                func.BeginInvoke,
                func.EndInvoke,
                nInputs, pInputs, cbSize,
                null);
        }
        private delegate int AsyncSendInputCaller(int nInputs, Input[] pInputs, int cbSize);


        /// <summary>
        /// キー入力の送信
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="vk"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private static async Task<int> SendInputAsync(Input[] inputs, VK vk, KEYEVENTF flags)
        {
            var input = InputKeyboard(vk, flags);
            Debug.WriteLine(string.Format(
              "  KEYBOARD: VK[{0}] SCAN[0x{1:X2}] FLAG[{2}]",
              input.ki.wVk, input.ki.wScan, input.ki.dwFlags));
            inputs[0] = input;
            return await SendInputAsync(inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
        }


        /// <summary>
        /// SendMessage。
        /// </summary>
        /// <param name="window"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        static private Task<uint> SendMessageAsync(IntPtr window, int msg, IntPtr wParam, IntPtr lParam)
        {
            var param = new SendMessageParam()
            {
                window = window,
                msg = msg,
                wParam = wParam,
                lParam = lParam
            };
            var func = new AsyncSendMessageCaller(SendMessageP);
            return Task.Factory.FromAsync<SendMessageParam, uint>(
                func.BeginInvoke,
                func.EndInvoke,
                param,
                null);
        }
        private struct SendMessageParam
        {
            public IntPtr window;
            public int msg;
            public IntPtr wParam;
            public IntPtr lParam;
        }
        private static uint SendMessageP(SendMessageParam param)
        {
            return SendMessage(param.window, param.msg, param.wParam, param.lParam);
        }
        private delegate uint AsyncSendMessageCaller(SendMessageParam param);


        /// <summary>
        /// キーをフラッシュして指定時間待ちます。
        /// </summary>
        /// <param name="dueTime"></param>
        /// <returns></returns>
        private static async Task FlushDelay(int dueTime)
        {
            await TaskEx.Run(System.Windows.Forms.SendKeys.Flush);
            await TaskEx.Delay(dueTime);
        }



        #endregion
        #region 内部関数

        /// <summary>
        /// このSendKeyInputクラスが生成したキー情報であることを示すマーク。
        /// ki.dwExtraInfoに設定されます。
        /// 現在のアプリケーションハンドルを指定します。
        /// </summary>
        public static readonly IntPtr ExtraInfoMark = InitMark();
        private static IntPtr InitMark()
        {
            Module[] ms = Assembly.GetEntryAssembly().GetModules();
            IntPtr hInstance = Marshal.GetHINSTANCE(ms[0]);
            return hInstance;
        }


        /// <summary>
        /// キーコードよりInput構造体を生成します。
        /// </summary>
        /// <param name="vk"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private static Input InputKeyboard(VK vk, KEYEVENTF flags)
        {
            Input rc = new Input();
            rc.type = INPUT_KEYBOARD;
            rc.ki.wVk = vk;
            rc.ki.wScan = scanCodes[(byte)vk];
            //rc.ki.wScan = (ushort)MapVirtualKey(vk, 0);
            rc.ki.dwFlags = flags;
            rc.ki.dwExtraInfo = ExtraInfoMark;
            rc.ki.time = 0;
            return rc;
        }
        private static readonly ushort[] scanCodes = InitScanCodes();
        private static ushort[] InitScanCodes()
        {
            ushort[] rc = new ushort[256];
            for (int i = 0; i < rc.Length; i++) rc[i] = (ushort)MapVirtualKey(i, 0);
            return rc;
        }

        /// <summary>
        /// ASCII文字→キーマップ変換。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static KeyValuePair<VK, bool> GetVCode(char c)
        {
            switch (c) {
                case ' ': return new KeyValuePair<VK, bool>(VK.SPACE, false);
                case '!': return new KeyValuePair<VK, bool>(VK._1, true);
                case '\"': return new KeyValuePair<VK, bool>(VK._2, true);
                case '#': return new KeyValuePair<VK, bool>(VK._3, true);
                case '$': return new KeyValuePair<VK, bool>(VK._4, true);
                case '%': return new KeyValuePair<VK, bool>(VK._5, true);
                case '&': return new KeyValuePair<VK, bool>(VK._6, true);
                case '\'': return new KeyValuePair<VK, bool>(VK._7, true);
                case '(': return new KeyValuePair<VK, bool>(VK._8, true);
                case ')': return new KeyValuePair<VK, bool>(VK._9, true);
                case '*': return new KeyValuePair<VK, bool>(VK.MULTIPLY, false);
                case '+': return new KeyValuePair<VK, bool>(VK.ADD, false);
                case ',': return new KeyValuePair<VK, bool>(VK.OEM_COMMA, false);
                case '-': return new KeyValuePair<VK, bool>(VK.SUBTRACT, false);
                case '.': return new KeyValuePair<VK, bool>(VK.DECIMAL, false);
                case '/': return new KeyValuePair<VK, bool>(VK.DIVIDE, false);
                case '0': return new KeyValuePair<VK, bool>(VK._0, false);
                case '1': return new KeyValuePair<VK, bool>(VK._1, false);
                case '2': return new KeyValuePair<VK, bool>(VK._2, false);
                case '3': return new KeyValuePair<VK, bool>(VK._3, false);
                case '4': return new KeyValuePair<VK, bool>(VK._4, false);
                case '5': return new KeyValuePair<VK, bool>(VK._5, false);
                case '6': return new KeyValuePair<VK, bool>(VK._6, false);
                case '7': return new KeyValuePair<VK, bool>(VK._7, false);
                case '8': return new KeyValuePair<VK, bool>(VK._8, false);
                case '9': return new KeyValuePair<VK, bool>(VK._9, false);
                case ':': return new KeyValuePair<VK, bool>(VK.OEM_1, false);
                case ';': return new KeyValuePair<VK, bool>(VK.OEM_PLUS, true);
                case '<': return new KeyValuePair<VK, bool>(VK.OEM_COMMA, true);
                case '=': return new KeyValuePair<VK, bool>(VK.OEM_MINUS, true);
                case '>': return new KeyValuePair<VK, bool>(VK.OEM_PERIOD, true);
                case '?': return new KeyValuePair<VK, bool>(VK.OEM_2, true);
                case '@': return new KeyValuePair<VK, bool>(VK.OEM_3, false);
                case 'A': return new KeyValuePair<VK, bool>(VK.A, true);
                case 'B': return new KeyValuePair<VK, bool>(VK.B, true);
                case 'C': return new KeyValuePair<VK, bool>(VK.C, true);
                case 'D': return new KeyValuePair<VK, bool>(VK.D, true);
                case 'E': return new KeyValuePair<VK, bool>(VK.E, true);
                case 'F': return new KeyValuePair<VK, bool>(VK.F, true);
                case 'G': return new KeyValuePair<VK, bool>(VK.G, true);
                case 'H': return new KeyValuePair<VK, bool>(VK.H, true);
                case 'I': return new KeyValuePair<VK, bool>(VK.I, true);
                case 'J': return new KeyValuePair<VK, bool>(VK.J, true);
                case 'K': return new KeyValuePair<VK, bool>(VK.K, true);
                case 'L': return new KeyValuePair<VK, bool>(VK.L, true);
                case 'M': return new KeyValuePair<VK, bool>(VK.M, true);
                case 'N': return new KeyValuePair<VK, bool>(VK.N, true);
                case 'O': return new KeyValuePair<VK, bool>(VK.O, true);
                case 'P': return new KeyValuePair<VK, bool>(VK.P, true);
                case 'Q': return new KeyValuePair<VK, bool>(VK.Q, true);
                case 'R': return new KeyValuePair<VK, bool>(VK.R, true);
                case 'S': return new KeyValuePair<VK, bool>(VK.S, true);
                case 'T': return new KeyValuePair<VK, bool>(VK.T, true);
                case 'U': return new KeyValuePair<VK, bool>(VK.U, true);
                case 'V': return new KeyValuePair<VK, bool>(VK.V, true);
                case 'W': return new KeyValuePair<VK, bool>(VK.W, true);
                case 'X': return new KeyValuePair<VK, bool>(VK.X, true);
                case 'Y': return new KeyValuePair<VK, bool>(VK.Y, true);
                case 'Z': return new KeyValuePair<VK, bool>(VK.Z, true);
                case '[': return new KeyValuePair<VK, bool>(VK.OEM_4, false);
                case '\\': return new KeyValuePair<VK, bool>(VK.OEM_5, false);
                case ']': return new KeyValuePair<VK, bool>(VK.OEM_6, false);
                case '^': return new KeyValuePair<VK, bool>(VK.OEM_7, false);
                case '_': return new KeyValuePair<VK, bool>(VK.OEM_102, true);
                case '`': return new KeyValuePair<VK, bool>(VK._7, true);
                case 'a': return new KeyValuePair<VK, bool>(VK.A, false);
                case 'b': return new KeyValuePair<VK, bool>(VK.B, false);
                case 'c': return new KeyValuePair<VK, bool>(VK.C, false);
                case 'd': return new KeyValuePair<VK, bool>(VK.D, false);
                case 'e': return new KeyValuePair<VK, bool>(VK.E, false);
                case 'f': return new KeyValuePair<VK, bool>(VK.F, false);
                case 'g': return new KeyValuePair<VK, bool>(VK.G, false);
                case 'h': return new KeyValuePair<VK, bool>(VK.H, false);
                case 'i': return new KeyValuePair<VK, bool>(VK.I, false);
                case 'j': return new KeyValuePair<VK, bool>(VK.J, false);
                case 'k': return new KeyValuePair<VK, bool>(VK.K, false);
                case 'l': return new KeyValuePair<VK, bool>(VK.L, false);
                case 'm': return new KeyValuePair<VK, bool>(VK.M, false);
                case 'n': return new KeyValuePair<VK, bool>(VK.N, false);
                case 'o': return new KeyValuePair<VK, bool>(VK.O, false);
                case 'p': return new KeyValuePair<VK, bool>(VK.P, false);
                case 'q': return new KeyValuePair<VK, bool>(VK.Q, false);
                case 'r': return new KeyValuePair<VK, bool>(VK.R, false);
                case 's': return new KeyValuePair<VK, bool>(VK.S, false);
                case 't': return new KeyValuePair<VK, bool>(VK.T, false);
                case 'u': return new KeyValuePair<VK, bool>(VK.U, false);
                case 'v': return new KeyValuePair<VK, bool>(VK.V, false);
                case 'w': return new KeyValuePair<VK, bool>(VK.W, false);
                case 'x': return new KeyValuePair<VK, bool>(VK.X, false);
                case 'y': return new KeyValuePair<VK, bool>(VK.Y, false);
                case 'z': return new KeyValuePair<VK, bool>(VK.Z, false);
                case '~': return new KeyValuePair<VK, bool>(VK.OEM_3, true);
                case '{': return new KeyValuePair<VK, bool>(VK.OEM_4, true);
                case '|': return new KeyValuePair<VK, bool>(VK.OEM_5, true);
                case '}': return new KeyValuePair<VK, bool>(VK.OEM_6, true);
            }
            return new KeyValuePair<VK, bool>(0, false);
        }

        #endregion
        #region インターフェース関数
        /// <summary>
        /// 指定された文字列のキーイベントを発行します。
        /// 特殊キー入力モードがあります。
        /// </summary>
        /// <remarks>
        /// 特殊キーモード
        ///  *x →[x:F1～F12(1～0,-,^)] FUNCキーを押して離す
        ///       1～0 F1～F10
        ///       -    F11
        ///       ^    F12
        ///  [x →[x:C,A,X] 特殊キーを押す
        ///       C CTRL
        ///       A ALT
        ///       X CTRL+ALT
        ///  ]  →特殊キーを離す
        ///  +  →[Shift+0]キーを押す
        ///  |  →[漢字]キーを押す
        ///  ~  →[Enter]キーを押す
        ///  @  →SendMessageを利用して前面窓に[Enter]キーを押す
        ///  _  →キー入力キューが無くなるまで待機し、その後約50ms待機する
        ///  :  →次のキーのとき、押して離す間に50ms待機する
        /// 特殊キーモードで利用しているキーを押したいとき
        /// * + 押したいキーでエスケープ
        /// </remarks>
        /// <param name="command">マクロテキスト</param>
        /// <returns>再生時間</returns>
        public static async Task<TimeSpan> SendKeysAsync(string command)
        {
            var startData = DateTime.Now;
            Debug.WriteLine("*---------------[SendKeys]: " + command);

            var inputs = new Input[1];

            var shift = false;
            var ctrl = false;
            var alt = false;
            var l_shift = false;
            var l_ctrl = false;
            var l_alt = false;
            var winHandle = new Lazy<IntPtr>(GetForegroundWindow);

            var ex = 0;
            var keyPushWaitMS = 0;
            foreach (char c in command) {
                VK vkey;
                switch (ex) {
                    case 0: // 特殊モードに入るかどうか判定
                        switch (c) {
                            case '*': ex = 1; continue;
                            case '[': ex = 2; continue;
                            case ']': ctrl = false; alt = false; continue;
                            case '|': vkey = VK.KANJI; goto SHIFT_DOWN;
                            case '\\': vkey = VK.ESCAPE; goto SHIFT_DOWN;
                            case '~': vkey = VK.RETURN; goto SHIFT_DOWN;
                            case ':': keyPushWaitMS += 50; continue;
                            case '_':
                                await FlushDelay(50);
                                continue;
                            case '@':
                                vkey = VK.CMD_EX_ENTER_KEY;
                                goto SHIFT_DOWN;
                            case '+':
                                vkey = VK._0;
                                shift = true;
                                goto EXT_KEY_CHANGE;
                        }
                        break;

                    case 1: // FUNCキーモード
                        ex = 0;
                        switch (c) {
                            case '1': vkey = VK.F1; goto SHIFT_DOWN;
                            case '2': vkey = VK.F2; goto SHIFT_DOWN;
                            case '3': vkey = VK.F3; goto SHIFT_DOWN;
                            case '4': vkey = VK.F4; goto SHIFT_DOWN;
                            case '5': vkey = VK.F5; goto SHIFT_DOWN;
                            case '6': vkey = VK.F6; goto SHIFT_DOWN;
                            case '7': vkey = VK.F7; goto SHIFT_DOWN;
                            case '8': vkey = VK.F8; goto SHIFT_DOWN;
                            case '9': vkey = VK.F9; goto SHIFT_DOWN;
                            case '0': vkey = VK.F10; goto SHIFT_DOWN;
                            case '-': vkey = VK.F11; goto SHIFT_DOWN;
                            case '^': vkey = VK.F12; goto SHIFT_DOWN;
                        }
                        break;

                    case 2: // 特殊キーモード
                        ex = 0;
                        switch (c) {
                            case 'C': ctrl = true; continue;
                            case 'A': alt = true; continue;
                            case 'X': ctrl = true; alt = true; continue;
                        }
                        break;
                }

                // キーペア取得
                KeyValuePair<VK, bool> pair = GetVCode(c);
                shift = pair.Value;
                vkey = pair.Key;
                goto EXT_KEY_CHANGE;

            SHIFT_DOWN:
                shift = false;

            EXT_KEY_CHANGE:
                // 特殊キーの状態変更
                if (l_ctrl != ctrl) {
                    await SendInputAsync(inputs, VK.CONTROL, ctrl ? KEYEVENTF.KEYDOWN : KEYEVENTF.KEYUP);
                    // CTRLキーは反応が鈍いのでWait入れておく
                    await FlushDelay(50);
                }
                if (l_shift != shift) await SendInputAsync(inputs, VK.SHIFT, shift ? KEYEVENTF.KEYDOWN : KEYEVENTF.KEYUP);
                if (l_alt != alt) await SendInputAsync(inputs, VK.MENU, alt ? KEYEVENTF.KEYDOWN : KEYEVENTF.KEYUP);
                l_shift = shift;
                l_ctrl = ctrl;
                l_alt = alt;

                // 特定窓にエンターを送信
                if (vkey == VK.CMD_EX_ENTER_KEY) {
                    await TaskEx.Run(System.Windows.Forms.SendKeys.Flush);
                    await SendEnterToWindowAsync(winHandle.Value, keyPushWaitMS);
                    keyPushWaitMS = 0;
                    continue;
                }

                // キーを押して離す
                await SendInputAsync(inputs, vkey, KEYEVENTF.KEYDOWN);
                if (keyPushWaitMS != 0) {
                    await FlushDelay(keyPushWaitMS);
                    keyPushWaitMS = 0;
                }
                await SendInputAsync(inputs, vkey, KEYEVENTF.KEYUP);
            }
            // 終了処理：特殊キーが押されたままなら最後に離す
            if (l_ctrl || l_alt) await TaskEx.Run(System.Windows.Forms.SendKeys.Flush);
            if (l_ctrl) await SendInputAsync(inputs, VK.CONTROL, KEYEVENTF.KEYUP);
            if (l_shift) await SendInputAsync(inputs, VK.SHIFT, KEYEVENTF.KEYUP);
            if (l_alt) await SendInputAsync(inputs, VK.MENU, KEYEVENTF.KEYUP);

            return DateTime.Now - startData;
        }



        /// <summary>
        /// keybd_eventを発行します。
        /// </summary>
        /// <param name="bVk"></param>
        /// <param name="dwFlags"></param>
        public static async Task SendKeyEventAsync(byte bVk, int dwFlags)
        {
            await TaskEx.Run(() => keybd_event(bVk, 0, dwFlags, ExtraInfoMark));
        }



        /// <summary>
        /// 指定したウィンドウにENTER入力を送信します。
        /// 指定ms待機してキーを離します。
        /// </summary>
        /// <param name="window"></param>
        /// <param name="waitMS"></param>
        public static async Task SendEnterToWindowAsync(IntPtr window, int waitMS)
        {
            IntPtr wParam = new IntPtr((int)VK.RETURN);
            IntPtr lParam = IntPtr.Zero;
            await SendMessageAsync(window, (int)WM.KEYDOWN, wParam, lParam);
            await TaskEx.Delay(waitMS);

            lParam = new IntPtr(
              (1 << 0) |
              (1 << 30) |
              (1 << 31)
              );
            await SendMessageAsync(window, (int)WM.KEYUP, wParam, lParam);
        }



        /// <summary>
        /// Unicode文字をキー入力します。最後にEnterを送信します。
        /// </summary>
        /// <param name="text"></param>
        public static async Task<int> SendUnicodeAsync(string text)
        {
            Input[] input = new Input[text.Length + 2];
            // UNICODE
            for (int i = 0; i < text.Length; i++) {
                input[i].type = INPUT_KEYBOARD;
                input[i].ki.wVk = 0;
                input[i].ki.wScan = (ushort)text[i];
                input[i].ki.dwFlags = KEYEVENTF.UNICODE;
                input[i].ki.dwExtraInfo = ExtraInfoMark;
                input[i].ki.time = 0;
            }
            // ENTER ON OFF
            int p = input.Length - 2;
            input[p].type = INPUT_KEYBOARD;
            input[p].ki.wVk = VK.RETURN;
            input[p].ki.wScan = 0;
            input[p].ki.dwFlags = KEYEVENTF.KEYDOWN;
            input[p].ki.dwExtraInfo = ExtraInfoMark;
            input[p].ki.time = 0;

            p++;
            input[p].type = INPUT_KEYBOARD;
            input[p].ki.wVk = VK.RETURN;
            input[p].ki.wScan = 0;
            input[p].ki.dwFlags = KEYEVENTF.KEYUP;
            input[p].ki.dwExtraInfo = ExtraInfoMark;
            input[p].ki.time = 0;

            return await SendInputAsync(input.Length, input, Marshal.SizeOf(typeof(Input)));
        }


        #endregion
    }
}
