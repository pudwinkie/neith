Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass()>
Public Class StockComparerSingleProviderWpfColdPath
    Dim result As SnapshotExtrema

    <TestInitialize()>
    Public Sub Setup()
        ' set up a mock IStockSnapshotProvider that yields
        ' with the symbol ZZZZ and +20%
        '
        Dim mockProvider = New Mock(Of IStockSnapshotProvider)()
        mockProvider.Setup(Function(provider) provider.GetLatestSnapshotAsync()).
            Returns(Async Function()
                        Await TaskEx.Yield()
                        Return New StockSnapshot With {.Symbol = "ZZZZ", .Percent = +20.0}
                    End Function)

        result = WpfContext.Run(Function()
                                    Return StockComparer.ComparePercentsAsync({mockProvider.Object})
                                End Function).Result

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