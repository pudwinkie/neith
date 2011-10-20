using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using System.Threading.Tasks;

namespace Neith.Signpost.Services.Flows
{
    /// <summary>
    /// 要求情報の仕分けを行います。
    /// </summary>
    public class FlowDispatcher : ITargetBlock<IFlowItem>
    {
        /// <summary>
        /// 処理ブロックを登録します。
        /// </summary>
        /// <param name="target"></param>
        public void Add(IFlowTargetBlock target)
        {
            lock (BlockDic) {
                var blocks = BlockDic[target.DispatchName];
                if (blocks == null) blocks = new Dictionary<Guid, IFlowTargetBlock>();
                blocks.Add(target.Guid, target);
            }
        }

        /// <summary>
        /// 処理ブロックを開放します。
        /// </summary>
        /// <param name="target"></param>
        public void Remove(IFlowTargetBlock target)
        {
            lock (BlockDic) {
                var name = target.DispatchName;
                var blocks = BlockDic[name];
                if (blocks == null) throw new ArgumentException(string.Format("DispatchName=[{0}] not found", name), "target");
                if (!blocks.Remove(target.Guid))
                    throw new ArgumentException(string.Format("Guid=[{0}] not found", target.Guid), "target");
                if (blocks.Count == 0) BlockDic.Remove(name);
            }
        }

        private readonly ITargetBlock<IFlowItem> DispatchAction;
        private readonly Dictionary<string, Dictionary<Guid, IFlowTargetBlock>> BlockDic = new Dictionary<string, Dictionary<Guid, IFlowTargetBlock>>();

        public FlowDispatcher()
        {
            var opt = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = int.MaxValue,
            };
            DispatchAction = new ActionBlock<IFlowItem>(item =>
            {
                try {
                    var blocks = BlockDic[item.To];
                    if (blocks == null) return;
                    foreach (var block in blocks.Values) block.Post(item);
                }
                catch (Exception ex) {
                    Debug.WriteLine(ex);
                }
            }, opt);
        }


        #region ITargetBlock<IFlowItem> メンバー

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, IFlowItem messageValue, ISourceBlock<IFlowItem> source, bool consumeToAccept)
        {
            return DispatchAction.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        #endregion

        #region IDataflowBlock メンバー

        public void Complete()
        {
            DispatchAction.Complete();
        }

        public System.Threading.Tasks.Task Completion { get { return DispatchAction.Completion; } }

        public void Fault(Exception exception)
        {
            DispatchAction.Fault(exception);
        }

        #endregion
    }
}
