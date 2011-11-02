using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace Neith.Signpost.Logger.XIV.Converters
{
    /// <summary>アイテム：使った</summary>
    [Export(typeof(IConvertModule))]
    [ConverterMetadata(9990, 32)]
    [DisplayName("アイテム：使った"), Category("アイテム")]
    public sealed class Use : BaseConvertModule
    {
        public Use()
            : base(reNAME("sender") + "は" + reTAGItem2("item") + @"を使った。", (src, m) =>
            {
                var sender = m.Groups["sender"].Value;
                var item = m.Groups["item"].Value;
                return SPAN(
                    ACT(src.idAct("use item ")),
                    B("sender", sender),
                    "は「",
                    I("item", item),
                    "」を使った。"
                    );
            }) { }
    }



    /// <summary>アイテム：飲みほした</summary>
    [Export(typeof(IConvertModule))]
    [ConverterMetadata(9990, 32)]
    [DisplayName("アイテム：飲みほした"), Category("アイテム")]
    public sealed class Drink : BaseConvertModule
    {
        public Drink()
            : base(reNAME("sender") + "は" + reTAGItem2("item") + @"を飲みほした。", (src, m) =>
            {
                var sender = m.Groups["sender"].Value;
                var item = m.Groups["item"].Value;
                return SPAN(
                    ACT(src.idAct("use item drink ")),
                    B("sender", sender),
                    "は「",
                    I("item", item),
                    "」を飲みほした。"
                    );
            }) { }
    }



}