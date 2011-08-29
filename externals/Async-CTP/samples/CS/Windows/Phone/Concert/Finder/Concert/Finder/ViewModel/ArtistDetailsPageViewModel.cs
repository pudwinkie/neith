using ConcertFinder.Model;

namespace ConcertFinder.ViewModel
{
    /// <summary>
    /// The view model for the ArtistDetailsPage.
    /// </summary>
    public class ArtistDetailsPageViewModel : AbstractDetailsPageViewModel
    {
        public ArtistDetailsPageViewModel()
        {
            this.EventGroup = EventGroup.Artist;
        }
    }
}
