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

namespace Smdn.Net.Pop3 {
  // enum types:
  // * PopStringEnum
  //     => handles string constants
  //     PopAuthenticationMechanism
  //       => handles authentication mechanisms
  //     PopCapability
  //       => handles capability constants

  [Serializable]
  public abstract class PopStringEnum : IStringEnum, IEquatable<PopStringEnum> {
    public virtual string Value {
      get; private set;
    }

    protected internal PopStringEnum(string @value)
    {
      if (@value == null)
        throw new ArgumentNullException("value");
      if (@value.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("value");

      this.Value = @value;
    }

#region "defined constants"
    protected static IEnumerable<TStringEnum> GetDefinedConstants<TStringEnum>() where TStringEnum : PopStringEnum
    {
      return StringEnumUtils.GetDefinedConstants<TStringEnum>();
    }

    protected static PopStringEnumSet<TStringEnum> CreateDefinedConstantsSet<TStringEnum>() where TStringEnum : PopStringEnum
    {
      return new PopStringEnumSet<TStringEnum>(true, GetDefinedConstants<TStringEnum>());
    }
#endregion

#region "conversion"
    /*
    public static implicit operator PopStringEnum(string str)
    {
      return new PopStringEnum(str);
    }
    */

    public static explicit operator string(PopStringEnum str)
    {
      if (str == null)
        return null;
      else
        return str.ToString();
    }

    public override string ToString()
    {
      return Value;
    }
#endregion

#region "equatable"
    public static bool operator == (PopStringEnum x, PopStringEnum y)
    {
      if (Object.ReferenceEquals(x, y))
        return true;
      else if ((object)x == null || (object)y == null)
        return false;
      else
        return x.Equals(y);
    }

    public static bool operator != (PopStringEnum x, PopStringEnum y)
    {
      return !(x == y);
    }

    public override bool Equals(object obj)
    {
      var popStringEnum = obj as PopStringEnum;

      if (popStringEnum == null) {
        var stringEnum = obj as IStringEnum;

        if (stringEnum != null)
          return Equals(stringEnum);

        var str = obj as string;

        if (str == null)
          return false;
        else
          return Equals(str);
      }
      else {
        return Equals(popStringEnum);
      }
    }

    public virtual bool Equals(PopStringEnum other)
    {
      return Equals((IStringEnum)other);
    }

    public bool Equals(IStringEnum other)
    {
      if (null == (object)other)
        return false;
      else
        return Object.ReferenceEquals(this, other) ||
          (GetType() == other.GetType() && Equals(other.Value));
    }

    public bool Equals(string other)
    {
      return string.Equals(Value, other, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
#endregion
  }
}
