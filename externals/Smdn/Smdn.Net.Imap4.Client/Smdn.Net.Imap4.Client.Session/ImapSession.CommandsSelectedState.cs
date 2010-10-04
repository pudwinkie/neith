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
using System.Text;

using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;
using Smdn.Net.Imap4.Client.Transaction;
using Smdn.Net.Imap4.Client.Transaction.BuiltIn;

namespace Smdn.Net.Imap4.Client.Session {
  partial class ImapSession {
    /*
     * transaction methods : selected state
     */

    /// <summary>sends CHECK command</summary>
    /// <remarks>valid in selected state</remarks>
    public ImapCommandResult Check()
    {
      RejectNonSelectedState();

      using (var t = new CheckTransaction(connection)) {
        return ProcessTransaction(t);
      }
    }

    /// <summary>sends CLOSE command</summary>
    /// <remarks>valid in selected state</remarks>
    public ImapCommandResult Close()
    {
      RejectNonAuthenticatedState();

      if (state != ImapSessionState.Selected)
        return new ImapCommandResult(ImapCommandResultCode.RequestDone,
                                     "mailbox has already closed or has not selected yet");

      using (var t = new CloseTransaction(connection)) {
        if (ProcessTransaction(t).Succeeded) {
          selectedMailbox = null;
          TransitStateTo(ImapSessionState.Authenticated);
        }

        return t.Result;
      }
    }

    /// <summary>sends EXPUNGE command</summary>
    /// <remarks>valid in selected state</remarks>
    public ImapCommandResult Expunge()
    {
      long[] discard;

      return Expunge(out discard);
    }

    /// <summary>sends EXPUNGE command</summary>
    /// <remarks>valid in selected state</remarks>
    public ImapCommandResult Expunge(out long[] expungedMessages)
    {
      using (var t = new ExpungeTransaction(connection, false)) {
        return ExpungeInternal(t, null, out expungedMessages);
      }
    }

    /// <summary>sends EXPUNGE command</summary>
    /// <remarks>valid in selected state</remarks>
    public ImapCommandResult UidExpunge(ImapSequenceSet uidSet)
    {
      long[] discard;

      return UidExpunge(uidSet, out discard);
    }

    /// <summary>sends EXPUNGE command</summary>
    /// <remarks>valid in selected state</remarks>
    public ImapCommandResult UidExpunge(ImapSequenceSet uidSet, out long[] expungedMessages)
    {
      if (uidSet == null)
        throw new ArgumentNullException("uidSet");
      if (!uidSet.IsUidSet)
        throw new ArgumentException("sequence set is not UID", "uidSet");
      if (uidSet.IsEmpty)
        throw new ArgumentException("uid set is empty", "uidSet");

      using (var t = new ExpungeTransaction(connection, true)) {
        return ExpungeInternal(t, uidSet, out expungedMessages);
      }
    }

    private ImapCommandResult ExpungeInternal(ExpungeTransaction t, ImapSequenceSet uidSet, out long[] expungedMessages)
    {
      RejectNonSelectedState();

      expungedMessages = null;

      // sequence set for UID EXPUNGE
      if (uidSet != null)
        t.RequestArguments["sequence set"] = uidSet.ToString();

      if (ProcessTransaction(t).Succeeded)
        expungedMessages = t.Result.Value;

      return t.Result;
    }

    /// <summary>sends SEARCH command</summary>
    /// <remarks>valid in selected state</remarks>
    public ImapCommandResult Search(ImapSearchCriteria searchingCriteria, out ImapMatchedSequenceSet matched)
    {
      return Search(searchingCriteria, null, out matched);
    }

    /// <summary>sends SEARCH command</summary>
    /// <remarks>valid in selected state</remarks>
    public ImapCommandResult Search(ImapSearchCriteria searchingCriteria, Encoding charset, out ImapMatchedSequenceSet matched)
    {
      using (var t = new SearchTransaction(connection, false, null)) {
        return SearchSortInternal(t, null, searchingCriteria, charset, null, out matched);
      }
    }

    /// <summary>sends SEARCH command with RETURN result specifier</summary>
    /// <remarks>
    /// valid in selected state.
    /// this method will fail if server does not support ESEARCH extension.
    /// </remarks>
    public ImapCommandResult ESearch(ImapSearchCriteria searchingCriteria, ImapSearchResultOptions resultOptions, out ImapMatchedSequenceSet matched)
    {
      return ESearch(searchingCriteria, null, resultOptions, out matched);
    }

    /// <summary>sends SEARCH command with RETURN result specifier</summary>
    /// <remarks>
    /// valid in selected state.
    /// this method will fail if server does not support ESEARCH extension.
    /// </remarks>
    public ImapCommandResult ESearch(ImapSearchCriteria searchingCriteria, Encoding charset, ImapSearchResultOptions resultOptions, out ImapMatchedSequenceSet matched)
    {
      using (var t = new SearchTransaction(connection, false, ImapCapability.ESearch)) {
        return ESearchESortInternal(t, false, null, searchingCriteria, charset, resultOptions, out matched);
      }
    }

