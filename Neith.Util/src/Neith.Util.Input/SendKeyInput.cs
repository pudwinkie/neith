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
        #region APIの非同期呼び出し


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
            return API.SendMessage(param.window, param.msg, param.wParam, param.lParam);
        }
        private delegate uint AsyncSendMessageCaller(SendMessageParam param);


        #endregion
        #region 内部関数



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
            var startTime = DateTime.Now;
            Debug.WriteLine("*---------------[SendKeys]: " + command);
            var input = new InputBuilder();
            var shift = false;
            var ctrl = false;
            var alt = false;
            var l_shift = false;
            var l_ctrl = false;
            var l_alt = false;
            var winHandle = new Lazy<IntPtr>(API.GetForegroundWindow);

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
                                await input.Flush(50);
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
                var pair = GetVCode(c);
                shift = pair.Value;
                vkey = pair.Key;
                goto EXT_KEY_CHANGE;

            SHIFT_DOWN:
                shift = false;

            EXT_KEY_CHANGE:
                // 特殊キーの状態変更
                if (l_ctrl != ctrl) {
                    input.Add(VK.CONTROL, ctrl ? KEYEVENTF.KEYDOWN : KEYEVENTF.KEYUP);
                    // CTRLキーは反応が鈍いのでWait入れておく
                    await input.Flush(50);
                }
                if (l_shift != shift) input.Add(VK.SHIFT, shift ? KEYEVENTF.KEYDOWN : KEYEVENTF.KEYUP);
                if (l_alt != alt) input.Add(VK.MENU, alt ? KEYEVENTF.KEYDOWN : KEYEVENTF.KEYUP);
                l_shift = shift;
                l_ctrl = ctrl;
                l_alt = alt;

                // 特定窓にエンターを送信
                if (vkey == VK.CMD_EX_ENTER_KEY) {
                    await input.Flush();
                    await SendEnterToWindowAsync(winHandle.Value, keyPushWaitMS);
                    keyPushWaitMS = 0;
                    continue;
                }

                // キーを押して離す
                input.Add(vkey, KEYEVENTF.KEYDOWN);
                if (keyPushWaitMS != 0) {
                    await input.Flush(keyPushWaitMS);
                    keyPushWaitMS = 0;
                }
                input.Add(vkey, KEYEVENTF.KEYUP);
            }
            // 終了処理：特殊キーが押されたままなら最後に離す
            if (l_ctrl || l_alt) await TaskEx.Run(System.Windows.Forms.SendKeys.Flush);
            if (l_ctrl) input.Add(VK.CONTROL, KEYEVENTF.KEYUP);
            if (l_shift) input.Add(VK.SHIFT, KEYEVENTF.KEYUP);
            if (l_alt) input.Add(VK.MENU, KEYEVENTF.KEYUP);

            // 終了
            await input.Flush();
            var endTime = DateTime.Now - startTime;
            Debug.WriteLine(string.Format("*---------------[SendKeys]: Time={0}sec, send={1}", endTime.TotalSeconds, input.SendCount));
            return endTime;
        }



        /// <summary>
        /// API.keybd_eventを発行します。
        /// </summary>
        /// <param name="bVk"></param>
        /// <param name="dwFlags"></param>
        public static async Task SendKeyEventAsync(byte bVk, int dwFlags)
        {
            await TaskEx.Run(() => API.keybd_event(bVk, 0, dwFlags, API.GetMessageExtraInfo()));
        }



        /// <summary>
        /// 指定したウィンドウにENTER入力を送信します。
        /// 指定ms待機してキーを離します。
        /// </summary>
        /// <param name="window"></param>
        /// <param name="waitMS"></param>
        public static async Task SendEnterToWindowAsync(IntPtr window, int waitMS)
        {
            var wParam = new IntPtr((int)VK.RETURN);
            var lParam = IntPtr.Zero;
            await SendMessageAsync(window, (int)API.WM.KEYDOWN, wParam, lParam);
            await TaskEx.Delay(waitMS);

            lParam = new IntPtr(
              (1 << 0) |
              (1 << 30) |
              (1 << 31)
              );
            await SendMessageAsync(window, (int)API.WM.KEYUP, wParam, lParam);
        }



        /// <summary>
        /// Unicode文字をキー入力します。最後にEnterを送信します。
        /// </summary>
        /// <param name="text"></param>
        public static int SendUnicode(string text)
        {
            var input = new API.Input[text.Length + 2];
            // UNICODE
            for (int i = 0; i < text.Length; i++) {
                input[i].type = API.INPUT_KEYBOARD;
                input[i].ki.wVk = 0;
                input[i].ki.wScan = (ushort)text[i];
                input[i].ki.dwFlags = KEYEVENTF.UNICODE;
                input[i].ki.dwExtraInfo = API.GetMessageExtraInfo();
                input[i].ki.time = 0;
            }
            // ENTER ON OFF
            int p = input.Length - 2;
            input[p].type = API.INPUT_KEYBOARD;
            input[p].ki.wVk = VK.RETURN;
            input[p].ki.wScan = 0;
            input[p].ki.dwFlags = KEYEVENTF.KEYDOWN;
            input[p].ki.dwExtraInfo = API.GetMessageExtraInfo();
            input[p].ki.time = 0;

            p++;
            input[p].type = API.INPUT_KEYBOARD;
            input[p].ki.wVk = VK.RETURN;
            input[p].ki.wScan = 0;
            input[p].ki.dwFlags = KEYEVENTF.KEYUP;
            input[p].ki.dwExtraInfo = API.GetMessageExtraInfo();
            input[p].ki.time = 0;

            return API.SendInput(input.Length, input, Marshal.SizeOf(typeof(API.Input)));
        }


        #endregion
    }
}
