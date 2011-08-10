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
using System.Threading;

using Smdn.Net.Imap4.Client.Transaction;
using Smdn.Net.Imap4.Client.Transaction.BuiltIn;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Session {
  partial class ImapSession {
    /*
     * transaction methods : authenticated/selected state
     */

    /// <summary>sends SETQUOTA command</summary>
    /// <remarks>valid in authenticated/selected state (undocumented)</remarks>
    public ImapCommandResult SetQuota(string quotaRoot, string resourceName, long resourceLimit)
    {
      ImapQuota discard;

      return SetQuota(quotaRoot, new[] {new ImapQuotaResource(resourceName, resourceLimit)}, out discard);
    }

    /// <summary>sends SETQUOTA command</summary>
    /// <remarks>valid in authenticated/selected state (undocumented)</remarks>
    public ImapCommandResult SetQuota(string quotaRoot, string resourceName, long resourceLimit, out ImapQuota changedQuota)
    {
      return SetQuota(quotaRoot, new[] {new ImapQuotaResource(resourceName, resourceLimit)}, out changedQuota);
    }

    /// <summary>sends SETQUOTA command</summary>
    /// <remarks>valid in authenticated/selected state (undocumented)</remarks>
    public ImapCommandResult SetQuota(string quotaRoot, ImapQuotaResource resourceLimit)
    {
      ImapQuota discard;

      return SetQuota(quotaRoot, new[] {resourceLimit}, out discard);
    }

    /// <summary>sends SETQUOTA command</summary>
    /// <remarks>valid in authenticated/selected state (undocumented)</remarks>
    public ImapCommandResult SetQuota(string quotaRoot, ImapQuotaResource resourceLimit, out ImapQuota changedQuota)
    {
      return SetQuota(quotaRoot, new[] {resourceLimit}, out changedQuota);
    }

    /// <summary>sends SETQUOTA command</summary>
    /// <remarks>valid in authenticated/selected state (undocumented)</remarks>
    public ImapCommandResult SetQuota(string quotaRoot, ImapQuotaResource[] resourceLimits)
    {
      ImapQuota discard;

      return SetQuota(quotaRoot, resourceLimits, out discard);
    }

    /// <summary>sends SETQUOTA command</summary>
    /// <remarks>valid in authenticated/selected state (undocumented)</remarks>
    public ImapCommandResult SetQuota(string quotaRoot, ImapQuotaResource[] resourceLimits, out ImapQuota changedQuota)
    {
      if (quotaRoot == null)
        throw new ArgumentNullException("quotaRoot");
      if (resourceLimits == null)
        throw new ArgumentNullException("resourceLimits");

      var listOfResourceLimits = new List<ImapString>(resourceLimits.Length * 2);

      foreach (var resourceLimit in resourceLimits) {
        if (resourceLimit == null)
          throw new ArgumentException("contains null", "resourceLimits");

        listOfResourceLimits.Add(resourceLimit.Name);
        listOfResourceLimits.Add(resourceLimit.Limit.ToString());
      }

      RejectNonAuthenticatedState();

      changedQuota = null;

      using (var t = new SetQuotaTransaction(connection)) {
        // quota root
        t.RequestArguments["quota root"] = new ImapQuotedString(quotaRoot);
        // list of resource limits
        t.RequestArguments["list of resource limits"] = new ImapParenthesizedString(listOfResourceLimits.ToArray());

        if (ProcessTransaction(t).Succeeded)
          changedQuota = t.Result.Value;

        return t.Result;
      }
    }

    /// <summary>sends GETQUOTA command</summary>
    /// <remarks>valid in authenticated/selected state (undocumented)</remarks>
    public ImapCommandResult GetQuota(string quotaRoot, out ImapQuota quota)
    {
      if (quotaRoot == null)
        throw new ArgumentNullException("quotaRoot");

      RejectNonAuthenticatedState();

      quota = null;

      using (var t = new GetQuotaTransaction(connection)) {
        // quota root
        t.RequestArguments["quota root"] = new ImapQuotedString(quotaRoot);

        if (ProcessTransaction(t).Succeeded)
          quota = t.Result.Value;

        return t.Result;
      }
    }

    /// <summary>sends GETQUOTAROOT command</summary>
    /// <remarks>valid in authenticated/selected state (undocumented)</remarks>
    public ImapCommandResult GetQuotaRoot(ImapMailbox mailbox, out IDictionary<string, ImapQuota[]> quotaRoots)
    {
      ValidateMailboxRelationship(mailbox);

      return GetQuotaRoot(mailbox.Name, out quotaRoots);
    }

    public ImapCommandResult GetQuotaRoot(string mailboxName, out IDictionary<string, ImapQuota[]> quotaRoots)
    {
      RejectNonAuthenticatedState();

      var argMailboxName = ImapMailboxNameString.CreateMailboxNameNonEmpty(mailboxName);

      quotaRoots = null;

      using (var t = new GetQuotaRootTransaction(connection)) {
        // mailbox name
        t.RequestArguments["mailbox name"] = argMailboxName;

        if (ProcessTransaction(t).Succeeded)
          quotaRoots = t.Result.Value;

        return t.Result;
      }
    }

    public ImapCommandResult Idle(TimeSpan timeout)
    {
      return IdleInternal((int)timeout.TotalMilliseconds, null, null);
    }

    public ImapCommandResult Idle(int millisecondsTimeout)
    {
      return IdleInternal(millisecondsTimeout, null, null);
    }

    public ImapCommandResult Idle(object state, ImapKeepIdleCallback keepIdleCallback)
    {
      return Idle(Timeout.Infinite, state, keepIdleCallback);
    }

    public ImapCommandResult Idle(TimeSpan idleTimeout, object state, ImapKeepIdleCallback keepIdleCallback)
    {
      return Idle((int)idleTimeout.TotalMilliseconds, state, keepIdleCallback);
    }

    public ImapCommandResult Idle(int idleMillisecondsTimeout, object state, ImapKeepIdleCallback keepIdleCallback)
    {
      if (keepIdleCallback == null)
        throw new ArgumentNullException("keepIdleCallback");

      return IdleInternal(idleMillisecondsTimeout, state, keepIdleCallback);
    }

    private ImapCommandResult IdleInternal(int idleMillisecondsTimeout, object keepIdleState, ImapKeepIdleCallback keepIdleCallback)
    {
      if (idleMillisecondsTimeout < -1)
        throw ExceptionUtils.CreateArgumentMustBeGreaterThanOrEqualTo(-1, "idleMillisecondsTimeout", idleMillisecondsTimeout);

      var idleAsyncResult = BeginIdle(keepIdleState, keepIdleCallback);

      idleAsyncResult.AsyncWaitHandle.WaitOne(idleMillisecondsTimeout, false);

      return EndIdle(idleAsyncResult);
    }

    /// <summary>sends IDLE command</summary>
    /// <remarks>valid in authenticated/selected state</remarks>
    public IAsyncResult BeginIdle()
    {
      return BeginIdle(null, null);
    }

    /// <summary>sends IDLE command</summary>
    /// <remarks>valid in authenticated/selected state</remarks>
    public IAsyncResult BeginIdle(object keepIdleState, ImapKeepIdleCallback keepIdleCallback)
    {
      IdleTransaction t = null;

      try {
        RejectNonAuthenticatedState();
        RejectIdling();

        t = new IdleTransaction(connection, keepIdleState, keepIdleCallback);

        var asyncResult = BeginProcessTransaction(t, handlesIncapableAsException);

        // wait for IDLE started or respond tagged error response
        t.WaitForIdleStateChanged();

        return asyncResult;
      }
      catch {
        if (t != null) {
          t.Dispose();
          t = null;
        }

        throw;
      }
    }

    /// <summary>sends DONE for IDLE command continuation request responses</summary>
    /// <remarks>valid in idling</remarks>
    public ImapCommandResult EndIdle(IAsyncResult asyncResult)
    {
      if (asyncResult == null)
        throw new ArgumentNullException("asyncResult");

      var idleAsyncResult = asyncResult as TransactionAsyncResult;

      if (idleAsyncResult == null)
        throw ExceptionUtils.CreateArgumentMustBeValidIAsyncResult("asyncResult");

      using (var t = idleAsyncResult.Transaction as IdleTransaction) {
        // send 'DONE' if idling
        t.Done();

        return EndProcessTransaction(idleAsyncResult);
      }
    }

    /// <summary>sends NAMESPACE command</summary>
    /// <remarks>valid in authenticated/selected state</remarks>
    public ImapCommandResult Namespace()
    {
      ImapNamespace discard;

      return Namespace(out discard);
    }

    /// <summary>sends NAMESPACE command</summary>
    /// <remarks>valid in authenticated/selected state</remarks>
    public ImapCommandResult Namespace(out ImapNamespace namespaces)
    {
      RejectNonAuthenticatedState();

      namespaces = null;

      using (var t = new NamespaceTransaction(connection)) {
        if (ProcessTransaction(t).Succeeded) {
          this.namespaces = t.Result.Value;
          namespaces = t.Result.Value;
        }

        return t.Result;
      }
    }

    /// <summary>sends COMPARATOR command</summary>
    /// <remarks>valid in authenticated/selected state</remarks>
    public ImapCommandResult Comparator()
    {
      ImapCollationAlgorithm discard;

      return Comparator(out discard);
    }

    /// <summary>sends COMPARATOR command</summary>
    /// <remarks>valid in authenticated/selected state</remarks>
    public ImapCommandResult Comparator(out ImapCollationAlgorithm activeComparator)
    {
      /*
       * RFC 5255 - Internet Message Access Protocol Internationalization
       * http://tools.ietf.org/html/rfc5255
       * 4.7. COMPARATOR Command
       *    When issued with no arguments, it results in a
       *    COMPARATOR response indicating the currently active comparator.
       */
      activeComparator = null;

      using (var t = new ComparatorTransaction(connection)) {
        if (ProcessTransaction(t).Succeeded) {
          this.activeComparator = t.Result.Value.Item1;

          activeComparator = t.Result.Value.Item1;
        }

        return t.Result;
      }
    }

    /// <summary>sends COMPARATOR command</summary>
    /// <remarks>valid in authenticated/selected state</remarks>
    public ImapCommandResult Comparator(ImapCollationAlgorithm comparator, params ImapCollationAlgorithm[] comparators)
    {
      ImapCollationAlgorithm discard1;
      ImapCollationAlgorithm[] discard2;

      return Comparator(out discard1, out discard2, comparator, comparators);
    }

    /// <summary>sends COMPARATOR command</summary>
    /// <remarks>valid in authenticated/selected state</remarks>
    public ImapCommandResult Comparator(out ImapCollationAlgorithm[] matchingComparators, ImapCollationAlgorithm comparator, params ImapCollationAlgorithm[] comparators)
    {
      ImapCollationAlgorithm discard;

      return Comparator(out discard, out matchingComparators, comparator, comparators);
    }

    /// <summary>sends COMPARATOR command</summary>
    /// <remarks>valid in authenticated/selected state</remarks>
    public ImapCommandResult Comparator(out ImapCollationAlgorithm activeComparator, out ImapCollationAlgorithm[] matchingComparators, ImapCollationAlgorithm comparator, params ImapCollationAlgorithm[] comparators)
    {
      RejectNonAuthenticatedState();

      if (comparator == null)
        throw new ArgumentNullException("comparator");

      var comparatorArguments = new List<ImapCollationAlgorithm>();

      comparatorArguments.Add(comparator);

      if (comparators != null) {
        for (var i = 0; i < comparators.Length; i++) {
          if (comparators[i] == null)
            throw new ArgumentNullException(string.Format("comparators[{0}]", i));

          comparatorArguments.Add(comparators[i]);
        }
      }

      activeComparator = null;
      matchingComparators = null;

      /*
       * RFC 5255 - Internet Message Access Protocol Internationalization
       * http://tools.ietf.org/html/rfc5255
       * 4.7. COMPARATOR Command
       *    When issued with one or more comparator arguments, it changes the
       *    active comparator as directed.
       */
      using (var t = new ComparatorTransaction(connection)) {
        t.RequestArguments["comparator order arguments"] = new ImapStringList(comparatorArguments.ConvertAll(delegate(ImapCollationAlgorithm comp) {
          return new ImapQuotedString(comp.ToString());
        }).ToArray());

        if (ProcessTransaction(t).Succeeded) {
          this.activeComparator = t.Result.Value.Item1;

          activeComparator = t.Result.Value.Item1;
          matchingComparators = t.Result.Value.Item2 ?? new ImapCollationAlgorithm[0];
        }

        return t.Result;
      }
    }

#region "GETMETADATA"
#region "server entries"
    /// <summary>sends GETMETADATA command</summary>
    /// <remarks>
    /// valid in authenticated/selected state.
    /// this method will fail if server does not support METADATA or METADATA-SERVER extension.
    /// </remarks>
    public ImapCommandResult GetMetadata(string entrySpecifier, out ImapMetadata[] metadata)
    {
      return GetMetadataInternal(new[] {entrySpecifier}, null, out metadata);
    }

    /// <summary>sends GETMETADATA command</summary>
    /// <remarks>
    /// valid in authenticated/selected state.
    /// this method will fail if server does not support METADATA or METADATA-SERVER extension.
    /// </remarks>
    public ImapCommandResult GetMetadata(string[] entrySpecifiers, out ImapMetadata[] metadata)
    {
      return GetMetadataInternal(entrySpecifiers, null, out metadata);
    }

    /// <summary>sends GETMETADATA command</summary>
    /// <remarks>
    /// valid in authenticated/selected state.
    /// this method will fail if server does not support METADATA or METADATA-SERVER extension.
    /// </remarks>
    public ImapCommandResult GetMetadata(string entrySpecifier, ImapGetMetadataOptions options, out ImapMetadata[] metadata)
    {
      if (options == null)
        throw new ArgumentNullException("options");

      return GetMetadataInternal(new[] {entrySpecifier}, options, out metadata);
    }

    /// <summary>sends GETMETADATA command</summary>
    /// <remarks>
    /// valid in authenticated/selected state.
    /// this method will fail if server does not support METADATA or METADATA-SERVER extension.
    /// </remarks>
    public ImapCommandResult GetMetadata(string[] entrySpecifiers, ImapGetMetadataOptions options, out ImapMetadata[] metadata)
    {
      if (options == null)
        throw new ArgumentNullException("options");

      return GetMetadataInternal(entrySpecifiers, options, out metadata);
    }

    private ImapCommandResult GetMetadataInternal(string[] entrySpecifiers, ImapGetMetadataOptions options, out ImapMetadata[] metadata)
    {
      return GetMetadataInternal(null,
                                 entrySpecifiers,
                                 options,
                                 out metadata);
    }
#endregion

#region "mailbox entries"
    /// <summary>sends GETMETADATA command</summary>
    /// <remarks>
    /// valid in authenticated/selected state.
    /// this method will fail if server does not support METADATA extension.
    /// </remarks>
    public ImapCommandResult GetMetadata(ImapMailbox mailbox, string entrySpecifier, out ImapMetadata[] metadata)
    {
      ValidateMailboxRelationship(mailbox);

      return GetMetadataInternal(ImapMailboxNameString.CreateMailboxNameNonEmpty(mailbox.Name),
                                 new[] {entrySpecifier},
                                 null,
                                 out metadata);
    }

    /// <remarks>
    /// valid in authenticated/selected state.
    /// this method will fail if server does not support METADATA extension.
    /// </remarks>
    public ImapCommandResult GetMetadata(ImapMailbox mailbox, string[] entrySpecifiers, out ImapMetadata[] metadata)
    {
      ValidateMailboxRelationship(mailbox);

      return GetMetadataInternal(ImapMailboxNameString.CreateMailboxNameNonEmpty(mailbox.Name),
                                 entrySpecifiers,
                                 null,
                                 out metadata);
    }

    /// <summary>sends GETMETADATA command</summary>
    /// <remarks>
    /// valid in authenticated/selected state.
    /// this method will fail if server does not support METADATA extension.
    /// </remarks>
    public ImapCommandResult GetMetadata(string mailboxName, string entrySpecifier, out ImapMetadata[] metadata)
    {
      return GetMetadataInternal(ImapMailboxNameString.CreateMailboxNameNonEmpty(mailboxName),
                                 new[] {entrySpecifier},
                                 null,
                                 out metadata);
    }

    /// <summary>sends GETMETADATA command</summary>
    /// <remarks>
    /// valid in authenticated/selected state.
    /// this method will fail if server does not support METADATA extension.
    /// </remarks>
    public ImapCommandResult GetMetadata(string mailboxName, string[] entrySpecifiers, out ImapMetadata[] metadata)
    {
      return GetMetadataInternal(ImapMailboxNameString.CreateMailboxNameNonEmpty(mailboxName),
                                 entrySpecifiers,
                                 null,
                                 out metadata);
    }

    /// <summary>sends GETMETADATA command</summary>
    /// <remarks>
    /// valid in authenticated/selected state.
    /// this method will fail if server does not support METADATA extension.
    /// </remarks>
    public ImapCommandResult GetMetadata(ImapMailbox mailbox, string entrySpecifier, ImapGetMetadataOptions options, out ImapMetadata[] metadata)
    {
      if (options == null)
        throw new ArgumentNullException("options");

      ValidateMailboxRelationship(mailbox);

      return GetMetadataInternal(ImapMailboxNameString.CreateMailboxNameNonEmpty(mailbox.Name),
                                 new[] {entrySpecifier},
                                 options,
                                 out metadata);
    }

    /// <summary>sends GETMETADATA command</summary>
    /// <remarks>
    /// valid in authenticated/selected state.
    /// this method will fail if server does not support METADATA extension.
    /// </remarks>
    public ImapCommandResult GetMetadata(ImapMailbox mailbox, string[] entrySpecifiers, ImapGetMetadataOptions options, out ImapMetadata[] metadata)
    {
      if (options == null)
        throw new ArgumentNullException("options");

      ValidateMailboxRelationship(mailbox);

      return GetMetadataInternal(ImapMailboxNameString.CreateMailboxNameNonEmpty(mailbox.Name),
                                 entrySpecifiers,
                                 options,
                                 out metadata);
    }

    /// <summary>sends GETMETADATA command</summary>
    /// <remarks>
    /// valid in authenticated/selected state.
    /// this method will fail if server does not support METADATA extension.
    /// </remarks>
    public ImapCommandResult GetMetadata(string mailboxName, string entrySpecifier, ImapGetMetadataOptions options, out ImapMetadata[] metadata)
    {
      if (options == null)
        throw new ArgumentNullException("options");

      return GetMetadataInternal(ImapMailboxNameString.CreateMailboxNameNonEmpty(mailboxName),
                                 new[] {entrySpecifier},
                                 options,
                                 out metadata);
    }

    /// <summary>sends GETMETADATA command</summary>
    /// <remarks>
    /// valid in authenticated/selected state.
    /// this method will fail if server does not support METADATA extension.
    /// </remarks>
    public ImapCommandResult GetMetadata(string mailboxName, string[] entrySpecifiers, ImapGetMetadataOptions options, out ImapMetadata[] metadata)
    {
      if (options == null)
        throw new ArgumentNullException("options");

      return GetMetadataInternal(ImapMailboxNameString.CreateMailboxNameNonEmpty(mailboxName),
                                 entrySpecifiers,
                                 options,
                                 out metadata);
    }
#endregion

    private ImapCommandResult GetMetadataInternal(ImapString mailboxName,
                                                  string[] entrySpecifiers,
                                                  ImapGetMetadataOptions options,
                                                  out ImapMetadata[] metadata)
    {
      if (entrySpecifiers == null)
        throw new ArgumentNullException("entrySpecifiers");
      if (entrySpecifiers.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyArray("entrySpecifiers");

      if (handlesIncapableAsException)
        CheckServerCapabilityMetadata(mailboxName != null);

      metadata = null;

      using (var t = new GetMetadataTransaction(connection)) {
        // mailbox-name
        if (mailboxName == null)
          t.RequestArguments["mailbox-name"] = ImapQuotedString.Empty;
        else
          t.RequestArguments["mailbox-name"] = mailboxName;

        // options
        if (options != null)
          t.RequestArguments["options"] = options;

        // entry-specifier
        if (entrySpecifiers.Length == 1)
          t.RequestArguments["entry-specifier"] = entrySpecifiers[0];
        else
          t.RequestArguments["entry-specifier"] = new ImapParenthesizedString(entrySpecifiers);

        if (ProcessTransaction(t).Succeeded)
          metadata = t.Result.Value;

        return t.Result;
      }
    }
#endregion

#region "SETMETADATA"
#region "server entries"
    /// <summary>sends SETMETADATA command</summary>
    /// <remarks>
    /// valid in authenticated/selected state.
    /// this method will fail if server does not support METADATA or METADATA-SERVER extension.
    /// </remarks>
    public ImapCommandResult SetMetadata(params ImapMetadata[] metadata)
    {
      return SetMetadataInternal(null,
                                 metadata);
    }

    /// <summary>sends SETMETADATA command and sets NIL value to remove entries</summary>
    /// <remarks>
    /// valid in authenticated/selected state.
    /// this method will fail if server does not support METADATA or METADATA-SERVER extension.
    /// </remarks>
    public ImapCommandResult SetMetadata(string[] entrySpecifiers)
    {
      return SetMetadataInternal(null,
                                 Array.ConvertAll<string, ImapMetadata>(entrySpecifiers, ImapMetadata.CreateNil));
    }
#endregion

#region "mailbox entries"
    /// <summary>sends SETMETADATA command</summary>
    /// <remarks>
    /// valid in authenticated/selected state.
    /// this method will fail if server does not support METADATA extension.
    /// </remarks>
    public ImapCommandResult SetMetadata(ImapMailbox mailbox, params ImapMetadata[] metadata)
    {
      ValidateMailboxRelationship(mailbox);

      return SetMetadataInternal(ImapMailboxNameString.CreateMailboxNameNonEmpty(mailbox.Name),
                                 metadata);
    }

    /// <summary>sends SETMETADATA command</summary>
    /// <remarks>
    /// valid in authenticated/selected state.
    /// this method will fail if server does not support METADATA extension.
    /// </remarks>
    public ImapCommandResult SetMetadata(string mailboxName, params ImapMetadata[] metadata)
    {
      return SetMetadataInternal(ImapMailboxNameString.CreateMailboxNameNonEmpty(mailboxName),
                                 metadata);
    }

    /// <summary>sends SETMETADATA command and sets NIL value to remove entries</summary>
    /// <remarks>
    /// valid in authenticated/selected state.
    /// this method will fail if server does not support METADATA extension.
    /// </remarks>
    public ImapCommandResult SetMetadata(ImapMailbox mailbox, params string[] entrySpecifiers)
    {
      ValidateMailboxRelationship(mailbox);

      return SetMetadataInternal(ImapMailboxNameString.CreateMailboxNameNonEmpty(mailbox.Name),
                                 Array.ConvertAll<string, ImapMetadata>(entrySpecifiers, ImapMetadata.CreateNil));
    }

    /// <summary>sends SETMETADATA command and sets NIL value to remove entries</summary>
    /// <remarks>
    /// valid in authenticated/selected state.
    /// this method will fail if server does not support METADATA extension.
    /// </remarks>
    public ImapCommandResult SetMetadata(string mailboxName, params string[] entrySpecifiers)
    {
      return SetMetadataInternal(ImapMailboxNameString.CreateMailboxNameNonEmpty(mailboxName),
                                 Array.ConvertAll<string, ImapMetadata>(entrySpecifiers, ImapMetadata.CreateNil));
    }
#endregion

    private ImapCommandResult SetMetadataInternal(ImapMailboxNameString mailboxName,
                                                  ImapMetadata[] metadata)
    {
      if (metadata == null)
        throw new ArgumentNullException("metadata");

      if (handlesIncapableAsException)
        CheckServerCapabilityMetadata(mailboxName != null);

      using (var t = new SetMetadataTransaction(connection)) {
        // mailbox-name
        if (mailboxName == null)
          t.RequestArguments["mailbox-name"] = ImapQuotedString.Empty;
        else
          t.RequestArguments["mailbox-name"] = mailboxName;

        // list of entry, values
        var listOfEntryAndValues = new List<ImapString>(metadata.Length);

        foreach (var m in metadata) {
          if (m == null)
            throw new ArgumentException("contains null", "metadata");

          listOfEntryAndValues.Add(m.EntryName);

          if (m.Value == null)
            listOfEntryAndValues.Add(ImapNilString.Nil);
          else
            listOfEntryAndValues.Add(m.Value);
        }

        t.RequestArguments["list of entry, values"] = new ImapParenthesizedString(listOfEntryAndValues.ToArray());

        return ProcessTransaction(t);
      }
    }
#endregion

    private void CheckServerCapabilityMetadata(bool requireMailboxAnnotations)
    {
      /*
       * RFC 5464 - The IMAP METADATA Extension
       * http://tools.ietf.org/html/rfc5464
       * 1. Introduction and Overview
       *
       *    A server that supports both server and mailbox annotations indicates
       *    the presence of this extension by returning "METADATA" as one of the
       *    supported capabilities in the CAPABILITY command response.
       * 
       *    A server that supports only server annotations indicates the presence
       *    of this extension by returning "METADATA-SERVER" as one of the
       *    supported capabilities in the CAPABILITY command response.
       */
      if (serverCapabilities.Contains(ImapCapability.Metadata))
        return;
      else if (requireMailboxAnnotations)
        throw new ImapIncapableException(ImapCapability.Metadata);
      else if (!serverCapabilities.Contains(ImapCapability.MetadataServer))
        throw new ImapIncapableException(ImapCapability.MetadataServer);
    }
  }
}
