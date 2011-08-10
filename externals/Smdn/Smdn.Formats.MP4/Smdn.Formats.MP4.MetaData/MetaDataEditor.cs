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

using Smdn.Formats.IsoBaseMediaFile;
using Smdn.Formats.IsoBaseMediaFile.IO;
using Smdn.Formats.IsoBaseMediaFile.Standards.Iso;

namespace Smdn.Formats.MP4.MetaData {
  public abstract class MetaDataEditor : IDisposable {
    public MediaFile MediaFile {
      get { CheckDisposed(); return mediaFile; }
    }

    public virtual string BaseFilePath {
      get
      {
        CheckDisposed();

        if (mediaFile.BaseStream is FileStream)
          return (mediaFile.BaseStream as FileStream).Name;
        else
          return null;
      }
    }

    public abstract bool HasMetaData {
      get;
    }

    public MetaDataEditor(string mediaFile)
      : this(mediaFile, true)
    {
    }

    public MetaDataEditor(string mediaFile, bool openAsWritable)
      : this(new MediaFile(mediaFile, openAsWritable))
    {
    }

    public MetaDataEditor(MediaFile mediaFile)
    {
      if (mediaFile == null)
        throw new ArgumentNullException("mediaFile");

      this.mediaFile = mediaFile;
    }

    public MetaDataEditor(MetaDataEditor editor)
    {
      if (editor == null)
        throw new ArgumentNullException("editor");

      this.mediaFile = editor.mediaFile;
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
        if (mediaFile != null) {
          mediaFile.Close();
          mediaFile = null;
        }
      }
    }

    public void Save()
    {
      CheckDisposed();

      SaveCore(null);
    }

    public void Save(string file)
    {
      CheckDisposed();

      if (file == null)
        throw new ArgumentNullException("file");

      using (var stream = File.OpenWrite(file)) {
        stream.SetLength(0L);

        SaveCore(stream);
      }
    }

    public void Save(Stream stream)
    {
      CheckDisposed();

      if (stream == null)
        throw new ArgumentNullException("stream");

      SaveCore(stream);
    }

    protected virtual void SaveCore(Stream stream)
    {
      UpdateBoxes();

      if (stream == null)
        mediaFile.Save();
      else
        mediaFile.Save(stream);
    }

    public abstract void UpdateBoxes();

    public abstract void RemoveMetaDataBoxes();

    private void CheckDisposed()
    {
      if (mediaFile == null)
        throw new ObjectDisposedException(GetType().FullName);
    }

    private MediaFile mediaFile;
  }
}
