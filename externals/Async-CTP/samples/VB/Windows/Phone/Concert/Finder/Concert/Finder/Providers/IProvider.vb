Imports System
Imports System.Collections.Generic
Imports System.Threading
Imports Concert_Finder.ConcertFinder.Model
Imports System.Threading.Tasks

Namespace ConcertFinder.Providers

    ''' <summary>
    ''' The interface for event data providers.
    ''' </summary>
    Public Interface IProvider

        ''' <summary>
        ''' Search for artists and venues based on the given query.
        ''' </summary>
        ''' <param name="Query">The search query.</param>
        ''' <param name="Token">The cancellation token.</param>
        Function SearchAsync(Query As String, Token As CancellationToken) As Task(Of SearchResults())

        ''' <summary>
        ''' Returns a list of events for the artist with the given id.
        ''' </summary>
        ''' <param name="Id">The artist id.</param>
        Function GetArtistEventsAsync(Id As String) As Task(Of EventsAvailable)

        ''' <summary>
        ''' Returns a list of events for the venue with the given id.
        ''' </summary>
        ''' <param name="Id">The venue id.</param>
        Function GetVenueEventsAsync(Id As String) As Task(Of EventsAvailable)
    End Interface

    ''' <summary>
    ''' Event args for the SearchResultsAvailable event.
    ''' </summary>
    Public Class SearchResults
        Public Property Results As List(Of ISearchable)
        Public Property [Type] As [Type]
        Public Property Query As String

        Public Sub New(results As List(Of ISearchable), [type] As [Type], query As String)
            Me.Results = results
            Me.[Type] = [type]
            Me.Query = query
        End Sub
    End Class

    ''' <summary>
    ''' Event args for the EventsAvailable event.
    ''' </summary>
    Public Class EventsAvailable
        Public Property Events As List(Of [Event])
        Public Property EventGroup As EventGroup
        Public Property State As Object

        Public Sub New(events As List(Of [Event]), eventGroup As EventGroup, Optional state As Object = Nothing)
            Me.Events = events
            Me.EventGroup = eventGroup
            Me.State = state
        End Sub
    End Class
End Namespace