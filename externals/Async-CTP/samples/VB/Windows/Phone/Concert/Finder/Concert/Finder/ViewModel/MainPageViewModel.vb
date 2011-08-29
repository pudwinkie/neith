Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Windows
Imports Microsoft.Phone.Reactive
Imports Concert_Finder.ConcertFinder.Model

Namespace ConcertFinder.ViewModel

    ''' <summary>
    ''' The view model for the MainPage.
    ''' </summary>
    Public Class MainPageViewModel
        Implements INotifyPropertyChanged

        ''' <summary>
        ''' The list of search results.
        ''' </summary>
        Public Property SearchResults As ObservableCollection(Of ISearchable)

        Public ReadOnly Property IsListEmpty As Boolean
            Get
                Return Not _SearchPending AndAlso Not SearchInProgress AndAlso SearchResults.Count = 0
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets whether a search is in progress.
        ''' </summary>
        Public Property SearchInProgress As Boolean
            Get
                Return _SearchInProgress
            End Get
            Set(value As Boolean)
                _SearchInProgress = value
                _SearchPending = False
                NotifyPropertyChanged("SearchInProgress")
                NotifyPropertyChanged("IsListEmpty")
            End Set
        End Property
        Private _SearchInProgress As Boolean = False
        Private _SearchPending As Boolean = True

        ''' <summary>
        ''' The default constructor.
        ''' </summary>
        Public Sub New()
            SearchResults = New ObservableCollection(Of ISearchable)()
        End Sub

        ''' <summary>
        ''' Perform a search based on the user's query.
        ''' </summary>
        ''' <param name="query">The search query.</param>
        Public Sub Search(query As String)
            Observable.FromEvent(Of SearchResultsAvailableEventArgs)(
                Sub(ev) AddHandler App.SearchResultsAvailable, ev,
                Sub(ev) RemoveHandler App.SearchResultsAvailable, ev).
            [Take](1).Subscribe(
                Sub(e)
                    Deployment.Current.Dispatcher.BeginInvoke(
                        Sub()
                            SearchResults.Clear()

                            For Each SearchResult In e.EventArgs.SearchResults
                                SearchResults.Add(SearchResult)
                            Next

                            SearchInProgress = False
                        End Sub)
                End Sub
            )

            SearchInProgress = True

            App.Search(query)
        End Sub

        ''' <summary>
        ''' The PropertyChanged event.
        ''' </summary>
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        ''' <summary>
        ''' Notify listeners of property changes.
        ''' </summary>
        ''' <param name="propertyName">The name of the changed property.</param>
        Private Sub NotifyPropertyChanged(propertyName As String)
            RaiseEvent PropertyChanged(Nothing, New PropertyChangedEventArgs(propertyName))
        End Sub

    End Class

End Namespace