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
  public class SusiePluginException : SystemException {
    public static Exception GetExceptionFromErrorCode(SusieErrorCode errorCode)
    {
      switch (errorCode) {
        case SusieErrorCode.Success: return null;
        case SusieErrorCode.NotImplemented:          return new NotImplementedException();
        case SusieErrorCode.Aborted:                 return new SusiePluginException("operation aborted", errorCode);
        case SusieErrorCode.UnsupportedFormat:       return new NotSupportedException();
        case SusieErrorCode.InvalidData:             return new System.IO.InvalidDataException();
        case SusieErrorCode.OutOfMemory:             return new OutOfMemoryException();
        case SusieErrorCode.MemoryError:             return new SusiePluginException("memory error", errorCode);
        case SusieErrorCode.FileReadError:           return new System.IO.IOException("file read error");
        case SusieErrorCode.WindowError:             return new SusiePluginException("window error", errorCode);
        case SusieErrorCode.InternalError:           return new SusiePluginException("internal error", errorCode);
        case SusieErrorCode.FileWriteError:          return new System.IO.IOException("file write error");
        case SusieErrorCode.UnexpectedEndOfStream:   return new System.IO.EndOfStreamException("unexpected end-of-stream");
        default:
          throw new SusiePluginException(string.Format("unknown error: {0}", errorCode), errorCode);
      }
    }

    internal static void ThrowIfError(SusieErrorCode errorCode)
    {
      if (errorCode != SusieErrorCode.Success)
        throw GetExceptionFromErrorCode(errorCode);
    }

    public SusieErrorCode ErrorCode {
      get; private set;
    }

    public SusiePluginException(SusieErrorCode errorCode)
      : this(string.Format("error: {0}", errorCode), errorCode)
    {
    }

    public SusiePluginException(string message, SusieErrorCode errorCode)
      : base(message)
    {
      this.ErrorCode = errorCode;
    }
  }
}
