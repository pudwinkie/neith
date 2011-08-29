using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Threading;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Xml.Linq;
using System.Collections.ObjectModel;

namespace AsyncPhoneBackgroundThreads
{



    public partial class MainPage : PhoneApplicationPage
    {
        // boolean used to track if the page is new, and therefore state needs to be restored.
        bool newPageInstance = false;

        // string used to represent the data used by the page. A real application will likely use a more
        // complex data structure, such as an XML document. The only requirement is that the object be serializable.
        string pageDataObject;


        /// <summary>
        /// Main page constructor.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            // The constructor is not called if the page is already in memory and therefore page and application
            // state are still valid. Set newPageInstance to true so that in OnNavigatedTo, we know we need to 
            // recreate page and application state.
            newPageInstance = true;
        }

        /// <summary>
        /// Override of Page's OnNavigatedFrom method. Use helper class to preserve the visual state of the page's
        /// controls. Preserving application state takes place, if necessary, in the tombstoning event handlers
        /// in App.xaml.cs.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            newPageInstance = false;
        }

        /// <summary>
        /// Override of Page's OnNavigatedTo method. The page's visual state is restored. Then, the application
        /// state data is either set from the Application state dictionary or it is retrieved asynchronously.
        /// </summary>
        /// <param name="e"></param>
        protected async override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
          
            base.OnNavigatedTo(e);
            if (newPageInstance)
            {
                // if the application member variable is empty, call the method that loads data.
                if ((Application.Current as AsyncPhoneBackgroundThreads.App).AppDataObject == null)
                {
                    // Let the user know that data is being loaded in the background.
                    statusTextBlock.Text = "loading data...";

                    //Asynchronously wait for the result of the background task
                    Result TextToSet = await TaskEx.RunEx(() => GetData());
                  
                    //Set the result of the background task on the UI thread
                    SetData(TextToSet.Data, TextToSet.Source);
                    
                }
                else
                {
                    // Otherwise set the page's data object from the application member variable
                    pageDataObject = (Application.Current as AsyncPhoneBackgroundThreads.App).AppDataObject;

                    // Show the data to the user
                    SetData(pageDataObject, "preserved state");
                }

            }
            // Set the new page instance to false. It will not be set to true 
            // unless the page constructor is called.
            newPageInstance = false;
        }


        /// <summary>
        /// GetData is called on a background thread. If data is present in Isolated Storage, and its save
        /// date is recent, load the data from Isolated Storage. Otherwise, start an asynchronous http
        /// request to obtain fresh data.
        /// </summary>
        public async Task<Result> GetData()
        {
            // Check the time elapsed since data was last saved to Isolated Storage
            TimeSpan TimeSinceLastSave = TimeSpan.FromSeconds(0);
            if (IsolatedStorageSettings.ApplicationSettings.Contains("DataLastSave"))
            {
                DateTime dataLastSave = (DateTime)IsolatedStorageSettings.ApplicationSettings["DataLastSave"];
                TimeSinceLastSave = DateTime.Now - dataLastSave;
            }

            // Check to see if data exists in Isolated Storage and see if the data is fresh.
            // This example uses 30 seconds as the valid time window to make it easy to test. 
            // Real apps will use a larger window.
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
            if (isoStore.FileExists("myDataFile.txt") && TimeSinceLastSave.TotalSeconds < 30)
            {
                // This method loads the data from Isolated Storage, if it is available.
                StreamReader sr = new StreamReader(isoStore.OpenFile("myDataFile.txt", FileMode.Open));
                
                // Read the data from the isolated storage file asynchronously using the Async feature
                string data = await sr.ReadToEndAsync();
                await PhoneWorkaround.TemporaryAsyncPhoneWorkaround;
                sr.Close();

             
                return new Result() { Data = data, Source = "Isolated storage" };
            }
            else
            {
                try
                {
                    // Otherwise it gets the data from the Web. 
                    WebClient client = new WebClient();

                    //Download the string from the web asynchronously using the Async feature
                    string data = await client.DownloadStringTaskAsync("http://windowsteamblog.com/windows_phone/b/windowsphone/rss.aspx");

                    await PhoneWorkaround.TemporaryAsyncPhoneWorkaround;

                    return new Result() { Data = data, Source = "Web" };
                }
                catch
                {
                    return new Result() { Data = null, Source = "An unexpected error occured" };
                }
            }


        }


        /// <summary>
        /// If data was obtained asynchronously from the Web or from Isolated Storage, this method
        /// is invoked on the UI thread to update the page to show the data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="source"></param>
        public async void SetData(string data, string source)
        {

            pageDataObject = data;
            var itemsCollection = new ObservableCollection<RSSItem>();
            var items = await TaskEx.Run(() => from item in XElement.Parse(data).Element("channel").Elements("item")
                                               select new RSSItem {Title = (string)item.Element("title"),
                                                                   Url = (string)item.Element("link"),
                                                                   Date = (string)item.Element("pubDate")}
                                        );
            foreach (var item in items)
                itemsCollection.Add(item);

            itemsListBox.ItemsSource = itemsCollection;
            
            // Set the Application class member variable to the data so that the
            // Application class can store it when the application is deactivated or closed.
            (Application.Current as AsyncPhoneBackgroundThreads.App).AppDataObject = pageDataObject;

            statusTextBlock.Text = "data retrieved from " + source + ".";
        }

        public void DeferredLoadListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listbox = sender as ListBox;
            if (listbox == null) return;

            var item = listbox.SelectedItem as RSSItem;
            if (item == null) return;

            webBrowser.Visibility = Visibility.Visible;
            webBrowser.Navigate(new Uri(item.Url));
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (webBrowser.Visibility == Visibility.Visible)
            {
                webBrowser.Visibility = Visibility.Collapsed;

                e.Cancel = true;
            }
        }

    }


    /// <summary>
    /// This struct is used to workaround a bug that will be fixed in a future update of the Windows Phone toolset
    /// </summary>
    public struct PhoneWorkaround
    {
        public static PhoneWorkaround TemporaryAsyncPhoneWorkaround { get { return default(PhoneWorkaround); } }

        public PhoneWorkaround GetAwaiter() { return this; }

        public bool IsCompleted { get { return false; } }
        public void OnCompleted(Action continuation) { TaskEx.Run(continuation); }
        public void GetResult() { }
    }
    /// <summary>
    /// This is a struct used to hold the result of the background operation
    /// </summary>
    public struct Result
    {
        public string Data { get; set; }
        public string Source { get; set; }
    }

    public class RSSItem
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Date { get; set; }
    }
}