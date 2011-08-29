using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using ConcertFinder.Model;
using ConcertFinder.Resources;

namespace ConcertFinder.Providers
{
    /// <summary>Eventful event data provider.</summary>
    public class EventfulProvider : IProvider
    {
        /// <summary>Search for artists and venues based on the given query.</summary>
        /// <param name="query">The search query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public Task<SearchResults[]> SearchAsync(string query, CancellationToken cancellationToken)
        {
            var artistsUri = new Uri(
                String.Format("http://api.eventful.com/rest/performers/search?keywords={0}&page_size=100&app_key={1}",
                query, Configuration.EventfulAppID));

            var venuesUri = new Uri(
                String.Format("http://api.eventful.com/rest/venues/search?keywords={0}&page_size=100&app_key={1}",
                query, Configuration.EventfulAppID));

            var artists = DownloadAndParse(artistsUri, query, typeof(Model.Artist), cancellationToken);
            var venues = DownloadAndParse(venuesUri, query, typeof(Model.Venue), cancellationToken);
            return TaskEx.WhenAll(artists, venues);
        }

        /// <summary>Downloads and parses the provided URI.</summary>
        /// <param name="uri">The URI to download for results.</param>
        /// <param name="query">The search query.</param>
        /// <param name="searchType">The search model type.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task<SearchResults> DownloadAndParse(
            Uri uri, string query, Type searchType, CancellationToken cancellationToken)
        {
            try
            {
                var content = await new WebClient().DownloadStringTaskAsync(uri, cancellationToken);
                var results = ParseSearchResults(searchType, XElement.Parse(content));
                return new SearchResults(results, searchType, query);
            }
            catch
            {
                return new SearchResults(new List<ISearchable>(), searchType, query);
            }
        }

        /// <summary>Returns a list of events for the artist with the given id.</summary>
        /// <param name="id">The artist id.</param>
        public async Task<EventsAvailable> GetArtistEventsAsync(string id)
        {
            var uri = new Uri(
               String.Format("http://api.eventful.com/rest/events/search?keywords=spid:{0}&page_size=100&category=music&sort_order=date&app_key={1}",
               id, Configuration.EventfulAppID));

            try
            {
                var parsed = XElement.Parse(await new WebClient().DownloadStringTaskAsync(uri));

                var events = from e in parsed.Element("events").Elements("event")
                             let _City = e.Element("city_name").Value
                             let _Region = e.Element("region_name").Value
                             let _Country = e.Element("country_name").Value
                             let _StartTime = !String.IsNullOrEmpty(e.Element("start_time").Value) ?
                                    (DateTime?)Convert.ToDateTime(e.Element("start_time").Value) : null
                             let _EndTime = !String.IsNullOrEmpty(e.Element("stop_time").Value) ?
                                    (DateTime?)Convert.ToDateTime(e.Element("stop_time").Value) : null
                             where !(_StartTime.HasValue && _EndTime.HasValue &&
                                     _StartTime.Value.AddDays(1).CompareTo(_EndTime.Value) < 0)
                             select new Model.Event()
                             {
                                 Id = e.Attribute("id").Value,
                                 Name = e.Element("title").Value,
                                 Location = String.Format("{0}, {1}", _Country.ToLower().Equals("united states") ? _Region : _City, _Country),
                                 Venue = new Venue()
                                 {
                                     Id = e.Element("venue_id").Value,
                                     Name = e.Element("venue_name").Value,
                                     Address = new Address()
                                     {
                                         Street = e.Element("venue_address").Value,
                                         City = e.Element("city_name").Value,
                                         Region = e.Element("region_name").Value,
                                         Country = e.Element("country_name").Value,
                                         Latitude = !String.IsNullOrEmpty(e.Element("latitude").Value) ?
                                            Convert.ToDouble(e.Element("latitude").Value, CultureInfo.InvariantCulture.NumberFormat) : (double?)null,
                                         Longitude = !String.IsNullOrEmpty(e.Element("longitude").Value) ?
                                            Convert.ToDouble(e.Element("longitude").Value, CultureInfo.InvariantCulture.NumberFormat) : (double?)null
                                     },
                                     Uri = e.Element("venue_url") != null &&
                                         !String.IsNullOrEmpty(e.Element("venue_url").Value) ?
                                         new Uri(e.Element("venue_url").Value) : null
                                 },
                                 StartTime = _StartTime,
                                 EndTime = _EndTime,
                                 IsSingleDay = !_EndTime.HasValue || _StartTime.HasValue &&
                                 _EndTime.Value - _StartTime.Value <= TimeSpan.FromDays(1),
                                 IsTimeSpecified = e.Element("all_day").Value.Equals("0"),
                                 Artists = (from a in e.Element("performers").Elements("performer")
                                            select new Artist()
                                            {
                                                Id = a.Element("id").Value,
                                                Name = a.Element("name").Value,
                                                Uri = new Uri(a.Element("url").Value)
                                            }).ToList<Artist>(),
                                 Description = e.Element("description").Value,
                                 Image = e.Element("image").Element("url") != null &&
                                    !String.IsNullOrEmpty(e.Element("image").Element("url").Value) &&
                                    !e.Element("image").Element("url").Value.EndsWith("gif") ?
                                    new Uri(e.Element("image").Element("url").Value) :
                                    null,
                                 Uri = new Uri(e.Element("url").Value)
                             };

                return new EventsAvailable(events.ToList(), EventGroup.Artist, id);
            }
            catch 
            {
                return new EventsAvailable(new List<Model.Event>(), EventGroup.Artist, id);
            }
        }

