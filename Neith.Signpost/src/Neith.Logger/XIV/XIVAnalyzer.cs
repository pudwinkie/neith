using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
        public NeithLog SetAnalyzeData(NeithLog log)
        {
            log.Analyzer = Name;
            var who = log.Actor;
            var message = log.LogText;
            var numType = log.Type.Substring(10).HexToInt32();
            var mType = numType.ToMessageType();

            if (mType == FFXILogMessageType.UNNONE) {
                log.Category =
                    string.Format("{0}:{1}", mType, numType);
            }
            else {
                log.Category = mType.ToString();
            }
            log.Message = FFXIVLog.GetLogString(mType, numType, who, message);
            return log;
        }
    }
}
