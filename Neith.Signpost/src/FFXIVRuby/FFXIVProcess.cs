using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace FFXIVRuby
{
    public class FFXIVProcess
    {
        // Fields
        private Memory memory;
        public Process Proc { get; private set; }

        // Methods
        public FFXIVProcess(Process p)
        {
            Proc = p;
            memory = new Memory();
        }

        public IEnumerable<Memory.MEMORY_BASIC_INFORMATION> GetMemoryBasicInfos()
        {
            return memory.GetMemoryInfos(Proc);
        }

        public byte[] ReadBytes(int address, int size)
        {
            return FFXIVMemoryProvidor.ReadProcessMemory(this.Proc.Handle, (IntPtr)address, size);
        }
        public byte[] ReadBytesOrNull(int address, int size)
        {
            try { return ReadBytes(address, size); }
            catch { return null; }
        }

        public int ReadInt32(int address)
        {
            byte[] buffer = FFXIVMemoryProvidor.ReadProcessMemory(this.Proc.Handle, (IntPtr)address, 4);
            return (((buffer[0] + (buffer[1] * 0x100)) + (buffer[2] * 0x10000)) + (buffer[3] * 0x1000000));
        }
        public int? ReadInt32OrNull(int address)
        {
            try { return ReadInt32(address); }
            catch { return null; }
        }


    }
}