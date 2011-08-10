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

namespace Smdn.Formats.IsoBaseMediaFile.Standards.Iso.Avc {
  [FieldStructure]
  public class AvcDecoderConfigurationRecord {
    [FieldLayout(0, 8)] public byte ConfigurationVersion = 1;
    [FieldLayout(1, 8)] public byte AvcProfileIndication;
    [FieldLayout(2, 8)] public byte AvcCompatibleProfiles;
    [FieldLayout(3, 8)] public byte AvcLevelIndication;
    [FieldLayout(4, 6)] public byte reserved = 0x3f; // 111111b
    [FieldLayout(5, 2)] public byte LengthSizeMinusOne;
    [FieldLayout(6, 3)] public byte reserved2 = 0x03; // 111b
    [FieldLayout(7, 5)] public byte NumOfSequenceParameterSets;
    [FieldLayout(8, 0, Count = "NumOfSequenceParameterSets")] public SequenceParameterSet[] SequenceParameterSets = new SequenceParameterSet[] {};
    [FieldLayout(9, 8)] public byte NumOfPictureParameterSets;
    [FieldLayout(10, 0, Count = "NumOfPictureParameterSets")] public PictureParameterSet[] PictureParameterSets = new PictureParameterSet[] {};

    [FieldStructure]
    public struct SequenceParameterSet {
      [FieldLayout(0, 16)] public uint SequenceParameterSetLength;
      [FieldLayout(1, SizeInBytes = "SequenceParameterSetLength")] public DataBlock SequenceParameterSetNALUnit;
    }

    [FieldStructure]
    public struct PictureParameterSet {
      [FieldLayout(0, 16)] public uint PictureParameterSetLength;
      [FieldLayout(1, SizeInBytes = "PictureParameterSetLength")] public DataBlock PictureParameterSetNALUnit;
    }

    /*
    protected override void Dispose(bool disposing)
    {
      if (disposing) {
        if (SequenceParameterSets != null) {
          foreach (var paramset in SequenceParameterSets) {
            if (paramset.SequenceParameterSetNALUnit != null) {
              paramset.SequenceParameterSetNALUnit.Dispose();
              paramset.SequenceParameterSetNALUnit = null;
            }
          }
        }
        if (PictureParameterSets != null) {
          foreach (var paramset in PictureParameterSets) {
            if (paramset.PictureParameterSetNALUnit != null) {
              paramset.PictureParameterSetNALUnit.Dispose();
              paramset.PictureParameterSetNALUnit = null;
            }
          }
        }
      }

      base.Dispose(disposing);
    }
    */
  }
}