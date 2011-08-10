// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2009-2011 smdn
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace Smdn {
  /*
   * System.Action* is available from .NET Framework 3.5
   */
#if !NET_3_5
  public delegate void Action
    ();
  public delegate void Action<T1, T2>
    (T1 arg1, T2 arg2);
  public delegate void Action<T1, T2, T3>
    (T1 arg1, T2 arg2, T3 arg3);
  public delegate void Action<T1, T2, T3, T4>
    (T1 arg1, T2 arg2, T3 arg3, T4 arg4);
#endif

#if !NET_4_0
  public delegate void Action<T1, T2, T3, T4, T5>
    (T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
  public delegate void Action<T1, T2, T3, T4, T5, T6>
    (T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
  public delegate void Action<T1, T2, T3, T4, T5, T6, T7>
    (T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
  public delegate void Action<T1, T2, T3, T4, T5, T6, T7, T8>
    (T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
  //public delegate void Action<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15, in T16>
  //(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16   )

#endif
}