    /// <summary>sends UID SEARCH command</summary>
    /// <remarks>valid in selected state</remarks>
    public ImapCommandResult UidSearch(ImapSearchCriteria searchingCriteria, out ImapMatchedSequenceSet matched)
    {
      return UidSearch(searchingCriteria, null, out matched);
    }

    /// <summary>sends UID SEARCH command</summary>
    /// <remarks>valid in selected state</remarks>
    public ImapCommandResult UidSearch(ImapSearchCriteria searchingCriteria, Encoding charset, out ImapMatchedSequenceSet matched)
    {
      using (var t = new SearchTransaction(connection, true, null)) {
        return SearchSortInternal(t, null, searchingCriteria, charset, null, out matched);
      }
    }

    /// <summary>sends SEARCH command with RETURN result specifier</summary>
    /// <remarks>
    /// valid in selected state.
    /// this method will fail if server does not support ESEARCH extension.
    /// </remarks>
    public ImapCommandResult UidESearch(ImapSearchCriteria searchingCriteria, ImapSearchResultOptions resultOptions, out ImapMatchedSequenceSet matched)
    {
      return UidESearch(searchingCriteria, null, resultOptions, out matched);
    }

    /// <summary>sends SEARCH command with RETURN result specifier</summary>
    /// <remarks>
    /// valid in selected state.
    /// this method will fail if server does not support ESEARCH extension.
    /// </remarks>
    public ImapCommandResult UidESearch(ImapSearchCriteria searchingCriteria, Encoding charset, ImapSearchResultOptions resultOptions, out ImapMatchedSequenceSet matched)
    {
      using (var t = new SearchTransaction(connection, true, ImapCapability.ESearch)) {
        return ESearchESortInternal(t, true, null, searchingCriteria, charset, resultOptions, out matched);
      }
    }

    /// <summary>sends SORT command</summary>
    /// <remarks>valid in selected state</remarks>
    public ImapCommandResult Sort(ImapSortCriteria sortingCriteria, ImapSearchCriteria searchingCriteria, out ImapMatchedSequenceSet matched)
    {
      return Sort(sortingCriteria, searchingCriteria, null, out matched);
    }

    /// <summary>sends SORT command</summary>
    /// <remarks>valid in selected state</remarks>
    public ImapCommandResult Sort(ImapSortCriteria sortingCriteria, ImapSearchCriteria searchingCriteria, Encoding charset, out ImapMatchedSequenceSet matched)
    {
      using (var t = new SortTransaction(connection, false, null)) {
        return SearchSortInternal(t, sortingCriteria, searchingCriteria, charset, null, out matched);
      }
    }

    /// <summary>sends SORT command with RETURN result specifier</summary>
    /// <remarks>
    /// valid in selected state.
    /// this method will fail if server does not support ESORT extension.
    /// </remarks>
    public ImapCommandResult ESort(ImapSortCriteria sortingCriteria, ImapSearchCriteria searchingCriteria, ImapSearchResultOptions resultOptions, out ImapMatchedSequenceSet matched)
    {
      return ESort(sortingCriteria, searchingCriteria, null, resultOptions, out matched);
    }

    /// <summary>sends SORT command with RETURN result specifier</summary>
    /// <remarks>
    /// valid in selected state.
    /// this method will fail if server does not support ESORT extension.
    /// </remarks>
    public ImapCommandResult ESort(ImapSortCriteria sortingCriteria, ImapSearchCriteria searchingCriteria, Encoding charset, ImapSearchResultOptions resultOptions, out ImapMatchedSequenceSet matched)
    {
      using (var t = new SortTransaction(connection, false, ImapCapability.ESort)) {
        return ESearchESortInternal(t, false, sortingCriteria, searchingCriteria, charset, resultOptions, out matched);
      }
    }

    /// <summary>sends UID SORT command</summary>
    /// <remarks>valid in selected state</remarks>
    public ImapCommandResult UidSort(ImapSortCriteria sortingCriteria, ImapSearchCriteria searchingCriteria, out ImapMatchedSequenceSet matched)
    {
      return UidSort(sortingCriteria, searchingCriteria, (Encoding)null, out matched);
    }

    /// <summary>sends UID SORT command</summary>
    /// <remarks>valid in selected state</remarks>
    public ImapCommandResult UidSort(ImapSortCriteria sortingCriteria, ImapSearchCriteria searchingCriteria, Encoding charset, out ImapMatchedSequenceSet matched)
    {
      using (var t = new SortTransaction(connection, true, null)) {
        return SearchSortInternal(t, sortingCriteria, searchingCriteria, charset, null, out matched);
      }
    }

