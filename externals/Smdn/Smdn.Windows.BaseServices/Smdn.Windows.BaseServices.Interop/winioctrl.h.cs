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

namespace Smdn.Windows.BaseServices.Interop {
  public static partial class Consts {
    public const uint FILE_ANY_ACCESS      = 0x00000000;
    public const uint FILE_SPECIAL_ACCESS  = FILE_ANY_ACCESS;
    public const uint FILE_READ_ACCESS     = 0x00000001;
    public const uint FILE_WRITE_ACCESS    = 0x00000002;

    public const uint VALID_NTFT = 0xc0;

    public const uint SERIAL_LSRMST_ESCAPE      = 0;
    public const uint SERIAL_LSRMST_LSR_DATA    = 1;
    public const uint SERIAL_LSRMST_LSR_NODATA  = 2;
    public const uint SERIAL_LSRMST_MST         = 3;

    public const byte SCSI_IOCTL_DATA_OUT         = 0;
    public const byte SCSI_IOCTL_DATA_IN          = 1;
    public const byte SCSI_IOCTL_DATA_UNSPECIFIED = 2;

    public const uint METHOD_BUFFERED    = 0;
    public const uint METHOD_IN_DIRECT   = 1;
    public const uint METHOD_OUT_DIRECT  = 2;
    public const uint METHOD_NEITHER     = 3;

