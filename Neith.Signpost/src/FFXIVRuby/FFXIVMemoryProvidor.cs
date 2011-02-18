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

        public static byte[] ReadProcessMemory(IntPtr hProcess, IntPtr address, int size)
        {
            var buffer = new byte[size];
            int readSize = ReadProcessMemory(hProcess, address, buffer, size);
            return buffer;
        }

        public static int ReadProcessMemory(IntPtr hProcess, IntPtr address, byte[] buffer)
        {
            return ReadProcessMemory(hProcess, address, buffer, buffer.Length);
        }

        public static int ReadProcessMemory(IntPtr hProcess, IntPtr address, byte[] buffer, int size)
        {
            uint readSize;
            ReadProcessMemory(hProcess, address, buffer, (uint)size, out readSize);
            return (int)readSize;
        }

        #region API
        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, uint size, out uint lpNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, uint size, out IntPtr lpNumberOfBytesWritten);
        #endregion

    }


}
