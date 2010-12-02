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
                var item = new Log();
                item.Timestamp = DateTimeOffset.Now;
                item.Host = "Host";
                item.Pid = 123;
                item.Application = "ろぐしゅつりょくテストなのよー";
                store.Store(item);
            }
            finally {
                LogStore.StoreClose();
            }
        }

    }
}
