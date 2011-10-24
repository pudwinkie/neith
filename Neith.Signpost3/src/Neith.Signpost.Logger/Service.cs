using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Windows.Threading;

namespace Neith.Signpost.Logger
{
    public class Service : IDisposable
    {
        private CompositeDisposable Tasks = new CompositeDisposable();
        public void Dispose()
        {
            Tasks.Dispose();
        }

        private readonly Dispatcher Dispatcher;

        private Service(Dispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            LogDBService.Instance.Add(Tasks);
            var watch = new XIV.WatchService(Dispatcher).Add(Tasks);
            watch.LinkTo(LogDBService.Instance, false).Add(Tasks);
        }

        public static Service Instance { get; private set; }
        static Service()
        {
            Instance = new Service(System.Windows.Application.Current.Dispatcher);
        }
    }
}
