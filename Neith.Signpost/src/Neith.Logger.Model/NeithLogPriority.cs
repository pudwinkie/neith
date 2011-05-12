using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Neith.Logger.Model
{
    [ProtoContract]
    public enum NeithLogPriority
    {
        /// <summary>致命的なエラーまたはアプリケーションのクラッシュ。</summary>
        Critical,

        /// <summary>回復可能なエラー。</summary>
        Error,

        /// <summary>重大でない問題。</summary>
        Warning,

        /// <summary>情報メッセージ。</summary>
        Information,

        /// <summary>トレースのデバッグ。</summary>
        Verbose,

        /// <summary>論理的な操作の開始。</summary>
        Start,

        /// <summary>論理的な操作の停止。</summary>
        Stop,

        /// <summary>論理的な操作の中断。</summary>
        Suspend,

        /// <summary>論理的な操作の再開。</summary>
        Resume,

        /// <summary>相関 ID の変更。</summary>
        Transfer,
    }
}
