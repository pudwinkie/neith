Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Net
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Xml.Linq
Imports Concert_Finder.My.Resources
Imports Concert_Finder.ConcertFinder.Model

Namespace ConcertFinder.Providers

    ''' <summary>
    ''' Eventful event data provider.
    ''' </summary>
    Public Class EventfulProvider
        Implements IProvider

        ''' <summary>
        ''' Search for artists and venues based on the given query.
        ''' </summary>
        ''' <param name="query">The search query.</param>
        ''' <param name="cancellationToken">The cancellation token.</param>
        Public Function SearchAsync(query As String, cancellationToken As CancellationToken) As Task(Of SearchResults()) Implements IProvider.SearchAsync
            Dim artistsUri = New Uri([String].Format("http://api.eventful.com/rest/performers/search?keywords={0}&page_size=100&app_key={1}", query, Configuration.EventfulAppID))
            Dim venuesUri = New Uri([String].Format("http://api.eventful.com/rest/venues/search?keywords={0}&page_size=100&app_key={1}", query, Configuration.EventfulAppID))
            Dim artists = DownloadAndParse(artistsUri, query, GetType(Model.Artist), cancellationToken)
            Dim venues = DownloadAndParse(venuesUri, query, GetType(Model.Venue), cancellationToken)
            Return TaskEx.WhenAll(artists, venues)
        End Function

        ''' <summary>Downloads and parses the provided URI.</summary>
        ''' <param name="uri">The URI to download for results.</param>
        ''' <param name="query">The search query.</param>
        ''' <param name="searchType">The search model type.</param>
        ''' <param name="cancellationToken">The cancellation token.</param>
        Private Async Function DownloadAndParse(
             uri As Uri, query As String, searchType As Type, cancellationToken As CancellationToken) As Task(Of SearchResults)
            Try
                Dim content = Await New WebClient().DownloadStringTaskAsync(uri, cancellationToken)
                Dim results = ParseSearchResults(searchType, XElement.Parse(content))
                Return New SearchResults(results, searchType, query)
            Catch
                Return New SearchResults(New List(Of ISearchable)(), searchType, query)
            End Try
        End Function

        ''' <summary>
        ''' Returns a list of events for the artist with the given id.
        ''' </summary>
        ''' <param name="Id">The artist id.</param>
        Public Async Function GetArtistEventsAsync(Id As String) As Task(Of EventsAvailable) Implements IProvider.GetArtistEventsAsync
            Dim Uri = New Uri([String].Format("http://api.eventful.com/rest/events/search?keywords=spid:{0}&page_size=100&category=music&sort_order=date&app_key={1}", Id, Configuration.EventfulAppID))
            Try
                Dim parsed = XElement.Parse(Await New WebClient().DownloadStringTaskAsync(Uri))
                Dim events = From e In parsed.<events>.<event>
                             Let _City = e.<city_name>.Value
                             Let _Region = e.<region_name>.Value
                             Let _Country = e.<country_name>.Value
                             Let _StartTime = If(Not [String].IsNullOrEmpty(e.<start_time>.Value), CDate(e.<start_time>.Value), Nothing)
                             Let _EndTime = If(Not [String].IsNullOrEmpty(e.<stop_time>.Value), CDate(e.<stop_time>.Value), Nothing)
                             Where Not (_StartTime.AddDays(1).CompareTo(_EndTime) < 0)
                             Select New Model.[Event]() With
                                    {
                                        .Id = e.@id,
                                        .Name = e.<title>.Value,
                                        .Location = [String].Format("{0}, {1}", If(_Country.ToLower().Equals("united states"), _Region, _City), _Country),
                                        .Venue = New Venue() With
                                                 {
                                                     .Id = e.<venue_id>.Value,
                                                     .Name = e.<venue_name>.Value,
                                                     .Address = New Address() With
                                                                {
                                                                    .Street = e.<venue_address>.Value,
                                                                    .City = e.<city_name>.Value,
                                                                    .Region = e.Element("region_name").Value,
                                                                    .Country = e.<country_name>.Value,
                                                                    .Latitude = If(Not [String].IsNullOrEmpty(e.<latitude>.Value), CDbl(e.<latitude>.Value), Nothing),
                                                                    .Longitude = If(Not [String].IsNullOrEmpty(e.<longitude>.Value), CDbl(e.Element("longitude").Value), Nothing)
                                                                },
                                                     .Uri = If(Not [String].IsNullOrEmpty(e.<venue_url>.Value), New Uri(e.<venue_url>.Value), Nothing)
                                                },
                                        .StartTime = _StartTime,
                                        .EndTime = _EndTime,
                                        .IsSingleDay = _EndTime - _StartTime <= TimeSpan.FromDays(1),
                                        .IsTimeSpecified = e.<all_day>.Value.Equals("0"),
                                        .Artists = (From a In e.<performers>.<performer>
                                                    Select New Artist() With
                                                           {
                                                               .Id = a.<id>.Value,
                                                               .Name = a.<name>.Value,
                                                               .Uri = New Uri(a.<url>.Value)
                                                           }).ToList(),
                                         .Description = e.<description>.Value,
                                         .Image = If(Not [String].IsNullOrEmpty(e.<image>.<url>.Value) AndAlso Not e.<image>.<url>.Value.EndsWith("gif"), New Uri(e.<image>.<url>.Value), Nothing),
                                         .Uri = New Uri(e.<url>.Value)
                                    }

                Return New EventsAvailable(events.ToList(), EventGroup.Artist, Id)
            Catch ex As Exception
                Return New EventsAvailable(New List(Of Model.Event)(), EventGroup.Artist, Id)
            End Try
        End Function

        ''' <summary>
        ''' Returns a list of events for the venue with the given id.
        ''' </summary>
        ''' <param name="Id">The venue id.</param>
        Public Async Function GetVenueEventsAsync(Id As String) As Task(Of EventsAvailable) Implements IProvider.GetVenueEventsAsync
            Dim Uri = New Uri([String].Format("http://api.eventful.com/rest/events/search?location={0}&page_size=100&category=music&sort_order=date&app_key={1}", Id, Configuration.EventfulAppID))
            Try
                Dim parsed = XElement.Parse(Await New WebClient().DownloadStringTaskAsync(Uri))
                Dim events = From e In parsed.<events>.<event>
                             Let _City = e.<city_name>.Value
                             Let _Region = e.<region_name>.Value
                             Let _Country = e.<country_name>.Value
                             Let _StartTime = If(Not [String].IsNullOrEmpty(e.<start_time>.Value), CDate(e.<start_time>.Value), Nothing)
                             Let _EndTime = If(Not [String].IsNullOrEmpty(e.<stop_time>.Value), CDate(e.<stop_time>.Value), Nothing)
                             Where Not (_StartTime.AddDays(1).CompareTo(_EndTime) < 0)
                             Select New Model.[Event]() With
                                    {
                                        .Id = e.@id,
                                        .Name = e.<title>.Value,
                                        .Location = e.<venue_name>.Value,
                                        .Venue = New Venue() With
                                                 {
                                                     .Id = e.<venue_id>.Value,
                                                     .Name = e.<venue_name>.Value,
                                                     .Address = New Address() With
                                                                {
                                                                    .Street = e.<venue_address>.Value,
                                                                    .City = e.<city_name>.Value,
                                                                    .Region = e.Element("region_name").Value,
                                                                    .Country = e.<country_name>.Value,
                                                                    .Latitude = If(Not [String].IsNullOrEmpty(e.<latitude>.Value), CDbl(e.<latitude>.Value), Nothing),
                                                                    .Longitude = If(Not [String].IsNullOrEmpty(e.<longitude>.Value), CDbl(e.Element("longitude").Value), Nothing)
                                                                },
                                                     .Uri = If(Not [String].IsNullOrEmpty(e.<venue_url>.Value), New Uri(e.<venue_url>.Value), Nothing)
                                                },
                                        .StartTime = _StartTime,
                                        .EndTime = _EndTime,
                                        .IsSingleDay = _EndTime - _StartTime <= TimeSpan.FromDays(1),
                                        .IsTimeSpecified = e.<all_day>.Value.Equals("0"),
                                        .Artists = (From a In e.<performers>.<performer>
                                                    Select New Artist() With
                                                           {
                                                               .Id = a.<id>.Value,
                                                               .Name = a.<name>.Value,
                                                               .Uri = New Uri(a.<url>.Value)
                                                           }).ToList(),
                                         .Description = e.<description>.Value,
                                         .Image = If(Not [String].IsNullOrEmpty(e.<image>.<url>.Value) AndAlso Not e.<image>.<url>.Value.EndsWith("gif"), New Uri(e.<image>.<url>.Value), Nothing),
                                         .Uri = New Uri(e.<url>.Value)
                                    }

                Return New EventsAvailable(events.ToList(), EventGroup.Venue, Id)
            Catch ex As Exception
                Return New EventsAvailable(New List(Of Model.Event)(), EventGroup.Venue, Id)
            End Try
        End Function

        ''' <summary>
        ''' Parse the given search response XML based on the given type.
        ''' </summary>
        ''' <param name="Type">The type of the search results.</param>
        ''' <param name="Xml">The search response XML.</param>
        ''' <returns>A list of ISearchable.</returns>
        Private Function ParseSearchResults([Type] As [Type], Xml As XElement) As List(Of ISearchable)
            If [Type].[Equals](GetType(Artist)) Then
                Dim Artists = From e In Xml.<performers>.<performer>
                              Select TryCast(New Artist() With
                                             {
                                                 .Id = e.<id>.Value,
                                                 .Name = e.<name>.Value,
                                                 .Image = If(Not String.IsNullOrEmpty(e.<image>...<url>.Value), New Uri(e.<image>...<url>.Value), Nothing),
                                                 .Uri = New Uri(e.<url>.Value)
                                             }, ISearchable)
                Return Enumerable.ToList(Of ISearchable)(Artists)
            ElseIf [Type].[Equals](GetType(Venue)) Then
                Dim Venues = From e In Xml.<venues>.<venue>
                            Select TryCast(New Venue() With
                                            {
                                                .Id = e.@id,
                                                .Name = e.<venue_name>.Value,
                                                .Image = If(Not String.IsNullOrEmpty(e.<image>.<url>.Value) AndAlso Not e.<image>.<url>.Value.EndsWith("gif"), New Uri(e.<image>.<url>.Value), Nothing),
                                                .Uri = New Uri(e.<url>.Value)
                                            }, ISearchable)
                Return Enumerable.ToList(Of ISearchable)(Venues)
            Else
                Return New List(Of ISearchable)()
            End If
        End Function

    End Class

End Namespace