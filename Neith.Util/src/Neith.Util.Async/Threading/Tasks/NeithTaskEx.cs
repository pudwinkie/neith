using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neith.Threading.Tasks
{
    public static class NeithTaskEx
    {
        public static IApm<TResult> ToApm<TResult>(this Func<Task<TResult>> taskFunc)
        {
            return new TaskApmWrapper<TResult>(taskFunc);
        }
        public static IApm<TArg1, TResult> ToApm<TArg1, TResult>(this Func<TArg1, Task<TResult>> taskFunc)
        {
            return new TaskApmWrapper<TArg1, TResult>(taskFunc);
        }
        public static IApm<TArg1, TArg2, TResult> ToApm<TArg1, TArg2, TResult>(this Func<TArg1, TArg2, Task<TResult>> taskFunc)
        {
            return new TaskApmWrapper<TArg1, TArg2, TResult>(taskFunc);
        }
        public static IApm<TArg1, TArg2, TArg3, TResult> ToApm<TArg1, TArg2, TArg3, TResult>(this Func<TArg1, TArg2, TArg3, Task<TResult>> taskFunc)
        {
            return new TaskApmWrapper<TArg1, TArg2, TArg3, TResult>(taskFunc);
        }
        public static IApm<TArg1, TArg2, TArg3, TArg4, TResult> ToApm<TArg1, TArg2, TArg3, TArg4, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, Task<TResult>> taskFunc)
        {
            return new TaskApmWrapper<TArg1, TArg2, TArg3, TArg4, TResult>(taskFunc);
        }


    }
}
