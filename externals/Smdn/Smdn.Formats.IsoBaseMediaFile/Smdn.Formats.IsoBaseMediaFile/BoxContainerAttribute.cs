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

namespace Smdn.Formats.IsoBaseMediaFile {
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
  public class BoxContainerAttribute : BoxAttribute {
    public FourCC[] Containers {
      get { return containers; }
    }

    public string UserType {
      get { return userType; }
      set { userType = value; }
    }

    public BoxContainerAttribute(string container)
    {
      this.containers = new[] {new FourCC(container)};
    }

    public BoxContainerAttribute(string container, params string[] containers)
    {
      var converted = Array.ConvertAll(containers, delegate(string input) {
        return (FourCC)input;
      });

      this.containers = new FourCC[1 + converted.Length];
      this.containers[0] = new FourCC(container);

      Array.Copy(converted, 0, this.containers, 1, converted.Length);
    }

    private readonly FourCC[] containers;
    private string userType = null;
  }
}
