using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFXIVRuby
{
    public class DataDisguiser
    {
        private static byte[] converttable;

        private static byte[] CreateDisguiseTable()
        {
            byte[] buffer = new byte[0x100];
            for (int i = 0; i < 0x100; i++) {
                buffer[Disguise(i)] = (byte)i;
            }
            return buffer;
        }

        public static byte[] Disguise(byte[] data)
        {
            byte[] buffer = new byte[data.Length];
            for (int i = 0; i < data.Length; i++) {
                buffer[i] = Disguise(data[i]);
            }
            return buffer;
        }

        public static byte Disguise(int data)
        {
            int num = data / 0x10;
            int num2 = data % 0x10;
            int num3 = num2 / 4;
            int num4 = num2 % 4;
            return (byte)(0x73 - (((0x10 * num) - (4 * num3)) + num4));
        }

        public static byte ThrewOff(byte dat)
        {
            if (converttable == null) {
                converttable = CreateDisguiseTable();
            }
            return converttable[dat];
        }

        public static byte[] ThrewOff(byte[] data)
        {
            byte[] buffer = new byte[data.Length];
            for (int i = 0; i < data.Length; i++) {
                buffer[i] = ThrewOff(data[i]);
            }
            return buffer;
        }

    }
}
