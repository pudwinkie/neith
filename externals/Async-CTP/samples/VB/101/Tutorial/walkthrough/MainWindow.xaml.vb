Imports <xmlns:xa="http://www.w3.org/2005/Atom">
Imports <xmlns:xd="http://schemas.microsoft.com/ado/2007/08/dataservices">
Imports <xmlns:xm="http://schemas.microsoft.com/ado/2007/08/dataservices/metadata">
Imports System.Xml.Linq
Imports System.Windows.Media.Imaging
Imports System.Net

Partial Public Class MainWindow
    Inherits Window

    Dim query As String = "http://odata.netflix.com/Catalog/Titles?$filter=ReleaseYear eq {0}&$skip={1}&$top={2}&$select=Url,BoxArt"

    Class Movie
        Public Property Title As String
        Public Property Url As String
        Public Property BoxArtUrl As String
    End Class


    Public Sub New()
        InitializeComponent()
        textBox.Focus()
    End Sub

    Private Sub searchButton_Click(sender As Object, e As RoutedEventArgs) Handles searchButton.Click
        LoadMovies(Int32.Parse(textBox.Text))
    End Sub

    Private Sub textBox1_TextChanged(sender As Object, e As TextChangedEventArgs) Handles textBox.TextChanged
        Dim year = 0
        searchButton.IsEnabled = Int32.TryParse(textBox.Text, year) AndAlso year >= 1900 AndAlso year <= 2099
    End Sub

    Private Sub cancelButton_Click(sender As Object, e As RoutedEventArgs) Handles cancelButton.Click
        MessageBox.Show("Cancellation not yet implemented")
    End Sub

    Sub LoadMovies(year As Integer)
        resultsPanel.Children.Clear()
        statusText.Text = ""
        Dim pageSize = 10, imageCount = 0
        While True
            statusText.Text = String.Format("Searching... {0} Titles", imageCount)
            ' TODO: once code below has been made async, then statusText will work properly
            ' (it doesn't now because the UI never gets a chance to update)
            Dim movies = QueryMovies(year, imageCount, pageSize)
            If movies.Length = 0 Then Exit While
            DisplayMovies(movies)
            imageCount += movies.Length
        End While
        statusText.Text = String.Format("{0} Titles", imageCount)
    End Sub

    Function QueryMovies(year As Integer,
                         first As Integer,
                         count As Integer) As Movie()
        Dim client As New WebClient
        Dim url = String.Format(query, year, first, count)

        ' TODO: make following code async. It's non-responsive due to the synchronous call to WebClient.DownloadString.
        ' To fix it, follow the async walkthrough: http://go.microsoft.com/fwlink/?LinkId=203988
        Dim data As String = client.DownloadString(New Uri(url))

        Dim movies = From entry In XDocument.Parse(data)...<xa:entry>
                     Let properties = entry.<xm:properties>
                     Select New Movie With {
                            .Title = entry.<xa:title>.Value,
                            .Url = properties.<xd:Url>.Value,
                            .BoxArtUrl = properties.<xd:BoxArt>.<xd:LargeUrl>.Value}

        Return movies.ToArray
    End Function

    Sub DisplayMovies(movies As Movie())
        For Each Movie In movies
            Dim image As New Image With
            {
                .Source = New BitmapImage(New Uri(Movie.BoxArtUrl)),
                .Width = 110,
                .Height = 150,
                .Margin = New Thickness(5)
            }
            Dim url = Movie.Url
            AddHandler image.MouseDown, Sub() System.Diagnostics.Process.Start(url)
            resultsPanel.Children.Add(image)
        Next
    End Sub

End Class
