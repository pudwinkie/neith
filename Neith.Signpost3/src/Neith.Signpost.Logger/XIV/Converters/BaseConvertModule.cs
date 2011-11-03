using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Neith.Util.Reflection;

namespace Neith.Signpost.Logger.XIV.Converters
{
    public abstract class BaseConvertModule : IConvertModule
    {
        public Regex Regex { get; private set; }
        private Func<SrcItem, Match, XElement> CalcFunc { get; set; }
        public int CallCount { get; private set; }
        public SrcItem LastItem { get; private set; }

        protected BaseConvertModule(string text, Func<SrcItem, Match, XElement> func)
        {
            Regex = new Regex("^" + text + "$", RegexOptions.Compiled);
            CalcFunc = func;
        }

        /// <summary>
        /// 変換処理。
        /// </summary>
        /// <param name="src"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public XElement Calc(SrcItem src, Match m)
        {
            LastItem = src;
            CallCount++;
            return CalcFunc(src, m);
        }

        public override string ToString()
        {
            var className = this.GetType().Name;
            return string.Format("{0} (call={1})", className, CallCount);
        }


        #region スタティック関数群
        protected const string reTAG = @"\{02([0-9A-F][0-9A-F])+}";
        protected const string reTAG2 = reTAG + reTAG;
        protected const string reATTACK = @"(?<sender>.+)は((?<target>.+)に)?((?<direction>.+)から)?「(?<skill>.+)」(?<tword>を実行した。)?";

        /// <summary>
        /// {tag}{tag}アイテム{tag}{tag}
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected static string reTAGItem(string key)
        {
            return reTAG2 + @"(?<" + key + @">.+)" + reTAG2;
        }

        /// <summary>
        /// 「{tag}{tag}アイテム{tag}{tag}」
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected static string reTAGItem2(string key)
        {
            return "「" + reTAGItem(key) + "」";
        }

        /// <summary>
        /// {tag}{tag}マテリア{tag}{tag}
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected static string reTAGMateria(string key)
        {
            return reTAG2 + @"(?<" + key + @">.+のマテリ.)" + reTAG2;
        }

        /// <summary>
        /// プレイヤー名
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected static string reNAME(string key)
        {
            return "(?<" + key + @">.+)";
        }

        #region 簡略タグ作成コマンド群
        protected static XAttribute SCOPE { get { return XIVExtensons.SCOPE; } }
        protected static XElement B(string name, object value) { return XIVExtensons.B(name, value); }
        protected static XElement I(string name, object value) { return XIVExtensons.I(name, value); }
        protected static XElement LI(string name, object value) { return XIVExtensons.LI(name, value); }
        protected static XElement META(string name, object value) { return XIVExtensons.META(name, value); }

        protected static XElement SPAN(params object[] values) { return XIVExtensons.SPAN(values); }
        protected static XAttribute PROP(object name) { return XIVExtensons.PROP(name); }
        protected static XElement TIME(string name, DateTimeOffset time) { return XIVExtensons.TIME(name, time); }
        protected static XElement ACT(object value) { return XIVExtensons.B("action", value); }

        protected static XElement XB(string name, object value)
        {
            return new XElement(XN.b, new XAttribute(XN.class_, name), value);
        }
        protected static XElement HIDDEN(string name, object value)
        {
            var el = B(name, value);
            el.Add(new XAttribute(XN.class_, "hidden"));
            return el;
        }

        protected static XElement ATTACK(Match m, object act, params object[] args)
        {
            var sender = m.Groups["sender"].Value;
            var target = m.Groups["target"].Value;
            var direction = m.Groups["direction"].Value;
            var skill = m.Groups["skill"].Value;
            var tword = m.Groups["tword"].Value;

            var items = new List<object>();
            items.Add(act);
            items.Add(B("sender", sender));
            items.Add("は");

            if (!string.IsNullOrWhiteSpace(target)) {
                items.Add(B("target", target));
                items.Add("に");
            }
            if (!string.IsNullOrWhiteSpace(direction)) {
                items.Add(B("direction", direction));
                items.Add("から");
            }
            items.Add("「");
            items.Add(B("skill", skill));
            items.Add("」");
            if (!string.IsNullOrWhiteSpace(tword)) {
                items.Add(tword);
            }

            items.AddRange(args);

            return SPAN(items.ToArray());
        }


        #endregion

        #endregion

    }
}
