Imports System.Threading.Tasks

Public Structure SnapshotExtrema
    Public MinDelta As StockSnapshot
    Public MaxDelta As StockSnapshot
End Structure

Public Structure StockSnapshot
    Public Symbol As String
    Public Percent As Double
End Structure

Public Interface IStockSnapshotProvider
    Function GetLatestSnapshotAsync() As Task(Of StockSnapshot)
End Interface