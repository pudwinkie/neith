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
  public delegate void waveInProc(IntPtr hwi, MM_WIM uMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2);
  public delegate void waveOutProc(IntPtr hwo, MM_WOM uMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2);

  public static class winmm {
    private const string dllname = "winmm.dll";

    [DllImport(dllname)] public static extern MMRESULT waveInAddBuffer(IntPtr hwi, ref WAVEHDR pwh, uint cbwh);
    [DllImport(dllname)] public static extern MMRESULT waveInClose(IntPtr hwi);
    [DllImport(dllname)] public static extern MMRESULT waveInGetDevCaps(uint uDeviceID, out WAVEINCAPS pwic, uint cbwic);
    [DllImport(dllname, CharSet = CharSet.Auto)] public static extern MMRESULT waveInGetErrorText(MMRESULT mmrError, IntPtr pszText, uint cchText);
    [DllImport(dllname)] public static extern MMRESULT waveInGetID(IntPtr hwi, out uint puDeviceID);
    [DllImport(dllname)] public static extern uint     waveInGetNumDevs();
    [DllImport(dllname)] public static extern MMRESULT waveInOpen(out IntPtr phwi, uint uDeviceID, ref WAVEFORMATEX pwfx, waveInProc dwCallback, IntPtr dwCallbackInstance, uint fdwOpen);
    [DllImport(dllname)] public static extern MMRESULT waveInPrepareHeader(IntPtr hwi, ref WAVEHDR pwh, uint cbwh);
    [DllImport(dllname)] public static extern MMRESULT waveInReset(IntPtr hwi);
    [DllImport(dllname)] public static extern MMRESULT waveInStart(IntPtr hwi);
    [DllImport(dllname)] public static extern MMRESULT waveInStop(IntPtr hwi);
    [DllImport(dllname)] public static extern MMRESULT waveInUnprepareHeader(IntPtr hwi, ref WAVEHDR pwh, uint cbwh);

    [DllImport(dllname)] public static extern unsafe MMRESULT waveInAddBuffer(IntPtr hwi, WAVEHDR* pwh, uint cbwh);
    [DllImport(dllname)] public static extern unsafe MMRESULT waveInPrepareHeader(IntPtr hwi, WAVEHDR* pwh, uint cbwh);
    [DllImport(dllname)] public static extern unsafe MMRESULT waveInUnprepareHeader(IntPtr hwi, WAVEHDR* pwh, uint cbwh);

    [DllImport(dllname)] public static extern MMRESULT waveOutBreakLoop(IntPtr hwo);
    [DllImport(dllname)] public static extern MMRESULT waveOutClose(IntPtr hwo);
    [DllImport(dllname)] public static extern MMRESULT waveOutGetDevCaps(uint uDeviceID, out WAVEOUTCAPS pwoc, uint cbwic);
    [DllImport(dllname, CharSet = CharSet.Auto)] public static extern MMRESULT waveOutGetErrorText(MMRESULT mmrError, IntPtr pszText, uint cchText);
    [DllImport(dllname)] public static extern MMRESULT waveOutGetID(IntPtr hwo, out uint puDeviceID);
    [DllImport(dllname)] public static extern uint     waveOutGetNumDevs();
    [DllImport(dllname)] public static extern MMRESULT waveOutGetPosition(IntPtr hwo, ref MMTIME pmmt, uint cbmmt);
    [DllImport(dllname)] public static extern MMRESULT waveOutGetVolume(IntPtr hwo, out uint dwVolume);
    [DllImport(dllname)] public static extern MMRESULT waveOutOpen(out IntPtr hwo, uint uDeviceID, ref WAVEFORMATEX pwfx, waveOutProc dwCallback, IntPtr dwCallbackInstance, uint fdwOpen);
    [DllImport(dllname)] public static extern MMRESULT waveOutPause(IntPtr hwo);
    [DllImport(dllname)] public static extern MMRESULT waveOutPrepareHeader(IntPtr hwo, ref WAVEHDR pwh, uint cbwh);
    [DllImport(dllname)] public static extern MMRESULT waveOutReset(IntPtr hwo);
    [DllImport(dllname)] public static extern MMRESULT waveOutRestart(IntPtr hwo);
    [DllImport(dllname)] public static extern MMRESULT waveOutSetVolume(IntPtr hwo, uint dwVolume);
    [DllImport(dllname)] public static extern MMRESULT waveOutUnprepareHeader(IntPtr hwo, ref WAVEHDR pwh, uint cbwh);
    [DllImport(dllname)] public static extern MMRESULT waveOutWrite(IntPtr hwo, ref WAVEHDR pwh, uint cbwh);

    [DllImport(dllname)] public static extern unsafe MMRESULT waveOutPrepareHeader(IntPtr hwo, WAVEHDR* pwh, uint cbwh);
    [DllImport(dllname)] public static extern unsafe MMRESULT waveOutUnprepareHeader(IntPtr hwo, WAVEHDR* pwh, uint cbwh);
    [DllImport(dllname)] public static extern unsafe MMRESULT waveOutWrite(IntPtr hwo, WAVEHDR* pwh, uint cbwh);

    [DllImport(dllname)] public extern static bool PlaySound(byte[] pszSound, IntPtr/*HMODULE*/ hmod, SND fdwSound);
    [DllImport(dllname, CharSet = CharSet.Auto)] public extern static bool PlaySound(string pszSound, IntPtr/*HMODULE*/ hmod, SND fdwSound);
    [DllImport(dllname)] public extern static bool PlaySound(IntPtr pszSound, IntPtr/*HMODULE*/ hmod, SND fdwSound);
  }
}