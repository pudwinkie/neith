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

namespace Smdn.Net.Imap4 {
  public class ImapMessageRfc822BodyStructure :
    ImapSinglePartBodyStructure,
    System.Collections.IEnumerable // re-implement
  {
    /// <summary>The envelope of encapsulated message.</summary>
    public ImapEnvelope Envelope {
      get; private set;
    }

    /// <summary>The body structure of encapsulated message.</summary>
    public IImapBodyStructure BodyStructure {
      get; private set;
    }

    public ImapMessageRfc822BodyStructure(string section,
                                          MimeType mediaType,
                                          IDictionary<string, string> parameters,
                                          string id,
                                          string description,
                                          string encoding,
                                          long size,
                                          ImapEnvelope envelope,
                                          IImapBodyStructure bodyStructure,
                                          long lineCount)
      : base(section, mediaType, parameters, id, description, encoding, size, lineCount)
    {
      if (envelope == null)
        throw new ArgumentNullException("envelope");
      if (bodyStructure == null)
        throw new ArgumentNullException("bodyStructure");

      this.Envelope       = envelope;
      this.BodyStructure  = bodyStructure;

      ImapBodyStructureUtils.SetParentStructure(this);
    }

    protected ImapMessageRfc822BodyStructure(ImapMessageRfc822BodyStructure baseStructure)
      : base(baseStructure)
    {
      if (baseStructure == null)
        throw new ArgumentNullException("baseStructure");

      this.Envelope       = baseStructure.Envelope;
      this.BodyStructure  = baseStructure.BodyStructure;

      ImapBodyStructureUtils.SetParentStructure(this);
    }

    public override IEnumerator<IImapBodyStructure> GetEnumerator()
    {
      yield return BodyStructure;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      yield return BodyStructure;
    }
  }
}
