using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ConcertFinder.Model
{
    /// <summary>
    /// Represents a search result.
    /// </summary>
    public interface ISearchable
    {
        /// <summary>
        /// The ID of the search result.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// The name of the search result.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The image for the search result.
        /// </summary>
        [XmlIgnore]
        Uri Image { get; set; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        string _Image { get; set; }

        /// <summary>
        /// The Uri of the search result.
        /// </summary>
        [XmlIgnore]
        Uri Uri { get; set; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        string _Uri { get; set; }
    }
}
