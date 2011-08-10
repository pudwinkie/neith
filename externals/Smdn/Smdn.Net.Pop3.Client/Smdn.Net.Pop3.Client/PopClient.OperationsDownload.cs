// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2010-2011 smdn
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
using System.IO;
using System.Text;

namespace Smdn.Net.Pop3.Client {
  partial class PopClient {
    public string DownloadMessageAsText(long messageNumber, Encoding encoding)
    {
      return GetMessage(messageNumber).ReadAllText(encoding);
    }

    public string DownloadFirstMessageAsText(Encoding encoding)
    {
      return GetFirstMessage().ReadAllText(encoding);
    }

    public string DownloadLastMessageAsText(Encoding encoding)
    {
      return GetLastMessage().ReadAllText(encoding);
    }

    public byte[] DownloadMessageAsByteArray(long messageNumber)
    {
      return GetMessage(messageNumber).ReadAllBytes();
    }

    public byte[] DownloadFirstMessageAsByteArray()
    {
      return GetFirstMessage().ReadAllBytes();
    }

    public byte[] DownloadLastMessageAsByteArray()
    {
      return GetLastMessage().ReadAllBytes();
    }

    public Stream DownloadMessageAsStream(long messageNumber)
    {
      return GetMessage(messageNumber).OpenRead();
    }

    public Stream DownloadFirstMessageAsStream()
    {
      return GetFirstMessage().OpenRead();
    }

    public Stream DownloadLastMessageAsStream()
    {
      return GetLastMessage().OpenRead();
    }

    public TOutput DownloadMessageAs<TOutput>(long messageNumber, Converter<Stream, TOutput> converter)
    {
      return GetMessage(messageNumber).ReadAs(converter);
    }

    public TResult DownloadMessageAs<T, TResult>(long messageNumber, Func<Stream, T, TResult> read, T arg)
    {
      return GetMessage(messageNumber).ReadAs(read, arg);
    }

    public TOutput DownloadFirstMessageAs<TOutput>(Converter<Stream, TOutput> converter)
    {
      return GetFirstMessage().ReadAs(converter);
    }

    public TResult DownloadFirstMessageAs<T, TResult>(Func<Stream, T, TResult> read, T arg)
    {
      return GetFirstMessage().ReadAs(read, arg);
    }

    public TOutput DownloadLastMessageAs<TOutput>(Converter<Stream, TOutput> converter)
    {
      return GetLastMessage().ReadAs(converter);
    }

    public TResult DownloadLastMessageAs<T, TResult>(Func<Stream, T, TResult> read, T arg)
    {
      return GetLastMessage().ReadAs(read, arg);
    }

    public void DownloadMessageToFile(long messageNumber, string path)
    {
      GetMessage(messageNumber).Save(path);
    }

    public void DownloadFirstMessageToFile(string path)
    {
      GetFirstMessage().Save(path);
    }

    public void DownloadLastMessageToFile(string path)
    {
      GetLastMessage().Save(path);
    }
  }
}