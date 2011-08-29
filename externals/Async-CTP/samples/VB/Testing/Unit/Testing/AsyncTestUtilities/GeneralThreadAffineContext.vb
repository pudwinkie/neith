Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Collections.Concurrent
Imports WorkItem = System.Collections.Generic.KeyValuePair(Of System.Threading.SendOrPostCallback, Object)

Public Module GeneralThreadAffineContext

    Class WorkQueue
        Private m_queue As New BlockingCollection(Of WorkItem)

        Friend Sub Shutdown()
            m_queue.CompleteAdding()
        End Sub

        Friend Sub Enqueue(workItem As WorkItem)
            Try
                m_queue.Add(workItem)
            Catch ex As InvalidOperationException
            End Try
        End Sub

        Friend Sub ExecuteWorkQueueLoop()
            For Each currentItem In m_queue.GetConsumingEnumerable()
                currentItem.Key.Invoke(currentItem.Value)
            Next
        End Sub
    End Class

    Class Context

        Inherits SynchronizationContext
        Friend ReadOnly WorkQueue As WorkQueue

        Friend Sub New()
            MyClass.New(New WorkQueue())
        End Sub

        Protected Sub New(queue As WorkQueue)
            Me.WorkQueue = queue
        End Sub

        Public Overrides Sub Post(callback As SendOrPostCallback, state As Object)
            WorkQueue.Enqueue(New WorkItem(callback, state))
        End Sub

        Public Overrides Function CreateCopy() As SynchronizationContext
            Return New Context(WorkQueue)
        End Function

        Public Overrides Sub Send(d As SendOrPostCallback, state As Object)
            Throw New NotImplementedException()
        End Sub

    End Class

    Public Function Run(Of TResult)(asyncMethod As Func(Of Task(Of TResult))) As Task(Of TResult)
        Return DirectCast(Run(DirectCast(asyncMethod, Func(Of Task))), Task(Of TResult))
    End Function

    Public Function Run(asyncMethod As Func(Of Task)) As Task
        Using SynchronizationContextSwitcher.Capture()

            Dim customContext = New Context()
            SynchronizationContext.SetSynchronizationContext(customContext)

            Dim task = asyncMethod.Invoke()

            If task IsNot Nothing Then
                task.ContinueWith(Sub() customContext.WorkQueue.Shutdown(), TaskContinuationOptions.ExecuteSynchronously)
            Else
                Return task
            End If

            customContext.WorkQueue.ExecuteWorkQueueLoop()

            task.RethrowForCompletedTasks()

            Return task
        End Using
    End Function

    ''' <summary>
    ''' Runs the action inside a message loop and continues looping work items
    ''' as long as any asynchronous operations have been registered
    ''' </summary>
    Public Sub Run(asyncAction As Action)

        Using SynchronizationContextSwitcher.Capture()

            Dim customContext = New VoidContext()
            SynchronizationContext.SetSynchronizationContext(customContext)

            customContext.OperationStarted()
            Try
                asyncAction.Invoke()
            Finally
                customContext.OperationCompleted()
            End Try

            customContext.WorkQueue.ExecuteWorkQueueLoop()

        End Using
    End Sub

    Class VoidContext

        Inherits Context

        Dim operationCount As Integer

        ''' <summary>Constructor for creating a new AsyncVoidSyncContext. Creates a new shared operation counter.</summary>
        Friend Sub New()
        End Sub

        Sub New(queue As WorkQueue)
            MyBase.New(queue)
        End Sub

        Public Overrides Function CreateCopy() As SynchronizationContext
            Return New VoidContext(Me.WorkQueue)
        End Function

        Public Overrides Sub OperationStarted()
            Interlocked.Increment(Me.operationCount)
        End Sub

        Public Overrides Sub OperationCompleted()
            If Interlocked.Decrement(Me.operationCount) = 0 Then Me.WorkQueue.Shutdown()
        End Sub

    End Class

End Module
