Imports System.Threading
Imports System.Runtime.CompilerServices

Public Module ExcelInterop

    <Extension()>
    Public Sub StartAsyncMethod(Application As Microsoft.Office.Interop.Excel.Application)
        If s_app Is Nothing Then s_app = Application
        If s_UI_SC Is Nothing Then
            If SynchronizationContext.Current Is Nothing Then
                Using c As New System.Windows.Forms.Control : Dim dummy = c.Handle : End Using
            End If
            s_UI_SC = SynchronizationContext.Current
        End If
        If s_Excel_SC Is Nothing Then s_Excel_SC = New ExcelSynchronizationContext
        If Not (TypeOf SynchronizationContext.Current Is ExcelSynchronizationContext) Then SynchronizationContext.SetSynchronizationContext(s_Excel_SC)
    End Sub

    Private s_queue As New Queue(Of Tuple(Of SendOrPostCallback, Object))
    Private s_app As Microsoft.Office.Interop.Excel.Application = Nothing
    Private s_UI_SC As SynchronizationContext = Nothing
    Private s_Excel_SC As New ExcelSynchronizationContext

    Private Class ExcelSynchronizationContext
        Inherits SynchronizationContext

        Public Overrides Sub Send(d As System.Threading.SendOrPostCallback, state As Object)
            Throw New NotSupportedException
        End Sub

        Public Overrides Sub Post(d As System.Threading.SendOrPostCallback, state As Object)
            SyncLock s_queue
                s_queue.Enqueue(Tuple.Create(d, state))
                If s_queue.Count = 1 Then s_UI_SC.Post(AddressOf OnThread, Nothing)
            End SyncLock
        End Sub

        Private Sub OnThread(state As Object)
            SynchronizationContext.SetSynchronizationContext(s_Excel_SC)
            Dim isReady = False
            Try
                Dim range = s_app.Sheets(1).Cells(1, 1)
                range.Value = range.Value
                If s_app.Ready Then isReady = True
            Catch ex As Runtime.InteropServices.COMException
                If ex.ErrorCode <> -2146827284 AndAlso ex.ErrorCode <> -2146777998 Then Throw
            End Try

            If Not isReady Then
                Call New Timer(Sub(self)
                                   DirectCast(self, IDisposable).Dispose()
                                   s_UI_SC.Post(AddressOf OnThread, Nothing)
                               End Sub).Change(250, Timeout.Infinite)
            Else
                Dim q2 As Queue(Of Tuple(Of SendOrPostCallback, Object))
                SyncLock s_queue
                    q2 = New Queue(Of Tuple(Of SendOrPostCallback, Object))(s_queue)
                    s_queue.Clear()
                End SyncLock
                For Each ds In q2
                    ds.Item1.Invoke(ds.Item2)
                Next
            End If
        End Sub
    End Class

End Module
