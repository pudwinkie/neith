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
using System.IO;
using System.Collections.Generic;

using Smdn.Formats.IsoBaseMediaFile.IO;
using Smdn.IO;

namespace Smdn.Formats.IsoBaseMediaFile {
  public class Box : IDisposable {
    public static class TypeFourCC {
      public static readonly FourCC Uuid = new FourCC("uuid");
      public static readonly FourCC Mdat = new FourCC("mdat");
      public static readonly FourCC Ftyp = new FourCC("ftyp");
    }

    public static class FtypFourCC {
      public static readonly FourCC Isom = new FourCC("isom");
      public static readonly FourCC Iso2 = new FourCC("iso2");
      public static readonly FourCC Avc1 = new FourCC("avc1");
      public static readonly FourCC MP21 = new FourCC("MP21");
      public static readonly FourCC MP41 = new FourCC("MP41");
      public static readonly FourCC MP42 = new FourCC("MP42");
      public static readonly FourCC MP71 = new FourCC("MP71");
    }

#region "instantiation"
#region "Box"
    public static Box Create(FourCC type)
    {
      return Create(type, null);
    }

    public static TBox Create<TBox>(FourCC type) where TBox : Box
    {
      return Create<TBox>(KnownBox.GetTypeOf(type), typeof(TBox));
    }

    public static Box Create(FourCC type, Type expectedBoxType)
    {
      return Create<Box>(KnownBox.GetTypeOf(type), expectedBoxType);
    }
#endregion

#region "UserExtensionBox"
    public static UserExtensionBox Create(Uuid userType)
    {
      return Create(userType, null);
    }

    public static TUserExtensionBox Create<TUserExtensionBox>(Uuid userType) where TUserExtensionBox : UserExtensionBox
    {
      return Create<TUserExtensionBox>(KnownBox.GetTypeOf(userType), typeof(TUserExtensionBox));
    }

    public static UserExtensionBox Create(Uuid userType, Type expectedBoxType)
    {
      return Create<UserExtensionBox>(KnownBox.GetTypeOf(userType), expectedBoxType);
    }
#endregion

    private static TBox Create<TBox>(Type actualType, Type expectedType) where TBox : Box
    {
      if (actualType == null)
        actualType = typeof(TBox);

      if (expectedType != null && !expectedType.IsAssignableFrom(actualType))
        throw new InvalidCastException(string.Format("type {0} is not assignable to expected type {1}", actualType, expectedType));

      var defaultConstructor = actualType.GetConstructor(System.Type.EmptyTypes);

      if (defaultConstructor == null)
        throw new InvalidOperationException(string.Format("type {0} has no default constructor", actualType));

      return (TBox)defaultConstructor.Invoke(null);
    }
#endregion

#region "enumeration and listing"
    public static bool Contains(IBoxContainer container, Box box)
    {
      return Find(container, delegate(Box b) {
        return (b == box);
      }) != null;
    }

    public static bool Exists(IBoxContainer container, Predicate<Box> match)
    {
      return Find(container, match) != null;
    }

    public static bool Exists(IBoxContainer container, Type boxType)
    {
      return Find(container, boxType) != null;
    }

    public static Box Find(IBoxContainer container, Predicate<Box> match)
    {
      if (container == null)
        throw new ArgumentNullException("container");
      if (match == null)
        throw new ArgumentNullException("match");

      foreach (var box in container.Boxes) {
        if (box != null && match(box))
          return box;
      }

      return null;
    }

    public static TBox Find<TBox>(IBoxContainer container) where TBox : Box
    {
      return Find(container, typeof(TBox)) as TBox;
    }

    public static Box Find(IBoxContainer container, Type boxType)
    {
      return Find(container, delegate(Box box) {
        return (boxType.IsAssignableFrom(box.GetType()));
      });
    }

    public static IList<Box> FindAll(IBoxContainer container, Predicate<Box> match)
    {
      if (container == null)
        throw new ArgumentNullException("container");
      if (match == null)
        throw new ArgumentNullException("match");

      var found = new List<Box>();

      foreach (var box in container.Boxes) {
        if (box != null && match(box))
          found.Add(box);
      }

      return found;
    }

    public static IList<Box> FindAll(IBoxContainer container, Type boxType)
    {
      return FindAll(container, delegate(Box box) {
        return (boxType.IsAssignableFrom(box.GetType()));
      });
    }

    public static Box FindLast(IBoxContainer container, Predicate<Box> match)
    {
      if (container == null)
        throw new ArgumentNullException("container");
      if (match == null)
        throw new ArgumentNullException("match");

      Box found = null;

      foreach (var box in container.Boxes) {
        if (box != null && match(box))
          found = box;
      }

      return found;
    }

