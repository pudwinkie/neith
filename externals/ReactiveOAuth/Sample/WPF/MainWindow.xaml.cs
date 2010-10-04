using System;
using System.Diagnostics;
using System.Disposables;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Xml.Linq;
using Codeplex.Data;
using Codeplex.OAuth;

namespace WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ServicePointManager.Expect100Continue = false;
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
                .Subscribe(token =>
                {
                    requestToken = token;
                    var url = authorizer.BuildAuthorizeUrl("http://twitter.com/oauth/authorize", token);
                    Process.Start(url); // open browser
                    MessageBox.Show("check browser, allow oauth and enter pincode");
                }, ex => MessageBox.Show(ReadWebException(ex)));
        }

        // Get AccessToken flow sample
        // TokenResponse's ExtraData is ILookup.
        // if twitter, you can take "user_id" and "screen_name".
        private void GetAccessTokenButton_Click(object sender, RoutedEventArgs e)
        {
            if (requestToken == null) { MessageBox.Show("at first, get requestToken"); return; }

            var pincode = PinCodeTextBox.Text;
            var authorizer = new OAuthAuthorizer(ConsumerKey, ConsumerSecret);
            authorizer.GetAccessToken("http://twitter.com/oauth/access_token", requestToken, pincode)
                .ObserveOnDispatcher()
                .Subscribe(res =>
                {
                    AuthorizedTextBlock.Text = "Authorized";
                    UserIdTextBlock.Text = res.ExtraData["user_id"].First();
                    ScreenNameTextBlock.Text = res.ExtraData["screen_name"].First();
                    accessToken = res.Token;
                }, ex => MessageBox.Show(ReadWebException(ex)));
        }

        // Twitter Read Sample
        // set parameters can use Collection Initializer
        // if you want to set webrequest parameters then use ApplyBeforeRequest
        private void GetTimeLineButton_Click(object sender, RoutedEventArgs e)
        {
            if (accessToken == null) { MessageBox.Show("at first, get accessToken"); return; }

            var client = new OAuthClient(ConsumerKey, ConsumerSecret, accessToken)
            {
                Url = "http://api.twitter.com/1/statuses/home_timeline.xml",
                Parameters = { { "count", 20 }, { "page", 1 } },
                ApplyBeforeRequest = req => { req.Timeout = 1000; req.UserAgent = "ReactiveOAuth"; }
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

            var client = new OAuthClient(ConsumerKey, ConsumerSecret, accessToken)
            {
                Url = "http://chirpstream.twitter.com/2b/user.json"
            };
            stramingHandle = client.GetResponseLines()
                .Where(s => !string.IsNullOrWhiteSpace(s)) // filter invalid data
                .Select(s => DynamicJson.Parse(s))
                .Where(d => d.text()) // has text is status
                .ObserveOnDispatcher()
                .Subscribe(
                    d => StreamingViewListBox.Items.Add(d.user.screen_name + ":" + d.text),
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
}