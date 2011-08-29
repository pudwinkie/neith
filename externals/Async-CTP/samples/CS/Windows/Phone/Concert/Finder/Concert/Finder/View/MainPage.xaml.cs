using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using ConcertFinder.Model;
using ConcertFinder.ViewModel;
using Microsoft.Phone.Controls;

namespace ConcertFinder.View
{
    public partial class MainPage : PhoneApplicationPage
    {
        /// <summary>
        /// The view model for the page.
        /// </summary>
        private MainPageViewModel viewModel;

        /// <summary>
        /// The default constructor.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            viewModel = new MainPageViewModel();
            DataContext = viewModel;
        }

        /// <summary>
        /// Perform the search when the user presses Enter.
        /// </summary>
        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter) && (SearchBox.Text.Length >= 2))
            {
                SearchResults.Focus();           
                viewModel.Search(SearchBox.Text);
            }
        }

        /// <summary>
        /// Select the text when the text box is entered.
        /// </summary>
        private void SearchBox_GotFocus(object sender, EventArgs e)
        {
            SearchBox.SelectAll();
        }

        /// <summary>
        /// Selection changed handler for the event list.
        /// </summary>
        private void SearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var AddedItem = e.AddedItems[0];

                if (AddedItem as Artist != null)
                {
                    App.SelectedArtist = AddedItem as Artist;
                    NavigationService.Navigate(new Uri("/View/ArtistDetailsPage.xaml", UriKind.Relative));
                }
                else if (AddedItem as Venue != null)
                {
                    App.SelectedVenue = AddedItem as Venue;
                    NavigationService.Navigate(new Uri("/View/VenueDetailsPage.xaml", UriKind.Relative));
                }
            }

            (sender as ListBox).SelectedItem = null;
        }

        /// <summary>
        /// Override for OnNavigatedTo.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SearchBox.Text = App.ActiveSearchQuery ?? String.Empty;

            if (SearchResults.Items.Count == 0)
            {
                // Queue the operation to ensure the text box does not lose focus.
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        SearchBox.Focus();
                    });
                });
            }
        }
    }
}