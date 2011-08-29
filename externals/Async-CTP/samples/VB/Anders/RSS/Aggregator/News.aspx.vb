Imports System.Net
Imports System.Net.Cache
Imports System.Data.SqlClient
Imports System.Threading.Tasks

Public Class News
    Inherits System.Web.UI.Page

    Protected Async Sub Page_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        Dim sw = Stopwatch.StartNew()

        Dim connectionString = "server=.\sqlexpress;AttachDbFilename=|DataDirectory|\RssFeeds.mdf;Integrated Security=True;User Instance=True;Asynchronous Processing=true;"
        Dim User = "Anders"

        Dim urls = New List(Of String)()
        Using con = New SqlConnection(connectionString)
            con.Open()
            Dim cmd = New SqlCommand("GetUserFeeds", con)
            cmd.CommandType = CommandType.StoredProcedure
            cmd.Parameters.AddWithValue("@UserID", User)
            Using reader = Await cmd.ExecuteReaderAsync()
                While reader.Read()
                    urls.Add(reader("FeedURL").ToString())
                End While
            End Using
        End Using

        'Uncomment to get the feeds using Task Parallel Library. This will run the query on multiple threads
        'Dim feeds = New String(urls.Count - 1) {}
        'Parallel.For(0, urls.Count, Sub(i) feeds(i) = CreateWebClient().DownloadString(urls(i)))

        'Uncomment to get the feeds using Async. 
        'Dim feeds = Await TaskEx.WhenAll(From url In urls Select CreateWebClient().DownloadStringTaskAsync(url))

        'This line gets the feeds synchronously.
        Dim feeds = (From url In urls Select CreateWebClient().DownloadString(url)).ToArray()

        Dim items =
            From feed In feeds
            From channel In XElement.Parse(feed).<channel>
            From item In channel.<item>.Take(3)
            Let pubdate = item.GetPubDate()
            Order By pubdate Descending
            Select New With {
                .Source = channel.<title>.Value,
                .SourceLink = channel.<link>.Value,
                .Title = item.<title>.Value,
                .Link = item.<link>.Value,
                .Description = item.<description>.Value,
                .PubDate = pubdate
            }

        NewsRepeater.DataSource = items.Take(50)
        NewsRepeater.DataBind()

        lblTime.InnerText = String.Format("({0:s.ff} seconds)", sw.Elapsed.ToString()) 'Behind firewall you may need to specify the proxy server for example: Proxy = new WebProxy("YourProxyServer")
    End Sub

    Private Shared Function CreateWebClient() As WebClient
        Return New WebClient() With {.CachePolicy = New RequestCachePolicy(RequestCacheLevel.NoCacheNoStore)} '
    End Function



End Class