using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Navigation;
using ConcertFinder.ViewModel;
using Microsoft.Phone.Controls;
using ConcertFinder.Model;

namespace ConcertFinder.View
{
    public partial class ArtistDetailsPage : PhoneApplicationPage
    {
        /// <summary>
        /// The view model for the page.
        /// </summary>
        private ArtistDetailsPageViewModel viewModel;

        /// <summary>
        /// Track whether the page has initialized.
        /// </summary>
        private bool isInitialized = false;

        /// <summary>
        /// The default constructor.
        /// </summary>
        public ArtistDetailsPage()
        {
            InitializeComponent();

            viewModel = new ArtistDetailsPageViewModel();
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
                    viewModel.Initialize(State["Artist"] as Model.Artist, State["Events"] as ObservableCollection<Model.Event>);
                }
                else
                {
                    viewModel.Initialize(App.SelectedArtist);
                }

                isInitialized = true;
            }

            App.SelectedArtist = viewModel.SearchResult as Model.Artist;
        }

        /// <summary>
        /// Override for OnNavigatedFrom.
        /// </summary>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            State["Artist"] = viewModel.SearchResult as Model.Artist;
            State["Events"] = viewModel.Events;
        }

        /// <summary>
        /// Reset the selected artist when the back key is pressed.
        /// </summary>
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            App.SelectedArtist = null;
        }
    }
}