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

using Smdn.Net.Imap4.Protocol.Client;
using Smdn.Net.Imap4.Client.Transaction.BuiltIn;

namespace Smdn.Net.Imap4.Client.Session {
  partial class ImapSession {
    /*
     * transaction methods : authenticated state / LIST/LSUB etc.
     */

    /// <summary>sends LIST command</summary>
    /// <remarks>
    /// valid in authenticated state
    /// This method sends LIST command with an empty mailbox name and an empty reference name.
    /// </remarks>
    public ImapCommandResult ListRoot(out ImapMailboxList root)
    {
      return ListRoot(string.Empty, out root);
    }

    /// <summary>sends LIST command</summary>
    /// <remarks>
    /// valid in authenticated state
    /// This method sends LIST command with an empty mailbox name.
    /// </remarks>
    public ImapCommandResult ListRoot(string referenceName, out ImapMailboxList root)
    {
      RejectNonAuthenticatedState();

      var argReferenceName = ImapMailboxNameString.CreateMailboxName(referenceName);

      root = null;

      using (var t = new ListTransaction(connection)) {
        t.RequestArguments["reference name"] = argReferenceName;
        t.RequestArguments["mailbox name"] = ImapQuotedString.Empty;

        if (ProcessTransaction(t).Succeeded) {
          hierarchyDelimiters[referenceName] = t.Result.Value[0].HierarchyDelimiter;

          root = t.Result.Value[0];
        }

        return t.Result;
      }
    }

    /// <summary>sends LIST command</summary>
    /// <remarks>
    /// valid in authenticated state
    /// This method sends LIST command with an empty reference name and a wildcard "*" as mailbox name
    /// </remarks>
    public ImapCommandResult List(out ImapMailbox[] mailboxes)
    {
      return ListInternal(string.Empty, "*", out mailboxes);
    }

    /// <summary>sends LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult List(string mailboxName, out ImapMailbox[] mailboxes)
    {
      if (mailboxName == null)
        throw new ArgumentNullException("mailboxName");

      return ListInternal(string.Empty,
                          ImapMailboxNameString.CreateListMailboxName(mailboxName),
                          out mailboxes);
    }

    /// <summary>sends LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult List(ImapMailbox mailbox)
    {
      ValidateMailboxRelationship(mailbox);

      ImapMailbox[] discard;

      return ListInternal(string.Empty,
                          ImapMailboxNameString.CreateListMailboxName(mailbox.Name),
                          out discard);
    }

    /// <summary>sends LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult List(string referenceName, string mailboxName, out ImapMailbox[] mailboxes)
    {
      return ListInternal(referenceName,
                          ImapMailboxNameString.CreateListMailboxName(mailboxName),
                          out mailboxes);
    }

    private ImapCommandResult ListInternal(string referenceName,
                                           ImapString mailboxName,
                                           out ImapMailbox[] mailboxes)
    {
      if (referenceName == null)
        throw new ArgumentNullException("referenceName");

      using (var t = new ListTransaction(connection)) {
        return ListLsubInternal(t, referenceName, mailboxName, out mailboxes);
      }
    }

    /// <summary>sends LSUB command</summary>
    /// <remarks>
    /// valid in authenticated state
    /// This method sends LSUB command with an empty reference name and a wildcard "*" as mailbox name
    /// </remarks>
    public ImapCommandResult Lsub(out ImapMailbox[] mailboxes)
    {
      return LsubInternal(string.Empty, "*", out mailboxes);
    }

    /// <summary>sends LSUB command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Lsub(string mailboxName, out ImapMailbox[] mailboxes)
    {
      return LsubInternal(string.Empty,
                          ImapMailboxNameString.CreateListMailboxName(mailboxName),
                          out mailboxes);
    }

    /// <summary>sends LSUB command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Lsub(ImapMailbox mailbox)
    {
      ValidateMailboxRelationship(mailbox);

      ImapMailbox[] discard;

      return LsubInternal(string.Empty,
                          ImapMailboxNameString.CreateListMailboxName(mailbox.Name),
                          out discard);
    }

    /// <summary>sends LSUB command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Lsub(string referenceName, string mailboxName, out ImapMailbox[] mailboxes)
    {
      return LsubInternal(referenceName,
                          ImapMailboxNameString.CreateListMailboxName(mailboxName),
                          out mailboxes);
    }

