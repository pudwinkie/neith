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

namespace Smdn.Media.Earthsoft.PV4.Multiplex.Iec61937 {
  public enum DataType : ushort {
    LinearPCM                                 = 0xffff,

    DataTypeMask                              = 0x001f, // 0000 0000 0001 1111b
    SubDataTypeMask                           = 0x0060, // 0000 0000 0110 0000b

    Null                                      = 0x0000,
    AC3                                       = 0x0001,
    SMPTE338M1                                = 0x0002,
    // SMPTE 338M                             = 0x0002,
      // SMPTE 338M                           = 0x0022,
      // SMPTE 338M                           = 0x0042,
      // SMPTE 338M                           = 0x0062,
    Pause                                     = 0x0003,
    MPEG1Layer1                               = 0x0004,
    MPEG1Layer2                               = 0x0005,
    MPEG1Layer3                               = 0x0005,
    MPEG2WithoutExtension                     = 0x0005,
    MPEG2WithExtension                        = 0x0006,
    MPEG2AAC                                  = 0x0007,
    MPEG2Layer1LowSamplingFrequency           = 0x0008,
    MPEG2Layer2LowSamplingFrequency           = 0x0009,
    MPEG2Layer3LowSamplingFrequency           = 0x000a,
    DTSTypeI                                  = 0x000b,
    DTSTypeII                                 = 0x000c,
    DTSTypeIII                                = 0x000d,
    ATRAC                                     = 0x000e,
    ATRAC23                                   = 0x000f,
    ATRACX                                    = 0x0010,
    DTSTypeIV                                 = 0x0011,
    WMAProfessional                           = 0x0012,
      WMAProfessionalTypeI                    = 0x0012,
      WMAProfessionalTypeII                   = 0x0032,
      WMAProfessionalTypeIII                  = 0x0052,
      WMAProfessionalTypeIV                   = 0x0072,
    MPEG2AACLowSamplingFrequency              = 0x0013,
      MPEG2AACLowSamplingFrequency_2048       = 0x0013,
      MPEG2AACLowSamplingFrequency_4096       = 0x0033,
      MPEG2AACLowSamplingFrequency_Reserved1  = 0x0053,
      MPEG2AACLowSamplingFrequency_Reserved2  = 0x0073,
    MPEG4AAC                                  = 0x0014,
      MPEG4AAC_1024                           = 0x0014,
      MPEG4AAC_2048                           = 0x0034,
      MPEG4AAC_4096                           = 0x0054,
      MPEG4AAC_512                            = 0x0074,
    EnhancedAC3                               = 0x0015,
      Reserved1                               = 0x0035,
      Reserved2                               = 0x0055,
      Reserved3                               = 0x0075,
    MAT                                       = 0x0016,
      Reserved4                               = 0x0036,
      Reserved5                               = 0x0056,
      Reserved6                               = 0x0076,
    Reserved7                                 = 0x0017,
    Reserved8                                 = 0x0018,
    Reserved9                                 = 0x0019,
    Reserved10                                = 0x001a,
    SMPTE338M2                                = 0x001b,
      // SMPTE 338M                           = 0x001b,
      // SMPTE 338M                           = 0x003b,
      // SMPTE 338M                           = 0x005b,
      // SMPTE 338M                           = 0x007b,
    SMPTE338M3                                = 0x001c,
      // SMPTE 338M                           = 0x001c,
      // SMPTE 338M                           = 0x003c,
      // SMPTE 338M                           = 0x005c,
      // SMPTE 338M                           = 0x007c,
    SMPTE338M4                                = 0x001d,
      // SMPTE 338M                           = 0x001d,
      // SMPTE 338M                           = 0x003d,
      // SMPTE 338M                           = 0x005d,
      // SMPTE 338M                           = 0x007d,
    SMPTE338M5                                = 0x001e,
      // SMPTE 338M                           = 0x001e,
      // SMPTE 338M                           = 0x003e,
      // SMPTE 338M                           = 0x005e,
      // SMPTE 338M                           = 0x007e,
    Extended                                  = 0x001f,
      Extended1                               = 0x001f,
      Extended2                               = 0x003f,
      Extended3                               = 0x005f,
      Extended4                               = 0x007f,
  }
}