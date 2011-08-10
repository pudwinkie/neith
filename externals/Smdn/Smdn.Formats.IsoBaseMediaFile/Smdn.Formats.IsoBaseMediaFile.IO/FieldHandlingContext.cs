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
using System.Linq;
using System.Reflection;

namespace Smdn.Formats.IsoBaseMediaFile.IO {
  public abstract class FieldHandlingContext {
    public object Instance {
      get; private set;
    }

    public FieldInfo FieldInfo {
      get; internal set;
    }

    public int FieldIndex {
      get; private set;
    }

    public int FieldSize { // in bits
      get; private set;
    }

    public Type FieldType {
      get; internal set;
    }

    public FieldDataType FieldDataType {
      get; internal set;
    }

    public bool IsStructuredField {
      get; private set;
    }

    public FieldReadHandlerDelegate ReadHandler {
      get; private set;
    }

    public FieldWriteHandlerDelegate WriteHandler {
      get; private set;
    }

    internal static FieldHandlingContext Create(object instance, FieldInfo fieldInfo, FieldLayoutAttribute layout, FieldDataTypeAttribute dataType, FieldHandlerAttribute handler)
    {
      if (typeof(IList).IsAssignableFrom(fieldInfo.FieldType)) {
        if (fieldInfo.FieldType.IsArray) {
          return new ArrayFieldHandlingContext(instance, fieldInfo, layout, dataType, handler);
        }
        else {
          var genericListInterface = fieldInfo.FieldType.FindInterfaces(delegate(Type t, object typeCriteria) {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>);
          }, null).FirstOrDefault();

          if (genericListInterface == null)
            throw new NotSupportedException(string.Format("unsupported field type: {0}", fieldInfo.FieldType));

          return new ListFieldHandlingContext(instance, fieldInfo, genericListInterface.GetGenericArguments()[0], layout, dataType, handler);
        }
      }
      else {
        return new DefaultFieldHandlingContext(instance, fieldInfo, layout, dataType, handler);
      }
    }

    internal protected FieldHandlingContext(object instance,
                                            FieldInfo fieldInfo,
                                            Type elementType,
                                            FieldLayoutAttribute layout,
                                            FieldDataTypeAttribute dataType,
                                            FieldHandlerAttribute handler)
    {
      Instance = instance;
      FieldInfo = fieldInfo;
      FieldIndex = layout.Index;
      FieldSize = layout.GetSizeInBits(instance);
      FieldType = fieldInfo.FieldType;

      if (elementType == null)
        IsStructuredField = !FieldType.IsPrimitive && Attribute.IsDefined(FieldType, typeof(FieldStructureAttribute));
      else
        IsStructuredField = !elementType.IsPrimitive && Attribute.IsDefined(elementType, typeof(FieldStructureAttribute));

      if (dataType == null)
        FieldDataType = FieldDataType.Default;
      else
        FieldDataType = dataType.Type;

      if (handler == null) {
        ReadHandler  = null;
        WriteHandler = null;
      }
      else {
        ReadHandler  = handler.GetReadHandler(instance.GetType());
        WriteHandler = handler.GetWriteHandler(instance.GetType());
      }
    }

    public abstract object GetValue();
    public abstract void SetValue(object @value);

    public override string ToString()
    {
      var col = this as CollectionFieldHandlingContext;

      return string.Format("TypeOfInstance: #{0}, FieldName: #{1}, FieldSize: {2}, FieldType: {3}({4}{5}){6}{7}{8}}}",
                           Instance.GetType().FullName,
                           FieldInfo.Name,
                           FieldSize,
                           FieldType,
                           (col == null)
                             ? string.Empty
                             : string.Format("{0}, ", FieldInfo.FieldType),
                           FieldDataType,
                           (col == null)
                             ? string.Empty
                             : string.Format(", Index: {0}/{1} IsElementOfCollection: {2}",
                                         col.ElementIndex,
                                         (col.ElementCount.HasValue) ? col.ElementCount.Value.ToString() : "?",
                                         col.IsElementOfCollection),
                           ReadHandler == null
                             ? string.Empty
                             : string.Format(", ReadHandler: {0}", ReadHandler.Method),
                           WriteHandler == null
                             ? string.Empty
                             : string.Format(", WriteHandler: {0}", WriteHandler.Method)
                           );
    }
  }
}
