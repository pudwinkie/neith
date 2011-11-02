using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace Neith.Signpost.Logger.XIV.Converters
{
    /// <summary>スキル：詠唱を始めた</summary>
    [Export(typeof(IConvertModule))]
    [ConverterMetadata(9990, 97)]
    [DisplayName("スキル：詠唱を始めた"), Category("スキル")]
    public sealed class MagicStart : BaseConvertModule
    {
        public MagicStart()
            : base(reNAME("sender") + @"は「(?<skill>.+)」の詠唱を始めた。", (src, m) =>
            {
                var sender = m.Groups["sender"].Value;
                var skill = m.Groups["skill"].Value;
                return SPAN(
                    ACT(src.idAct + "magic start "),
                    B("sender", sender),
                    "は「",
                    B("skill", skill),
                    "」の詠唱を始めた。"
                    );
            }) { }
    }



    /// <summary>スキル：効果がなかった</summary>
    [Export(typeof(IConvertModule))]
    [ConverterMetadata(9990, 91)]
    [DisplayName("スキル：効果がなかった"), Category("スキル")]
    public sealed class SkillNothing : BaseConvertModule
    {
        public SkillNothing()
            : base(
            reNAME("sender") + "は" +
            reNAME("target") + @"に「(?<skill>.+)」　⇒　しかし、効果がなかった。", (src, m) =>
            {
                var sender = m.Groups["sender"].Value;
                var target = m.Groups["target"].Value;
                var skill = m.Groups["skill"].Value;
                return SPAN(
                    ACT(src.idAct + "magic nothing "),
                    B("sender", sender),
                    "は",
                    B("target", target),
                    "に「",
                    B("skill", skill),
                    "」　⇒　しかし、効果がなかった。"
                    );
            }) { }
    }



    /// <summary>スキル：攻撃を外してしまった</summary>
    [Export(typeof(IConvertModule))]
    [ConverterMetadata(9990, 91)]
    [DisplayName("スキル：攻撃を外してしまった"), Category("スキル")]
    public sealed class SkillNotHit : BaseConvertModule
    {
        public SkillNotHit()
            : base(
            reNAME("sender") + "は" +
            reNAME("target") + @"に「(?<skill>.+)」　⇒　攻撃を外してしまった。", (src, m) =>
            {
                var sender = m.Groups["sender"].Value;
                var target = m.Groups["target"].Value;
                var skill = m.Groups["skill"].Value;
                return SPAN(
                    ACT(src.idAct + "magic nothit "),
                    B("sender", sender),
                    "は",
                    B("target", target),
                    "に「",
                    B("skill", skill),
                    "」　⇒　攻撃を外してしまった。"
                    );
            }) { }
    }



    /// <summary>スキル：ＨＰを回復した</summary>
    [Export(typeof(IConvertModule))]
    [ConverterMetadata(9990, 97)]
    [DisplayName("スキル：ＨＰを回復した"), Category("スキル")]
    public sealed class MagicAided : BaseConvertModule
    {
        public MagicAided()
            : base(
            reNAME("sender") + "は" +
            reNAME("target") + @"に「(?<skill>.+)」　⇒　\k<target>はＨＰを(?<value>\d+)回復した。", (src, m) =>
            {
                var sender = m.Groups["sender"].Value;
                var target = m.Groups["target"].Value;
                var skill = m.Groups["skill"].Value;
                var value = m.Groups["value"].Value;
                return SPAN(
                    ACT(src.idAct + "magic aided "),
                    B("sender", sender),
                    "は",
                    B("target", target),
                    "に「",
                    B("skill", skill),
                    "」　⇒　",
                    XB("target", target),
                    "」はＨＰを",
                    B("value", value),
                    "回復した。",
                    META("attribute", "HP")
                    );
            }) { }
    }



    /// <summary>スキル：効果がかかった</summary>
    [Export(typeof(IConvertModule))]
    [ConverterMetadata(9990, 103, 109)]
    [DisplayName("スキル：効果がかかった"), Category("スキル")]
    public sealed class EffectOn : BaseConvertModule
    {
        public EffectOn()
            : base(@"(?<target>.+)に「(?<attribute>.+)」の効果がかかった。", (src, m) =>
            {
                var target = m.Groups["target"].Value;
                var attribute = m.Groups["attribute"].Value;
                return SPAN(
                    ACT(src.idAct + "on "),
                    B("target", target),
                    "の「",
                    B("attribute", attribute),
                    "」の効果がかかった。"
                    );
            }) { }
    }



    /// <summary>スキル：効果がかかった</summary>
    [Export(typeof(IConvertModule))]
    [ConverterMetadata(9990, 103, 109)]
    [DisplayName("スキル：効果が切れた"), Category("スキル")]
    public sealed class EffectOff : BaseConvertModule
    {
        public EffectOff()
            : base(@"(?<target>.+)の「(?<attribute>.+)」の効果が切れた。", (src, m) =>
        {
            var target = m.Groups["target"].Value;
            var attribute = m.Groups["attribute"].Value;
            return SPAN(
                ACT(src.idAct + "off "),
                B("target", target),
                "の「",
                B("attribute", attribute),
                "」の効果が切れた。"
                );
            }) { }
    }



}