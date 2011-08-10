using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using NUnit.Framework;

namespace Smdn.Formats {
  public static class TestUtils {
    public static class Encodings {
      public static readonly Encoding Jis = Encoding.GetEncoding("iso-2022-jp");
      public static readonly Encoding ShiftJis = Encoding.GetEncoding("shift_jis");
      public static readonly Encoding EucJP = Encoding.GetEncoding("euc-jp");
    }

    public static void SerializeBinary<TSerializable>(TSerializable obj)
      where TSerializable : ISerializable
    {
      SerializeBinary(obj, null);
    }

    public static void SerializeBinary<TSerializable>(TSerializable obj,
                                                      Action<TSerializable> action)
      where TSerializable : ISerializable
    {
      var serializeFormatter = new BinaryFormatter();

      using (var stream = new MemoryStream()) {
        serializeFormatter.Serialize(stream, obj);

        stream.Position = 0L;

        var deserializeFormatter = new BinaryFormatter();
        var deserialized = deserializeFormatter.Deserialize(stream);

        Assert.IsNotNull(deserialized);
        Assert.AreNotSame(obj, deserialized);
        Assert.IsInstanceOfType(typeof(TSerializable), deserialized);

        if (action != null)
          action((TSerializable)deserialized);
      }
    }
  }
}
