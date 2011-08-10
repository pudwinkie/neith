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
  public static class BoxFields {
    public static void ForEachFlattenFields(Box box, FieldHandlerDelegate handler)
    {
      ForEachFlattenFields(box, typeof(Box), null, handler);
    }

    public static void ForEachFlattenFields(object obj, FieldHandlerDelegate handler)
    {
      ForEachFlattenFields(obj, null, null, handler);
    }

    public static void ForEachFlattenFields(object obj, Type exceptFieldsOf, FieldHandlerDelegate handler)
    {
      ForEachFlattenFields(obj, exceptFieldsOf, null, handler);
    }

    private static void ForEachFlattenFields(object instance, Type exceptFieldsOf, object parentInstance, FieldHandlerDelegate handler)
    {
      var typeOfInstance = instance.GetType();

      if (exceptFieldsOf == null)
        exceptFieldsOf = typeof(object);

      if (typeOfInstance.IsClass) {
        if (!exceptFieldsOf.IsAssignableFrom(typeOfInstance))
          throw new ArgumentException(string.Format("{0} is not assignable from {1}", exceptFieldsOf, typeOfInstance));

        var typeHierarchy = new Stack<Type>();
        var type = instance.GetType();

        while (type != exceptFieldsOf) {
          typeHierarchy.Push(type);
          type = type.BaseType;
        }

        while (0 < typeHierarchy.Count) {
          ForEachFields(instance, typeHierarchy.Pop(), parentInstance, handler);
        }
      }
      else {
        ForEachFields(instance, typeOfInstance, parentInstance, handler);
      }
    }

    private static void ForEachFields(object instance, Type type, object parentInstance, FieldHandlerDelegate fieldHandler)
    {
      var listOfFields = new List<FieldInfo>();
      var declaredFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
      var layoutOfFields = new Dictionary<FieldInfo, FieldLayoutAttribute>(declaredFields.Length);
      var dataTypeOfFields = new Dictionary<FieldInfo, FieldDataTypeAttribute>(declaredFields.Length);
      var handlerOfFields = new Dictionary<FieldInfo, FieldHandlerAttribute>(declaredFields.Length);

      foreach (var fieldInfo in declaredFields) {
        // get attributes of field
        foreach (var attr in fieldInfo.GetCustomAttributes(typeof(FieldAttribute), false)) {
          var attrFieldLayout = attr as FieldLayoutAttribute;

          if (attrFieldLayout != null) {
            var attrFieldVersionSpecificLayout = attrFieldLayout as FieldVersionSpecificLayoutAttribute;

            if (attrFieldVersionSpecificLayout == null) {
              layoutOfFields[fieldInfo] = attrFieldLayout;
            }
            else {
              var fullBox = (instance as FullBox) ?? (parentInstance as FullBox);
              var version = (fullBox == null) ? (byte)0 : fullBox.Version;

              if (attrFieldVersionSpecificLayout.Version <= version)
                layoutOfFields[fieldInfo] = attrFieldVersionSpecificLayout; // replace layout info
            }

            continue;
          }

          var attrFieldDataType = attr as FieldDataTypeAttribute;

          if (attrFieldDataType != null) {
            dataTypeOfFields.Add(fieldInfo, attrFieldDataType);

            continue;
          }

          var attrFieldHandler = attr as FieldHandlerAttribute;

          if (attrFieldHandler != null) {
            handlerOfFields.Add(fieldInfo, attrFieldHandler);

            continue;
          }
        }

        FieldLayoutAttribute layout;

        if (layoutOfFields.TryGetValue(fieldInfo, out layout))
          listOfFields.Insert(layout.Index, fieldInfo);
      }

      for (var fieldIndex = 0; fieldIndex < listOfFields.Count; fieldIndex++) {
        var field = listOfFields[fieldIndex];
        var layout = layoutOfFields[field];

        FieldDataTypeAttribute dataType;
        FieldHandlerAttribute handler;

        if (!dataTypeOfFields.TryGetValue(field, out dataType))
          dataType = null;
        if (!handlerOfFields.TryGetValue(field, out handler))
          handler = null;

        var context = FieldHandlingContext.Create(instance, field, layout, dataType, handler);
        var collectionContext = context as CollectionFieldHandlingContext;

        try {
          if (collectionContext != null) {
            if (collectionContext.FieldType.IsArray && 1 < context.FieldType.GetArrayRank())
              throw new NotSupportedException("array with rank != 1 is not supported");

            fieldHandler(context);

            // for each element of array
            collectionContext.IsElementOfCollection = true;
            collectionContext.FieldType = collectionContext.ElementType;

            for (collectionContext.ElementIndex = 0; ; collectionContext.ElementIndex++) {
              if (collectionContext.ElementCount.HasValue && collectionContext.ElementCount.Value <= collectionContext.ElementIndex)
                break;

              fieldHandler(context);
            }
          }
          else {
            fieldHandler(context);
          }
        }
        catch (Exception ex) {
          throw new FieldHandlingException(context, ex);
        }
      }
    }

    internal static void ReadFields(object instance, BoxFieldReader reader)
    {
      ReadFields(instance, null, reader);
    }

    private static void ReadFields(object instance, object parentInstance, BoxFieldReader reader)
    {
      Type expectFieldsOf = (instance is UserExtensionBox) ? typeof(UserExtensionBox) : // to ignore uuid field
                            (instance is Box) ? typeof(Box) :
                            null;

      ForEachFlattenFields(instance, expectFieldsOf, parentInstance, delegate(FieldHandlingContext context) {
        if (context.ReadHandler != null) {
          context.ReadHandler(reader, context);
          return; // continue;
        }

        // Array(length-specified array) or IList(length-unspecified array)
        var collectionContext = context as CollectionFieldHandlingContext;

        if (collectionContext != null) {
          if (!collectionContext.IsElementOfCollection) {
            collectionContext.InitializeCollection();
            return; // continue;
          }

          if (collectionContext.IsElementOfCollection && !collectionContext.ElementCount.HasValue && reader.BaseStream.Length <= reader.BaseStream.Position) {
            // end of stream reached while reading an element of length-unspecified array
            collectionContext.ElementCount = -1;
            return; // continue;
          }
        }

        // read field data
        if (context.IsStructuredField) {
          var fieldValue = Activator.CreateInstance(context.FieldType);

          ReadFields(fieldValue, instance, reader);

          context.SetValue(fieldValue);
        }
        else {
          context.SetValue(reader.ReadField(context));
        }
      });
    }

    internal static void WriteFields(object instance, BoxFieldWriter writer)
    {
      WriteFields(instance, null, writer);
    }

    private static void WriteFields(object instance, object parentInstance, BoxFieldWriter writer)
    {
      Type expectFieldsOf = (instance is Box) ? typeof(Box) : null;

      ForEachFlattenFields(instance, expectFieldsOf, parentInstance, delegate(FieldHandlingContext context) {
        try {
          if (context.WriteHandler != null) {
            context.WriteHandler(writer, context);
            return; // continue;
          }

          var collectionContext = context as CollectionFieldHandlingContext;

          if (collectionContext != null && !collectionContext.IsElementOfCollection) {
            // get length of Array(length-specified array) or IList(length-unspecified array)
            if (!collectionContext.ElementCount.HasValue)
              collectionContext.ElementCount = collectionContext.GetActualElementCount();
            return; // continue;
          }

          // read field data
          if (context.IsStructuredField)
            WriteFields(context.GetValue(), instance, writer);
          else
            writer.WriteField(context);
        }
        catch (Exception ex) {
          throw new FormatException(string.Format("invalid format at {0}", context), ex);
        }
      });
    }
  }
}
