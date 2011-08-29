using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Logger.Parsers
{
    /// <summary>
    /// パーサステージ１。暗号解釈を担当する。
    /// 要求に基づき、バイナリ情報より以下のいずれかのトークンを切り出す。
    /// ■１行要求
    /// 　・１行分の平文テキスト
    /// ■改行要求
    /// 　・改行
    /// ■テキストブロック要求
    /// 　・空改行で終わるテキストブロック
    /// 　・暗号化されたテキストブロック
    /// ■バイナリブロック要求
    /// 　・指定長のバイナリブロック
    /// 　・指定長で暗号化されたバイナリブロック
    /// 
    /// モードとして、平文モードと暗号モードがある。暗号モードの場合、復号して返す。
    /// </summary>
    public class Stage1
    {


    }
}
