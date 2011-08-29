Imports System
Imports System.Linq.Expressions
Imports System.Threading.Tasks

Class Mock(Of TInterface)

    ReadOnly mockImpl As MockSetup = New MockSetup()

    Friend Sub New()
        If Not GetType(TInterface).[Equals](GetType(IStockSnapshotProvider)) Then
            Throw New NotImplementedException("This sample only supports mocking IStockSnapshotProvider")
        End If
    End Sub

    ' Our setup method only supports mocking IStockSnapshotProvider.GetLatestSnapshotAsync()
    Friend Function Setup([function] As Expression(Of Func(Of IStockSnapshotProvider, Task(Of StockSnapshot)))) As MockSetup
        If [function].Body.NodeType = ExpressionType.[Call] Then
            Dim innerCall = DirectCast([function].Body, MethodCallExpression)
            Dim methodInfo = innerCall.Method
            If GetType(IStockSnapshotProvider).[Equals](methodInfo.DeclaringType) AndAlso methodInfo.IsStatic = False AndAlso methodInfo.Name.[Equals]("GetLatestSnapshotAsync") AndAlso methodInfo.GetGenericArguments().Length = 0 AndAlso methodInfo.GetParameters().Length = 0 Then
                Return Me.mockImpl
            End If
        End If
        Throw New NotImplementedException("This sample only supports mocking IStockSnapshotProvider.GetLatestSnapshotAsync()")
    End Function

    Friend ReadOnly Property [Object] As IStockSnapshotProvider
        Get
            Return Me.mockImpl
        End Get
    End Property

End Class

Class MockSetup
    Implements IStockSnapshotProvider

    Dim generalCallback As Action = Nothing
    Dim valueCallback As Func(Of Task(Of StockSnapshot)) = Nothing
    Dim fixedValue As Task(Of StockSnapshot) = Nothing

    Friend Function Callback(action As Action) As MockSetup
        Me.generalCallback = action
        Return Me
    End Function

    Friend Sub Returns(fixedValue As Task(Of StockSnapshot))
        Me.fixedValue = fixedValue
    End Sub

    Friend Sub Returns(callback As Func(Of Task(Of StockSnapshot)))
        Me.valueCallback = callback
    End Sub

    Function GetLatestSnapshotAsync() As Task(Of StockSnapshot) Implements IStockSnapshotProvider.GetLatestSnapshotAsync
        If Me.generalCallback IsNot Nothing Then
            Me.generalCallback.Invoke()
        End If
        If Me.valueCallback IsNot Nothing Then
            Return valueCallback.Invoke()
        End If
        If Me.fixedValue IsNot Nothing Then
            Return fixedValue
        End If
        Throw New NotImplementedException("Not enough information to mock IStockSnapshotProvider")
    End Function

End Class