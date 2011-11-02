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
                    ACT(src.idAct()),
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
                    ACT(src.idAct()),
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
                    ACT(src.idAct("level up")),
                    B("sender", sender),
                    "は「",
                    B("attribute", attribute),
                    "」のレベルが",
                    B("value", value),
                    "に上がった！"
                    );
            }) { }
    }



    /// <summary>クラスチェンジ</summary>
    [Export(typeof(IConvertModule))]
    [ConverterMetadata(9990, 32)]
    [DisplayName("クラスチェンジ"), Category("会話・その他")]
    public sealed class ClassChange : BaseConvertModule
    {
        public ClassChange()
            : base(reNAME("sender") + @"はクラスを「(?<attribute>.+)」にチェンジした。", (src, m) =>
            {
                var sender = m.Groups["sender"].Value;
                var attribute = m.Groups["attribute"].Value;
                var value = m.Groups["value"].Value;
                return SPAN(
                    ACT(src.idAct("change class")),
                    B("sender", sender),
                    "はクラスを「",
                    B("attribute", attribute),
                    "」にチェンジした。"
                    );
            }) { }
    }



    /// <summary>装備した</summary>
    [Export(typeof(IConvertModule))]
    [ConverterMetadata(9990, 32)]
    [DisplayName("装備した"), Category("会話・その他")]
    public sealed class EquipSet : BaseConvertModule
    {
        public EquipSet()
            : base(@"(?<attribute>.+)に" + reTAGItem2("item") + "を装備した。", (src, m) =>
            {
                var attribute = m.Groups["attribute"].Value;
                var item = m.Groups["item"].Value;
                return SPAN(
                    ACT(src.idAct("equip set")),
                    HIDDEN("sender", "my"),
                    B("attribute", attribute),
                    "に「",
                    B("item", item),
                    "」を装備した。"
                    );
            }) { }
    }



    /// <summary>装備セット</summary>
    [Export(typeof(IConvertModule))]
    [ConverterMetadata(9990, 32)]
    [DisplayName("装備セット"), Category("会話・その他")]
    public sealed class EquipRemove : BaseConvertModule
    {
        public EquipRemove()
            : base(@"(?<attribute>.+)の" + reTAGItem2("item") + "を外した。", (src, m) =>
            {
                var attribute = m.Groups["attribute"].Value;
                var item = m.Groups["item"].Value;
                return SPAN(
                    ACT(src.idAct("equip remove")),
                    HIDDEN("sender", "my"),
                    B("attribute", attribute),
                    "の「",
                    B("item", item),
                    "」を外した。"
                    );
            }) { }
    }




}