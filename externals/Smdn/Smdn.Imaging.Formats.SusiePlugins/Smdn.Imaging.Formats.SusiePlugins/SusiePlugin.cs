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
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.Win32;

using Smdn.IO;
using Smdn.Interop;

using Smdn.Imaging.Formats.AtoBConverterPlugins;

namespace Smdn.Imaging.Formats.SusiePlugins {
  /*
   * specification:
   *   http://www.digitalpad.co.jp/~takechin/archives/spi32008.lzh (spi_api.txt; Susie 32bit Plug-in 仕様 rev5)
   * 
   *   export plugin spec:
   *     http://www.asahi-net.or.jp/~KH4S-SMZ/spi/abc/exp_api.txt
   * 
   * reference:
   *   http://www.asahi-net.or.jp/~kh4s-smz/spi/make_spi.html
   *   http://d.hatena.ne.jp/myugaru/20071217/1197934740
   *   http://d.hatena.ne.jp/myugaru/20071221/1198248651
   *   http://d.hatena.ne.jp/myugaru/20080109
   * 
   *   http://home.netyou.jp/cc/susumu/progSusie.html
   *   http://www.asahi-net.or.jp/~KH4S-SMZ/spi/note/spiapimp.html
   */
  public abstract class SusiePlugin : IImageCodec, IDisposable {
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)] public delegate int SpiGetPluginInfo(int infono, [MarshalAs(UnmanagedType.LPStr)] StringBuilder buf, int buflen);
    public delegate int SpiProgressCallback(int nNum, int nDenom, IntPtr lData);

    /*
     * class members
     */
    public const string PluginDefaultExtension = ".spi";
    public const string AtoBConverterExportPluginDefaultExtension = ".xpi";

    private static Dictionary<IntPtr, SusiePlugin> loadedPlugins = new Dictionary<IntPtr, SusiePlugin>();

    public static IEnumerable<SusiePlugin> GetLoadedPlugins()
    {
      lock (((System.Collections.ICollection)loadedPlugins).SyncRoot) {
        return new List<SusiePlugin>(loadedPlugins.Values);
      }
    }

    public static IEnumerable<SusiePlugin> LoadInstalled()
    {
      return LoadInstalled(true);
    }

    public static IEnumerable<SusiePlugin> LoadInstalled(bool ignoreUnsupportedOrInvalid)
    {
      if (!Runtime.IsRunningOnWindows)
        return new SusiePlugin[] {};

      var plugins = new List<SusiePlugin>();

      // search susie installed directory
      using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Takechin\Susie\Plug-in", false)) {
        var path = (key == null) ? null : (string)key.GetValue("Path");

        if (path != null)
          plugins.AddRange(SearchAndLoadFromDirectory(path));
      }

      // search entry assembly directory
      plugins.AddRange(SearchAndLoadFromDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)));

      return plugins;
    }

    public static IEnumerable<SusiePlugin> SearchAndLoadFromDirectory(string directory)
    {
      return SearchAndLoadFromDirectory(directory, true);
    }

    public static IEnumerable<SusiePlugin> SearchAndLoadFromDirectory(string directory, bool ignoreUnsupportedOrInvalid)
    {
      if (!Runtime.IsRunningOnWindows)
        return new SusiePlugin[] {};

      if (!Directory.Exists(directory))
        throw new DirectoryNotFoundException();

      var plugins = new List<SusiePlugin>();

      foreach (var pluginFile in DirectoryUtils.GetFiles(directory, SearchOption.TopDirectoryOnly, delegate(string file) {
        return
          PathUtils.AreExtensionEqual(file, PluginDefaultExtension) ||
          PathUtils.AreExtensionEqual(file, AtoBConverterExportPluginDefaultExtension);
      })) {
        try {
          plugins.Add(LoadFrom(pluginFile));
        }
        catch (NotSupportedException) {
          if (!ignoreUnsupportedOrInvalid)
            throw;
        }
        catch (NotImplementedException) {
          if (!ignoreUnsupportedOrInvalid)
            throw;
        }
        catch (Win32Exception) {
          if (!ignoreUnsupportedOrInvalid)
            throw;
        }
        catch (FunctionNotFoundException) {
          if (!ignoreUnsupportedOrInvalid)
            throw;
        }
      }

      return plugins;
    }

    public static SusiePlugin LoadFrom(string pluginFile)
    {
      if (!Runtime.IsRunningOnWindows)
        throw new PlatformNotSupportedException();

      if (pluginFile == null)
        throw new ArgumentNullException("pluginFile");
      else if (!File.Exists(pluginFile))
        throw new FileNotFoundException("file not found", pluginFile);

      try {
        DynamicLinkLibrary library;

        try {
          library = new DynamicLinkLibrary(pluginFile);
        }
        catch (Win32Exception) {
          throw;
        }

        lock (((System.Collections.ICollection)loadedPlugins).SyncRoot) {
          var handle = library.Handle;

          if (loadedPlugins.ContainsKey(handle)) {
            library.Free();
            return loadedPlugins[handle];
          }
        }

        var getPluginInfo = library.GetFunction<SpiGetPluginInfo>("GetPluginInfo");

        string apiVersionString;
        var apiVersion = GetPluginApiVersion(getPluginInfo, out apiVersionString);

        if ((int)apiVersion == 0)
          throw new NotSupportedException(string.Format("invalid plugin version: {0}", apiVersionString));

        SusiePlugin ret = null;

        switch (apiVersion & SusiePluginApiVersion.VersionMask) {
          case SusiePluginApiVersion.Version00: {
            switch (apiVersion & SusiePluginApiVersion.FilterTypeMask) {
              case SusiePluginApiVersion.ImportFilter:     ret = new SusieImportFilterPlugin(library, getPluginInfo, apiVersion); break;
              case SusiePluginApiVersion.ExportFilter:     ret = new SusieExportFilterPlugin(library, getPluginInfo, apiVersion); break;
              case SusiePluginApiVersion.ArchiveExtractor: ret = new SusieArchiveExtractorPlugin(library, getPluginInfo, apiVersion); break;
            }
            break;
          }
          case SusiePluginApiVersion.VersionT0: {
            switch (apiVersion & SusiePluginApiVersion.FilterTypeMask) {
              case SusiePluginApiVersion.ExportFilter:     ret = new AtoBConverterExportPlugin(library, getPluginInfo, apiVersion); break;
            }
            break;
          }
        }

        if (ret == null)
          throw new NotSupportedException(string.Format("unsupported plugin version: {0}", apiVersionString));

        lock (((System.Collections.ICollection)loadedPlugins).SyncRoot) {
          loadedPlugins.Add(ret.Library.Handle, ret);
        }

        return ret;
      }
      catch (FunctionNotFoundException) {
        throw;
      }
    }

    private static string GetPluginInfo(SpiGetPluginInfo getPluginInfo, int infono)
    {
      var sb = new StringBuilder(0x100, 0x100); // XXX: capacity

      var ret = getPluginInfo(infono, sb, sb.MaxCapacity);

      if (ret == 0)
        return null;
      else
        return sb.ToString();
    }

    private static SusiePluginApiVersion GetPluginApiVersion(SpiGetPluginInfo getPluginInfo, out string apiVersionString)
    {
      apiVersionString = GetPluginInfo(getPluginInfo, 0); // SPI-SPEC: 0 = Plugin API version

      var ret = (SusiePluginApiVersion)0;

      if (apiVersionString == null || apiVersionString.Length != 4)
        return ret;

      for (var index = 0; index < 4; index++) {
        ret |= (SusiePluginApiVersion)((byte)apiVersionString[index] << ((3 - index) * 8));
      }

      return ret;
    }

    /*
     * instance members
     */
    protected DynamicLinkLibrary Library {
      get { CheckDisposed(); return library; }
    }

    public SusiePluginApiVersion ApiVersion {
      get; private set;
    }

    public string Name {
      get; private set;
    }

    public FileType[] FileTypes {
      get; private set;
    }

    /*
     * IImageCodec members
     */
    private Guid guid = Guid.Empty;
    private MimeType mimeType = null;

    Guid IImageCodec.Guid {
      get
      {
        if (guid == Guid.Empty) {
          var url = new Uri(string.Format("http://smdn.jp/works/libs/Smdn.Imaging/uuid/{0}", Name));

          guid = Uuid.CreateNameBased(url, UuidVersion.NameBasedMD5Hash).ToGuid();
        }

        return guid;
      }
    }

    string IImageCodec.Name {
      get { return Name; }
    }

    string[] IImageCodec.Extensions {
      get { return Array.ConvertAll(FileTypes, delegate(FileType type) { return type.Extension; }); }
    }

    public MimeType MimeType {
      get
      {
        if (mimeType == null) {
          foreach (var fileType in FileTypes) {
            mimeType = MimeType.GetMimeTypeByExtension(fileType.Extension);
            if (mimeType != null)
              break;
          }

          if (mimeType == null)
            mimeType = MimeType.ApplicationOctetStream;
        }

        return mimeType;
      }
    }

    protected SusiePlugin(DynamicLinkLibrary library, SpiGetPluginInfo getPluginInfo, SusiePluginApiVersion apiVersion)
    {
      this.library = library;
      this.getPluginInfo = getPluginInfo;
      this.ApiVersion = apiVersion;

      this.Name = GetPluginInfo(1); // SPI-SPEC: 1 = Plugin name, version info, copyright

      // SPI-SPEC:
      //   2n + 2 = extension
      //   2n + 3 = file type name
      // XPI-SPEC:
      //   2 = file type name
      //   3 = extension
      var fileTypes = new List<FileType>();

      for (var infono = 2;;) {
        var extension = GetPluginInfo(infono++);

        if (extension == null)
          break;
        else if (extension.StartsWith("*."))
          // SPI-SPEC: extension contain wildcards
          extension = extension.Substring(1); // '*.ext' -> '.ext'

        var description = GetPluginInfo(infono++);

        if (description == null)
          break;

        if ((ApiVersion & SusiePluginApiVersion.VersionMask) == SusiePluginApiVersion.VersionT0) {
          // XPI-SPEC
          var temp = extension;
          extension = description;
          description = temp;
        }

        fileTypes.Add(new FileType(description, extension));
      }

      this.FileTypes = fileTypes.ToArray();
    }

    ~SusiePlugin()
    {
      Dispose(false);
    }

    void IDisposable.Dispose()
    {
      Close();
    }

    public void Close()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing) {
        if (library != null) {
          lock (((System.Collections.ICollection)loadedPlugins).SyncRoot) {
            loadedPlugins.Remove(library.Handle);
          }

          library.Free();
          library = null;
        }
      }
    }

    public string GetPluginInfo(int infono)
    {
      CheckDisposed();

      return GetPluginInfo(getPluginInfo, infono);
    }

    internal void CheckDisposed()
    {
      if (library == null)
        throw new ObjectDisposedException(GetType().FullName);
    }

    public override string ToString()
    {
      return Name;
    }

    private DynamicLinkLibrary library;
    private SpiGetPluginInfo getPluginInfo;
  }
}
