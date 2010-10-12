using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neith.Crawler.Sites.Zam.AbilityList;

namespace Neith.Crawler.Test.Zam.AbilityList
{
    using NUnit.Framework;
    //[TestFixture]
    public class Tests
    {
        //[Test]
        public void CrawlerTest()
        {
            CrawlerTask.Run();
        }

        [Test]
        public void ParseListTest()
        {
            Properties.Resources.Zam_AbilityList_Test
                .AreEqual("漢字読み込み試験");


            

            //CrawlerTask.ParseList();
        }

    }
}
