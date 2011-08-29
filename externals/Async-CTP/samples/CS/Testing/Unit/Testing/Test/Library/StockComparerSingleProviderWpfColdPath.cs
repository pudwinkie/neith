using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class StockComparerSingleProviderWpfColdPath
{
    SnapshotExtrema result;

    [TestInitialize]
    public void Setup()
    {
        // set up a mock IStockSnapshotProvider that yields
        // with the symbol ZZZZ and +20%
        //
        var mockProvider = new Mock<IStockSnapshotProvider>();

        // This two-line mock method impl will guarantee to induce the "cold"
        // path on callers that await the method, provided they are on a thread-affine context (e.g. WPF or WinForms).
        // 
        // How it works is that Task.Yield (or TaskEx in the CTP) always yield the below lambda's
        // continuation to the sync context ( SynchronizationContext.Post(...) ).
        //
        // In a thread-affine sync context the yielded continuation is guaranteed to be processed later,
        // because the current thread is busy firing off the async lambda. This delayed processing thus
        // guaranteeds that callers awaiting this lambda will also be forced to post their continuation
        // rather than continue executing synchronously.
        //
        // However, under default threadpool contexts, Yield() will cause the continuation to
        // post to any thread in the threadpool. There's a real race then to see if that small
        // continuation completes before the caller's await is evaluated.
        //
        // Thus, this is only guaranteed to induce the "cold" path in thread-affine contexts.
        mockProvider.Setup(provider => provider.GetLatestSnapshotAsync())
                    .Returns(async () =>
                     {
                         await TaskEx.Yield();
                         return new StockSnapshot { Symbol = "ZZZZ", Percent = +20.0 };
                     });

        // run the method we're testing in a thread-affine context
        result = WpfContext.Run(() => StockComparer.ComparePercentsAsync(new[] { mockProvider.Object })).Result;
    }

    [TestMethod]
    public void MinDeltaSymbol()
    {
        Assert.AreEqual(result.MinDelta.Symbol, "ZZZZ");
    }

    [TestMethod]
    public void MaxDeltaSymbol()
    {
        Assert.AreEqual(result.MaxDelta.Symbol, "ZZZZ");
    }

    [TestMethod]
    public void MinDeltaPercent()
    {
        Assert.AreEqual(result.MinDelta.Percent, 20.0);
    }

    [TestMethod]
    public void MaxDeltaPercent()
    {
        Assert.AreEqual(result.MaxDelta.Percent, 20.0);
    }
}
