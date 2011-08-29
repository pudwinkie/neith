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

Imports <xmlns:xa="http://www.w3.org/2005/Atom">
Imports <xmlns:xd="http://schemas.microsoft.com/ado/2007/08/dataservices">
Imports <xmlns:xm="http://schemas.microsoft.com/ado/2007/08/dataservices/metadata">

Namespace VB_Netflix_WPF_AsyncWithoutAwait
    Partial Public Class MainWindow
        Inherits Window

        Const query = "http://odata.netflix.com/Catalog/Titles?$filter=ReleaseYear eq {0}&$skip={1}&$top={2}&$select=Url,BoxArt"

        Class Movie
            Public Property Title As String
            Public Property Url As String
            Public Property BoxArtUrl As String
        End Class

        Dim cts As CancellationTokenSource

        Public Sub New()
            InitializeComponent()
            textBox.Focus()
        End Sub

        Private Sub searchButton_Click(sender As Object, e As RoutedEventArgs) Handles searchButton.Click
            LoadMoviesAsync(Integer.Parse(textBox.Text))

            Dim timer = New Timer(
                Sub(self)
                    CType(self, Timer).Dispose()
                    If cts IsNot Nothing Then
                        cts.Cancel()
                        Dispatcher.BeginInvoke(CType(Sub() statusText.Text = "Timeout", Action))
                    End If
                End Sub)

            timer.Change(20000, Timeout.Infinite)
        End Sub

        Private Sub textBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles textBox.TextChanged
            Dim year As Integer
            searchButton.IsEnabled = Integer.TryParse(textBox.Text, year) AndAlso year >= 1900 AndAlso year <= 2099
        End Sub

        Private Sub cancelButton_Click(sender As Object, e As RoutedEventArgs) Handles cancelButton.Click
            If cts IsNot Nothing Then
                cts.Cancel()
            End If
        End Sub

        Sub LoadMoviesAsync(year As Integer)
            resultsPanel.Children.Clear()
            cts = New CancellationTokenSource
            statusText.Text = ""
            Dim pageSize = 10
            Dim imageCount = 0
            Dim action As Action(Of Movie(), Exception) = Nothing
            action =
                Sub(movies, ex)
                    If ex IsNot Nothing Then
                        cts = Nothing
                        If (TypeOf ex Is TaskCanceledException OrElse
                            (TypeOf ex Is WebException AndAlso CType(ex, WebException).Status = WebExceptionStatus.RequestCanceled)) Then
                            statusText.Text = "Cancelled"
                            Return
                        Else
                            Throw ex
                        End If
                    End If

                    statusText.Text = String.Format("Searching...  {0} Titles", imageCount)
                    If (movies.Length > 0) Then
                        DisplayMovies(movies)
                        imageCount += movies.Length
                        QueryMoviesAsync(year, imageCount, pageSize, cts.Token, action)
                    Else
                        statusText.Text = String.Format("{0} Titles", imageCount)
                    End If
                End Sub

            QueryMoviesAsync(year, imageCount, pageSize, cts.Token, action)
        End Sub

        Sub QueryMoviesAsync(year As Integer, first As Integer, count As Integer, ct As CancellationToken, processMovies As Action(Of Movie(), Exception))
            Dim client As New WebClient
            Dim url = String.Format(query, year, first, count)

            ct.Register(Sub() client.CancelAsync())

            AddHandler client.DownloadStringCompleted,
                Sub(sender, e)
                    If e.Error IsNot Nothing Then
                        processMovies(Nothing, e.Error)
                        Return
                    End If
                    If e.Cancelled Then
                        processMovies(Nothing, New TaskCanceledException)
                        Return
                    End If

                    Dim data = e.Result
                    Dim movies =
                        From entry In XDocument.Parse(data)...<xa:entry>
                        Let properties = entry.<xm:properties>
                        Select New Movie With
                        {
                            .Title = entry.<xa:title>.Value,
                            .Url = properties.<xd:Url>.Value,
                            .BoxArtUrl = properties.<xd:BoxArt>.<xd:LargeUrl>.Value
                        }
                    processMovies(movies.ToArray(), Nothing)
                End Sub

            Try
                client.DownloadStringAsync(New Uri(url))
            Catch ex As Exception
                processMovies(Nothing, ex)
            End Try
        End Sub

        Sub DisplayMovies(movies As Movie())
            For Each movie In movies
                Dim image As New Image With
                {
                    .Source = New BitmapImage(New Uri(movie.BoxArtUrl)),
                    .Width = 110,
                    .Height = 150,
                    .Margin = New Thickness(5),
                    .ToolTip = New ToolTip With {.Content = movie.Title}
                }
                Dim url = movie.Url
                AddHandler image.MouseDown, Sub(sender, e) System.Diagnostics.Process.Start(url)

                resultsPanel.Children.Add(image)
            Next
        End Sub

    End Class
End Namespace
