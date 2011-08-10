// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2004-2011 smdn
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
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Smdn.Formats.Thbgm {
  public class TrackInfo : IFormattable {
    public ProductInfo Product {
      get; private set;
    }

    public string Title {
      get; private set;
    }

    /// <value>track number, one-based value</value>
    public int TrackNumber {
      get
      {
        if (Product == null)
          return 0;
        else
          return Product.Tracks.IndexOf(this) + 1;
      }
    }

    public StreamFormat StreamFormat {
      get; internal set;
    }

    public long IntroOffset {
      get; private set;
    }

    public long IntroLength {
      get; private set;
    }

    public long RepeatLength {
      get; private set;
    }

    public TrackInfo(ProductInfo product, string title, StreamFormat streamFormat, long introOffset, long introLength, long repeatLength)
      : this(product, title, introOffset, introLength, repeatLength)
    {
      if (streamFormat == null)
        throw new ArgumentNullException("streamFormat");

      this.StreamFormat = streamFormat;
    }

    internal TrackInfo(ProductInfo product, string title, long introOffset, long introLength, long repeatLength)
    {
      this.Product = product;
      this.Title = title;
      this.IntroOffset = introOffset;
      this.IntroLength = introLength;
      this.RepeatLength = repeatLength;
    }

    public string GetUnifiedTitle(TitleUnificationOptions unificationOptions)
    {
      if (Title == null)
        return null;
      else
        return ProductInfo.GetUnifiedTitle(Title, unificationOptions);
    }

    public int CalculateRepeatedTimes(TimeSpan totalLength)
    {
      var length = totalLength - TimeSpan.FromMilliseconds(1000 * IntroLength / StreamFormat.BytesPerSecond);
      var repeatLength = TimeSpan.FromMilliseconds(1000 * RepeatLength / StreamFormat.BytesPerSecond);

      for (int repetition = 0;; repetition++) {
        length -= repeatLength;

        if (length < TimeSpan.Zero)
          return repetition;
      }
    }

    public TimeSpan CalculateRepeatPosition(int timesToRepeat)
    {
      return TimeSpan.FromMilliseconds((1000.0 * CalculateRepeatPositionInBytes(timesToRepeat, false)) / StreamFormat.BytesPerSecond);
    }

    public long CalculateRepeatPositionInBytes(int timesToRepeat, bool containTrackOffset)
    {
      var position = IntroLength + RepeatLength * timesToRepeat;

      if (containTrackOffset)
        position += IntroOffset;

      return position;
    }

    public BgmStream GetStream(int timesToRepeat)
    {
      return GetStream(timesToRepeat, false);
    }

    public BgmStream GetStream(int timesToRepeat, bool loadOnMemory)
    {
      return GetStream<BgmStream>(Product.InstalledOrDefaultBgmSourcePath, timesToRepeat, loadOnMemory, null);
    }

    public TBgmStream GetStream<TBgmStream>(int timesToRepeat) where TBgmStream : BgmStream
    {
      return GetStream<TBgmStream>(timesToRepeat, false);
    }

    public TBgmStream GetStream<TBgmStream>(int timesToRepeat, bool loadOnMemory) where TBgmStream : BgmStream
    {
      return GetStream<TBgmStream>(Product.InstalledOrDefaultBgmSourcePath, timesToRepeat, loadOnMemory, null);
    }

    public TBgmStream GetStream<TBgmStream>(int timesToRepeat, params object[] args) where TBgmStream : BgmStream
    {
      return GetStream<TBgmStream>(timesToRepeat, false, args);
    }

    public TBgmStream GetStream<TBgmStream>(int timesToRepeat, bool loadOnMemory, params object[] args) where TBgmStream : BgmStream
    {
      return GetStream<TBgmStream>(Product.InstalledOrDefaultBgmSourcePath, timesToRepeat, loadOnMemory, args);
    }

    public BgmStream GetStream(string thbgmPath, int timesToRepeat)
    {
      return GetStream(thbgmPath, timesToRepeat, false);
    }

    public BgmStream GetStream(string thbgmPath, int timesToRepeat, bool loadOnMemory)
    {
      return GetStream<BgmStream>(thbgmPath, timesToRepeat, loadOnMemory, null);
    }

    public TBgmStream GetStream<TBgmStream>(string thbgmPath, int timesToRepeat) where TBgmStream : BgmStream
    {
      return GetStream<TBgmStream>(thbgmPath, timesToRepeat, false);
    }

    public TBgmStream GetStream<TBgmStream>(string thbgmPath, int timesToRepeat, bool loadOnMemory) where TBgmStream : BgmStream
    {
      return GetStream<TBgmStream>(thbgmPath, timesToRepeat, loadOnMemory, null);
    }

    public TBgmStream GetStream<TBgmStream>(string thbgmPath, int timesToRepeat, params object[] args) where TBgmStream : BgmStream
    {
      return GetStream<TBgmStream>(thbgmPath, timesToRepeat, false, args);
    }

    public TBgmStream GetStream<TBgmStream>(string thbgmPath, int timesToRepeat, bool loadOnMemory, params object[] args) where TBgmStream : BgmStream
    {
      if (thbgmPath == null)
        throw new ArgumentNullException("thbgmPath");

      if (args == null)
        args = new object[0];

      var constructArgs = new object[args.Length + 4];

      constructArgs[0] = StreamFormat.GetStreamFile(thbgmPath, TrackNumber);
      constructArgs[1] = this;
      constructArgs[2] = timesToRepeat;
      constructArgs[3] = loadOnMemory;

      Array.Copy(args, 0, constructArgs, 4, args.Length);

      try {
        return (TBgmStream)Activator.CreateInstance(typeof(TBgmStream),
                                                    BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance,
                                                    null,
                                                    constructArgs,
                                                    System.Globalization.CultureInfo.CurrentCulture);
      }
      catch (TargetInvocationException ex) {
        throw ex.InnerException;
      }
    }

    public override string ToString()
    {
      return ToString(null, null);
    }

    public string ToString(string format)
    {
      return ToString(format, null);
    }

    public string ToString(string format, IFormatProvider formatProvider)
    {
      if (format == null)
        return string.Format("[TrackInfo: Product={0}, Title={1}, TrackNumber={2}, StreamFormat={3}, IntroOffset={4}, IntroLength={5}, RepeatLength={6}]", Product, Title, TrackNumber, StreamFormat, IntroOffset, IntroLength, RepeatLength);

      var trackFormat = (formatProvider == null)
        ? null
        : (TrackFormatInfo)formatProvider.GetFormat(typeof(TrackFormatInfo));

      var unificationOptions  = (trackFormat == null) ? TitleUnificationOptions.NoUnify : trackFormat.TitleUnificationOptions;
      var zeroPadding         = (trackFormat == null) ? true : trackFormat.ZeroPadding;

      var formatRegex = new Regex(@"\$([A-Za-z0-9]+)", RegexOptions.Singleline);

      return formatRegex.Replace(format, delegate(Match m) {
        switch (m.Groups[1].Value.ToLower()) {
          case "title":     return ProductInfo.GetUnifiedTitle(Title, unificationOptions);
          case "trackno":   return TrackNumber.ToString(zeroPadding ? "D2" : "D");
          case "tracks":    return ((Product == null) ? 0 : Product.Tracks.Count).ToString(zeroPadding ? "D2" : "D");
          case "creator":   return (Product == null) ? string.Empty : Product.Creator;
          case "product":   return (Product == null) ? string.Empty : ProductInfo.GetUnifiedTitle(Product.Title, unificationOptions);
          case "pshort1":   return (Product == null) ? string.Empty : Product.ShortName;
          case "pshort2":   return (Product == null) ? string.Empty : Product.AbbreviatedShortName;
          case "pshortest": return (Product == null) ? string.Empty : Product.ShortestName;
          case "prefix":    return (Product == null) ? string.Empty : Product.Prefix;
          case "ryear":     return (Product == null) ? string.Empty : Product.ReleaseDate.Year.ToString(zeroPadding ? "D4" : "D");
          case "rmonth":    return (Product == null) ? string.Empty : Product.ReleaseDate.Month.ToString(zeroPadding ? "D2" : "D");
          case "rday":      return (Product == null) ? string.Empty : Product.ReleaseDate.Day.ToString(zeroPadding ? "D2" : "D");
        }

        return m.Value;
      });
    }
  }
}
