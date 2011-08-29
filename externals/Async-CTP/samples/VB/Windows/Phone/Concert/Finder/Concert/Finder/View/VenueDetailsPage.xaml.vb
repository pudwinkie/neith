Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Windows.Navigation
Imports Microsoft.Phone.Controls
Imports Concert_Finder.ConcertFinder.ViewModel

Namespace ConcertFinder.View

    Partial Public Class VenueDetailsPage
        Inherits PhoneApplicationPage

        ''' <summary>
        ''' The view model for the page.
        ''' </summary>
        Private viewModel As VenueDetailsPageViewModel

        ''' <summary>
        ''' Track whether the page has initialized.
        ''' </summary>
        Private isInitialized As Boolean = False

        ''' <summary>
        ''' The default constructor.
        ''' </summary>
        Public Sub New()
            InitializeComponent()
            viewModel = New VenueDetailsPageViewModel()
            DataContext = viewModel
        End Sub

        ''' <summary>
        ''' Override for OnNavigatedTo.
        ''' </summary>
        Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)
            If Not isInitialized Then
                If State.Count > 0 Then
                    viewModel.Initialize(TryCast(State("Venue"), Model.Venue), TryCast(State("Events"), ObservableCollection(Of Model.[Event])))
                Else
                    viewModel.Initialize(App.SelectedVenue)
                End If
                isInitialized = True
            End If
            App.SelectedVenue = TryCast(viewModel.SearchResult, Model.Venue)
        End Sub

        ''' <summary>
        ''' Override for OnNavigatedFrom.
        ''' </summary>
        Protected Overrides Sub OnNavigatedFrom(e As NavigationEventArgs)
            State("Venue") = TryCast(viewModel.SearchResult, Model.Venue)
            State("Events") = viewModel.Events
        End Sub

        ''' <summary>
        ''' Reset the selected venue when the back key is pressed.
        ''' </summary>
        Protected Overrides Sub OnBackKeyPress(e As CancelEventArgs)
            App.SelectedVenue = Nothing
        End Sub

    End Class

End Namespace