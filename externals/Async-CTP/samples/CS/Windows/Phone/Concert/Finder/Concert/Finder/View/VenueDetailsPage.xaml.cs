using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Navigation;
using ConcertFinder.Model;
using ConcertFinder.ViewModel;
using Microsoft.Phone.Controls;

namespace ConcertFinder.View
{
    public partial class VenueDetailsPage : PhoneApplicationPage
    {
        /// <summary>
        /// The view model for the page.
        /// </summary>
        private VenueDetailsPageViewModel viewModel;

        /// <summary>
        /// Track whether the page has initialized.
        /// </summary>
        private bool isInitialized = false;

        /// <summary>
        /// The default constructor.
        /// </summary>
        public VenueDetailsPage()
        {
            InitializeComponent();

            viewModel = new VenueDetailsPageViewModel();
            DataContext = viewModel;
        }

        /// <summary>
        /// Override for OnNavigatedTo.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!isInitialized)
            {
                if (State.Count > 0)
                {
                    viewModel.Initialize(State["Venue"] as Model.Venue, State["Events"] as ObservableCollection<Model.Event>);
                }
                else
                {
                    viewModel.Initialize(App.SelectedVenue);
                }

                isInitialized = true;
            }

            App.SelectedVenue = viewModel.SearchResult as Model.Venue;
        }

        /// <summary>
        /// Override for OnNavigatedFrom.
        /// </summary>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            State["Venue"] = viewModel.SearchResult as Model.Venue;
            State["Events"] = viewModel.Events;
        }

        /// <summary>
        /// Reset the selected venue when the back key is pressed.
        /// </summary>
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            App.SelectedVenue = null;
        }
    }
}