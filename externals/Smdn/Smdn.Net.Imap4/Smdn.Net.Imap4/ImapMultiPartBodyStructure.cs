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
  public class ImapMultiPartBodyStructure : IImapBodyStructure {
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

    public IEnumerator<IImapBodyStructure> GetEnumerator()
    {
      foreach (var nestedStructure in NestedStructures) {
        yield return nestedStructure;
      }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return NestedStructures.GetEnumerator();
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
      get { return true; }
    }

    /*
     * nested structures
     */
    public IImapBodyStructure[] NestedStructures {
      get; private set;
    }

    public ImapMultiPartBodyStructure(string section,
                                      IImapBodyStructure[] nestedStructures,
                                      string subtype)
    {
      if (section == null)
        throw new ArgumentNullException("section");
      if (nestedStructures == null)
        throw new ArgumentNullException("nestedStructures");

      this.Section          = section;
      this.MediaType        = MimeType.CreateMultipartType(subtype);
      this.NestedStructures = nestedStructures;

      ImapBodyStructureUtils.SetParentStructure(this);
    }

    protected ImapMultiPartBodyStructure(ImapMultiPartBodyStructure baseStructure)
    {
      if (baseStructure == null)
        throw new ArgumentNullException("baseStructure");

      this.Section          = baseStructure.Section;
      this.MediaType        = baseStructure.MediaType;
      this.NestedStructures = baseStructure.NestedStructures;

      ImapBodyStructureUtils.SetParentStructure(this);
    }

    public override string ToString()
    {
      return string.Format("{{Section={0}, MediaType={1}, NestedStructures={2}}}",
                           Section,
                           MediaType,
                           Array.ConvertAll(NestedStructures, delegate(IImapBodyStructure nested) {
                             return nested.ToString();
                           }));
    }
  }
}
