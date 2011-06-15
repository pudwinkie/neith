using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Signpost
{
    /// <summary>タイマーの動作状態</summary>
    public enum CountDownTimerStatus
    {
        /// <summary>初期化処理。ただちにリセット状態に戻る</summary>
        Init,

        /// <summary>リセット</summary>
        Reset,

        /// <summary>動作中</summary>
        Run,

        /// <summary>中断中</summary>
        Pause,

        /// <summary>終了</summary>
        Fin,
    }

    /// <summary>残り時間ステータス</summary>
    public enum CountDownTimerRemainStatus
    {
        /// <summary>無し</summary>
        Zero,

        /// <summary>～2400秒</summary>
        Hurry,

        /// <summary>～100分</summary>
        Normal,

        /// <summary>～24時間</summary>
        Hours,

        /// <summary>１日以上</summary>
        Days,


    }

}
