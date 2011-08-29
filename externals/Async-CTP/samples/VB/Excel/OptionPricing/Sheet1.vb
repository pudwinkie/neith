Option Strict On

Imports System.Threading
Imports System.Threading.Tasks

Public Class Sheet1

    Private rngUp, rngDown, rngInitial, rngExercise, rngInterest, rngPeriods, rngRuns, rngRemote As Excel.Range
    Private _cancellation As CancellationTokenSource
    Private Shared s_rand As New ThreadLocal(Of Random)(Function() New Random(Thread.CurrentThread.ManagedThreadId Xor Environment.TickCount))


    Private Sub Sheet1_Startup() Handles Me.Startup
        rngUp = Me.Range("B2")
        rngDown = Me.Range("B3")
        rngInterest = Me.Range("B4")
        rngInitial = Me.Range("B5")
        rngPeriods = Me.Range("B6")
        rngExercise = Me.Range("B7")
        rngRuns = Me.Range("B8")
        rngRemote = Me.Range("B9")
    End Sub


    Private Async Sub btnRun_Click(sender As System.Object, e As System.EventArgs) Handles btnRun.Click
        Application.StartAsyncMethod()

        ' Set up a cancellation source to use to cancel background work
        If _cancellation IsNot Nothing Then _cancellation.Cancel() : Return
        _cancellation = New CancellationTokenSource

        ' Get data from form
        Dim initial = CDbl(rngInitial.Value2), exercise = CDbl(rngInitial.Value2), interest = CDbl(rngInterest.Value2)
        Dim up = CDbl(rngUp.Value2), down = CDbl(rngDown.Value2)
        Dim periods = CInt(rngPeriods.Value2), runs = CInt(rngRuns.Value2)

        ' Run for a number of iterations
        Dim columns = {"D", "E", "F", "G", "H", "I", "J", "K", "L", "M"}
        Dim rows = {2, 3, 4, 5, 6, 7, 8, 9, 10, 11}

        Me.Range("C2").Value2 = "Calculating..."
        btnRun.Text = "Cancel"
        btnClear.Enabled = False

        Try
            Dim cells = From column In columns
                        From row In rows
                        Select column, row

            Dim results = From cell In cells.AsParallel.WithCancellation(_cancellation.Token).WithMergeOptions(ParallelMergeOptions.NotBuffered)
                          Let price = PriceAsianOptions(initial, exercise, up, down, interest, periods, runs)
                          Select price, cell.column, cell.row

            ' Initialize aggregation data
            Dim count = 0
            Dim sumPrice = 0.0, sumSquarePrice = 0.0, min = Double.MaxValue, max = Double.MinValue
            Dim stdDev = 0.0, stdErr = 0.0

            ' Run the query and process its results
            Dim enumerator = results.GetEnumerator()
            While Await TaskEx.Run(Function() enumerator.MoveNext())
                Dim result = enumerator.Current
                count += 1
                sumPrice += result.price
                Me.Range("D13").Value2 = sumPrice / count

                min = Math.Min(min, result.price)
                max = Math.Max(max, result.price)
                Me.Range("D14").Value2 = min
                Me.Range("D15").Value2 = max

                sumSquarePrice += result.price * result.price
                stdDev = Math.Sqrt(sumSquarePrice - sumPrice * sumPrice / count) / If(count = 1, 1, count - 1)
                stdErr = stdDev / Math.Sqrt(count)
                Me.Range("D16").Value2 = stdDev
                Me.Range("D17").Value2 = stdErr
                Me.Range(result.column & result.row).Value2 = result.price
            End While

        Catch ex As OperationCanceledException
        Finally
            ' Reset controls
            btnRun.Text = "Run"
            Me.Range("C2").ClearContents()
            btnClear.Enabled = True
            _cancellation = Nothing
        End Try
    End Sub

    Private Sub btnClear_Click(sender As System.Object, e As System.EventArgs) Handles btnClear.Click
        Me.Range("D2", "M11").ClearContents()
        Me.Range("D13", "D18").ClearContents()
    End Sub

    Private Shared Function PriceAsianOptions(initial As Double, exercise As Double, up As Double, down As Double, interest As Double, periods As Integer, runs As Integer) As Double
        Dim pricePath = New Double(periods + 1) {}
        Dim piup = (interest - down) / (up - down)
        Dim pidown = 1 - piup
        Dim temp = 0.0, priceAverage = 0.0, callPayOff = 0.0

        For run = 0 To runs - 1
            pricePath(0) = initial
            Dim sumPricePath = initial
            For i = 1 To periods
                Dim rn = s_rand.Value.NextDouble()
                pricePath(i) = pricePath(i - 1) * If(rn > pidown, up, down)
                sumPricePath += pricePath(i)
            Next
            priceAverage = sumPricePath / (periods + 1)
            callPayOff = Math.Max(priceAverage - exercise, 0)
            temp += callPayOff
        Next
        Return (temp / Math.Pow(interest, periods)) / runs
    End Function

End Class
