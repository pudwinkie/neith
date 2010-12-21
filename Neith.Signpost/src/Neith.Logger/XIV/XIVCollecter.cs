using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Concurrency;
using System.Diagnostics;
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

        public void Dispose()
        {
        }

        /// <summary>
        /// コレクターを発行します。
        /// </summary>
        /// <returns></returns>
        public IObservable<Log> RxCollect()
        {
            var host = Environment.MachineName;
            var currentProcess = Process.GetCurrentProcess();

            var q1 = XIVProcessWatch.EnReadMemoryLog().Select(a =>
            {
                var log = Log.Create();
                log.Collector = Name;
                log.Host = host;
                if (a.FFXIV != null) log.Pid = a.FFXIV.Proc.Id;
                else log.Pid = currentProcess.Id;
                log.Application = Application;
                log.Domain = Domain;
                log.User = User;
                log.Message = a.ToString();
                var data = new MemoryStream();
                Serializer.Serialize(data, a);
                log.Data = data.ToArray();
                return SetAnalyzeData(log, a);
            });

            return q1.ToObservable(Scheduler.NewThread);
        }

        public Log SetAnalyzeData(Log log, FFXIVLog fflog)
        {
            log.Analyzer = Name;
            if (fflog.MessageType == FFXILogMessageType.UNNONE) {
                log.Category =
                    string.Format("{0}:0x{1:X4}", fflog.MessageType, fflog.MessageTypeID);
            }
            else {
                log.Category = fflog.MessageType.ToString();
            }
            log.Actor = fflog.Who;
            return log;
        }

    }
}
