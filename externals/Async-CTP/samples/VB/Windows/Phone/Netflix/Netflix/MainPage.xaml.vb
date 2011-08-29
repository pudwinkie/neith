Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Windows.Shapes
Imports System.Xml.Linq
Imports System.Net
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Collections.ObjectModel
Imports System.ComponentModel

Imports <xmlns:xa="http://www.w3.org/2005/Atom">
Imports <xmlns:xd="http://schemas.microsoft.com/ado/2007/08/dataservices">
Imports <xmlns:xm="http://schemas.microsoft.com/ado/2007/08/dataservices/metadata">

Partial Public Class MainPage
    Inherits PhoneApplicationPage

    ' Constructor
    Public Sub New()
        InitializeComponent()
    End Sub

    Const query = "http://odata.netflix.com/Catalog/Titles?$filter=ReleaseYear eq {0}&$skip={1}&$top={2}&$select=Url,ReleaseYear,Rating,Runtime,AverageRating,BoxArt"

    Private Sub textBox_KeyDown(sender As System.Object, e As System.Windows.Input.KeyEventArgs) Handles TextBox.KeyDown
        If e.Key = Key.Enter Then Me.Focus()
    End Sub

    Private Sub textBox_LostFocus(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles TextBox.LostFocus
        Dim year As Integer
        If Integer.TryParse(Me.textBox.Text, year) Then
            ' Check whether this year is already in the pivot:
            Dim yearMovies = yearPivot.Items.OfType(Of YearMovies)().Where(Function(ym) ym.Year = year).SingleOrDefault()

            If yearMovies Is Nothing Then
                LoadMoviesAsync(year)
            Else
                yearPivot.SelectedItem = yearMovies
            End If
        End If
    End Sub

    Async Sub LoadMoviesAsync(year As Integer)
        Dim movieCollection As New ObservableCollection(Of Movie)
        Dim yearMovies As New YearMovies With
        {
            .Year = year,
            .Movies = movieCollection
        }
        yearPivot.Items.Add(yearMovies)
        yearPivot.SelectedItem = yearMovies

        yearMovies.StatusText = ""

        Dim pageSize = 10
        Dim imageCount = 0
        Do
            yearMovies.StatusText = String.Format("Searching...  {0} titles so far...", imageCount)
            Dim movies = Await QueryMoviesAsync(year, imageCount, pageSize)
            If movies.Length = 0 Then Exit Do
            For Each movie In movies
                movieCollection.Add(movie)
            Next
            imageCount += movies.Length
        Loop
        yearMovies.StatusText = String.Format("{0} titles found", imageCount)
    End Sub

    Async Function QueryMoviesAsync(year As Integer, first As Integer, count As Integer) As Task(Of Movie())
        Dim client As New WebClient
        Dim url = String.Format(query, year, first, count)
        Dim data = Await client.DownloadStringTaskAsync(New Uri(url))

        Return Await TaskEx.Run(Function()
                                    Dim movies =
                                        From entry In XDocument.Parse(data)...<xa:entry>
                                        Let properties = entry.<xm:properties>
                                        Select New Movie With
                                        {
                                            .Title = entry.<xa:title>.Value,
                                            .Url = properties.<xd:Url>.Value,
                                            .Year = properties.<xd:ReleaseYear>.Value,
                                            .Rating = properties.<xd:Rating>.Value,
                                            .Length = String.Format("{0} min", Math.Round(Integer.Parse("0" & properties.<xd:Runtime>.Value) / 60.0)),
                                            .UserReview = New String("*", Math.Round(Decimal.Parse("0" + properties.<xd:AverageRating>.Value))),
                                            .BoxArtUrl = properties.<xd:BoxArt>.<xd:LargeUrl>.Value
                                        }
                                    Return movies.ToArray()
                                End Function)
    End Function

    Private Sub DeferredLoadListBox_SelectionChanged(sender As System.Object, e As System.Windows.Controls.SelectionChangedEventArgs)
        Dim listbox = TryCast(sender, ListBox)
        If listbox Is Nothing Then Return

        Dim item = TryCast(listbox.SelectedItem, Movie)
        If item Is Nothing Then Return

        webBrowser.Visibility = Visibility.Visible
        webBrowser.Navigate(New Uri(item.Url))
    End Sub

    Protected Overrides Sub OnBackKeyPress(e As CancelEventArgs)
        If webBrowser.Visibility = Visibility.Visible Then
            webBrowser.Visibility = Visibility.Collapsed

            e.Cancel = True
        End If
    End Sub
End Class

Public Class YearMovies
    Implements INotifyPropertyChanged

    Public Property Year As Integer
    Public Property Movies As ObservableCollection(Of Movie)

    Private myStatusText As String
    Public Property StatusText As String
        Get
            Return myStatusText
        End Get
        Set(value As String)
            myStatusText = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("StatusText"))
        End Set
    End Property

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
End Class

Public Class Movie
    Public Property Title As String
    Public Property Year As String
    Public Property Rating As String
    Public Property Length As String
    Public Property UserReview As String
    Public Property Url As String
    Public Property BoxArtUrl As String
End Class
