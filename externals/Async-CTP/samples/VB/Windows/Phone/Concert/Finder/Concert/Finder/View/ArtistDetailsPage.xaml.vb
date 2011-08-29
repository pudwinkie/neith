Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Windows.Controls
Imports System.Windows.Navigation
Imports Microsoft.Phone.Controls
Imports Concert_Finder.ConcertFinder.ViewModel

Namespace ConcertFinder.View

    Partial Public Class ArtistDetailsPage
        Inherits PhoneApplicationPage

        ''' <summary>
        ''' The view model for the page.
        ''' </summary>
        Private viewModel As ArtistDetailsPageViewModel

        ''' <summary>
        ''' Track whether the page has initialized.
        ''' </summary>
        Private isInitialized As Boolean = False

        ''' <summary>
        ''' The default constructor.
        ''' </summary>
        Public Sub New()
            InitializeComponent()
            viewModel = New ArtistDetailsPageViewModel()
            DataContext = viewModel
        End Sub

        ''' <summary>
        ''' Override for OnNavigatedTo.
        ''' </summary>
        Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)
            If Not isInitialized Then
                If State.Count > 0 Then
                    viewModel.Initialize(TryCast(State("Artist"), Model.Artist), TryCast(State("Events"), ObservableCollection(Of Model.[Event])))
                Else
                    viewModel.Initialize(App.SelectedArtist)
                End If
                isInitialized = True
            End If
            App.SelectedArtist = TryCast(viewModel.SearchResult, Model.Artist)
        End Sub

        ''' <summary>
        ''' Override for OnNavigatedFrom.
        ''' </summary>
        Protected Overrides Sub OnNavigatedFrom(e As NavigationEventArgs)
            State("Artist") = TryCast(viewModel.SearchResult, Model.Artist)
            State("Events") = viewModel.Events
        End Sub

        ''' <summary>
        ''' Reset the selected artist when the back key is pressed.
        ''' </summary>
        Protected Overrides Sub OnBackKeyPress(e As CancelEventArgs)
            App.SelectedArtist = Nothing
        End Sub

    End Class

End Namespace