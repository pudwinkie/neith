using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using ConcertFinder.Model;
using ConcertFinder.Providers;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Reactive;
using Microsoft.Phone.Shell;
using System.Linq;
using System.Threading.Tasks;

namespace ConcertFinder
{
    public partial class App : Application
    {
        /// <summary>Provides easy access to the root frame of the Phone Application.</summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>The active search query.</summary>
        public static string ActiveSearchQuery
        {
            get { return _ActiveSearchQuery; }
            set
            {
                _ActiveSearchQuery = value;
                PhoneApplicationService.Current.State["ActiveSearchQuery"] = _ActiveSearchQuery;
            }
        }
        private static string _ActiveSearchQuery;

        /// <summary>The cancellation token source for searches.</summary>
        private static CancellationTokenSource SearchCancellationTokenSource;

        /// <summary>The artist which the user has selected from the search page.</summary>
        public static Model.Artist SelectedArtist
        {
            get { return _SelectedArtist; }
            set
            {
                _SelectedArtist = value;
                PhoneApplicationService.Current.State["SelectedArtist"] = _SelectedArtist;
            }
        }
        private static Model.Artist _SelectedArtist;

        /// <summary>The venue which the user has selected from the search page.</summary>
        public static Model.Venue SelectedVenue
        {
            get { return _SelectedVenue; }
            set
            {
                _SelectedVenue = value;
                PhoneApplicationService.Current.State["SelectedVenue"] = _SelectedVenue;
            }
        }
        public static Model.Venue _SelectedVenue;

        /// <summary>The collection of events.</summary>
        public static Dictionary<EventGroup, List<Model.Event>> Events = new Dictionary<EventGroup, List<Model.Event>>();

        /// <summary>The data provider.</summary>
        private static IProvider Provider = new EventfulProvider();

        /// <summary>Constructor for the Application object.</summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are being GPU accelerated with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;
            }

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Enable the tilt effect.
            TiltEffect.SetIsTiltEnabled(RootFrame, true);
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e) { }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            ActiveSearchQuery = PhoneApplicationService.Current.State.ContainsKey("ActiveSearchQuery") ?
                PhoneApplicationService.Current.State["ActiveSearchQuery"] as string : null;
            SelectedArtist = PhoneApplicationService.Current.State.ContainsKey("SelectedArtist") ?
                PhoneApplicationService.Current.State["SelectedArtist"] as Model.Artist : null;
            SelectedVenue = PhoneApplicationService.Current.State.ContainsKey("SelectedVenue") ?
                PhoneApplicationService.Current.State["SelectedVenue"] as Model.Venue : null;
        }

        /// <summary>
        /// Perform a search based on the user's query.
        /// </summary>
        /// <param name="query">The search query.</param>
        public static async void Search(string query)
        {
            ActiveSearchQuery = query;

            if (SearchCancellationTokenSource != null) SearchCancellationTokenSource.Cancel();
            SearchCancellationTokenSource = new CancellationTokenSource();

            List<ISearchable> searchResults = new List<ISearchable>();
            try
            {
                var search = await Provider.SearchAsync(query, SearchCancellationTokenSource.Token);
                searchResults.AddRange(search.SelectMany(r => r.Results));
            }
            catch { }
            if (query != ActiveSearchQuery) return;
            RaiseSearchResultsAvailable(searchResults);
        }

        /// <summary>
        /// Get the events for the given EventGroup.
        /// </summary>
        /// <param name="eventGroup">The EventGroup of events.</param>
        /// <param name="id">An ID used in the search.</param>
        public static async void GetEvents(EventGroup eventGroup, string id)
        {
            EventsAvailable eventsAvailable = await (eventGroup == EventGroup.Artist ?
                Provider.GetArtistEventsAsync(id) : Provider.GetVenueEventsAsync(id));
            Events[eventGroup] = eventsAvailable.Events;
            RaiseEventsAvailable(eventGroup, eventsAvailable.State);
        }

        /// <summary>The SearchResultsAvailable event.</summary>
        public static event EventHandler<SearchResultsAvailableEventArgs> SearchResultsAvailable;

        /// <summary>
        /// Raises the SearchResultsAvailable event.
        /// </summary>
        /// <param name="searchResults"></param>
        private static void RaiseSearchResultsAvailable(List<ISearchable> searchResults)
        {
            var handler = SearchResultsAvailable;
            if (handler != null) handler(null, new SearchResultsAvailableEventArgs(searchResults));
        }

        /// <summary>
        /// The EventsAvailable event.
        /// </summary>
        public static event EventHandler<EventsAvailableEventArgs> EventsAvailable;

        /// <summary>Raises the EventsAvailable event for the given EventGroup.</summary>
        private static void RaiseEventsAvailable(EventGroup EventGroup, object State = null)
        {
            var handler = EventsAvailable;
            if (handler != null) handler(null, new EventsAvailableEventArgs(EventGroup, State));
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }

    /// <summary>Event args for the SearchResultsAvailable event.</summary>
    public class SearchResultsAvailableEventArgs : EventArgs
    {
        public List<ISearchable> SearchResults { get; private set; }

        public SearchResultsAvailableEventArgs(List<ISearchable> SearchResults)
        {
            this.SearchResults = SearchResults;
        }
    }

    /// <summary>Event args for the EventsAvailable event.</summary>
    public class EventsAvailableEventArgs : EventArgs
    {
        public EventGroup EventGroup { get; private set; }
        public object State { get; private set; }

        public EventsAvailableEventArgs(EventGroup EventGroup, object State = null)
        {
            this.EventGroup = EventGroup;
            this.State = State;
        }
    }
}