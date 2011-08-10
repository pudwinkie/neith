// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2009-2011 smdn
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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Smdn.Formats.Loliconvert {
  public static class LoliFile {
#region "utility methods"
    public static bool IsLolizedFile(string path)
    {
      if (path == null)
        throw new ArgumentNullException("path");
      else if (!File.Exists(path))
        throw new FileNotFoundException("file not found", path);

      using (var stream = File.OpenRead(path)) {
        var reader = new BinaryReader(stream);
        var bytes = reader.ReadBytes(40);

        for (var i = 0; i < bytes.Length; i++) {
          if (bytes[i] != LoliOctets.ロ &&
              bytes[i] != LoliOctets.リ &&
              bytes[i] != LoliOctets.コ &&
              bytes[i] != LoliOctets.ン)
            return false;
        }

        return true;
      }
    }

    public static string GetEncodedPathOf(string path)
    {
      if (path == null)
        throw new ArgumentNullException("path");
      if (string.Empty.Equals(Path.GetFileName(path)))
        throw new ArgumentException("path is not file");

      return path + ".txt";
    }

    public static string GetDecodedPathOf(string path)
    {
      if (path == null)
        throw new ArgumentNullException("path");
      if (string.Empty.Equals(Path.GetFileName(path)))
        throw new ArgumentException("path is not file");

#if true
      var extension = Path.GetExtension(path);

      if (extension.Length == 0) {
        if (Path.GetFileName(path).Length < 5)
          throw new LolisticException("file name of path is too short, at least 5 is required");
        return path.Substring(0, path.Length - 4/*".txt".Length*/);
      }
      else {
        // XXX: not compliant?
        return path.Substring(0, path.Length - extension.Length);
      }
#else
      return path.Substring(0, path.Length - 4);
#endif
    }
#endregion

#region "Open*"
    public static Stream OpenRead(string path)
    {
      if (!IsLolizedFile(path))
        throw new LolisticException("file is not lolized");

      return new LoliStream(File.OpenRead(path), LoliconvertMode.Decode);
    }

    public static Stream OpenWrite(string path)
    {
      return new LoliStream(File.OpenWrite(path), LoliconvertMode.Encode);
    }
#endregion

#region "Convert, Encode, Decode"
    /// <returns>returns path of converted file</returns>
    public static string Convert(string path)
    {
      if (IsLolizedFile(path))
        return Decode(path);
      else
        return Encode(path);
    }

    /// <returns>returns path of encoded file</returns>
    public static string Encode(string path)
    {
      var pathEncoded = GetEncodedPathOf(path);

      Encode(path, pathEncoded);

      return pathEncoded;
    }

    public static void Encode(string pathEncode, string pathEncoded)
    {
      using (var fileStream = File.OpenRead(pathEncode))
      using (var encodeStream = OpenWrite(pathEncoded)) {
        var buffer = new byte[1024];

        for (;;) {
          var read = fileStream.Read(buffer, 0, buffer.Length);

          encodeStream.Write(buffer, 0, read);

          if (read <= 0)
            break;
        }

        encodeStream.Flush();
      }
    }

    /// <returns>returns path of decoded file</returns>
    public static string Decode(string path)
    {
      var pathDecoded = GetDecodedPathOf(path);

      Decode(path, pathDecoded);

      return pathDecoded;
    }

    public static void Decode(string pathDecode, string pathDecoded)
    {
      using (var fileStream = File.OpenWrite(pathDecoded))
      using (var decodeStream = OpenRead(pathDecode)) {
        var buffer = new byte[1024];

        for (;;) {
          var read = decodeStream.Read(buffer, 0, buffer.Length);

          fileStream.Write(buffer, 0, read);

          if (read <= 0)
            break;
        }

        fileStream.Flush();
      }
    }
#endregion

#region "ReadAll*"
    public static byte[] ReadAllBytes(string path)
    {
      using (var stream = OpenRead(path)) {
        using (var readStream = new MemoryStream()) {
          var reader = new BinaryReader(stream);

          for (;;) {
            var read = reader.ReadBytes(1024);

            if (read.Length <= 0)
              break;

            readStream.Write(read, 0, read.Length);
          }

          readStream.Close();

          return readStream.ToArray();
        }
      }
    }

    public static string[] ReadAllLines(string path)
    {
      return ReadAllLinesCore(path, null);
    }

    public static string[] ReadAllLines(string path, Encoding encoding)
    {
      if (encoding == null)
        throw new ArgumentNullException("encoding");

      return ReadAllLinesCore(path, encoding);
    }

    private static string[] ReadAllLinesCore(string path, Encoding encoding)
    {
      using (var stream = OpenRead(path)) {
        var reader = (encoding == null)
          ? new StreamReader(stream, false)
          : new StreamReader(stream, encoding, false);
        var read = new List<string>();

        for (;;) {
          var line = reader.ReadLine();

          if (line == null)
            break;

          read.Add(line);
        }

        return read.ToArray();
      }
    }

    public static string ReadAllText(string path)
    {
      return ReadAllTextCore(path, null);
    }

    public static string ReadAllText(string path, Encoding encoding)
    {
      if (encoding == null)
        throw new ArgumentNullException("encoding");

      return ReadAllTextCore(path, encoding);
    }

    private static string ReadAllTextCore(string path, Encoding encoding)
    {
      using (var stream = OpenRead(path)) {
        var reader = (encoding == null)
          ? new StreamReader(stream, false)
          : new StreamReader(stream, encoding, false);

        return reader.ReadToEnd();
      }
    }
#endregion

#region "WriteAll*"
    public static void WriteAllBytes(string path, byte[] bytes)
    {
      using (var stream = OpenWrite(path)) {
        if (bytes == null)
          return;

        var writer = new BinaryWriter(stream);

        writer.Write(bytes);
        writer.Flush();
      }
    }

    public static void WriteAllLines(string path, string[] contents)
    {
      WriteAllLinesCore(path, contents, null);
    }

    public static void WriteAllLines(string path, string[] contents, Encoding encoding)
    {
      if (encoding == null)
        throw new ArgumentNullException("encoding");

      WriteAllLinesCore(path, contents, encoding);
    }

    private static void WriteAllLinesCore(string path, string[] contents, Encoding encoding)
    {
      using (var stream = OpenWrite(path)) {
        var writer = (encoding == null)
          ? new StreamWriter(stream)
          : new StreamWriter(stream, encoding);

        if (contents != null) {
          foreach (var line in contents) {
            writer.WriteLine(line);
          }
        }

        writer.Flush();
      }
    }

    public static void WriteAllText(string path, string contents)
    {
      WriteAllTextCore(path, contents, null);
    }

    public static void WriteAllText(string path, string contents, Encoding encoding)
    {
      if (encoding == null)
        throw new ArgumentNullException("encoding");

      WriteAllTextCore(path, contents, encoding);
    }

    private static void WriteAllTextCore(string path, string contents, Encoding encoding)
    {
      using (var stream = OpenWrite(path)) {
        var writer = (encoding == null)
          ? new StreamWriter(stream)
          : new StreamWriter(stream, encoding);

        if (contents != null)
          writer.Write(contents);

        writer.Flush();
      }
    }
#endregion
  }
}
