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
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Netflix
{
    public partial class MainPage : PhoneApplicationPage
    {
        XNamespace xa = "http://www.w3.org/2005/Atom";
        XNamespace xd = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        XNamespace xm = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        string query = "http://odata.netflix.com/Catalog/Titles?$filter=ReleaseYear eq {0}&$skip={1}&$top={2}&$select=Url,ReleaseYear,Rating,Runtime,AverageRating,BoxArt";

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.Focus();
        }

        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            int year;
            if (int.TryParse(textBox.Text, out year))
            {
                // Check whether this year is already in the pivot:
                var yearMovies = yearPivot.Items.OfType<YearMovies>().Where(ym => ym.Year == year).SingleOrDefault();

                if (yearMovies == null)
                    LoadMoviesAsync(year);
                else
                    yearPivot.SelectedItem = yearMovies;
            }
        }

        async void LoadMoviesAsync(int year)
        {
            var movieCollection = new ObservableCollection<Movie>();
            var yearMovies = new YearMovies
            {
                Year = year,
                Movies = movieCollection
            };
            yearPivot.Items.Add(yearMovies);
            yearPivot.SelectedItem = yearMovies;

            yearMovies.StatusText = "";

            var pageSize = 10;
            var imageCount = 0;
            while (true)
            {
                yearMovies.StatusText = string.Format("Searching...  {0} titles so far...", imageCount);
                var movies = await QueryMoviesAsync(year, imageCount, pageSize);
                if (movies.Length == 0) break;
                foreach (var movie in movies)
                    movieCollection.Add(movie);
                imageCount += movies.Length;
            }
            yearMovies.StatusText = string.Format("{0} titles found", imageCount);
        }

        async Task<Movie[]> QueryMoviesAsync(int year, int first, int count)
        {
            var client = new WebClient();
            var url = String.Format(query, year, first, count);

            string data = await client.DownloadStringTaskAsync(new Uri(url));

            return await TaskEx.Run(delegate
            {
                var movies = from entry in XDocument.Parse(data).Descendants(xa + "entry")
                             let properties = entry.Element(xm + "properties")
                             select new Movie
                             {
                                 Title = (string)entry.Element(xa + "title"),
                                 Url = (string)properties.Element(xd + "Url"),
                                 Year = (string)properties.Element(xd + "ReleaseYear"),
                                 Rating = (string)properties.Element(xd + "Rating"),
                                 Length = string.Format("{0} min", (int)Math.Round(int.Parse("0" + (string)properties.Element(xd + "Runtime")) / 60.0)),
                                 UserReview = new string('*', (int)Math.Round(decimal.Parse("0" + (string)properties.Element(xd + "AverageRating")))),
                                 BoxArtUrl = (string)properties.Element(xd + "BoxArt").Element(xd + "LargeUrl")
                             };
                return movies.ToArray();
            });
        }

        public void DeferredLoadListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listbox = sender as ListBox;
            if (listbox == null) return;

            var item = listbox.SelectedItem as Movie;
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

    public class YearMovies : INotifyPropertyChanged
    {
        public int Year { get; set; }
        public ObservableCollection<Movie> Movies { get; set; }

        private string statusText;
        public string StatusText
        {
            get { return statusText; }
            set { statusText = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("StatusText")); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class Movie
    {
        public string Title { get; set; }
        public string Year { get; set; }
        public string Rating { get; set; }
        public string Length { get; set; }
        public string UserReview { get; set; }
        public string Url { get; set; }
        public string BoxArtUrl { get; set; }
    }
}