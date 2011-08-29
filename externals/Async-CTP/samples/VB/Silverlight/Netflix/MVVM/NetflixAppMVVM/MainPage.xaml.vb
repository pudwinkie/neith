Imports System.Windows.Data
Imports System.Collections.ObjectModel
Imports NetflixAppMVVM.Model
Imports System.Threading
Imports System.Globalization

Partial Public Class MainPage
    Inherits UserControl

    Public Sub New()
        InitializeComponent()
    End Sub

End Class



Public Class MainPageViewModel
    Inherits DependencyObject

    Private Const defaultQuery = "http://odata.netflix.com/Catalog/Titles?$inlinecount=allpages&$filter=ReleaseYear%20le%201942"

    Dim q As NetflixQuery(Of Movie)
    Dim cancelSource As CancellationTokenSource

    Async Sub DoFetch()
        Fetching = True
        Progress = Nothing
        ProgressText = "Loading..."
        cancelSource = New CancellationTokenSource
        q = New NetflixQuery(Of Movie)(Query)
        Movies = q.Entities
        AddHandler Movies.CollectionChanged,
            Sub()
                If q.EntitiesExpected IsNot Nothing Then
                    Progress = Movies.Count / q.EntitiesExpected
                    ProgressText = String.Format("Loaded {0} of {1} movies so far...", Movies.Count, q.EntitiesExpected)
                Else
                    Progress = Nothing
                    ProgressText = String.Format("Loaded {0} movies so far...", Movies.Count, q.EntitiesExpected)
                End If
            End Sub

        Try
            Await q.FetchEntitiesAsync(cancelSource.Token)
            ProgressText = String.Format("Loaded {0} movies.", Movies.Count)
        Catch ex As OperationCanceledException
            ProgressText = String.Format("Cancelled after {0} movies.", Movies.Count)
        Catch ex As Exception
            ProgressText = "Error!"
            MessageBox.Show(ex.ToString())
        End Try

        Progress = 1.0
        Fetching = False
    End Sub

    Sub DoCancel()
        cancelSource.Cancel()
    End Sub

#Region "Dependency Properties"
    ' DependencyProperties enable animation, styling, binding, etc...

    Public Shared ReadOnly QueryProperty As DependencyProperty =
        DependencyProperty.Register("Query", GetType(String), GetType(MainPageViewModel), New PropertyMetadata(defaultQuery))

    Public Shared ReadOnly MoviesProperty As DependencyProperty =
        DependencyProperty.Register("Movies", GetType(ObservableCollection(Of Movie)), GetType(MainPageViewModel), New PropertyMetadata(Nothing))

    Public Shared ReadOnly ProgressProperty As DependencyProperty =
        DependencyProperty.Register("Progress", GetType(Double?), GetType(MainPageViewModel), New PropertyMetadata(0.0))

    Public Shared ReadOnly FetchingProperty As DependencyProperty =
        DependencyProperty.Register("Fetching", GetType(Boolean), GetType(MainPageViewModel), New PropertyMetadata(
                                    New PropertyChangedCallback(Sub(d, e)
                                                                    Dim vm = CType(d, MainPageViewModel)
                                                                    vm.Fetch.Invalidate()
                                                                    vm.Cancel.Invalidate()
                                                                End Sub)))

    Public Shared ReadOnly ProgressTextProperty As DependencyProperty =
        DependencyProperty.Register("ProgressText", GetType(String), GetType(MainPageViewModel), New PropertyMetadata(""))

    Public Property Query As String
        Get
            Return CStr(GetValue(QueryProperty))
        End Get
        Set(value As String)
            SetValue(QueryProperty, value)
        End Set
    End Property

    Public Property Movies As ObservableCollection(Of Movie)
        Get
            Return CType(GetValue(MoviesProperty), ObservableCollection(Of Movie))
        End Get
        Set(value As ObservableCollection(Of Movie))
            SetValue(MoviesProperty, value)
        End Set
    End Property

    Public Property Progress As Double?
        Get
            Return CType(GetValue(ProgressProperty), Double?)
        End Get
        Set(value As Double?)
            SetValue(ProgressProperty, value)
        End Set
    End Property

    Public Property Fetching As Boolean
        Get
            Return CBool(GetValue(FetchingProperty))
        End Get
        Set(value As Boolean)
            SetValue(FetchingProperty, value)
        End Set
    End Property

    Public Property ProgressText As String
        Get
            Return CStr(GetValue(ProgressTextProperty))
        End Get
        Set(value As String)
            SetValue(ProgressTextProperty, value)
        End Set
    End Property
#End Region

#Region "Commands"
    Private _fetch As FetchCommand = Nothing
    Public ReadOnly Property Fetch As FetchCommand
        Get
            If _fetch Is Nothing Then _fetch = New FetchCommand With {.vm = Me}
            Return _fetch
        End Get
    End Property


    Public Class FetchCommand
        Implements ICommand
        Friend vm As MainPageViewModel

        Friend Sub Invalidate()
            RaiseEvent CanExecuteChanged(Me, EventArgs.Empty)
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements System.Windows.Input.ICommand.CanExecute
            Return Not vm.Fetching
        End Function

        Public Event CanExecuteChanged(sender As Object, e As System.EventArgs) Implements System.Windows.Input.ICommand.CanExecuteChanged

        Public Sub Execute(parameter As Object) Implements System.Windows.Input.ICommand.Execute
            vm.DoFetch()
        End Sub

    End Class

    Private _cancel As CancelCommand = Nothing
    Public ReadOnly Property Cancel As CancelCommand
        Get
            If _cancel Is Nothing Then _cancel = New CancelCommand With {.vm = Me}
            Return _cancel
        End Get
    End Property

    Public Class CancelCommand
        Implements ICommand
        Friend vm As MainPageViewModel

        Friend Sub Invalidate()
            RaiseEvent CanExecuteChanged(Me, EventArgs.Empty)
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements System.Windows.Input.ICommand.CanExecute
            Return vm.Fetching
        End Function

        Public Event CanExecuteChanged(sender As Object, e As System.EventArgs) Implements System.Windows.Input.ICommand.CanExecuteChanged

        Public Sub Execute(parameter As Object) Implements System.Windows.Input.ICommand.Execute
            vm.DoCancel()
        End Sub
    End Class
#End Region

End Class



Public Class NotConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Return Not CBool(value)
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException
    End Function
End Class

Public Class HasValueConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim valueExpected = Boolean.Parse(If(TryCast(parameter, String), "True"))
        If valueExpected Then Return (value IsNot Nothing) Else Return (value Is Nothing)
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException
    End Function
End Class

Public Class CoalesceConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Return If(value, parameter)
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException
    End Function
End Class

