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
using System.Text;

using Smdn.Formats.ApacheLog.Entities;
using Smdn.Formats.ApacheLog.FormatExpressions;

namespace Smdn.Formats.ApacheLog {
  public class ApacheLogReader : ApacheLogReader<NcsaCombinedLogEntry> {
    public ApacheLogReader(Stream baseStream)
      : base(baseStream, NcsaCombinedLogEntry.LogFormatString)
    {
    }

    public ApacheLogReader(TextReader reader)
      : base(reader, NcsaCombinedLogEntry.LogFormatString)
    {
    }
  }

  public class ApacheLogReader<TLogEntry> : IDisposable where TLogEntry : LogEntry, new() {
    public string LogFormat {
      get; private set;
    }

    public ApacheLogReader(Stream baseStream)
      : this(new StreamReader(baseStream, Encoding.ASCII))
    {
    }

    public ApacheLogReader(Stream baseStream, string logFormat)
      : this(new StreamReader(baseStream, Encoding.ASCII), logFormat)
    {
    }

    public ApacheLogReader(TextReader reader)
      : this(reader, GetFormatStringFromLogEntryType())
    {
    }

    public ApacheLogReader(TextReader reader, string logFormat)
    {
      if (reader == null)
        throw new ArgumentNullException("reader");
      if (logFormat == null)
        throw new ArgumentNullException("logFormat");

      this.baseReader = reader;
      this.formatExpressions = FormatExpression.Parse(logFormat);
    }

    private static string GetFormatStringFromLogEntryType()
    {
      var type = typeof(TLogEntry);
      var attrs = type.GetCustomAttributes(typeof(ApacheLogFormatAttribute), true);

      if (attrs.Length == 0)
        throw new InvalidOperationException(string.Format("attribute {0} not found in {1}", typeof(ApacheLogFormatAttribute).Name, type.FullName));

      return (attrs[0] as ApacheLogFormatAttribute).FormatString;
    }

    ~ApacheLogReader()
    {
      Dispose(false);
    }

    void IDisposable.Dispose()
    {
      Close();
    }

    protected void Dispose(bool disposing)
    {
      if (disposing) {
        if (baseReader != null) {
          baseReader.Close();
          baseReader = null;
        }
      }
    }

    public void Close()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    public TLogEntry ReadEntry()
    {
      var line = baseReader.ReadLine();

      if (line == null)
        return null;
      else
        return Parser.ParseLine<TLogEntry>(formatExpressions, line);
    }

    public IEnumerable<TLogEntry> ReadAllEntries()
    {
      var entries = new List<TLogEntry>();

      for (;;) {
        var entry = ReadEntry();

        if (entry == null)
          return entries;
        else
          entries.Add(entry);
      }
    }

    private TextReader baseReader;
    private FormatExpression[] formatExpressions;
  }
}