        /// <summary>
        /// Returns a list of events for the venue with the given id.
        /// </summary>
        /// <param name="id">The venue id.</param>
        public async Task<EventsAvailable> GetVenueEventsAsync(string id)
        {
            var uri = new Uri(
               String.Format("http://api.eventful.com/rest/events/search?location={0}&page_size=100&category=music&sort_order=date&app_key={1}",
               id, Configuration.EventfulAppID));

            try
            {
                var parsed = XElement.Parse(await new WebClient().DownloadStringTaskAsync(uri));

                var events = from e in parsed.Element("events").Elements("event")
                             let _City = e.Element("city_name").Value
                             let _Region = e.Element("region_name").Value
                             let _Country = e.Element("country_name").Value
                             let _StartTime = !String.IsNullOrEmpty(e.Element("start_time").Value) ?
                                    (DateTime?)Convert.ToDateTime(e.Element("start_time").Value) :
                                    null
                             let _EndTime = !String.IsNullOrEmpty(e.Element("stop_time").Value) ?
                                    (DateTime?)Convert.ToDateTime(e.Element("stop_time").Value) :
                                    null
                             where !(_StartTime.HasValue && _EndTime.HasValue &&
                                     _StartTime.Value.AddDays(1).CompareTo(_EndTime.Value) < 0)
                             select new Model.Event()
                             {
                                 Id = e.Attribute("id").Value,
                                 Name = e.Element("title").Value,
                                 Location = e.Element("venue_name").Value,
                                 Venue = new Venue()
                                 {
                                     Id = e.Element("venue_id").Value,
                                     Name = e.Element("venue_name").Value,
                                     Address = new Address()
                                     {
                                         Street = e.Element("venue_address").Value,
                                         City = e.Element("city_name").Value,
                                         Region = e.Element("region_name").Value,
                                         Country = e.Element("country_name").Value,
                                         Latitude = !String.IsNullOrEmpty(e.Element("latitude").Value) ?
                                            Convert.ToDouble(e.Element("latitude").Value, CultureInfo.InvariantCulture.NumberFormat) :
                                            (double?)null,
                                         Longitude = !String.IsNullOrEmpty(e.Element("longitude").Value) ?
                                            Convert.ToDouble(e.Element("longitude").Value, CultureInfo.InvariantCulture.NumberFormat) :
                                            (double?)null
                                     },
                                     Uri = e.Element("venue_url") != null &&
                                         !String.IsNullOrEmpty(e.Element("venue_url").Value) ?
                                         new Uri(e.Element("venue_url").Value) :
                                         null
                                 },
                                 StartTime = _StartTime,
                                 EndTime = _EndTime,
                                 IsSingleDay = !_EndTime.HasValue || _StartTime.HasValue &&
                                 _EndTime.Value - _StartTime.Value <= TimeSpan.FromDays(1),
                                 IsTimeSpecified = e.Element("all_day").Value.Equals("0"),
                                 Artists = (from a in e.Element("performers").Elements("performer")
                                            select new Artist()
                                            {
                                                Id = a.Element("id").Value,
                                                Name = a.Element("name").Value,
                                                Uri = new Uri(a.Element("url").Value)
                                            }).ToList<Artist>(),
                                 Description = e.Element("description").Value,
                                 Image = e.Element("image").Element("url") != null &&
                                    !String.IsNullOrEmpty(e.Element("image").Element("url").Value) &&
                                    !e.Element("image").Element("url").Value.EndsWith("gif") ?
                                    new Uri(e.Element("image").Element("url").Value) :
                                    null,
                                 Uri = new Uri(e.Element("url").Value)
                             };

                return new EventsAvailable(events.ToList<Model.Event>(), EventGroup.Venue, id);
            }
            catch 
            {
                return new EventsAvailable(new List<Model.Event>(), EventGroup.Venue, id);
            }
        }

        /// <summary>
        /// Parse the given search response XML based on the given type.
        /// </summary>
        /// <param name="Type">The type of the search results.</param>
        /// <param name="Xml">The search response XML.</param>
        /// <returns>A list of ISearchable.</returns>
        private List<ISearchable> ParseSearchResults(Type Type, XElement Xml)
        {
            if (Type.Equals(typeof(Artist)))
            {
                var Artists = from e in Xml.Element("performers").Elements("performer")
                              select new Artist()
                              {
                                  Id = e.Element("id").Value,
                                  Name = e.Element("name").Value,
                                  Image = e.Element("image").Descendants("url").Count() > 0 &&
                                     !String.IsNullOrEmpty(e.Element("image").Descendants("url").First().Value) ?
                                     new Uri(e.Element("image").Descendants("url").First().Value) :
                                     null,
                                  Uri = new Uri(e.Element("url").Value)
                              } as ISearchable;

                return Artists.ToList<ISearchable>();
            }
            else if (Type.Equals(typeof(Venue)))
            {
                var Venues = from e in Xml.Element("venues").Elements("venue")
                             select new Venue()
                             {
                                 Id = e.Attribute("id").Value,
                                 Name = e.Element("venue_name").Value,
                                 Image = e.Element("image").Element("url") != null &&
                                    !String.IsNullOrEmpty(e.Element("image").Element("url").Value) &&
                                    !e.Element("image").Element("url").Value.EndsWith("gif") ?
                                    new Uri(e.Element("image").Element("url").Value) :
                                    null,
                                 Uri = new Uri(e.Element("url").Value)
                             } as ISearchable;

                return Venues.ToList<ISearchable>();
            }
            else
            {
                return new List<ISearchable>();
            }
        }
    }
}