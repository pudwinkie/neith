using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace Neith.Signpost.Services.Flows
{
    public interface IFlowTargetBlock : ITargetBlock<IFlowItem>
    {
        /// <summary>Guid。</summary>
        Guid Guid { get; }

        /// <summary>モジュール名。</summary>
        string Name { get; }

        /// <summary>要求振り分け名。</summary>
        string DispatchName { get; }
    }
}
