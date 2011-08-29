using System;
using System.Globalization;
using System.Net;

namespace ConcertFinder.Model
{
    /// <summary>
    /// Represents an address.
    /// </summary>
    public class Address
    {
        /// <summary>
        /// The street component of the address.
        /// </summary>
        public string Street
        {
            get { return _Street; }
            set
            {
                _Street = HttpUtility.HtmlDecode(value);
            }
        }
        private string _Street;

        /// <summary>
        /// The city component of the address.
        /// </summary>
        public string City
        {
            get { return _City; }
            set
            {
                _City = HttpUtility.HtmlDecode(value);
            }
        }
        private string _City;
        public bool IsCityEmpty { get { return String.IsNullOrEmpty(City); } }

        /// <summary>
        /// The region component of the address.
        /// </summary>
        public string Region
        {
            get { return _Region; }
            set
            {
                _Region = HttpUtility.HtmlDecode(value);
            }
        }
        private string _Region;
        public bool IsRegionEmpty { get { return String.IsNullOrEmpty(Region); } }

        /// <summary>
        /// The country component of the address.
        /// </summary>
        public string Country
        {
            get { return _Country; }
            set
            {
                _Country = HttpUtility.HtmlDecode(value);
            }
        }
        private string _Country;

        /// <summary>
        /// The latitude coordinate of the venue.
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// The longitude coordinate of the venue.
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// Convert an address to a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Latitude.HasValue && Longitude.HasValue ?
                
                String.Format(
                "{0},{1}",
                Latitude.Value.ToString(CultureInfo.InvariantCulture.NumberFormat),
                Longitude.Value.ToString(CultureInfo.InvariantCulture.NumberFormat)) :

                String.Format(
                "{0} {1} {2} {3}",
                Street,
                City,
                Region,
                Country);
        }
    }
}
