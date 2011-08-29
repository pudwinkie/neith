Imports Concert_Finder.ConcertFinder.Model
Imports Concert_Finder.ConcertFinder.ViewModel

Partial Public Class MainPage
    Inherits PhoneApplicationPage

    ''' <summary>
    ''' The view model for the page.
    ''' </summary>
    Private viewModel As MainPageViewModel

    ''' <summary>
    ''' The default constructor.
    ''' </summary>
    Public Sub New()
        InitializeComponent()

        viewModel = New MainPageViewModel()
        DataContext = viewModel
    End Sub

    ''' <summary>
    ''' Perform the search when the user presses Enter.
    ''' </summary>
    Private Sub SearchBox_KeyDown(sender As Object, e As KeyEventArgs)
        If (e.[Key] = [Key].Enter) AndAlso (SearchBox.[Text].Length >= 2) Then
            SearchResults.Focus()
            viewModel.Search(SearchBox.[Text])
        End If
    End Sub

    ''' <summary>
    ''' Select the text when the text box is entered.
    ''' </summary>
    Private Sub SearchBox_GotFocus(sender As Object, e As EventArgs)
        SearchBox.SelectAll()
    End Sub

    ''' <summary>
    ''' Selection changed handler for the event list.
    ''' </summary>
    Private Sub SearchResults_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If e.AddedItems.Count > 0 Then
            Dim AddedItem = e.AddedItems(0)
            If TryCast(AddedItem, Artist) IsNot Nothing Then
                App.SelectedArtist = TryCast(AddedItem, Artist)
                NavigationService.Navigate(New Uri("/View/ArtistDetailsPage.xaml", UriKind.Relative))
            ElseIf TryCast(AddedItem, Venue) IsNot Nothing Then
                App.SelectedVenue = TryCast(AddedItem, Venue)
                NavigationService.Navigate(New Uri("/View/VenueDetailsPage.xaml", UriKind.Relative))
            End If
        End If
        TryCast(sender, ListBox).SelectedItem = Nothing
    End Sub

    ''' <summary>
    ''' Override for OnNavigatedTo.
    ''' </summary>
    Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)
        SearchBox.[Text] = If(App.ActiveSearchQuery, [String].Empty)
        If SearchResults.Items.Count = 0 Then
            '' Queue the operation to ensure the text box does not lose focus.
            Deployment.Current.Dispatcher.BeginInvoke(
                Sub()
                    Deployment.Current.Dispatcher.BeginInvoke(
                        Sub()
                            SearchBox.Focus()
                        End Sub
                    )
                End Sub
            )
        End If
    End Sub

End Class
