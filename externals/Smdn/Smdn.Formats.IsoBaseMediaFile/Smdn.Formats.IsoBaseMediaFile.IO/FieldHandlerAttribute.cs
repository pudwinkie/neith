// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2008-2011 smdn
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

using System;
using System.Reflection;

namespace Smdn.Formats.IsoBaseMediaFile.IO {
  [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
  public class FieldHandlerAttribute : FieldAttribute {
    public FieldHandlerAttribute(string readHandlerName, string writeHandlerName)
    {
      this.readHandlerName = readHandlerName;
      this.writeHandlerName = writeHandlerName;
    }

    public FieldReadHandlerDelegate GetReadHandler(Type type)
    {
      return GetHandler<FieldReadHandlerDelegate>(type, readHandlerName);
    }

    public FieldWriteHandlerDelegate GetWriteHandler(Type type)
    {
      return GetHandler<FieldWriteHandlerDelegate>(type, writeHandlerName);
    }

    private THandlerDelegate GetHandler<THandlerDelegate>(Type type, string handlerName) where THandlerDelegate : class
    {
      var methodInfo = type.GetMethod(handlerName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);

      if (methodInfo == null)
        return null;

      return Delegate.CreateDelegate(typeof(THandlerDelegate), methodInfo) as THandlerDelegate;
    }

    private readonly string readHandlerName;
    private readonly string writeHandlerName;
  }
}
