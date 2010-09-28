using System;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;

using Smdn.Security.Cryptography;

namespace Smdn.Formats {
  [TestFixture]
  public class ToModifiedBase64TransformTests {
    [Test]
    public void TestTransform()
    {
      foreach (var test in new[] {
        new {Data = new byte[] {0xfb},              ExpectedBase64 = "+w==", Expected2152Base64 = "+w",   Expected3501Base64 = "+w"},
        new {Data = new byte[] {0xfb, 0xf0},        ExpectedBase64 = "+/A=", Expected2152Base64 = "+/A",  Expected3501Base64 = "+,A"},
        new {Data = new byte[] {0xfb, 0xf0, 0x00},  ExpectedBase64 = "+/AA", Expected2152Base64 = "+/AA", Expected3501Base64 = "+,AA"},
      }) {
        //Assert.AreEqual(test.ExpectedBase64, TextConvert.ToBase64String(test.Data), "Base64");
        Assert.AreEqual(Encoding.ASCII.GetBytes(test.Expected2152Base64),
                        ICryptoTransformExtensions.TransformBytes(new ToRFC2152ModifiedBase64Transform(), test.Data),
                        "RFC2152 Base64");
        Assert.AreEqual(Encoding.ASCII.GetBytes(test.Expected3501Base64),
                        ICryptoTransformExtensions.TransformBytes(new ToRFC3501ModifiedBase64Transform(), test.Data),
                        "RFC3501 Base64");
      }
    }
  }
}
