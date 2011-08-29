Imports Concert_Finder.ConcertFinder.Model
Imports Concert_Finder.ConcertFinder
Imports Concert_Finder.ConcertFinder.Providers
Imports System.Threading
Imports Microsoft.Phone.Reactive

Partial Public Class App
    Inherits Application

    ''' <summary>
    ''' Provides easy access to the root frame of the Phone Application.
    ''' </summary>
    ''' <returns>The root frame of the Phone Application.</returns>
    Public Property RootFrame As PhoneApplicationFrame

    ''' <summary>
    ''' The active search query.
    ''' </summary>
    Public Shared Property ActiveSearchQuery As String
        Get
            Return _ActiveSearchQuery
        End Get
        Set(value As String)
            _ActiveSearchQuery = value
            PhoneApplicationService.Current.State("ActiveSearchQuery") = _ActiveSearchQuery
        End Set
    End Property
    Private Shared _ActiveSearchQuery As String

    ''' <summary>
    ''' The cancellation token source for searches.
    ''' </summary>
    Private Shared SearchCancellationTokenSource As CancellationTokenSource

    ''' <summary>
    ''' The artist which the user has selected from the search page.
    ''' </summary>
    Public Shared Property SelectedArtist As Model.Artist
        Get
            Return _SelectedArtist
        End Get
        Set(value As Model.Artist)
            _SelectedArtist = value
            PhoneApplicationService.Current.State("SelectedArtist") = _SelectedArtist
        End Set
    End Property
    Private Shared _SelectedArtist As Model.Artist

    ''' <summary>
    ''' The venue which the user has selected from the search page.
    ''' </summary>
    Public Shared Property SelectedVenue As Model.Venue
        Get
            Return _SelectedVenue
        End Get
        Set(value As Model.Venue)
            _SelectedVenue = value
            PhoneApplicationService.Current.State("SelectedVenue") = _SelectedVenue
        End Set
    End Property
    Public Shared _SelectedVenue As Model.Venue

    ''' <summary>
    ''' The collection of events.
    ''' </summary>
    Public Shared Events As Dictionary(Of EventGroup, List(Of Model.[Event])) = New Dictionary(Of EventGroup, List(Of Model.[Event]))()

    ''' <summary>
    ''' The data provider.
    ''' </summary>
    Private Shared Provider As IProvider = New EventfulProvider()

    ''' <summary>
    ''' Constructor for the Application object.
    ''' </summary>
    Public Sub New()
        ' Show graphics profiling information while debugging.
        If Diagnostics.Debugger.IsAttached Then
            ' Display the current frame rate counters.
            Application.Current.Host.Settings.EnableFrameRateCounter = True

            ' Show the areas of the app that are being redrawn in each frame.
            'Application.Current.Host.Settings.EnableRedrawRegions = True

            ' Enable non-production analysis visualization mode, 
            ' which shows areas of a page that are being GPU accelerated with a colored overlay.
            'Application.Current.Host.Settings.EnableCacheVisualization = True
        End If

        ' Standard Silverlight initialization
        InitializeComponent()

        ' Phone-specific initialization
        InitializePhoneApplication()
    End Sub

    ' Code to execute when the application is launching (eg, from Start)
    ' This code will not execute when the application is reactivated
    Private Sub Application_Launching(sender As Object, e As LaunchingEventArgs)
    End Sub

    ' Code to execute when the application is activated (brought to foreground)
    ' This code will not execute when the application is first launched
    Private Sub Application_Activated(sender As Object, e As ActivatedEventArgs)
        ActiveSearchQuery = If(PhoneApplicationService.Current.State.ContainsKey("ActiveSearchQuery"), TryCast(PhoneApplicationService.Current.State("ActiveSearchQuery"), String), Nothing)
        SelectedArtist = If(PhoneApplicationService.Current.State.ContainsKey("SelectedArtist"), TryCast(PhoneApplicationService.Current.State("SelectedArtist"), Model.Artist), Nothing)
        SelectedVenue = If(PhoneApplicationService.Current.State.ContainsKey("SelectedVenue"), TryCast(PhoneApplicationService.Current.State("SelectedVenue"), Model.Venue), Nothing)
    End Sub

    ''' <summary>
    ''' Perform a search based on the user's query.
    ''' </summary>
    ''' <param name="Query">The search query.</param>
    Public Shared Async Sub Search(Query As String)
        ActiveSearchQuery = Query

        If SearchCancellationTokenSource IsNot Nothing Then SearchCancellationTokenSource.Cancel()
        SearchCancellationTokenSource = New CancellationTokenSource()

        Dim searchResults As New List(Of ISearchable)
        Try
            Dim search = Await Provider.SearchAsync(Query, SearchCancellationTokenSource.Token)
            searchResults.AddRange(search.SelectMany(Function(r) r.Results))
        Catch
        End Try
        If Query <> ActiveSearchQuery Then Return
        RaiseSearchResultsAvailable(searchResults)
    End Sub

    ''' <summary>
    ''' Get the events for the given EventGroup.
    ''' </summary>
    ''' <param name="EventGroup">The EventGroup of events.</param>
    ''' <param name="Id">An ID used in the search.</param>
    Public Shared Async Sub GetEvents(EventGroup As EventGroup, Id As String)
        Dim results As EventsAvailable = Await If(EventGroup = EventGroup.Artist,
                Provider.GetArtistEventsAsync(Id), Provider.GetVenueEventsAsync(Id))
        Events(EventGroup) = results.Events
        RaiseEventsAvailable(EventGroup, results.State)
    End Sub

    ''' <summary>
    ''' The SearchResultsAvailable event.
    ''' </summary>
    Public Shared Event SearchResultsAvailable As EventHandler(Of SearchResultsAvailableEventArgs)

    ''' <summary>
    ''' Raises the SearchResultsAvailable event.
    ''' </summary>
    ''' <param name = "SearchResults"></param>
    Private Shared Sub RaiseSearchResultsAvailable(SearchResults As List(Of ISearchable))
        RaiseEvent SearchResultsAvailable(Nothing, New SearchResultsAvailableEventArgs(SearchResults))
    End Sub

    ''' <summary>
    ''' The EventsAvailable event.
    ''' </summary>
    Public Shared Event EventsAvailable As EventHandler(Of EventsAvailableEventArgs)

    ''' <summary>
    ''' Raises the EventsAvailable event for the given EventGroup.
    ''' </summary>
    Private Shared Sub RaiseEventsAvailable(EventGroup As EventGroup, Optional State As Object = Nothing)
        RaiseEvent EventsAvailable(Nothing, New EventsAvailableEventArgs(EventGroup, State))
    End Sub

    ' Code to execute when the application is deactivated (sent to background)
    ' This code will not execute when the application is closing
    Private Sub Application_Deactivated(sender As Object, e As DeactivatedEventArgs)
    End Sub

    ' Code to execute when the application is closing (eg, user hit Back)
    ' This code will not execute when the application is deactivated
    Private Sub Application_Closing(sender As Object, e As ClosingEventArgs)
    End Sub

    ' Code to execute if a navigation fails
    Private Sub RootFrame_NavigationFailed(sender As Object, e As NavigationFailedEventArgs)
        If Diagnostics.Debugger.IsAttached Then
            ' A navigation has failed; break into the debugger
            Diagnostics.Debugger.Break()
        End If
    End Sub

    Public Sub Application_UnhandledException(sender As Object, e As ApplicationUnhandledExceptionEventArgs) Handles Me.UnhandledException

        ' Show graphics profiling information while debugging.
        If Diagnostics.Debugger.IsAttached Then
            Diagnostics.Debugger.Break()
        Else
            e.Handled = True
            MessageBox.Show(e.ExceptionObject.Message & Environment.NewLine & e.ExceptionObject.StackTrace,
                            "Error", MessageBoxButton.OK)
        End If
    End Sub

#Region "Phone application initialization"
    ' Avoid double-initialization
    Private phoneApplicationInitialized As Boolean = False

    ' Do not add any additional code to this method
    Private Sub InitializePhoneApplication()
        If phoneApplicationInitialized Then
            Return
        End If

        ' Create the frame but don't set it as RootVisual yet; this allows the splash
        ' screen to remain active until the application is ready to render.
        RootFrame = New PhoneApplicationFrame()
        AddHandler RootFrame.Navigated, AddressOf CompleteInitializePhoneApplication

        ' Handle navigation failures
        AddHandler RootFrame.NavigationFailed, AddressOf RootFrame_NavigationFailed

        ' Ensure we don't initialize again
        phoneApplicationInitialized = True
    End Sub

    ' Do not add any additional code to this method
    Private Sub CompleteInitializePhoneApplication(sender As Object, e As NavigationEventArgs)
        ' Set the root visual to allow the application to render
        If RootVisual IsNot RootFrame Then
            RootVisual = RootFrame
        End If

        ' Remove this handler since it is no longer needed
        RemoveHandler RootFrame.Navigated, AddressOf CompleteInitializePhoneApplication
    End Sub
#End Region

End Class

''' <summary>
''' Event args for the SearchResultsAvailable event.
''' </summary>
Public Class SearchResultsAvailableEventArgs

    Inherits EventArgs

    Public Property SearchResults As List(Of ISearchable)

    Public Sub New(SearchResults As List(Of ISearchable))
        Me.SearchResults = SearchResults
    End Sub

End Class

''' <summary>
''' Event args for the EventsAvailable event.
''' </summary>
Public Class EventsAvailableEventArgs

    Inherits EventArgs

    Public Property EventGroup As EventGroup

    Public Property State As Object

    Public Sub New(EventGroup As EventGroup, Optional State As Object = Nothing)
        Me.EventGroup = EventGroup
        Me.State = State
    End Sub

End Class