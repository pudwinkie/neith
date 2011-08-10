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

using Smdn.Collections;
using Smdn.Net.Imap4.Protocol;

namespace Smdn.Net.Imap4 {
  public class ImapExtendedMultiPartBodyStructure : ImapMultiPartBodyStructure, IImapBodyStructureExtension {
    public IDictionary<string, string> Parameters {
      get; private set;
    }

    public ImapBodyDisposition Disposition {
      get; private set;
    }

    public string[] Languages {
      get; private set;
    }

    public Uri Location {
      get; private set;
    }

    public ImapData[] Extensions {
      get; private set;
    }

    public ImapExtendedMultiPartBodyStructure(ImapMultiPartBodyStructure baseStructure,
                                              IDictionary<string, string> parameters,
                                              ImapBodyDisposition disposition,
                                              string[] languages,
                                              Uri location,
                                              ImapData[] extensions)
      : base(baseStructure)
    {
      this.Parameters = (parameters ?? new Dictionary<string, string>()).AsReadOnly(StringComparer.OrdinalIgnoreCase);
      this.Disposition = disposition;
      this.Languages = languages;
      this.Location = location;
      this.Extensions = extensions;
    }
  }
}
