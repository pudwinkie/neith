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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Smdn.Formats.Thbgm {
  public class ProductInfo {
    /*
     * class members
     */
    private static bool embeddedProductsLoaded = false;
    private static Dictionary<string, ProductInfo> embeddedProducts = new Dictionary<string, ProductInfo>() {
      {"th06",        null},
      {"th07",        null},
      {"th08",        null},
      {"th08tr",      null},
      {"th09",        null},
      {"th09tr",      null},
      {"th095",       null},
      {"th10",        null},
      {"th10tr",      null},
      {"th10tr_web",  null},
      {"th11",        null},
      {"th11tr",      null},
      {"th12",        null},
      {"th12tr",      null},

      {"alcostg",     null},
    };

    private static void EnsureEmbeddedProductsLoaded()
    {
      if (embeddedProductsLoaded)
        return;

      LoadEmbeddedProductInfos(embeddedProducts);

      embeddedProductsLoaded = true;
    }

    public static ProductInfo Th06 {
      get {
        EnsureEmbeddedProductsLoaded();
        return embeddedProducts["th06"];
      }
    }

    public static ProductInfo Th07 {
      get {
        EnsureEmbeddedProductsLoaded();
        return embeddedProducts["th07"];
      }
    }

    public static ProductInfo Th08 {
      get {
        EnsureEmbeddedProductsLoaded();
        return embeddedProducts["th08"];
      }
    }

    public static ProductInfo Th08tr {
      get {
        EnsureEmbeddedProductsLoaded();
        return embeddedProducts["th08tr"];
      }
    }

    public static ProductInfo Th09 {
      get {
        EnsureEmbeddedProductsLoaded();
        return embeddedProducts["th09"];
      }
    }

    public static ProductInfo Th09tr {
      get {
        EnsureEmbeddedProductsLoaded();
        return embeddedProducts["th09tr"];
      }
    }

    public static ProductInfo Th095 {
      get {
        EnsureEmbeddedProductsLoaded();
        return embeddedProducts["th095"];
      }
    }

    public static ProductInfo Th10 {
      get {
        EnsureEmbeddedProductsLoaded();
        return embeddedProducts["th10"];
      }
    }

    public static ProductInfo Th10tr {
      get {
        EnsureEmbeddedProductsLoaded();
        return embeddedProducts["th10tr"];
      }
    }

    public static ProductInfo Th10trWeb {
      get {
        EnsureEmbeddedProductsLoaded();
        return embeddedProducts["th10tr_web"];
      }
    }

    public static ProductInfo Th11 {
      get {
        EnsureEmbeddedProductsLoaded();
        return embeddedProducts["th11"];
      }
    }

    public static ProductInfo Th11tr {
      get {
        EnsureEmbeddedProductsLoaded();
        return embeddedProducts["th11tr"];
      }
    }

    public static ProductInfo Th12 {
      get {
        EnsureEmbeddedProductsLoaded();
        return embeddedProducts["th12"];
      }
    }

    public static ProductInfo Th12tr {
      get {
        EnsureEmbeddedProductsLoaded();
        return embeddedProducts["th12tr"];
      }
    }

    public static ProductInfo Alcostg {
      get {
        EnsureEmbeddedProductsLoaded();
        return embeddedProducts["alcostg"];
      }
    }

    public static IEnumerable<ProductInfo> EmbeddedProducts {
      get {
        EnsureEmbeddedProductsLoaded();
        return embeddedProducts.Values;
      }
    }

    /*
     * instance members
     */
    private enum ProductType {
      Th06,
      ThXX,
    }

    public bool IsEmbedded {
      get { return embeddedProducts.ContainsValue(this); }
    }

    public string Title {
      get; private set;
    }

    public string Creator {
      get; private set;
    }

    public string Prefix {
      get; private set;
    }

    public string ShortName {
      get; private set;
    }

    public string AbbreviatedShortName {
      get; private set;
    }

    public /*char*/ string ShortestName {
      get; private set;
    }

    public DateTime ReleaseDate {
      get; private set;
    }

    public string DefaultBgmSourcePath {
      get; private set;
    }

    public string InstalledBgmSourcePath {
      get; set;
    }

    public string InstalledOrDefaultBgmSourcePath {
      get
      {
        if (string.IsNullOrEmpty(InstalledBgmSourcePath))
          return DefaultBgmSourcePath;
        else
          return InstalledBgmSourcePath;
      }
    }

    public Guid THxxBGMProductGuid {
      get; private set;
    }

    public IList<TrackInfo> Tracks {
      get; private set;
    }

    private long bgmSourceLength = 0L;
    private byte[] bgmSourceHash = null;
    private byte[] bgmSourceIdentificationHash = null;

    private ProductInfo()
    {
      Title = null;
      Creator = null;
      DefaultBgmSourcePath = null;
      InstalledBgmSourcePath = null;
      THxxBGMProductGuid = Guid.Empty;
    }

    public ProductInfo(string title, string creator, string defaultBgmSourcePath, IList<TrackInfo> tracks)
    {
      this.Title = title;
      this.Creator = creator;
      this.DefaultBgmSourcePath = defaultBgmSourcePath;
      this.Tracks = tracks;

      this.InstalledBgmSourcePath = null;
      this.THxxBGMProductGuid = Guid.Empty;
    }

    public void ValidateBgmSource(string bgmSourcePath)
    {
      // TODO: impl
      throw new NotImplementedException();
    }

    public override string ToString()
    {
      return string.Format("[ProductInfo: Title={0}, Creator={1}, DefaultBgmSourcePath={2}, Tracks={3}]", Title, Creator, DefaultBgmSourcePath, Tracks);
    }

    public string GetUnifiedTitle(TitleUnificationOptions unificationOptions)
    {
      if (Title == null)
        return null;
      else
        return GetUnifiedTitle(Title, unificationOptions);
    }

    public static string GetUnifiedTitle(string title, TitleUnificationOptions options)
    {
      if (title == null)
        throw new ArgumentNullException("title");

      var spaceOption           = (options & TitleUnificationOptions.SpaceOptionMask);
      var titleDelimiterOption  = (options & TitleUnificationOptions.TitleDelimiterOptionMask);

      if (spaceOption == TitleUnificationOptions.DoubleSpace)
        title = title.Replace("\u3000", "\u0020\u0020");
      else if (spaceOption == TitleUnificationOptions.SingleIdeographicSpace)
        title = title.Replace("\u0020\u0020", "\u3000");

      if (titleDelimiterOption == TitleUnificationOptions.FullWidthTilde)
        title = title.Replace('\u301c', '\uff5e');
      else if (titleDelimiterOption == TitleUnificationOptions.WaveDash)
        title = title.Replace('\uff5e', '\u301c');

      return title;
    }

    public static ProductInfo FindMatchedProduct(string bgmSourcePath)
    {
      return FindMatchedProduct(bgmSourcePath, embeddedProducts.Values);
    }

    public static ProductInfo FindMatchedProduct(string bgmSourcePath, IEnumerable<ProductInfo> products)
    {
      if (!File.Exists(bgmSourcePath))
        return null;

      EnsureEmbeddedProductsLoaded();

      /*
       * step 1: compare file length
       */
      var candidates = new List<ProductInfo>();
      var length = (new FileInfo(bgmSourcePath)).Length;

      foreach (var product in products) {
        // length matched
        if (product.bgmSourceLength == length)
          candidates.Add(product);
      }

      if (candidates.Count == 0) {
        // not found
        return null;
      }
      else if (candidates.Count == 1) {
        // matched found
        candidates[0].InstalledBgmSourcePath = bgmSourcePath;
        return candidates[0];
      }

      /*
       * step 2: compare file hash
       */
      const long hintLength = 0x400000;
      byte[] hash;

      using (var stream = File.OpenRead(bgmSourcePath)) {
        if (stream.Length < hintLength)
          // too short
          return null;

        hash = System.Security.Cryptography.MD5.Create().ComputeHash(new Smdn.IO.PartialStream(stream, stream.Length - hintLength, hintLength, true, true));
      }

      foreach (var candidate in candidates) {
        if (candidate.bgmSourceIdentificationHash == null || candidate.bgmSourceIdentificationHash.Length != 16)
          continue; // can't compare

        if (ArrayExtensions.EqualsAll(hash, candidate.bgmSourceIdentificationHash)) {
          // matched found
          candidate.InstalledBgmSourcePath = bgmSourcePath;
          return candidate;
        }
      }

      // not found
      return null;
    }

    /*
     * read/write
     */
    public static ProductInfo LoadFrom(string file)
    {
      using (var stream = File.OpenRead(file)) {
        return LoadFrom(stream);
      }
    }

    public static ProductInfo LoadFrom(string file, Encoding encoding)
    {
      using (var stream = File.OpenRead(file)) {
        return LoadFrom(stream, file, encoding);
      }
    }

    public static ProductInfo LoadFrom(Stream stream)
    {
      Encoding encoding;

      var verifiedStream = VerifyInputStream(stream, out encoding);

      return LoadFrom(verifiedStream, (stream is FileStream) ? (stream as FileStream).Name : null, encoding);
    }

    public static ProductInfo LoadFrom(Stream stream, Encoding encoding)
    {
      return LoadFrom(stream, (stream is FileStream) ? (stream as FileStream).Name : null, encoding);
    }

    private static readonly byte[] shiftJisCharsetMark = new byte[] {0x93, 0x8C, 0x95, 0xFB}; // shift_jis-encoded string of '東方'
    private static readonly byte[] utf8CharsetMark = new byte[] {0xe6, 0x9d, 0xb1, 0xe6, 0x96, 0xb9}; // utf8-encoded string of '東方'
    private static readonly byte[] utf8ByteOrderMark = new byte[] {0xef, 0xbb, 0xbf};

    private static Stream VerifyInputStream(Stream stream, out Encoding encoding)
    {
      if (0xa00000 < stream.Length)
        // larger than 10kB, probably broken or invalid file
        throw new InvalidDataException("stream is too large. probably broken or invalid data");

      var reader = new BinaryReader(stream);

      reader.BaseStream.Position = 0;

      var data = reader.ReadBytes((int)reader.BaseStream.Length);
      var verifiedDataStream = new MemoryStream(data); // return value

      // check preamble
      if (data.Length < utf8ByteOrderMark.Length)
        // too few
        throw new InvalidDataException("stream is too short. probably broken or invalid data");

      var bomFound = true;

      for (var i = 0; i < utf8ByteOrderMark.Length; i++) {
        if (data[i] != utf8ByteOrderMark[i]) {
          bomFound = false;
          break;
        }
      }

      if (bomFound) {
        encoding = Encoding.UTF8;
        return verifiedDataStream;
      }

      // search charset mark
      var matches = new[] {
        // length of CharsetMark must be longer than 1
        new {CharsetMark = shiftJisCharsetMark.GetEnumerator(), Encoding = Encoding.GetEncoding("shift_jis")},
        new {CharsetMark = utf8CharsetMark.GetEnumerator(),     Encoding = Encoding.UTF8},
      };

      foreach (var match in matches) {
        match.CharsetMark.MoveNext();
      }

      for (var i = 0; i < data.Length; i++) {
        foreach (var match in matches) {
          if (data[i] == (byte)match.CharsetMark.Current) {
            if (!match.CharsetMark.MoveNext()) {
              encoding = match.Encoding;
              return verifiedDataStream;
            }
          }
          else {
            match.CharsetMark.Reset();
            match.CharsetMark.MoveNext();
          }
        }
      }

      encoding = Encoding.ASCII; // default

      return verifiedDataStream;
    }

    private static ProductInfo LoadFrom(Stream stream, string filename, Encoding encoding)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");
      if (encoding == null)
        throw new ArgumentNullException("encoding");

      using (var reader = new StreamReader(stream, encoding)) {
        var product = new ProductInfo();
        var tracks = new List<TrackInfo>();
        var productType = ProductType.ThXX;

        if (filename != null)
          // guess prefix from filename
          product.Prefix = Path.GetFileNameWithoutExtension(filename).ToLowerInvariant().Replace("titles_", string.Empty);
        else
          product.Prefix = null;

        for (var lineNumber = 0;; lineNumber++) {
          var line = reader.ReadLine();

          if (line == null)
            break;

          if (string.Empty.Equals(line))
            continue; // empty line

          if (line.StartsWith("#")) {
            // comment line
            if (line.StartsWith("#=")) {
              TrackInfo track;

              line = line.Substring(2);

              if (ReadTrackLineExtended(line, product, out track))
                // extended track line (thplay specific)
                tracks.Add(track);
              else
                // product info extension
                ReadExtensionLine(line, product);
            }

            continue;
          }

          if (line.StartsWith("@")) {
            if (!ReadProductLine(line, product, out productType))
              throw new InvalidDataException(string.Format("invalid data at product line (line: {0})", lineNumber + 1));
          }
          else if (line.StartsWith("%")) {
            TrackInfo track;

            line = line.Substring(1);

            if (!ReadTrackLineStreamFileSpecified(line, product, out track))
              throw new InvalidDataException(string.Format("invalid data at track line (line: {0})", lineNumber + 1));

            tracks.Add(track);
          }
          else {
            TrackInfo track;

            if (!ReadTrackLine(line, product, out track))
              throw new InvalidDataException(string.Format("invalid data at track line (line: {0})", lineNumber + 1));

            if (productType == ProductType.Th06)
              track.StreamFormat = StreamFormat.Th06;
            else
              track.StreamFormat = StreamFormat.ThXX;

            tracks.Add(track);
          }
        } // for

        product.Tracks = tracks.AsReadOnly();

        return product;
      } // using
    }

    private static bool ReadProductLine(string line, ProductInfo product, out ProductType productType)
    {
      productType = ProductType.ThXX;

      // product info
      var data = line.Split(new char[] {','});

      if (data.Length < 2)
        return false;

      string defaultBgmSourcePath;

      if (data[0].StartsWith("@")) {
        // @path
        defaultBgmSourcePath = product.DefaultBgmSourcePath = data[0].Substring(1);

        if (!Path.IsPathRooted(product.DefaultBgmSourcePath)) {
          if (Path.DirectorySeparatorChar != '\\')
            product.DefaultBgmSourcePath = product.DefaultBgmSourcePath.Replace('\\', Path.DirectorySeparatorChar);

          if (Runtime.IsRunningOnWindows)
            product.DefaultBgmSourcePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), product.DefaultBgmSourcePath);
          else
            product.DefaultBgmSourcePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), product.DefaultBgmSourcePath);
        }
      }
      else {
        defaultBgmSourcePath = product.DefaultBgmSourcePath = data[0];
      }

      var productTitleIndex = 1;

      if (3 <= data.Length) {
        switch (data[1].ToLowerInvariant()) {
          case "th06":
            productType = ProductType.Th06;
            productTitleIndex = 2;
            break;
        }
      }

      product.Title = string.Join(",", data, productTitleIndex, data.Length - productTitleIndex).Trim();

      // guess abbreviations from title
      int delimiter;

      delimiter = defaultBgmSourcePath.IndexOf('\\');

      if (1 <= delimiter)
        product.Creator = defaultBgmSourcePath.Substring(0, delimiter);
      else
        product.Creator = null;

      delimiter = product.Title.IndexOfAny(new[] {' ', '\u3000'});

      if (1 <= delimiter)
        product.ShortName = product.Title.Substring(0, delimiter);
      else
        product.ShortName = product.Title;

      if (product.ShortName.StartsWith("東方"))
        product.AbbreviatedShortName = product.ShortName.Substring(2);
      else
        product.AbbreviatedShortName = product.ShortName;

      product.ShortestName = product.AbbreviatedShortName.Substring(0, 1);

      return true;
    }

    private static bool ReadTrackLine(string line, ProductInfo product, out TrackInfo track)
    {
      track = null;

      var data = line.Split(new char[] {','});

      if (data.Length < 3)
        return false;

      track = ToTrackInfo(data, 0, product);

      return true;
    }

    private static TrackInfo ToTrackInfo(string[] data, int startIndex, ProductInfo product)
    {
      var dataCount = data.Length - startIndex;

      return new TrackInfo(product,
                           ((4 <= dataCount) ? string.Join(",", data, startIndex + 3, dataCount - 3) : null),
                           Convert.ToInt64(data[startIndex + 0], 16),
                           Convert.ToInt64(data[startIndex + 1], 16),
                           Convert.ToInt64(data[startIndex + 2], 16));
    }

    private static bool ReadTrackLineStreamFileSpecified(string line, ProductInfo product, out TrackInfo track)
    {
      track = null;

      var data = line.Split(new char[] {','});

      if (data.Length < 4)
        return false;

      // XXX: thplay specific
      track = ToTrackInfo(data, 1, product);
      track.StreamFormat = StreamFormat.Create(data[0],
                                               StreamFormat.ThXX.SamplesPerSecond,
                                               StreamFormat.ThXX.BitsPerSample,
                                               StreamFormat.ThXX.Channels);

      return true;
    }

    private static bool ReadTrackLineExtended(string line, ProductInfo product, out TrackInfo track)
    {
      track = null;

      var data = line.Split(new char[] {','});

      if (data.Length < 5)
        return false;

      var index = 0;
      var parsed = false;
      var samplesPerSecond = StreamFormat.ThXX.SamplesPerSecond; // as default

      for (; index < data.Length; index++) {
        var exitFor = false;

        switch (data[index]) {
          case "SamplingRate":
            // XXX: thplay specific
            parsed = true;
            samplesPerSecond = Convert.ToInt32(data[++index]);
            break;

          default:
            exitFor = true;
            break;
        }

        if (exitFor)
          break;
      }

      if (!parsed)
        return false;

      track = ToTrackInfo(data, index, product);
      track.StreamFormat = StreamFormat.Create(null,
                                               samplesPerSecond,
                                               StreamFormat.ThXX.BitsPerSample,
                                               StreamFormat.ThXX.Channels);

      return true;
    }

    private static void ReadExtensionLine(string line, ProductInfo product)
    {
      var data = line.Split(new char[] {','});

      if (data.Length < 1)
        return;

      if (data[0] == "ProductInfo" && 2 < data.Length) {
        var joined = string.Join(",", data, 2, data.Length - 2);

        switch (data[1]) {
          case "Creator":               if (!string.IsNullOrEmpty(joined)) product.Creator               = joined; break;
          case "Prefix":                if (!string.IsNullOrEmpty(joined)) product.Prefix                = joined; break;
          case "ShortName":             if (!string.IsNullOrEmpty(joined)) product.ShortName             = joined; break;
          case "AbbreviatedShortName":  if (!string.IsNullOrEmpty(joined)) product.AbbreviatedShortName  = joined; break;
          case "ShortestName":          if (!string.IsNullOrEmpty(joined)) product.ShortestName          = joined; break;

          case "ReleaseDate":
            product.ReleaseDate = DateTime.ParseExact(data[2], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal);
            break;

          case "BgmSourceLength":
            if (data.Length < 3)
              break;
            else if (string.IsNullOrEmpty(data[2]))
              break;

            product.bgmSourceLength = long.Parse(data[2]);

            break;

          case "BgmSourceHash":
          case "BgmSourceIdentificationHash":
            if (data.Length < 4)
              break;

            if (data[2] == "MD5Sum" && 32 == data[3].Length) {
              var hash = Hexadecimals.ToByteArray(data[3]);

              if (data[1] == "BgmSourceHash")
                product.bgmSourceHash = hash;
              else if (data[1] == "BgmSourceIdentificationHash")
                product.bgmSourceIdentificationHash = hash;
            }

            break;

          case "THxxBGMProductGuid":
            if (data.Length < 3)
              break;
            else if (string.IsNullOrEmpty(data[2]))
              break;

            product.THxxBGMProductGuid = new Guid(data[2]);
            break;
        }
      }
    }

    private static void LoadEmbeddedProductInfos(Dictionary<string, ProductInfo> embeddedProducts)
    {
      var assembly = Assembly.GetExecutingAssembly();
      var resourceNames = new List<string>(assembly.GetManifestResourceNames());

      foreach (var name in new List<string>(embeddedProducts.Keys)) {
        var resourceName = string.Format("titles_{0}.txt", name);

        if (!resourceNames.Contains(resourceName))
          continue;

        embeddedProducts[name] = LoadFrom(assembly.GetManifestResourceStream(resourceName), Encoding.UTF8);
      }
    }

    public void Save(string file)
    {
      using (var stream = File.OpenWrite(file)) {
        Save(stream);
      }
    }

    public void Save(Stream stream)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      stream.SetLength(0L);

      var writer = new StreamWriter(stream, Encoding.UTF8);
      var entryAssemblyName = (Assembly.GetEntryAssembly() == null) ? null : Assembly.GetEntryAssembly().GetName();
      var executingAssemblyName = Assembly.GetExecutingAssembly().GetName();

      // write header
      writer.Write("#「{0}」 曲データ (", Title);

      if (entryAssemblyName != null)
        writer.Write("{0} {1}; ", entryAssemblyName.Name, entryAssemblyName.Version);

      writer.WriteLine("{0} {1}; {2})",
                       executingAssemblyName.Name,
                       executingAssemblyName.Version,
                       DateTime.Now.ToString("o"));

      // write product line
      var rootPath = Environment.GetFolderPath(Runtime.IsRunningOnWindows ? Environment.SpecialFolder.ProgramFiles : Environment.SpecialFolder.Personal);
      var bgmSourcePath = DefaultBgmSourcePath;

      if (bgmSourcePath.StartsWith(rootPath)) {
        // make releative
        bgmSourcePath = bgmSourcePath.Substring(rootPath.Length);

        if (bgmSourcePath[0] == Path.DirectorySeparatorChar)
          bgmSourcePath = bgmSourcePath.Substring(1);
      }

      // TODO: Th06 product type
      if (true)
        writer.WriteLine("@{0},{1}", bgmSourcePath, Title);
      else
        writer.WriteLine("@{0},th06,{1}", bgmSourcePath, Title);

      // write extension lines
      writer.WriteLine("#=ProductInfo,Creator,{0}", Creator);
      writer.WriteLine("#=ProductInfo,Prefix,{0}", Prefix);
      writer.WriteLine("#=ProductInfo,ShortName,{0}", ShortName);
      writer.WriteLine("#=ProductInfo,AbbreviatedShortName,{0}", AbbreviatedShortName);
      writer.WriteLine("#=ProductInfo,ShortestName,{0}", ShortestName);
      writer.WriteLine("#=ProductInfo,ReleaseDate,{0:yyyy-MM-dd}", ReleaseDate);
      writer.WriteLine("#=ProductInfo,BgmSourceLength,{0}", (bgmSourceLength == 0L) ? string.Empty : bgmSourceLength.ToString());
      writer.WriteLine("#=ProductInfo,BgmSourceHash,MD5Sum,{0}", Hexadecimals.ToLowerString(bgmSourceHash ?? new byte[] {}));
      writer.WriteLine("#=ProductInfo,BgmSourceIdentificationHash,MD5Sum,{0}", Hexadecimals.ToLowerString(bgmSourceIdentificationHash ?? new byte[] {}));
      writer.WriteLine("#=ProductInfo,THxxBGMProductGuid,{0}", Guid.Empty.Equals(THxxBGMProductGuid) ? string.Empty : THxxBGMProductGuid.ToString("D"));

      // write track line
      foreach (var track in Tracks) {
        writer.WriteLine("{0:x8},{1:x8},{2:x8},{3}", track.IntroOffset, track.IntroLength, track.RepeatLength, track.Title);
      }

      writer.Flush();
    }
  }
}
