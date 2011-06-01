using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using Neith.Logger.Model;
using FFXIVRuby;

namespace Neith.Logger.XIV
{
  public  class DummyXIVCollecter : ICollector
    {
        public string Name { get { return "XIV.DummyXIVCollecter"; } }

        public string Application { get { return "FF14"; } }

        public string Domain { get; set; }

        public string User { get; set; }

        private IDisposable CollectTask;

        private XIVAnalyzer Analyzer { get; set; }

        public DummyXIVCollecter()
        {
            Analyzer = new XIVAnalyzer();
            CollectTask = GenCollectTask();
        }

        public void Dispose()
        {
            if (CollectTask == null) return;
            CollectTask.Dispose();
        }

        private IDisposable GenCollectTask()
        {
            var host = Environment.MachineName;
            var currentProcess = Process.GetCurrentProcess();
            var span = 0;
            var rand = new Random();

            Action<FFXILogMessageType, string, string> SendLog = (typeID, who, mes) =>
            {
                var log = NeithLog.Create();
                log.Collector = Name;
                log.Host = host;
                log.Pid = currentProcess.Id;
                log.HWnd = IntPtr.Zero;
                log.Application = Application;
                log.Domain = Domain;
                log.User = User;
                log.Actor = who;
                log.LogText = mes;
                log.Type = string.Format("FFXIV_LOG.{0:X5}", (int)typeID);
                OnCollect(Analyzer.SetAnalyzeData(log));
            };

            return Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.1), Scheduler.TaskPool)
                .Subscribe(a =>
                {
                    if ((a % 30) == 0) {
                        span = rand.Next(10) + 1;
                        SendLog(FFXILogMessageType.INTERNAL_TRACE, "Dummy Span", "送信間隔変更->{0}00ms"._(span));
                    }
                    if ((a % span) != 0) return;
                    SendLog(FFXILogMessageType.TALK_SAY, "Dummy Say", "({0:0000000000})ダミー会話ですまっちょ。ダミー会話ですまっちょ。"._(a));
                });
        }

        public event NeithLogEventHandler Collect;
        private void OnCollect(NeithLog log)
        {
            if (Collect == null) return;
            Collect(this, new NeithLogEventArgs(log));
        }

    }
}
