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
using System.Runtime.InteropServices;

using Smdn.Formats.Earthsoft.PV4.IO;
using Smdn.Mathematics;

namespace Smdn.Formats.Earthsoft.PV4 {
  public class DV : IDisposable {
    public const int AudioChannels = 2;
    public const int AudioBitsPerSample = 16;
    public static readonly Fraction InterlacedFrameRate  = new Fraction(30000, 1001);
    public static readonly Fraction ProgressiveFrameRate = new Fraction(60000, 1001);
    public const string StreamFileExtension = ".dv";
    public const string IndexFileExtension = ".dvi";

    public StreamFileHeaderData Header {
      get { return header; }
    }

    public Fraction FrameRate {
      get
      {
        switch (header.FrameScanning) {
          case FrameScanning.Interlaced:
            return InterlacedFrameRate;
          case FrameScanning.Progressive:
            return ProgressiveFrameRate;
          default:
            throw new InvalidDataException("invalid frame scanning type");
        }
      }
    }

    public FrameScanning FrameScanning {
      get { return header.FrameScanning; }
    }

    public int PixelsHorizontal {
      get { return header.HorizontalPixels << 4; }
    }

    public int PixelsVertical {
      get { return header.VerticalPixels << 3; }
    }

    public Fraction DisplayAspectRatio {
      get { return displayAspectRatio; }
    }

    public Fraction AudioSamplingRate {
      get { return audioSamplingRate; }
    }

    public int FrameCount {
      get { return indices.Count; }
    }

    public IEnumerable<IndexFileEntry> Indices {
      get { return indices; }
    }

    public string StreamFile {
      get { return streamFile; }
    }

    public string IndexFile {
      get { return indexFile; }
    }

    public StreamFileReader Reader {
      get { return reader; }
    }

    protected DV(string streamOrIndexFile, bool allowGenerateIndexFromStreamFile)
    {
      if (Path.GetExtension(streamOrIndexFile) == StreamFileExtension) {
        streamFile = streamOrIndexFile;
        indexFile = Utils.GetIndexFilePath(streamOrIndexFile);
      }
      else {
        streamFile = Utils.GetStreamFilePath(streamOrIndexFile);
        indexFile = streamOrIndexFile;
      }

      if (!File.Exists(streamFile))
        throw new FileNotFoundException(string.Format("stream file '{0}' not exist", streamFile));

      if (!File.Exists(indexFile)) {
        if (allowGenerateIndexFromStreamFile)
          indexFile = null;
        else
          throw new FileNotFoundException(string.Format("index file '{0}' not exist", indexFile));
      }

      indices = new List<IndexFileEntry>(Utils.GetFrameList((indexFile == null) ? streamFile : indexFile, allowGenerateIndexFromStreamFile));
      reader = new StreamFileReader(streamFile);
      header = reader.ReadHeader();

      // read first frame info and get aspect ratio
      reader.SeekToFirstFrame();

      var data = reader.ReadFrameData(false, false);

      displayAspectRatio = data.Video.DisplayAspectRatio;
      audioSamplingRate  = data.Audio.SamplingRate;
    }

    public static DV Open(string streamOrIndexFile)
    {
      return Open(streamOrIndexFile, false);
    }

    public static DV Open(string streamOrIndexFile, bool allowGenerateIndexFromStreamFile)
    {
      return new DV(streamOrIndexFile, allowGenerateIndexFromStreamFile);
    }

    void IDisposable.Dispose()
    {
      Close();
    }

    public void Close()
    {
      Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing) {
        if (reader != null) {
          reader.Close();
          reader = null;
        }
      }

      disposed = true;
    }

    public IndexFileEntry GetIndex(int frame)
    {
      RejectDisposed();

      return indices[frame];
    }

    public void ForEachIndex(IndexFileEntryHandler action)
    {
      RejectDisposed();

      for (var frameNumber = 0; frameNumber < indices.Count; frameNumber++) {
        action(frameNumber, indices[frameNumber]);
      }
    }

    public StreamFileFrameData GetAudio(int frame)
    {
      return GetFrame(frame, false, true);
    }

    public StreamFileFrameData GetVideo(int frame)
    {
      return GetFrame(frame, true, false);
    }

    public StreamFileFrameData GetFrame(int frame)
    {
      return GetFrame(frame, true, true);
    }

