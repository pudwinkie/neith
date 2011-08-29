using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class StockComparerSingleProviderHotPath
{
    SnapshotExtrema result;

    [TestInitialize]
    public void Setup()
    {
        // set up a mock IStockSnapshotProvider that returns a completed task
        // with the symbol ZZZZ and +20%
        //
        // Completed tasks simulate as if somehow the getting of the result did
        // not have to occur asynchronously - for example, if it was acceptable
        // to use a cached result
        var mockProvider = new Mock<IStockSnapshotProvider>();

        mockProvider.Setup(provider => provider.GetLatestSnapshotAsync())
                    .Returns(TaskEx.FromResult(new StockSnapshot { Symbol = "ZZZZ", Percent = +20.0 }));

        // Now actually perform the test action
        var asyncTask = StockComparer.ComparePercentsAsync(new[] { mockProvider.Object });

        // Wait for the method to finish to verify the results
        asyncTask.Wait();

        // store the result for our assertion methods
        result = asyncTask.Result;
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
