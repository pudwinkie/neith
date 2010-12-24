using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Neith.Logger.Model;
using FFXIVRuby;

namespace Neith.Logger.XIV
{
    public class XIVCollecter : ICollector
    {
        public string Name { get { return "XIV.XIVCollecter"; } }

        public string Application { get { return "FF14"; } }

        public string Domain { get; set; }

        public string User { get; set; }

        private Thread CollectThread { get; set; }

        private ManualResetEvent WO { get; set; }

        private XIVAnalyzer Analyzer { get; set; }

        public XIVCollecter()
        {
            Analyzer = new XIVAnalyzer();
            WO = new ManualResetEvent(false);
            CollectThread = new Thread(CollectTask);
            CollectThread.IsBackground = true;
            CollectThread.Start();
        }

        public void Dispose()
        {
            if (CollectThread == null) return;
            WO.Set();
            CollectThread.Join(1000);
            if (CollectThread.IsAlive) CollectThread.Abort();
            CollectThread = null;
            WO.Dispose();
            WO = null;
        }

        private void CollectTask()
        {
            var host = Environment.MachineName;
            var currentProcess = Process.GetCurrentProcess();
            foreach (var a in XIVProcessWatch.EnReadMemoryLog(WO)) {
                var log = Log.Create();
                log.Collector = Name;
                log.Host = host;
                if (a.FFXIV != null) log.Pid = a.FFXIV.Proc.Id;
                else log.Pid = currentProcess.Id;
                log.Application = Application;
                log.Domain = Domain;
                log.User = User;
                log["who"] = a.Who;
                log["message"] = a.Message;
                log["typeID"] = a.MessageTypeID.ToString("X5");
                OnCollect(Analyzer.SetAnalyzeData(log));
            }
        }

        public event LogEventHandler Collect;

        private void OnCollect(Log log)
        {
            if (Collect == null) return;
            Collect(this, new LogEventArgs(log));
        }

    }
}
