using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using ProtoBuf;
using Neith.Logger.Model;
using FFXIVRuby;

namespace Neith.Logger.XIV
{
    public class XIVCollecter : ICollector
    {
        public string Name { get { return "XIVCollecter"; } }

        public string Application { get { return "FF14"; } }

        public string Domain { get; set; }

        public string User { get; set; }

        private Thread CollectThread { get; set; }

        private ManualResetEventSlim WO { get; set; }

        public XIVCollecter()
        {
            WO = new ManualResetEventSlim();
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
                var data = new MemoryStream();
                Serializer.Serialize(data, a);
                log.LogData = data.ToArray();
                log.LogObject = a;
                OnCollect(SetAnalyzeData(log, a));
            }
        }

        /// <summary>
        /// ログ情報を解析して追加データを登録
        /// </summary>
        /// <param name="log"></param>
        /// <param name="src"></param>
        /// <returns></returns>
        public Log SetAnalyzeData(Log log, FFXIVLog src)
        {
            if (src == null) {
                var st = new MemoryStream(log.LogData);
                src = Serializer.Deserialize<FFXIVLog>(st);
            }
            log.Analyzer = Name;
            if (src.MessageType == FFXILogMessageType.UNNONE) {
                log.Category =
                    string.Format("{0}:0x{1:X4}", src.MessageType, src.MessageTypeID);
            }
            else {
                log.Category = src.MessageType.ToString();
            }
            log.Actor = src.Who;
            log.Message = src.ToString();
            return log;
        }

        public event LogEventHandler Collect;

        private void OnCollect(Log log)
        {
            if (Collect == null) return;
            Collect(this, new LogEventArgs(log));
        }

    }
}