    public static TBox FindLast<TBox>(IBoxContainer container) where TBox : Box
    {
      return FindLast(container, typeof(TBox)) as TBox;
    }

    public static Box FindLast(IBoxContainer container, Type boxType)
    {
      return FindLast(container, delegate(Box box) {
        return (boxType.IsAssignableFrom(box.GetType()));
      });
    }

    public static void ForEach(IBoxContainer container, Action<Box> action)
    {
      if (container == null)
        throw new ArgumentNullException("container");
      if (action == null)
        throw new ArgumentNullException("action");

      foreach (var box in container.Boxes) {
        if (box == null)
          continue;
        action(box);
      }
    }

    public static Box[] ToArray(IBoxContainer container)
    {
      return (new List<Box>(container.Boxes)).ToArray();
    }

    public static bool TrueForAll(IBoxContainer container, Predicate<Box> match)
    {
      if (container == null)
        throw new ArgumentNullException("container");
      if (match == null)
        throw new ArgumentNullException("match");

      foreach (var box in container.Boxes) {
        if (box != null && !match(box))
          return false;
      }

      return true;
    }
#endregion

    internal void UpdateNestOffset(ulong containerOffset)
    {
      UpdateNestOffset(this, containerOffset);
    }

    internal static void UpdateNestOffset(Box box, ulong containerOffset)
    {
      if (box == null)
        return;

      box.Offset += containerOffset;

      if (box is IBoxContainer) {
        foreach (var containedBox in (box as IBoxContainer).Boxes) {
          UpdateNestOffset(containedBox, containerOffset);
        }
      }
    }

    internal void ReadDataBlockIntoMemory()
    {
      ReadDataBlockIntoMemory(this);
    }

    internal static void ReadDataBlockIntoMemory(Box box)
    {
      if (box == null)
        return;

      if (box.UndefinedFieldData != null)
        box.UndefinedFieldData.ReadIntoMemory();

      BoxFields.ForEachFlattenFields(box, delegate(FieldHandlingContext context) {
        var collectionContext = context as CollectionFieldHandlingContext;

        if (collectionContext != null && !collectionContext.IsElementOfCollection) {
          // get length of Array(length-specified array) or IList(length-unspecified array)
          if (!collectionContext.ElementCount.HasValue)
            collectionContext.ElementCount = collectionContext.GetActualElementCount();
          return; // continue;
        }

        if (context.FieldType == typeof(DataBlock)) {
          var dataBlock = (context.GetValue() as DataBlock);

          if (dataBlock != null)
            dataBlock.ReadIntoMemory();
        }
        else if (typeof(Box).IsAssignableFrom(context.FieldType)) {
          ReadDataBlockIntoMemory(context.GetValue() as Box);
        }
      });
    }

#region "instance members"
    public ulong Size {
      get { CheckDisposed(); return size; }
      internal set { CheckDisposed(); size = value; }
    }

    public FourCC Type {
      get { CheckDisposed(); return type; }
      protected internal set { CheckDisposed(); type = value; }
    }

    public ulong Offset {
      get { CheckDisposed(); return offset; }
      protected internal set { CheckDisposed(); offset = value; }
    }

    public DataBlock UndefinedFieldData {
      get { CheckDisposed(); return undefinedFieldData; }
      set { CheckDisposed(); undefinedFieldData = value; }
    }

    internal protected IBoxContainer ContainedIn {
      get { CheckDisposed(); return containedIn; }
      internal set { CheckDisposed(); containedIn = value; }
    }

    public Box()
    {
      var typeOfThis = GetType();

      if (typeof(UserExtensionBox).IsAssignableFrom(typeOfThis))
        type = TypeFourCC.Uuid;
      else if (typeOfThis == typeof(Box))
        type = FourCC.Empty;
      else
        type = KnownBox.GetTypeOf(typeOfThis);
    }

    ~Box()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing) {
        if (undefinedFieldData != null) {
          undefinedFieldData.Dispose();
          undefinedFieldData = null;
        }
      }

      disposed = true;
    }

    public override string ToString()
    {
      return string.Format("Box{{Type='{0}', Size={1}, Offset={2}}}", type, size, offset);
    }

    private void CheckDisposed()
    {
      if (disposed)
        throw new ObjectDisposedException(GetType().FullName);
    }

    private bool disposed = false;
    private ulong size = 0; // size or largesize(if size = 1)
    private FourCC type = FourCC.Empty;
    private ulong offset = 0;
    private DataBlock undefinedFieldData = null;
    private IBoxContainer containedIn = null;
#endregion
  }
}
