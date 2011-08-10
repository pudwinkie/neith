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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

using Smdn.Windows.UserInterfaces.Interop;

namespace Smdn.Windows.UserInterfaces {
  public static class Desktop {
    public static void SetWallpaper(string file, WallpaperStyle style)
    {
      SetWallpaper(file, style, true);
    }

    public static void SetWallpaper(string file, WallpaperStyle style, bool notifyChanged)
    {
      SetWallpaper(file, style, 0, 0, notifyChanged);
    }

    public static void SetWallpaper(string file, int x, int y)
    {
      SetWallpaper(file, x, y);
    }

    public static void SetWallpaper(string file, int x, int y, bool notifyChanged)
    {
      SetWallpaper(file, WallpaperStyle.Tile, x, y, notifyChanged);
    }

    private static void SetWallpaper(string file, WallpaperStyle style, int x, int y, bool notifyChanged)
    {
      if (file == null)
        throw new ArgumentNullException(file);

      using (var regkeyDesktop = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true)) {
        if (regkeyDesktop == null)
          throw new Win32Exception(Marshal.GetLastWin32Error());

        switch (style) {
          case WallpaperStyle.Stretch:
            regkeyDesktop.SetValue("TileWallpaper", "0");
            regkeyDesktop.SetValue("Wallpaperstyle", "2");
            break;
          case WallpaperStyle.Center:
            regkeyDesktop.SetValue("TileWallpaper", "0");
            regkeyDesktop.SetValue("Wallpaperstyle", "0");
            break;
          case WallpaperStyle.Tile:
            regkeyDesktop.SetValue("TileWallpaper", "1");
            regkeyDesktop.SetValue("Wallpaperstyle", "0");
            break;
          default:
            throw ExceptionUtils.CreateNotSupportedEnumValue(style);
        }

        regkeyDesktop.SetValue("WallpaperOriginX", x.ToString());
        regkeyDesktop.SetValue("WallpaperOriginY", y.ToString());
        regkeyDesktop.SetValue("Wallpaper", file);
      }

      if (notifyChanged) {
        var flags = (Environment.OSVersion.Platform == PlatformID.Win32NT)
          ? SPIF.SENDWININICHANGE
          : SPIF.SENDWININICHANGE | SPIF.UPDATEINIFILE;

        if (!user32.SystemParametersInfo(SPI.SETDESKWALLPAPER, 0, file, flags))
          throw new Win32Exception(Marshal.GetLastWin32Error());
      }
    }

    public static void UnsetWallpaper()
    {
      UnsetWallpaper(true);
    }

    public static void UnsetWallpaper(bool notifyChanged)
    {
      SetWallpaper(string.Empty, WallpaperStyle.Tile, notifyChanged);
    }

    public static string GetCurrentWallpaper()
    {
      using (var regkeyDesktop = Registry.CurrentUser.OpenSubKey( @"Control Panel\Desktop", false)) {
        if (regkeyDesktop == null)
          return null;
        else
          return (string)regkeyDesktop.GetValue("Wallpaper");
      }
    }
  }
}
