Imports System.Threading.Tasks

<TestClass()>
Public Class StockComparerDualProviderHotPath
    Dim result As SnapshotExtrema

    <TestInitialize()>
    Public Sub Setup()
        ' set up two mock IStockSnapshotProviders that return completed tasks
        '
        ' Completed tasks simulate as if somehow the getting of the result did
        ' not have to occur asynchronously - for example, if it was acceptable
        ' to use a cached result
        Dim firstMock = New Mock(Of IStockSnapshotProvider)()
        firstMock.Setup(Function(provider) provider.GetLatestSnapshotAsync()).Returns(TaskEx.FromResult(New StockSnapshot With {.Symbol = "AAAA",
                                                                                                                                .Percent = +20.0}))

        Dim secondMock = New Mock(Of IStockSnapshotProvider)()
        secondMock.Setup(Function(provider) provider.GetLatestSnapshotAsync()).Returns(TaskEx.FromResult(New StockSnapshot With {.Symbol = "BBBB",
                                                                                                                                 .Percent = -10.0}))

        ' Now actually perform the test action
        Dim asyncTask = StockComparer.ComparePercentsAsync({firstMock.Object, secondMock.Object})

        asyncTask.Wait()

        result = asyncTask.Result

    End Sub

    <TestMethod()>
    Public Sub MinDeltaSymbol()
        Assert.AreEqual(result.MinDelta.Symbol, "BBBB")
    End Sub

    <TestMethod()>
    Public Sub MinDeltaPercent()
        Assert.AreEqual(result.MinDelta.Percent, -10.0)
    End Sub

    <TestMethod()>
    Public Sub MaxDeltaSymbol()
        Assert.AreEqual(result.MaxDelta.Symbol, "AAAA")
    End Sub

    <TestMethod()>
    Public Sub MaxDeltaPercent()
        Assert.AreEqual(result.MaxDelta.Percent, 20.0)
    End Sub

End Class