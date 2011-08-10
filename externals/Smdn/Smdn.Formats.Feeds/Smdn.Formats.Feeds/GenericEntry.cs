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

using Smdn.Formats.Feeds.Modules;

namespace Smdn.Formats.Feeds {
  public class GenericEntry : EntryBase, IEntry {
    public string Title {
      get; set;
    }

    public Atom.Content Description {
      get; set;
    }

    public Uri Link {
      get { return link; }
    }

    public Uri Id {
      get { return id; }
    }

    public bool? IsIdPermaLink {
      get { return isIdPermaLink; }
    }

    public DateTimeOffset? Published {
      get; set;
    }

    string IEntry.Description {
      get { return (string)Description; }
      set { Description = (Atom.Content)value; }
    }

    Uri IEntry.Link {
      get { return link; }
      set { throw new NotSupportedException("readonly"); }
    }

    string IEntry.Id {
      get { return id.ToString(); }
      set { throw new NotSupportedException("readonly"); }
    }

    DateTimeOffset? IEntry.Date {
      get { return Published; }
      set { Published = value; }
    }

    IEnumerable<ModuleBase> IEntry.Modules {
      get { return base.Modules.Values; }
    }

    public GenericEntry(Uri permanentLink)
      : this(permanentLink, permanentLink, true)
    {
    }

    public GenericEntry(Uri link, Uri id)
      : this(link, id, null)
    {
    }

    public GenericEntry(Uri link, Uri id, bool? isIdPermaLink)
    {
      if (link == null)
        throw new ArgumentNullException("link");
      if (id == null)
        throw new ArgumentNullException("id");

      this.Title = null;
      this.Description = null;
      this.link = link;
      this.id = id;
      this.isIdPermaLink = isIdPermaLink;
      this.Published = null;
    }

    public virtual IEntry ConvertTo(FeedVersion feedType)
    {
      feedType = feedType & FeedVersion.TypeMask;

      if (feedType == FeedVersion.Atom) {
        var entry = new Atom.Entry();

        foreach (var module in Modules) {
          entry.Modules.Add(module.Key, module.Value);
        }

        entry.Title = Title;
        entry.Content = Description;
        entry.Summary = null;
        entry.Published = Published;
        entry.Updated = Published;
        entry.Links.Add(new Atom.Link(link));
        entry.Id = id;

        return entry;
      }
      else if (feedType == FeedVersion.RdfRss) {
        var item = new RdfRss.Item();

        foreach (var module in Modules) {
          item.Modules.Add(module.Key, module.Value);
        }

        item.Title = Title;
        item.Description = Title;
        item.Link = link;
        if (Description != null) {
          if (!item.Modules.ContainsKey(Content.NamespaceUri))
            item.Modules.Add(Content.NamespaceUri, new Content());
          item.ContentModule.Encoded = Description;
        }
        if (Published != null) {
          if (!item.Modules.ContainsKey(DublinCore.NamespaceUri))
            item.Modules.Add(DublinCore.NamespaceUri, new DublinCore());
          item.DublinCoreModule.SetDate(Published);
        }
        item.Resource = id;

        return item;
      }
      else if (feedType == FeedVersion.Rss) {
        var item = new Rss.Item();

        foreach (var module in Modules) {
          item.Modules.Add(module.Key, module.Value);
        }

        item.Title = Title;
        item.Description = Title;
        item.Link = link;
        if (Description != null) {
          if (!item.Modules.ContainsKey(Content.NamespaceUri))
            item.Modules.Add(Content.NamespaceUri, new Content());
          item.ContentModule.Encoded = Description;
        }
        item.PubDate = Published;
        item.Guid = new Rss.Guid(id.ToString(), isIdPermaLink);

        return item;
      }
      else {
        throw new VersionNotSupportedException("unsupported or invalid version");
      }
    }

    private /*readonly*/ Uri link;
    private /*readonly*/ Uri id;
    private readonly bool? isIdPermaLink;
  }
}
