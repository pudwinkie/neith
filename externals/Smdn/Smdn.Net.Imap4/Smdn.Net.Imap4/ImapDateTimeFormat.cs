// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2008-2011 smdn
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Globalization;

namespace Smdn.Net.Imap4 {
  public static class ImapDateTimeFormat {
    public static string ToDateString(DateTime date)
    {
      return date.ToString("d-MMM-yyyy", CultureInfo.InvariantCulture);
    }

    // date            = date-text / DQUOTE date-text DQUOTE
    // date-day        = 1*2DIGIT
    //                     ; Day of month
    // date-day-fixed  = (SP DIGIT) / 2DIGIT
    //                     ; Fixed-format version of date-day
    // date-month      = "Jan" / "Feb" / "Mar" / "Apr" / "May" / "Jun" /
    //                   "Jul" / "Aug" / "Sep" / "Oct" / "Nov" / "Dec"
    // date-text       = date-day "-" date-month "-" date-year
    // date-year       = 4DIGIT
    // date-time       = DQUOTE date-day-fixed "-" date-month "-" date-year
    //                   SP time SP zone DQUOTE
    // time            = 2DIGIT ":" 2DIGIT ":" 2DIGIT
    //                     ; Hours minutes seconds
    // zone            = ("+" / "-") 4DIGIT
    //                     ; Signed four-digit value of hhmm representing
    //                     ; hours and minutes east of Greenwich (that is,
    //                     ; the amount that the given time differs from
    //                     ; Universal Time).  Subtracting the timezone
    //                     ; from the given time will give the UT form.
    //                     ; The Universal Time zone is "+0000".
    public static string ToDateTimeString(DateTimeOffset dateTime)
    {
      return string.Concat("\"",
                           dateTime.ToString("dd-MMM-yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                           " ",
                           dateTime.ToString("zzz", CultureInfo.InvariantCulture).RemoveChars(':'),
                           "\"");
    }

    public static DateTimeOffset FromDateTimeString(string dateTime)
    {
      try {
        return DateTimeOffset.ParseExact(dateTime,
                                         dateTimeOffsetFormats,
                                         CultureInfo.InvariantCulture,
                                         DateTimeStyles.AssumeUniversal);
      }
      catch (FormatException) {
        throw new Smdn.Net.Imap4.Protocol.ImapFormatException(string.Format("invalid date-time format: '{0}'", dateTime));
      }
    }

#if false
    private static string[] dateTimeOffsetFormats = new[] {"d-MMM-yyyy HH:mm:ss zzz"};
#else
    private static string[] dateTimeOffsetFormats = InitializeFormat();

    private static string[] InitializeFormat()
    {
      // https://bugzilla.novell.com/show_bug.cgi?id=547675
      foreach (var formats in new[] {
        // for netfx
        new[] {
          "dd-MMM-yyyy HH:mm:ss zzz", "\u0020d-MMM-yyyy HH:mm:ss zzz",
        },
        // for mono
        new[] {
          "dd-MMM-yyyy HH:mm:ss zzz", "\u0020d-MMM-yyyy HH:mm:ss zzz",
          "dd-MMM-yyyy HH:mm:ss zz",  "\u0020d-MMM-yyyy HH:mm:ss zz",
        },
      }) {
        try {
          DateTimeOffset.ParseExact("17-Jul-1996 02:44:25 -0700",
                                    formats,
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.AssumeUniversal);
          return formats;
        }
        catch (FormatException) {
          continue;
        }
      }

      // XXX
      throw new NotSupportedException();
    }
#endif
  }
}