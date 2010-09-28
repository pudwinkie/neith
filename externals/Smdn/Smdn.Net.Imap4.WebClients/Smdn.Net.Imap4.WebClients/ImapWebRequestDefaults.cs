// 
// Author:
//       smdn <smdn@mail.invisiblefulmoon.net>
// 
// Copyright (c) 2008-2010 smdn
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
using System.Collections.Generic;

using Smdn.Net.Imap4.Protocol;

namespace Smdn.Net.Imap4.WebClients {
  public static class ImapWebRequestDefaults {
    public static bool KeepAlive {
      get; set;
    }

    private static int timeout;

    public static int Timeout {
      get { return timeout; }
      set
      {
        if (value < -1)
          throw new ArgumentOutOfRangeException("Timeout", value, "must be greater than or equals to -1");
        timeout = value;
      }
    }

    private static int readWriteTimeout;

    public static int ReadWriteTimeout {
      get { return readWriteTimeout; }
      set
      {
        if (value < -1)
          throw new ArgumentOutOfRangeException("ReadWriteTimeout", value, "must be greater than or equals to -1");
        readWriteTimeout = value;
      }
    }

    public static bool ReadOnly {
      get; set;
    }

    public static bool UseTlsIfAvailable {
      get; set;
    }

    public static bool Subscription {
      get; set;
    }

    public static bool AllowCreateMailbox {
      get; set;
    }

    private static int fetchBlockSize;

    public static int FetchBlockSize {
      get { return fetchBlockSize; }
      set
      {
        if (value <= 0)
          throw new ArgumentOutOfRangeException("FetchBlockSize", value, "must be non-zero positive number");
        fetchBlockSize = value;
      }
    }

    public static bool FetchPeek {
      get; set;
    }

    public static ImapFetchDataItemMacro FetchDataItem {
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

    private static ImapResponseCode[] expectedErrorResponseCodes;

    public static ImapResponseCode[] ExpectedErrorResponseCodes {
      get { return expectedErrorResponseCodes; }
      set { expectedErrorResponseCodes = value ?? new ImapResponseCode[0]; }
    }

    private static Dictionary<string, string> clientID = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public static Dictionary<string, string> ClientID {
      get { return clientID; }
    }

    static ImapWebRequestDefaults()
    {
      var section = System.Configuration.ConfigurationManager.GetSection("smdn.net.imap4.client/webRequestDefaults") as Hashtable;

      KeepAlive           = GetValue<bool>(section, "keepAlive", true, Convert.ToBoolean);
      Timeout             = GetValue<int>(section, "timeout", System.Threading.Timeout.Infinite, Convert.ToInt32);
      ReadWriteTimeout    = GetValue<int>(section, "readWriteTimeout", 300 * 1000, Convert.ToInt32); // 5 mins; same as default value of FtpWebRequest.ReadWriteTimeout
      ReadOnly            = GetValue<bool>(section, "readOnly", false, Convert.ToBoolean);
      UseTlsIfAvailable   = GetValue<bool>(section, "useTlsIfAvailable", true, Convert.ToBoolean);
      Subscription        = GetValue<bool>(section, "subscription", true, Convert.ToBoolean);
      AllowCreateMailbox  = GetValue<bool>(section, "allowCreateMailbox", true, Convert.ToBoolean);
      FetchBlockSize      = GetValue<int>(section, "fetchBlockSize", 10 * 1024, Convert.ToInt32);
      FetchPeek           = GetValue<bool>(section, "fetchPeek", true, Convert.ToBoolean);
      FetchDataItem       = GetValue<ImapFetchDataItemMacro>(section, "fetchDataItem", ImapFetchDataItemMacro.All, delegate(object val) {
        return EnumUtils.ParseIgnoreCase<ImapFetchDataItemMacro>((string)val);
      });
      UsingSaslMechanisms = GetValue<string[]>(section, "usingSaslMechanisms", new[] {"DIGEST-MD5", "CRAM-MD5", "NTLM"}, TrimmedSplit);
      AllowInsecureLogin  = GetValue<bool>(section, "allowInsecureLogin", false, Convert.ToBoolean);
      ExpectedErrorResponseCodes = GetValue<ImapResponseCode[]>(section, "expectedErrorResponseCodes", new ImapResponseCode[0], delegate(object val) {
        return Array.ConvertAll<string, ImapResponseCode>(TrimmedSplit(val),
                                                          ImapResponseCode.GetKnownOrCreate);
      });

      clientID.Clear();

      if (section != null && section.ContainsKey("clientID")) {
        foreach (var pair in TrimmedSplit(section["clientID"])) {
          var keyValue = pair.Split('=');

          clientID.Add(keyValue[0], keyValue[1]);
        }
      }
      else {
        /*
         * 3.3. Defined Field Values
         *    Any string may be sent as a field, but the following are defined to
         *    describe certain values that might be sent.
         * 
         *      name            Name of the program
         *      version         Version number of the program
         *      os              Name of the operating system
         *      os-version      Version of the operating system
         *      vendor          Vendor of the client/server
         *      support-url     URL to contact for support
         *      address         Postal address of contact/vendor
         *      date            Date program was released, specified as a date-time
         *                        in IMAP4rev1
         *      command         Command used to start the program
         *      arguments       Arguments supplied on the command line, if any
         *                        if any
         *      environment     Description of environment, i.e., UNIX environment
         *                        variables or Windows registry settings
         */
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();

        clientID.Add("name", assembly.GetName().Name);
        clientID.Add("version", assembly.GetName().Version.ToString());
        clientID.Add("environment", string.Format("{0} {1}", Runtime.Name, Environment.Version));
      }
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
