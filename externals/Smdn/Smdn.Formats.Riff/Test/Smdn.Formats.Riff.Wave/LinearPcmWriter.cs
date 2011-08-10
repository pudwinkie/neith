using System;
using System.IO;
using NUnit.Framework;

using Smdn.Media;

namespace Smdn.Formats.Riff.Wave {
  [TestFixture]
  public class LinearPcmWriterTests {
    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestCreateNonLinear()
    {
      var format = new WAVEFORMATEX();

      format.wFormatTag = WAVE_FORMAT_TAG.WAVE_FORMAT_UNKNOWN;

      LinearPcmWriter.Create(Stream.Null, format);
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
          LinearPcmWriter.Create(Stream.Null, format);
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
          LinearPcmWriter.Create(Stream.Null, format);
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

          using (var writer = LinearPcmWriter.Create(stream, format)) {
            Assert.IsNotNull(writer);
            Assert.AreEqual(Platform.Endianness, writer.Endianness, "writer.Endianness");
            Assert.AreEqual(test.Channels, writer.Channels, "writer.Channels");
            Assert.AreEqual(test.BitsPerSample, writer.BitsPerSample, "writer.BitsPerSample");

            Assert.AreEqual(0L, writer.SamplePosition, "writer.SamplePosition");
            Assert.AreEqual(test.ExpectedSampleLength, writer.SampleLength, "writer.SampleLength");
          }

          try {
            stream.WriteByte(0);
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

          using (var writer = LinearPcmWriter.Create(stream, format, true)) {
            Assert.IsNotNull(writer);
          }

          try {
            stream.WriteByte(0);
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

          using (var writer = LinearPcmWriter.Create(stream, format)) {
            Assert.AreEqual(0L, writer.SamplePosition, "writer.SamplePosition ({0})", writer.GetType());

            Assert.AreEqual(test.ExpectedResult, writer.SeekSample(1L, SeekOrigin.Current), "SeekSample return value ({0})", writer.GetType());
            Assert.AreEqual(test.ExpectedSamplePosition, writer.SamplePosition, "writer.SamplePosition ({0})", writer.GetType());
          }
        }
      }
    }

    [Test]
    public void TestWriteSample()
    {
      var format = new WAVEFORMATEX();

      format.wFormatTag = WAVE_FORMAT_TAG.WAVE_FORMAT_PCM;

      foreach (var test in new[] {
        new {Channels = (ushort)1, BitsPerSample = (ushort)8,   BlockAlign = (ushort)1, WriteLValue = (ushort)0x00ff, WriteRValue = (ushort)0x00ff, WriteSamples = new byte[] {0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07}, ExpectedSampleLength = 8L},
        new {Channels = (ushort)1, BitsPerSample = (ushort)16,  BlockAlign = (ushort)2, WriteLValue = (ushort)0x0180, WriteRValue = (ushort)0x0080, WriteSamples = new byte[] {0x02, 0x03, 0x04, 0x05, 0x06, 0x07}, ExpectedSampleLength = 4L},
        new {Channels = (ushort)2, BitsPerSample = (ushort)8,   BlockAlign = (ushort)2, WriteLValue = (ushort)0x0000, WriteRValue = (ushort)0x0100, WriteSamples = new byte[] {0x02, 0x03, 0x04, 0x05, 0x06, 0x07}, ExpectedSampleLength = 4L},
        new {Channels = (ushort)2, BitsPerSample = (ushort)16,  BlockAlign = (ushort)4, WriteLValue = (ushort)0x0100, WriteRValue = (ushort)0x0302, WriteSamples = new byte[] {0x04, 0x05, 0x06, 0x07}, ExpectedSampleLength = 2L},
      }) {
        format.nChannels = test.Channels;
        format.wBitsPerSample = test.BitsPerSample;
        format.nBlockAlign = test.BlockAlign;

        using (var stream = new MemoryStream()) {
          using (var writer = LinearPcmWriter.Create(stream, format)) {
            writer.WriteSample(test.WriteLValue, test.WriteRValue);
            writer.WriteSamples(test.WriteSamples);

            Assert.AreEqual(8L, writer.BaseStream.Position, "writer.BaseStream.Position ({0})", writer.GetType());

            Assert.AreEqual(test.ExpectedSampleLength, writer.SampleLength, "writer.SampleLength ({0})", writer.GetType());

            writer.Flush();
            writer.Close();

            CollectionAssert.AreEqual(new byte[] {
              0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
            }, stream.ToArray(), "written bytes ({0})", writer.GetType());
          }
        }
      }
    }
  }
}

