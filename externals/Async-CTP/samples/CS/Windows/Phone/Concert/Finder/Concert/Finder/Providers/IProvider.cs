using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConcertFinder.Model;

namespace ConcertFinder.Providers
{
    /// <summary>The interface for event data providers.</summary>
    public interface IProvider
    {
        /// <summary>Asynchronously searches for artists and venues based on the given query.</summary>
        /// <param name="query">The search query.</param>
        /// <param name="token">The cancellation token.</param>
        Task<SearchResults[]> SearchAsync(string query, CancellationToken token);

        /// <summary>Asynchronously returns a list of events for the artist with the given id.</summary>
        /// <param name="id">The artist id.</param>
        Task<EventsAvailable> GetArtistEventsAsync(string id);

        /// <summary>Asynchronously a list of events for the venue with the given id.</summary>
        /// <param name="id">The venue id.</param>
        Task<EventsAvailable> GetVenueEventsAsync(string id);
    }

    /// <summary>Search results holder.</summary>
    public class SearchResults
    {
        public List<ISearchable> Results { get; private set; }
        public Type Type { get; private set; }
        public string Query { get; private set; }

        public SearchResults(List<ISearchable> results, Type type, string query)
        {
            this.Results = results;
            this.Type = type;
            this.Query = query;
        }
    }

    /// <summary>Events available holder.</summary>
    public class EventsAvailable
    {
        public List<Event> Events { get; private set; }
        public EventGroup EventGroup { get; private set; }
        public object State { get; private set; }

        public EventsAvailable(List<Event> events, EventGroup eventGroup, object state)
        {
            this.Events = events;
            this.EventGroup = eventGroup;
            this.State = state;
        }
    }
}