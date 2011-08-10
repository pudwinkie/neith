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
using System.Xml;

using Smdn.Xml;

namespace Smdn.Formats.Feeds.Modules {
  // RDF Site Summary 1.0 Modules: Syndication
  //   http://web.resource.org/rss/1.0/modules/syndication/
  public class Syndication : ModuleBase {
    public enum Period {
      Hourly,
      Daily,
      Weekly,
      Monthly,
      Yearly,
    }

    public const string Prefix = "sy";
    public const string NamespaceUri = "http://purl.org/rss/1.0/modules/syndication/";

    public static readonly Syndication Null = new Syndication();

    public override string ModulePrefix {
      get { return Prefix; }
    }

    public override string ModuleNamespaceUri {
      get { return NamespaceUri; }
    }

    protected internal override bool IsNull {
      get { return this == Null; }
    }

    /// <summary>Describes the period over which the channel format is updated. Acceptable values are: hourly, daily, weekly, monthly, yearly. If omitted, daily is assumed.</summary>
    public Period? UpdatePeriod {
      get; set;
    }

    /// <summary>Used to describe the frequency of updates in relation to the update period. A positive integer indicates how many times in that period the channel is updated. For example, an updatePeriod of daily, and an updateFrequency of 2 indicates the channel format is updated twice daily. If omitted a value of 1 is assumed.</summary>
    public int? UpdateFrequency {
      get; set;
    }

    /// <summary>Defines a base date to be used in concert with updatePeriod and updateFrequency to calculate the publishing schedule. The date format takes the form: yyyy-mm-ddThh:mm</summary>
    public DateTimeOffset? UpdateBase {
      get; set;
    }

    internal protected override void Parse(FeedBase feed, XmlNode parent, XmlNamespaceManager nsmgr)
    {
      try {
        nsmgr.PushScope();
        nsmgr.AddNamespace("sy", NamespaceUri);

        UpdatePeriod    = parent.GetSingleNodeValueOf<Period?>("sy:updatePeriod/text()", nsmgr, null, ConvertUtils.ToEnumNullable<Period>);
        UpdateFrequency = parent.GetSingleNodeValueOf<int?>("sy:updateFrequency/text()", nsmgr, null, ConvertUtils.ToInt32Nullable);
        UpdateBase      = parent.GetSingleNodeValueOf<DateTimeOffset?>("sy:updateBase/text()", nsmgr, null, DateTimeConvert.FromW3CDateTimeOffsetStringNullable);
      }
      finally {
        nsmgr.PopScope();
      }
    }

    internal protected override void Format(FeedBase feed, XmlNode parent)
    {
      foreach (var pair in new[] {
        new {LocalName = "updatePeriod",    Value = (UpdatePeriod == null ? null : UpdatePeriod.Value.ToString().ToLowerInvariant())},
        new {LocalName = "updateFrequency", Value = ConvertUtils.ToStringNullable(UpdateFrequency)},
        new {LocalName = "updateBase",      Value = DateTimeConvert.ToW3CDateTimeStringNullable(UpdateBase)},
      }) {
        if (pair.Value == null)
          continue;

        parent.AppendChild(parent.OwnerDocument.CreateElement(Prefix, pair.LocalName, NamespaceUri))
              .AppendChild(parent.OwnerDocument.CreateTextNode(pair.Value));
      }
    }
  }
}