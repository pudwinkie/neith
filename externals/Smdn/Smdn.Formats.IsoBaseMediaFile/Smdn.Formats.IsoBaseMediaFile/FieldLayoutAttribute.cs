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

namespace Smdn.Formats.IsoBaseMediaFile {
  [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
  public class FieldLayoutAttribute : FieldAttribute {
    public int Index {
      get { return index; }
    }

    public object SizeInBits {
      /*protected*/ get { return sizeInBitsValueOrFieldName; }
      set { sizeInBitsValueOrFieldName = value; }
    }

    public object SizeInBytes {
      /*protected*/ get { return sizeInBytesValueOrFieldName; }
      set { sizeInBytesValueOrFieldName = value; }
    }

    public object Count {
      /*protected*/ get { return countValueOrFieldName; }
      set { countValueOrFieldName = value; }
    }

    public FieldLayoutAttribute(int index)
    {
      this.index = index;
      this.sizeInBitsValueOrFieldName = null;
    }

    public FieldLayoutAttribute(int index, int sizeInBits)
    {
      this.index = index;
      this.sizeInBitsValueOrFieldName = sizeInBits;
    }

    public int GetSizeInBits(object instance)
    {
      if (sizeInBitsValueOrFieldName == null && sizeInBytesValueOrFieldName == null)
        throw new MemberAccessException("field name which contains size is not specified");

      if (sizeInBitsValueOrFieldName != null)
        return (int)GetLong(instance, sizeInBitsValueOrFieldName);
      else
        return (int)GetLong(instance, sizeInBytesValueOrFieldName) * 8;
    }

    public long? GetCount(object instance)
    {
      if (countValueOrFieldName == null)
        return null;
      else
        return GetLong(instance, countValueOrFieldName);
    }

    private long GetLong(object instance, object valueOrFieldName)
    {
      if (valueOrFieldName is int || valueOrFieldName is uint || valueOrFieldName is long) {
        return Convert.ToInt64(valueOrFieldName);
      }
      else if (valueOrFieldName is string) {
        var typeOfInstance = instance.GetType();
        var fieldName = valueOrFieldName as string;

        foreach (var field in typeOfInstance.GetFields(BindingFlags.Instance | BindingFlags.Public)) {
          if (field.Name == fieldName)
            return Convert.ToInt64(field.GetValue(instance));
        }
      }

      throw new MissingFieldException(string.Format("field '{0}' was not found", valueOrFieldName));
    }

    private readonly int index;
    private object sizeInBytesValueOrFieldName = null;
    private object sizeInBitsValueOrFieldName = null;
    private object countValueOrFieldName = null;
  }
}
