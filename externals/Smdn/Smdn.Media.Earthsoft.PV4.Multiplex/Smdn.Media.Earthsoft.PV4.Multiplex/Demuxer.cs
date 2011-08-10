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

using Smdn.Formats.Earthsoft.PV4;
using Smdn.Media.Earthsoft.PV4.Multiplex.Iec61937;

namespace Smdn.Media.Earthsoft.PV4.Multiplex {
  public class Demuxer : IDisposable {
    private struct DataBurst {
      public static readonly DataBurst None = new DataBurst(DataType.LinearPCM, 0, 0);

      public DataType DataType;
      public int StartFrame;
      public int StartSample;
      public int EndFrame;
      public int EndSample;

      public DataBurst(DataType dataType, int startFrame, int startSample)
      {
        DataType          = dataType;
        StartFrame        = startFrame;
        StartSample       = startSample;
        EndFrame          = 0;
        EndSample         = 0;
      }
    }

    public DV DV {
      get { return dv; }
    }

    public Demuxer(DV dv)
    {
      if (dv == null)
        throw new ArgumentNullException("dv");

      this.dv = dv;

#if false
      InitializeDataBurstList();

      foreach (var dataBurst in dataBursts) {
        Console.WriteLine("{0}: {1}.{2} => {3}.{4}", dataBurst.DataType, dataBurst.StartFrame, dataBurst.StartSample, dataBurst.EndFrame, dataBurst.EndSample);
      }
#else
      throw new NotImplementedException();
#endif
    }

    public void Dispose()
    {
      // nothing to do
    }

#if false
    private void InitializeDataBurstList()
    {
      var currentDataBurst = new DataBurst(DataType.LinearPCM, 0, 0);
      var repetitionPeriod = 0;
      var syncedAtPrevFrame = false;

      dv.ForEachAudioFrame(delegate(int frameNumber, StreamFileFrameData frameData) {
        var reader = new BigEndianBinaryReader(new MemoryStream(frameData.Audio));

        for (var sample = 0; sample < frameData.AudioSampleCount;) {
          if (0 < repetitionPeriod) {
            if (repetitionPeriod < frameData.AudioSampleCount - sample) {
              sample += repetitionPeriod;

              reader.BaseStream.Seek(4 * repetitionPeriod, SeekOrigin.Current);

              repetitionPeriod = 0;

              continue;
            }
            else {
              repetitionPeriod -= (frameData.AudioSampleCount - sample);
              break;
            }
          }

          var offset = sample;

          if (syncedAtPrevFrame) {
            syncedAtPrevFrame = false;
          }
          else {
            var preamble = reader.ReadUInt32();

            offset = sample++;

            if (preamble == 0xf8724e1f) {
              if (offset == frameData.AudioSampleCount - 1) {
                Console.WriteLine("synced at last frame");
                syncedAtPrevFrame = true;
                break;
              }
            }
            else {
              if (currentDataBurst.DataType != DataType.LinearPCM) {
                // new data burst
                currentDataBurst.EndFrame  = frameNumber;
                currentDataBurst.EndSample = offset;

                dataBursts.Add(currentDataBurst);

                currentDataBurst = new DataBurst(DataType.LinearPCM, frameNumber, offset);
              }
              continue;
            }
          }
          Console.WriteLine("preamble offset: {0} {1}/{2} {3}/{4}", reader.BaseStream.Position, offset, reader.BaseStream.Length, frameNumber, dv.FrameCount);
          //Console.WriteLine("preamble offset: {4}/{5}", reader.BaseStream.Position, offset, reader.BaseStream.Length, preamble, frameNumber, dv.FrameCount);

          // sync word detected
          var burstInfo  = reader.ReadUInt16();
          var lengthCode = reader.ReadUInt16();
          var dataType   = (DataType)(burstInfo & (ushort)(DataType.DataTypeMask | DataType.SubDataTypeMask));

          sample++;

          Console.WriteLine("  data type: {0}", dataType);

          if (dataType != currentDataBurst.DataType) {
            // new data burst
            currentDataBurst.EndFrame  = frameNumber;
            currentDataBurst.EndSample = offset;

            dataBursts.Add(currentDataBurst);

            currentDataBurst = new DataBurst(dataType, frameNumber, offset);
          }

          currentDataBurst.DataType = dataType;

          repetitionPeriod = DataBurstDefinition.GetRepetitionPeriodOf(dataType);
        }
      });

      currentDataBurst.EndFrame  = dv.FrameCount - 1;
      currentDataBurst.EndSample = dv.GetIndex(dv.FrameCount - 1).AudioSampleCount - 1;

      dataBursts.Add(currentDataBurst);
    }

    private readonly List<DataBurst> dataBursts = new List<DataBurst>();
#endif

    private readonly DV dv;
  }
}
