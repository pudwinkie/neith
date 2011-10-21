using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFXIVRuby
{
    public class LogStatusFoundEventArgs : EventArgs
    {
        public FFXIVLogReader LogStatus { get; private set; }

        public LogStatusFoundEventArgs(FFXIVLogReader logStatus)
        {
            LogStatus = logStatus;
        }
    }

    public delegate void LogStatusFoundEventHandler(object sender, LogStatusFoundEventArgs args);
}
