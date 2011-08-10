using System;
using System.IO;
using NUnit.Framework;

using Smdn.Media;

namespace Smdn.Formats.Riff.Wave {
  [TestFixture]
  public class LinearPcmReaderTests {
    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestCreateNonLinear()
    {
      var format = new WAVEFORMATEX();

      format.wFormatTag = WAVE_FORMAT_TAG.WAVE_FORMAT_UNKNOWN;

      LinearPcmReader.Create(Stream.Null, format);
    }

    [Test]
    public void TestCreateUnsupportedChannles()
    {
      var format = new WAVEFORMATEX();

      format.wFormatTag = WAVE_FORMAT_TAG.WAVE_FORMAT_PCM;
      format.wBitsPerSample = 16;

      foreach (var channels in new ushort[] {0, 3}) {
        format.nChannels = channels;

        try {
          LinearPcmReader.Create(Stream.Null, format);
          Assert.Fail("NotSupportedException not thrown: channels = {0}", format.nChannels);
        }
        catch (NotSupportedException) {
        }
      }
    }

    [Test]
    public void TestCreateUnsupportedBitsPerSample()
    {
      var format = new WAVEFORMATEX();

      format.wFormatTag = WAVE_FORMAT_TAG.WAVE_FORMAT_PCM;
      format.nChannels = 2;

      foreach (var bits in new ushort[] {0, 12, 24}) {
        format.wBitsPerSample = bits;

        try {
          LinearPcmReader.Create(Stream.Null, format);
          Assert.Fail("NotSupportedException not thrown: channels = {0}", format.nChannels);
        }
        catch (NotSupportedException) {
        }
      }
    }

    [Test]
    public void TestCreate()
    {
      var format = new WAVEFORMATEX();

      format.wFormatTag = WAVE_FORMAT_TAG.WAVE_FORMAT_PCM;

      foreach (var test in new[] {
        new {Channels = (ushort)1, BitsPerSample = (ushort)8,   BlockAlign = (ushort)1, ExpectedSampleLength = 16L},
        new {Channels = (ushort)1, BitsPerSample = (ushort)16,  BlockAlign = (ushort)2, ExpectedSampleLength = 8L},
        new {Channels = (ushort)2, BitsPerSample = (ushort)8,   BlockAlign = (ushort)2, ExpectedSampleLength = 8L},
        new {Channels = (ushort)2, BitsPerSample = (ushort)16,  BlockAlign = (ushort)4, ExpectedSampleLength = 4L},
      }) {
        format.nChannels = test.Channels;
        format.wBitsPerSample = test.BitsPerSample;
        format.nBlockAlign = test.BlockAlign;

        using (var stream = new MemoryStream()) {
          stream.Write(new byte[16], 0, 16);
          stream.Position = 0L;

          using (var reader = LinearPcmReader.Create(stream, format)) {
            Assert.IsNotNull(reader);
            Assert.AreEqual(Platform.Endianness, reader.Endianness, "reader.Endianness");
            Assert.AreEqual(test.Channels, reader.Channels, "reader.Channels");
            Assert.AreEqual(test.BitsPerSample, reader.BitsPerSample, "reader.BitsPerSample");

            Assert.AreEqual(0L, reader.SamplePosition, "reader.SamplePosition");
            Assert.AreEqual(test.ExpectedSampleLength, reader.SampleLength, "reader.SampleLength");
          }

          try {
            stream.ReadByte();
            Assert.Fail("ObjectDisposedException not thrown");
          }
          catch (ObjectDisposedException) {
          }
        }
      }
    }

    [Test]
    public void TestCreateLeaveBaseStreamOpen()
    {
      var format = new WAVEFORMATEX();

      format.wFormatTag = WAVE_FORMAT_TAG.WAVE_FORMAT_PCM;

      foreach (var test in new[] {
        new {Channels = (ushort)1, BitsPerSample = (ushort)8,   BlockAlign = (ushort)1},
        new {Channels = (ushort)1, BitsPerSample = (ushort)16,  BlockAlign = (ushort)2},
        new {Channels = (ushort)2, BitsPerSample = (ushort)8,   BlockAlign = (ushort)2},
        new {Channels = (ushort)2, BitsPerSample = (ushort)16,  BlockAlign = (ushort)4},
      }) {
        format.nChannels = test.Channels;
        format.wBitsPerSample = test.BitsPerSample;
        format.nBlockAlign = test.BlockAlign;

        using (var stream = new MemoryStream()) {
          stream.Write(new byte[16], 0, 16);
          stream.Position = 0L;

          using (var reader = LinearPcmReader.Create(stream, format, true)) {
            Assert.IsNotNull(reader);
          }

          try {
            Assert.AreNotEqual(-1, stream.ReadByte());
          }
          catch (ObjectDisposedException) {
            Assert.Fail("ObjectDisposedException thrown");
          }
        }
      }
    }

