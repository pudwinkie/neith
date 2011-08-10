using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4.Protocol {
  [TestFixture]
  public class ImapResponseCodeTests {
    [Test]
    public void TestEquals()
    {
      Assert.IsTrue(ImapResponseCode.Alert.Equals(ImapResponseCode.Alert));
      Assert.IsTrue(ImapResponseCode.Alert.Equals("ALERT"));
      Assert.IsTrue(ImapResponseCode.Alert.Equals(new ImapString("ALERT")));
      Assert.IsFalse(ImapResponseCode.Alert.Equals(ImapResponseCode.BadCharset));

      Assert.IsTrue(ImapResponseCode.MetadataTooMany.Equals(ImapResponseCode.MetadataTooMany));
      Assert.IsTrue(ImapResponseCode.MetadataTooMany.Equals(ImapResponseCode.GetKnownOrCreate("METADATA",
                                                                                              ImapData.CreateTextData(ByteString.CreateImmutable("TOOMANY")))));
      Assert.IsFalse(ImapResponseCode.MetadataTooMany.Equals(ImapResponseCode.Metadata));
      Assert.IsFalse(ImapResponseCode.MetadataTooMany.Equals(ImapResponseCode.MetadataNoPrivate));

      Assert.IsTrue(ImapResponseCode.UidValidity.Equals(ImapResponseCode.GetKnownOrCreate("UIDVALIDITY",
                                                                                          ImapData.CreateTextData(ByteString.CreateImmutable("1")))));
    }

    [Test]
    public void TestGetKnownOrCreate()
    {
      Assert.AreSame(ImapResponseCode.Alert,
                     ImapResponseCode.GetKnownOrCreate("ALERT"));

      Assert.AreSame(ImapResponseCode.MetadataTooMany,
                     ImapResponseCode.GetKnownOrCreate("METADATA",
                                                       ImapData.CreateTextData(ByteString.CreateImmutable("TOOMANY"))));

      Assert.AreSame(ImapResponseCode.UidValidity,
                     ImapResponseCode.GetKnownOrCreate("UIDVALIDITY",
                                                       ImapData.CreateTextData(ByteString.CreateImmutable("1"))));
    }

    [Test]
    public void TestOpEquality()
    {
      ImapResponseCode respCode;

      respCode = ImapResponseCode.GetKnownOrCreate("ALERT");

      Assert.IsTrue(ImapResponseCode.Alert == respCode);

      respCode = ImapResponseCode.GetKnownOrCreate("METADATA",
                                                   ImapData.CreateTextData(ByteString.CreateImmutable("TOOMANY")));

      Assert.IsTrue(ImapResponseCode.MetadataTooMany == respCode);

      respCode = ImapResponseCode.GetKnownOrCreate("UIDVALIDITY",
                                                   ImapData.CreateTextData(ByteString.CreateImmutable("1")));

      Assert.IsTrue(ImapResponseCode.UidValidity == respCode);
    }

    [Test]
    public void TestSerializeBinary1()
    {
      TestUtils.SerializeBinary(ImapResponseCode.Alert, delegate(ImapResponseCode deserialized) {
        Assert.AreEqual(ImapResponseCode.Alert, deserialized);
        Assert.AreEqual(ImapResponseCode.Alert.HasArguments, deserialized.HasArguments);
      });
    }

    [Test]
    public void TestSerializeBinary2()
    {
      TestUtils.SerializeBinary(ImapResponseCode.MetadataTooMany, delegate(ImapResponseCode deserialized) {
        Assert.AreEqual(ImapResponseCode.MetadataTooMany, deserialized);
        Assert.AreEqual(ImapResponseCode.MetadataTooMany.HasArguments, deserialized.HasArguments);
      });
    }

    [Test]
    public void TestSerializeBinary3()
    {
      var respCode = ImapResponseCode.GetKnownOrCreate("UIDVALIDITY",
                                                       ImapData.CreateTextData(ByteString.CreateImmutable("1")));

      TestUtils.SerializeBinary(respCode, delegate(ImapResponseCode deserialized) {
        Assert.AreEqual(ImapResponseCode.UidValidity, deserialized);
        Assert.AreEqual(ImapResponseCode.UidValidity.HasArguments, deserialized.HasArguments);
      });
    }
  }
}