    public StreamFileFrameData GetFrame(int frame, bool readVideo, bool readAudio)
    {
      StreamFileFrameData frameData = null;

      if (GetFrame(frame, ref frameData, readVideo, readAudio))
        return frameData;
      else
        return null;
    }

    public bool GetAudio(int frame, ref StreamFileFrameData buffer)
    {
      return GetFrame(frame, ref buffer, false, true);
    }

    public bool GetVideo(int frame, ref StreamFileFrameData buffer)
    {
      return GetFrame(frame, ref buffer, true, false);
    }

    public bool GetFrame(int frame, ref StreamFileFrameData buffer)
    {
      return GetFrame(frame, ref buffer, true, true);
    }

    public bool GetFrame(int frame, ref StreamFileFrameData buffer, bool readVideo, bool readAudio)
    {
      RejectDisposed();

      reader.SeekToFrame(indices[frame]);

      if (buffer == null) {
        if ((buffer = reader.ReadFrameData(readAudio, readVideo)) == null)
          return false;
      }
      else {
        if (!reader.ReadFrameData(buffer, readAudio, readVideo))
          return false;
      }

      if (readVideo)
        displayAspectRatio = buffer.Video.DisplayAspectRatio;
      if (readAudio)
        audioSamplingRate = buffer.Audio.SamplingRate;

      return true;
    }

    public void ForEachFrame(StreamFileFrameDataHandler action)
    {
      ForEachFrame(true, true, 0, FrameCount, false, action);
    }

    public void ForEachFrame(bool reuseBuffer, StreamFileFrameDataHandler action)
    {
      ForEachFrame(true, true, 0, FrameCount, reuseBuffer, action);
    }

    public void ForEachFrame(int start, StreamFileFrameDataHandler action)
    {
      ForEachFrame(true, true, start, FrameCount - start, false, action);
    }

    public void ForEachFrame(int start, int count, StreamFileFrameDataHandler action)
    {
      ForEachFrame(true, true, start, count, false, action);
    }

    public void ForEachFrame(int start, int count, bool reuseBuffer, StreamFileFrameDataHandler action)
    {
      ForEachFrame(true, true, start, count, reuseBuffer, action);
    }

    public void ForEachAudioFrame(StreamFileFrameDataHandler action)
    {
      ForEachFrame(false, true, 0, FrameCount, false, action);
    }

    public void ForEachAudioFrame(bool reuseBuffer, StreamFileFrameDataHandler action)
    {
      ForEachFrame(false, true, 0, FrameCount, reuseBuffer, action);
    }

    public void ForEachAudioFrame(int start, StreamFileFrameDataHandler action)
    {
      ForEachFrame(false, true, start, FrameCount - start, false, action);
    }

    public void ForEachAudioFrame(int start, int count, StreamFileFrameDataHandler action)
    {
      ForEachFrame(false, true, start, count, false, action);
    }

    public void ForEachAudioFrame(int start, int count, bool reuseBuffer, StreamFileFrameDataHandler action)
    {
      ForEachFrame(false, true, start, count, reuseBuffer, action);
    }

    public void ForEachVideoFrame(StreamFileFrameDataHandler action)
    {
      ForEachFrame(true, false, 0, FrameCount, false, action);
    }

    public void ForEachVideoFrame(bool reuseBuffer, StreamFileFrameDataHandler action)
    {
      ForEachFrame(true, false, 0, FrameCount, reuseBuffer, action);
    }

    public void ForEachVideoFrame(int start, StreamFileFrameDataHandler action)
    {
      ForEachFrame(true, false, start, FrameCount - start, false, action);
    }

    public void ForEachVideoFrame(int start, int count, StreamFileFrameDataHandler action)
    {
      ForEachFrame(true, false, start, count, false, action);
    }

    public void ForEachVideoFrame(int start, int count, bool reuseBuffer, StreamFileFrameDataHandler action)
    {
      ForEachFrame(true, false, start, count, reuseBuffer, action);
    }

    public void ForEachFrame(bool readVideo, bool readAudio, StreamFileFrameDataHandler action)
    {
      ForEachFrame(readVideo, readAudio, 0, FrameCount, false, action);
    }