    /// <summary>sends UID SORT command with RETURN result specifier</summary>
    /// <remarks>
    /// valid in selected state.
    /// this method will fail if server does not support ESORT extension.
    /// </remarks>
    public ImapCommandResult UidESort(ImapSortCriteria sortingCriteria, ImapSearchCriteria searchingCriteria, ImapSearchResultOptions resultOptions, out ImapMatchedSequenceSet matched)
    {
      return UidESort(sortingCriteria, searchingCriteria, null, resultOptions, out matched);
    }

    /// <summary>sends UID SORT command with RETURN result specifier</summary>
    /// <remarks>
    /// valid in selected state.
    /// this method will fail if server does not support ESORT extension.
    /// </remarks>
    public ImapCommandResult UidESort(ImapSortCriteria sortingCriteria, ImapSearchCriteria searchingCriteria, Encoding charset, ImapSearchResultOptions resultOptions, out ImapMatchedSequenceSet matched)
    {
      using (var t = new SortTransaction(connection, true, ImapCapability.ESort)) {
        return ESearchESortInternal(t, true, sortingCriteria, searchingCriteria, charset, resultOptions, out matched);
      }
    }

    private ImapCommandResult ESearchESortInternal(SearchTransactionBase t, bool uid, ImapSortCriteria sortingCriteria, ImapSearchCriteria searchingCriteria, Encoding charset, ImapSearchResultOptions resultOptions, out ImapMatchedSequenceSet matched)
    {
      if (resultOptions == null)
        throw new ArgumentNullException("resultOptions");

      /*
       * RFC 5182 - IMAP Extension for Referencing the Last SEARCH Result
       * http://tools.ietf.org/html/rfc5182
       */
      var saveResult = resultOptions.ContainsOneOf(ImapSearchResultOptions.Save);

      // ESEARCH or ESORT capability
      if (t is SearchTransaction) {
        if (!resultOptions.RequiredCapabilities.Contains(ImapCapability.ESearch))
          resultOptions.RequiredCapabilities.Add(ImapCapability.ESearch);
        if (resultOptions.RequiredCapabilities.Contains(ImapCapability.ESort))
          resultOptions.RequiredCapabilities.Remove(ImapCapability.ESort);
      }
      else if (t is SortTransaction) {
        if (!resultOptions.RequiredCapabilities.Contains(ImapCapability.ESort))
          resultOptions.RequiredCapabilities.Add(ImapCapability.ESort);
        if (resultOptions.RequiredCapabilities.Contains(ImapCapability.ESearch))
          resultOptions.RequiredCapabilities.Remove(ImapCapability.ESearch);
      }

      SearchSortInternal(t, sortingCriteria, searchingCriteria, charset, resultOptions, out matched);

      if (saveResult) {
        var saved = t.Result.Succeeded && (t.Result.GetResponseCode(ImapResponseCode.NotSaved) == null);

        if (saved)
          matched = ImapMatchedSequenceSet.CreateSavedResult(matched == null
                                                             ? matched = new ImapMatchedSequenceSet(ImapSequenceSet.CreateSet(uid, new long[] {}))
                                                             : matched);
      }

      return t.Result;
    }

