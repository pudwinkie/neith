using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Neith.Crawler.Sites.Zam.AbilityList;

namespace Neith.Crawler.Test.Zam.AbilityList
{
    using NUnit.Framework;
    [TestFixture]
    public class Tests
    {
        //[Test]
        public void CrawlerTest()
        {
            CrawlerTask.Run();
        }

        private static XElement ToXHtmlElement(string text)
        {
            var reader = new StringReader(text);
            return reader.ToXElementHtml();
        }

        [Test]
        public void ParseTest()
        {
            Properties.Resources.Zam_AbilityList_Test
                .AreEqual("漢字読み込み試験");

            var doc = ToXHtmlElement(Properties.Resources.Zam_AbilityList_List);

            doc
                .GetNextUrl()
                .AreEqual(@"http://ffxiv.zam.com/ja/abilitylist.html?1&#38;page=2");

            doc
                .ParseList()
                .FirstOrDefault()
                .AreEqual(@"http://ffxiv.zam.com/ja/ability.html?ffxivability=28930");

        }

        [Test]
        public void ItemTest()
        {
            var doc = ToXHtmlElement(Properties.Resources.Zam_AbilityList_Item28930);

        }


    }
}
