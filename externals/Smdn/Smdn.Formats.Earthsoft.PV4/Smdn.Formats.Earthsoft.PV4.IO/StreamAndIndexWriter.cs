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

namespace Smdn.Formats.Earthsoft.PV4.IO {
  public sealed class StreamAndIndexWriter : IDisposable {
    public StreamAndIndexWriter(StreamFileHeaderData header, string file)
      : this(header, Utils.GetStreamFilePath(file), Utils.GetIndexFilePath(file))
    {
    }

    public StreamAndIndexWriter(StreamFileHeaderData header, string streamFile, string indexFile)
      : this(header,
             new FileStream(streamFile, FileMode.CreateNew, FileAccess.Write, FileShare.None),
             new FileStream(indexFile, FileMode.CreateNew, FileAccess.Write, FileShare.None))
    {
    }

    public StreamAndIndexWriter(StreamFileHeaderData header, Stream streamOfStreamFile)
      : this(header, streamOfStreamFile, null)
    {
    }

    public StreamAndIndexWriter(StreamFileHeaderData header, Stream streamOfStreamFile, Stream streamOfIndexFile)
    {
      if (header == null)
        throw new ArgumentNullException("header");

      if (streamOfStreamFile == null)
        throw new ArgumentNullException("streamOfStreamFile");

      this.streamOfStreamFile = streamOfStreamFile;
      this.streamOfIndexFile  = streamOfIndexFile;

      streamWriter = new StreamFileWriter(streamOfStreamFile);

      if (streamOfIndexFile == null)
        indexWriter = null;
      else
        indexWriter = new IndexFileWriter(streamOfIndexFile);

      streamWriter.Write(header);
      streamWriter.Flush();
    }

    void IDisposable.Dispose()
    {
      Close();
    }

    public void Close()
    {
      if (streamOfStreamFile != null) {
        streamOfStreamFile.Close();
        streamOfStreamFile = null;
      }

      if (streamOfIndexFile != null) {
        streamOfIndexFile.Close();
        streamOfIndexFile = null;
      }

      streamWriter = null;
      indexWriter = null;

      disposed = true;
    }

    public void Flush()
    {
      if (disposed)
        throw new ObjectDisposedException(GetType().Name);

      streamWriter.Flush();

      if (indexWriter != null)
        indexWriter.Flush();
    }

    public void Write(StreamFileFrameData frameData)
    {
      if (disposed)
        throw new ObjectDisposedException(GetType().Name);

      // wrote frame data
      long frameOffset = (indexWriter == null) ? 0L : streamWriter.BaseStream.Position;

      frameData.Audio.PrecedentSampleCountValue = (UInt48)precedentAudioSampleCount;

      streamWriter.Write(frameData);

      if (indexWriter != null) {
        var entry = new IndexFileEntry();

        entry.FrameOffset               = frameOffset;
        entry.FrameSize                 = (int)(streamWriter.BaseStream.Position - frameOffset);
        entry.AudioSampleCount          = frameData.Audio.SampleCountValue;
        entry.PrecedentAudioSampleCount = frameData.Audio.PrecedentSampleCountValue.ToInt64();
        entry.EncodingQuality           = frameData.Video.EncodingQualityValue;

        indexWriter.Write(entry);
      }

      precedentAudioSampleCount += frameData.Audio.SampleCountValue;
    }

    private Stream streamOfStreamFile = null;
    private Stream streamOfIndexFile = null;
    private StreamFileWriter streamWriter = null;
    private IndexFileWriter indexWriter = null;
    private long precedentAudioSampleCount = 0L;
    private bool disposed = false;
  }
}
