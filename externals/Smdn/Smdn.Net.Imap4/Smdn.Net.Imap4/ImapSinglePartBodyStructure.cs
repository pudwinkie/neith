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

namespace Smdn.Net.Imap4 {
  public class ImapSinglePartBodyStructure : IImapBodyStructure {
    /*
     * IImapUrl impl
     */
    public Uri Url {
      get { return ImapBodyStructureUtils.GetUrl(this, uriBuilder); }
    }

    /// <summary>Infrastructure. It is not intended to be used directly from your code.</summary>
    void IImapUrl.SetBaseUrl(ImapUriBuilder baseUrl)
    {
      uriBuilder = baseUrl.Clone();

      foreach (var nested in this) {
        nested.SetBaseUrl(baseUrl);
      }
    }

    private ImapUriBuilder uriBuilder = null;

    public virtual IEnumerator<IImapBodyStructure> GetEnumerator()
    {
      yield break;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      yield break;
    }

    /*
     * IImapBodyStructure impl
     */
    public IImapBodyStructure ParentStructure {
      get; internal set;
    }

    public string Section {
      get; private set;
    }

    public MimeType MediaType {
      get; private set;
    }

    public bool IsMultiPart {
      get { return false; }
    }

    /*
     * basic fields
     */
    public IDictionary<string, string> Parameters {
      get; private set;
    }

    public string Id {
      get; private set;
    }

    public string Description {
      get; private set;
    }

    public string Encoding {
      get; private set;
    }

    public long Size {
      get; private set;
    }

    /*
     * type TEXT, type MESSAGE
     */
    public long LineCount { // the size of the body in text lines(type TEXT)
                            // size in text lines of the encapsulated message(type MESSAGE)
      get; private set;
    }

    public ImapSinglePartBodyStructure(string section,
                                       MimeType mediaType,
                                       IDictionary<string, string> parameters,
                                       string id,
                                       string description,
                                       string encoding,
                                       long size,
                                       long lineCount)
    {
      if (section == null)
        throw new ArgumentNullException("section");
      if (mediaType == null)
        throw new ArgumentNullException("mediaType");
      if (size < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("size", size);
      if (lineCount < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("lineCount", lineCount);

      this.Section        = section;
      this.MediaType      = mediaType;
      this.Parameters     = (parameters ?? new Dictionary<string, string>()).AsReadOnly(StringComparer.OrdinalIgnoreCase);
      this.Id             = id;
      this.Description    = description;
      this.Encoding       = encoding;
      this.Size           = size;
      this.LineCount      = lineCount;
    }

    protected ImapSinglePartBodyStructure(ImapSinglePartBodyStructure baseStructure)
    {
      if (baseStructure == null)
        throw new ArgumentNullException("baseStructure");

      this.Section        = baseStructure.Section;
      this.MediaType      = baseStructure.MediaType;
      this.Parameters     = baseStructure.Parameters;
      this.Id             = baseStructure.Id;
      this.Description    = baseStructure.Description;
      this.Encoding       = baseStructure.Encoding;
      this.Size           = baseStructure.Size;
      this.LineCount      = baseStructure.LineCount;
    }

    public override string ToString()
    {
      return string.Format("{{Section={0}, MediaType={1}, Parameters={2}, Id={3}, Description={4}, Encoding={5}, Size={6}, LineCount={7}}}",
                           Section,
                           MediaType,
                           Parameters.Count,
                           Id,
                           Description,
                           Encoding,
                           Size,
                           LineCount);
    }
  }
}