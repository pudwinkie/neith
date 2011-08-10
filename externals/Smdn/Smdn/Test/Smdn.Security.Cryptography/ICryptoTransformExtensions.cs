using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using NUnit.Framework;

namespace Smdn.Security.Cryptography {
  [TestFixture]
  public class ICryptoTransformExtensionsTests {
    [Test]
    public void TestTransformBytes()
    {
      var buffer = new byte[] {0xff, 0xff, 0x61, 0x62, 0x63, 0x64, 0x65, 0xff, 0xff};
      var expected = new byte[] {0x59, 0x57, 0x4a, 0x6a, 0x5a, 0x47, 0x55, 0x3d};

      using (var transform = new ToBase64Transform()) {
        Assert.AreEqual(expected,
                        ICryptoTransformExtensions.TransformBytes(transform, buffer.Slice(2, 5)));
        Assert.AreEqual(expected,
                        ICryptoTransformExtensions.TransformBytes(transform, buffer, 2, 5));
      }
    }

    [Test]
    public void TestTransformBytesArgumentException()
    {
      var buffer = new byte[] {0xff, 0xff, 0x61, 0x62, 0x63, 0x64, 0x65, 0xff, 0xff};

      using (var transform = new ToBase64Transform()) {
        try {
          ICryptoTransformExtensions.TransformBytes(null, buffer, 0, 9);
          Assert.Fail("ArgumentNullException");
        }
        catch (ArgumentNullException) {
        }

        try {
          ICryptoTransformExtensions.TransformBytes(transform, null, 0, 9);
          Assert.Fail("ArgumentNullException");
        }
        catch (ArgumentNullException) {
        }

        try {
          ICryptoTransformExtensions.TransformBytes(transform, buffer, -1, 10);
          Assert.Fail("ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) {
        }

        try {
          ICryptoTransformExtensions.TransformBytes(transform, buffer, 10, -1);
          Assert.Fail("ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) {
        }

        try {
          ICryptoTransformExtensions.TransformBytes(transform, buffer, 1, 9);
          Assert.Fail("ArgumentException");
        }
        catch (ArgumentException) {
        }

        try {
          ICryptoTransformExtensions.TransformBytes(transform, buffer, 9, 1);
          Assert.Fail("ArgumentException");
        }
        catch (ArgumentException) {
        }
      }
    }

    private byte[] TransformByCryptoStream(HashAlgorithm algorithm, byte[] bytes)
    {
      algorithm.Initialize();

      return TransformByCryptoStream((ICryptoTransform)algorithm, bytes);
    }

    private byte[] TransformByCryptoStream(SymmetricAlgorithm algorithm, byte[] bytes, bool encrypt)
    {
      algorithm.Clear();

      if (encrypt)
        return TransformByCryptoStream(algorithm.CreateEncryptor(), bytes);
      else
        return TransformByCryptoStream(algorithm.CreateDecryptor(), bytes);
    }

    private byte[] TransformByCryptoStream(ICryptoTransform transform, byte[] bytes)
    {
      using (var memoryStream = new MemoryStream()) {
        using (var cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write)) {
          cryptoStream.Write(bytes, 0, bytes.Length);
        }

        memoryStream.Close();

        return memoryStream.ToArray();
      }
    }

    [Test]
    public void TestTranformBytesWithHashAlgorithm()
    {
      var bytes = Encoding.ASCII.GetBytes("The quick brown fox jumps over the lazy dog");

      var hashAlgorithms = new HashAlgorithm[] {
        new HMACMD5(),
        new HMACSHA512(),
        MD5.Create(),
        new SHA512Managed(),
        new RIPEMD160Managed(),
      };

      foreach (var hashAlgorithm in hashAlgorithms) {
        hashAlgorithm.Initialize();

        Assert.AreEqual(hashAlgorithm.TransformBytes(bytes),
                        TransformByCryptoStream(hashAlgorithm, bytes),
                        "HashAlgorithm: {0}",
                        hashAlgorithm.GetType());
      }
    }

    [Test]
    public void TestTranformBytesWithSymmetricAlgorithm()
    {
      var bytes = Encoding.ASCII.GetBytes("The quick brown fox jumps over the lazy dog");

      var symmetricAlgorithms = new SymmetricAlgorithm[] {
        Rijndael.Create(),
        DES.Create(),
        TripleDES.Create(),
        RC2.Create(),
      };

      foreach (var symmetricAlgorithm in symmetricAlgorithms) {
        symmetricAlgorithm.Key = MathUtils.GetRandomBytes(symmetricAlgorithm.KeySize / 8);
        symmetricAlgorithm.GenerateIV();

        symmetricAlgorithm.Clear();

        var encrypted = symmetricAlgorithm.CreateEncryptor().TransformBytes(bytes);

        Assert.AreEqual(TransformByCryptoStream(symmetricAlgorithm, bytes, true),
                        encrypted,
                        "SymmetricAlgorithm (Encrypt): {0}",
                        symmetricAlgorithm.GetType());

        symmetricAlgorithm.Clear();

        var decrypted = Encoding.ASCII.GetString(symmetricAlgorithm.CreateDecryptor().TransformBytes(encrypted));

        Assert.AreEqual(Encoding.ASCII.GetString(TransformByCryptoStream(symmetricAlgorithm, encrypted, false)),
                        decrypted,
                        "SymmetricAlgorithm (Decrypt): {0}",
                        symmetricAlgorithm.GetType());
      }
    }
  }
}
