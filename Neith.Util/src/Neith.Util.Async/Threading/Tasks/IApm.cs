using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Neith.Threading.Tasks
{
    public interface IApm<TResult>
    {
        IAsyncResult BeginInvoke(AsyncCallback callback, object state);
        TResult EndInvoke(IAsyncResult ar);
    }

    public interface IApm<TArg1, TResult>
    {
        IAsyncResult BeginInvoke(TArg1 arg1, AsyncCallback callback, object state);
        TResult EndInvoke(IAsyncResult ar);
    }

    public interface IApm<TArg1, TArg2, TResult>
    {
        IAsyncResult BeginInvoke(TArg1 arg1, TArg2 arg2, AsyncCallback callback, object state);
        TResult EndInvoke(IAsyncResult ar);
    }

    public interface IApm<TArg1, TArg2, TArg3, TResult>
    {
        IAsyncResult BeginInvoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, AsyncCallback callback, object state);
        TResult EndInvoke(IAsyncResult ar);
    }

    public interface IApm<TArg1, TArg2, TArg3, TArg4, TResult>
    {
        IAsyncResult BeginInvoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, AsyncCallback callback, object state);
        TResult EndInvoke(IAsyncResult ar);
    }

}
