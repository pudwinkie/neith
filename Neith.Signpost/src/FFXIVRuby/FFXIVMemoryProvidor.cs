using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FFXIVRuby
{
    public static class FFXIVMemoryProvidor
    {
        public static Process GetFFXIVGameProcess()
        {
            return Process.GetProcessesByName("ffxivgame").FirstOrDefault();
        }

        public static int ReadMemoryInt32(IntPtr handle, IntPtr addr)
        {
            IntPtr zero = IntPtr.Zero;
            int num = 0;
            zero = ReadProcessMemorySafe(handle, addr, 4);
            try {
                num = Marshal.ReadInt32(zero);
            }
            finally {
                Marshal.FreeHGlobal(zero);
            }
            return num;
        }

        public static byte[] ReadProcessMemory(IntPtr hProcess, int lpBaseAddress, int size)
        {
            IntPtr ptr;
            byte[] buffer = new byte[size];
            ReadProcessMemory(hProcess, (IntPtr)lpBaseAddress, buffer, (uint)size, out ptr);
            return buffer;
        }


        public static IntPtr ReadProcessMemorySafe(IntPtr Handle, IntPtr Address, uint nBytesToRead)
        {
            IntPtr outputBuffer = Marshal.AllocHGlobal((int)nBytesToRead);
            UIntPtr zero = UIntPtr.Zero;
            UIntPtr nBufferSize = (UIntPtr)nBytesToRead;
            if (!ReadProcessMemorySafe(Handle, Address, outputBuffer, nBufferSize, out zero)) {
                return IntPtr.Zero;
            }
            return outputBuffer;
        }

        #region API
        [DllImport("kernel32.dll")]
        private static extern int ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, uint size, out IntPtr lpNumberOfBytesRead);
        [DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory")]
        private static extern bool ReadProcessMemorySafe(IntPtr handle, IntPtr addr, IntPtr OutputBuffer, UIntPtr nBufferSize, out UIntPtr lpNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        private static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, uint size, out IntPtr lpNumberOfBytesWritten);
        #endregion

    }


}
