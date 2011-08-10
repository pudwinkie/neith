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

using Smdn.Formats;
using Smdn.Net.MessageAccessProtocols;

namespace Smdn.Net.Imap4 {
  // enum types:
  // * ImapStringEnum : ImapString
  //     => handles string constants
  //     ImapAuthenticationMechanism
  //       => handles authentication mechanisms
  //     ImapCapability
  //       => handles capability constants
  //     ImapCollationAlgorithm
  //       => handles COMPARATOR extension comparator order
  //     ImapCompressionMechanism
  //       => handles COMPRESS extension compression mechanism
  //     ImapMailboxFlag
  //       => handles mailbox flags
  //     ImapMessageFlag
  //       => handles system flags and keywords
  //     ImapThreadingAlgorithm
  //       => handles THREAD extension threading algorithm
  //     Protocol.ImapResponseCode
  //       => handles response codes
  //     Protocol.ImapDataResponseType
  //       => handles server response types

  [Serializable]
  public abstract class ImapStringEnum : ImapString, IStringEnum, IEquatable<ImapStringEnum> {
    string IStringEnum.Value {
      get { return base.Value; }
    }

    protected ImapStringEnum(string @value)
      : base(@value)
    {
      if (@value == null)
        throw new ArgumentNullException("value");
      if (@value.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("value");
    }

#region "defined constants"
    protected static IEnumerable<TStringEnum> GetDefinedConstants<TStringEnum>() where TStringEnum : ImapStringEnum
    {
      return StringEnumUtils.GetDefinedConstants<TStringEnum>();
    }

    protected static ImapStringEnumSet<TStringEnum> CreateDefinedConstantsSet<TStringEnum>() where TStringEnum : ImapStringEnum
    {
      return new ImapStringEnumSet<TStringEnum>(true, GetDefinedConstants<TStringEnum>());
    }
#endregion

#region "equatable"
    public static bool operator == (ImapStringEnum x, ImapStringEnum y)
    {
      if (Object.ReferenceEquals(x, y))
        return true;
      else if ((object)x == null || (object)y == null)
        return false;
      else
        return x.Equals(y);
    }

    public static bool operator != (ImapStringEnum x, ImapStringEnum y)
    {
      return !(x == y);
    }

    public override bool Equals(object obj)
    {
      var imapStringEnum = obj as ImapStringEnum;

      if (imapStringEnum == null) {
        var stringEnum = obj as IStringEnum;

        if (stringEnum != null)
          return Equals(stringEnum);

        var imapString = obj as ImapString;

        if (imapString != null)
          return Equals(imapString);

        var str = obj as string;

        if (str == null)
          return false;
        else
          return Equals(str);
      }
      else {
        return Equals(imapStringEnum);
      }
    }

    public virtual bool Equals(ImapStringEnum other)
    {
      return Equals(other as IStringEnum);
    }

    public bool Equals(IStringEnum other)
    {
      if (null == (object)other)
        return false;
      else
        return Object.ReferenceEquals(this, other) ||
          (GetType() == other.GetType() && Equals(other.Value));
    }

    public override bool Equals(ImapString other)
    {
      if (null == (object)other)
        return false;
      else if (other.GetType() == typeof(ImapString))
        return Equals(other.Value);
      else
        return false;
    }

    public override bool Equals(string other)
    {
      return string.Equals(base.Value, other, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
#endregion
  }
}
