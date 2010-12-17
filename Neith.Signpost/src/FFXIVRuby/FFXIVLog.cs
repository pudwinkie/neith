using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using ProtoBuf;

namespace FFXIVRuby
{
    [ProtoContract]
    public class FFXIVLog
    {
        // Fields
        [ProtoMember(1)]
        private string _Message;

        [ProtoMember(2)]
        private FFXILogMessageType _MessageType;

        [ProtoMember(3)]
        private string _Who;

        // Properties
        public string Message { get { return _Message; } }

        public FFXILogMessageType MessageType { get { return _MessageType; } }

        public string Who { get { return _Who; } }


        // Nested Types
        public enum FFXILogMessageType
        {
            TALK_SAY = 0x0001,
            TALK_SHOUT = 0x0002,
            TALK_TELL = 0x0003,
            TALK_PARTY = 0x0004,
            TALK_LS1 = 0x0005,
            TALK_LS2 = 0x0006,
            TALK_LS3 = 0x0007,
            TALK_LS4 = 0x0008,
            TALK_LS5 = 0x0009,
            TALK_LS6 = 0x000A,
            TALK_LS7 = 0x000B,
            TALK_LS8 = 0x000C,
            TALK_TELL_SELF = 0x000D,
            TALK_LS1_CURRENT = 0x000E,
            TALK_LS2_CURRENT = 0x000F,
            TALK_LS3_CURRENT = 0x0010,
            TALK_LS4_CURRENT = 0x0011,
            TALK_LS5_CURRENT = 0x0012,
            TALK_LS6_CURRENT = 0x0013,
            TALK_LS7_CURRENT = 0x0014,
            TALK_LS8_CURRENT = 0x0015,
            TALK_EMOTE = 0x001B,
            SYSTEM_INFO = 0x001D,
            SYSTEM_MY_ACTION = 0x0020,
            SYSTEM_2 = 0x0021,
            TALK_NPC1 = 0x0023,
            TALK_NPC2 = 0x0026,
            TALK_NPC3 = 0x0028,
            MY_GAIN = 0x0042,
            OTHER_GAIN = 0x0043,
            MY_ENEMY_DOWNED = 0x0044,
            OTHER_DOWNED = 0x0045,
            MY_HIT = 0x0050,
            MY_DAMAGE = 0x0051,
            PARTY_HIT = 0x0052,
            PARTY_DAMAGE = 0x0053,
            OTHER_ACTION_ME = 0x0054,
            OTHER_ACTION = 0x0055,
            MY_MISS = 0x0056,
            PARTY_MISS = 0x0058,
            OTHER_HIT_PARTY_MISS = 0x0059,
            OTHER_ACTION_MISS = 0x005B,
            MY_ACTION = 0x005C,
            PAETY_HEALED = 0x005E,
            OTHER_ACTION_OTHER1 = 0x0061,
            MY_EFFECT_REMOVED = 0x0062,
            PARTY_EFFECT = 0x0064,
            ENEMY_EFFECT = 0x0066,
            OTHER_EFFECT_REMOVED = 0x0067,
            MY_ENFEEBLE_REMOVED = 0x0068,
            MY_ENFEEBLE = 0x0069,
            PARTY_ENFEEBLE_REMOVED2 = 0x006A,
            PARTY_ENFEEBLE0 = 0x006B,
            OTHER_ENFEEBLE2 = 0x006C,
            OTHER_ENFEEBLE1 = 0x006D,
        }


        // Methods
        public FFXIVLog(FFXILogMessageType logtype, string who, string message)
        {
            _MessageType = logtype;
            _Who = who;
            _Message = message;
        }

        public override string ToString()
        {
            string str = string.Format("{1}", Who, Message);
            if (this.Who != "") {
                str = string.Format("{0} : {1}", Who, Message);
            }
            switch (MessageType) {
                case FFXILogMessageType.TALK_TELL:
                    return string.Format("{0} >> {1}", Who, Message);

                case FFXILogMessageType.TALK_PARTY:
                    return string.Format("( {0} ) {1}", Who, Message);

                case FFXILogMessageType.TALK_LS1:
                    return string.Format("[1]<{0}> {1}", Who, Message);

                case FFXILogMessageType.TALK_LS2:
                    return string.Format("[2]<{0}> {1}", Who, Message);

                case FFXILogMessageType.TALK_LS3:
                    return string.Format("[3]<{0}> {1}", Who, Message);

                case FFXILogMessageType.TALK_LS4:
                    return string.Format("[4]<{0}> {1}", Who, Message);

                case FFXILogMessageType.TALK_LS5:
                    return string.Format("[5]<{0}> {1}", Who, Message);

                case FFXILogMessageType.TALK_LS6:
                    return string.Format("[6]<{0}> {1}", Who, Message);

                case FFXILogMessageType.TALK_LS7:
                    return string.Format("[7]<{0}> {1}", Who, Message);

                case FFXILogMessageType.TALK_LS8:
                    return string.Format("[8]<{0}> {1}", Who, Message);

                case FFXILogMessageType.TALK_TELL_SELF:
                    return string.Format(">> {0} : {1}", Who, Message);

                case FFXILogMessageType.TALK_LS1_CURRENT:
                    return string.Format("[1]<{0}> {1}", Who, Message);

                case FFXILogMessageType.TALK_LS2_CURRENT:
                    return string.Format("[2]<{0}> {1}", Who, Message);

                case FFXILogMessageType.TALK_LS3_CURRENT:
                    return string.Format("[3]<{0}> {1}", Who, Message);

                case FFXILogMessageType.TALK_LS4_CURRENT:
                    return string.Format("[4]<{0}> {1}", Who, Message);

                case FFXILogMessageType.TALK_LS5_CURRENT:
                    return string.Format("[5]<{0}> {1}", Who, Message);

                case FFXILogMessageType.TALK_LS6_CURRENT:
                    return string.Format("[6]<{0}> {1}", Who, Message);

                case FFXILogMessageType.TALK_LS7_CURRENT:
                    return string.Format("[7]<{0}> {1}", Who, Message);

                case FFXILogMessageType.TALK_LS8_CURRENT:
                    return string.Format("[8]<{0}> {1}", Who, Message);
            }
            return str;
        }
    }
}