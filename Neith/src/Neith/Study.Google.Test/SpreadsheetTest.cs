using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using Codeplex.OAuth;

namespace Study.Google.Test
{
    using NUnit.Framework;
    [TestFixture]
    public class SpreadsheetTest
    {
        private const string ReqTokenURL = "https://www.google.com/accounts/OAuthGetRequestToken";

        [Test]
        public void OAuthTest()
        {
            Debug.WriteLine("[SpreadsheetTest::OAuthTest]スケジュール");

            var ConsumerKey = "consumerkey";
            var ConsumerSecret = "consumersecret";
            RequestToken requestToken;
            Exception err = null;
            var sig = new object();

            var authorizer = new OAuthAuthorizer(ConsumerKey, ConsumerSecret);
            lock (sig) {
                using (var job = authorizer
                    .GetRequestToken(ReqTokenURL)
                    .Select(res => res.Token)
                    .Subscribe(token =>
                    {
                        Debug.WriteLine("[SpreadsheetTest::OAuthTest]BuildAuthorizeUrl");
                        requestToken = token;
                        var url = authorizer.BuildAuthorizeUrl("https://www.google.com/analytics/feeds/accounts/default", token);
                        Debug.WriteLine("[SpreadsheetTest::OAuthTest]完了");
                    }, error =>
                    {
                        err = error;
                        lock (sig) Monitor.Pulse(sig);
                    }, () =>
                    {
                        lock (sig) Monitor.Pulse(sig);
                    }
                    )) { Monitor.Wait(sig); }
            }
            if (err != null) Assert.Fail(err.ToString());
        }
    }
}
