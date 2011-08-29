using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;

using X = NetflixApp.Model.ODataXML;

namespace NetflixApp.Model
{
    public class NetflixQuery<T> where T : NetflixEntity, new()
    {
        public NetflixQuery(string query)
        {
            Query = query;
            Entities = new ObservableCollection<T>();
            EntitiesExpected = null;
        }

        public string Query { get; private set; }
        public ObservableCollection<T> Entities { get; private set; }
        public int? EntitiesExpected { get; private set; }

        WebClient client = new WebClient();

        public async Task FetchEntitiesAsync(CancellationToken cancel)
        {
            string next = Query;

            // No try block -- cancellation and errors bubble up to the caller naturally
            while (next != null)
            {
                var result = XDocument.Parse(await client.DownloadStringTaskAsync(new Uri(next), cancel));

                var countElement = result.Descendants(X.mName("count"))
                         .SingleOrDefault() as XElement;
                if (countElement != null)
                {
                    int itemsTotal = int.Parse(countElement.Value);
                    EntitiesExpected = itemsTotal;
                }

                var entries = result.Descendants(X.Name("entry"));
                foreach (var entry in entries)
                {
                    T entity = new T();
                    entity.LoadFromXML(entry);
                    Entities.Add(entity);
                }

                next = GetNextUri(result);
            }

            EntitiesExpected = Entities.Count;
        }

        private string GetNextUri(XDocument xml)
        {
            return (from elem in xml.Element(X.Name("feed")).Elements(X.Name("link"))
                    where elem.Attribute("rel").Value == "next"
                    select elem.Attribute("href").Value).SingleOrDefault();
        }

        // No CancelAsync method needed.  Cancellation requests flow from
        // ViewModel -> Model -> the underlying async API via the CancellationToken.
    }
}
