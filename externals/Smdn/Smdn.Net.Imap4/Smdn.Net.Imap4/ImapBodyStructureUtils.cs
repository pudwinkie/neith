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
#if NET_3_5
using System.Linq;
#endif
using System.Text;

using Smdn.Collections;

namespace Smdn.Net.Imap4 {
  public static class ImapBodyStructureUtils {
    public static void Traverse(this IImapBodyStructure bodyStructure, Action<IImapBodyStructure> action)
    {
      if (bodyStructure == null)
        throw new ArgumentNullException("bodyStructure");
      if (action == null)
        throw new ArgumentNullException("action");

      foreach (var s in bodyStructure.EnumerateDepthFirst()) {
        action(s);
      }
    }

    public static IImapBodyStructure Find(this IImapBodyStructure bodyStructure, Predicate<IImapBodyStructure> match)
    {
      return FindAll(bodyStructure, match).FirstOrDefault();
    }

    public static IEnumerable<IImapBodyStructure> FindAll(this IImapBodyStructure bodyStructure, Predicate<IImapBodyStructure> match)
    {
      if (bodyStructure == null)
        throw new ArgumentNullException("bodyStructure");
      if (match == null)
        throw new ArgumentNullException("match");

      foreach (var s in bodyStructure.EnumerateDepthFirst()) {
        if (match(s))
          yield return s;
      }
    }

    public static IImapBodyStructure Find(this IImapBodyStructure bodyStructure, MimeType mediaType)
    {
      return FindAll(bodyStructure, mediaType).FirstOrDefault();
    }

    public static IEnumerable<IImapBodyStructure> FindAll(this IImapBodyStructure bodyStructure, MimeType mediaType)
    {
      if (mediaType == null)
        throw new ArgumentNullException("mediaType");

      return FindAll(bodyStructure, delegate(IImapBodyStructure s) {
        return s.MediaType.EqualsIgnoreCase(mediaType);
      });
    }

    public static IImapBodyStructure FindSection(this IImapBodyStructure bodyStructure, params int[] subSections)
    {
      var section = new StringBuilder(0x10);

      for (var i = 0; i < subSections.Length; i++) {
        if (subSections[i] < 1)
          throw new ArgumentOutOfRangeException("subSections",
                                                subSections[i],
                                                string.Format("contains zero or negative number (index: {0})", i));

        if (i != 0)
          section.Append('.');

        section.Append(subSections[i]);
      }

      return FindSection(bodyStructure, section.ToString());
    }

    public static IImapBodyStructure FindSection(this IImapBodyStructure bodyStructure, string section)
    {
      if (bodyStructure == null)
        throw new ArgumentNullException("bodyStructure");

      return Find(bodyStructure, delegate(IImapBodyStructure s) {
        return string.Equals(s.Section, section, StringComparison.Ordinal);
      });
    }

    public static IImapBodyStructure GetRootStructure(this IImapBodyStructure bodyStructure)
    {
      if (bodyStructure == null)
        throw new ArgumentNullException("bodyStructure");

      for (var s = bodyStructure;; ) {
        if (s.ParentStructure == null)
          return s;
        else
          s = s.ParentStructure;
      }
    }

    internal static Uri GetUrl(IImapBodyStructure bodyStructure, ImapUriBuilder baseUrl)
    {
      if (baseUrl == null)
        throw new NotSupportedException("The base URL is not specified.");
      else if (baseUrl.Uid == 0L)
        throw new NotSupportedException("The UID of the base URL is not specified.");

      if (baseUrl.Section != bodyStructure.Section)
        baseUrl.Section = bodyStructure.Section;

      return baseUrl.Uri;
    }

    internal static void SetParentStructure(ImapMessageRfc822BodyStructure parent)
    {
      if (parent.BodyStructure is ImapSinglePartBodyStructure)
        (parent.BodyStructure as ImapSinglePartBodyStructure).ParentStructure = parent;
      else if (parent.BodyStructure is ImapMultiPartBodyStructure)
        (parent.BodyStructure as ImapMultiPartBodyStructure).ParentStructure = parent;
    }

    internal static void SetParentStructure(ImapMultiPartBodyStructure parent)
    {
      foreach (var nested in parent.NestedStructures) {
        if (nested is ImapSinglePartBodyStructure)
          (nested as ImapSinglePartBodyStructure).ParentStructure = parent;
        else if (nested is ImapMultiPartBodyStructure)
          (nested as ImapMultiPartBodyStructure).ParentStructure = parent;
      }
    }
  }
}
