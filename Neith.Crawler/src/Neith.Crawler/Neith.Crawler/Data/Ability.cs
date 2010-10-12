using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel.Syndication;

namespace Neith.Crawler.Data
{
    [DataContract(Name = "Ability", Namespace = Const.NS14)]
    public class Ability : SyndicationItem
    {
        [DataMember(Name = "ActionCost")]
        public int ActionCost { get; set; }

        [DataMember(Name = "Recast")]
        public TimeSpan Recast { get; set; }

        [DataMember(Name = "MP")]
        public int MP { get; set; }

        [DataMember(Name = "TP")]
        public int TP { get; set; }

    }
}
