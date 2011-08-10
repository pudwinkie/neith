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

namespace Smdn.Imaging.Formats.SusiePlugins {
  public enum SusieErrorCode : int {
    /// <summary>正常終了</summary>
    Success = 0,

    /// <summary>その機能はインプリメントされていない</summary>
    NotImplemented = -1,

    /// <summary>コールバック関数が非0を返したので展開を中止した</summary>
    Aborted = 1,

    /// <summary>未知のフォーマット</summary>
    UnsupportedFormat = 2,

    /// <summary>データが壊れている</summary>
    InvalidData = 3,

    /// <summary>メモリーが確保出来ない</summary>
    OutOfMemory = 4,

    /// <summary>メモリーエラー（Lock出来ない、等）</summary>
    MemoryError = 5,

    /// <summary>ファイルリードエラー</summary>
    FileReadError = 6,

    /// <summary>（予約）</summary>
    Reserved = 7,

    /// <summary>窓が開けない</summary>
    /// <remarks>undocumented: <see cref="http://www.asahi-net.or.jp/~kh4s-smz/spi/make_spi.html"/></remarks>
    WindowError = 7,

    /// <summary>内部エラー</summary>
    InternalError = 8,

    /// <summary>書き込みエラー</summary>
    /// <remarks>undocumented: <see cref="http://www.asahi-net.or.jp/~kh4s-smz/spi/make_spi.html"/></remarks>
    FileWriteError = 9,

    /// <summary>ファイル終端</summary>
    /// <remarks>undocumented: <see cref="http://www.asahi-net.or.jp/~kh4s-smz/spi/make_spi.html"/></remarks>
    UnexpectedEndOfStream = 10,
  }
}