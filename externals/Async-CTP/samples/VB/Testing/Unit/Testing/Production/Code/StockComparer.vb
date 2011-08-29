Imports System.Linq
Imports System.Threading.Tasks

Public Module StockComparer

    Public Async Function ComparePercentsAsync(stockProviders As IStockSnapshotProvider()) As Task(Of SnapshotExtrema)

        If stockProviders.Length < 1 Then
            Throw New ArgumentException("Must provide at least one provider.", "stockProviders")
        End If

        ' initialize our extrema so that we don't have to special-case the first element
        Dim extrema As SnapshotExtrema = New SnapshotExtrema With {
                                            .MinDelta = New StockSnapshot With {.Percent = Double.PositiveInfinity},
                                            .MaxDelta = New StockSnapshot With {.Percent = Double.NegativeInfinity}
                                         }

        ' LINQ to objects is an easy way to synchronously call GetLatestSnapshotAsync() on all of them and aggregate the tasks
        Dim providerTasks = (From provider In stockProviders Select provider.GetLatestSnapshotAsync()).ToList()

        For Each providerTask In providerTasks

            ' await the next provider task
            Await providerTask

            Dim snapshot = providerTask.Result

            If extrema.MaxDelta.Percent < snapshot.Percent Then
                extrema.MaxDelta = snapshot
            End If

            If snapshot.Percent < extrema.MinDelta.Percent Then
                extrema.MinDelta = snapshot
            End If
        Next

        Return extrema
    End Function

End Module