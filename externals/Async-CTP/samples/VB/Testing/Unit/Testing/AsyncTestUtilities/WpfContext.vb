Imports System
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Threading

''' <summary>
''' Async methods can run in a myriad of contexts - some have a "thread affinity"
''' such that continuations are posted back in a way that ensures that they always
''' execute on the originating thread.
''' 
''' WPF is one of such contexts.
''' </summary>
Public Module WpfContext

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
        Using SynchronizationContextSwitcher.Capture()
            Dim dispatcher As Dispatcher = dispatcher.CurrentDispatcher
            Dim frame As DispatcherFrame = New DispatcherFrame(exitWhenRequested:=True)
            Dim message = New TaskFunctionLaunchMessage(func, dispatcher, frame)
            dispatcher.BeginInvoke(New Action(AddressOf message.LaunchMessageImpl))
            dispatcher.PushFrame(frame)
            If message.ReturnedTask IsNot Nothing Then
                message.ReturnedTask.RethrowForCompletedTasks()
            End If
            Return message.ReturnedTask
        End Using
    End Function

    Class TaskFunctionLaunchMessage
        Public ReturnedTask As Task
        ReadOnly taskFunction As Func(Of Task)
        ReadOnly dispatcher As Dispatcher
        ReadOnly frame As DispatcherFrame

        Public Sub New(taskFunction As Func(Of Task), postingContext As Dispatcher, frame As DispatcherFrame)
            Me.taskFunction = taskFunction
            Me.dispatcher = postingContext
            Me.frame = frame
        End Sub

        Public Sub LaunchMessageImpl()
            ReturnedTask = taskFunction.Invoke()
            If ReturnedTask IsNot Nothing Then
                ReturnedTask.ContinueWith(
                    Sub() dispatcher.BeginInvoke(Sub() frame.Continue = False),
                    TaskContinuationOptions.ExecuteSynchronously)
            Else
                frame.Continue = False
            End If
        End Sub

    End Class

    ''' <summary>
    ''' Runs the action inside a message loop and continues pumping messages
    ''' as long as any asynchronous operations have been registered
    ''' </summary>
    Public Sub Run(asyncAction As Action)
        Using SynchronizationContextSwitcher.Capture()
            Dim dispatcher As Dispatcher = dispatcher.CurrentDispatcher
            Dim frame As DispatcherFrame = New DispatcherFrame(exitWhenRequested:=True)
            Dim message = New AsyncActionLaunchMessage(asyncAction, dispatcher, frame)
            dispatcher.BeginInvoke(New Action(AddressOf message.LaunchMessageImpl))
            dispatcher.PushFrame(frame)
        End Using
    End Sub

    Class AsyncActionLaunchMessage
        ReadOnly asyncAction As Action
        ReadOnly dispatcher As Dispatcher
        ReadOnly frame As DispatcherFrame
        Dim asyncVoidContext As AsyncVoidSyncContext

        Public Sub New(asyncAction As Action, dispatcher As Dispatcher, frame As DispatcherFrame)
            Me.asyncAction = asyncAction
            Me.dispatcher = dispatcher
            Me.frame = frame
        End Sub

        Public Sub LaunchMessageImpl()
            Me.asyncVoidContext = New AsyncVoidSyncContext(SynchronizationContext.Current, frame)
            SynchronizationContext.SetSynchronizationContext(asyncVoidContext)
            asyncVoidContext.OperationStarted()
            Try
                asyncAction.Invoke()
            Finally
                asyncVoidContext.OperationCompleted()
            End Try
        End Sub

    End Class

    Class AsyncVoidSyncContext

        Inherits SynchronizationContext
        ReadOnly inner As SynchronizationContext
        ReadOnly frame As DispatcherFrame
        Dim operationCount As Integer

        ''' <summary>Constructor for creating a new AsyncVoidSyncContext. Creates a new shared operation counter.</summary>
        Public Sub New(innerContext As SynchronizationContext, frame As DispatcherFrame)
            Me.inner = innerContext
            Me.frame = frame
        End Sub

        Public Overrides Function CreateCopy() As SynchronizationContext
            Return New AsyncVoidSyncContext(Me.inner.CreateCopy(), frame)
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
                Me.Post(Sub(ignoredState) Me.frame.Continue = False, state:=Nothing)
            End If
        End Sub

    End Class

End Module