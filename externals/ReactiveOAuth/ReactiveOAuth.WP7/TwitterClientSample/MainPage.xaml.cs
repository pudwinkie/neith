using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using Codeplex.OAuth;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Reactive;

namespace TwitterClientSample
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        // set your consumerkey and secret
        const string ConsumerKey = "";
        const string ConsumerSecret = "";

        RequestToken requestToken;
        AccessToken accessToken;

        private string ReadWebException(Exception e)
        {
            var ex = e as WebException;
            if (ex != null)
            {
                using (var stream = ex.Response.GetResponseStream())
                using (var sr = new StreamReader(stream))
                {
                    return sr.ReadToEnd();
                }
            }
            else return e.ToString();
        }

        // Get RequestToken flow sample
        // create authorizer and call "GetRequestToken" and "BuildAuthorizeUrl"
        private void GetRequestTokenButton_Click(object sender, RoutedEventArgs e)
        {
            var authorizer = new OAuthAuthorizer(ConsumerKey, ConsumerSecret);
            authorizer.GetRequestToken("http://twitter.com/oauth/request_token")
                .Select(res => res.Token)
                .ObserveOnDispatcher()
                .Subscribe(token =>
                {
                    requestToken = token;
                    var url = authorizer.BuildAuthorizeUrl("http://twitter.com/oauth/authorize", token);
                    webBrowser1.Navigate(new Uri(url));
                    BrowserAuthorize.Visibility = System.Windows.Visibility.Visible;
                }, ex => MessageBox.Show(ReadWebException(ex)));
        }

        // Get AccessToken flow sample
        // TokenResponse's ExtraData is ILookup.
        // if twitter, you can take "user_id" and "screen_name".
        private void GetAccessTokenButton_Click(object sender, RoutedEventArgs e)
        {
            var pincode = PinCodeTextBox.Text;

            var authorizer = new OAuthAuthorizer(ConsumerKey, ConsumerSecret);
            authorizer.GetAccessToken("http://twitter.com/oauth/access_token", requestToken, pincode)
                .ObserveOnDispatcher()
                .Subscribe(res =>
                {
                    BrowserAuthorize.Visibility = System.Windows.Visibility.Collapsed;

                    AuthorizedTextBlock.Text = "Authorized";
                    UserIdTextBlock.Text = res.ExtraData["user_id"].First();
                    ScreenNameTextBlock.Text = res.ExtraData["screen_name"].First();
                    accessToken = res.Token;
                }, ex => MessageBox.Show(ReadWebException(ex)));
        }

        // Twitter Read Sample
        // set parameters can use Collection Initializer
        private void GetTimeLineButton_Click(object sender, RoutedEventArgs e)
        {
            if (accessToken == null) { MessageBox.Show("at first, get accessToken"); return; }

            var client = new OAuthClient(ConsumerKey, ConsumerSecret, accessToken)
            {
                Url = "http://api.twitter.com/1/statuses/home_timeline.xml",
                Parameters = { { "count", 20 }, { "page", 1 } }
            };
            client.GetResponseText()
                .Select(s => XElement.Parse(s))
                .SelectMany(x => x.Descendants("status"))
                .Select(x => new
                {
                    Text = x.Element("text").Value,
                    Name = x.Element("user").Element("screen_name").Value
                })
                .ObserveOnDispatcher()
                .Subscribe(
                    a => TimeLineViewListBox.Items.Add(a.Name + ":" + a.Text),
                    ex => MessageBox.Show(ReadWebException(ex)));
        }

        // Twitter Post Sample
        // if post then set MethodType = MethodType.Post
        private void PostButton_Click(object sender, RoutedEventArgs e)
        {
            if (accessToken == null) { MessageBox.Show("at first, get accessToken"); return; }

            var postText = PostTextBox.Text;
            var client = new OAuthClient(ConsumerKey, ConsumerSecret, accessToken)
            {
                MethodType = MethodType.Post,
                Url = "http://api.twitter.com/1/statuses/update.xml",
                Parameters = { { "status", postText } }
            };
            client.GetResponseText()
                .Select(s => XElement.Parse(s))
                .ObserveOnDispatcher()
                .Subscribe(x => MessageBox.Show("Post Success:" + x.Element("text").Value),
                    ex => MessageBox.Show(ReadWebException(ex)));
        }

        IDisposable stramingHandle = Disposable.Empty;

        // StreamingAPI sample
        // read json using with DynamicJson
        // see details -> http://dynamicjson.codeplex.com/
        private void StreamingStartButton_Click(object sender, RoutedEventArgs e)
        {
            if (accessToken == null) { MessageBox.Show("at first, get accessToken"); return; }

            StreamingStartButton.Content = "Reading StreamingAPI";
            StreamingStartButton.IsEnabled = false;
            var serializer = new DataContractJsonSerializer(typeof(Tweet));

            var client = new OAuthClient(ConsumerKey, ConsumerSecret, accessToken)
            {
                Url = "http://chirpstream.twitter.com/2b/user.json"
            };
            stramingHandle = client.GetResponseLines()
                .Where(s => s.Contains("text")) // filtered status only
                .Select(s =>
                {
                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(s)))
                    {
                        return (Tweet)serializer.ReadObject(stream);
                    }
                })
                .ObserveOnDispatcher()
                .Subscribe(
                    t => StreamingViewListBox.Items.Add(t.Text),
                    ex => MessageBox.Show(ReadWebException(ex)));
        }

        // if you want to stop streaming then call dispose.
        private void StreamingStopButton_Click(object sender, RoutedEventArgs e)
        {
            stramingHandle.Dispose();
            StreamingStartButton.Content = "StreamingAPI Read Start";
            StreamingStartButton.IsEnabled = true;
        }
    }

    [DataContract]
    public class Tweet
    {
        [DataMember(Name = "text")]
        public string Text { get; set; }
    }
}
