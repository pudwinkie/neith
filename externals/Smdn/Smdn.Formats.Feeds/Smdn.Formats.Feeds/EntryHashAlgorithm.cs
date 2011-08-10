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
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Smdn.Formats.Feeds {
  public class EntryHashAlgorithm<THashAlgorithm> : EntryHashAlgorithm where THashAlgorithm : HashAlgorithm {
    public int HashSize {
      get
      {
        CheckDisposed();
        return hasher.HashSize;
      }
    }

    public EntryHashAlgorithm()
      : base()
    {
      var method = typeof(THashAlgorithm).GetMethod("Create",
                                                    BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly,
                                                    null,
                                                    Type.EmptyTypes,
                                                    null);

      hasher = (THashAlgorithm)method.Invoke(null, null);
    }

    ~EntryHashAlgorithm()
    {
      Dispose(false);
    }

    internal protected override void Initialize()
    {
      CheckDisposed();

      hasher.Initialize();
    }

    internal protected override byte[] ComputeHash(IEntry entry, XmlNode entryNode)
    {
      CheckDisposed();

      using (var stream = new System.IO.MemoryStream()) {
        var setting = new XmlWriterSettings();

        setting.OmitXmlDeclaration = true;

        var writer = XmlWriter.Create(stream, setting);

        entryNode.WriteTo(writer);

        writer.Flush();

        stream.Position = 0;

        lock (stream) {
          return hasher.ComputeHash(stream);
        }
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing) {
        if (hasher != null) {
          hasher.Clear();
          hasher = null;
        }
      }

      base.Dispose(disposing);

      disposed = true;
    }

    private void CheckDisposed()
    {
      if (disposed)
        throw new ObjectDisposedException(GetType().FullName);
    }

    private bool disposed = false;
    private THashAlgorithm hasher;
  }

  public abstract class EntryHashAlgorithm : IDisposable {
    protected EntryHashAlgorithm()
    {
    }

    internal protected abstract byte[] ComputeHash(IEntry entry, XmlNode entryNode);

    internal protected virtual void Initialize()
    {
      // nothing to do
    }

    public virtual void Clear()
    {
      Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
      // nothing to do
    }

    void IDisposable.Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
  }
}
