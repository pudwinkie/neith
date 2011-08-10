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
using System.IO;

using Smdn.Formats.IsoBaseMediaFile.IO;

namespace Smdn.Formats.IsoBaseMediaFile {
  public class MediaFile : IBoxContainer, IDisposable {
    public IEnumerable<Box> Boxes {
      get { CheckDisposed(); return boxes; }
    }

    public Stream BaseStream {
      get { CheckDisposed(); return stream; }
    }

    public MediaFile()
      : this(new Box[] {})
    {
    }

    public MediaFile(IEnumerable<Box> boxes)
    {
      this.boxes = new BoxList(this, boxes);
    }

    public MediaFile(string file)
      : this(file, false)
    {
    }

    public MediaFile(string file, bool openAsWritable)
      : this(openAsWritable
              ? File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.Read)
              : File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
    {
    }

    public MediaFile(Stream stream)
      : this()
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      this.stream = stream;

      ReadFrom(stream);
    }

    ~MediaFile()
    {
      Dispose(false);
    }

    void IDisposable.Dispose()
    {
      Close();
    }

    public void Close()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing) {
        if (stream != null) {
          stream.Close();
          stream = null;
        }

        foreach (var box in boxes) {
          box.Dispose();
        }
      }

      disposed = true;
    }

    private void ReadFrom(Stream stream)
    {
      using (var reader = new BoxReader(stream, true)) {
        for (;;) {
          var box = reader.ReadBox(true);

          if (box == null)
            break;

          boxes.Add(box);
        }
      }
    }

    public void Save()
    {
      CheckDisposed();

      if (!stream.CanWrite)
        throw ExceptionUtils.CreateNotSupportedWritingStream();
      if (!stream.CanSeek)
        throw ExceptionUtils.CreateNotSupportedSeekingStream();

      foreach (var box in boxes) {
        box.ReadDataBlockIntoMemory();
      }

      SaveTo(stream);
    }

    public void Save(string file)
    {
      using (var stream = File.OpenWrite(file)) {
        Save(stream);
      }
    }

    public void Save(Stream stream)
    {
      CheckDisposed();

      if (stream == null)
        throw new ArgumentNullException("stream");
      else if (stream == this.stream)
        throw new InvalidOperationException("cannot save to stream that is used by instance of MediaFile");
      else if (stream is FileStream && this.stream is FileStream &&
               Smdn.IO.PathUtils.AreSameFile((stream as FileStream).Name, (this.stream as FileStream).Name))
        throw new InvalidOperationException("cannot save to file that is used by instance of MediaFile");

      SaveTo(stream);
    }

    private void SaveTo(Stream stream)
    {
      stream.SetLength(0);

      using (var writer = new BoxWriter(stream, true)) {
        writer.Write(boxes);
      }

      stream.Flush();
    }

    public Box Find(Predicate<Box> match)
    {
      CheckDisposed();

      return Box.Find(this, match);
    }

    public Box Find(Type boxType)
    {
      CheckDisposed();

      return Box.Find(this, boxType);
    }

    public IList<Box> FindAll(Predicate<Box> match)
    {
      CheckDisposed();

      return Box.FindAll(this, match);
    }

    public IList<Box> FindAll(Type boxType)
    {
      CheckDisposed();

      return Box.FindAll(this, boxType);
    }

    public Box Find(FourCC type, params FourCC[] childTypes)
    {
      return Find(ArrayExtensions.Prepend(childTypes, type));
    }

    private Box Find(FourCC[] types)
    {
      var found = FindAll(types);

      if (found.Count == 0)
        return null;
      else
        return found[0];
    }

    public IList<Box> FindAll(FourCC type, params FourCC[] childTypes)
    {
      return FindAll(ArrayExtensions.Prepend(childTypes, type));
    }

    private IList<Box> FindAll(FourCC[] types)
    {
      CheckDisposed();

      var hierarchy = new Queue<FourCC>(types);
      var matchedContainers = new List<IBoxContainer>();
      var matched = new List<Box>();

      matchedContainers.Add(this);

      while (0 < hierarchy.Count) {
        if (matchedContainers.Count == 0)
          return new List<Box>();

        var type = hierarchy.Dequeue();

        matched.Clear();

        foreach (var container in matchedContainers) {
          matched.AddRange(Box.FindAll(container, delegate(Box box) {
            return (box.Type == type);
          }));
        }

        matchedContainers.Clear();

        foreach (var box in matched) {
          if (box is IBoxContainer)
            matchedContainers.Add(box as IBoxContainer);
        }
      }

      return matched;
    }

    public Box FindOrCreate(FourCC type, params FourCC[] childTypes)
    {
      return FindOrCreate<Box>(null, type, childTypes);
    }

    public TBox FindOrCreate<TBox>(FourCC type, params FourCC[] childTypes) where TBox : Box
    {
      return FindOrCreate<TBox>(null, type, childTypes);
    }

    public TBox FindOrCreate<TBox>(Action<TBox> actionOnCreated, FourCC type, params FourCC[] childTypes) where TBox : Box
    {
      var box = (TBox)Find(type, childTypes);

      if (box != null)
        return box;

      if (childTypes.Length == 0)
        box = Box.Create<TBox>(type);
      else
        box = Box.Create<TBox>(childTypes[childTypes.Length - 1]);

      if (actionOnCreated != null)
        actionOnCreated(box);

      if (childTypes.Length == 0)
        Append(box);
      else
        AppendInto(box, type, ArrayExtensions.Slice(childTypes, 0, childTypes.Length - 1));

      return box;
    }

    public void Append(Box box)
    {
      CheckDisposed();

      if (box == null)
        throw new ArgumentNullException("box");

      boxes.Add(box);
    }

    public void Remove(Box box)
    {
      CheckDisposed();

      if (box == null)
        throw new ArgumentNullException("box");

      boxes.Remove(box);
    }

    public void Remove(FourCC type, params FourCC[] childTypes)
    {
      CheckDisposed();

      var box = Find(type, childTypes);

      if (box == null)
        throw new InvalidOperationException("box not found");

      if (box.ContainedIn is ContainerBoxBase)
        (box.ContainedIn as ContainerBoxBase).Boxes.Remove(box);
      else if (box.ContainedIn is MediaFile)
        (box.ContainedIn as MediaFile).boxes.Remove(box);
      else
        throw new InvalidOperationException("box is not containable");
    }

    public void AppendInto(Box box, FourCC type, params FourCC[] childTypes)
    {
      if (box == null)
        throw new ArgumentNullException("box");

      var container = Find(type, childTypes);

      if (!(container is ContainerBoxBase))
        throw new InvalidOperationException("box is not containable");

      (container as ContainerBoxBase).Boxes.Add(box);
    }

    public void RemoveFrom(Box box, FourCC type, params FourCC[] childTypes)
    {
      var container = Find(type, childTypes);

      if (!(container is ContainerBoxBase))
        throw new InvalidOperationException("box is not containable");

      (container as ContainerBoxBase).Boxes.Remove(box);
    }

    public void InsertAfter(Box box, FourCC type, params FourCC[] childTypes)
    {
      if (box == null)
        throw new ArgumentNullException("box");

      int index;
      var container = GetContainerOf(out index, type, childTypes);

      container.Boxes.Insert(index + 1, box);
    }

    public void InsertBefore(Box box, FourCC type, params FourCC[] childTypes)
    {
      if (box == null)
        throw new ArgumentNullException("box");

      int index;
      var container = GetContainerOf(out index, type, childTypes);

      container.Boxes.Insert(index, box);
    }

    private ContainerBoxBase GetContainerOf(out int index, FourCC type, params FourCC[] childTypes)
    {
      CheckDisposed();

      index = -1;

      var target = Find(type, childTypes);

      if (target == null)
        throw new InvalidOperationException("box not found");

      if (!(target.ContainedIn is ContainerBoxBase))
        throw new InvalidOperationException("box is not containable");

      var container = target.ContainedIn as ContainerBoxBase;

      index = container.Boxes.IndexOf(target);

      return container;
    }

    private void CheckDisposed()
    {
      if (disposed)
        throw new ObjectDisposedException(GetType().FullName);
    }

    private bool disposed = false;
    private Stream stream;
    private readonly BoxList boxes;
  }
}
