using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Neith.Logger.Model
{
    [ProtoContract]
    public class LogStorePosition
    {
        private static readonly DateTime OFFSET_DATE = new DateTime(2014, 1, 1);
        private static readonly int OFFSET = (int)(OFFSET_DATE.Ticks / TimeSpan.TicksPerDay);

        [ProtoMember(1)]
        private int position;

        [ProtoMember(2)]
        private int offsetDateCount;

        public int Positon { get { return position; } }

        private int DateCount { get { return offsetDateCount + OFFSET; } }
        private long DateTicks { get { return DateCount * TimeSpan.TicksPerDay; } }
        public DateTime Date { get { return new DateTime(DateTicks); } }

        public LogStorePosition(DateTime date, long pos)
        {
            offsetDateCount = (int)(date.Date.Ticks / TimeSpan.TicksPerDay) - OFFSET;
            position = (int)pos;
        }
    }
}
