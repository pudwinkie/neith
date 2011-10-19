using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Neith.Threading.Tasks
{
    internal abstract class TaskApmWrapperBase<TResult>
    {
        public TResult EndInvoke(IAsyncResult ar)
        {
            var task = ar as Task<TResult>;
            return task.Result;
        }

        protected static IAsyncResult BeginInvokeImpl(Task<TResult> task, AsyncCallback callback, object state)
        {
            var t2 = new Task<TResult>(a => task.Result, state);
            task.ContinueWith(a =>
            {
                t2.RunSynchronously();
                callback(t2);
            });
            if (task.Status == TaskStatus.Created) task.Start();
            return t2;
        }
    }

    internal class TaskApmWrapper<TResult> : TaskApmWrapperBase<TResult>, IApm<TResult>
    {
        private readonly Func<Task<TResult>> TaskFunc;

        public TaskApmWrapper(Func<Task<TResult>> taskFunc)
        {
            TaskFunc = taskFunc;
        }

        public IAsyncResult BeginInvoke(AsyncCallback callback, object state)
        {
            return BeginInvokeImpl(TaskFunc(), callback, state);
        }
    }

    internal class TaskApmWrapper<TArg1, TResult> : TaskApmWrapperBase<TResult>, IApm<TArg1, TResult>
    {
        private readonly Func<TArg1, Task<TResult>> TaskFunc;

        public TaskApmWrapper(Func<TArg1, Task<TResult>> taskFunc)
        {
            TaskFunc = taskFunc;
        }

        public IAsyncResult BeginInvoke(TArg1 arg1, AsyncCallback callback, object state)
        {
            return BeginInvokeImpl(TaskFunc(arg1), callback, state);
        }
    }

    internal class TaskApmWrapper<TArg1, TArg2, TResult> : TaskApmWrapperBase<TResult>, IApm<TArg1, TArg2, TResult>
    {
        private readonly Func<TArg1, TArg2, Task<TResult>> TaskFunc;

        public TaskApmWrapper(Func<TArg1, TArg2, Task<TResult>> taskFunc)
        {
            TaskFunc = taskFunc;
        }

        public IAsyncResult BeginInvoke(TArg1 arg1, TArg2 arg2, AsyncCallback callback, object state)
        {
            return BeginInvokeImpl(TaskFunc(arg1, arg2), callback, state);
        }
    }

    internal class TaskApmWrapper<TArg1, TArg2, TArg3, TResult> : TaskApmWrapperBase<TResult>, IApm<TArg1, TArg2, TArg3, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, Task<TResult>> TaskFunc;

        public TaskApmWrapper(Func<TArg1, TArg2, TArg3, Task<TResult>> taskFunc)
        {
            TaskFunc = taskFunc;
        }

        public IAsyncResult BeginInvoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, AsyncCallback callback, object state)
        {
            return BeginInvokeImpl(TaskFunc(arg1, arg2, arg3), callback, state);
        }
    }

    internal class TaskApmWrapper<TArg1, TArg2, TArg3, TArg4, TResult> : TaskApmWrapperBase<TResult>, IApm<TArg1, TArg2, TArg3, TArg4, TResult>
    {
        private readonly Func<TArg1, TArg2, TArg3, TArg4, Task<TResult>> TaskFunc;

        public TaskApmWrapper(Func<TArg1, TArg2, TArg3, TArg4, Task<TResult>> taskFunc)
        {
            TaskFunc = taskFunc;
        }

        public IAsyncResult BeginInvoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, AsyncCallback callback, object state)
        {
            return BeginInvokeImpl(TaskFunc(arg1, arg2, arg3, arg4), callback, state);
        }
    }

}
