using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFXIVRuby
{
    /// <summary>
    /// メッセージ種別
    /// </summary>
    public enum FFXILogMessageType
    {
        UNNONE   = 0x01FFFF,
        INTERNAL_START = 0x010001,
        INTERNAL_FOUND14 = 0x010002,
        INTERNAL_LOST14 = 0x010003,
        INTERNAL_WAIT = 0x010004,

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
        TALK_NPC1 = 0x0023,
        TALK_NPC2 = 0x0026,
        TALK_NPC3 = 0x0028,
        SYSTEM_INFO = 0x001D,
        SYSTEM_MY_ACTION = 0x0020,
        SYSTEM_2 = 0x0021,

        MY_GAIN = 0x0042,
        MY_ENEMY_DOWNED = 0x0044,
        MY_HIT = 0x0050,
        MY_DAMAGE = 0x0051,
        MY_MISS = 0x0056,
        MY_ACTION = 0x005C,
        MY_EFFECT_REMOVED = 0x0062,
        MY_ENFEEBLE_REMOVED = 0x0068,
        MY_ENFEEBLE = 0x0069,

        PARTY_HIT = 0x0052,
        PARTY_DAMAGE = 0x0053,
        PARTY_MISS = 0x0058,
        PAETY_HEALED = 0x005E,
        PARTY_EFFECT = 0x0064,
        PARTY_ENFEEBLE_REMOVED2 = 0x006A,
        PARTY_ENFEEBLE0 = 0x006B,

        ENEMY_EFFECT = 0x0066,

        OTHER_GAIN = 0x0043,
        OTHER_DOWNED = 0x0045,
        OTHER_ACTION_ME = 0x0054,
        OTHER_ACTION = 0x0055,
        OTHER_HIT_PARTY_MISS = 0x0059,
        OTHER_ACTION_MISS = 0x005B,
        OTHER_ACTION_OTHER1 = 0x0061,
        OTHER_EFFECT_REMOVED = 0x0067,
        OTHER_ENFEEBLE2 = 0x006C,
        OTHER_ENFEEBLE1 = 0x006D,
    }
}
