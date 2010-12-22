using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Logger.Model
{
    public class LogEventArgs : EventArgs
    {
        public Log Log { get; private set; }

        public LogEventArgs(Log log)
        {
            Log = log;
        }
    }

    public delegate void LogEventHandler(object sender, LogEventArgs args);

}
