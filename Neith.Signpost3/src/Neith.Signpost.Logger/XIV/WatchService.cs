using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Neith.Signpost.Logger;
using Neith.Signpost.Logger.Model;

namespace Neith.Signpost.Logger.XIV
{
    public class WatchService : IDisposable, ISourceBlock<NeithLog>
    {
        private CompositeDisposable Tasks = new CompositeDisposable();
        public void Dispose()
        {
            Tasks.Dispose();
        }

        public WatchService()
        {


        }



        #region ISourceBlock<NeithLog> メンバー

        public NeithLog ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<NeithLog> target, out bool messageConsumed)
        {
            throw new NotImplementedException();
        }

        public IDisposable LinkTo(ITargetBlock<NeithLog> target, bool unlinkAfterOne)
        {
            throw new NotImplementedException();
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<NeithLog> target)
        {
            throw new NotImplementedException();
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<NeithLog> target)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDataflowBlock メンバー

        public void Complete()
        {
            throw new NotImplementedException();
        }

        public Task Completion
        {
            get { throw new NotImplementedException(); }
        }

        public void Fault(Exception exception)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
