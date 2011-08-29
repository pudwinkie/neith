using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using ConcertFinder.Model;
using Microsoft.Phone.Reactive;

namespace ConcertFinder.ViewModel
{
    /// <summary>
    /// The base view model for the ArtistDetailsPage and the VenueDetailsPage.
    /// </summary>
    public abstract class AbstractDetailsPageViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The search result to display.
        /// </summary>
        public ISearchable SearchResult
        {
            get { return _SearchResult; }
            set
            {
                _SearchResult = value;
                NotifyPropertyChanged("SearchResult");
            }
        }
        private ISearchable _SearchResult;

        /// <summary>
        /// The events to display.
        /// </summary>
        public ObservableCollection<Model.Event> Events { get; set; }
        public bool IsListEmpty { get { return !SearchInProgress && Events.Count == 0; } }

        /// <summary>
        /// The event group for the page.
        /// </summary>
        public EventGroup EventGroup { get; set; }

        /// <summary>
        /// Gets or sets whether a search is in progress.
        /// </summary>
        public bool SearchInProgress
        {
            get { return _SearchInProgress; }
            set
            {
                _SearchInProgress = value;
                NotifyPropertyChanged("SearchInProgress");
                NotifyPropertyChanged("IsListEmpty");
            }
        }
        private bool _SearchInProgress = false;

        /// <summary>
        /// Initialize the page with a search result and the given events.
        /// </summary>
        /// <param name="SearchResult">The search result to display.</param>
        /// <param name="Events">The events to display.</param>
        public void Initialize(ISearchable SearchResult, ObservableCollection<Model.Event> Events)
        {
            this.SearchResult = SearchResult;
            this.Events = Events;
        }

        /// <summary>
        /// Initialize the page with a search result.
        /// </summary>
        /// <param name="SearchResult">The search result to display.</param>
        public void Initialize(ISearchable SearchResult) 
        {
            Initialize(SearchResult, new ObservableCollection<Model.Event>());

            GetEvents();
        }

        /// <summary>
        /// Initialize the page with events.
        /// </summary>
        public void GetEvents()
        {
            Observable.FromEvent<EventsAvailableEventArgs>(
                ev => App.EventsAvailable += ev,
                ev => App.EventsAvailable -= ev).
                Where(args => args.EventArgs.EventGroup == EventGroup && (args.EventArgs.State as string).Equals(SearchResult.Id)).
                Take(1).
                Subscribe(
                (args) =>
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        Events.Clear();

                        foreach (var Event in App.Events[args.EventArgs.EventGroup])
                        {
                            Events.Add(Event);
                        }

                        SearchInProgress = false;
                    });
                });

            SearchInProgress = true;

            App.GetEvents(EventGroup, SearchResult.Id);
        }

        #region INotifyPropertyChanged

        /// <summary>
        /// The PropertyChanged event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notify listeners of changes to the property with the given name.
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
