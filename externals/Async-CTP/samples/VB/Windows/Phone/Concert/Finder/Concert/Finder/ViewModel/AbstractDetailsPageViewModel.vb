Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Windows
Imports Microsoft.Phone.Reactive
Imports Concert_Finder.ConcertFinder.Model

Namespace ConcertFinder.ViewModel

    ''' <summary>
    ''' The base view model for the ArtistDetailsPage and the VenueDetailsPage.
    ''' </summary>
    Public MustInherit Class AbstractDetailsPageViewModel
        Implements INotifyPropertyChanged

        ''' <summary>
        ''' The search result to display.
        ''' </summary>
        Public Property SearchResult As ISearchable
            Get
                Return _SearchResult
            End Get
            Set(value As ISearchable)
                _SearchResult = value
                NotifyPropertyChanged("SearchResult")
            End Set
        End Property
        Private _SearchResult As ISearchable

        ''' <summary>
        ''' The events to display.
        ''' </summary>
        Public Property Events As ObservableCollection(Of Model.[Event])

        Public ReadOnly Property IsListEmpty As Boolean
            Get
                Return Not SearchInProgress AndAlso Events.Count = 0
            End Get
        End Property

        ''' <summary>
        ''' The event group for the page.
        ''' </summary>
        Public Property EventGroup As EventGroup

        ''' <summary>
        ''' Gets or sets whether a search is in progress.
        ''' </summary>
        Public Property SearchInProgress As Boolean
            Get
                Return _SearchInProgress
            End Get
            Set(value As Boolean)
                _SearchInProgress = value
                NotifyPropertyChanged("SearchInProgress")
                NotifyPropertyChanged("IsListEmpty")
            End Set
        End Property
        Private _SearchInProgress As Boolean = False

        ''' <summary>
        ''' Initialize the page with a search result and the given events.
        ''' </summary>
        ''' <param name="SearchResult">The search result to display.</param>
        ''' <param name="Events">The events to display.</param>
        Public Sub Initialize(SearchResult As ISearchable, Events As ObservableCollection(Of Model.[Event]))
            Me.SearchResult = SearchResult
            Me.Events = Events
        End Sub

        ''' <summary>
        ''' Initialize the page with a search result.
        ''' </summary>
        ''' <param name="SearchResult">The search result to display.</param>
        Public Sub Initialize(SearchResult As ISearchable)
            Initialize(SearchResult, New ObservableCollection(Of Model.[Event])())
            GetEvents()
        End Sub

        ''' <summary>
        ''' Initialize the page with events.
        ''' </summary>
        Public Sub GetEvents()
            Observable.FromEvent(Of EventsAvailableEventArgs)(
                Sub(ev) AddHandler App.EventsAvailable, ev,
                Sub(ev) RemoveHandler App.EventsAvailable, ev).
            [Where](Function(args) args.EventArgs.EventGroup = EventGroup AndAlso (TryCast(args.EventArgs.State, String)).[Equals](SearchResult.Id)).
            [Take](1).
            Subscribe(
                Sub(args)
                    Deployment.Current.Dispatcher.BeginInvoke(
                        Sub()
                            Events.Clear()

                            For Each e In App.Events(args.EventArgs.EventGroup)
                                Events.Add(e)
                            Next

                            SearchInProgress = False
                        End Sub)
                End Sub
            )

            SearchInProgress = True

            App.GetEvents(EventGroup, SearchResult.Id)
        End Sub

        ''' <summary>
        ''' The PropertyChanged event.
        ''' </summary>
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        ''' <summary>
        ''' Notify listeners of changes to the property with the given name.
        ''' </summary>
        ''' <param name="propertyName">The name of the changed property.</param>
        Private Sub NotifyPropertyChanged(propertyName As String)
            RaiseEvent PropertyChanged(Nothing, New PropertyChangedEventArgs(propertyName))
        End Sub

    End Class

End Namespace