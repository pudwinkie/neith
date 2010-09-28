// 
// Author:
//       smdn <smdn@mail.invisiblefulmoon.net>
// 
// Copyright (c) 2008-2010 smdn
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

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif
using Smdn.Formats;
using Smdn.Net.Imap4.Protocol;

namespace Smdn.Net.Imap4 {
  public class ImapMessageBody :
    ImapMessageAttributeBase,
    IImapMessageBody
  {
    internal protected ImapMessageBody(long sequence)
      : base(sequence)
    {
    }

    internal void SetContentData(string specifier, ImapData bodyText)
    {
      bodyData[specifier] = bodyText;
    }

    public Stream GetFirstBody()
    {
      return GetFirstBodyData().GetTextAsStream();
    }

    public string GetFirstBodyAsString()
    {
      return GetFirstBodyData().GetTextAsString();
    }

    public byte[] GetFirstBodyAsByteArray()
    {
      return GetFirstBodyData().GetTextAsByteArray();
    }

    public long GetFirstBodyLength()
    {
      return GetFirstBodyData().GetTextLength();
    }

    public void CopyFirstBody(byte[] buffer, int offset, int count)
    {
      GetFirstBodyData().CopyText(buffer, offset, count);
    }

    public int GetFirstBody(ref byte[] buffer)
    {
      return GetFirstBodyData().GetText(ref buffer);
    }

    private ImapData GetFirstBodyData()
    {
      return bodyData.Values.First();
    }

    public Stream GetBody(string specifier)
    {
      var data = GetBodyData(specifier);

      return (data == null) ? null : data.GetTextAsStream();
    }

    public string GetBodyAsString(string specifier)
    {
      var data = GetBodyData(specifier);

      return (data == null) ? null : data.GetTextAsString();
    }

    public byte[] GetBodyAsByteArray(string specifier)
    {
      var data = GetBodyData(specifier);

      return (data == null) ? null : data.GetTextAsByteArray();
    }

    public long? GetBodyLength(string specifier)
    {
      var data = GetBodyData(specifier);

      return (data == null) ? (long?)null : (long?)data.GetTextLength();
    }

    public void CopyBody(string specifier, byte[] buffer, int offset, int count)
    {
      var data = GetBodyData(specifier);

      if (data != null)
        data.CopyText(buffer, offset, count);
    }

    private ImapData GetBodyData(string specifier)
    {
      ImapData body;

      if (bodyData.TryGetValue(specifier, out body))
        return body;
      else
        throw new ArgumentException("no such content", "specifier");
    }

    private Dictionary<string, ImapData> bodyData = new Dictionary<string, ImapData>(StringComparer.OrdinalIgnoreCase);
  }
}