    private ImapCommandResult LsubInternal(string referenceName,
                                           ImapString mailboxName,
                                           out ImapMailbox[] mailboxes)
    {
      if (referenceName == null)
        throw new ArgumentNullException("referenceName");

      using (var t = new LsubTransaction(connection)) {
        return ListLsubInternal(t, referenceName, mailboxName, out mailboxes);
      }
    }

    /// <summary>sends Gimap XLIST command</summary>
    /// <remarks>
    /// valid in authenticated state
    /// This method sends Gimap XLIST command with an empty reference name and a wildcard "*" as mailbox name
    /// </remarks>
    public ImapCommandResult XList(out ImapMailbox[] mailboxes)
    {
      return XListInternal(string.Empty, "*", out mailboxes);
    }

    /// <summary>sends Gimap LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult XList(string mailboxName, out ImapMailbox[] mailboxes)
    {
      if (mailboxName == null)
        throw new ArgumentNullException("mailboxName");

      return XListInternal(string.Empty,
                           ImapMailboxNameString.CreateListMailboxName(mailboxName),
                           out mailboxes);
    }

    /// <summary>sends Gimap LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult XList(ImapMailbox mailbox)
    {
      ValidateMailboxRelationship(mailbox);

      ImapMailbox[] discard;

      return XListInternal(string.Empty,
                           ImapMailboxNameString.CreateListMailboxName(mailbox.Name),
                           out discard);
    }

    /// <summary>sends Gimap LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult XList(string referenceName, string mailboxName, out ImapMailbox[] mailboxes)
    {
      return XListInternal(referenceName,
                           ImapMailboxNameString.CreateListMailboxName(mailboxName),
                           out mailboxes);
    }

    private ImapCommandResult XListInternal(string referenceName,
                                            ImapString mailboxName,
                                            out ImapMailbox[] mailboxes)
    {
      if (referenceName == null)
        throw new ArgumentNullException("referenceName");

      using (var t = new XListTransaction(connection)) {
        return ListLsubInternal(t, referenceName, mailboxName, out mailboxes);
      }
    }

    /// <summary>sends RLIST command</summary>
    /// <remarks>
    /// valid in authenticated state
    /// This method sends RLIST command with an empty reference name and a wildcard "*" as mailbox name
    /// </remarks>
    public ImapCommandResult RList(out ImapMailbox[] mailboxes)
    {
      return RListInternal(string.Empty, "*", out mailboxes);
    }

    /// <summary>sends RLIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult RList(string mailboxName, out ImapMailbox[] mailboxes)
    {
      return RListInternal(string.Empty,
                           ImapMailboxNameString.CreateListMailboxName(mailboxName),
                           out mailboxes);
    }

    /// <summary>sends RLIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult RList(string referenceName, string mailboxName, out ImapMailbox[] mailboxes)
    {
      return RListInternal(referenceName,
                           ImapMailboxNameString.CreateListMailboxName(mailboxName),
                           out mailboxes);
    }

    private ImapCommandResult RListInternal(string referenceName,
                                            ImapString mailboxName,
                                            out ImapMailbox[] mailboxes)
    {
      if (referenceName == null)
        throw new ArgumentNullException("referenceName");

      using (var t = new RListTransaction(connection)) {
        return ListLsubInternal(t, referenceName, mailboxName, out mailboxes);
      }
    }

    /// <summary>sends RLSUB command</summary>
    /// <remarks>
    /// valid in authenticated state
    /// This method sends LIST command with an empty reference name and a wildcard "*" as mailbox name
    /// </remarks>
    public ImapCommandResult RLsub(out ImapMailbox[] mailboxes)
    {
      return RLsubInternal(string.Empty, "*", out mailboxes);
    }

    /// <summary>sends RLSUB command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult RLsub(string mailboxName, out ImapMailbox[] mailboxes)
    {
      return RLsubInternal(string.Empty,
                           ImapMailboxNameString.CreateListMailboxName(mailboxName),
                           out mailboxes);
    }