    private ImapCommandResult SearchSortInternal(SearchTransactionBase t, ImapSortCriteria sortingCriteria, ImapSearchCriteria searchingCriteria, Encoding charset, ImapSearchResultOptions resultOptions, out ImapMatchedSequenceSet matched)
    {
      if (t is SortTransaction && sortingCriteria == null)
        throw new ArgumentNullException("sortingCriteria");
      if (searchingCriteria == null)
        throw new ArgumentNullException("searchingCriteria");

      RejectNonSelectedState();

      matched = null;

      /*
       * 6.4.4. SEARCH Command
       *    US-ASCII MUST be supported; other [CHARSET]s MAY be supported.
       * 
       * http://tools.ietf.org/html/rfc5256
       * BASE.6.4.SORT. SORT Command
       *    The US-ASCII and [UTF-8] charsets MUST be implemented.
       *    All other charsets are optional.
       */
      if (charset == null) {
        if (t is SortTransaction)
          charset = Encoding.UTF8;
        else
          charset = Encoding.ASCII;
      }

      var charsetSpecified = searchingCriteria.SetCharset(charset);

      /*
       * RFC 4466 - Collected Extensions to IMAP4 ABNF
       * http://tools.ietf.org/html/rfc4466
       *
       *    search          = "SEARCH" [search-return-opts]
       *                      SP search-program
       *    search-correlator  = SP "(" "TAG" SP tag-string ")"
       *    search-program     = ["CHARSET" SP charset SP]
       *                         search-key *(SP search-key)
       *                         ;; CHARSET argument to SEARCH MUST be
       *                         ;; registered with IANA.
       *    search-return-data = search-modifier-name SP search-return-value
       *                         ;; Note that not every SEARCH return option
       *                         ;; is required to have the corresponding
       *                         ;; ESEARCH return data.
       *    search-return-opts = SP "RETURN" SP "(" [search-return-opt
       *                         *(SP search-return-opt)] ")"
       *    search-return-opt = search-modifier-name [SP search-mod-params]
       *    search-return-value = tagged-ext-val
       *                         ;; Data for the returned search option.
       *                         ;; A single "nz-number"/"number" value
       *                         ;; can be returned as an atom (i.e., without
       *                         ;; quoting).  A sequence-set can be returned
       *                         ;; as an atom as well.
       *    search-modifier-name = tagged-ext-label
       *    search-mod-params = tagged-ext-val
       *                      ;; This non-terminal shows recommended syntax
       *                      ;; for future extensions.
       */

      /* 
       * http://tools.ietf.org/html/rfc5256
       * BASE.6.4.SORT. SORT Command
       * The charset argument is mandatory (unlike SEARCH)
       */
      // charset specification
      if (t is SortTransaction || charsetSpecified)
        t.RequestArguments["charset specification"] = charset.WebName;

      // search-return-opt
      if (resultOptions != null)
        t.RequestArguments["result specifier"] = resultOptions;

      // searching criteria
      t.RequestArguments["searching criteria"] = searchingCriteria;

        // sort criteria
      if (sortingCriteria != null)
        t.RequestArguments["sort criteria"] = sortingCriteria;

      if (ProcessTransaction(t).Succeeded)
        matched = t.Result.Value;

      // TODO: BADCHARSET
      // 6.4.4. SEARCH Command
      //       If the server does not support the specified [CHARSET], it MUST
      //       return a tagged NO response (not a BAD).  This response SHOULD
      //       contain the BADCHARSET response code, which MAY list the
      //       [CHARSET]s supported by the server.

      return t.Result;
    }

    /// <summary>Infrastructure. It is not intended to be used directly from your code.</summary>
    public ImapCommandResult UidSortPreformatted(ImapSortCriteria sortingCriteria, ImapSearchCriteria searchingCriteria, string charset, out ImapMatchedSequenceSet matched)
    {
      if (sortingCriteria == null)
        throw new ArgumentNullException("sortingCriteria");
      if (searchingCriteria == null)
        throw new ArgumentNullException("searchingCriteria");

      RejectNonSelectedState();

      matched = null;

      if (charset == null)
        charset = Encoding.UTF8.WebName;

      using (var t = new SortTransaction(connection, true, null)) {
        // charset specification
        t.RequestArguments["charset specification"] = charset;

        // searching criteria
        t.RequestArguments["searching criteria"] = searchingCriteria;

        // sort criteria
        t.RequestArguments["sort criteria"] = sortingCriteria;

        if (ProcessTransaction(t).Succeeded)
          matched = t.Result.Value;

        return t.Result;
      }
    }

    /// <summary>sends THREAD command</summary>
    /// <remarks>valid in selected state</remarks>
    public ImapCommandResult Thread(ImapThreadingAlgorithm threadingAlgorithm, ImapSearchCriteria searchingCriteria, out ImapThreadList threadList)
    {
      using (var t = new ThreadTransaction(connection, false)) {
        return ThreadInternal(t, threadingAlgorithm, searchingCriteria, Encoding.UTF8, out threadList);
      }
    }

    /// <summary>sends THREAD command</summary>
    /// <remarks>valid in selected state</remarks>
    public ImapCommandResult Thread(ImapThreadingAlgorithm threadingAlgorithm, ImapSearchCriteria searchingCriteria, Encoding charset, out ImapThreadList threadList)
    {
      using (var t = new ThreadTransaction(connection, false)) {
        return ThreadInternal(t, threadingAlgorithm, searchingCriteria, charset, out threadList);
      }
    }

    /// <summary>sends UID THREAD command</summary>
    /// <remarks>valid in selected state</remarks>
    public ImapCommandResult UidThread(ImapThreadingAlgorithm threadingAlgorithm, ImapSearchCriteria searchingCriteria, out ImapThreadList threadList)
    {
      using (var t = new ThreadTransaction(connection, true)) {
        return ThreadInternal(t, threadingAlgorithm, searchingCriteria, Encoding.UTF8, out threadList);
      }
    }

    /// <summary>sends UID THREAD command</summary>
    /// <remarks>valid in selected state</remarks>
    public ImapCommandResult UidThread(ImapThreadingAlgorithm threadingAlgorithm, ImapSearchCriteria searchingCriteria, Encoding charset, out ImapThreadList threadList)
    {
      using (var t = new ThreadTransaction(connection, true)) {
        return ThreadInternal(t, threadingAlgorithm, searchingCriteria, charset, out threadList);
      }
    }

