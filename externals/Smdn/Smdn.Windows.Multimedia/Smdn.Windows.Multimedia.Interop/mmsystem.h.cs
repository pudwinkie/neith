// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2009-2011 smdn
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
using System.Runtime.InteropServices;

using Smdn.Media;

namespace Smdn.Windows.Multimedia.Interop {
  public static partial class Consts {
    public const uint CALLBACK_TYPEMASK = 0x00070000;    /* callback type mask */
    public const uint CALLBACK_NULL     = 0x00000000;    /* no callback */
    public const uint CALLBACK_WINDOW   = 0x00010000;    /* dwCallback is a HWND */
    public const uint CALLBACK_TASK     = 0x00020000;    /* dwCallback is a HTASK */
    public const uint CALLBACK_THREAD   = CALLBACK_TASK; /* dwCallback is a thread ID */
    public const uint CALLBACK_FUNCTION = 0x00030000;    /* dwCallback is a FARPROC */
    public const uint CALLBACK_EVENT    = 0x00050000;    /* dwCallback is an EVENT Handler */

    public const uint WAVE_FORMAT_QUERY        = 0x00000001;
    public const uint WAVE_ALLOWSYNC           = 0x00000002;
    public const uint WAVE_MAPPED              = 0x00000004;
    public const uint WAVE_FORMAT_DIRECT       = 0x00000008;
    public const uint WAVE_FORMAT_DIRECT_QUERY = WAVE_FORMAT_QUERY | WAVE_FORMAT_DIRECT;

    public const int MAXPNAMELEN = 32;
    public const int MAXERRORLENGTH = 256;
  }

  [Flags]
  public enum WAVEHDR_FLAG : uint {
    WHDR_DONE      = 1 << 0,
    WHDR_PREPARED  = 1 << 1,
    WHDR_BEGINLOOP = 1 << 2,
    WHDR_ENDLOOP   = 1 << 3,
    WHDR_INQUEUE   = 1 << 4,
  }

  [StructLayout(LayoutKind.Sequential, Pack = 0)]
  public struct WAVEHDR {
    public IntPtr lpData;
    public uint dwBufferLength;
    public uint dwBytesRecorded;
    public IntPtr dwUser;
    public WAVEHDR_FLAG dwFlags;
    public uint dwLoops;
    public IntPtr lpNext;
    public IntPtr reserved;

    public static readonly uint Size = (uint)Marshal.SizeOf(typeof(WAVEHDR));
  }

  [Flags]
  public enum WAVECAPS : uint {
    WAVECAPS_PITCH          = 1 << 0,
    WAVECAPS_PLAYBACKRATE   = 1 << 1,
    WAVECAPS_VOLUME         = 1 << 2,
    WAVECAPS_LRVOLUME       = 1 << 3,
    WAVECAPS_SYNC           = 1 << 4,
    WAVECAPS_SAMPLEACCURATE = 1 << 5,
    WAVECAPS_DIRECTSOUND    = 1 << 6,
  }

