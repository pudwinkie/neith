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
    private static byte[] TABHead = new byte[] { 2, 0x2e };
    public static readonly string TabStartString = "{";
    private static Regex TabStringRegex = new Regex(@"\{02\w\w\w\w\w+03\}");
    public static readonly string TabTerminalString = "}";

    // Methods
    public static byte[] TabEscape(byte[] data)
    {
        if (data.Length == 0)
        {
            return new byte[0];
        }
        MemoryStream stream = new MemoryStream();
        for (int i = 0; i < (data.Length - 1); i++)
        {
            if ((data[i] == TABHead[0]) && (data[i + 1] == TABHead[1]))
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("{");
                builder.Append(TABHead[0].ToString("X").PadLeft(2, '0'));
                builder.Append(TABHead[1].ToString("X").PadLeft(2, '0'));
                int num2 = data[i + 2];
                builder.Append(num2.ToString("X").PadLeft(2, '0'));
                for (int j = 0; j < num2; j++)
                {
                    builder.Append(data[(i + 3) + j].ToString("X").PadLeft(2, '0'));
                }
                builder.Append("}");
                string s = builder.ToString();
                foreach (byte num4 in Encoding.ASCII.GetBytes(s))
                {
                    stream.WriteByte(num4);
                }
                i += num2 + 2;
            }
            else
            {
                stream.WriteByte(data[i]);
            }
        }
        stream.WriteByte(data[data.Length - 1]);
        return stream.ToArray();
    }

    public static byte[] TabReEscape(string text, Encoding enc)
    {
        MemoryStream stream = new MemoryStream();
        MatchCollection matchs = TabStringRegex.Matches(text);
        string[] strArray = TabStringRegex.Split(text);
        foreach (byte num in enc.GetBytes(strArray[0]))
        {
            stream.WriteByte(num);
        }
        for (int i = 0; i < matchs.Count; i++)
        {
            string str = matchs[i].Value.Replace(TabStartString, "").Replace(TabTerminalString, "");
            for (int j = 0; j < str.Length; j += 2)
            {
                byte num4 = byte.Parse(str.Substring(j, 2), NumberStyles.AllowHexSpecifier);
                stream.WriteByte(num4);
            }
            foreach (byte num5 in enc.GetBytes(strArray[i + 1]))
            {
                stream.WriteByte(num5);
            }
        }
        return stream.ToArray();
    }
}
}
