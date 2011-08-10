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
  internal class ArrayFieldHandlingContext : CollectionFieldHandlingContext {
    internal ArrayFieldHandlingContext(object instance,
                                       FieldInfo fieldInfo,
                                       FieldLayoutAttribute layout,
                                       FieldDataTypeAttribute dataType,
                                       FieldHandlerAttribute handler)
      : base(instance, fieldInfo, fieldInfo.FieldType.GetElementType(), layout, dataType, handler)
    {
    }

    private Array GetArray(bool throwIfNull)
    {
      if (array == null) {
        array = FieldInfo.GetValue(Instance) as Array;

        if (throwIfNull && array == null)
          throw new InvalidOperationException("field is null");
      }

      return array;
    }

    internal override void InitializeCollection()
    {
      if (!ElementCount.HasValue)
        throw new InvalidOperationException("ElementCount must be specified with Array field");

      array = Array.CreateInstance(ElementType, ElementCount.Value);

      FieldInfo.SetValue(Instance, array);
    }

    internal override int GetActualElementCount()
    {
      return GetArray(true).Length;
    }

    public override object GetValue()
    {
      if (IsElementOfCollection)
        return GetArray(true).GetValue(ElementIndex);
      else
        return GetArray(false);
    }

    public override void SetValue(object val)
    {
      if (IsElementOfCollection) {
        GetArray(true).SetValue(val, ElementIndex);
      }
      else {
        FieldInfo.SetValue(Instance, val);

        array = null;
      }
    }

    private Array array;
  }
}

