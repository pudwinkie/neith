using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Xml.Serialization;

namespace ConcertFinder.Model
{
    /// <summary>
    /// Represents an event.
    /// </summary>
    public class Event : ISearchable, IEquatable<Event>
    {
        /// <summary>
        /// The ID of the event.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name of the event.
        /// </summary>
        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = HttpUtility.HtmlDecode(value);
            }
        }
        private string _Name;

        /// <summary>
        /// The location of the event used for databinding.
        /// </summary>
        public string Location
        {
            get { return _Location; }
            set
            {
                _Location = HttpUtility.HtmlDecode(value);
            }
        }
        private string _Location;

        /// <summary>
        /// The venue of the event.
        /// </summary>
        public Venue Venue { get; set; }

        /// <summary>
        /// The start time of the event.
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// The end time of the event.
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Whether the start and end times fall within a day.
        /// </summary>
        public bool IsSingleDay { get; set; }

        /// <summary>
        /// Whether the start and end dates contain time components.
        /// </summary>
        public bool IsTimeSpecified { get; set; }

        /// <summary>
        /// The list of artists performing at the event.
        /// </summary>
        public List<Artist> Artists { get; set; }
        public bool IsListEmpty { get { return Artists.Count == 0; } }

        /// <summary>
        /// The description of the event.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The image for the event.
        /// </summary>
        [XmlIgnore]
        public Uri Image
        {
            get { return !String.IsNullOrEmpty(_Image) ? new Uri(_Image) : null; }
            set { _Image = value != null ? value.OriginalString : null; }
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string _Image { get; set; }

        /// <summary>
        /// The Uri of the event.
        /// </summary>
        [XmlIgnore]
        public Uri Uri
        {
            get { return !String.IsNullOrEmpty(_Uri) ? new Uri(_Uri) : null; }
            set { _Uri = value != null ? value.OriginalString : null; }
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string _Uri { get; set; }

        #region IEquatable

        /// <summary>
        /// Override for Equals.
        /// </summary>
        public bool Equals(Event Event)
        {
            var other = Event as Event;

            return other != null && other.Id.Equals(this.Id);
        }

        #endregion
    }

    /// <summary>
    /// An IComparer that compares start times.
    /// </summary>
    public class StartTimeComparer : IComparer<Event>
    {
        public int Compare(Event x, Event y)
        {
            var result = 0;

            if (x.StartTime.HasValue && y.StartTime.HasValue)
            {
                result = x.StartTime.Value.CompareTo(y.StartTime.Value);
            }

            return result;
        }
    }

    /// <summary>
    /// An enumeration of event groups.
    /// </summary>
    public enum EventGroup
    {
        Artist = 0,
        Venue = 1
    }
}
