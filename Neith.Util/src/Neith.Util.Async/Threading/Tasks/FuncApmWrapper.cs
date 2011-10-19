using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Threading.Tasks
{
    internal class FuncApmWrapper<TResult> : IApm<TResult>
    {
        private readonly Func<TResult> ApmFunc;

        public FuncApmWrapper(Func<TResult> func)
        {
            ApmFunc = func;
        }

        public IAsyncResult BeginInvoke(AsyncCallback callback, object state)
        {
            return ApmFunc.BeginInvoke(callback, state);
        }

        public TResult EndInvoke(IAsyncResult ar)
        {
            return ApmFunc.EndInvoke(ar);
        }
    }

}
