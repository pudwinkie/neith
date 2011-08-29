using ConcertFinder.Model;

namespace ConcertFinder.ViewModel
{
    /// <summary>
    /// The view model for the VenueDetailsPage.
    /// </summary>
    public class VenueDetailsPageViewModel : AbstractDetailsPageViewModel
    {
        public VenueDetailsPageViewModel()
        {
            this.EventGroup = EventGroup.Venue;
        }
    }
}