    /// <summary>sends RLSUB command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult RLsub(string referenceName, string mailboxName, out ImapMailbox[] mailboxes)
    {
      return RLsubInternal(referenceName,
                           ImapMailboxNameString.CreateListMailboxName(mailboxName),
                           out mailboxes);
    }

    private ImapCommandResult RLsubInternal(string referenceName,
                                            ImapString mailboxName,
                                            out ImapMailbox[] mailboxes)
    {
      if (referenceName == null)
        throw new ArgumentNullException("referenceName");

      using (var t = new RLsubTransaction(connection)) {
        return ListLsubInternal(t, referenceName, mailboxName, out mailboxes);
      }
    }

#region "extended LIST with no reference name"
#region "extended LIST with multiple mailbox name pattern"
    /// <summary>sends extended LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult ListExtended(out ImapMailbox[] mailboxes,
                                          string mailboxNamePattern,
                                          params string[] mailboxNamePatterns)
    {
      return ListExtendedInternalNoRefName(mailboxNamePattern, mailboxNamePatterns, null, null, out mailboxes);
    }

    /// <summary>sends extended LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult ListExtended(ImapListSelectionOptions selectionOptions,
                                          out ImapMailbox[] mailboxes,
                                          string mailboxNamePattern,
                                          params string[] mailboxNamePatterns)
    {
      if (selectionOptions == null)
        throw new ArgumentNullException("selectionOptions");

      return ListExtendedInternalNoRefName(mailboxNamePattern, mailboxNamePatterns, selectionOptions, null, out mailboxes);
    }

      /// <summary>sends extended LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult ListExtended(ImapListReturnOptions returnOptions,
                                          out ImapMailbox[] mailboxes,
                                          string mailboxNamePattern,
                                          params string[] mailboxNamePatterns)
    {
      if (returnOptions == null)
        throw new ArgumentNullException("returnOptions");

      return ListExtendedInternalNoRefName(mailboxNamePattern, mailboxNamePatterns, null, returnOptions, out mailboxes);
    }

    /// <summary>sends extended LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult ListExtended(ImapListSelectionOptions selectionOptions,
                                          ImapListReturnOptions returnOptions,
                                          out ImapMailbox[] mailboxes,
                                          string mailboxNamePattern,
                                          params string[] mailboxNamePatterns)
    {
      if (selectionOptions == null)
        throw new ArgumentNullException("selectionOptions");
      if (returnOptions == null)
        throw new ArgumentNullException("returnOptions");

      return ListExtendedInternalNoRefName(mailboxNamePattern, mailboxNamePatterns, selectionOptions, returnOptions, out mailboxes);
    }
#endregion

#region "extended LIST with single mailbox name pattern"
    /// <summary>sends extended LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult ListExtended(string mailboxNamePattern,
                                          out ImapMailbox[] mailboxes)
    {
      return ListExtendedInternalNoRefName(mailboxNamePattern, null, null, null, out mailboxes);
    }

    /// <summary>sends extended LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult ListExtended(string mailboxNamePattern,
                                          ImapListSelectionOptions selectionOptions,
                                          out ImapMailbox[] mailboxes)
    {
      if (selectionOptions == null)
        throw new ArgumentNullException("selectionOptions");

      return ListExtendedInternalNoRefName(mailboxNamePattern, null, selectionOptions, null, out mailboxes);
    }

      /// <summary>sends extended LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult ListExtended(string mailboxNamePattern,
                                          ImapListReturnOptions returnOptions,
                                          out ImapMailbox[] mailboxes)
    {
      if (returnOptions == null)
        throw new ArgumentNullException("returnOptions");

      return ListExtendedInternalNoRefName(mailboxNamePattern, null, null, returnOptions, out mailboxes);
    }

    /// <summary>sends extended LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult ListExtended(string mailboxNamePattern,
                                          ImapListSelectionOptions selectionOptions,
                                          ImapListReturnOptions returnOptions,
                                          out ImapMailbox[] mailboxes)
    {
      if (selectionOptions == null)
        throw new ArgumentNullException("selectionOptions");
      if (returnOptions == null)
        throw new ArgumentNullException("returnOptions");

      return ListExtendedInternalNoRefName(mailboxNamePattern, null, selectionOptions, returnOptions, out mailboxes);
    }
