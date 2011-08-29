using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Net;

namespace TutorialCS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        XNamespace xa = "http://www.w3.org/2005/Atom";
        XNamespace xd = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        XNamespace xm = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        string query = "http://odata.netflix.com/Catalog/Titles?$filter=ReleaseYear eq {0}&$skip={1}&$top={2}&$select=Url,BoxArt";

        class Movie
        {
            public string Title { get; set; }
            public string Url { get; set; }
            public string BoxArtUrl { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
            textBox.Focus();
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            LoadMovies(Int32.Parse(textBox.Text));
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            int year;
            searchButton.IsEnabled = Int32.TryParse(textBox.Text, out year) && year >= 1900 && year <= 2099;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Cancellation not yet implemented");
        }

        void LoadMovies(int year)
        {
            resultsPanel.Children.Clear();
            statusText.Text = "";
            var pageSize = 10;
            var imageCount = 0;
            while (true)
            {
                statusText.Text = string.Format("Searching...  {0} Titles", imageCount);
                // TODO: once code below has been made async, then statusText will work properly
                var movies = QueryMovies(year, imageCount, pageSize);
                if (movies.Length == 0) break;
                DisplayMovies(movies);
                imageCount += movies.Length;
            }
            statusText.Text = string.Format("{0} Titles", imageCount);
        }

        Movie[] QueryMovies(int year, int first, int count)
        {
            var client = new WebClient();
            var url = String.Format(query, year, first, count);

            // TODO: make following code async. It's non-responsive due to the synchronous call to WebClient.DownloadString.
            // To fix it, follow the async walkthrough: http://go.microsoft.com/fwlink/?LinkId=203988
            string data = client.DownloadString(new Uri(url));

            var movies =
                from entry in XDocument.Parse(data).Descendants(xa + "entry")
                let properties = entry.Element(xm + "properties")
                select new Movie
                {
                    Title = (string)entry.Element(xa + "title"),
                    Url = (string)properties.Element(xd + "Url"),
                    BoxArtUrl = (string)properties.Element(xd + "BoxArt").Element(xd + "LargeUrl")
                };
            return movies.ToArray();
        }

        void DisplayMovies(Movie[] movies)
        {
            foreach (var movie in movies)
            {
                var bitmap = new BitmapImage(new Uri(movie.BoxArtUrl));
                var image = new Image() { Source = bitmap, Width = 110, Height = 150, Margin = new Thickness(5) };
                var url = movie.Url;
                image.MouseDown += (sender, e) => System.Diagnostics.Process.Start(url);
                resultsPanel.Children.Add(image);
            }
        }

    }
}





