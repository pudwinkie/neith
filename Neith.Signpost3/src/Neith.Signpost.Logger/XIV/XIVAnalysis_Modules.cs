using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Neith.Signpost.Logger.XIV.Converters;

namespace Neith.Signpost.Logger.XIV
{
    public static partial class XIVAnalysis
    {
        /// <summary>解析フィルター一覧</summary>
        public static IEnumerable<IConvertModule> AnalysisModules { get; private set; }

        /// <summary>解析フィルターの参照辞書</summary>
        public static Dictionary<int, IConvertModule[]> AnalysisModulesDic { get; private set; }

        /// <summary>ID属性の参照辞書</summary>
        public static Dictionary<int, string> AnalysisIdDic { get; private set; }

        private static void InitConverters(CompositionContainer container)
        {
            AnalysisIdDic = InitAnalysisIdDic();

            var items = container
                .GetExports<IConvertModule, IConvertMetadata>()
                .ToArray();

            var q1 = items
                .SelectMany(a =>
                {
                    return a.Metadata.Id
                        .Select(id => new { id, priority = a.Metadata.Priority, module = a.Value });
                });
            var q2 = from a in q1
                     group a by a.id;

            AnalysisModulesDic = q2.ToDictionary(
                a => a.Key,
                a => a
                    .OrderByDescending(b => b.priority)
                    .Select(b => b.module)
                    .ToArray());
            AnalysisModules = items.Select(a => a.Value).ToArray();
        }

        private static Dictionary<int, string> InitAnalysisIdDic()
        {
            var items = new[]{
                new{id =   1, action = " talk say " },              // 会話：say
                new{id =   2, action = " talk shout " },            // 会話：シャウト

                new{id =   5, action = " talk ls ls1 " },           // 会話：LS：LS1
                new{id =   6, action = " talk ls ls2 " },           // 会話：LS：LS2
                new{id =   7, action = " talk ls ls3 " },           // 会話：LS：LS3
                new{id =   8, action = " talk ls ls4 " },           // 会話：LS：LS4
                new{id =   9, action = " talk ls ls5 " },           // 会話：LS：LS5
                new{id =  10, action = " talk ls ls6 " },           // 会話：LS：LS6
                new{id =  11, action = " talk ls ls7 " },           // 会話：LS：LS7
                new{id =  12, action = " talk ls ls8 " },           // 会話：LS：LS8

                new{id =  14, action = " talk ls ls1 current " },   // 会話：LS：LS1：カレント
                new{id =  15, action = " talk ls ls2 current " },   // 会話：LS：LS2：カレント
                new{id =  16, action = " talk ls ls3 current " },   // 会話：LS：LS3：カレント
                new{id =  17, action = " talk ls ls4 current " },   // 会話：LS：LS4：カレント
                new{id =  18, action = " talk ls ls5 current " },   // 会話：LS：LS5：カレント
                new{id =  19, action = " talk ls ls6 current " },   // 会話：LS：LS6：カレント
                new{id =  20, action = " talk ls ls7 current " },   // 会話：LS：LS7：カレント
                new{id =  21, action = " talk ls ls8 current " },   // 会話：LS：LS8：カレント


                new{id =  29, action = " system info " },           // システム：インフォメーション
                new{id =  32, action = " my notify " },             // 通知

                new{id =  67, action = " other level " },           // 他人：レベル
                new{id =  71, action = " other claft " },           // 他人：生産

                new{id =  91, action = " other mistake " },         // 他人：失敗
                new{id =  97, action = " other aid " },             // 他人：回復
                new{id = 103, action = " other effect enchant " },  // 他人：強化
                new{id = 109, action = " other effect weak " },     // 他人：弱体
            };

            return items.ToDictionary(a => a.id, a => a.action);
        }


    }
}
