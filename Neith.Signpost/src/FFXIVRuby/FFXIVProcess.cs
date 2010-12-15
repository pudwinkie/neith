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
        private Process proc;

        // Methods
        public FFXIVProcess(Process p)
        {
            this.proc = p;
            this.memory = new Memory();
        }

        public Memory.MEMORY_BASIC_INFORMATION[] GetMemoryBasicInfos()
        {
            return memory.GetMemoryInfos(proc);
        }

        public byte[] ReadBytes(int address, int size)
        {
            return FFXIVMemoryProvidor.ReadProcessMemory(this.proc.Handle, address, size);
        }

        public int ReadInt32(int address)
        {
            byte[] buffer = FFXIVMemoryProvidor.ReadProcessMemory(this.proc.Handle, address, 4);
            return (((buffer[0] + (buffer[1] * 0x100)) + (buffer[2] * 0x10000)) + (buffer[3] * 0x1000000));
        }
    }

 

}