    private ImapCommandResult ThreadInternal(ThreadTransaction t, ImapThreadingAlgorithm threadingAlgorithm, ImapSearchCriteria searchingCriteria, Encoding charset, out ImapThreadList threadList)
    {
      if (searchingCriteria == null)
        throw new ArgumentNullException("searchingCriteria");
      if (charset == null)
        throw new ArgumentNullException("charset");

      /*
       * http://tools.ietf.org/html/rfc5256
       * BASE.6.4.THREAD. THREAD Command
       *   The US-ASCII and [UTF-8] charsets MUST be implemented.
       */
      if (charset == null)
        charset = Encoding.UTF8;

      searchingCriteria.SetCharset(charset);

      return ThreadInternal(t, threadingAlgorithm, searchingCriteria, charset.WebName, out threadList);
    }

    /// <summary>Infrastructure. It is not intended to be used directly from your code.</summary>
    public ImapCommandResult UidThreadPreformatted(ImapThreadingAlgorithm threadingAlgorithm, ImapSearchCriteria searchingCriteria, string charset, out ImapThreadList threadList)
    {
      using (var t = new ThreadTransaction(connection, true)) {
        return ThreadInternal(t, threadingAlgorithm, searchingCriteria, charset ?? Encoding.UTF8.WebName, out threadList);
      }
    }

    private ImapCommandResult ThreadInternal(ThreadTransaction t, ImapThreadingAlgorithm threadingAlgorithm, ImapSearchCriteria searchingCriteria, string charset, out ImapThreadList threadList)
    {
      if (threadingAlgorithm == null)
        throw new ArgumentNullException("threadingAlgorithm");

      RejectNonSelectedState();

      threadList = null;

      // threading algorithm
      t.RequestArguments["threading algorithm"] = threadingAlgorithm;

      // charset specification
      t.RequestArguments["charset specification"] = charset;

      // searching criteria
      t.RequestArguments["searching criteria"] = searchingCriteria;

      if (ProcessTransaction(t).Succeeded)
        threadList = t.Result.Value;

      return t.Result;
    }

    /// <summary>sends FETCH command</summary>
    /// <remarks>valid in selected state</remarks>
    public ImapCommandResult Fetch<TMessageAttribute>(ImapSequenceSet sequenceOrUidSet,
                                                      ImapFetchDataItem messageDataItems,
                                                      out TMessageAttribute[] messages)
      where TMessageAttribute : ImapMessageAttributeBase
    {
      return FetchInternal(sequenceOrUidSet, messageDataItems, null, null, out messages);
    }

    /// <summary>sends FETCH command with CHANGEDSINCE FETCH modifier</summary>
    /// <remarks>
    /// valid in selected state.
    /// this method will fail if server does not support CONDSTORE extension.
    /// </remarks>
    [CLSCompliant(false)]
    public ImapCommandResult FetchChangedSince<TMessageAttribute>(ImapSequenceSet sequenceOrUidSet,
                                                                  ImapFetchDataItem messageDataItems,
                                                                  ulong modSeq,
                                                                  out TMessageAttribute[] messages)
      where TMessageAttribute : ImapMessageAttributeBase
    {
      var fetchModifiers = new ImapParenthesizedString("CHANGEDSINCE", modSeq.ToString());

      return FetchInternal(sequenceOrUidSet, messageDataItems, fetchModifiers, ImapCapability.CondStore, out messages);
    }

    private ImapCommandResult FetchInternal<TMessageAttribute>(ImapSequenceSet sequenceOrUidSet,
                                                               ImapFetchDataItem messageDataItems,
                                                               ImapParenthesizedString fetchModifiers,
                                                               ImapCapability fetchModifiersCapabilityRequirement,
                                                               out TMessageAttribute[] messages)
      where TMessageAttribute : ImapMessageAttributeBase
    {
      RejectNonSelectedState();

      if (sequenceOrUidSet == null)
        throw new ArgumentNullException("sequenceOrUidSet");
      else if (sequenceOrUidSet.IsEmpty)
        throw new ArgumentException("sequence or uid set is empty", "sequenceOrUidSet");

      if (messageDataItems == null)
        throw new ArgumentNullException("messageDataItems");

      messages = null;

      using (var t = new FetchTransaction<TMessageAttribute>(connection, sequenceOrUidSet.IsUidSet, fetchModifiersCapabilityRequirement)) {
        /*
         * RFC 4466 - Collected Extensions to IMAP4 ABNF
         * http://tools.ietf.org/html/rfc4466
         *
         *    fetch           = "FETCH" SP sequence-set SP ("ALL" / "FULL" /
         *                      "FAST" / fetch-att /
         *                      "(" fetch-att *(SP fetch-att) ")")
         *                      [fetch-modifiers]
         *                      ;; modifies the original IMAP4 FETCH command to
         *                      ;; accept optional modifiers
         *    fetch-modifiers = SP "(" fetch-modifier *(SP fetch-modifier) ")"
         *    fetch-modifier  = fetch-modifier-name [ SP fetch-modif-params ]
         *    fetch-modif-params  = tagged-ext-val
         *                      ;; This non-terminal shows recommended syntax
         *                      ;; for future extensions.
         *    fetch-modifier-name = tagged-ext-label
         */
        // sequence set
        t.RequestArguments["sequence set"] = sequenceOrUidSet.ToString();

        // message data item names or macro
        t.RequestArguments["message data item names or macro"] = messageDataItems;

        // fetch-modifiers
        if (fetchModifiers != null)
          t.RequestArguments["fetch modifiers"] = fetchModifiers;

        if (ProcessTransaction(t).Succeeded) {
          messages = t.Result.Value;

          UpdateBaseUrls(messages);
        }

        return t.Result;
      }
    }

