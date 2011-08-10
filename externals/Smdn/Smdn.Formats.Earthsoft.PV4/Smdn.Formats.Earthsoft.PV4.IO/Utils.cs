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
using System.Runtime.Remoting.Messaging; // AsyncResult

namespace Smdn.Formats.Earthsoft.PV4.IO {
  public static class Utils {
    public static bool IsStreamFile(string file)
    {
      using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read)) {
        if (stream.Length < StreamFileHeaderData.Size)
          return false;

        var preamble = new byte[StreamFileHeaderData.StreamPreamble.Length];

        if (stream.Read(preamble, 0, preamble.Length) != preamble.Length)
          return false;

        return StreamFileHeaderData.StreamPreamble.Equals(preamble);
      }
    }

    public static FileInfo GetStreamFile(FileInfo indexFile)
    {
      return new FileInfo(Path.Combine(indexFile.DirectoryName, Path.GetFileNameWithoutExtension(indexFile.Name) + DV.StreamFileExtension));
    }

    public static string GetStreamFilePath(string streamOrIndexFile)
    {
      return Path.Combine(Path.GetDirectoryName(streamOrIndexFile), Path.GetFileNameWithoutExtension(streamOrIndexFile) + DV.StreamFileExtension);
    }

    public static FileInfo GetIndexFile(FileInfo streamFile)
    {
      return new FileInfo(Path.Combine(streamFile.DirectoryName, Path.GetFileNameWithoutExtension(streamFile.Name) + DV.IndexFileExtension));
    }

    public static string GetIndexFilePath(string streamOrIndexFile)
    {
      return Path.Combine(Path.GetDirectoryName(streamOrIndexFile), Path.GetFileNameWithoutExtension(streamOrIndexFile) + DV.IndexFileExtension);
    }

    public static IEnumerable<IndexFileEntry> GetFrameList(string streamOrIndexFile)
    {
      return GetFrameList(streamOrIndexFile, true);
    }

    public static IEnumerable<IndexFileEntry> GetFrameList(string streamOrIndexFile, bool allowGenerateIndexFromStreamFile)
    {
      var indexFile = GetIndexFilePath(streamOrIndexFile);

      if (System.IO.File.Exists(indexFile))
        return GetFrameListFromIndexFile(indexFile);

      if (!allowGenerateIndexFromStreamFile)
        throw new FileNotFoundException("index file not found");

      return GetFrameListFromStreamFile(streamOrIndexFile);
    }

    public static IEnumerable<IndexFileEntry> GetFrameListFromStreamFile(string streamFile)
    {
      var frames = new List<IndexFileEntry>();

      using (var reader = new StreamFileReader(streamFile)) {
        reader.SeekToFirstFrame();

        var offset = reader.BaseStream.Position;

        for (;;) {
          var frameData = reader.ReadFrameData(false, false);

          if (frameData == null)
            break;

          var position = reader.BaseStream.Position;

          frames.Add(new IndexFileEntry(offset, (int)(position - offset)));

          offset = position;
        }
      }

      return frames;
    }

    public static IEnumerable<IndexFileEntry> GetFrameListFromIndexFile(string indexFile)
    {
      using (var reader = new IndexFileReader(indexFile)) {
        return reader.ReadAllEntry();
      }
    }

#region "ExtractToFile methods"
    private delegate void ExtractFromFileToFileDelegate(string streamOrIndexFile, int frameStart, int frameCount, string extractFile);

    public static void ExtractToFile(string streamOrIndexFile, int frameStart, int frameCount, string extractFile)
    {
      using (var dv = DV.Open(streamOrIndexFile)) {
        ExtractToFile(dv, frameStart, frameCount, extractFile);
      }
    }

    private delegate void ExtractFromDVToFileDelegate(DV dv, int frameStart, int frameCount, string extractFile, StreamFileFrameDataHandler preprocessFrameData);

    public static void ExtractToFile(DV dv, int frameStart, int frameCount, string extractFile)
    {
      ExtractToFile(dv, frameStart, frameCount, extractFile);
    }

    public static void ExtractToFile(DV dv, int frameStart, int frameCount, string extractFile, StreamFileFrameDataHandler preprocessFrameData)
    {
      using (var writer = new StreamAndIndexWriter(dv.Header, extractFile)) {
        // read and write frame
        dv.ForEachFrame(true, true, frameStart, frameCount, true, delegate(int frameNumber, StreamFileFrameData frameData) {
          if (preprocessFrameData != null)
            preprocessFrameData(frameNumber, frameData);

          writer.Write(frameData);
        });
      }
    }

    public static IAsyncResult BeginExtractToFile(string streamOrIndexFile, int frameStart, int frameCount, string extractFile)
    {
      return BeginExtractToFile(streamOrIndexFile, frameStart, frameCount, extractFile, null, null);
    }

    public static IAsyncResult BeginExtractToFile(string streamOrIndexFile, int frameStart, int frameCount, string extractFile, AsyncCallback callback, object state)
    {
      var asyncMethod = new ExtractFromFileToFileDelegate(ExtractToFile);

      return asyncMethod.BeginInvoke(streamOrIndexFile, frameStart, frameCount, extractFile, callback, state);
    }

    public static IAsyncResult BeginExtractToFile(DV dv, int frameStart, int frameCount, string extractFile)
    {
      return BeginExtractToFile(dv, frameStart, frameCount, extractFile, null, null, null);
    }

    public static IAsyncResult BeginExtractToFile(DV dv, int frameStart, int frameCount, string extractFile, AsyncCallback callback, object state)
    {
      return BeginExtractToFile(dv, frameStart, frameCount, extractFile, null, callback, state);
    }

    public static IAsyncResult BeginExtractToFile(DV dv, int frameStart, int frameCount, string extractFile, StreamFileFrameDataHandler preprocessFrameData)
    {
      return BeginExtractToFile(dv, frameStart, frameCount, extractFile, preprocessFrameData, null, null);
    }

    public static IAsyncResult BeginExtractToFile(DV dv, int frameStart, int frameCount, string extractFile, StreamFileFrameDataHandler preprocessFrameData, AsyncCallback callback, object state)
    {
      var asyncMethod = new ExtractFromDVToFileDelegate(ExtractToFile);

      return asyncMethod.BeginInvoke(dv, frameStart, frameCount, extractFile, preprocessFrameData, callback, state);
    }

    public static void EndExtractToFile(IAsyncResult asyncResult)
    {
      if (asyncResult == null)
        throw new ArgumentNullException("asyncResult");

      var result = asyncResult as AsyncResult;

      if (result == null)
        throw ExceptionUtils.CreateArgumentMustBeValidIAsyncResult("asyncResult");
      else if (result.AsyncDelegate is ExtractFromFileToFileDelegate)
        (result.AsyncDelegate as ExtractFromFileToFileDelegate).EndInvoke(result);
      else if (result.AsyncDelegate is ExtractFromDVToFileDelegate)
        (result.AsyncDelegate as ExtractFromDVToFileDelegate).EndInvoke(result);
      else
        throw ExceptionUtils.CreateArgumentMustBeValidIAsyncResult("asyncResult");
    }
#endregion
  }
}
