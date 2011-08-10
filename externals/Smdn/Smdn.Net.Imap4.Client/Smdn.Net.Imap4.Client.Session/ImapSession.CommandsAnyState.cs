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
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Smdn.Collections;
using Smdn.Net.Imap4.Client.Transaction;
using Smdn.Net.Imap4.Client.Transaction.BuiltIn;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Session {
  partial class ImapSession {
    /*
     * transaction methods : any state
     */

    /// <summary>sends CAPABILITY command</summary>
    /// <remarks>valid in any state</remarks>
    public ImapCommandResult Capability()
    {
      ImapCapabilitySet discard;

      return Capability(out discard);
    }

    /// <summary>sends CAPABILITY command</summary>
    /// <remarks>valid in any state</remarks>
    public ImapCommandResult Capability(out ImapCapabilitySet capabilities)
    {
      RejectNonConnectedState();

      capabilities = null;

      using (var t = new CapabilityTransaction(connection)) {
        if (ProcessTransaction(t).Succeeded) {
          SetServerCapabilities(t.Result.Value);
          capabilities = t.Result.Value;
        }

        return t.Result;
      }
    }

    /// <summary>sends NOOP command</summary>
    /// <remarks>valid in any state</remarks>
    public ImapCommandResult NoOp()
    {
      RejectNonConnectedState();

      using (var t = new NoOpTransaction(connection)) {
        return ProcessTransaction(t);
      }
    }

    /// <summary>sends LOGOUT command</summary>
    /// <remarks>valid in any state</remarks>
    public ImapCommandResult Logout()
    {
      CheckDisposed();

      if (state == ImapSessionState.NotConnected)
        return new ImapCommandResult(ImapCommandResultCode.RequestDone,
                                     "already logged out or disconnected");

      using (var t = new LogoutTransaction(connection)) {
        if (ProcessTransaction(t).Code == ImapCommandResultCode.Ok) {
          TransitStateTo(ImapSessionState.NotAuthenticated);
          CloseConnection();
        }

        return t.Result;
      }
    }

    /// <summary>sends ID command</summary>
    /// <remarks>valid in any state</remarks>
    public ImapCommandResult ID(IDictionary<string, string> clientParameterList)
    {
      IDictionary<string, string> discard;

      return ID(clientParameterList, out discard);
    }

    /// <summary>sends ID command</summary>
    /// <remarks>valid in any state</remarks>
    public ImapCommandResult ID(IDictionary<string, string> clientParameterList, out IDictionary<string, string> serverParameterList)
    {
      RejectNonConnectedState();

      serverParameterList = null;

      using (var t = new IdTransaction(connection)) {
        // client parameter list
        if (clientParameterList != null && 0 < clientParameterList.Count) {
          // RFC 2971 IMAP4 ID extension
          // http://tools.ietf.org/html/rfc2971
          // 3.3. Defined Field Values
          //    Field strings MUST NOT be longer than 30 octets.  Value strings MUST
          //    NOT be longer than 1024 octets.  Implementations MUST NOT send more
          //    than 30 field-value pairs.
          if (30 < clientParameterList.Count)
            throw new ArgumentException("number of client parameter list pairs must be less than or equals to 30",
                                        "clientParameterList");

          var idParamsList = new List<ImapString>(clientParameterList.Count * 2);

          try {
            foreach (var param in clientParameterList) {
              if (30 < NetworkTransferEncoding.Transfer7Bit.GetByteCount(param.Key))
                throw new ArgumentException("field string of client parameter list must be shorter than or equals to 30 octets",
                                            "clientParameterList");

              idParamsList.Add(new ImapQuotedString(param.Key));

              if (param.Value == null) {
                idParamsList.Add(ImapNilString.Nil);
              }
              else {
                if (1024 < NetworkTransferEncoding.Transfer7Bit.GetByteCount(param.Value))
                  throw new ArgumentException("value string of client parameter list must be shorter than or equals to 1024 octets",
                                              "clientParameterList");
                idParamsList.Add(new ImapQuotedString(param.Value));
              }
            }
          }
          catch (EncoderFallbackException) {
            throw new ArgumentException("both field and value string must not contain non-ASCII chars",
                                        "clientParameterList");
          }

          t.RequestArguments["client parameter list"] = new ImapParenthesizedString(idParamsList.ToArray());
        }

        if (ProcessTransaction(t).Succeeded) {
          this.serverID = t.Result.Value;

          serverParameterList = t.Result.Value;
        }

        return t.Result;
      }
    }

    /// <summary>sends COMPRESS command</summary>
    /// <remarks>valid in any state</remarks>
    public ImapCommandResult Compress(ImapCompressionMechanism compressionMechanism)
    {
      RejectNonConnectedState();

      if (compressionMechanism == null)
        throw new ArgumentNullException("compressionMechanism");

      using (var t = new CompressTransaction(connection)) {
        t.RequestArguments["compression mechanism"] = compressionMechanism;

        return ProcessTransaction(t);
      }
    }

    /// <summary>sends LANGUAGE command</summary>
    /// <remarks>valid in any state</remarks>
    public ImapCommandResult Language(out string[] supportedLanguageTags)
    {
      RejectNonConnectedState();

      supportedLanguageTags = null;

      /*
       * RFC 5255 - Internet Message Access Protocol Internationalization
       * http://tools.ietf.org/html/rfc5255
       * 3.2. LANGUAGE Command
       *    If there aren't any arguments, the server SHOULD send an untagged
       *    LANGUAGE response listing the languages it supports.  If the server
       *    is unable to enumerate the list of languages it supports it MAY
       *    return a tagged NO response to the enumeration request.
       */
      using (var t = new LanguageTransaction(connection)) {
        if (ProcessTransaction(t).Succeeded)
          supportedLanguageTags = t.Result.Value.Item1;

        return t.Result;
      }
    }

    /// <summary>sends LANGUAGE command</summary>
    /// <remarks>valid in any state</remarks>
    public ImapCommandResult Language(CultureInfo cultureInfo, params CultureInfo[] cultureInfos)
    {
      string discard;

      return Language(out discard, cultureInfo, cultureInfos);
    }

    /// <summary>sends LANGUAGE command</summary>
    /// <remarks>valid in any state</remarks>
    public ImapCommandResult Language(out string selectedLanguageTag, CultureInfo cultureInfo, params CultureInfo[] cultureInfos)
    {
      if (cultureInfo == null)
        throw new ArgumentNullException("cultureInfo");

      return Language(out selectedLanguageTag, cultureInfo.IetfLanguageTag, Array.ConvertAll(cultureInfos, delegate(CultureInfo info) {
        return info.IetfLanguageTag;
      }));
    }

    /// <summary>sends LANGUAGE command</summary>
    /// <remarks>
    /// valid in any state.
    /// this method sends LANGUAGE command with language of "default".
    /// </remarks>
    public ImapCommandResult Language()
    {
      string discard;

      return Language(out discard);
    }

    /// <summary>sends LANGUAGE command</summary>
    /// <remarks>
    /// valid in any state.
    /// this method sends LANGUAGE command with language of "default".
    /// </remarks>
    public ImapCommandResult Language(out string selectedLanguageTag)
    {
      /*
       * RFC 5255 - Internet Message Access Protocol Internationalization
       * http://tools.ietf.org/html/rfc5255
       * 3.2. LANGUAGE Command
       *    The special "default" language range argument indicates a request to
       *    use a language designated as preferred by the server administrator.
       *    The preferred language MAY vary based on the currently active user.
       */
      return Language(out selectedLanguageTag, "default");
    }

    /// <summary>sends LANGUAGE command</summary>
    /// <remarks>valid in any state</remarks>
    public ImapCommandResult Language(string languageRange, params string[] languageRanges)
    {
      string discard;

      return Language(out discard, languageRange, languageRanges);
    }

    /// <summary>sends LANGUAGE command</summary>
    /// <remarks>valid in any state</remarks>
    public ImapCommandResult Language(out string selectedLanguageTag, string languageRange, params string[] languageRanges)
    {
      RejectNonConnectedState();

      if (languageRange == null)
        throw new ArgumentNullException("languageRange");
      if (languageRange.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("languageRange");

      var languageRangeArguments = new List<string>();

      languageRangeArguments.Add(languageRange);

      if (languageRanges != null) {
        for (var i = 0; i < languageRanges.Length; i++) {
          if (languageRanges[i] == null)
            throw new ArgumentNullException(string.Format("languageRanges[{0}]", i));
          if (languageRanges[i].Length == 0)
            throw ExceptionUtils.CreateArgumentMustBeNonEmptyString(string.Format("languageRanges[{0}]", i));

          languageRangeArguments.Add(languageRanges[i]);
        }
      }

      selectedLanguageTag = null;

      using (var t = new LanguageTransaction(connection)) {
        t.RequestArguments["language range arguments"] = new ImapStringList(languageRangeArguments.ToArray());

        if (ProcessTransaction(t).Succeeded) {
          this.selectedLanguage = t.Result.Value.Item1[0];

          if (t.Result.Value.Item2 != null)
            this.namespaces = t.Result.Value.Item2;

          selectedLanguageTag = t.Result.Value.Item1[0];
        }

        return t.Result;
      }
    }
  }
}