    public static readonly uint IOCTL_STORAGE_BASE                = (uint)FILE_DEVICE.MASS_STORAGE;
    public static readonly uint IOCTL_STORAGE_CHECK_VERIFY        = CTL_CODE(IOCTL_STORAGE_BASE, 0x0200, METHOD_BUFFERED, FILE_READ_ACCESS);
    public static readonly uint IOCTL_STORAGE_CHECK_VERIFY2       = CTL_CODE(IOCTL_STORAGE_BASE, 0x0200, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_STORAGE_MEDIA_REMOVAL       = CTL_CODE(IOCTL_STORAGE_BASE, 0x0201, METHOD_BUFFERED, FILE_READ_ACCESS);
    public static readonly uint IOCTL_STORAGE_EJECT_MEDIA         = CTL_CODE(IOCTL_STORAGE_BASE, 0x0202, METHOD_BUFFERED, FILE_READ_ACCESS);
    public static readonly uint IOCTL_STORAGE_LOAD_MEDIA          = CTL_CODE(IOCTL_STORAGE_BASE, 0x0203, METHOD_BUFFERED, FILE_READ_ACCESS);
    public static readonly uint IOCTL_STORAGE_LOAD_MEDIA2         = CTL_CODE(IOCTL_STORAGE_BASE, 0x0203, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_STORAGE_RESERVE             = CTL_CODE(IOCTL_STORAGE_BASE, 0x0204, METHOD_BUFFERED, FILE_READ_ACCESS);
    public static readonly uint IOCTL_STORAGE_RELEASE             = CTL_CODE(IOCTL_STORAGE_BASE, 0x0205, METHOD_BUFFERED, FILE_READ_ACCESS);
    public static readonly uint IOCTL_STORAGE_FIND_NEW_DEVICES    = CTL_CODE(IOCTL_STORAGE_BASE, 0x0206, METHOD_BUFFERED, FILE_READ_ACCESS);
    public static readonly uint IOCTL_STORAGE_EJECTION_CONTROL    = CTL_CODE(IOCTL_STORAGE_BASE, 0x0250, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_STORAGE_MCN_CONTROL         = CTL_CODE(IOCTL_STORAGE_BASE, 0x0251, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_STORAGE_GET_MEDIA_TYPES     = CTL_CODE(IOCTL_STORAGE_BASE, 0x0300, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_STORAGE_GET_MEDIA_TYPES_EX  = CTL_CODE(IOCTL_STORAGE_BASE, 0x0301, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_STORAGE_RESET_BUS           = CTL_CODE(IOCTL_STORAGE_BASE, 0x0400, METHOD_BUFFERED, FILE_READ_ACCESS);
    public static readonly uint IOCTL_STORAGE_RESET_DEVICE        = CTL_CODE(IOCTL_STORAGE_BASE, 0x0401, METHOD_BUFFERED, FILE_READ_ACCESS);
    public static readonly uint IOCTL_STORAGE_GET_DEVICE_NUMBER   = CTL_CODE(IOCTL_STORAGE_BASE, 0x0420, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_STORAGE_PREDICT_FAILURE     = CTL_CODE(IOCTL_STORAGE_BASE, 0x0440, METHOD_BUFFERED, FILE_ANY_ACCESS);

    public static readonly uint IOCTL_DISK_BASE                     = (uint)FILE_DEVICE.DISK;
    public static readonly uint IOCTL_DISK_GET_DRIVE_GEOMETRY       = CTL_CODE(IOCTL_DISK_BASE, 0, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_DISK_GET_PARTITION_INFO       = CTL_CODE(IOCTL_DISK_BASE, 1, METHOD_BUFFERED, FILE_READ_ACCESS);
    public static readonly uint IOCTL_DISK_SET_PARTITION_INFO       = CTL_CODE(IOCTL_DISK_BASE, 2, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);
    public static readonly uint IOCTL_DISK_GET_DRIVE_LAYOUT         = CTL_CODE(IOCTL_DISK_BASE, 3, METHOD_BUFFERED, FILE_READ_ACCESS);
    public static readonly uint IOCTL_DISK_SET_DRIVE_LAYOUT         = CTL_CODE(IOCTL_DISK_BASE, 4, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);
    public static readonly uint IOCTL_DISK_VERIFY                   = CTL_CODE(IOCTL_DISK_BASE, 5, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_DISK_FORMAT_TRACKS            = CTL_CODE(IOCTL_DISK_BASE, 6, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);
    public static readonly uint IOCTL_DISK_REASSIGN_BLOCKS          = CTL_CODE(IOCTL_DISK_BASE, 7, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);
    public static readonly uint IOCTL_DISK_PERFORMANCE              = CTL_CODE(IOCTL_DISK_BASE, 8, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_DISK_IS_WRITABLE              = CTL_CODE(IOCTL_DISK_BASE, 9, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_DISK_LOGGING                  = CTL_CODE(IOCTL_DISK_BASE, 10, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_DISK_FORMAT_TRACKS_EX         = CTL_CODE(IOCTL_DISK_BASE, 11, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);
    public static readonly uint IOCTL_DISK_HISTOGRAM_STRUCTURE      = CTL_CODE(IOCTL_DISK_BASE, 12, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_DISK_HISTOGRAM_DATA           = CTL_CODE(IOCTL_DISK_BASE, 13, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_DISK_HISTOGRAM_RESET          = CTL_CODE(IOCTL_DISK_BASE, 14, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_DISK_REQUEST_STRUCTURE        = CTL_CODE(IOCTL_DISK_BASE, 15, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_DISK_REQUEST_DATA             = CTL_CODE(IOCTL_DISK_BASE, 16, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_DISK_GET_PARTITION_INFO_EX    = CTL_CODE(IOCTL_DISK_BASE, 0x12, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_DISK_SET_PARTITION_INFO_EX    = CTL_CODE(IOCTL_DISK_BASE, 0x13, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);
    public static readonly uint IOCTL_DISK_GET_DRIVE_LAYOUT_EX      = CTL_CODE(IOCTL_DISK_BASE, 0x14, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_DISK_SET_DRIVE_LAYOUT_EX      = CTL_CODE(IOCTL_DISK_BASE, 0x15, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);
    public static readonly uint IOCTL_DISK_CREATE_DISK              = CTL_CODE(IOCTL_DISK_BASE, 0x16, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);
    public static readonly uint IOCTL_DISK_GET_LENGTH_INFO          = CTL_CODE(IOCTL_DISK_BASE, 0x17, METHOD_BUFFERED, FILE_READ_ACCESS);
    public static readonly uint IOCTL_DISK_PERFORMANCE_OFF          = CTL_CODE(IOCTL_DISK_BASE, 0x18, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_DISK_GET_DRIVE_GEOMETRY_EX    = CTL_CODE(IOCTL_DISK_BASE, 0x28, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_DISK_GROW_PARTITION           = CTL_CODE(IOCTL_DISK_BASE, 0x34, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);
    public static readonly uint IOCTL_DISK_GET_CACHE_INFORMATION    = CTL_CODE(IOCTL_DISK_BASE, 0x35, METHOD_BUFFERED, FILE_READ_ACCESS);
    public static readonly uint IOCTL_DISK_SET_CACHE_INFORMATION    = CTL_CODE(IOCTL_DISK_BASE, 0x36, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);
    public static readonly uint IOCTL_DISK_DELETE_DRIVE_LAYOUT      = CTL_CODE(IOCTL_DISK_BASE, 0x40, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);
    public static readonly uint IOCTL_DISK_UPDATE_PROPERTIES        = CTL_CODE(IOCTL_DISK_BASE, 0x50, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_DISK_CHECK_VERIFY             = CTL_CODE(IOCTL_DISK_BASE, 0x200, METHOD_BUFFERED, FILE_READ_ACCESS);
    public static readonly uint IOCTL_DISK_MEDIA_REMOVAL            = CTL_CODE(IOCTL_DISK_BASE, 0x201, METHOD_BUFFERED, FILE_READ_ACCESS);
    public static readonly uint IOCTL_DISK_EJECT_MEDIA              = CTL_CODE(IOCTL_DISK_BASE, 0x202, METHOD_BUFFERED, FILE_READ_ACCESS);
    public static readonly uint IOCTL_DISK_LOAD_MEDIA               = CTL_CODE(IOCTL_DISK_BASE, 0x203, METHOD_BUFFERED, FILE_READ_ACCESS);
    public static readonly uint IOCTL_DISK_RESERVE                  = CTL_CODE(IOCTL_DISK_BASE, 0x204, METHOD_BUFFERED, FILE_READ_ACCESS);
    public static readonly uint IOCTL_DISK_RELEASE                  = CTL_CODE(IOCTL_DISK_BASE, 0x205, METHOD_BUFFERED, FILE_READ_ACCESS);
    public static readonly uint IOCTL_DISK_FIND_NEW_DEVICES         = CTL_CODE(IOCTL_DISK_BASE, 0x206, METHOD_BUFFERED, FILE_READ_ACCESS);
    public static readonly uint IOCTL_DISK_REMOVE_DEVICE            = CTL_CODE(IOCTL_DISK_BASE, 0x207, METHOD_BUFFERED, FILE_READ_ACCESS);
    public static readonly uint IOCTL_DISK_GET_MEDIA_TYPES          = CTL_CODE(IOCTL_DISK_BASE, 0x300, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_DISK_UPDATE_DRIVE_SIZE        = CTL_CODE(IOCTL_DISK_BASE, 0x0032, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);
    public static readonly uint IOCTL_SERIAL_LSRMST_INSERT          = CTL_CODE((uint)FILE_DEVICE.SERIAL_PORT, 31, METHOD_BUFFERED, FILE_ANY_ACCESS);

    private static readonly uint FSCTL_BASE                   = (uint)FILE_DEVICE.FILE_SYSTEM;
    public static readonly uint FSCTL_LOCK_VOLUME             = CTL_CODE(FSCTL_BASE, 6, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint FSCTL_UNLOCK_VOLUME           = CTL_CODE(FSCTL_BASE, 7, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint FSCTL_DISMOUNT_VOLUME         = CTL_CODE(FSCTL_BASE, 8, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint FSCTL_MOUNT_DBLS_VOLUME       = CTL_CODE(FSCTL_BASE, 13, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint FSCTL_GET_COMPRESSION         = CTL_CODE(FSCTL_BASE, 15, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint FSCTL_SET_COMPRESSION         = CTL_CODE(FSCTL_BASE, 16, METHOD_BUFFERED, FILE_READ_DATA | FILE_WRITE_DATA);
    public static readonly uint FSCTL_READ_COMPRESSION        = CTL_CODE(FSCTL_BASE, 17, METHOD_NEITHER, FILE_READ_DATA);
    public static readonly uint FSCTL_WRITE_COMPRESSION       = CTL_CODE(FSCTL_BASE, 18, METHOD_NEITHER, FILE_WRITE_DATA);
    public static readonly uint FSCTL_GET_NTFS_VOLUME_DATA    = CTL_CODE(FSCTL_BASE, 25, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint FSCTL_GET_VOLUME_BITMAP       = CTL_CODE(FSCTL_BASE, 27, METHOD_NEITHER, FILE_ANY_ACCESS);
    public static readonly uint FSCTL_GET_RETRIEVAL_POINTERS  = CTL_CODE(FSCTL_BASE, 28, METHOD_NEITHER, FILE_ANY_ACCESS);
    public static readonly uint FSCTL_MOVE_FILE               = CTL_CODE(FSCTL_BASE, 29, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint FSCTL_GET_REPARSE_POINT       = CTL_CODE(FSCTL_BASE, 42, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint FSCTL_SET_REPARSE_POINT       = CTL_CODE(FSCTL_BASE, 41, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint FSCTL_DELETE_REPARSE_POINT    = CTL_CODE(FSCTL_BASE, 43, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint FSCTL_SET_SPARSE              = CTL_CODE(FSCTL_BASE, 49, METHOD_BUFFERED, FILE_SPECIAL_ACCESS);

    private static readonly uint IOCTL_SCSI_BASE              = (uint)FILE_DEVICE.CONTROLLER;
    public static readonly uint IOCTL_SCSI_PASS_THROUGH         = CTL_CODE(IOCTL_SCSI_BASE, 0x0401, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);
    public static readonly uint IOCTL_SCSI_MINIPORT             = CTL_CODE(IOCTL_SCSI_BASE, 0x0402, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);
    public static readonly uint IOCTL_SCSI_GET_INQUIRY_DATA     = CTL_CODE(IOCTL_SCSI_BASE, 0x0403, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_SCSI_GET_CAPABILITIES     = CTL_CODE(IOCTL_SCSI_BASE, 0x0404, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_SCSI_PASS_THROUGH_DIRECT  = CTL_CODE(IOCTL_SCSI_BASE, 0x0405, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);
    public static readonly uint IOCTL_SCSI_GET_ADDRESS          = CTL_CODE(IOCTL_SCSI_BASE, 0x0406, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_SCSI_RESCAN_BUS           = CTL_CODE(IOCTL_SCSI_BASE, 0x0407, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_SCSI_GET_DUMP_POINTERS    = CTL_CODE(IOCTL_SCSI_BASE, 0x0408, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_SCSI_FREE_DUMP_POINTERS   = CTL_CODE(IOCTL_SCSI_BASE, 0x0409, METHOD_BUFFERED, FILE_ANY_ACCESS);
    public static readonly uint IOCTL_IDE_PASS_THROUGH          = CTL_CODE(IOCTL_SCSI_BASE, 0x040a, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);

    public static uint CTL_CODE(uint t, uint f, uint m, uint a)
    {
      return (uint)((t << 16) | (a << 14) | (f << 2) | m);
    }

    public static uint DEVICE_TYPE_FROM_CTL_CODE(uint c)
    {
      return (uint)((c & 0xffff0000) >> 16);
    }
  }

  public enum BOOLEAN : byte {
    FALSE = 0,
    TRUE  = 1,
  }

  [CLSCompliant(false)]
  public enum FILE_DEVICE : uint {
    BEEP                    = 1,
    CD_ROM                  = 2,
    CD_ROM_FILE_SYSTEM      = 3,
    CONTROLLER              = 4,
    DATALINK                = 5,
    DFS                     = 6,
    DISK                    = 7,
    DISK_FILE_SYSTEM        = 8,
    FILE_SYSTEM             = 9,
    INPORT_PORT             = 10,
    KEYBOARD                = 11,
    MAILSLOT                = 12,
    MIDI_IN                 = 13,
    MIDI_OUT                = 14,
    MOUSE                   = 15,
    MULTI_UNC_PROVIDER      = 16,
    NAMED_PIPE              = 17,
    NETWORK                 = 18,
    NETWORK_BROWSER         = 19,
    NETWORK_FILE_SYSTEM     = 20,
    NULL                    = 21,
    PARALLEL_PORT           = 22,
    PHYSICAL_NETCARD        = 23,
    PRINTER                 = 24,
    SCANNER                 = 25,
    SERIAL_MOUSE_PORT       = 26,
    SERIAL_PORT             = 27,
    SCREEN                  = 28,
    SOUND                   = 29,
    STREAMS                 = 30,
    TAPE                    = 31,
    TAPE_FILE_SYSTEM        = 32,
    TRANSPORT               = 33,
    UNKNOWN                 = 34,
    VIDEO                   = 35,
    VIRTUAL_DISK            = 36,
    WAVE_IN                 = 37,
    WAVE_OUT                = 38,
    DEVICE_8042_PORT        = 39, // FILE_DEVICE_8042_PORT
    NETWORK_REDIRECTOR      = 40,
    BATTERY                 = 41,
    BUS_EXTENDER            = 42,
    MODEM                   = 43,
    VDM                     = 44,
    MASS_STORAGE            = 45,
    SMB                     = 46,
    KS                      = 47,
    CHANGER                 = 48,
    SMARTCARD               = 49,
    ACPI                    = 50,
    DVD                     = 51,
    FULLSCREEN_VIDEO        = 52,
    DFS_FILE_SYSTEM         = 53,
    DFS_VOLUME              = 54,
    SERENUM                 = 55,
    TERMSRV                 = 56,
    KSEC                    = 57
  }

  [CLSCompliant(false)]
  public enum PARTITION : uint {
    ENTRY_UNUSED    = 0,
    FAT_12          = 1,
    XENIX_1         = 2,
    XENIX_2         = 3,
    FAT_16          = 4,
    EXTENDED        = 5,
    HUGE            = 6,
    IFS             = 7,
    FAT32           = 0x0B,
    FAT32_XINT13    = 0x0C,
    XINT13          = 0x0E,
    XINT13_EXTENDED = 0x0F,
    PREP            = 0x41,
    LDM             = 0x42,
    UNIX            = 0x63,
    NTFT            = 128,
  }

  [CLSCompliant(false), StructLayout(LayoutKind.Sequential)]
  public struct PREVENT_MEDIA_REMOVAL {
    public BOOLEAN PreventMediaRemoval;
  }

  [CLSCompliant(false), StructLayout(LayoutKind.Sequential)]
  public unsafe struct SCSI_PASS_THROUGH {
    public ushort Length;
    public byte   ScsiStatus;
    public byte   PathId;
    public byte   TargetId;
    public byte   Lun;
    public byte   CdbLength;
    public byte   SenseInfoLength;
    public byte   DataIn;
    public uint   DataTransferLength;
    public uint   TimeOutValue;
    public IntPtr   DataBufferOffset;
    public uint   SenseInfoOffset;
    public fixed byte Cdb[16];

    public static readonly int Size = Marshal.SizeOf(typeof(SCSI_PASS_THROUGH));
  }

  [CLSCompliant(false), StructLayout(LayoutKind.Sequential)]
  public unsafe struct SCSI_PASS_THROUGH_DIRECT {
    public ushort Length;
    public byte   ScsiStatus;
    public byte   PathId;
    public byte   TargetId;
    public byte   Lun;
    public byte   CdbLength;
    public byte   SenseInfoLength;
    public byte   DataIn;
    public uint   DataTransferLength;
    public uint   TimeOutValue;
    public void*  DataBuffer;
    public uint   SenseInfoOffset;
    public fixed byte Cdb[16];

    public static readonly int Size = Marshal.SizeOf(typeof(SCSI_PASS_THROUGH_DIRECT));
  }
}
