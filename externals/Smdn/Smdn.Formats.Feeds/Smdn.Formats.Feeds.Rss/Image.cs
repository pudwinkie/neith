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

namespace Smdn.Formats.Feeds.Rss {
  // http://cyber.law.harvard.edu/rss/rss.html
  // <image> sub-element of <channel>
  // <image> is an optional sub-element of <channel>, which contains three required and three optional sub-elements.
  public class Image {
    // <url> is the URL of a GIF, JPEG or PNG image that represents the channel.
    public Uri Url {
      get; set;
    }

    // <title> describes the image, it's used in the ALT attribute of the HTML <img> tag when the channel is rendered in HTML.
    public string Title {
      get; set;
    }

    // <link> is the URL of the site, when the channel is rendered, the image is a link to the site. (Note, in practice the image <title> and <link> should have the same value as the channel's <title> and <link>.
    public Uri Link {
      get; set;
    }

    /// <description> contains text that is included in the TITLE attribute of the link formed around the image in the HTML rendering.
    public string Description {
      get; set;
    }

    // Optional elements include <width> and <height>, numbers, indicating the width and height of the image in pixels. <description> contains text that is included in the TITLE attribute of the link formed around the image in the HTML rendering. * */
    public int? Width {
      get; set;
    }

    public int? Height {
      get; set;
    }

    public Image()
      : this(null, null, null, null, null, null)
    {
    }

    public Image(Uri uri, string title, Uri link, string description, int? width, int? height)
    {
      this.Url = uri;
      this.Title = title;
      this.Link = link;
      this.Description = description;
      this.Width = width;
      this.Height = height;
    }
  }
}
