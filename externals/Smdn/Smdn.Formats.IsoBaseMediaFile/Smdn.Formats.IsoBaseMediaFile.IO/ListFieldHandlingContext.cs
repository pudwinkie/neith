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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Smdn.Formats.IsoBaseMediaFile.IO {
  internal class ListFieldHandlingContext : CollectionFieldHandlingContext {
    internal ListFieldHandlingContext(object instance,
                                      FieldInfo fieldInfo,
                                      Type elementType,
                                      FieldLayoutAttribute layout,
                                      FieldDataTypeAttribute dataType,
                                      FieldHandlerAttribute handler)
      : base(instance, fieldInfo, elementType, layout, dataType, handler)
    {
    }

    private IList GetList(bool throwIfNull)
    {
      if (list == null) {
        list = (FieldInfo.GetValue(Instance) as IList);

        if (throwIfNull && list == null)
          throw new InvalidOperationException("field is null");
      }

      return list;
    }

    internal override void InitializeCollection()
    {
      GetList(true).Clear();
    }

    internal override int GetActualElementCount()
    {
      return GetList(true).Count;
    }

    public override object GetValue()
    {
      if (IsElementOfCollection)
        return GetList(true)[(int)ElementIndex];
      else
        return GetList(false);
    }

    public override void SetValue(object val)
    {
      if (IsElementOfCollection) {
        // as Item_set
        var l = GetList(true);

        if (ElementIndex < l.Count)
          l.RemoveAt((int)ElementIndex);

        l.Insert((int)ElementIndex, val);
      }
      else {
        FieldInfo.SetValue(Instance, val);

        list = null;
      }
    }

    private IList list;
  }
}

