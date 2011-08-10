using System;
using System.Text;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapSearchCriteriaTest {
    private static void TraverseCriteriaAssertion(ImapSearchCriteria criteria, params ImapString[] expected)
    {
      var index = 0;

      criteria.Traverse(delegate(ImapString val) {
        Assert.Less(index, expected.Length);
        Assert.IsInstanceOfType(expected[index].GetType(), val);
        Assert.AreEqual(expected[index], val);
        index++;
      });
    }

    [Test]
    public void TestCreateDateCriteria()
    {
      Assert.AreEqual("BEFORE 25-Feb-2008", ImapSearchCriteria.Before(new DateTime(2008, 2, 25)).ToString());
      Assert.AreEqual("ON 25-Feb-2008", ImapSearchCriteria.On(new DateTime(2008, 2, 25)).ToString());
      Assert.AreEqual("SINCE 25-Feb-2008", ImapSearchCriteria.Since(new DateTime(2008, 2, 25)).ToString());

      Assert.AreEqual("SENTBEFORE 25-Feb-2008", ImapSearchCriteria.SentBefore(new DateTime(2008, 2, 25)).ToString());
      Assert.AreEqual("SENTON 25-Feb-2008", ImapSearchCriteria.SentOn(new DateTime(2008, 2, 25)).ToString());
      Assert.AreEqual("SENTSINCE 25-Feb-2008", ImapSearchCriteria.SentSince(new DateTime(2008, 2, 25)).ToString());
    }

    [Test]
    public void TestCreateKeywordCriteria()
    {
      Assert.AreEqual("KEYWORD \\Answered", ImapSearchCriteria.Keyword(ImapMessageFlag.Answered).ToString());
      Assert.AreEqual("UNKEYWORD \\Answered", ImapSearchCriteria.Unkeyword(ImapMessageFlag.Answered).ToString());

      Assert.AreEqual("KEYWORD $label1", ImapSearchCriteria.Keyword("$label1").ToString());
      Assert.AreEqual("UNKEYWORD $label1", ImapSearchCriteria.Unkeyword("$label1").ToString());

      for (var i = 0; i < 4; i++) {
        try {
          switch (i) {
            case 0: ImapSearchCriteria.Keyword((ImapMessageFlag)null); break;
            case 1: ImapSearchCriteria.Unkeyword((ImapMessageFlag)null); break;
            case 2: ImapSearchCriteria.Keyword((string)null); break;
            case 3: ImapSearchCriteria.Unkeyword((string)null); break;
          }

          Assert.Fail("ArgumentNullException not thrown (#{0})", i);
        }
        catch (ArgumentNullException) {
        }
      }

      for (var i = 0; i < 4; i++) {
        try {
          switch (i) {
            case 0: ImapSearchCriteria.Keyword(string.Empty); break;
            case 1: ImapSearchCriteria.Unkeyword(string.Empty); break;
            case 2: ImapSearchCriteria.Keyword("(invalid keyword)"); break;
            case 3: ImapSearchCriteria.Unkeyword("(invalid keyword)"); break;
          }

          Assert.Fail("ArgumentException not thrown (#{0})", i);
        }
        catch (ArgumentException) {
        }
      }
    }

    [Test]
    public void TestCreateSizeCriteria()
    {
      Assert.AreEqual("LARGER 72", ImapSearchCriteria.Larger(72).ToString());
      Assert.AreEqual("SMALLER 72", ImapSearchCriteria.Smaller(72).ToString());

      try {
        ImapSearchCriteria.Larger(-1);
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }

      try {
        ImapSearchCriteria.Smaller(-1);
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }
    }

    private delegate ImapSearchCriteria CreateCriteriaMethod(string val);

    [Test]
    public void TestCreateStringCriteria()
    {
      foreach (var test in new[] {
        new {Criterion = "BCC",     Method = (CreateCriteriaMethod)ImapSearchCriteria.Bcc},
        new {Criterion = "BODY",    Method = (CreateCriteriaMethod)ImapSearchCriteria.Body},
        new {Criterion = "CC",      Method = (CreateCriteriaMethod)ImapSearchCriteria.Cc},
        new {Criterion = "FROM",    Method = (CreateCriteriaMethod)ImapSearchCriteria.From},
        new {Criterion = "SUBJECT", Method = (CreateCriteriaMethod)ImapSearchCriteria.Subject},
        new {Criterion = "TEXT",    Method = (CreateCriteriaMethod)ImapSearchCriteria.Text},
        new {Criterion = "TO",      Method = (CreateCriteriaMethod)ImapSearchCriteria.To},
      }) {
        var criteria = test.Method("テキスト");

        StringAssert.StartsWith(test.Criterion, criteria.ToString());

        TraverseCriteriaAssertion(criteria,
                                  new ImapString(test.Criterion),
                                  new ImapLiteralString("テキスト"));

        try {
          test.Method(null);
          Assert.Fail("ArgumentNullException not thrown");
        }
        catch (ArgumentNullException) {
        }
      }
    }

    [Test]
    public void TestHeader()
    {
      TraverseCriteriaAssertion(ImapSearchCriteria.Header("Message-ID", "xxxx"),
                                new ImapString("HEADER"),
                                new ImapQuotedString("Message-ID"),
                                new ImapLiteralString("xxxx"));
    }

    [Test, ExpectedException(typeof(ArgumentNullException))]
    public void TestHeaderFieldNameNull()
    {
      ImapSearchCriteria.Header(null, "value");
    }

    [Test, ExpectedException(typeof(ArgumentNullException))]
    public void TestHeaderValueNull()
    {
      ImapSearchCriteria.Header("field name", null);
    }

    [Test]
    public void TestSequenceSet()
    {
      var criteria = ImapSearchCriteria.SequenceSet(ImapSequenceSet.CreateSet(1, 4, 5));

      Assert.AreEqual("1,4,5", criteria.ToString());

      try {
        ImapSearchCriteria.SequenceSet(ImapSequenceSet.CreateUidSet(1, 4, 5));
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }

      try {
        ImapSearchCriteria.SequenceSet(ImapSequenceSet.CreateSet(new long[0]));
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }

      try {
        ImapSearchCriteria.SequenceSet(null);
        Assert.Fail("ArgumentNullException not thrown");
      }
      catch (ArgumentNullException) {
      }
    }

    [Test]
    public void TestUid()
    {
      var criteria = ImapSearchCriteria.Uid(ImapSequenceSet.CreateUidSet(1, 4, 5));

      Assert.AreEqual("UID 1,4,5", criteria.ToString());

      try {
        ImapSearchCriteria.Uid(ImapSequenceSet.CreateSet(1, 4, 5));
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }

      try {
        ImapSearchCriteria.Uid(ImapSequenceSet.CreateUidSet(new long[0]));
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }

      try {
        ImapSearchCriteria.Uid(null);
        Assert.Fail("ArgumentNullException not thrown");
      }
      catch (ArgumentNullException) {
      }
    }

    [Test]
    public void TestSequenceSetFromSavedResult()
    {
      var matched = ImapMatchedSequenceSet.CreateSavedResult(ImapMatchedSequenceSet.CreateEmpty(true));

      Assert.AreEqual("$", matched.ToString());
      Assert.AreEqual("$", ImapSearchCriteria.SequenceSet(matched).ToString());
    }

    [Test]
    public void TestUidSetFromSavedResult()
    {
      var matched = ImapMatchedSequenceSet.CreateSavedResult(ImapMatchedSequenceSet.CreateEmpty(false));

      Assert.AreEqual("$", matched.ToString());
      Assert.AreEqual("UID $", ImapSearchCriteria.Uid(matched).ToString());
    }

    [Test]
    public void TestSequenceOrUidSet()
    {
      Assert.AreEqual("1,4,5", ImapSearchCriteria.SequenceOrUidSet(ImapSequenceSet.CreateSet(1, 4, 5)).ToString());
      Assert.AreEqual("UID 1,4,5", ImapSearchCriteria.SequenceOrUidSet(ImapSequenceSet.CreateUidSet(1, 4, 5)).ToString());

      try {
        ImapSearchCriteria.SequenceOrUidSet(ImapSequenceSet.CreateSet(new long[0]));
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }

      try {
        ImapSearchCriteria.SequenceOrUidSet(ImapSequenceSet.CreateUidSet(new long[0]));
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }

      try {
        ImapSearchCriteria.SequenceOrUidSet(null);
        Assert.Fail("ArgumentNullException not thrown");
      }
      catch (ArgumentNullException) {
      }
    }

    [Test]
    public void TestAnd1()
    {
      var criteria1 = ImapSearchCriteria.SentBefore(new DateTime(2010, 01, 30));
      var criteria2 = ImapSearchCriteria.Recent;
      var expected = "SENTBEFORE 30-Jan-2010 RECENT";

      Assert.AreEqual(expected, (criteria1 & criteria2).ToString());
      Assert.AreEqual(expected, criteria1.And(criteria2).ToString());
      Assert.AreEqual(expected, ImapSearchCriteria.And(criteria1, criteria2).ToString());
    }

    [Test]
    public void TestAnd2()
    {
      var criteria1 = ImapSearchCriteria.SentBefore(new DateTime(2010, 01, 30));
      var criteria2 = ImapSearchCriteria.Keyword("label1");

      Assert.AreEqual("SENTBEFORE 30-Jan-2010 KEYWORD label1", (criteria1 & criteria2).ToString());
      Assert.AreEqual("KEYWORD label1 SENTBEFORE 30-Jan-2010", (criteria2 & criteria1).ToString());
    }

    [Test]
    public void TestAnd3()
    {
      var criteria = ImapSearchCriteria.Flagged & ImapSearchCriteria.Undeleted;

      Assert.AreEqual("FLAGGED UNDELETED", criteria.ToString());
    }

    [Test]
    public void TestOr1()
    {
      var criteria1 = ImapSearchCriteria.Recent;
      var criteria2 = ImapSearchCriteria.Keyword("label1");
      var expected = "OR (RECENT) (KEYWORD label1)";

      Assert.AreEqual(expected, (criteria1 | criteria2).ToString());
      Assert.AreEqual(expected, criteria1.Or(criteria2).ToString());
      Assert.AreEqual(expected, ImapSearchCriteria.Or(criteria1, criteria2).ToString());
    }

    [Test]
    public void TestOr2()
    {
      var criteria1 = ImapSearchCriteria.SentBefore(new DateTime(2010, 01, 30)) & ImapSearchCriteria.Keyword("label1");
      var criteria2 = ImapSearchCriteria.Recent;

      Assert.AreEqual("OR (SENTBEFORE 30-Jan-2010 KEYWORD label1) (RECENT)", (criteria1 | criteria2).ToString());
      Assert.AreEqual("OR (RECENT) (SENTBEFORE 30-Jan-2010 KEYWORD label1)", (criteria2 | criteria1).ToString());
    }

    [Test]
    public void TestNot1()
    {
      var criteria = ImapSearchCriteria.Recent;
      var expected = "NOT (RECENT)";

      Assert.AreEqual(expected, (!criteria).ToString());
      Assert.AreEqual(expected, criteria.Not().ToString());
      Assert.AreEqual(expected, ImapSearchCriteria.Not(criteria).ToString());
    }

    [Test]
    public void TestAndOrNot1()
    {
      var criteria1 = ImapSearchCriteria.SentBefore(new DateTime(2010, 01, 30)) & ImapSearchCriteria.Keyword("label1");
      var criteria2 = ImapSearchCriteria.Recent;
      var criteria = !(criteria1 | criteria2);

      Assert.AreEqual("NOT (OR (SENTBEFORE 30-Jan-2010 KEYWORD label1) (RECENT))", criteria.ToString());
    }

    [Test]
    public void TestAndOrNot2()
    {
      var criteria1 = ImapSearchCriteria.SentBefore(new DateTime(2010, 01, 30)) | ImapSearchCriteria.Keyword("label1");
      var criteria2 = ImapSearchCriteria.Recent;
      var criteria = criteria1 & !criteria2;

      Assert.AreEqual("OR (SENTBEFORE 30-Jan-2010) (KEYWORD label1) NOT (RECENT)", criteria.ToString());
    }

    [Test]
    public void TestCombineWithContainsRequiredCapability()
    {
      var criteria1 = ImapSearchCriteria.ModSeq(720162338UL);
      var criteria2 = ImapSearchCriteria.Larger(50000);

      var criteria = !criteria1 | criteria2;

      Assert.AreEqual("OR (NOT (MODSEQ 720162338)) (LARGER 50000)", criteria.ToString());

      var caps = (criteria as IImapExtension).RequiredCapabilities;

      CollectionAssert.AreEquivalent(new[] {ImapCapability.CondStore}, caps);
    }

    [Test]
    public void TestSearchInThreadAndThreadRefs()
    {
      var criteria = ImapSearchCriteria.InThread & ImapSearchCriteria.MessageId("4321.1234321@example.com");

      Assert.AreEqual("INTHREAD MESSAGEID <4321.1234321@example.com>", criteria.ToString());

      var caps = (criteria as IImapExtension).RequiredCapabilities;

      CollectionAssert.AreEquivalent(new[] {ImapCapability.SearchInThread, ImapCapability.ThreadRefs},
                                     caps);
    }

    [Test]
    public void TestCreateWithinCriteria()
    {
      Assert.AreEqual("YOUNGER 17", ImapSearchCriteria.Younger(17L).ToString());
      Assert.AreEqual("OLDER 17", ImapSearchCriteria.Older(17L).ToString());

      Assert.AreEqual("YOUNGER 3600", ImapSearchCriteria.Younger(TimeSpan.FromHours(1.0)).ToString());
      Assert.AreEqual("OLDER 3600", ImapSearchCriteria.Older(TimeSpan.FromHours(1.0)).ToString());

      try {
        ImapSearchCriteria.Younger(0L);
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }

      try {
        ImapSearchCriteria.Older(0L);
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }
    }

    [Test]
    public void TestFromUri1()
    {
      //   imap://<iserver>/<enc_mailbox>[uidvalidity][?<enc_search>]
      var uri = new Uri("imap://;AUTH=*@minbari.org/gray%20council?SUBJECT%20shadows");
      var criteria = ImapSearchCriteria.FromUri(uri);

      Assert.AreEqual("gray council", ImapStyleUriParser.GetMailbox(uri));
      Assert.AreEqual("?SUBJECT%20shadows", uri.Query);
      Assert.AreEqual(ImapUriForm.SearchMessages, ImapStyleUriParser.GetUriForm(uri));

      Assert.AreEqual("SUBJECT shadows", criteria.ToString());
    }

    [Test]
    public void TestFromUri2()
    {
      var uri = new Uri("imap://john;AUTH=*@minbari.example.org/babylon5/personel?charset%20UTF-8%20SUBJECT%20%7B14+%7D%0D%0A%D0%98%D0%B2%D0%B0%D0%BD%D0%BE%D0%B2%D0%B0");
      var criteria = ImapSearchCriteria.FromUri(uri);

      Assert.AreEqual(ImapUriForm.SearchMessages, ImapStyleUriParser.GetUriForm(uri));
      Assert.AreEqual("charset UTF-8 SUBJECT {14+}\r\n\xD0\x98\xD0\xB2\xD0\xB0\xD0\xBD\xD0\xBE\xD0\xB2\xD0\xB0", criteria.ToString());
    }

    [Test]
    public void TestFromUriQueryNotPresent()
    {
      var uri = new Uri("imap://localhost/INBOX");
      var criteria = ImapSearchCriteria.FromUri(uri);

      Assert.AreEqual(ImapUriForm.ListMessages, ImapStyleUriParser.GetUriForm(uri));
      Assert.IsNull(criteria);
    }

    [Test]
    public void TestFromUriEmptyQuery()
    {
      var uri = new Uri("imap://localhost/INBOX?");
      var criteria = ImapSearchCriteria.FromUri(uri);

      Assert.AreEqual(ImapUriForm.SearchMessages, ImapStyleUriParser.GetUriForm(uri));
      Assert.IsNotNull(criteria);
      Assert.AreEqual(string.Empty, criteria.ToString());
    }

    [Test]
    public void TestFromUriConvertToNonSynchronized()
    {
      var baseUri = new Uri("imap://localhost/INBOX");

      foreach (var test in new[] {
        new {Expected = "SUBJECT {3+}\r\nabc",   Input = "SUBJECT {3}\r\nabc"},
        new {Expected = "SUBJECT {3+}\r\nabc",   Input = "SUBJECT {3+}\r\nabc"},
        new {Expected = "FROM {3+}\r\nabc SUBJECT \"{3}\"",   Input = "FROM {3}\r\nabc SUBJECT \"{3}\""},
        new {Expected = "FROM {3+}\r\nabc SUBJECT \"{3}\"",   Input = "FROM {3+}\r\nabc SUBJECT \"{3}\""},
      }) {
        try {
          var criteria = ImapSearchCriteria.FromUri(new Uri(baseUri, "?" + test.Input));

          Assert.IsNotNull(criteria);
          Assert.AreEqual(test.Expected, criteria.ToString());
        }
        catch (ArgumentException ex) {
          Assert.Fail("ArgumentException thrown: {0} {1}", test.Input, ex);
        }
      }
    }

    [Test, Ignore("disabled feature")]
    public void TestFromUriConvertToSynchronized()
    {
      var baseUri = new Uri("imap://localhost/INBOX");

      foreach (var test in new[] {
        new {Expected = "SUBJECT {3}\r\nabc",   Input = "SUBJECT {3}\r\nabc"},
        new {Expected = "SUBJECT {3}\r\nabc",   Input = "SUBJECT {3+}\r\nabc"},
        new {Expected = "FROM {3}\r\nabc SUBJECT \"{3+}\"",   Input = "FROM {3}\r\nabc SUBJECT \"{3+}\""},
        new {Expected = "FROM {3}\r\nabc SUBJECT \"{3+}\"",   Input = "FROM {3+}\r\nabc SUBJECT \"{3+}\""},
      }) {
        try {
          var criteria = ImapSearchCriteria.FromUri(new Uri(baseUri, "?" + test.Input)/*, true, true*/);

          Assert.IsNotNull(criteria);
          Assert.AreEqual(test.Expected, criteria.ToString());
        }
        catch (ArgumentException ex) {
          Assert.Fail("ArgumentException thrown: {0} {1}", test.Input, ex);
        }
      }
    }

    [Test]
    public void TestFromUriSplitCharset()
    {
      bool discard;
      string charset;
      ImapSearchCriteria criteria;

      criteria = ImapSearchCriteria.FromUri(new Uri("imap://;AUTH=*@minbari.org/gray%20council?SUBJECT%20shadows"),
                                            true, out discard, out charset);

      Assert.IsNull(charset);
      Assert.AreEqual("SUBJECT shadows", criteria.ToString());

      criteria = ImapSearchCriteria.FromUri(new Uri("imap://john;AUTH=*@minbari.example.org/babylon5/personel?charset%20UTF-8%20SUBJECT%20%7B14+%7D%0D%0A%D0%98%D0%B2%D0%B0%D0%BD%D0%BE%D0%B2%D0%B0"),
                                            true, out discard, out charset);

      Assert.AreEqual("UTF-8", charset);
      Assert.AreEqual("SUBJECT {14+}\r\n\xD0\x98\xD0\xB2\xD0\xB0\xD0\xBD\xD0\xBE\xD0\xB2\xD0\xB0", criteria.ToString());
    }

    [Test]
    public void TestFromUriContainsValidQuotedStringAndLiteral()
    {
      var baseUri = new Uri("imap://localhost/INBOX");

      foreach (var query in new[] {
        "SUBJECT {1+}\r\n\"",
        "SUBJECT {2+}\r\n\"\\",
        "SUBJECT {3+}\r\n\"\\x",
        "SUBJECT {5+}\r\n\"\\\"x\\",
        "SUBJECT {6+}\r\n\"\\\"x\\\"",

        "SUBJECT \"\"",
        "SUBJECT \"x\"",
        "SUBJECT \"\\\"x\\\"\"",

        "SUBJECT \"{\"",
        "SUBJECT \"{a\"",
        "SUBJECT \"{0\"",
        "SUBJECT \"{1a\"",
        "SUBJECT \"{1-\"",
        "SUBJECT \"{1+\"",
        "SUBJECT \"{1+1\"",
        "SUBJECT \"{3}x\"",
        "SUBJECT \"{3}\\\\rx\"",
      }) {
        try {
          Assert.AreEqual(query, ImapSearchCriteria.FromUri(new Uri(baseUri, "?" + query)).ToString());
        }
        catch (ArgumentException ex) {
          Assert.Fail("ArgumentException thrown: {0} {1}", query, ex);
        }
      }
    }

    [Test]
    public void TestFromUriContainsInvalidQuotedString()
    {
      var baseUri = new Uri("imap://localhost/INBOX");

      foreach (var uri in new[] {
        new Uri(baseUri, "?SUBJECT \""),
        new Uri(baseUri, "?SUBJECT \"\\"),
        new Uri(baseUri, "?SUBJECT \"\\x"),
        new Uri(baseUri, "?SUBJECT \"\\\"x\\"),
        new Uri(baseUri, "?SUBJECT \"\\\"x\\\""),
      }) {
        try {
          ImapSearchCriteria.FromUri(uri);
          Assert.Fail("ArgumentException not thrown");
        }
        catch (ArgumentException) {
        }
      }
    }

    [Test]
    public void TestFromUriContainsInvalidLiteral()
    {
      var baseUri = new Uri("imap://localhost/INBOX");

      foreach (var uri in new[] {
        new Uri(baseUri, "?SUBJECT {"),
        new Uri(baseUri, "?SUBJECT {a"),
        new Uri(baseUri, "?SUBJECT {0"),
        new Uri(baseUri, "?SUBJECT {1a"),
        new Uri(baseUri, "?SUBJECT {1-"),
        new Uri(baseUri, "?SUBJECT {1+"),
        new Uri(baseUri, "?SUBJECT {1+1"),
        new Uri(baseUri, "?SUBJECT {3}x"),
        new Uri(baseUri, "?SUBJECT {3}\rx"),
        new Uri(baseUri, "?SUBJECT {1}\r\n"),

        new Uri(baseUri, "?SUBJECT {3}\r\nabc{"),
        new Uri(baseUri, "?SUBJECT {3}\r\nabc{a"),
        new Uri(baseUri, "?SUBJECT {3}\r\nabc{0"),
        new Uri(baseUri, "?SUBJECT {3}\r\nabc{1a"),
        new Uri(baseUri, "?SUBJECT {3}\r\nabc{1-"),
        new Uri(baseUri, "?SUBJECT {3}\r\nabc{1+"),
        new Uri(baseUri, "?SUBJECT {3}\r\nabc{1+1"),
        new Uri(baseUri, "?SUBJECT {3}\r\nabc{3}x"),
        new Uri(baseUri, "?SUBJECT {3}\r\nabc{3}\rx"),
        new Uri(baseUri, "?SUBJECT {3}\r\nabc{1}\r\n"),
      }) {
        try {
          ImapSearchCriteria.FromUri(uri);
          Assert.Fail("ArgumentException not thrown");
        }
        catch (ArgumentException) {
        }
      }
    }

    [Test, Ignore("disabled feature")]
    public void TestFromUriRejectNonSynchronizedLiteral()
    {
      var baseUri = new Uri("imap://localhost/INBOX");

      foreach (var uri in new[] {
        new Uri(baseUri, "?SUBJECT {1}\r\na"),
        new Uri(baseUri, "?SUBJECT {3}\r\nabc"),
        new Uri(baseUri, "?SUBJECT \"{1}\" FROM {1}\r\na"),
        new Uri(baseUri, "?SUBJECT \"{1}\" FROM {3}\r\nabc"),
      }) {
        try {
          ImapSearchCriteria.FromUri(uri);
          Assert.Fail("ArgumentException not thrown");
        }
        catch (ArgumentException) {
        }
      }
    }

    [Test]
    public void TestIImapUrlSearchQuery_BuildUrlListsOfMessagesFormUriSearchCriteria()
    {
      var b = new ImapUriBuilder("imap://localhost/", "INBOX", 12345L, ImapSearchCriteria.All);

      Assert.AreEqual(new Uri("imap://localhost/INBOX;UIDVALIDITY=12345?ALL"), b.Uri);

      b.Charset = Encoding.UTF8;

      Assert.AreEqual(new Uri("imap://localhost/INBOX;UIDVALIDITY=12345?ALL"), b.Uri);

      b.SearchCriteria = null;

      Assert.AreEqual(new Uri("imap://localhost/INBOX;UIDVALIDITY=12345"), b.Uri);

      b.Charset = null;

      Assert.AreEqual(new Uri("imap://localhost/INBOX;UIDVALIDITY=12345"), b.Uri);
    }

    [Test]
    public void TestIImapUrlSearchQuery_BuildUrlListsOfMessagesFormUriSearchCriteriaContainsLiterals()
    {
      var b = new ImapUriBuilder("imap://localhost/", "INBOX", 12345L, ImapSearchCriteria.From("差出人"));

      try {
        Assert.IsNotNull(b.Uri);
        Assert.Fail("InvalidOperationException not thrown");
      }
      catch (InvalidOperationException) {
      }

      b.Charset = Encoding.UTF8;

      Assert.AreEqual(new Uri("imap://localhost/INBOX;UIDVALIDITY=12345?CHARSET%20utf-8%20FROM%20%7B9+%7D%0D%0A%E5%B7%AE%E5%87%BA%E4%BA%BA"),
                      b.Uri);

      ImapSearchCriteria.FromUri(b.Uri); // no exceptions will be thrown

      b.Charset = Encoding.GetEncoding("shift_jis");

      Assert.AreEqual(new Uri("imap://localhost/INBOX;UIDVALIDITY=12345?CHARSET%20shift_jis%20FROM%20%7B6+%7D%0D%0A%8D%B7%8Fo%90l"),
                      b.Uri);

      ImapSearchCriteria.FromUri(b.Uri); // no exceptions will be thrown

      b.SearchCriteria = null;

      Assert.AreEqual(new Uri("imap://localhost/INBOX;UIDVALIDITY=12345"),
                      b.Uri);

      b.Charset = null;

      Assert.AreEqual(new Uri("imap://localhost/INBOX;UIDVALIDITY=12345"),
                      b.Uri);
    }

    [Test]
    public void TestConstructImapUriBuilder()
    {
      var uri = "imap://localhost/INBOX?FROM {12+}\r\nfrom address";
      var b = new ImapUriBuilder(uri, ImapSearchCriteria.FromUri);

      Assert.IsNotNull(b.SearchCriteria);
      Assert.IsInstanceOfType(typeof(ImapSearchCriteria), b.SearchCriteria);
      Assert.AreEqual(new Uri("imap://localhost/INBOX?FROM%20%7B12+%7D%0D%0Afrom%20address"), b.Uri);
    }

    [Test]
    public void TestIImapUrlSearchQuery_BuildUrlExample1()
    {
      var b = new ImapUriBuilder();

      b.Scheme = "imap";
      b.Host = "minbari.example.org";
      b.AuthType = ImapAuthenticationMechanism.SelectAppropriate;
      b.Mailbox = "gray council";
      b.SearchCriteria = ImapSearchCriteria.Subject("shadows");

      /*
       Assert.AreEqual(new Uri("imap://;AUTH=*@minbari.example.org/gray%20council?SUBJECT%20shadows"),
                       b.Uri);
      */
      Assert.AreEqual(new Uri("imap://;AUTH=*@minbari.example.org/gray%20council?SUBJECT%20%7B7+%7D%0D%0Ashadows"),
                      b.Uri);

      ImapSearchCriteria.FromUri(b.Uri); // no exceptions will be thrown
    }

    [Test]
    public void TestIImapUrlSearchQuery_BuildUrlExample2()
    {
      var b = new ImapUriBuilder();

      b.Scheme = "imap";
      b.Host = "minbari.example.org";
      b.UserName = "john";
      b.AuthType = ImapAuthenticationMechanism.SelectAppropriate;
      b.Mailbox = "babylon5/personel";
      b.Charset = Encoding.UTF8;
      b.SearchCriteria = ImapSearchCriteria.Subject("Иванова");

      /*
      Assert.AreEqual(new Uri("imap://john;AUTH=*@minbari.example.org/babylon5/personel?" +
                              "charset%20UTF-8%20SUBJECT%20%7B14+%7D%0D%0A%D0%98%D0%B2%" +
                              "D0%B0%D0%BD%D0%BE%D0%B2%D0%B0"),
                      b.Uri);
                      */
      Assert.AreEqual(new Uri("imap://john;AUTH=*@minbari.example.org/babylon5/personel?" +
                              "CHARSET%20utf-8%20SUBJECT%20%7B14+%7D%0D%0A%D0%98%D0%B2%" +
                              "D0%B0%D0%BD%D0%BE%D0%B2%D0%B0"),
                      b.Uri);

      ImapSearchCriteria.FromUri(b.Uri); // no exceptions will be thrown
    }
  }
}
