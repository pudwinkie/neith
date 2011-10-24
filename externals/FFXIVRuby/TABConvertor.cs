using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace FFXIVRuby
{
    public class TABConvertor
    {
        // Fields
        private const byte TAB_HEADER = 0x02;

        public static readonly string TabStartString = "{";
        public static readonly string TabTerminalString = "}";

        private static readonly Regex TabStringRegex = new Regex(@"\{02\w\w\w\w\w+03\}", RegexOptions.Compiled);


        // Methods
        public static byte[] TabEscape(byte[] data)
        {
            if (data.Length == 0) {
                return new byte[0];
            }
            MemoryStream stream = new MemoryStream();
            for (int i = 0; i < (data.Length - 1); i++) {
                if (data[i] != TAB_HEADER) i += ConvertTagString(stream, data, i);
                else stream.WriteByte(data[i]);
            }
            stream.WriteByte(data[data.Length - 1]);
            return stream.ToArray();
        }

        private static int ConvertTagString(MemoryStream stream, byte[] data, int index)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{");
            builder.Append(ToHex(data[index]));
            builder.Append(ToHex(data[index + 1]));

            int length = data[index + 2];
            builder.Append(ToHex(data[index + 2]));
            for (int j = 0; j < length; j++) {
                builder.Append(ToHex(data[index + 3 + j]));
            }
            builder.Append("}");
            string s = builder.ToString();
            foreach (byte num4 in Encoding.ASCII.GetBytes(s)) {
                stream.WriteByte(num4);
            }
            return length + 2;
        }

        private static string ToHex(byte data) { return data.ToString("X").PadLeft(2, '0'); }


        public static byte[] TabReEscape(string text, Encoding enc)
        {
            MemoryStream stream = new MemoryStream();
            MatchCollection matchs = TabStringRegex.Matches(text);
            string[] strArray = TabStringRegex.Split(text);
            foreach (byte num in enc.GetBytes(strArray[0])) {
                stream.WriteByte(num);
            }
            for (int i = 0; i < matchs.Count; i++) {
                string str = matchs[i].Value.Replace(TabStartString, "").Replace(TabTerminalString, "");
                for (int j = 0; j < str.Length; j += 2) {
                    byte num4 = byte.Parse(str.Substring(j, 2), NumberStyles.AllowHexSpecifier);
                    stream.WriteByte(num4);
                }
                foreach (byte num5 in enc.GetBytes(strArray[i + 1])) {
                    stream.WriteByte(num5);
                }
            }
            return stream.ToArray();
        }
    }
}