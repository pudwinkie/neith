using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Neith.Logger.Model
{
    [Description("通知優先度")]
    public enum NeithLogPriority
    {
        /// <summary>ゆっくり</summary>
        [Description("ゆっくり")]
        VeryLow = -2,

        /// <summary>控えめ</summary>
        [Description("控えめ")]
        Moderate = -1,

        /// <summary>通常</summary>
        [Description("通常")]
        Normal = 0,

        /// <summary>優先</summary>
        [Description("優先")]
        High = 1,

        /// <summary>緊急</summary>
        [Description("緊急")]
        Emergency = 2
    }
}
