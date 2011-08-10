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

using Smdn.IO;

namespace Smdn.Formats.Riff {
  public class RiffStructure : List {
#region "static members"
    public static RiffStructure[] ReadFrom(string file)
    {
      using (var stream = File.OpenRead(file)) {
        return ReadFrom(stream);
      }
    }

    public static RiffStructure[] ReadFrom(Stream stream)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");
      if (!stream.CanSeek)
        throw ExceptionUtils.CreateArgumentMustBeSeekableStream("stream");

      var reader = new LittleEndianBinaryReader(stream);
      var riffStructures = new List<RiffStructure>();

      //reader.BaseStream.Position = 0;

      while (!reader.EndOfStream) {
        var offset = reader.BaseStream.Position;

        if (KnownFourCC.Riff != reader.ReadFourCC())
          throw new InvalidDataException("stream is not valid RIFF");

        try {
          var size    = (long)reader.ReadUInt32();
          var fourcc  = reader.ReadFourCC();

          if (size < 0)
            throw new InvalidDataException("invalid chunk size");

          var chunks  = ReadChunks(reader, size);

          riffStructures.Add(new RiffStructure(fourcc, offset, size, chunks));
        }
        catch (IOException ex) {
          throw new InvalidDataException("invalid data", ex);
        }
      }

      return riffStructures.ToArray();
    }

    private static IEnumerable<Chunk> ReadChunks(LittleEndianBinaryReader reader, long chunkSize)
    {
      var chunks = new List<Chunk>();
      var endOfChunk = reader.BaseStream.Position + chunkSize;

      while (reader.BaseStream.Position < endOfChunk && !reader.EndOfStream) {
        var offset  = reader.BaseStream.Position;
        var fourcc  = reader.ReadFourCC();
        var size    = (long)reader.ReadUInt32();

        if (fourcc == KnownFourCC.List) {
          if (size < 4)
            throw new InvalidDataException("invalid chunk size");

          fourcc = reader.ReadFourCC();

          chunks.Add(new List(fourcc, offset, size, ReadChunks(reader, size - 4)));
        }
        else {
          if (size < 0)
            throw new InvalidDataException("invalid chunk size");

          chunks.Add(new Chunk(fourcc, offset, size));

          reader.BaseStream.Seek(size, SeekOrigin.Current);
        }
      }

      return chunks;
    }

    public static PartialStream GetChunkStream(Stream riffStream, Chunk chunk)
    {
      if (chunk is List)
        throw new NotSupportedException();

      return new PartialStream(riffStream, chunk.DataOffset, chunk.Size, true, true, true);
    }
#endregion

#region "instance members"
    public RiffType RiffType {
      get
      {
        if (FourCC == KnownFourCC.RiffType.Wave)
          return RiffType.Wave;
        else if (FourCC == KnownFourCC.RiffType.Avi)
          return RiffType.Avi;
        else if (FourCC == KnownFourCC.RiffType.Avix)
          return RiffType.Avix;
        else
          return RiffType.Unknown;
      }
    }

    public RiffStructure(FourCC fourcc, IEnumerable<Chunk> subChunks)
      : this(fourcc, 0L, 0L, subChunks)
    {
    }

    public RiffStructure(FourCC fourcc, long offset, long size, IEnumerable<Chunk> subChunks)
      : base(fourcc, offset, size, subChunks)
    {
    }

    public Chunk FindChunk(params string[] hierarchy)
    {
      return FindChunk(Array.ConvertAll(hierarchy, delegate(string fourcc) {
        return (FourCC)fourcc;
      }));
    }

    public Chunk FindChunk(params FourCC[] hierarchy)
    {
      if (hierarchy == null)
        throw new ArgumentNullException("hierarchy");

      if (hierarchy.Length == 0)
        return this;

      if (hierarchy.Length == 1) {
        return SubChunks.Find(delegate(Chunk chunk) {
          return chunk.FourCC == hierarchy[0];
        });
      }
      else {
        throw new NotImplementedException();
      }
    }
#endregion
  }
}
