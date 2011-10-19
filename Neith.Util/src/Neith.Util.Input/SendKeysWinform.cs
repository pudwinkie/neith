using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;

namespace Neith.Util.Input
{
    public static class SendKeysWinform
    {
        /// <summary>キーストロークをアクティブなアプリケーションに送信します。</summary>
        /// <param name="keys">送信するキーストロークの文字列。</param>
        /// <exception cref="T:System.InvalidOperationException">キーストロークの送信先となるアクティブなアプリケーションはありません。</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="keys" /> が有効なキーストロークを表していません。</exception>
        public static void Send(string keys)
        {
            SendKeys.Send(keys);
        }

        /// <summary>特定のキーをアクティブなアプリケーションに送信し、メッセージが処理されるまで待機します。</summary>
        /// <param name="keys">送信するキーストロークの文字列。</param>
        public static async Task<TimeSpan> SendWaitAsync(string keys)
        {
            var start = DateTime.Now;
            await TaskEx.Run(() => SendKeys.SendWait(keys));
            return DateTime.Now - start;
        }

        /// <summary>メッセージ キューに現在ある Windows メッセージをすべて処理します。</summary>
        public static async Task FlushAsync()
        {
            await TaskEx.Run(() => SendKeys.Flush());
        }


        /// <summary>
        /// {wait 999}を解釈するSendWaitAsync。
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static async Task<TimeSpan> SendWaitExAsync(string keys)
        {
            var start = DateTime.Now;
            foreach (var item in Parse(keys)) {
                var text = item.Item1;
                var wait = item.Item2;
                Debug.WriteLine(string.Format("[SendKeysWinform::SendWaitExAsync] text=[{0}] ,wait={1}", text, wait));
                if (!string.IsNullOrEmpty(text)) await TaskEx.Run(() => SendKeys.SendWait(text));
                if (wait > 0) await TaskEx.Delay(wait);
            }
            return DateTime.Now - start;
        }

        private static IEnumerable<Tuple<string, int>> Parse(string keys)
        {
            var pos = 0;
            while (true) {
                var m = reWait.Match(keys, pos);
                if (!m.Success) {
                    var last = keys.Substring(pos);
                    yield return new Tuple<string, int>(last, 0);
                    yield break;
                }
                var text = keys.Substring(pos, m.Index - pos);
                var wait = int.Parse(m.Groups["wait"].Value);
                yield return new Tuple<string, int>(text, wait);
                pos = m.Index + m.Length;
            }
        }
        private static Regex reWait = new Regex(@"{wait\s+(?<wait>\d+)}", RegexOptions.Compiled | RegexOptions.IgnoreCase);


    }
}
