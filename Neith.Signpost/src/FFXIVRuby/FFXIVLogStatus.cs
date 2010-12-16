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
            ffxiv = _ffxiv;
            Entry = entry;
        }

        public int GetEntryPoint()
        {
            return ffxiv.ReadInt32(this.Entry);
        }

        private byte[] GetLogData()
        {
            return ffxiv.ReadBytes(GetEntryPoint(), GetTerminalPoint() - GetEntryPoint());
        }

        private byte[] GetLogData(int from, int size)
        {
            return ffxiv.ReadBytes(GetEntryPoint(), GetTerminalPoint() - GetEntryPoint());
        }

        public IEnumerable<FFXIVLog> GetLogs()
        {
            return FFXIVLog.GetLogs(GetLogData(), Encoding.GetEncoding("utf-8"));
        }

        public IEnumerable<FFXIVLog> GetLogs(int from)
        {
            return FFXIVLog.GetLogs(GetLogData(from, GetTerminalPoint() - from), Encoding.GetEncoding("utf-8"));
        }

        public int GetTerminalPoint()
        {
            return this.ffxiv.ReadInt32(Entry + 4);
        }

        // Properties
        public int EntryPoint
        {
            get
            {
                return GetEntryPoint();
            }
        }

        public int TerminalPoint
        {
            get
            {
                return GetTerminalPoint();
            }
        }
    }
}
