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

namespace Smdn.Media.Earthsoft.PV4.Multiplex.Iec61937 {
  internal static class DataBurstDefinitions {
    public static int GetRepetitionPeriodOf(DataType dataType)
    {
      if (repetitionPeriodTable.ContainsKey(dataType))
        return repetitionPeriodTable[dataType];
      else
        throw new System.IO.InvalidDataException(string.Format("unknown data type {0}", dataType));
    }

    private static readonly Dictionary<DataType, int> repetitionPeriodTable = new Dictionary<DataType, int>() {
      {DataType.LinearPCM,                                   0},
      {DataType.Null,                                        0},
      {DataType.AC3,                                      1536},
      {DataType.SMPTE338M1,                                  0},
      {DataType.Pause,                                       0},
      {DataType.MPEG1Layer1,                               384},
      {DataType.MPEG1Layer2,                              1152},
      //{DataType.MPEG1Layer3,                            1152}, // same as MPEG1Layer2
      //{DataType.MPEG2WithoutExtension,                  1152}, // same as MPEG1Layer2
      {DataType.MPEG2WithExtension,                       1152},
      {DataType.MPEG2AAC,                                 1024},
      {DataType.MPEG2Layer1LowSamplingFrequency,           768},
      {DataType.MPEG2Layer2LowSamplingFrequency,          2304},
      {DataType.MPEG2Layer3LowSamplingFrequency,          1152},
      {DataType.DTSTypeI,                                  512},
      {DataType.DTSTypeII,                                1024},
      {DataType.DTSTypeIII,                               2048},
      {DataType.ATRAC,                                     512},
      {DataType.ATRAC23,                                  1024},
      {DataType.ATRACX,                                   2048},
      {DataType.DTSTypeIV,                                   0}, // See IEC 61937-5
      {DataType.WMAProfessionalTypeI,                     2048},
      {DataType.WMAProfessionalTypeII,                    2048},
      {DataType.WMAProfessionalTypeIII,                   1024},
      {DataType.WMAProfessionalTypeIV,                     512},
      {DataType.MPEG2AACLowSamplingFrequency_2048,        2048},
      {DataType.MPEG2AACLowSamplingFrequency_4096,        4096},
      {DataType.MPEG2AACLowSamplingFrequency_Reserved1,      0}, // reserved
      {DataType.MPEG2AACLowSamplingFrequency_Reserved2,      0}, // reserved
      {DataType.MPEG4AAC_1024,                            1024},
      {DataType.MPEG4AAC_2048,                            2048},
      {DataType.MPEG4AAC_4096,                            4096},
      {DataType.MPEG4AAC_512,                              512},
      {DataType.EnhancedAC3,                              6144},
      {DataType.MAT,                                     15360},
      {DataType.Extended1,                                   0},
      {DataType.Extended2,                                   0},
      {DataType.Extended3,                                   0},
      {DataType.Extended4,                                   0},
    };
  }
}
