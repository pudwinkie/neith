using System;
using System.Text;
using NUnit.Framework;

using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Session {
  [TestFixture]
  public class ImapSessionCommandsSelectedStateTests : ImapSessionTestsBase {
    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestCloseInNonAuthenticatedState()
    {
      using (var session = Connect()) {
        session.Close();
      }
    }

    [Test]
    public void TestCloseInAuthenticatedState()
    {
      using (var session = Authenticate()) {
        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
        Assert.IsNull(session.SelectedMailbox);

        var result = session.Close();

        Assert.IsTrue((bool)result);
        Assert.AreEqual(result.Code, ImapCommandResultCode.RequestDone);

        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
        Assert.IsNull(session.SelectedMailbox);
      }
    }

    [Test]
    public void TestCheckStatusUpdate()
    {
      using (var session = SelectMailbox()) {
        // CHECK transaction
        server.EnqueueResponse("* 23 EXISTS\r\n" +
                               "* 1 RECENT\r\n" +
                               "0004 OK CHECK completed\r\n");

        Assert.IsTrue((bool)session.Check());

        Assert.AreEqual("0004 CHECK\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(23L, session.SelectedMailbox.ExistsMessage, "selected mailbox exist message count");
        Assert.AreEqual(1L, session.SelectedMailbox.RecentMessage, "selected mailbox recent message count");

        CloseMailbox(session);
      }
    }

    [Test]
    public void TestUnselect()
    {
      using (var session = SelectMailbox("UNSELECT")) {
        // EXPUNGE transaction
        server.EnqueueResponse("0004 OK UNSELECT completed\r\n");
  
        Assert.IsTrue((bool)session.Unselect());
  
        Assert.AreEqual("0004 UNSELECT\r\n",
                        server.DequeueRequest());
  
        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
        Assert.IsNull(session.SelectedMailbox);
      }
    }

    [Test]
    public void TestSearch()
    {
      using (var session = SelectMailbox()) {
        // SEARCH transaction
        server.EnqueueResponse("* SEARCH 2 84 882\r\n" +
                               "0004 OK SEARCH completed\r\n");

        ImapMatchedSequenceSet matched;

        Assert.IsTrue((bool)session.Search(ImapSearchCriteria.Unseen, out matched));

        Assert.AreEqual("0004 SEARCH UNSEEN\r\n",
                        server.DequeueRequest());

        Assert.IsFalse(matched.IsUidSet);

        var messages = matched.ToArray();

        Assert.AreEqual(3, messages.Length);
        Assert.AreEqual(2,    messages[0]);
        Assert.AreEqual(84,   messages[1]);
        Assert.AreEqual(882,  messages[2]);

        CloseMailbox(session);
      }
    }

    [Test]
    public void TestSearchWithLiteralAndDefaultCharset()
    {
      using (var session = SelectMailbox()) {
        // SEARCH transaction
        server.EnqueueResponse("+ continue\r\n");
        server.EnqueueResponse("* SEARCH 2 84 882\r\n" +
                               "0004 OK SEARCH completed\r\n");

        ImapMatchedSequenceSet matched;

        Assert.IsTrue((bool)session.Search(ImapSearchCriteria.Subject("subject"), out matched));

        Assert.AreEqual("0004 SEARCH CHARSET us-ascii SUBJECT {7}\r\n" +
                        "subject\r\n",
                        server.DequeueAll());

        Assert.IsNotNull(matched);
        Assert.IsFalse(matched.IsUidSet);
        Assert.IsFalse(matched.IsEmpty);

        CloseMailbox(session);
      }
    }

    [Test]
    public void TestSearchWithLiteralAndSpecifiedCharset()
    {
      using (var session = SelectMailbox()) {
        // SEARCH transaction
        server.EnqueueResponse("+ continue\r\n");
        server.EnqueueResponse("* SEARCH 2 84 882\r\n" +
                               "0004 OK SEARCH completed\r\n");

        ImapMatchedSequenceSet matched;

        Assert.IsTrue((bool)session.Search(ImapSearchCriteria.Subject("件名"), Encoding.GetEncoding(932), out matched));

        Assert.AreEqual("0004 SEARCH CHARSET shift_jis SUBJECT {4}\r\n" +
                        "\x008c\x008f\x0096\x00bc\r\n",
                        server.DequeueAll(NetworkTransferEncoding.Transfer8Bit));

        Assert.IsNotNull(matched);
        Assert.IsFalse(matched.IsUidSet);
        Assert.IsFalse(matched.IsEmpty);

        CloseMailbox(session);
      }
    }

    [Test]
    public void TestSearchNothingMatched()
    {
      using (var session = SelectMailbox()) {
        // SEARCH transaction
        server.EnqueueResponse("+ continue\r\n");
        server.EnqueueResponse("* SEARCH\r\n" +
                               "0004 OK SEARCH completed\r\n");

        ImapMatchedSequenceSet matched;

        Assert.IsTrue((bool)session.Search(ImapSearchCriteria.Text("string not in mailbox"), Encoding.UTF8, out matched));

        Assert.AreEqual("0004 SEARCH CHARSET utf-8 TEXT {21}\r\n" +
                        "string not in mailbox\r\n",
                        server.DequeueAll());

        Assert.IsFalse(matched.IsUidSet);
        Assert.IsTrue(matched.IsEmpty);

        CloseMailbox(session);
      }
    }

    [Test]
    public void TestSearchWithCriteriaFromUri()
    {
      using (var session = SelectMailbox("LITERAL+")) {
        var b = new ImapUriBuilder(session.SelectedMailbox.Url);

        b.SearchCriteria = ImapSearchCriteria.From("差出人") & (ImapSearchCriteria.Seen | ImapSearchCriteria.Subject("件名"));
        b.Charset = Encoding.UTF8;

        // SEARCH transaction
        server.EnqueueResponse("* SEARCH 2 84 882\r\n" +
                               "0004 OK SEARCH completed\r\n");

        ImapMatchedSequenceSet matched;

        Assert.IsTrue((bool)session.Search(ImapSearchCriteria.FromUri(b.Uri),
                                           out matched));

        Assert.AreEqual("0004 SEARCH CHARSET utf-8 FROM {9+}\r\n" +
                                   "\x00E5\x00B7\x00AE\x00E5\x0087\x00BA\x00E4\x00BA\x00BA OR (SEEN) (SUBJECT {6+}\r\n" +
                                   "\x00E4\x00BB\x00B6\x00E5\x0090\x008D)\r\n",
                                   server.DequeueAll(NetworkTransferEncoding.Transfer8Bit));

        Assert.IsNotNull(matched);
        Assert.IsFalse(matched.IsUidSet);
        Assert.IsFalse(matched.IsEmpty);

        CloseMailbox(session);
      }
    }

    [Test, Ignore("not implemented")]
    public void TestSearchBadCharset()
    {
      using (var session = SelectMailbox()) {
        // SEARCH transaction
        server.EnqueueResponse("0004 NO [BADCHARSET ONDUL EUC-JP] Unknown charset\r\n");
        server.EnqueueResponse("+ OK\r\n");
        server.EnqueueResponse("0005 OK SEARCH completed\r\n");
  
        ImapMatchedSequenceSet matched;
  
        Assert.IsTrue((bool)session.Search(ImapSearchCriteria.Body("ほげほげ"), out matched));
  
        Assert.AreEqual("0004 search charset utf-8 BODY {12}\r\n",
                        server.DequeueRequest());
        Assert.AreEqual("0005 search charset EUC-JP BODY {8}\r\n",
                        server.DequeueRequest());
        server.DequeueRequest();
  
        CloseMailbox(session, "0006");
      }
    }

    [Test]
    public void TestSearchCondStoreModSeq()
    {
      using (var session = SelectMailbox("CONDSTORE")) {
        // SEARCH transaction
        server.EnqueueResponse("* SEARCH 2 5 6 (MODSEQ 917162500)\r\n" +
                               "0004 OK SEARCH completed\r\n");

        ImapMatchedSequenceSet matched;

        Assert.IsTrue((bool)session.Search(ImapSearchCriteria.ModSeq(620162338UL), out matched));

        Assert.AreEqual("0004 SEARCH MODSEQ 620162338\r\n",
                        server.DequeueRequest());

        Assert.IsFalse(matched.IsUidSet);
        Assert.AreEqual(917162500UL, matched.HighestModSeq);

        var messages = matched.ToArray();

        Assert.AreEqual(3, messages.Length);
        Assert.AreEqual(2, messages[0]);
        Assert.AreEqual(5, messages[1]);
        Assert.AreEqual(6, messages[2]);

        CloseMailbox(session);
      }
    }

    [Test]
    public void TestSearchCondStoreModSeqWithEntry()
    {
      using (var session = SelectMailbox("CONDSTORE")) {
        // SEARCH transaction
        server.EnqueueResponse("* SEARCH 2 5 6 7 11 12 18 19 20 23 (MODSEQ 917162500)\r\n" +
                               "0004 OK SEARCH completed\r\n");

        ImapMatchedSequenceSet matched;

        Assert.IsTrue((bool)session.Search(ImapSearchCriteria.ModSeqAllEntry(620162338UL, ImapMessageFlag.Draft), out matched));

        Assert.AreEqual("0004 SEARCH MODSEQ \"/flags/\\\\Draft\" all 620162338\r\n",
                        server.DequeueRequest());

        Assert.IsFalse(matched.IsUidSet);
        Assert.AreEqual(917162500UL, matched.HighestModSeq);

        var messages = matched.ToArray();

        Assert.AreEqual(10, messages.Length);
        Assert.AreEqual(2, messages[0]);
        Assert.AreEqual(5, messages[1]);
        Assert.AreEqual(6, messages[2]);
        Assert.AreEqual(23, messages[9]);

        CloseMailbox(session);
      }
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestSearchCondStoreIncapable()
    {
      using (var session = SelectMailbox()) {
        ImapMatchedSequenceSet matched;

        session.Search(ImapSearchCriteria.ModSeqAllEntry(620162338UL, ImapMessageFlag.Draft), out matched);
      }
    }

    [Test]
    public void TestESearch()
    {
      using (var session = SelectMailbox("ESEARCH")) {
        // SEARCH transaction
        server.EnqueueResponse("+ continue\r\n");
        server.EnqueueResponse("* ESEARCH (TAG \"0004\") MIN 2 COUNT 3\r\n" +
                               "0004 OK SEARCH completed\r\n");

        var criteria = ImapSearchCriteria.Flagged &
          ImapSearchCriteria.Since(new DateTime(1994, 2, 1)) &
          !(ImapSearchCriteria.From("Smith"));

        ImapMatchedSequenceSet matched;

        Assert.IsTrue((bool)session.ESearch(criteria, ImapSearchResultOptions.Min + ImapSearchResultOptions.Count, out matched));

        Assert.AreEqual("0004 SEARCH RETURN (MIN COUNT) CHARSET us-ascii FLAGGED SINCE 1-Feb-1994 NOT (FROM {5}\r\nSmith)\r\n",
                                   server.DequeueAll());

        Assert.IsFalse(matched.IsUidSet);
        Assert.IsTrue(matched.IsEmpty);
        Assert.IsNotNull(matched.Tag);
        Assert.AreEqual("0004", matched.Tag);
        Assert.IsNotNull(matched.Min);
        Assert.AreEqual(2, matched.Min);
        Assert.IsNotNull(matched.Count);
        Assert.AreEqual(3, matched.Count);

        CloseMailbox(session);
      }
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestESearchIncapable()
    {
      using (var session = SelectMailbox()) {
        ImapMatchedSequenceSet matched;

        session.ESearch(ImapSearchCriteria.All, ImapSearchResultOptions.Min, out matched);
      }
    }

    [Test]
    public void TestESearchSearchres()
    {
      using (var session = SelectMailbox("ESEARCH", "SEARCHRES")) {
        // SEARCH transaction
        server.EnqueueResponse("+ continue\r\n");
        server.EnqueueResponse("0004 OK SEARCH completed, result saved\r\n");

        var criteria = ImapSearchCriteria.Flagged &
          ImapSearchCriteria.Since(new DateTime(1994, 2, 1)) &
          !(ImapSearchCriteria.From("Smith"));

        ImapMatchedSequenceSet matched;

        Assert.IsTrue((bool)session.ESearch(criteria, ImapSearchResultOptions.Save, out matched));

        Assert.AreEqual("0004 SEARCH RETURN (SAVE) CHARSET us-ascii FLAGGED SINCE 1-Feb-1994 NOT (FROM {5}\r\nSmith)\r\n",
                                   server.DequeueAll());

        Assert.IsFalse(matched.IsUidSet);
        Assert.IsFalse(matched.IsEmpty);
        Assert.IsTrue(matched.IsSavedResult);

        // FETCH transaction
        server.EnqueueResponse("* 2 FETCH (UID 14)\r\n" +
                               "* 84 FETCH (UID 100)\r\n" +
                               "* 882 FETCH (UID 1115)\r\n" +
                               "0005 OK completed\r\n");

        ImapMessage[] fetched;

        Assert.IsTrue((bool)session.Fetch(matched, ImapFetchDataItem.Uid, out fetched));

        Assert.AreEqual("0005 FETCH $ (UID)\r\n",
                        server.DequeueRequest());

        CloseMailbox(session, "0006");
      }
    }

    [Test]
    public void TestESearchSearchresSearchWithSavedResult()
    {
      using (var session = SelectMailbox("ESEARCH", "SEARCHRES")) {
        // SEARCH transaction
        server.EnqueueResponse("+ continue\r\n");
        server.EnqueueResponse("0004 OK SEARCH completed, result saved\r\n");

        var criteria = ImapSearchCriteria.Flagged &
          ImapSearchCriteria.Since(new DateTime(1994, 2, 1)) &
          !(ImapSearchCriteria.From("Smith"));

        ImapMatchedSequenceSet matched;

        Assert.IsTrue((bool)session.ESearch(criteria, ImapSearchResultOptions.Save, out matched));

        Assert.AreEqual("0004 SEARCH RETURN (SAVE) CHARSET us-ascii FLAGGED SINCE 1-Feb-1994 NOT (FROM {5}\r\nSmith)\r\n",
                                   server.DequeueAll());

        Assert.IsFalse(matched.IsUidSet);
        Assert.IsFalse(matched.IsEmpty);
        Assert.IsTrue(matched.IsSavedResult);

        // SEARCH transaction
        server.EnqueueResponse("* SEARCH 17 900 901\r\n" +
                               "0005 OK completed\r\n");

        ImapMatchedSequenceSet matched2;

        Assert.IsTrue((bool)session.UidSearch(ImapSearchCriteria.Uid(matched) & ImapSearchCriteria.Smaller(4096), out matched2));

        Assert.AreEqual("0005 UID SEARCH UID $ SMALLER 4096\r\n",
                        server.DequeueRequest());

        Assert.IsTrue(matched2.IsUidSet);
        Assert.IsFalse(matched2.IsEmpty);

        var arr = matched2.ToArray();

        Assert.AreEqual(3, arr.Length);
        Assert.AreEqual(17, arr[0]);
        Assert.AreEqual(900, arr[1]);
        Assert.AreEqual(901, arr[2]);

        CloseMailbox(session, "0006");
      }
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestESearchSearchresIncapable()
    {
      using (var session = SelectMailbox("ESEARCH")) {
        // SEARCH transaction
        var criteria = ImapSearchCriteria.Flagged &
          ImapSearchCriteria.Since(new DateTime(1994, 2, 1)) &
          !(ImapSearchCriteria.From("Smith"));

        ImapMatchedSequenceSet matched;

        session.ESearch(criteria, ImapSearchResultOptions.Save, out matched);
      }
    }

    [Test]
    public void TestSort()
    {
      using (var session = SelectMailbox("SORT")) {
        // SORT transaction
        server.EnqueueResponse("* SORT 5 3 4 1 2\r\n" +
                               "0004 OK SORT completed\r\n");

        ImapMatchedSequenceSet matched;

        Assert.IsTrue((bool)session.Sort(ImapSortCriteria.SizeReverse + ImapSortCriteria.Date, ImapSearchCriteria.All, out matched));

        Assert.AreEqual("0004 SORT (REVERSE SIZE DATE) utf-8 ALL\r\n",
                        server.DequeueRequest());

        Assert.IsFalse(matched.IsUidSet);

        var messages = matched.ToArray();

        Assert.AreEqual(5, messages.Length);
        Assert.AreEqual(5, messages[0]);
        Assert.AreEqual(3, messages[1]);
        Assert.AreEqual(4, messages[2]);
        Assert.AreEqual(1, messages[3]);
        Assert.AreEqual(2, messages[4]);

        CloseMailbox(session);
      }
    }

    [Test]
    public void TestSortWithLiteralAndDefaultCharset()
    {
      using (var session = SelectMailbox("SORT")) {
        // SORT transaction
        server.EnqueueResponse("+ continue\r\n");
        server.EnqueueResponse("* SORT 5 3 4 1 2\r\n" +
                               "0004 OK SORT completed\r\n");

        ImapMatchedSequenceSet matched;

        Assert.IsTrue((bool)session.Sort(ImapSortCriteria.SizeReverse + ImapSortCriteria.Date,
                                         ImapSearchCriteria.Subject("件名"),
                                         out matched));

        Assert.AreEqual("0004 SORT (REVERSE SIZE DATE) utf-8 SUBJECT {6}\r\n" +
                        "件名\r\n",
                        server.DequeueAll());

        Assert.IsNotNull(matched);
        Assert.IsFalse(matched.IsUidSet);
        Assert.IsFalse(matched.IsEmpty);

        CloseMailbox(session);
      }
    }

    [Test]
    public void TestSortWithLiteralAndSpecifiedCharset()
    {
      using (var session = SelectMailbox("SORT")) {
        // SORT transaction
        server.EnqueueResponse("+ continue\r\n");
        server.EnqueueResponse("* SORT 5 3 4 1 2\r\n" +
                               "0004 OK SORT completed\r\n");

        ImapMatchedSequenceSet matched;

        Assert.IsTrue((bool)session.Sort(ImapSortCriteria.SizeReverse + ImapSortCriteria.Date,
                                         ImapSearchCriteria.Subject("件名"),
                                         Encoding.GetEncoding(932),
                                         out matched));

        Assert.AreEqual("0004 SORT (REVERSE SIZE DATE) shift_jis SUBJECT {4}\r\n" +
                        "\x008c\x008f\x0096\x00bc\r\n",
                        server.DequeueAll(NetworkTransferEncoding.Transfer8Bit));

        Assert.IsNotNull(matched);
        Assert.IsFalse(matched.IsUidSet);
        Assert.IsFalse(matched.IsEmpty);

        CloseMailbox(session);
      }
    }

    [Test, ExpectedException(typeof(ImapIncapableException))]
    public void TestSortIncapable()
    {
      using (var session = SelectMailbox()) {
        ImapMatchedSequenceSet matched;

        session.Sort(ImapSortCriteria.SizeReverse + ImapSortCriteria.Date, ImapSearchCriteria.All, out matched);
      }
    }

    [Test]
    public void TestESort()
    {
      using (var session = SelectMailbox("SORT", "ESORT")) {
        // SORT transaction
        server.EnqueueResponse("* ESEARCH (TAG \"0004\") UID ALL 23765,23764,23763,23761\r\n" +
                               "0004 OK SORT completed\r\n");

        ImapMatchedSequenceSet matched;

        Assert.IsTrue((bool)session.UidESort(ImapSortCriteria.DateReverse,
                                       ImapSearchCriteria.Undeleted & ImapSearchCriteria.Unkeyword("$Junk"),
                                       ImapSearchResultOptions.All,
                                       out matched));

        Assert.AreEqual("0004 UID SORT RETURN (ALL) (REVERSE DATE) utf-8 UNDELETED UNKEYWORD $Junk\r\n",
                        server.DequeueRequest());

        Assert.IsTrue(matched.IsUidSet);
        Assert.AreEqual("0004", matched.Tag);

        var messages = matched.ToArray();

        Assert.AreEqual(4, messages.Length);
        Assert.AreEqual(23765, messages[0]);
        Assert.AreEqual(23764, messages[1]);
        Assert.AreEqual(23763, messages[2]);
        Assert.AreEqual(23761, messages[3]);

        CloseMailbox(session);
      }
    }

    [Test, ExpectedException(typeof(ImapIncapableException))]
    public void TestESortIncapable()
    {
      using (var session = SelectMailbox("SORT")) {
        // SORT transaction
        ImapMatchedSequenceSet matched;

        Assert.IsTrue((bool)session.UidESort(ImapSortCriteria.DateReverse,
                                       ImapSearchCriteria.Undeleted & ImapSearchCriteria.Unkeyword("$Junk"),
                                       ImapSearchResultOptions.All,
                                       out matched));
      }
    }

    [Test]
    public void TestSortDisplayIncapable()
    {
      using (var session = SelectMailbox("SORT")) {
        ImapMatchedSequenceSet matched;

        try {
          session.Sort(ImapSortCriteria.Date + ImapSortCriteria.DisplayFrom, ImapSearchCriteria.All, out matched);
          Assert.Fail("ImapIncapableException not thrown");
        }
        catch (ImapIncapableException ex) {
          Assert.AreEqual(ImapCapability.SortDisplay, ex.RequiredCapability);
        }
      }
    }

    [Test]
    public void TestThread()
    {
      using (var session = SelectMailbox("THREAD=REFERENCES")) {
        // THREAD transaction
        server.EnqueueResponse("* THREAD (166)(167)(168)(169)(172)(170)(171)(173)(174 (175)(176)(178)(181)(180))(179)(177 (183)(182)(188)(184)(185)(186)(187)(189))(190)(191)(192)(193)(194 195)(196 (197)(198))(199)(200 202)(201)(203)(204)(205)(206 207)(208)\r\n" +
                               "0004 OK THREAD completed\r\n");

        ImapThreadList threadList;

        Assert.IsTrue((bool)session.Thread(ImapThreadingAlgorithm.References, ImapSearchCriteria.All, out threadList));

        Assert.AreEqual("0004 THREAD REFERENCES utf-8 ALL\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(25, threadList.Children.Length);

        CloseMailbox(session);
      }
    }

    [Test]
    public void TestThreadWithLiteralAndDefaultCharset()
    {
      using (var session = SelectMailbox("THREAD=REFERENCES")) {
        // THREAD transaction
        server.EnqueueResponse("+ continue\r\n");
        server.EnqueueResponse("* THREAD (206 207)(208)\r\n" +
                               "0004 OK THREAD completed\r\n");

        ImapThreadList threadList;

        Assert.IsTrue((bool)session.Thread(ImapThreadingAlgorithm.References,
                                           ImapSearchCriteria.Subject("件名"),
                                           out threadList));

        Assert.AreEqual("0004 THREAD REFERENCES utf-8 SUBJECT {6}\r\n" +
                        "件名\r\n",
                        server.DequeueAll());

        Assert.IsNotNull(threadList);

        CloseMailbox(session);
      }
    }

    [Test]
    public void TestThreadWithLiteralAndSpecifiedCharset()
    {
      using (var session = SelectMailbox("THREAD=REFERENCES")) {
        // THREAD transaction
        server.EnqueueResponse("+ continue\r\n");
        server.EnqueueResponse("* THREAD (206 207)(208)\r\n" +
                               "0004 OK THREAD completed\r\n");

        ImapThreadList threadList;

        Assert.IsTrue((bool)session.Thread(ImapThreadingAlgorithm.References,
                                           ImapSearchCriteria.Subject("件名"),
                                           Encoding.GetEncoding(932),
                                           out threadList));

        Assert.AreEqual("0004 THREAD REFERENCES shift_jis SUBJECT {4}\r\n" +
                        "\x008c\x008f\x0096\x00bc\r\n",
                        server.DequeueAll(NetworkTransferEncoding.Transfer8Bit));

        Assert.IsNotNull(threadList);

        CloseMailbox(session);
      }
    }

    [Test, ExpectedException(typeof(ImapIncapableException))]
    public void TestThreadIncapable()
    {
      using (var session = SelectMailbox("THREAD=ORDEREDSUBJECT")) {
        ImapThreadList threadList;

        session.Thread(ImapThreadingAlgorithm.References, ImapSearchCriteria.All, out threadList);
      }
    }

    [Test]
    public void TestStore()
    {
      using (var session = SelectMailbox()) {
        // STORE
        server.EnqueueResponse("* 2 FETCH (FLAGS (\\Deleted \\Seen))\r\n" +
                               "* 3 FETCH (FLAGS (\\Deleted))\r\n" +
                               "* 4 FETCH (FLAGS (\\Deleted \\Flagged \\Seen))\r\n" +
                               "0004 OK STORE completed\r\n");

        ImapMessageAttribute[] messagesAttrs;

        Assert.IsTrue((bool)session.Store(ImapSequenceSet.CreateRangeSet(2, 4),
                                    ImapStoreDataItem.AddFlags(ImapMessageFlag.Deleted),
                                    out messagesAttrs));

        Assert.AreEqual("0004 STORE 2:4 +FLAGS (\\Deleted)\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(3, messagesAttrs.Length);

        Assert.AreEqual(2, messagesAttrs[0].Sequence);
        Assert.AreEqual(0UL, messagesAttrs[0].ModSeq);
        Assert.AreEqual(2, messagesAttrs[0].Flags.Count);
        Assert.IsTrue(messagesAttrs[0].Flags.Has(ImapMessageFlag.Deleted));
        Assert.IsTrue(messagesAttrs[0].Flags.Has(ImapMessageFlag.Seen));

        Assert.AreEqual(3, messagesAttrs[1].Sequence);
        Assert.AreEqual(0UL, messagesAttrs[1].ModSeq);
        Assert.AreEqual(1, messagesAttrs[1].Flags.Count);
        Assert.IsTrue(messagesAttrs[1].Flags.Has(ImapMessageFlag.Deleted));

        Assert.AreEqual(4, messagesAttrs[2].Sequence);
        Assert.AreEqual(0UL, messagesAttrs[2].ModSeq);
        Assert.AreEqual(3, messagesAttrs[2].Flags.Count);
        Assert.IsTrue(messagesAttrs[2].Flags.Has(ImapMessageFlag.Deleted));
        Assert.IsTrue(messagesAttrs[2].Flags.Has(ImapMessageFlag.Flagged));
        Assert.IsTrue(messagesAttrs[2].Flags.Has(ImapMessageFlag.Seen));

        CloseMailbox(session);
      }
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestStoreEmptySequenceSet()
    {
      using (var session = SelectMailbox()) {
        session.Store(ImapSequenceSet.CreateSet(new long[] {}), ImapStoreDataItem.AddFlags(ImapMessageFlag.Deleted));
      }
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestStoreEmptyUidSet()
    {
      using (var session = SelectMailbox()) {
        session.Store(ImapSequenceSet.CreateUidSet(new long[] {}), ImapStoreDataItem.AddFlags(ImapMessageFlag.Deleted));
      }
    }

    [Test]
    public void TestStoreUnchangedSince()
    {
      using (var session = SelectMailbox("CONDSTORE")) {
        // STORE
        server.EnqueueResponse("* 7 FETCH (MODSEQ (320162342) FLAGS (\\Seen \\Deleted))\r\n" +
                               "* 5 FETCH (MODSEQ (320162350))\r\n" +
                               "* 9 FETCH (MODSEQ (320162349) FLAGS (\\Answered))\r\n" +
                               "0004 OK [MODIFIED 7,9] Conditional STORE failed\r\n");

        ImapMessageAttribute[] messagesAttrs;
        ImapSequenceSet failedMessageSet;
  
        Assert.IsTrue((bool)session.StoreUnchangedSince(ImapSequenceSet.CreateSet(7, 5, 9),
                                                  ImapStoreDataItem.AddFlagsSilent(ImapMessageFlag.Deleted),
                                                  320162338UL,
                                                  out messagesAttrs,
                                                  out failedMessageSet));
  
        Assert.AreEqual("0004 STORE 7,5,9 (UNCHANGEDSINCE 320162338) +FLAGS.SILENT (\\Deleted)\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(3, messagesAttrs.Length);

        Assert.AreEqual(7, messagesAttrs[0].Sequence);
        Assert.AreEqual(320162342UL, messagesAttrs[0].ModSeq);
        Assert.AreEqual(2, messagesAttrs[0].Flags.Count);
        Assert.IsTrue(messagesAttrs[0].Flags.Has(ImapMessageFlag.Seen));
        Assert.IsTrue(messagesAttrs[0].Flags.Has(ImapMessageFlag.Deleted));

        Assert.AreEqual(5, messagesAttrs[1].Sequence);
        Assert.AreEqual(320162350UL, messagesAttrs[1].ModSeq);
        Assert.AreEqual(0, messagesAttrs[1].Flags.Count);

        Assert.AreEqual(9, messagesAttrs[2].Sequence);
        Assert.AreEqual(320162349UL, messagesAttrs[2].ModSeq);
        Assert.AreEqual(1, messagesAttrs[2].Flags.Count);
        Assert.IsTrue(messagesAttrs[2].Flags.Has(ImapMessageFlag.Answered));

        var failed = failedMessageSet.ToArray();

        Assert.AreEqual(2, failed.Length);
        Assert.AreEqual(7, failed[0]);
        Assert.AreEqual(9, failed[1]);

        CloseMailbox(session);
      }
    }

    [Test]
    public void TestStoreUnchangedSinceWithTaggedNoResponse()
    {
      using (var session = SelectMailbox("CONDSTORE")) {
        // STORE
        server.EnqueueResponse("* 1 FETCH (MODSEQ (320172342) FLAGS (\\SEEN))\r\n" +
                               "* 3 FETCH (MODSEQ (320172342) FLAGS (\\SEEN))\r\n" +
                               "0004 NO [MODIFIED 2] Some of the messages no longer exist.\r\n");

        ImapMessageAttribute[] messagesAttrs;
        ImapSequenceSet failedMessageSet;

        Assert.IsFalse((bool)session.StoreUnchangedSince(ImapSequenceSet.CreateRangeSet(1, 7),
                                                   ImapStoreDataItem.AddFlags(ImapMessageFlag.Seen),
                                                   320172338UL,
                                                   out messagesAttrs,
                                                   out failedMessageSet));

        Assert.AreEqual("0004 STORE 1:7 (UNCHANGEDSINCE 320172338) +FLAGS (\\Seen)\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(messagesAttrs);
        Assert.AreEqual(2, messagesAttrs.Length);

        Assert.AreEqual(1, messagesAttrs[0].Sequence);
        Assert.AreEqual(320172342UL, messagesAttrs[0].ModSeq);
        Assert.AreEqual(1, messagesAttrs[0].Flags.Count);
        Assert.IsTrue(messagesAttrs[0].Flags.Has(ImapMessageFlag.Seen));

        Assert.AreEqual(3, messagesAttrs[1].Sequence);
        Assert.AreEqual(320172342UL, messagesAttrs[1].ModSeq);
        Assert.AreEqual(1, messagesAttrs[1].Flags.Count);
        Assert.IsTrue(messagesAttrs[1].Flags.Has(ImapMessageFlag.Seen));

        var failed = failedMessageSet.ToArray();

        Assert.AreEqual(1, failed.Length);
        Assert.AreEqual(2, failed[0]);

        CloseMailbox(session);
      }
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestStoreUnchangedSinceIncapable()
    {
      using (var session = SelectMailbox()) {
        // STORE
        ImapMessageAttribute[] messagesAttrs;
        ImapSequenceSet failedMessageSet;
        session.StoreUnchangedSince(ImapSequenceSet.CreateRangeSet(1, 7),
                                    ImapStoreDataItem.AddFlags(ImapMessageFlag.Seen),
                                    320172338UL,
                                    out messagesAttrs,
                                    out failedMessageSet);
      }
    }

    [Test]
    public void TestExpunge()
    {
      using (var session = SelectMailbox()) {
        // EXPUNGE transaction
        server.EnqueueResponse("* 3 EXPUNGE\r\n" +
                               "* 4 EXPUNGE\r\n" +
                               "* 7 EXPUNGE\r\n" +
                               "* 11 EXPUNGE\r\n" +
                               "0004 OK EXPUNGE completed\r\n");
  
        long[] expunged;
  
        Assert.IsTrue((bool)session.Expunge(out expunged));
  
        Assert.AreEqual("0004 EXPUNGE\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(4, expunged.Length);
        Assert.AreEqual(3,  expunged[0]);
        Assert.AreEqual(4,  expunged[1]);
        Assert.AreEqual(7,  expunged[2]);
        Assert.AreEqual(11, expunged[3]);
  
        CloseMailbox(session);
      }
    }

    [Test]
    public void TestUidExpunge()
    {
      using (var session = SelectMailbox("UIDPLUS")) {
        // EXPUNGE transaction
        server.EnqueueResponse("* 3 EXPUNGE\r\n" +
                               "* 3 EXPUNGE\r\n" +
                               "* 3 EXPUNGE\r\n" +
                               "0004 OK EXPUNGE completed\r\n");
  
        long[] expunged;
  
        Assert.IsTrue((bool)session.UidExpunge(ImapSequenceSet.CreateUidFromSet(1), out expunged));
  
        Assert.AreEqual("0004 UID EXPUNGE 1:*\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(3, expunged.Length);
        Assert.AreEqual(3,  expunged[0]);
        Assert.AreEqual(3,  expunged[1]);
        Assert.AreEqual(3,  expunged[2]);
  
        CloseMailbox(session);
      }
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestExpungeSequenceSet()
    {
      using (var session = SelectMailbox("UIDPLUS")) {
        long[] expunged;

        session.UidExpunge(ImapSequenceSet.CreateFromSet(1), out expunged);
      }
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestUidExpungeEmptyUidSet()
    {
      using (var session = SelectMailbox("UIDPLUS")) {
        long[] expunged;

        session.UidExpunge(ImapSequenceSet.CreateUidSet(new long[] {}), out expunged);
      }
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestUidExpungeIncapable()
    {
      using (var session = SelectMailbox()) {
        Assert.IsFalse(session.ServerCapabilities.Has(ImapCapability.UidPlus));

        long[] expunged;

        session.UidExpunge(ImapSequenceSet.CreateUidFromSet(1), out expunged);
      }
    }

    [Test]
    public void TestCopy()
    {
      using (var session = SelectMailbox()) {
        // COPY transaction
        server.EnqueueResponse("0004 OK COPY completed\r\n");
  
        Assert.IsTrue((bool)session.Copy(ImapSequenceSet.CreateRangeSet(1, 10), "INBOX"));
  
        Assert.AreEqual("0004 COPY 1:10 \"INBOX\"\r\n",
                        server.DequeueRequest());
  
        CloseMailbox(session);
      }
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestCopyEmptySequenceSet()
    {
      using (var session = SelectMailbox()) {
        session.Copy(ImapSequenceSet.CreateSet(new long[] {}), "INBOX");
      }
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestCopyEmptyUidSet()
    {
      using (var session = SelectMailbox()) {
        session.Copy(ImapSequenceSet.CreateUidSet(new long[] {}), "INBOX");
      }
    }

    [Test]
    public void TestCopyTryCreate()
    {
      using (var session = SelectMailbox()) {
        // COPY transaction
        server.EnqueueResponse("0004 NO [TRYCREATE] Mailbox doesn't exist: INBOX.copyto\r\n");
        server.EnqueueResponse("0005 OK CREATE completed\r\n");
        server.EnqueueResponse("0006 OK COPY completed\r\n");
  
        ImapMailbox created;
  
        Assert.IsTrue((bool)session.Copy(ImapSequenceSet.CreateUidRangeSet(1, 10), "INBOX.copyto", out created));
  
        Assert.AreEqual("INBOX.copyto", created.Name);
        Assert.AreEqual(new Uri(uri, "./INBOX.copyto"), created.Url);
        Assert.IsNotNull(created.Flags);
        Assert.IsNotNull(created.ApplicableFlags);
        Assert.IsNotNull(created.PermanentFlags);

        Assert.AreEqual("0004 UID COPY 1:10 \"INBOX.copyto\"\r\n",
                        server.DequeueRequest());
        Assert.AreEqual("0005 CREATE \"INBOX.copyto\"\r\n",
                        server.DequeueRequest());
        Assert.AreEqual("0006 UID COPY 1:10 \"INBOX.copyto\"\r\n",
                        server.DequeueRequest());
  
        CloseMailbox(session, "0007");
      }
    }

    [Test]
    public void TestCopyTryCreateNoReferralResponseCode()
    {
      using (var session = SelectMailbox()) {
        // COPY transaction
        server.EnqueueResponse("0004 NO [REFERRAL IMAP://user;AUTH=*@SERVER2/SHARED/STUFF] Unable to copy message(s) to SERVER2.\r\n");

        ImapMailbox created;

        Assert.IsFalse((bool)session.Copy(ImapSequenceSet.CreateUidRangeSet(1, 10), "SHARED/STUFF", out created));

        Assert.IsNull(created);

        Assert.AreEqual("0004 UID COPY 1:10 \"SHARED/STUFF\"\r\n",
                        server.DequeueRequest());
      }
    }

    [Test]
    public void TestCopyDontTryCreate()
    {
      using (var session = SelectMailbox()) {
        // COPY transaction
        server.EnqueueResponse("0004 NO [TRYCREATE] Mailbox doesn't exist: INBOX.copyto\r\n");
  
        Assert.IsFalse((bool)session.Copy(ImapSequenceSet.CreateRangeSet(1, 10), "INBOX.copyto"));
  
        Assert.AreEqual("0004 COPY 1:10 \"INBOX.copyto\"\r\n",
                        server.DequeueRequest());
  
        CloseMailbox(session);
      }
    }

    [Test]
    public void TestCopyWithAppendUidResponseCode()
    {
      using (var session = SelectMailbox()) {
        // COPY transaction
        server.EnqueueResponse("0004 OK [COPYUID 38505 304,319:320 3956:3958] Done\r\n");
  
        ImapCopiedUidSet copied;

        Assert.IsTrue((bool)session.Copy(ImapSequenceSet.CreateRangeSet(2, 4), "meeting", out copied));
  
        Assert.AreEqual("0004 COPY 2:4 \"meeting\"\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(copied);
        Assert.AreEqual(38505, copied.UidValidity);

        var copiedUids = copied.CopiedUidSet.ToArray();

        Assert.AreEqual(3, copiedUids.Length);
        Assert.AreEqual(304, copiedUids[0]);
        Assert.AreEqual(319, copiedUids[1]);
        Assert.AreEqual(320, copiedUids[2]);

        var assignedUids = copied.AssignedUidSet.ToArray();

        Assert.AreEqual(3, assignedUids.Length);
        Assert.AreEqual(3956, assignedUids[0]);
        Assert.AreEqual(3957, assignedUids[1]);
        Assert.AreEqual(3958, assignedUids[2]);
  
        CloseMailbox(session);
      }
    }
  }
}
