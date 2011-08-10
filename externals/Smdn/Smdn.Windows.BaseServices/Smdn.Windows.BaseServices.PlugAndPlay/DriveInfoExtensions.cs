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
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

using Smdn.Windows.BaseServices.Interop;

namespace Smdn.Windows.BaseServices.PlugAndPlay {
  public static class DriveInfoExtensions {
#region "disposable CreateFile/CloseHandle wrapper"
    private class Volume : IDisposable {
      public DriveInfo Drive {
        get; private set;
      }

      public IntPtr Handle {
        get; private set;
      }

      public Volume(DriveInfo drive, uint accessFlags)
      {
        this.Drive = drive;
        this.Handle = kernel32.CreateFile(string.Format(@"\\.\{0}:", drive.Name[0]),
                                          accessFlags,
                                          FILE_SHARE.READ | FILE_SHARE.WRITE,
                                          IntPtr.Zero,
                                          Consts.OPEN_EXISTING,
                                          FILE_ATTRIBUTE.None,
                                          IntPtr.Zero);

        if (this.Handle == IntPtr.Zero)
          throw new Win32Exception(Marshal.GetLastWin32Error());
      }

      public void Dispose()
      {
        if (Handle != IntPtr.Zero) {
          Smdn.Interop.kernel32.CloseHandle(Handle); // ignore any error
          Handle = IntPtr.Zero;
        }
      }
    }
#endregion

    public static bool HasMedia(this DriveInfo drive)
    {
      using (var volume = OpenRemovableDrive(drive)) {
        return HasMedia(volume);
      }
    }

    private static bool HasMedia(Volume volume)
    {
      return kernel32.DeviceIoControl(volume.Handle, Consts.IOCTL_STORAGE_CHECK_VERIFY);
    }

    public static bool IsTrayOpened(this DriveInfo drive)
    {
      using (var volume = OpenRemovableDrive(drive, Consts.GENERIC_WRITE)) {
        return IsTrayOpened(volume);
      }
    }

    private unsafe static bool IsTrayOpened(Volume volume)
    {
      /*
       * http://www.eggheadcafe.com/conversation.aspx?messageid=33820121&threadid=33794406
       * http://forum.sources.ru/index.php?showtopic=225102
       */
      byte* buffer = stackalloc byte[8];
      SCSI_PASS_THROUGH_DIRECT* sptd = stackalloc SCSI_PASS_THROUGH_DIRECT[1];

      // XXX
      sptd[0].Length = (ushort)SCSI_PASS_THROUGH_DIRECT.Size;
      sptd[0].PathId = 0;
      sptd[0].TargetId = 0;
      sptd[0].CdbLength = 12;
      sptd[0].DataIn = Consts.SCSI_IOCTL_DATA_IN;
      sptd[0].DataTransferLength = 8;
      sptd[0].TimeOutValue = 5;
      sptd[0].DataBuffer = (void*)buffer;
      sptd[0].Cdb[0] = 0xbd; // mechanism status
      sptd[0].Cdb[9] = 8; // timeout value

      uint bytesReturned;

      if (!kernel32.DeviceIoControl(volume.Handle, Consts.IOCTL_SCSI_PASS_THROUGH_DIRECT, (void*)sptd, (uint)SCSI_PASS_THROUGH_DIRECT.Size, (void*)sptd, (uint)SCSI_PASS_THROUGH_DIRECT.Size, out bytesReturned, IntPtr.Zero))
        throw new Win32Exception(Marshal.GetLastWin32Error());

      return ((buffer[1] & 0x10) == 0x10);
    }

#region "eject/load removable drive"
    private static readonly int defaultMaxRetry = 20;
    private static readonly TimeSpan defaultTimeout = TimeSpan.FromMilliseconds(10000);

    public static void Toggle(this DriveInfo drive)
    {
      Toggle(drive, defaultTimeout, defaultMaxRetry);
    }

