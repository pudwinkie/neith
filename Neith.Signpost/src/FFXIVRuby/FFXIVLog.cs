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
        public FFXIVProcess FFXIV { get; private set; }

        [ProtoMember(1)]
        public int MessageTypeID { get; private set; }

        [ProtoMember(2)]
        public string Who { get; private set; }

        [ProtoMember(3)]
        public string Message { get; private set; }

        public FFXILogMessageType MessageType
        {
            get
            {
                try { return (FFXILogMessageType)MessageTypeID; }
                catch { }
                return FFXILogMessageType.UNNONE;
            }
        }

        public bool IsUnnone { get { return MessageType == FFXILogMessageType.UNNONE; } }





        // Methods
        public FFXIVLog(FFXIVProcess ffxiv, int messageId, string who, string message)
        {
            FFXIV = ffxiv;
            MessageTypeID = messageId;
            Who = who;
            Message = message;

            if (string.IsNullOrEmpty(Who)) Who = string.Empty;
            if (string.IsNullOrEmpty(Message)) Message = string.Empty;
        }

        public override string ToString()
        {
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

                case FFXILogMessageType.UNNONE:
                    return string.Format("[{0,4:X}]<{1}> {2}", MessageTypeID, Who, Message);
                default:

                    return string.Format("[{0}]<{1}> {2}", MessageType, Who, Message);
            }
        }
    }
}