#endregion

    private ImapCommandResult ListExtendedInternalNoRefName(string mailboxNamePattern,
                                                            string[] mailboxNamePatterns,
                                                            ImapListSelectionOptions selectionOptions,
                                                            ImapListReturnOptions returnOptions,
                                                            out ImapMailbox[] mailboxes)
    {
      if (mailboxNamePattern == null)
        throw new ArgumentNullException("mailboxNamePattern");

      if (mailboxNamePatterns == null || mailboxNamePatterns.Length == 0)
        return ListExtendedInternal(string.Empty, new[] {mailboxNamePattern}, selectionOptions, returnOptions, out mailboxes);
      else
        return ListExtendedInternal(string.Empty, mailboxNamePatterns.Prepend(mailboxNamePattern), selectionOptions, returnOptions, out mailboxes);
    }
#endregion

    /// <summary>sends extended LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult ListExtended(string referenceName,
                                          string[] mailboxNamePatterns,
                                          ImapListSelectionOptions selectionOptions,
                                          ImapListReturnOptions returnOptions,
                                          out ImapMailbox[] mailboxes)
    {
      if (referenceName == null)
        throw new ArgumentNullException("referenceName");
      if (mailboxNamePatterns == null)
        throw new ArgumentNullException("mailboxNamePatterns");
      if (mailboxNamePatterns.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyArray("mailboxNamePatterns");
      if (selectionOptions == null)
        throw new ArgumentNullException("selectionOptions");
      if (returnOptions == null)
        throw new ArgumentNullException("returnOptions");

      return ListExtendedInternal(referenceName, mailboxNamePatterns, selectionOptions, returnOptions, out mailboxes);
    }

    private ImapCommandResult ListExtendedInternal(string referenceName,
                                                   string[] mailboxNamePatterns,
                                                   ImapListSelectionOptions selectionOptions,
                                                   ImapListReturnOptions returnOptions,
                                                   out ImapMailbox[] mailboxes)
    {
      using (var t = new ListExtendedTransaction(connection)) {
        if (selectionOptions != null)
        t.RequestArguments["selection options"] = selectionOptions;

        if (returnOptions != null)
          t.RequestArguments["return options"] = returnOptions;

        var quotedMailboxNamePatterns = Array.ConvertAll(mailboxNamePatterns, delegate(string pattern) {
          return ImapMailboxNameString.CreateListMailboxName(pattern);
        });

        if (quotedMailboxNamePatterns.Length == 1)
          ListLsubInternal(t, referenceName, quotedMailboxNamePatterns[0], out mailboxes);
        else
          ListLsubInternal(t, referenceName, new ImapParenthesizedString(quotedMailboxNamePatterns), out mailboxes);

        /*
         * IMAP4 Extension for Returning STATUS Information in Extended LIST
         * http://tools.ietf.org/html/rfc5819
         */
        if (returnOptions != null && returnOptions.RequiredCapabilities.Contains(ImapCapability.ListStatus)) {
          // XXX: converting STATUS response
          foreach (var response in t.Result.ReceivedResponses) {
            var data = response as ImapDataResponse;

            if (data == null || data.Type != ImapDataResponseType.Status)
              continue;

            string mailboxName;
            var statusAttr = ImapDataResponseConverter.FromStatus(data, out mailboxName);

            var statusMailbox = Array.Find(mailboxes, delegate(ImapMailbox mailbox) {
              return mailbox.Name == mailboxName;
            });

            if (statusMailbox != null)
              statusMailbox.UpdateStatus(statusAttr);
          }
        }

        return t.Result;
      }
    }

    private ImapCommandResult ListLsubInternal(ListTransactionBase t,
                                               string referenceName,
                                               ImapString mailboxName,
                                               out ImapMailbox[] mailboxes)
    {
      RejectNonAuthenticatedState();

      mailboxes = null;

      t.RequestArguments["reference name"] = ImapMailboxNameString.CreateMailboxName(referenceName);
      t.RequestArguments["mailbox name"] = mailboxName;

      if (ProcessTransaction(t).Succeeded)
        mailboxes = Array.ConvertAll<ImapMailboxList, ImapMailbox>(t.Result.Value, mailboxManager.GetExistOrCreate);

      return t.Result;
    }
  }
}