using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFXIVRuby
{
    public class FFXIVLogStatus
    {
        // Fields
        public int Entry;
        private FFXIVProcess ffxiv;

        // Methods
        public FFXIVLogStatus(FFXIVProcess _ffxiv, int entry)
        {
            this.ffxiv = _ffxiv;
            this.Entry = entry;
        }

        public int GetEntryPoint()
        {
            return this.ffxiv.ReadInt32(this.Entry);
        }

        private byte[] GetLogData()
        {
            return this.ffxiv.ReadBytes(this.GetEntryPoint(), this.GetTerminalPoint() - this.GetEntryPoint());
        }

        private byte[] GetLogData(int from, int size)
        {
            return this.ffxiv.ReadBytes(this.GetEntryPoint(), this.GetTerminalPoint() - this.GetEntryPoint());
        }

        public FFXIVLog[] GetLogs()
        {
            return FFXIVLog.GetLogs(this.GetLogData(), Encoding.GetEncoding("utf-8"));
        }

        public FFXIVLog[] GetLogs(int from)
        {
            return FFXIVLog.GetLogs(this.GetLogData(from, this.GetTerminalPoint() - from), Encoding.GetEncoding("utf-8"));
        }

        public int GetTerminalPoint()
        {
            return this.ffxiv.ReadInt32(this.Entry + 4);
        }

        // Properties
        public int EntryPoint
        {
            get
            {
                return this.GetEntryPoint();
            }
        }

        public int TerminalPoint
        {
            get
            {
                return this.GetTerminalPoint();
            }
        }
    }


}
