Imports System.Runtime.CompilerServices
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Text

Public Class Form1

    Sub Form1_Shown() Handles Me.Shown
        MessageBox.Show("This application shows some 'pull' techniques for getting progress reports from a long-running task. Click the buttons, see how the app is still responsive, and look at the source code.", "Progress")
    End Sub

    '================================================================================================================================================================
    '================================================================================================================================================================
    '== Latest Progress
    '================================================================================================================================================================
    '================================================================================================================================================================

    Private Async Sub btnLatest_Click(sender As System.Object, e As System.EventArgs) Handles btnLatest.Click
        For Each button In panel1.Controls : button.Enabled = False : Next
        btnCancel.Enabled = True
        Dim cts As New CancellationTokenSource
        AddHandler btnCancel.Click, AddressOf cts.EventHandler

        Try
            Dim progress As New LatestProgress(Of ScanInfo)
            Dim task = ScanImageForLightAsync(pictureBox1.Image, cts.Token, progress)
            Dim scalex = CSng(pictureBox1.Width / pictureBox1.Image.Width)
            Dim scaley = CSng(pictureBox1.Height / pictureBox1.Image.Height)
            Using g = pictureBox1.CreateGraphics
                While Await progress.Progress(task, 50)
                    pictureBox1.Refresh()
                    g.FillRectangle(Brushes.Yellow, 0, progress.Latest.Row * scaley, pictureBox1.Width, 4 * scaley)
                    Await TaskEx.Run(Sub() Console.Beep(progress.Latest.RowCount * 30 + 200, 50))
                End While
            End Using
            Dim c = Await task
            MessageBox.Show("We found " & c & " light pixels", "Count")
        Catch ex As Exception
            pictureBox1.Invalidate()
        Finally
            RemoveHandler btnCancel.Click, AddressOf cts.EventHandler
            For Each button In panel1.Controls : button.Enabled = True : Next
            btnCancel.Enabled = False
        End Try
    End Sub

    Class LatestProgress(Of T)
        Implements IProgress(Of T)

        Dim _latest As T
        Dim tcs As New TaskCompletionSource(Of Object)

        Public ReadOnly Property Latest As T
            Get
                SyncLock Me : Return _latest : End SyncLock
            End Get
        End Property

        Public Async Function Progress(UnderlyingTask As Task, Optional MinimumDelay As Integer = 0) As Task(Of Boolean)
            Await TaskEx.WhenAny(UnderlyingTask, TaskEx.WhenAll(tcs.Task, TaskEx.Delay(MinimumDelay)))
            If UnderlyingTask.IsCompleted Then Await UnderlyingTask : Return False
            tcs = New TaskCompletionSource(Of Object) : Return True
        End Function

        Private Sub Report(value As T) Implements IProgress(Of T).Report
            SyncLock Me : _latest = value : End SyncLock
            tcs.TrySetResult(Nothing)
        End Sub
    End Class

    Structure ScanInfo
        Public Row As Integer
        Public RowCount As Integer
        Public TotalCount As Integer
    End Structure

    Private Async Function ScanImageForLightAsync(img0 As Image, cancel As CancellationToken, progress As IProgress(Of ScanInfo)) As Task(Of Integer)
        ' This routine counts how many light pixels there are in an image.
        ' So as to give useful feedback, it does it in a sweep from bottom to top,
        ' and it makes a tone to show how much light each row had.

        Dim bmp As New Bitmap(img0)
        Dim count = 0
        For y = bmp.Height - 1 To 0 Step -1
            cancel.ThrowIfCancellationRequested()

            Dim rowcount = 0
            For x = 0 To bmp.Width - 1
                Dim col = bmp.GetPixel(x, y)
                Dim lightness = 0.0 + col.R + col.G + col.B
                If lightness > 350 Then rowcount += 1
            Next
            count += rowcount
            If progress IsNot Nothing Then progress.Report(New ScanInfo With {.Row = y, .RowCount = rowcount, .TotalCount = count})

            ' Actually the code above runs to completion almost immediately. So let's have some artificial slowdown...
            Await TaskEx.Delay(20)
        Next
        Return count
    End Function



    '================================================================================================================================================================
    '================================================================================================================================================================
    '== Queued Progress
    '================================================================================================================================================================
    '================================================================================================================================================================



    Private Async Sub btnQueued_Click(sender As System.Object, e As System.EventArgs) Handles btnQueued.Click
        For Each button In panel1.Controls : button.Enabled = False : Next
        btnCancel.Enabled = True
        Dim cts As New CancellationTokenSource
        AddHandler btnCancel.Click, AddressOf cts.EventHandler
        pictureBox1.Invalidate()

        Try
            Dim progress As New QueuedProgress(Of Point)
            Dim task = MakeImageLightAsync(pictureBox1.Image, cts.Token, progress)
            Dim scalex = CSng(pictureBox1.Width / pictureBox1.Image.Width)
            Dim scaley = CSng(pictureBox1.Height / pictureBox1.Image.Height)
            Using g = pictureBox1.CreateGraphics
                While Await progress.NextProgress(task)
                    g.FillRectangle(Brushes.Yellow, progress.Current.X * scalex, progress.Current.Y * scaley, 4 * scalex, 4 * scaley)
                End While
            End Using
        Catch ex As Exception
            pictureBox1.Invalidate()
        Finally
            RemoveHandler btnCancel.Click, AddressOf cts.EventHandler
            For Each button In panel1.Controls : button.Enabled = True : Next
            btnCancel.Enabled = False
        End Try

    End Sub


    Class QueuedProgress(Of T)
        Implements IProgress(Of T)

        Dim pastFirstElement = False
        Dim reports As New System.Collections.Concurrent.ConcurrentQueue(Of T)
        Dim tcs As New TaskCompletionSource(Of Object)

        Public ReadOnly Property Current As T
            Get
                Dim value As T
                If Not reports.TryPeek(value) Then Throw New InvalidOperationException("Must await NextProgress")
                Return value
            End Get
        End Property

        Public Async Function NextProgress(UnderlyingTask As Task) As Task(Of Boolean)
            Dim value As T
            If pastFirstElement Then
                If Not reports.TryDequeue(value) Then Throw New Exception("Invalid state")
            End If
            pastFirstElement = True
            Await TaskEx.WhenAny(UnderlyingTask, tcs.Task)
            If UnderlyingTask.IsCompleted Then Await UnderlyingTask : Return False
            tcs = New TaskCompletionSource(Of Object)
            If Not reports.TryPeek(value) Then Throw New Exception("Invalid state")
            Return True
        End Function

        Private Sub Report(value As T) Implements IProgress(Of T).Report
            reports.Enqueue(value)
            tcs.TrySetResult(Nothing)
        End Sub
    End Class

    Async Function MakeImageLightAsync(img0 As Image, cancel As CancellationToken, progress As IProgress(Of Point)) As Task(Of Bitmap)
        ' 1. Get the raw bits that make up the bitmap. We're doing this low-level bitwise
        ' manipulation because we need the speed.
        Dim bmp As New Bitmap(img0.Width, img0.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb)
        Using g = Graphics.FromImage(bmp) : g.DrawImage(img0, 0, 0) : End Using
        Dim bounds As New Rectangle(0, 0, bmp.Width, bmp.Height)
        Dim bmpData = bmp.LockBits(bounds, System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat)
        Dim p = New Byte(bmpData.Stride * bmpData.Height) {}
        System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, p, 0, bmpData.Stride * bmpData.Height)
        Dim r As New Random()

        ' 2: One by one, find the lightest pixel on the image. This work is computationally expensive
        ' so we're going to run it on a background thread to take the heat off the UI thread
        ' (if we had several CPU cores free, we might want to run it on all of them.)
        Await TaskEx.Run(
            Sub()
                While True
                    cancel.ThrowIfCancellationRequested()

                    ' find the lightest pixel in the image that's not already light
                    Dim x1 = 0, y1 = 0, i1 = 0, lightness1 = -1
                    Dim yc = r.Next(bmp.Height), xc = r.Next(bmp.Width)
                    For y = Math.Max(0, yc - 100) To Math.Min(yc + 100, bmp.Height) - 1 Step 2
                        For x = Math.Max(0, xc - 100) To Math.Min(xc + 100, bmp.Width) - 1 Step 2
                            Dim i = y * bmpData.Stride + x * 4
                            Dim lightness = 0.0 + p(i + 2) + p(i + 1) + p(i)
                            If lightness >= 760 Then Continue For ' almost light is light enough
                            If lightness > lightness1 Then lightness1 = lightness : x1 = x : y1 = y : i1 = i
                        Next
                    Next

                    ' 3. If we didn't find any non-light pixels, then we're finished:
                    If lightness1 = -1 Then Exit While

                    ' But if we did find non-light pixels, then mark them as light and report progress:
                    For y = 0 To 3
                        For x = 0 To 3
                            If y1 + y >= bmp.Height OrElse x1 + x >= bmp.Width Then Continue For
                            Dim offset = y * bmpData.Stride + x * 3
                            p(i1 + offset + 2) = 255
                            p(i1 + offset + 1) = 255
                            p(i1 + offset + 0) = 255
                        Next
                    Next
                    If progress IsNot Nothing Then progress.Report(New Point(x1, y1))
                End While
            End Sub)

        ' 5. Once we've made everything light, we can return our bitmap.
        bmp.UnlockBits(bmpData)
        Return bmp
    End Function

End Class


Module Helpers
    <Extension()>
    Sub EventHandler(this As CancellationTokenSource, sender As Object, e As EventArgs)
        this.Cancel()
    End Sub
End Module