  [StructLayout(LayoutKind.Sequential, Pack = 0)]
  public struct WAVEOUTCAPS {
    public ushort wMid;
    public ushort wPid;
    public uint /*MMVERSION*/ vDriverVersion;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Consts.MAXPNAMELEN)] public string szTypeName;
    public WAVE_FORMAT dwFormats;
    public ushort wChannels;
    public ushort wReserved1;
    public uint dwSupport;

    public static readonly uint Size = (uint)Marshal.SizeOf(typeof(WAVEOUTCAPS));
  }

  [StructLayout(LayoutKind.Sequential, Pack = 0)]
  public struct WAVEINCAPS {
    public ushort wMid;
    public ushort wPid;
    public uint /*MMVERSION*/ vDriverVersion;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Consts.MAXPNAMELEN)] public string szTypeName;
    public WAVE_FORMAT dwFormats;
    public ushort wChannels;
    public ushort wReserved1;

    public static readonly uint Size = (uint)Marshal.SizeOf(typeof(WAVEINCAPS));
  }

  public enum MMRESULT : uint {
    MMSYSERR_BASE           = 0x00000000,
    MMSYSERR_NOERROR        = 0x00000000, /* no error */
    MMSYSERR_ERROR          = MMSYSERR_BASE + 1, /* unspecified error */
    MMSYSERR_BADDEVICEID    = MMSYSERR_BASE + 2, /* device ID out of range */
    MMSYSERR_NOTENABLED     = MMSYSERR_BASE + 3, /* driver failed enable */
    MMSYSERR_ALLOCATED      = MMSYSERR_BASE + 4, /* device already allocated */
    MMSYSERR_INVALHANDLE    = MMSYSERR_BASE + 5, /* device handle is invalid */
    MMSYSERR_NODRIVER       = MMSYSERR_BASE + 6, /* no device driver present */
    MMSYSERR_NOMEM          = MMSYSERR_BASE + 7, /* memory allocation error */
    MMSYSERR_NOTSUPPORTED   = MMSYSERR_BASE + 8, /* function isn't supported */
    MMSYSERR_BADERRNUM      = MMSYSERR_BASE + 9, /* error value out of range */
    MMSYSERR_INVALFLAG      = MMSYSERR_BASE + 10, /* invalid flag passed */
    MMSYSERR_INVALPARAM     = MMSYSERR_BASE + 11, /* invalid parameter passed */
    MMSYSERR_HANDLEBUSY     = MMSYSERR_BASE + 12, /* handle being used */
    MMSYSERR_INVALIDALIAS   = MMSYSERR_BASE + 13, /* specified alias not found */
    MMSYSERR_BADDB          = MMSYSERR_BASE + 14, /* bad registry database */
    MMSYSERR_KEYNOTFOUND    = MMSYSERR_BASE + 15, /* registry key not found */
    MMSYSERR_READERROR      = MMSYSERR_BASE + 16, /* registry read error */
    MMSYSERR_WRITEERROR     = MMSYSERR_BASE + 17, /* registry write error */
    MMSYSERR_DELETEERROR    = MMSYSERR_BASE + 18, /* registry delete error */
    MMSYSERR_VALNOTFOUND    = MMSYSERR_BASE + 19, /* registry value not found */
    MMSYSERR_NODRIVERCB     = MMSYSERR_BASE + 20, /* driver does not call */
    MMSYSERR_MOREDATA       = MMSYSERR_BASE + 21, /* more data to be returned */
    MMSYSERR_LASTERROR      = MMSYSERR_BASE + 21, /* last error in range */
  }

  public enum WAVERR : uint {
    WAVERR_BASE         = 32,
    WAVERR_BADFORMAT    = WAVERR_BASE + 0, /* unsupported wave format */
    WAVERR_STILLPLAYING = WAVERR_BASE + 1, /* still something playing */
    WAVERR_UNPREPARED   = WAVERR_BASE + 2, /* header not prepared */
    WAVERR_SYNC         = WAVERR_BASE + 3, /* device is synchronous */
    WAVERR_LASTERROR    = WAVERR_BASE + 3, /* last error in range */
  }

  public enum MIXERR : uint {
    MIXERR_BASE         = 1024,
    MIXERR_INVALLINE    = MIXERR_BASE + 0,
    MIXERR_INVALCONTROL = MIXERR_BASE + 1,
    MIXERR_INVALVALUE   = MIXERR_BASE + 2,
    MIXERR_LASTERROR    = MIXERR_BASE + 2,
  }

  public enum MM_WOM : uint {
    MM_WOM_OPEN  = 0x000003bb,
    MM_WOM_CLOSE = 0x000003bc,
    MM_WOM_DONE  = 0x000003bd,
  }

  public enum MM_WIM : uint {
    MM_WIM_OPEN  = 0x000003be,
    MM_WIM_CLOSE = 0x000003bf,
    MM_WIM_DATA  = 0x000003c0,
  }

  public enum TIME : uint {
    TIME_MS       = 1 << 0,
    TIME_SAMPLES  = 1 << 1,
    TIME_BYTES    = 1 << 2,
    TIME_SMPTE    = 1 << 3,
    TIME_MIDI     = 1 << 4,
    TIME_TICKS    = 1 << 5,
  }

  [StructLayout(LayoutKind.Sequential, Pack = 0)]
  public struct MMTIME_SMPTE {
    public byte hour;
    public byte min;
    public byte sec;
    public byte frame;
    public byte fps;
    public byte dummy;
    public byte pad0;
    public byte pad1;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 0)]
  public struct MMTIME_MIDI {
    public uint songptrpos;
  }

  [StructLayout(LayoutKind.Explicit)]
  public /*union*/ struct MMTIME_UNIT {
    [FieldOffset(0)] public uint ms;
    [FieldOffset(0)] public uint sample;
    [FieldOffset(0)] public uint cb;
    [FieldOffset(0)] public uint ticks;
    [FieldOffset(0)] public MMTIME_SMPTE smpte;
    [FieldOffset(0)] public MMTIME_MIDI midi;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 0)]
  public struct MMTIME {
    public TIME wType;
    public MMTIME_UNIT u;

    public MMTIME(TIME wType)
    {
      this.wType = wType;
      this.u = new MMTIME_UNIT();
    }

    public static readonly uint Size = (uint)Marshal.SizeOf(typeof(MMTIME));
  }

  [Flags]
  public enum SND : uint {
    None        = 0,

    SYNC        = 0,
    ASYNC       = 1,
    NODEFAULT   = 2,
    MEMORY      = 4,
    LOOP        = 8,
    NOSTOP      = 16,
    NOWAIT      = 0x2000,
    ALIAS       = 0x10000,
    ALIAS_ID    = 0x110000,
    FILENAME    = 0x20000,
    RESOURCE    = 0x40004,
    PURGE       = 0x40,

    APPLICATION = 0x80,

    // SENTRY = ?
    // SYSTEM = ?

    ALIAS_START = 0,
    ALIAS_SYSTEMASTERISK    = ALIAS_START + (uint)('*' | ('S' << 8)), // sndAlias('S','*')
    ALIAS_SYSTEMQUESTION    = ALIAS_START + (uint)('?' | ('S' << 8)), // sndAlias('S','?')
    ALIAS_SYSTEMHAND        = ALIAS_START + (uint)('H' | ('S' << 8)), // sndAlias('S','H')
    ALIAS_SYSTEMEXIT        = ALIAS_START + (uint)('E' | ('S' << 8)), // sndAlias('S','E')
    ALIAS_SYSTEMSTART       = ALIAS_START + (uint)('S' | ('S' << 8)), // sndAlias('S','S')
    ALIAS_SYSTEMWELCOME     = ALIAS_START + (uint)('W' | ('S' << 8)), // sndAlias('S','W')
    ALIAS_SYSTEMEXCLAMATION = ALIAS_START + (uint)('!' | ('S' << 8)), // sndAlias('S','!')
    ALIAS_SYSTEMDEFAULT     = ALIAS_START + (uint)('D' | ('S' << 8)), // sndAlias('S','D')
    //#define sndAlias(c0,c1) (SND_ALIAS_START+(DWORD)(BYTE)(c0)|((DWORD)(BYTE)(c1)<<8))
  }
}