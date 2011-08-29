Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass()>
Public Class StockComparerSingleProviderHotPath
    Dim result As SnapshotExtrema

    <TestInitialize()>
    Public Sub Setup()
        ' set up a mock IStockSnapshotProvider that returns a completed task
        ' with the symbol ZZZZ and +20%
        '
        ' Completed tasks simulate as if somehow the getting of the result did
        ' not have to occur asynchronously - for example, if it was acceptable
        ' to use a cached result
        Dim mockProvider = New Mock(Of IStockSnapshotProvider)()
        mockProvider.Setup(Function(provider) provider.GetLatestSnapshotAsync()).
            Returns(TaskEx.FromResult(New StockSnapshot With {.Symbol = "ZZZZ", .Percent = +20.0}))

        ' Now actually perform the test action
        Dim asyncTask = StockComparer.ComparePercentsAsync({mockProvider.Object})
        asyncTask.Wait()
        result = asyncTask.Result
    End Sub

    <TestMethod()>
    Public Sub MinDeltaSymbol()
        Assert.AreEqual(result.MinDelta.Symbol, "ZZZZ")
    End Sub

    <TestMethod()>
    Public Sub MaxDeltaSymbol()
        Assert.AreEqual(result.MaxDelta.Symbol, "ZZZZ")
    End Sub

    <TestMethod()>
    Public Sub MinDeltaPercent()
        Assert.AreEqual(result.MinDelta.Percent, 20.0)
    End Sub

    <TestMethod()>
    Public Sub MaxDeltaPercent()
        Assert.AreEqual(result.MaxDelta.Percent, 20.0)
    End Sub

End Class