    [Test]
    public void TestSeekSample()
    {
      var format = new WAVEFORMATEX();

      format.wFormatTag = WAVE_FORMAT_TAG.WAVE_FORMAT_PCM;

      foreach (var test in new[] {
        new {Channels = (ushort)1, BitsPerSample = (ushort)8,   BlockAlign = (ushort)1, ExpectedResult = 1L, ExpectedSamplePosition = 1L},
        new {Channels = (ushort)1, BitsPerSample = (ushort)16,  BlockAlign = (ushort)2, ExpectedResult = 2L, ExpectedSamplePosition = 1L},
        new {Channels = (ushort)2, BitsPerSample = (ushort)8,   BlockAlign = (ushort)2, ExpectedResult = 2L, ExpectedSamplePosition = 1L},
        new {Channels = (ushort)2, BitsPerSample = (ushort)16,  BlockAlign = (ushort)4, ExpectedResult = 4L, ExpectedSamplePosition = 1L},
      }) {
        format.nChannels = test.Channels;
        format.wBitsPerSample = test.BitsPerSample;
        format.nBlockAlign = test.BlockAlign;

        using (var stream = new MemoryStream()) {
          stream.Write(new byte[16], 0, 16);
          stream.Position = 0L;

          using (var reader = LinearPcmReader.Create(stream, format)) {
            Assert.AreEqual(0L, reader.SamplePosition, "reader.SamplePosition ({0})", reader.GetType());

            Assert.AreEqual(test.ExpectedResult, reader.SeekSample(1L, SeekOrigin.Current), "SeekSample return value ({0})", reader.GetType());
            Assert.AreEqual(test.ExpectedSamplePosition, reader.SamplePosition, "reader.SamplePosition ({0})", reader.GetType());
          }
        }
      }
    }

    [Test]
    public void TestReadSample()
    {
      var format = new WAVEFORMATEX();

      format.wFormatTag = WAVE_FORMAT_TAG.WAVE_FORMAT_PCM;

      foreach (var test in new[] {
        new {Channels = (ushort)1, BitsPerSample = (ushort)8,   BlockAlign = (ushort)1, ExpectedSample = new byte[] {0x00}, ExpectedLValue = (ushort)0x0100, ExpectedRValue = (ushort)0x0100},
        new {Channels = (ushort)1, BitsPerSample = (ushort)16,  BlockAlign = (ushort)2, ExpectedSample = new byte[] {0x00, 0x01}, ExpectedLValue = (ushort)0x0302, ExpectedRValue = (ushort)0x0302},
        new {Channels = (ushort)2, BitsPerSample = (ushort)8,   BlockAlign = (ushort)2, ExpectedSample = new byte[] {0x00, 0x01}, ExpectedLValue = (ushort)0x0200, ExpectedRValue = (ushort)0x0300},
        new {Channels = (ushort)2, BitsPerSample = (ushort)16,  BlockAlign = (ushort)4, ExpectedSample = new byte[] {0x00, 0x01, 0x02, 0x03}, ExpectedLValue = (ushort)0x0504, ExpectedRValue = (ushort)0x0706},
      }) {
        format.nChannels = test.Channels;
        format.wBitsPerSample = test.BitsPerSample;
        format.nBlockAlign = test.BlockAlign;

        using (var stream = new MemoryStream()) {
          stream.Write(new byte[] {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
          }, 0, 8);
          stream.Position = 0L;

          using (var reader = LinearPcmReader.Create(stream, format)) {
            CollectionAssert.AreEqual(test.ExpectedSample, reader.ReadSample(), "byte[] ReadSample ({0})", reader.GetType());

            ushort left, right;

            reader.ReadSample(out left, out right);

            Assert.AreEqual(test.ExpectedLValue, left, "left of ReadSample(out ushort, out ushort) ({0})", reader.GetType());
            Assert.AreEqual(test.ExpectedRValue, right, "right of ReadSample(out ushort, out ushort) ({0})", reader.GetType());
          }
        }
      }
    }
  }
}