    private void UpdateBaseUrls(ImapMessageAttributeBase[] messages)
    {
      if (messages == null)
        return;

      foreach (var message in messages) {
        var staticAttr = message as IImapMessageStaticAttribute;

        if (staticAttr == null)
          continue;

        staticAttr.SetBaseUrl(SelectedMailbox.UrlBuilder);
      }
    }

    /// <summary>sends STORE command</summary>
    /// <remarks>valid in selected state</remarks>
    public ImapCommandResult Store(ImapSequenceSet sequenceOrUidSet, ImapStoreDataItem storeDataItem)
    {
      ImapMessageAttribute[] discard;

      return Store(sequenceOrUidSet, storeDataItem, out discard);
    }

    /// <summary>sends STORE command</summary>
    /// <remarks>valid in selected state</remarks>
    public ImapCommandResult Store(ImapSequenceSet sequenceOrUidSet, ImapStoreDataItem storeDataItem, out ImapMessageAttribute[] messageAttributes)
    {
      return StoreInternal(sequenceOrUidSet, storeDataItem, null, null, out messageAttributes);
    }

    /// <summary>sends STORE command with UNCHANGEDSINCE STORE modifier</summary>
    /// <remarks>
    /// valid in selected state.
    /// this method will fail if server does not support CONDSTORE extension.
    /// </remarks>
    [CLSCompliant(false)]
    public ImapCommandResult StoreUnchangedSince(ImapSequenceSet sequenceOrUidSet, ImapStoreDataItem storeDataItem, ulong modSeq)
    {
      ImapMessageAttribute[] discard;
      ImapSequenceSet discard2;

      return StoreUnchangedSince(sequenceOrUidSet, storeDataItem, modSeq, out discard, out discard2);
    }

    /// <summary>sends STORE command with UNCHANGEDSINCE STORE modifier</summary>
    /// <remarks>
    /// valid in selected state.
    /// this method will fail if server does not support CONDSTORE extension.
    /// </remarks>
    [CLSCompliant(false)]
    public ImapCommandResult StoreUnchangedSince(ImapSequenceSet sequenceOrUidSet, ImapStoreDataItem storeDataItem, ulong modSeq, out ImapMessageAttribute[] messageAttributes)
    {
      ImapSequenceSet discard;

      return StoreUnchangedSince(sequenceOrUidSet, storeDataItem, modSeq, out messageAttributes, out discard);
    }

    /// <summary>sends STORE command with UNCHANGEDSINCE STORE modifier</summary>
    /// <remarks>
    /// valid in selected state.
    /// this method will fail if server does not support CONDSTORE extension.
    /// </remarks>
    [CLSCompliant(false)]
    public ImapCommandResult StoreUnchangedSince(ImapSequenceSet sequenceOrUidSet, ImapStoreDataItem storeDataItem, ulong modSeq, out ImapSequenceSet failedMessageSet)
    {
      ImapMessageAttribute[] discard;

      return StoreUnchangedSince(sequenceOrUidSet, storeDataItem, modSeq, out discard, out failedMessageSet);
    }

    /// <summary>sends STORE command with UNCHANGEDSINCE STORE modifier</summary>
    /// <remarks>
    /// valid in selected state.
    /// this method will fail if server does not support CONDSTORE extension.
    /// </remarks>
    [CLSCompliant(false)]
    public ImapCommandResult StoreUnchangedSince(ImapSequenceSet sequenceOrUidSet, ImapStoreDataItem storeDataItem, ulong modSeq, out ImapMessageAttribute[] messageAttributes, out ImapSequenceSet failedMessageSet)
    {
      failedMessageSet = null;
      messageAttributes = null;

      var storeModifiers = new ImapParenthesizedString("UNCHANGEDSINCE", modSeq.ToString());

      var ret = StoreInternal(sequenceOrUidSet, storeDataItem, storeModifiers, ImapCapability.CondStore, out messageAttributes);

      var modified = lastTransactionResult.GetResponseCode(ImapResponseCode.Modified);

      if (modified is ImapTaggedStatusResponse)
        failedMessageSet = ImapResponseTextConverter.FromModified(modified.ResponseText, sequenceOrUidSet.IsUidSet);

      return ret;
    }

