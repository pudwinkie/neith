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

using Smdn.IO;
using Smdn.Media;

namespace Smdn.Formats.Riff.Wave {
  public abstract class RiffWave : IDisposable {
    public RiffStructure WaveStructure {
      get { CheckDisposed(); return wave; }
      protected set { CheckDisposed(); wave = value; }
    }

    protected Stream BaseStream {
      get { return stream; }
    }

    public WAVEFORMATEX Format {
      get { CheckDisposed(); return fmt; }
    }

    public virtual int Channels {
      get { CheckDisposed(); return fmt.nChannels; }
    }

    public virtual long SamplesPerSecond {
      get { CheckDisposed(); return (long)fmt.nSamplesPerSec; }
    }

    public virtual int BitsPerSample {
      get { CheckDisposed(); return fmt.wBitsPerSample; }
    }

    public virtual int BlockAlign {
      get { CheckDisposed(); return fmt.nBlockAlign; }
    }

    public virtual long SampleCount {
      get { CheckDisposed(); return (fmt.nBlockAlign == 0) ? 0L : dataChunk.Size / fmt.nBlockAlign; }
    }

    public RiffWave(string file)
      : this(File.OpenRead(file), null)
    {
    }

    public RiffWave(Stream stream)
      : this(stream, null)
    {
    }

    protected RiffWave(Stream stream, WAVEFORMATEX? fmt)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      this.stream = stream;

      if (fmt == null)
        ReadWaveStructure();
      else
        CreateWaveStructure(fmt.Value);
    }

    ~RiffWave()
    {
      Dispose(false);
    }

    void IDisposable.Dispose()
    {
      Close();
    }

    public void Close()
    {
      Flush();

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
      }
    }

    protected virtual void ReadWaveStructure()
    {
      this.wave = null;

      foreach (var riff in RiffStructure.ReadFrom(stream)) {
        if (riff.RiffType == RiffType.Wave) {
          this.wave = riff;
          break;
        }
      }

      if (this.wave == null)
        throw new InvalidDataException("stream is not RIFF WAVE");

      this.dataChunk = wave.FindChunk(KnownFourCC.ChunkType.Data);

      if (this.dataChunk == null)
        throw new InvalidDataException(string.Format("'{0}' chunk not found", KnownFourCC.ChunkType.Data));

      var fmtChunk = wave.FindChunk(KnownFourCC.ChunkType.Format);

      if (fmtChunk == null)
        throw new InvalidDataException(string.Format("'{0}' chunk not found", KnownFourCC.ChunkType.Format));

      using (var fmtChunkStream = RiffStructure.GetChunkStream(stream, fmtChunk)) {
        this.fmt = ReadWaveFormatEx(fmtChunk, fmtChunkStream);
      }
    }

    protected virtual WAVEFORMATEX ReadWaveFormatEx(Chunk fmtChunk, Stream fmtChunkStream)
    {
      if (fmtChunk.Size != 0x00000010 && fmtChunk.Size != 0x00000012)
        throw new InvalidDataException("invalid fmt length");

      return WAVEFORMATEX.ReadFrom(fmtChunkStream);
    }

    protected virtual void CreateWaveStructure(WAVEFORMATEX format)
    {
      this.fmt = format;

      var fmtChunk = new Chunk(KnownFourCC.ChunkType.Format, 12, (format.cbSize == 0) ? 0x10 : 0x12 + format.cbSize);

      this.dataChunk = new Chunk(KnownFourCC.ChunkType.Data, fmtChunk.Offset + fmtChunk.Size, 0L);
      this.wave = new RiffStructure(KnownFourCC.RiffType.Wave, new Chunk[] {
        fmtChunk,
        dataChunk,
      });
    }

    // XXX
    public virtual void Flush()
    {
      CheckDisposed();

      stream.Position = 0;

      var writer = new Smdn.IO.LittleEndianBinaryWriter(stream);

      // RIFF
      writer.Write(KnownFourCC.Riff);
      writer.Write((uint)(stream.Length - 8));
      writer.Write(KnownFourCC.RiffType.Wave);

      // fmt
      var fmtChunk = wave.FindChunk(KnownFourCC.ChunkType.Format);

      writer.Write(fmtChunk.FourCC);
      writer.Write((uint)fmtChunk.Size);
      writer.Flush();

      fmt.WriteTo(stream);

      // data
      writer.Write(dataChunk.FourCC);
      writer.Write((uint)(stream.Length - dataChunk.Offset));

      writer.Flush();
    }

    public virtual PartialStream GetDataStream()
    {
      CheckDisposed();

      if (dataChunk.Size == 0)
        return new PartialStream(stream, dataChunk.Offset, false, true, true);
      else
        return new PartialStream(stream, dataChunk.Offset, dataChunk.Size, true, true, true);
    }

    protected void CheckDisposed()
    {
      if (stream == null)
        throw new ObjectDisposedException(GetType().FullName);
    }

    private RiffStructure wave;
    private WAVEFORMATEX fmt;
    private Chunk dataChunk;
    private Stream stream;
  }
}
