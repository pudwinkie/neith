Imports System.Runtime.CompilerServices

Public Module Helpers
    <Extension()>
    Public Function GetPubDate(item As XElement) As DateTime
        Dim s = item.<pubDate>.Value
        s = s.Replace("EST", "-0500")
        s = s.Replace("EDT", "-0400")
        s = s.Replace("CST", "-0600")
        s = s.Replace("CDT", "-0500")
        s = s.Replace("MST", "-0700")
        s = s.Replace("MDT", "-0600")
        s = s.Replace("PST", "-0800")
        s = s.Replace("PDT", "-0700")
        Dim d As DateTime
        If DateTime.TryParse(s, d) Then Return d
        Return DateTime.MinValue
    End Function
End Module
