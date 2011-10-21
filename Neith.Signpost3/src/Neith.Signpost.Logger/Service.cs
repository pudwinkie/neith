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
        }




        public static Service Instance { get; private set; }
        static Service()
        {
            Instance = new Service();
        }
    }
}
