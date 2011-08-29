namespace ConcertFinder.Resources
{
    /// <summary>
    /// Wrapper class for data binding to resource strings.
    /// </summary>
    public class Resources
    {
        /// <summary>
        /// Create a property to databind to strings in Configuration.resx.
        /// </summary>
        public Configuration Configuration
        {
            get { return _Configuration; }
        }
        private Configuration _Configuration = new Configuration();

        /// <summary>
        /// Create a property to databind to strings in Strings.resx.
        /// </summary>
        public Strings Strings
        {
            get { return _Strings; }
        }
        private Strings _Strings = new Strings();
    }
}