    private ImapCommandResult StoreInternal(ImapSequenceSet sequenceOrUidSet,
                                            ImapStoreDataItem storeDataItem,
                                            ImapParenthesizedString storeModifiers,
                                            ImapCapability storeModifiersCapabilityRequirement,
                                            out ImapMessageAttribute[] messageAttributes)
    {
      RejectNonSelectedState();

      if (sequenceOrUidSet == null)
        throw new ArgumentNullException("sequenceOrUidSet");
      else if (sequenceOrUidSet.IsEmpty)
        throw new ArgumentException("sequence or uid set is empty", "sequenceOrUidSet");

      if (storeDataItem == null)
        throw new ArgumentNullException("storeDataItem");

      messageAttributes = null;

      using (var t = new StoreTransaction(connection, sequenceOrUidSet.IsUidSet, storeModifiersCapabilityRequirement)) {
        /*
         * RFC 4466 - Collected Extensions to IMAP4 ABNF
         * http://tools.ietf.org/html/rfc4466
         *
         *    store           = "STORE" SP sequence-set [store-modifiers]
         *                      SP store-att-flags
         *                      ;; extend [IMAP4] STORE command syntax
         *                      ;; to allow for optional store-modifiers
         *    store-modifiers =  SP "(" store-modifier *(SP store-modifier)
         *                        ")"
         *    store-modifier  = store-modifier-name [SP store-modif-params]
         *    store-modif-params = tagged-ext-val
         *                      ;; This non-terminal shows recommended syntax
         *                      ;; for future extensions.
         *    store-modifier-name = tagged-ext-label
         */
        // message set
        t.RequestArguments["message set"] = sequenceOrUidSet.ToString();

        // store-modifiers
        if (storeModifiers != null)
          t.RequestArguments["store modifiers"] = storeModifiers;

        // message data item name
        t.RequestArguments["message data item name"] = storeDataItem.ItemName;

        // value for message data item
        t.RequestArguments["value for message data item"] = storeDataItem;

        ProcessTransaction(t);

        messageAttributes = t.Result.Value;

        UpdateBaseUrls(messageAttributes);

        return t.Result;
      }
    }

    /// <summary>sends COPY command</summary>
    /// <remarks>valid in selected state</remarks>
    public ImapCommandResult Copy(ImapSequenceSet sequenceOrUidSet, ImapMailbox mailbox)
    {
      ValidateMailboxRelationship(mailbox);

      ImapCopiedUidSet discard;
      ImapMailbox discard2;

      return CopyInternal(sequenceOrUidSet, mailbox.Name, false, out discard, out discard2);
    }

    /// <summary>sends COPY command</summary>
    /// <remarks>
    /// valid in selected state.
    /// the out parameter <paramref name="copiedUids"/> will be set if the server supports UIDPLUS extension and returns [COPYUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult Copy(ImapSequenceSet sequenceOrUidSet, ImapMailbox mailbox, out ImapCopiedUidSet copiedUids)
    {
      ValidateMailboxRelationship(mailbox);

      ImapMailbox discard;

      return CopyInternal(sequenceOrUidSet, mailbox.Name, false, out copiedUids, out discard);
    }

    /// <summary>sends COPY command</summary>
    /// <remarks>valid in selected state</remarks>
    public ImapCommandResult Copy(ImapSequenceSet sequenceOrUidSet, string mailboxName)
    {
      ImapCopiedUidSet discard;
      ImapMailbox discard2;

      return CopyInternal(sequenceOrUidSet, mailboxName, false, out discard, out discard2);
    }

    /// <summary>sends COPY command</summary>
    /// <remarks>
    /// valid in selected state.
    /// the out parameter <paramref name="copiedUids"/> will be set if the server supports UIDPLUS extension and returns [COPYUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult Copy(ImapSequenceSet sequenceOrUidSet, string mailboxName, out ImapCopiedUidSet copiedUids)
    {
      ImapMailbox discard;

      return CopyInternal(sequenceOrUidSet, mailboxName, false, out copiedUids, out discard);
    }

    /// <summary>sends COPY command</summary>
    /// <remarks>
    /// valid in selected state.
    /// this method tries to automatically create the mailbox when server sent [TRYCREATE] response code
    /// </remarks>
    public ImapCommandResult Copy(ImapSequenceSet sequenceOrUidSet, string mailboxName, out ImapMailbox createdMailbox)
    {
      ImapCopiedUidSet discard;

      return CopyInternal(sequenceOrUidSet, mailboxName, true, out discard, out createdMailbox);
    }

