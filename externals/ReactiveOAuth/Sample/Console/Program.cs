using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Codeplex.OAuth;

static class Program
{
    static void Main(string[] args)
    {
        ServicePointManager.Expect100Continue = false;

        // set your consumerkey/secret
        const string ConsumerKey = "";
        const string ConsumerSecret = "";

        AccessToken accessToken = null;
        string userId, screenName;

        // get accesstoken flow
        // create authorizer and call "GetRequestToken" ,"BuildAuthorizeUrl", "GetAccessToken"
        // TokenResponse's ExtraData is ILookup.
        // if twitter, you can take "user_id" and "screen_name".
        // Run is sync. Subscribe is async.
        var authorizer = new OAuthAuthorizer(ConsumerKey, ConsumerSecret);
        authorizer.GetRequestToken("http://twitter.com/oauth/request_token")
             .Do(r =>
             {
                 Console.WriteLine("Check Browser and input PinCode");
                 Process.Start(authorizer.BuildAuthorizeUrl("http://twitter.com/oauth/authorize", r.Token)); // open browser
             })
             .Select(r => new { RequestToken = r.Token, PinCode = Console.ReadLine() })
             .SelectMany(a => authorizer.GetAccessToken("http://twitter.com/oauth/access_token", a.RequestToken, a.PinCode))
             .Run(r =>
              {
                  userId = r.ExtraData["user_id"].First();
                  screenName = r.ExtraData["screen_name"].First();
                  accessToken = r.Token;
              });

        // get accesstoken flow by xAuth
        //new OAuthAuthorizer(ConsumerKey, ConsumerSecret)
        //    .GetAccessToken("https://api.twitter.com/oauth/access_token", "username", "password")
        //    .Run(r => accessToken = r.Token);


        // get timeline flow
        // set parameters can use Collection Initializer
        // if you want to set webrequest parameters then use ApplyBeforeRequest
        var client = new OAuthClient(ConsumerKey, ConsumerSecret, accessToken)
        {
            Url = "http://api.twitter.com/1/statuses/home_timeline.xml",
            Parameters = { { "count", 20 }, { "page", 1 } },
            ApplyBeforeRequest = req => { req.Timeout = 1000; req.UserAgent = "ReactiveOAuth"; }
        };
        client.GetResponseText()
            .Select(s => XElement.Parse(s))
            .Run(x => Console.WriteLine(x.ToString()));

        // post flow
        // if post then set MethodType = MethodType.Post
        //new OAuthClient(ConsumerKey, ConsumerSecret, accessToken)
        //{
        //    MethodType = MethodType.Post,
        //    Url = "http://api.twitter.com/1/statuses/update.xml",
        //    Parameters = { { "status", "PostTest from ReactiveOAuth" } }
        //}.GetResponseText()
        //    .Select(s => XElement.Parse(s))
        //    .Run(x => Console.WriteLine("Post Success:" + x.Element("text")));

        // StreamingAPI sample
        // if you use streaming api, recommend call GetResponseLines.
        // see details -> WPF Sample.
        //new OAuthClient(ConsumerKey, ConsumerSecret, accessToken)
        //{
        //    Url = "http://chirpstream.twitter.com/2b/user.json"
        //}.GetResponseLines()
        //.Run(s => Console.WriteLine(s));
    }
}