using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace Neith.Signpost.Logger.XIV.Converters
{
    /// <summary>制作：完成させた！</summary>
    [Export(typeof(IConvertModule))]
    [ConverterMetadata(9990, 71)]
    [DisplayName("制作：完成させた！"), Category("制作")]
    public sealed class CraftCreate : BaseConvertModule
    {
        public CraftCreate()
            : base(reNAME("sender") + "は" + reTAGItem2("item") + @"×(?<value>\d+)を完成させた！", (src, m) =>
            {
                var sender = m.Groups["sender"].Value;
                var item = m.Groups["item"].Value;
                var value = m.Groups["value"].Value;
                return SPAN(
                    ACT(src.idAct + "create item "),
                    B("sender", sender),
                    "は「",
                    I("item", item),
                    "」×",
                    B("value", value),
                    "を完成させた！"
                    );
            }) { }
    }



    /// <summary>制作：製作に失敗した</summary>
    [Export(typeof(IConvertModule))]
    [ConverterMetadata(9990, 71)]
    [DisplayName("制作：製作に失敗した"), Category("制作")]
    public sealed class CraftMiss : BaseConvertModule
    {
        public CraftMiss()
            : base(reNAME("sender") + "は製作に失敗した……", (src, m) =>
            {
                var sender = m.Groups["sender"].Value;
                return SPAN(
                    ACT(src.idAct + "miss "),
                    B("sender", sender),
                    "は製作に失敗した……"
                    );
            }) { }
    }



    /// <summary>制作：修理した</summary>
    [Export(typeof(IConvertModule))]
    [ConverterMetadata(9990, 71)]
    [DisplayName("制作：修理"), Category("制作")]
    public sealed class CraftRepair : BaseConvertModule
    {
        public CraftRepair()
            : base(reNAME("sender") + "は" + reTAGItem2("item") + @"を修理した。", (src, m) =>
            {
                var sender = m.Groups["sender"].Value;
                var item = m.Groups["item"].Value;
                return SPAN(
                    ACT(src.idAct + "repair item "),
                    B("sender", sender),
                    "は「",
                    I("item", item),
                    "」を修理した。"
                    );
            }) { }
    }



    /// <summary>制作：マテリア生成</summary>
    [Export(typeof(IConvertModule))]
    [ConverterMetadata(9990, 71)]
    [DisplayName("制作：マテリア生成"), Category("制作")]
    public sealed class MateriaCreate : BaseConvertModule
    {
        public MateriaCreate()
            : base(reNAME("sender") + "の" + reTAGItem("target") + "から" + reTAGMateria("item") + @"が生成された！", (src, m) =>
            {
                var sender = m.Groups["sender"].Value;
                var target = m.Groups["target"].Value;
                var item = m.Groups["item"].Value;
                return SPAN(
                    ACT(src.idAct + "materia create "),
                    B("sender", sender),
                    "の",
                    I("target", target),
                    "から",
                    I("item", item),
                    "が生成された！"
                    );
            }) { }
    }



}