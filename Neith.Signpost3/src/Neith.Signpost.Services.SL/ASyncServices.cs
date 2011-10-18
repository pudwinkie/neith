using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Threading.Tasks;

namespace Neith.Signpost.Services
{
    public static class ASyncServices
    {
        public static Task<DateTimeOffset> GetServerTimeAsync(this ISignpostContext THIS)
        {
            return Task.Factory.FromAsync<DateTimeOffset>(
                THIS.BeginGetServerTime,
                THIS.EndGetServerTime,
                null);
        }

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
        /// <param name="THIS">チャンネル</param>
        /// <param name="command">マクロテキスト</param>
        /// <returns>再生時間</returns>
        public static Task<TimeSpan> SendKeysAsync(this ISignpostContext THIS, string command)
        {
            return Task.Factory.FromAsync<string, TimeSpan>(
                THIS.BeginSendKeys,
                THIS.EndSendKeys,
                command,
                null);
        }

    }
}
