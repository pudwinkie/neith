using System;
using System.Text;
using NUnit.Framework;

namespace Smdn.Security.Authentication.Sasl.Client {
  public static class BytesAssert {
    public static void AreEqual(string expected, byte[] actual)
    {
      AreEqual(Encoding.ASCII.GetBytes(expected), actual);
    }

    public static void AreEqual(byte[] expected, byte[] actual)
    {
      if (!ArrayExtensions.EqualsAll(expected, actual)) {
        Assert.Fail("not equalexpected\nExpected string: {0}\nActual string  : {1}\nExpected bytes: {2}\nActual bytes  : {3}",
                    Encoding.ASCII.GetString(expected).Replace("\0", "<NUL>"),
                    Encoding.ASCII.GetString(actual).Replace("\0", "<NUL>"),
                    BitConverter.ToString(expected),
                    BitConverter.ToString(actual));
      }
    }
  }
}
