using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Logger.Model
{
    public class NeithLogEventArgs : EventArgs
    {
        public NeithLog Log { get; private set; }

        public NeithLogEventArgs(NeithLog log)
        {
            Log = log;
        }

        public override string ToString()
        {
            return Log.ToString();
        }
    }

    public delegate void NeithLogEventHandler(object sender, NeithLogEventArgs args);

}
