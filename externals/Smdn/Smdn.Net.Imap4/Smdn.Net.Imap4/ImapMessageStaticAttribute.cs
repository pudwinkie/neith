// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2008-2011 smdn
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

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif
using Smdn.Formats;

namespace Smdn.Net.Imap4 {
  public sealed class ImapMessageStaticAttribute :
    ImapMessageAttributeBase,
    IImapMessageStaticAttribute
  {
    public Uri Url {
      get
      {
        if (uriBuilder == null)
          throw new NotSupportedException("The base URL is not specified.");

        return uriBuilder.Uri;
      }
    }

    public long Uid {
      get; internal set;
    }

    public ImapEnvelope Envelope {
      get; internal set;
    }

    public IImapBodyStructure BodyStructure {
      get; internal set;
    }

    public long Rfc822Size {
      get; internal set;
    }

    public DateTimeOffset? InternalDate {
      get; internal set;
    }

    /*
     * RFC 3516 - IMAP4 Binary Content Extension
     * http://tools.ietf.org/html/rfc3516
     */
    public long BinarySize {
      get { return sectionBinarySize.Values.First(); }
    }

    internal ImapMessageStaticAttribute(long sequence)
      : base(sequence)
    {
    }

    internal void SetSectionBinarySize(string section, long binarySize)
    {
      sectionBinarySize[section] = binarySize;
    }

    public long GetBinarySizeOf(string section)
    {
      long binarySize;

      if (sectionBinarySize.TryGetValue(section, out binarySize))
        return binarySize;
      else
        throw new ArgumentException("no such section", "section");
    }

    /// <summary>Infrastructure. It is not intended to be used directly from your code.</summary>
    void IImapUrl.SetBaseUrl(ImapUriBuilder baseUrl)
    {
      uriBuilder = baseUrl.Clone();

      if (Uid == 0L) {
        uriBuilder.Uid = 0L;
        uriBuilder.SearchCriteria = GetSequenceNumberSearchCriteria();
      }
      else if (Uid != uriBuilder.Uid) {
        uriBuilder.Uid = Uid;
        uriBuilder.SearchCriteria = null;
      }

      if (BodyStructure != null)
        BodyStructure.SetBaseUrl(uriBuilder);
    }

    public override string ToString()
    {
      return string.Format("{{Sequence={0}, Uid={1}, BodyStructure={2}, Envelope={3}, InternalDate={4}, Rfc822Size={5}}}",
                           Sequence,
                           Uid,
                           BodyStructure,
                           Envelope,
                           InternalDate,
                           Rfc822Size);
    }

    private ImapUriBuilder uriBuilder;
    private Dictionary<string, long> sectionBinarySize = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
  }
}
