using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace Neith.Signpost.Logger.XIV.Converters
{
    /// <summary>会話</summary>
    [Export(typeof(IConvertModule))]
    [ConverterMetadata(9990,
        1, 2,
        5, 6, 7, 8, 9, 10, 11, 12,
        14, 15, 16, 17, 18, 19, 20, 21)]
    [DisplayName("会話"), Category("会話・その他")]
    public sealed class Talk : BaseConvertModule
    {
        public Talk()
            : base(@"(?<message>.+)", (src, m) =>
            {
                var message = m.Groups["message"].Value;
                var sender = src.who;
                return SPAN(
                    ACT(src.idAct),
                    B("sender", sender),
                    B("message", message)
                    );
            }) { }
    }



    /// <summary>システムインフォ</summary>
    [Export(typeof(IConvertModule))]
    [ConverterMetadata(9990, 29)]
    [DisplayName("システムインフォ"), Category("会話・その他")]
    public sealed class SystemInfo : BaseConvertModule
    {
        public SystemInfo()
            : base(@"(?<message>.+)", (src, m) =>
            {
                var message = m.Groups["message"].Value;
                return SPAN(
                    ACT(src.idAct),
                    B("sender", "system"),
                    B("message", message)
                    );
            }) { }
    }



    /// <summary>レベルアップ</summary>
    [Export(typeof(IConvertModule))]
    [ConverterMetadata(9990, 67)]
    [DisplayName("レベルアップ"), Category("会話・その他")]
    public sealed class LevelUp : BaseConvertModule
    {
        public LevelUp()
            : base(reNAME("sender") + @"は「(?<attribute>.+)」のレベルが(?<value>\d+)に上がった！", (src, m) =>
            {
                var sender = m.Groups["sender"].Value;
                var attribute = m.Groups["attribute"].Value;
                var value = m.Groups["value"].Value;
                return SPAN(
                    ACT(src.idAct + "up "),
                    B("sender", sender),
                    "は「",
                    B("attribute", attribute),
                    "」のレベルが",
                    B("value", value),
                    "に上がった！"
                    );
            }) { }
    }




}