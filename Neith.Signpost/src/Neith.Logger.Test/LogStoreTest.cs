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
            try {
                var item = Log.Create();
                item.Host = "Host";
                item.Application = "ろぐしゅつりょくテストなのよー";
                store.Store(item);
            }
            finally {
                LogStore.StoreClose();
            }
        }

        [Test]
        public void ReadWriteTest1()
        {
            var store = LogStore.Instance;
            try {
                var item = Log.Create();
                item.Host = "ほ～すと";
                item.Application = "出力しちゃったのですね！";
                var pos = store.Store(item);
                store.Flush();
                using (var loader = new LogLoader()) {
                    var item2 = loader.Load(pos);
                    if (item != item2) Assert.Fail();
                }
            }
            finally {
                LogStore.StoreClose();
            }

        }

    }
}
