using System;
using System.Xml.Linq;

public static class Helpers
{
    public static DateTime GetPubDate(this XElement item)
    {
        var s = (string)item.Element("pubDate");
        s = s.Replace("EST", "-0500");
        s = s.Replace("EDT", "-0400");
        s = s.Replace("CST", "-0600");
        s = s.Replace("CDT", "-0500");
        s = s.Replace("MST", "-0700");
        s = s.Replace("MDT", "-0600");
        s = s.Replace("PST", "-0800");
        s = s.Replace("PDT", "-0700");
        DateTime d;
        if (DateTime.TryParse(s, out d)) return d;
        return DateTime.MinValue;
    }
}