    public static void Toggle(this DriveInfo drive, int millisecondsTimeout, int maxRetry)
    {
      Toggle(drive, TimeSpan.FromMilliseconds(millisecondsTimeout), maxRetry);
    }

    public static void Toggle(this DriveInfo drive, TimeSpan timeout, int maxRetry)
    {
      using (var volume = OpenRemovableDrive(drive, Consts.GENERIC_WRITE)) {
        if (IsTrayOpened(volume))
          Load(volume);
        else
          Eject(volume, timeout, maxRetry);
      }
    }

    public static void Load(this DriveInfo drive)
    {
      using (var volume = OpenRemovableDrive(drive)) {
        Load(volume);
      }
    }

    private static void Load(Volume volume)
    {
      if (!kernel32.DeviceIoControl(volume.Handle, Consts.IOCTL_STORAGE_LOAD_MEDIA))
        throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    public static void Eject(this DriveInfo drive)
    {
      Eject(drive, defaultTimeout, defaultMaxRetry);
    }

    public static void Eject(this DriveInfo drive, int millisecondsTimeout, int maxRetry)
    {
      Eject(drive, TimeSpan.FromMilliseconds(millisecondsTimeout), maxRetry);
    }

    public static void Eject(this DriveInfo drive, TimeSpan timeout, int maxRetry)
    {
      using (var volume = OpenRemovableDrive(drive)) {
        Eject(volume, timeout, maxRetry);
      }
    }

    private static void Eject(Volume volume, TimeSpan timeout, int maxRetry)
    {
      /*
       * http://support.microsoft.com/kb/165721/en
       */
      if (timeout < TimeSpan.Zero)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("timeout", timeout);
      if (maxRetry < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("maxRetry", maxRetry);

      // Lock the volume
      var locked = false;

      for (var t = 0; t < maxRetry; t++) {
        if (kernel32.DeviceIoControl(volume.Handle, Consts.FSCTL_LOCK_VOLUME)) {
          locked = true;
          break;
        }
        else {
          System.Threading.Thread.Sleep((int)(timeout.TotalMilliseconds / maxRetry));
        }
      }

      if (!locked)
        throw new IOException("device busy"); // XXX:

      // Dismount the volume
      if (!kernel32.DeviceIoControl(volume.Handle, Consts.FSCTL_DISMOUNT_VOLUME))
        throw new Win32Exception(Marshal.GetLastWin32Error());

      // Set prevent removal to false
      unsafe {
        uint bytesReturned;
        PREVENT_MEDIA_REMOVAL* inBuffer = stackalloc PREVENT_MEDIA_REMOVAL[1];

        inBuffer[0].PreventMediaRemoval = BOOLEAN.FALSE;

        if (!kernel32.DeviceIoControl(volume.Handle, Consts.IOCTL_STORAGE_MEDIA_REMOVAL, (void*)inBuffer, (uint)Marshal.SizeOf(typeof(PREVENT_MEDIA_REMOVAL)), IntPtr.Zero, 0, out bytesReturned, IntPtr.Zero))
          throw new Win32Exception(Marshal.GetLastWin32Error());
      }

      // Eject the volume
      if (!kernel32.DeviceIoControl(volume.Handle, Consts.IOCTL_STORAGE_EJECT_MEDIA))
        throw new Win32Exception(Marshal.GetLastWin32Error());
    }
#endregion

#region "utility methods"
    private static Volume OpenRemovableDrive(DriveInfo drive)
    {
      return OpenRemovableDrive(drive, 0);
    }

    private static Volume OpenRemovableDrive(DriveInfo drive, uint accessFlags)
    {
      if (drive.DriveType == DriveType.CDRom)
        return new Volume(drive, Consts.GENERIC_READ | accessFlags);
      else if (drive.DriveType == DriveType.Removable)
        return new Volume(drive, Consts.GENERIC_READ | Consts.GENERIC_WRITE | accessFlags);
      else
        throw new NotSupportedException("drive is not removable");
    }
#endregion
  }
}
