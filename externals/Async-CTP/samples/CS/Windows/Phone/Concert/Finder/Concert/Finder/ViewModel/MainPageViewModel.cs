using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using ConcertFinder.Model;
using Microsoft.Phone.Reactive;

namespace ConcertFinder.ViewModel
{
    /// <summary>
    /// The view model for the MainPage.
    /// </summary>
    public class MainPageViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The list of search results.
        /// </summary>
        public ObservableCollection<ISearchable> SearchResults { get; set; }
        public bool IsListEmpty { get { return !_SearchPending && !SearchInProgress && SearchResults.Count == 0; } }

        /// <summary>
        /// Gets or sets whether a search is in progress.
        /// </summary>
        public bool SearchInProgress
        {
            get { return _SearchInProgress; }
            set
            {
                _SearchInProgress = value;
                _SearchPending = false;
                NotifyPropertyChanged("SearchInProgress");
                NotifyPropertyChanged("IsListEmpty");
            }
        }
        private bool _SearchInProgress = false;
        private bool _SearchPending = true;

        /// <summary>
        /// The default constructor.
        /// </summary>
        public MainPageViewModel()
        {
            SearchResults = new ObservableCollection<ISearchable>();
        }

        /// <summary>
        /// Perform a search based on the user's query.
        /// </summary>
        /// <param name="query">The search query.</param>
        public void Search(string query)
        {
            Observable.FromEvent<SearchResultsAvailableEventArgs>(
                ev => App.SearchResultsAvailable += ev,
                ev => App.SearchResultsAvailable -= ev).
                Take(1).
                Subscribe(
                (e) =>
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        SearchResults.Clear();

                        foreach (var SearchResult in e.EventArgs.SearchResults)
                        {
                            SearchResults.Add(SearchResult);
                        }

                        SearchInProgress = false;
                    });
                });

            SearchInProgress = true;

            App.Search(query);
        }

        #region INotifyPropertyChanged

        /// <summary>
        /// The PropertyChanged event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notify listeners of property changes.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        private void NotifyPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;

            if (handler != null)
            {
                handler(null, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
