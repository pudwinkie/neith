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
using System.Collections.Generic;
using System.Reflection;

namespace Smdn.Formats.IsoBaseMediaFile {
  public static class KnownBox {
    static KnownBox()
    {
      foreach (var assm in AppDomain.CurrentDomain.GetAssemblies()) {
        RegisterBoxes(assm);
      }
    }

    public static void RegisterBoxes(Type typeOfAssembly)
    {
      if (typeOfAssembly == null)
        throw new ArgumentNullException("typeOfAssembly");

      RegisterBoxes(typeOfAssembly.Assembly);
    }

    public static void RegisterBoxes(Assembly assembly)
    {
      if (assembly == null)
        throw new ArgumentNullException("assembly");

      var boxBaseType = typeof(Box);

      foreach (var type in assembly.GetExportedTypes()) {
        if (!type.IsSubclassOf(boxBaseType))
          continue;

        foreach (var attr in type.GetCustomAttributes(typeof(BoxAttribute), false)) {
          var attrBoxType = attr as BoxTypeAttribute;

          if (attrBoxType != null) {
            typeToFourccDictionary[type] = attrBoxType.Types[0]; // overwrite

            foreach (var fourcc in attrBoxType.Types) {
              fourccToTypeDictionary[fourcc] = type; // overwrite
            }

            continue;
          }

          var attrUserType = attr as UserTypeBoxAttribute;

          if (attrUserType != null)
            uuidToTypeDictionary[attrUserType.UserType] = type; // overwrite
        }
      }
    }

    /*
    public static IEnumerable<FourCC> ContainedBy(FourCC boxType)
    {
      var containedBy = new List<FourCC>();

      foreach (var pair in knownBoxDefinitionDictionary) {
        if (pair.Value.Container == boxType)
          containedBy.Add(pair.Value.Type);
      }

      return containedBy;
    }
    */

    public static FourCC GetTypeOf(Type type)
    {
      FourCC ret;

      if (typeToFourccDictionary.TryGetValue(type, out ret))
        return ret;
      else
        return FourCC.Empty;
    }

    public static Uuid GetUserTypeOf(Type type)
    {
      foreach (var pair in uuidToTypeDictionary) {
        if (pair.Value == type)
          return pair.Key;
      }

      return Uuid.Nil;
    }

    public static Type GetTypeOf(FourCC boxType)
    {
      Type ret;

      if (fourccToTypeDictionary.TryGetValue(boxType, out ret))
        return ret;
      else
        return null;
    }

    public static Type GetTypeOf(Uuid userType)
    {
      Type ret;

      if (uuidToTypeDictionary.TryGetValue(userType, out ret))
        return ret;
      else
        return null;
    }

    private static readonly Dictionary<Type, FourCC> typeToFourccDictionary = new Dictionary<Type, FourCC>();
    private static readonly Dictionary<FourCC, Type> fourccToTypeDictionary = new Dictionary<FourCC, Type>();
    private static readonly Dictionary<Uuid, Type> uuidToTypeDictionary = new Dictionary<Uuid, Type>();
  }
}
