using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Neith.Signpost.Logger.XIV
{
    public static partial class XIVAnalysis
    {
        private const string reTAG = @"\{02([0-9A-F][0-9A-F])+}";
        private const string reTAG2 = reTAG + reTAG;
        private const string reTAGItem = "「" + reTAG2 + @"(?<item>.+)" + reTAG2 + "」";

        private static string reNAME(string key)
        {
            return "(?<" + key + @">\w+ \w+)";
        }

        #region スキルの使用・結果
        /// <summary>詠唱開始</summary>
        private static readonly ConvertModule mMagicStart = new ConvertModule(reNAME("sender") + @"は「(?<skill>.+)」の詠唱を始めた。", (src, m) =>
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
        });

        /// <summary>回復</summary>
        private static readonly ConvertModule mMagicAided = new ConvertModule(
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
                    new XElement(XN.b, new XAttribute(XN.class_, "target"), target),
                    "」はＨＰを",
                    B("value", value),
                    "回復した。",
                    META("attribute", "HP")
                    );
            });

        /// <summary>効果なし</summary>
        private static readonly ConvertModule mSkillNothing = new ConvertModule(
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
            });



        /// <summary>効果が切れた</summary>
        private static readonly ConvertModule mEffectOff = new ConvertModule(@"(?<target>.+)の「(?<attribute>.+)」の効果が切れた。", (src, m) =>
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
        });

        /// <summary>効果がかかった</summary>
        private static readonly ConvertModule mEffectOn = new ConvertModule(@"(?<target>.+)に「(?<attribute>.+)」の効果がかかった。", (src, m) =>
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
        });


        #endregion
        /// <summary>飲みほした</summary>
        private static readonly ConvertModule mDrink = new ConvertModule(reNAME("sender") + "は" + reTAGItem + @"を飲みほした。", (src, m) =>
        {
            var sender = m.Groups["sender"].Value;
            var item = m.Groups["item"].Value;
            return SPAN(
                ACT(src.idAct + "use "),
                B("sender", sender),
                "は「",
                I("item", item),
                "」を飲みほした。"
                );
        });

        /// <summary>レベルアップ</summary>
        private static readonly ConvertModule mLevelUp = new ConvertModule(reNAME("sender") + @"は「(?<attribute>.+)」のレベルが(?<value>\d+)に上がった！", (src, m) =>
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
        });



        /// <summary>制作：完成</summary>
        private static readonly ConvertModule mCraftComplete = new ConvertModule(reNAME("sender") + "は" + reTAGItem + @"×(?<value>\d+)を完成させた！", (src, m) =>
        {
            var sender = m.Groups["sender"].Value;
            var item = m.Groups["item"].Value;
            var value = m.Groups["value"].Value;
            return SPAN(
                ACT(src.idAct + "complete "),
                B("sender", sender),
                "は「",
                I("item", item),
                "」×",
                B("value", value),
                "を完成させた！"
                );
        });

        /// <summary>制作：失敗</summary>
        private static readonly ConvertModule mCraftMiss = new ConvertModule(reNAME("sender") + "は製作に失敗した……", (src, m) =>
        {
            var sender = m.Groups["sender"].Value;
            return SPAN(
                ACT(src.idAct + "miss "),
                B("sender", sender),
                "は製作に失敗した……"
                );
        });


        /// <summary>システムインフォメーション</summary>
        private static readonly ConvertModule mSystemInfo = new ConvertModule(@"(?<message>.+)", (src, m) =>
        {
            var message = m.Groups["message"].Value;
            return SPAN(
                ACT(src.idAct),
                B("sender", "system"),
                B("message", message)
                );
        });

        /// <summary>会話</summary>
        private static readonly ConvertModule mTalk = new ConvertModule(@"(?<message>.+)", (src, m) =>
        {
            var message = m.Groups["message"].Value;
            var sender = src.who;
            return SPAN(
                ACT(src.idAct),
                B("sender", sender),
                B("message", message)
                );
        });



        private readonly static Dictionary<int, ConvertModule[]> AnalysisModulesDic = InitAnalysisModules();
        private readonly static Dictionary<int, string> AnalysisIdDic = InitAnalysisIdDic();


        private static Dictionary<int, ConvertModule[]> InitAnalysisModules()
        {
            var items = new[]{
                new{id =   1, module = mTalk },             // 会話：say
                new{id =   2, module = mTalk },             // 会話：シャウト

                new{id =   5, module = mTalk },             // 会話：LS：LS1
                new{id =   6, module = mTalk },             // 会話：LS：LS2
                new{id =   7, module = mTalk },             // 会話：LS：LS3
                new{id =   8, module = mTalk },             // 会話：LS：LS4
                new{id =   9, module = mTalk },             // 会話：LS：LS5
                new{id =  10, module = mTalk },             // 会話：LS：LS6
                new{id =  11, module = mTalk },             // 会話：LS：LS7
                new{id =  12, module = mTalk },             // 会話：LS：LS8

                new{id =  14, module = mTalk },             // 会話：LS：LS1：カレント
                new{id =  15, module = mTalk },             // 会話：LS：LS2：カレント
                new{id =  16, module = mTalk },             // 会話：LS：LS3：カレント
                new{id =  17, module = mTalk },             // 会話：LS：LS4：カレント
                new{id =  18, module = mTalk },             // 会話：LS：LS5：カレント
                new{id =  19, module = mTalk },             // 会話：LS：LS6：カレント
                new{id =  20, module = mTalk },             // 会話：LS：LS7：カレント
                new{id =  21, module = mTalk },             // 会話：LS：LS8：カレント

                new{id =  29, module = mSystemInfo },       // システム：インフォメーション

                new{id =  32, module = mDrink },            // 自分への通知：飲みほした

                new{id =  67, module = mLevelUp },          // 他人：レベル：上がった

                new{id =  71, module = mCraftComplete },    // 生産：完成
                new{id =  71, module = mCraftMiss },        // 生産：失敗

                new{id =  91, module = mSkillNothing },     // 他人：失敗：効果なし

                new{id =  97, module = mMagicStart },       // 他人：回復：詠唱開始
                new{id =  97, module = mMagicAided },       // 他人：回復：回復

                new{id = 103, module = mEffectOn },         // 他人：強化：かかった

                new{id = 109, module = mEffectOff },        // 他人：弱体：切れた
            };

            var q1 = from a in items
                     group a.module by a.id;
            return q1.ToDictionary(a => a.Key, a => a.ToArray());
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
                new{id =  32, action = " my notify " },             // 自分への通知

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
