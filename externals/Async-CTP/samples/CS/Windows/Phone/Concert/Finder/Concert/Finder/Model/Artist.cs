using System;
using System.ComponentModel;
using System.Net;
using System.Xml.Serialization;

namespace ConcertFinder.Model
{
    /// <summary>
    /// Represents an artist.
    /// </summary>
    public class Artist : ISearchable, IEquatable<Artist>
    {
        /// <summary>
        /// The ID of the artist.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name of the artist.
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
        /// The image for the artist.
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
        /// The Uri of the artist.
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
        public bool Equals(Artist Artist)
        {
            var other = Artist as Artist;

            return other != null && other.Id.Equals(this.Id);
        }

        #endregion
    }
}
