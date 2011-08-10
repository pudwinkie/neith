// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2009-2011 smdn
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

namespace Smdn.Formats.ApacheLog.FormatExpressions {
  internal class FormatExpression {
    protected FormatExpression()
    {
    }

    public static FormatExpression[] Parse(string format)
    {
      var chars = format.ToCharArray();
      var index = 0;
      var expressions = new List<FormatExpression>();

      for (;;) {
        var delimiterStartIndex = index;
        var eos = false;

        for (;;index++) {
          eos = (chars.Length <= index);

          if (eos || chars[index] == '%') {
            if (delimiterStartIndex <= index)
              expressions.Add(new DelimiterFormatExpression() {Delimiter = ArrayExtensions.Slice(chars, delimiterStartIndex, index - delimiterStartIndex)});
            index++;
            break;
          }
        }

        if (eos)
          break;

        if (chars[index] == '{') {
          index++;

          var keyOffset = index;
          var keyLength = 0;

          for (;;) {
            if (chars[index++] == '}')
              break;
            else
              keyLength++;
          }

          expressions.Add(new EntityFormatExpression() {Entity = string.Format("{{}}{0}", chars[index++]), Key = new string(chars, keyOffset, keyLength)});
        }
        else {
          if (chars[index] == '>')
            index++; // %>s

          expressions.Add(new EntityFormatExpression() {Entity = chars[index++].ToString(), Key = null});
        }
      }

      return expressions.ToArray();
    }
  }
}
