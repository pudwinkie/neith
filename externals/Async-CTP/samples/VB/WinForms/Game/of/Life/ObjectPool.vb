'--------------------------------------------------------------------------
' 
'  Copyright (c) Microsoft Corporation.  All rights reserved. 
' 
'  File: ObjectPool.vb
'
'--------------------------------------------------------------------------

Imports System.Collections.Concurrent

'''<summary>Provides a thread-safe object pool.</summary>
'''<typeparam name="T">Specifies the type of the elements stored in the pool.</typeparam>
<DebuggerDisplay("Count={Count}")>
Public NotInheritable Class ObjectPool(Of T)
    Private ReadOnly m_generator As Func(Of T)
    Private ReadOnly m_objects As IProducerConsumerCollection(Of T) = New ConcurrentQueue(Of T)()

    ''' <summary>Initializes an instance of the ObjectPool class.</summary>
    ''' <param name="generator">The function used to create items when no items exist in the pool.</param>
    Public Sub New(generator As Func(Of T))
        If (generator Is Nothing) Then Throw New ArgumentNullException("generator")
        m_generator = generator
    End Sub

    ''' <summary>Adds the provided item into the pool.</summary>
    ''' <param name="item">The item to be added.</param>
    Public Sub PutObject(item As T)
        m_objects.TryAdd(item)
    End Sub

    '''<summary>Gets an item from the pool.</summary>
    ''' <returns>The removed or created item.</returns>
    ''' <remarks>If the pool is empty, a new item will be created and returned.</remarks>
    Public Function GetObject() As T
        Dim value As T
        Return If(m_objects.TryTake(value), value, m_generator())
    End Function

End Class