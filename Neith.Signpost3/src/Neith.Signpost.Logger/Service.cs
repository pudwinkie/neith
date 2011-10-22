using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Disposables;

namespace Neith.Signpost.Logger
{
    public class Service : IDisposable
    {
        private CompositeDisposable Tasks = new CompositeDisposable();
        public void Dispose()
        {
            Tasks.Dispose();
        }

        private Service()
        {
            LogDBService.Instance.Add(Tasks);
            var watch = new XIV.WatchService().Add(Tasks);
            watch.LinkTo(LogDBService.Instance, false).Add(Tasks);
        }




        public static Service Instance { get; private set; }
        static Service()
        {
            Instance = new Service();
        }
    }
}