    /// <summary>sends COPY command</summary>
    /// <remarks>
    /// valid in selected state.
    /// this method tries to automatically create the mailbox when server sent [TRYCREATE] response code.
    /// the out parameter <paramref name="copiedUids"/> will be set if the server supports UIDPLUS extension and returns [COPYUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult Copy(ImapSequenceSet sequenceOrUidSet, string mailboxName, out ImapCopiedUidSet copiedUids, out ImapMailbox createdMailbox)
    {
      return CopyInternal(sequenceOrUidSet, mailboxName, true, out copiedUids, out createdMailbox);
    }

    private ImapCommandResult CopyInternal(ImapSequenceSet sequenceOrUidSet, string mailboxName, bool tryCreate, out ImapCopiedUidSet copiedUids, out ImapMailbox createdMailbox)
    {
      RejectNonSelectedState();

      if (sequenceOrUidSet == null)
        throw new ArgumentNullException("sequenceOrUidSet");
      else if (sequenceOrUidSet.IsEmpty)
        throw new ArgumentException("sequence or uid set is empty", "sequenceOrUidSet");

      RejectInvalidMailboxNameArgument(mailboxName);

      copiedUids = null;
      createdMailbox = null;

      ImapCommandResult failedResult = null;

      for (var i = 0; i < 2; i++) {
        var respTryCreate = false;

        using (var t = new CopyTransaction(connection, sequenceOrUidSet.IsUidSet)) {
          // sequence set
          t.RequestArguments["sequence set"] = sequenceOrUidSet.ToString();

          // mailbox name
          t.RequestArguments["mailbox name"] = new ImapMailboxNameString(mailboxName);

          if (ProcessTransaction(t).Succeeded) {
            copiedUids = t.Result.Value;
            return t.Result;
          }
          else {
            if (ProcessMailboxRefferalResponse(t.Result.TaggedStatusResponse) || !tryCreate)
              return t.Result;
          }

          failedResult = t.Result;

          // 6.4.7. COPY Command
          //       If the destination mailbox does not exist, a server SHOULD return
          //       an error.  It SHOULD NOT automatically create the mailbox.  Unless
          //       it is certain that the destination mailbox can not be created, the
          //       server MUST send the response code "[TRYCREATE]" as the prefix of
          //       the text of the tagged NO response.  This gives a hint to the
          //       client that it can attempt a CREATE command and retry the COPY if
          //       the CREATE is successful.
          respTryCreate = (t.Result.GetResponseCode(ImapResponseCode.TryCreate) is ImapTaggedStatusResponse);
        }

        // try create
        if (i == 0 && respTryCreate)
          if (Create(mailboxName, out createdMailbox).Failed)
            return failedResult;
      }

      return failedResult;
    }

    /// <summary>sends UNSELECT command</summary>
    /// <remarks>valid in selected state</remarks>
    public ImapCommandResult Unselect()
    {
      RejectNonSelectedState();

      using (var t = new UnselectTransaction(connection)) {
        var r = ProcessTransaction(t);

        if (r.Succeeded) {
          selectedMailbox = null;
          TransitStateTo(ImapSessionState.Authenticated);
        }

        return r;
      }
    }

    private void ProcessUpdatedSizeAndStatusResponse(ImapCommandResult result)
    {
      if (selectedMailbox == null || !updateSelectedMailboxSizeAndStatus)
        return;

      // 7.2. Server Responses - Server and Mailbox Status
      // 7.2.6. FLAGS Response
      // 7.3. Server Responses - Mailbox Size
      // 7.3.1. EXISTS Response
      // 7.3.2. RECENT Response
      // 7.4. Server Responses - Message Status
      // 7.4.1. EXPUNGE Response
      // 7.4.2. FETCH Response
      foreach (var resp in result.ReceivedResponses) {
        var data = resp as ImapDataResponse;

        if (data == null)
          continue;

        if (data.Type == ImapDataResponseType.Flags) {
          //       The update from the FLAGS response MUST be recorded by the client.
          selectedMailbox.ApplicableFlags = ImapDataResponseConverter.FromFlags(data);
        }
        else if (data.Type == ImapDataResponseType.Exists) {
          //       The update from the EXISTS response MUST be recorded by the
          //       client.
          selectedMailbox.ExistsMessage = ImapDataResponseConverter.FromExists(data);
        }
        else if (data.Type == ImapDataResponseType.Recent) {
          //       The update from the RECENT response MUST be recorded by the
          //       client.
          selectedMailbox.RecentMessage = ImapDataResponseConverter.FromRecent(data);
        }
        else if (data.Type == ImapDataResponseType.Expunge) {
          //       The update from the EXPUNGE response MUST be recorded by the
          //       client.
          if (0L < selectedMailbox.ExistsMessage)
            selectedMailbox.ExistsMessage -= 1L;
        }
      }
    }
  }
}
