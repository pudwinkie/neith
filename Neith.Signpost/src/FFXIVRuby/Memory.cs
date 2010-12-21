using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FFXIVRuby
{
    public class Memory
    {
        // Fields

        // Methods
        public IEnumerable<MEMORY_BASIC_INFORMATION> GetMemoryInfos(Process proc)
        {
            var system_info = GetSystemInfo();
            var minAdd = system_info.lpMinimumApplicationAddress.ToInt64();
            var maxAdd = system_info.lpMaximumApplicationAddress.ToInt64();
            while (minAdd < maxAdd) {
                var info = new MEMORY_BASIC_INFORMATION();
                var ptr = new IntPtr(minAdd);
                VirtualQueryEx(proc.Handle, ptr, out info, Marshal.SizeOf(info));
                yield return info;
                minAdd = info.BaseAddress.ToInt64() + info.RegionSize.ToInt64();
            }
        }

        [DllImport("kernel32.dll")]
        private static extern void GetSystemInfo([MarshalAs(UnmanagedType.Struct)] ref SYSTEM_INFO lpSystemInfo);

        private static SYSTEM_INFO GetSystemInfo()
        {
            var system_info = new SYSTEM_INFO();
            GetSystemInfo(ref system_info);
            return system_info;
        }


        [DllImport("kernel32.dll")]
        private static extern int VirtualQuery(IntPtr lpAddress, ref MEMORY_BASIC_INFORMATION lpBuffer, IntPtr dwLength);
        [DllImport("kernel32.dll")]
        private static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, int dwLength);

        // Nested Types
        public enum MEMORY_ALLOCATION_PROTECT : uint
        {
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_GUARD = 0x100,
            PAGE_NOACCESS = 1,
            PAGE_NOCACHE = 0x200,
            PAGE_READONLY = 2,
            PAGE_READWRITE = 4,
            PAGE_WRITECOMBINE = 0x400,
            PAGE_WRITECOPY = 8
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public Memory.MEMORY_ALLOCATION_PROTECT AllocationProtect;
            public IntPtr RegionSize;
            public Memory.MEMORY_STATE State;
            public Memory.MEMORY_ALLOCATION_PROTECT Protect;
            public uint Type;
        }

        public enum MEMORY_STATE : uint
        {
            MEM_COMMIT = 0x1000,
            MEM_FREE = 0x10000,
            MEM_RESERVE = 0x2000
        }

        public enum MEMORY_TYPE : uint
        {
            MEM_IMAGE = 0x1000000,
            MEM_MAPPED = 0x40000,
            MEM_PRIVATE = 0x20000
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct PROCESSOR_INFO_UNION
        {
            // Fields
            [FieldOffset(0)]
            internal uint dwOemId;
            [FieldOffset(0)]
            internal ushort wProcessorArchitecture;
            [FieldOffset(2)]
            internal ushort wReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_INFO
        {
            internal Memory.PROCESSOR_INFO_UNION p;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public uint dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public uint wProcessorLevel;
            public uint wProcessorRevision;
        }
    }
}