using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var store = LogStore.Instance;
            var item = NeithLog.Create();
            item.Host = "ほ～すと";
            item.Application = "出力しちゃったのですね！";
            store.Store(item);
            var item2 = store.Dic[item.UtcTime];
            if (item.Application != item2.Application) Assert.Fail();
        }

    }
}
