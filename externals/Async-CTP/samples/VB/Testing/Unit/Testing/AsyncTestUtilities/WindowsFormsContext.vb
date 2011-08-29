Imports System
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports System.Runtime.CompilerServices

''' <summary>
''' Async methods can run in a myriad of contexts - some have a "thread affinity"
''' such that continuations are posted back in a way that ensures that they always
''' execute on the originating thread.
''' 
''' Windows Forms is one of such contexts.
''' </summary>
Public Module WindowsFormsContext

    ''' <summary>
    ''' Runs the function inside a message loop and continues pumping messages
    ''' until the returned task completes. 
    ''' </summary>
    ''' <returns>The completed task returned by the delegate's invocation</returns>
    Public Function Run(Of TResult)(func As Func(Of Task(Of TResult))) As Task(Of TResult)
        Return (DirectCast(Run(DirectCast(func, Func(Of Task))), Task(Of TResult)))
    End Function

    ''' <summary>
    ''' Runs the function inside a message loop and continues pumping messages
    ''' until the returned task completes. 
    ''' </summary>
    ''' <returns>The completed task returned by the delegate's invocation</returns>
    Public Function Run(func As Func(Of Task)) As Task
        Using InstallerAndRestorer.Install()

            ' InstallerAndRestorer ensures the WinForms context is installed
            Dim winFormsContext = SynchronizationContext.Current

            Dim message = New TaskFunctionLaunchMessage(func, winFormsContext)
            winFormsContext.Post(AddressOf message.LaunchMessageImpl, state:=Nothing)
            Application.Run()
            If message.ReturnedTask IsNot Nothing Then
                message.ReturnedTask.RethrowForCompletedTasks()
            End If
            Return message.ReturnedTask
        End Using
    End Function

    Class TaskFunctionLaunchMessage
        Public ReturnedTask As Task
        ReadOnly taskFunction As Func(Of Task)
        ReadOnly postingContext As SynchronizationContext

        Public Sub New(taskFunction As Func(Of Task), postingContext As SynchronizationContext)
            Me.taskFunction = taskFunction
            Me.postingContext = postingContext
        End Sub

        ' this signature is to match SendOrPostCallback
        Public Sub LaunchMessageImpl(ignoredState As Object)
            ReturnedTask = taskFunction.Invoke()
            If ReturnedTask IsNot Nothing Then
                ReturnedTask.ContinueWith(Sub() postingContext.RequestMessageLoopTermination(), TaskContinuationOptions.ExecuteSynchronously)
            Else
                Application.ExitThread()
            End If
        End Sub

    End Class

    ''' <summary>
    ''' Runs the action inside a message loop and continues pumping messages
    ''' as long as any asynchronous operations have been registered
    ''' </summary>
    Public Sub Run(asyncAction As Action)
        Using InstallerAndRestorer.Install()
            ' InstallerAndRestorer ensures the WinForms context is installed
            ' capture that WinFormsContext
            Dim winFormsContext = SynchronizationContext.Current
            ' wrap the WinForms context in our own decorator context and install that
            Dim asyncVoidContext = New AsyncVoidSyncContext(winFormsContext)
            SynchronizationContext.SetSynchronizationContext(asyncVoidContext)
            ' queue up the first message before we start running the loop
            Dim message = New AsyncActionLaunchMessage(asyncAction, asyncVoidContext)
            asyncVoidContext.Post(AddressOf message.LaunchMessageImpl, state:=Nothing)
            Application.Run()
        End Using
    End Sub

    Class AsyncActionLaunchMessage
        ReadOnly asyncAction As Action
        ReadOnly postingContext As AsyncVoidSyncContext

        Public Sub New(asyncAction As Action, postingContext As AsyncVoidSyncContext)
            Me.asyncAction = asyncAction
            Me.postingContext = postingContext
        End Sub

        ' this signature is to match SendOrPostCallback
        Public Sub LaunchMessageImpl(ignoredState As Object)
            postingContext.OperationStarted()
            Try
                asyncAction.Invoke()
            Finally
                postingContext.OperationCompleted()
            End Try
        End Sub

    End Class

    <Extension()>
    Sub RequestMessageLoopTermination(syncContext As SynchronizationContext)
        syncContext.Post(Sub(state) Application.ExitThread(), state:=Nothing)
    End Sub

    Structure InstallerAndRestorer
        Implements IDisposable

        Dim originalAutoInstallValue As Boolean
        Dim originalSyncContext As SynchronizationContext
        Dim tempControl As Control

        Public Shared Function Install() As InstallerAndRestorer
            Dim iar = New InstallerAndRestorer()
            iar.originalAutoInstallValue = WindowsFormsSynchronizationContext.AutoInstall
            iar.originalSyncContext = SynchronizationContext.Current
            WindowsFormsSynchronizationContext.AutoInstall = True
            iar.tempControl = New Control With {.Visible = False}
            Return iar
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            If tempControl IsNot Nothing Then
                tempControl.Dispose()
                tempControl = Nothing
            End If
            WindowsFormsSynchronizationContext.AutoInstall = originalAutoInstallValue
            SynchronizationContext.SetSynchronizationContext(originalSyncContext)
        End Sub

    End Structure

    Class AsyncVoidSyncContext

        Inherits SynchronizationContext
        ReadOnly inner As SynchronizationContext
        Dim operationCount As Integer

        ''' <summary>Constructor for creating a new AsyncVoidSyncContext. Creates a new shared operation counter.</summary>
        Public Sub New(innerContext As SynchronizationContext)
            Me.inner = innerContext
        End Sub

        Public Overrides Function CreateCopy() As SynchronizationContext
            Return New AsyncVoidSyncContext(Me.inner.CreateCopy())
        End Function

        Public Overrides Sub Post(d As SendOrPostCallback, state As Object)
            inner.Post(d, state)
        End Sub

        Public Overrides Sub Send(d As SendOrPostCallback, state As Object)
            inner.Send(d, state)
        End Sub

        Public Overrides Function Wait(waitHandles As IntPtr(), waitAll As Boolean, millisecondsTimeout As Integer) As Integer
            Return inner.Wait(waitHandles, waitAll, millisecondsTimeout)
        End Function

        Public Overrides Sub OperationStarted()
            inner.OperationStarted()
            Interlocked.Increment(Me.operationCount)
        End Sub

        Public Overrides Sub OperationCompleted()
            inner.OperationCompleted()
            If Interlocked.Decrement(Me.operationCount) = 0 Then
                Me.RequestMessageLoopTermination()
            End If
        End Sub

    End Class

End Module