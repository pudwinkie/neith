using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neith.Logger.Model;

namespace Neith.Logger.XIV
{
    public class XIVCollecter : ICollector
    {
        public string Name { get { return "XIVCollecter"; } }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IObservable<Log> RxCollect()
        {

            throw new NotImplementedException();
        }

    }
}
