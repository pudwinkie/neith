// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2010-2011 smdn
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
using System.Collections;
using System.Configuration;

using Smdn.Net.Pop3.Protocol;

namespace Smdn.Net.Pop3.WebClients {
  public static class PopWebRequestDefaults {
    public static bool KeepAlive {
      get; set;
    }

    private static int timeout;

    public static int Timeout {
      get { return timeout; }
      set
      {
        if (value < -1)
          throw ExceptionUtils.CreateArgumentMustBeGreaterThanOrEqualTo(-1, "Timeout", value);
        timeout = value;
      }
    }

    private static int readWriteTimeout;

    public static int ReadWriteTimeout {
      get { return readWriteTimeout; }
      set
      {
        if (value < -1)
          throw ExceptionUtils.CreateArgumentMustBeGreaterThanOrEqualTo(-1, "ReadWriteTimeout", value);
        readWriteTimeout = value;
      }
    }

    public static bool UseTlsIfAvailable {
      get; set;
    }

    public static bool DeleteAfterRetrieve {
      get; set;
    }

    private static string[] usingSaslMechanisms;

    public static string[] UsingSaslMechanisms {
      get { return usingSaslMechanisms; }
      set { usingSaslMechanisms = value ?? new string[0]; }
    }

    public static bool AllowInsecureLogin {
      get; set;
    }

    private static PopResponseCode[] expectedErrorResponseCodes;

    public static PopResponseCode[] ExpectedErrorResponseCodes {
      get { return expectedErrorResponseCodes; }
      set { expectedErrorResponseCodes = value ?? new PopResponseCode[0]; }
    }

    static PopWebRequestDefaults()
    {
      var section = System.Configuration.ConfigurationManager.GetSection("smdn.net.pop3.client/webRequestDefaults") as Hashtable;

      KeepAlive           = GetValue<bool>(section, "keepAlive", true, Convert.ToBoolean);
      Timeout             = GetValue<int>(section, "timeout", System.Threading.Timeout.Infinite, Convert.ToInt32);
      ReadWriteTimeout    = GetValue<int>(section, "readWriteTimeout", 300 * 1000, Convert.ToInt32); // 5 mins; same as default value of FtpWebRequest.ReadWriteTimeout
      UseTlsIfAvailable   = GetValue<bool>(section, "useTlsIfAvailable", true, Convert.ToBoolean);
      DeleteAfterRetrieve = GetValue<bool>(section, "deleteAfterRetrieve", false, Convert.ToBoolean);
      UsingSaslMechanisms = GetValue<string[]>(section, "usingSaslMechanisms", new[] {"DIGEST-MD5", "CRAM-MD5", "NTLM"}, TrimmedSplit);
      AllowInsecureLogin  = GetValue<bool>(section, "allowInsecureLogin", false, Convert.ToBoolean);
      ExpectedErrorResponseCodes = GetValue<PopResponseCode[]>(section, "expectedErrorResponseCodes", new PopResponseCode[0], delegate(object val) {
        return Array.ConvertAll<string, PopResponseCode>(TrimmedSplit(val),
                                                         PopResponseCode.GetKnownOrCreate);
      });
    }

    private static string[] TrimmedSplit(object val)
    {
      return Array.ConvertAll(((string)val).Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries), delegate(string splitted) {
        return splitted.Trim();
      });
    }

    private static TValue GetValue<TValue>(Hashtable section, string key, TValue defaultValue, Converter<object, TValue> convert)
    {
      if (section == null || !section.ContainsKey(key))
        return defaultValue;
      else
        return convert(section[key]);
    }
  }
}
