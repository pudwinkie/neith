using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Cache;

namespace NewsFromRSSFeeds
{
	public partial class News : System.Web.UI.Page
	{
		protected async void Page_Load(object sender, EventArgs e)
		{

            var sw = Stopwatch.StartNew();

            var connectionString = @"server=.\sqlexpress;AttachDbFilename=|DataDirectory|\RssFeeds.mdf;Integrated Security=True;User Instance=True;Asynchronous Processing=true;";
            var user = "Anders";

            var urls = new List<string>();
            using (var con = new SqlConnection(connectionString)) {
                con.Open();
                var cmd = new SqlCommand("GetUserFeeds", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserID", user);
                using (var reader = await cmd.ExecuteReaderAsync()) {
                    while (reader.Read()) urls.Add(reader["FeedURL"].ToString());
                }
            }

            /* //Uncomment to get the feeds using the Parallel library. This will run the query on multiple threads
            var feeds = new string[urls.Count];
            Parallel.For(0, urls.Count, i => feeds[i] = CreateWebClient().DownloadString(urls[i]));
            */

            /* //Uncomment to get the feeds using Async. 
            var feeds = await TaskEx.WhenAll(from url in urls select CreateWebClient().DownloadStringTaskAsync(url));
            */
            
            //This line gets the feeds synchronously.   
            var feeds = (from url in urls select CreateWebClient().DownloadString(url)).ToArray();

            var items =
                from feed in feeds
                from channel in XElement.Parse(feed).Elements("channel")
                from item in channel.Elements("item").Take(3)
                let date = item.GetPubDate()
                orderby date descending
                select new {
                    Source = (string)channel.Element("title"),
                    SourceLink = (string)channel.Element("link"),
                    Title = (string)item.Element("title"),
                    Link = (string)item.Element("link"),
                    Description = (string)item.Element("description"),
                    Date = date
                };

            NewsRepeater.DataSource = items.Take(50);
            NewsRepeater.DataBind();

            lblTime.InnerText = string.Format("({0:s\\.ff} seconds)", sw.Elapsed);
	    }

        private static WebClient CreateWebClient()
        {
            return new WebClient() { CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore) }; //Behind firewall you may need to specify the proxy server for example: Proxy = new WebProxy("YourProxyServer")
        }
	}
}