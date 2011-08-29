using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class StockComparerDualProviderHotPath
{
    SnapshotExtrema result;

    [TestInitialize]
    public void Setup()
    {
        // set up two mock IStockSnapshotProviders that return completed tasks
        //
        // Completed tasks simulate as if somehow the getting of the result did
        // not have to occur asynchronously - for example, if it was acceptable
        // to use a cached result
        var firstMock = new Mock<IStockSnapshotProvider>();

        firstMock.Setup(provider => provider.GetLatestSnapshotAsync())
                 .Returns(TaskEx.FromResult(new StockSnapshot { Symbol = "AAAA", Percent = +20.0 }));

        var secondMock = new Mock<IStockSnapshotProvider>();

        secondMock.Setup(provider => provider.GetLatestSnapshotAsync())
                  .Returns(TaskEx.FromResult(new StockSnapshot { Symbol = "BBBB", Percent = -10.0 }));

        // Now actually perform the test action
        var asyncTask = StockComparer.ComparePercentsAsync(new[] { firstMock.Object, secondMock.Object });

        // Wait for the method to finish to verify the results
        asyncTask.Wait();

        // store the result for our assertion methods
        result = asyncTask.Result;
    }

    [TestMethod]
    public void MinDeltaSymbol()
    {
        Assert.AreEqual(result.MinDelta.Symbol, "BBBB");
    }

    [TestMethod]
    public void MinDeltaPercent()
    {
        Assert.AreEqual(result.MinDelta.Percent, -10.0);
    }

    [TestMethod]
    public void MaxDeltaSymbol()
    {
        Assert.AreEqual(result.MaxDelta.Symbol, "AAAA");
    }

    [TestMethod]
    public void MaxDeltaPercent()
    {
        Assert.AreEqual(result.MaxDelta.Percent, 20.0);
    }
}
