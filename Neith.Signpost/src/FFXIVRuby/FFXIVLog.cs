using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace FFXIVRuby
{
    public class FFXIVLog
    {
        // Fields
        private string _Message;
        private FFXILogMessageType _MessageType;
        private string _Who;
        public static readonly string Extension = "LOG";
        private static Regex regex = new Regex("[0-9A-F]{4}:");

        // Methods
        private FFXIVLog(FFXILogMessageType logtype, string who, string message)
        {
            this._MessageType = logtype;
            this._Who = who;
            this._Message = message;
        }

        public static FFXIVLog[] GetLogs(byte[] LogData, Encoding enc)
        {
            List<FFXIVLog> list = new List<FFXIVLog>();
            MemoryStream stream = new MemoryStream();
            bool flag = false;
            for (int i = 0; i < LogData.Length; i++) {
                if (flag) {
                    stream.WriteByte(LogData[i]);
                }
                else if (LogData[i] == 0x30) {
                    flag = true;
                    stream.WriteByte(LogData[i]);
                }
            }
            string input = enc.GetString(TABConvertor.TabEscape(stream.ToArray()));
            MatchCollection matchs = regex.Matches(input);
            string[] strArray = regex.Split(input);
            for (int j = 1; j < strArray.Length; j++) {
                string[] strArray2 = strArray[j].Split(new char[] { ':' }, 2, StringSplitOptions.None);
                FFXIVLog item = new FFXIVLog((FFXILogMessageType)int.Parse(matchs[j - 1].Value.TrimEnd(new char[] { ':' }), NumberStyles.AllowHexSpecifier), strArray2[0].Replace("\0", ""), strArray2[1].Replace("\0", ""));
                list.Add(item);
            }
            return list.ToArray();
        }

        public override string ToString()
        {
            string str = string.Format("{1}", this.Who, this.Message);
            if (this.Who != "") {
                str = string.Format("{0} : {1}", this.Who, this.Message);
            }
            switch (this.MessageType) {
                case FFXILogMessageType.TELL:
                    return string.Format("{0} >> {1}", this.Who, this.Message);

                case FFXILogMessageType.PARTY:
                    return string.Format("( {0} ) {1}", this.Who, this.Message);

                case FFXILogMessageType.LINKSHELL1:
                    return string.Format("[1]<{0}> {1}", this.Who, this.Message);

                case FFXILogMessageType.LINKSHELL2:
                    return string.Format("[2]<{0}> {1}", this.Who, this.Message);

                case FFXILogMessageType.LINKSHELL3:
                    return string.Format("[3]<{0}> {1}", this.Who, this.Message);

                case FFXILogMessageType.LINKSHELL4:
                    return string.Format("[4]<{0}> {1}", this.Who, this.Message);

                case FFXILogMessageType.LINKSHELL5:
                    return string.Format("[5]<{0}> {1}", this.Who, this.Message);

                case FFXILogMessageType.LINKSHELL6:
                    return string.Format("[6]<{0}> {1}", this.Who, this.Message);

                case FFXILogMessageType.LINKSHELL7:
                    return string.Format("[7]<{0}> {1}", this.Who, this.Message);

                case FFXILogMessageType.LINKSHELL8:
                    return string.Format("[8]<{0}> {1}", this.Who, this.Message);

                case FFXILogMessageType.MY_TELL:
                    return string.Format(">> {0} : {1}", this.Who, this.Message);

                case FFXILogMessageType.CURRENT_LINKSHELL1:
                    return string.Format("[1]<{0}> {1}", this.Who, this.Message);

                case FFXILogMessageType.CURRENT_LINKSHELL2:
                    return string.Format("[2]<{0}> {1}", this.Who, this.Message);

                case FFXILogMessageType.CURRENT_LINKSHELL3:
                    return string.Format("[3]<{0}> {1}", this.Who, this.Message);

                case FFXILogMessageType.CURRENT_LINKSHELL4:
                    return string.Format("[4]<{0}> {1}", this.Who, this.Message);

                case FFXILogMessageType.CURRENT_LINKSHELL5:
                    return string.Format("[5]<{0}> {1}", this.Who, this.Message);

                case FFXILogMessageType.CURRENT_LINKSHELL6:
                    return string.Format("[6]<{0}> {1}", this.Who, this.Message);

                case FFXILogMessageType.CURRENT_LINKSHELL7:
                    return string.Format("[7]<{0}> {1}", this.Who, this.Message);

                case FFXILogMessageType.CURRENT_LINKSHELL8:
                    return string.Format("[8]<{0}> {1}", this.Who, this.Message);
            }
            return str;
        }

        // Properties
        public string Message
        {
            get
            {
                return this._Message;
            }
        }

        public FFXILogMessageType MessageType
        {
            get
            {
                return this._MessageType;
            }
        }

        public string Who
        {
            get
            {
                return this._Who;
            }
        }

        // Nested Types
        public enum FFXILogMessageType
        {
            CURRENT_LINKSHELL1 = 14,
            CURRENT_LINKSHELL2 = 15,
            CURRENT_LINKSHELL3 = 0x10,
            CURRENT_LINKSHELL4 = 0x11,
            CURRENT_LINKSHELL5 = 0x12,
            CURRENT_LINKSHELL6 = 0x13,
            CURRENT_LINKSHELL7 = 20,
            CURRENT_LINKSHELL8 = 0x15,
            EMOTE = 0x1b,
            ENEMY_EFFECT = 0x66,
            LINKSHELL1 = 5,
            LINKSHELL2 = 6,
            LINKSHELL3 = 7,
            LINKSHELL4 = 8,
            LINKSHELL5 = 9,
            LINKSHELL6 = 10,
            LINKSHELL7 = 11,
            LINKSHELL8 = 12,
            MY_ACTION = 0x5c,
            MY_DAMAGE = 0x51,
            MY_EFFECT_REMOVED = 0x62,
            MY_ENEMY_DOWNED = 0x44,
            MY_ENFEEBLE = 0x69,
            MY_ENFEEBLE_REMOVED = 0x68,
            MY_GAIN = 0x42,
            MY_HIT = 80,
            MY_MISS = 0x56,
            MY_TELL = 13,
            NPC_SAY1 = 0x23,
            NPC_SAY2 = 0x26,
            NPC_SAY3 = 40,
            OTHER_ACTION = 0x55,
            OTHER_ACTION_ME = 0x54,
            OTHER_ACTION_MISS = 0x5b,
            OTHER_ACTION_OTHER1 = 0x61,
            OTHER_DOWNED = 0x45,
            OTHER_EFFECT_REMOVED = 0x67,
            OTHER_ENFEEBLE1 = 0x6d,
            OTHER_ENFEEBLE2 = 0x6c,
            OTHER_GAIN = 0x43,
            OTHER_HIT_PARTY_MISS = 0x59,
            PAETY_HEALED = 0x5e,
            PARTY = 4,
            PARTY_DAMAGE = 0x53,
            PARTY_EFFECT = 100,
            PARTY_ENFEEBLE_REMOVED2 = 0x6a,
            PARTY_ENFEEBLE0 = 0x6b,
            PARTY_HIT = 0x52,
            PARTY_MISS = 0x58,
            SAY = 1,
            SHOUT = 2,
            SYSTEM_0 = 0x1d,
            SYSTEM_1 = 0x20,
            SYSTEM_2 = 0x21,
            TELL = 3
        }
    }

}