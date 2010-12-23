using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ProtoBuf;
using Neith.Logger.Model;
using FFXIVRuby;

namespace Neith.Logger.XIV
{
    public class XIVAnalyzer : IAnalyzer
    {
        public string Name { get { return "XIV.XIVAnalyzer"; } }

        public void Dispose()
        {
        }

        /// <summary>
        /// ログ情報を解析して追加データを登録します。
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public Log SetAnalyzeData(Log log)
        {
            var src = log.LogObject as FFXIVLog;
            if (src == null) {
                var ms = new MemoryStream(log.LogData);
                src = Serializer.Deserialize<FFXIVLog>(ms);
                log.LogObject = src;
            }
            return SetAnalyzeData(log, src);
        }

        /// <summary>
        /// ログ情報を解析して追加データを登録します。
        /// </summary>
        /// <param name="log"></param>
        /// <param name="src"></param>
        /// <returns></returns>
        public Log SetAnalyzeData(Log log, FFXIVLog src)
        {
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

    }
}
