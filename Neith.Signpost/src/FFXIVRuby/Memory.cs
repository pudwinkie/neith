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
        private MEMORY_BASIC_INFORMATION mbi = new MEMORY_BASIC_INFORMATION();
        private SYSTEM_INFO system_info = new SYSTEM_INFO();

        // Methods
        public MEMORY_BASIC_INFORMATION[] GetMemoryInfos(Process proc)
        {
            GetSystemInfo(ref this.system_info);
            uint lpMinimumApplicationAddress = this.system_info.lpMinimumApplicationAddress;
            MEMORY_BASIC_INFORMATION lpBuffer = new MEMORY_BASIC_INFORMATION();
            List<MEMORY_BASIC_INFORMATION> list = new List<MEMORY_BASIC_INFORMATION>();
            while (lpMinimumApplicationAddress < this.system_info.lpMaximumApplicationAddress) {
                VirtualQueryEx(proc.Handle, lpMinimumApplicationAddress, out lpBuffer, Marshal.SizeOf(lpBuffer));
                list.Add(lpBuffer);
                lpMinimumApplicationAddress = ((uint)lpBuffer.BaseAddress) + ((uint)((int)lpBuffer.RegionSize));
            }
            return list.ToArray();
        }

        [DllImport("kernel32.dll")]
        private static extern void GetSystemInfo([MarshalAs(UnmanagedType.Struct)] ref SYSTEM_INFO lpSystemInfo);
        public void ShowMemory()
        {
            GetSystemInfo(ref this.system_info);
            Console.WriteLine("dwProcessorType: {0}", this.system_info.dwProcessorType.ToString());
            Console.WriteLine("dwPageSize: {0}", this.system_info.dwPageSize.ToString());
            if (VirtualQuery(ref this.system_info.dwPageSize, ref this.mbi, Marshal.SizeOf(this.mbi)) != 0) {
                Console.WriteLine("AllocationBase: {0}", this.mbi.AllocationBase);
                Console.WriteLine("BaseAddress: {0}", this.mbi.BaseAddress);
                Console.WriteLine("RegionSize: {0}", this.mbi.RegionSize);
            }
            else {
                Console.WriteLine("ERROR: VirtualQuery() failed.");
            }
        }

        public void ShowMemory(Process proc)
        {
            GetSystemInfo(ref this.system_info);
            uint lpMinimumApplicationAddress = this.system_info.lpMinimumApplicationAddress;
            MEMORY_BASIC_INFORMATION lpBuffer = new MEMORY_BASIC_INFORMATION();
            while (lpMinimumApplicationAddress < this.system_info.lpMaximumApplicationAddress) {
                VirtualQueryEx(proc.Handle, lpMinimumApplicationAddress, out lpBuffer, Marshal.SizeOf(lpBuffer));
                Console.WriteLine("{0} {1}", ((uint)lpBuffer.BaseAddress).ToString("X"), lpBuffer.RegionSize.ToString("X"));
                lpMinimumApplicationAddress = ((uint)lpBuffer.BaseAddress) + ((uint)((int)lpBuffer.RegionSize));
            }
        }

        [DllImport("kernel32.dll")]
        private static extern int VirtualQuery(ref uint lpAddress, ref MEMORY_BASIC_INFORMATION lpBuffer, int dwLength);
        [DllImport("kernel32.dll")]
        private static extern int VirtualQueryEx(IntPtr hProcess, uint lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, int dwLength);

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
            public UIntPtr BaseAddress;
            public UIntPtr AllocationBase;
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
            public uint lpMinimumApplicationAddress;
            public uint lpMaximumApplicationAddress;
            public uint dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public uint wProcessorLevel;
            public uint wProcessorRevision;
        }
    }
}