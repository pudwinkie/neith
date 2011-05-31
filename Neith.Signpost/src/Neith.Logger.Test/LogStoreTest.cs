using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Neith.Logger;
using Neith.Logger.Model;

namespace Neith.Logger.Test
{
    using NUnit.Framework;

    [TestFixture]
    public class LogStoreTest
    {
        [Test]
        public void WriteTest1()
        {
            var store = LogStore.Instance;
            var item = NeithLog.Create();
            item.Host = "Host";
            item.Application = "ろぐしゅつりょくテストなのよー";
            store.Store(item);
        }

        [Test]
        public void ReadWriteTest1()
        {
            var rand = new Random();
            var store = LogStore.Instance;
            var item = NeithLog.Create();
            item.Host = "ほ～すと";
            item.Actor = "そこの誰か";
            item.Application = "出力しちゃったのですね！" + rand.Next();
            store.Store(item);
            var item2 = store.Dic[item.UtcTime];
            if (item.Application != item2.Application) Assert.Fail();

            var item3 = store.IndexActor
                .WhereKey(item.Actor, store)
                .Where(a => a.UtcTime == item.UtcTime)
                .First();

            if (item.Application != item3.Application) Assert.Fail();

            Debug.WriteLine("IndexActor.Count = " + store.IndexActor.Index.Count);
        }

    }
}