    public void ForEachFrame(bool readVideo, bool readAudio, bool reuseBuffer, StreamFileFrameDataHandler action)
    {
      ForEachFrame(readVideo, readAudio, 0, FrameCount, reuseBuffer, action);
    }

    public void ForEachFrame(bool readVideo, bool readAudio, int start, StreamFileFrameDataHandler action)
    {
      ForEachFrame(readVideo, readAudio, start, FrameCount - start, false, action);
    }

    public void ForEachFrame(bool readVideo, bool readAudio, int start, int count, StreamFileFrameDataHandler action)
    {
      ForEachFrame(readVideo, readAudio, start, count, false, action);
    }

    /*
     * TODO: C# 4.0
    public void ForEachFrame(StreamFileFrameDataHandler action, int Start = 0, bool ReadVideo = true, bool ReadAudio = true, bool ReuseBuffer = false)
    {
      ForEachFrame(ReadVideo, ReadAudio, Start, FrameCount - Start, ReuseBuffer, action);
    }

    public void ForEachFrame(int start, int count, StreamFileFrameDataHandler action, bool ReadVideo = true, bool ReadAudio = true, bool ReuseBuffer = false)
    {
      ForEachFrame(ReadVideo, ReadAudio, start, count, ReuseBuffer, action);
    }
    */

    public void ForEachFrame(bool readVideo, bool readAudio, int start, int count, bool reuseBuffer, StreamFileFrameDataHandler action)
    {
      RejectDisposed();

      if (action == null)
        throw new ArgumentNullException("action");

      reader.SeekToFrame(GetIndex(start));

      StreamFileFrameData frameData = null;

      for (int index = 0, frameNumber = start; index < count; index++, frameNumber++) {
        if (reuseBuffer && frameData != null) {
          if (!reader.ReadFrameData(frameData, readAudio, readVideo))
            break;
        }
        else {
          if ((frameData = reader.ReadFrameData(readAudio, readVideo)) == null)
            break;
        }

        if (readVideo)
          displayAspectRatio = frameData.Video.DisplayAspectRatio;

        if (readAudio)
          audioSamplingRate = frameData.Audio.SamplingRate;

        action(frameNumber, frameData);
      }
    }

    /*
     * frame to TimeSpan conversion methods
     */
    public TimeSpan VideoFrameToTimeSpan(int frame)
    {
      return TimeSpan.FromSeconds((double)frame / FrameRate);
    }

    public int TimeSpanToVideoFrame(TimeSpan time)
    {
      return (int)(time.TotalSeconds * FrameRate);
    }

    public TimeSpan AudioSampleToTimeSpan(int sample)
    {
      return TimeSpan.FromSeconds((double)sample / audioSamplingRate);
    }

    public int TimeSpanToAudioSample(TimeSpan time)
    {
      return (int)(time.TotalSeconds * audioSamplingRate);
    }

    public int AudioSampleToVideoFrame(int sample)
    {
      return AudioSampleToVideoFrame(sample, audioSamplingRate);
    }

    public int AudioSampleToVideoFrame(long sample, Fraction samplingRate)
    {
      return (int)(sample * FrameRate / samplingRate);
    }

    public long VideoFrameToAudioSample(int frame)
    {
      return VideoFrameToAudioSample(frame, audioSamplingRate);
    }

    public long VideoFrameToAudioSample(int frame, Fraction samplingRate)
    {
      return (long)(frame * samplingRate / FrameRate);
    }

    private void RejectDisposed()
    {
      if (disposed)
        throw new ObjectDisposedException(GetType().Name);
    }

    public override string ToString()
    {
      return string.Format("{{Format={0}x{1}{2}, FrameCount={3}, StreamFile='{4}', IndexFile='{5}'}}",
                           PixelsHorizontal,
                           PixelsVertical,
                           FrameScanning == FrameScanning.Progressive ? "p" :
                           FrameScanning == FrameScanning.Interlaced ? "i" : "(?)",
                           FrameCount,
                           streamFile,
                           indexFile);
    }

    private readonly string streamFile;
    private readonly string indexFile;
    private StreamFileReader reader;
    private StreamFileHeaderData header;
    private IList<IndexFileEntry> indices;

    private Fraction displayAspectRatio;
    private Fraction audioSamplingRate;

    private bool disposed = false;
  }
}
