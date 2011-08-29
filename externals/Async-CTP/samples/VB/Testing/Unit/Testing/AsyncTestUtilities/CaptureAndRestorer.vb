Imports System.Threading

Friend Structure SynchronizationContextSwitcher
    Implements IDisposable

    Dim originalSyncContext As SynchronizationContext

    Public Shared Function Capture() As SynchronizationContextSwitcher
        Return New SynchronizationContextSwitcher With {.originalSyncContext = SynchronizationContext.Current}
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        SynchronizationContext.SetSynchronizationContext(originalSyncContext)
    End Sub

End Structure