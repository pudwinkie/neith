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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

using Microsoft.Win32;

namespace Smdn.Windows.UserInterfaces {
  public static class WallpaperRenderer {
    /*
     * class members
     */
    public static string WallpaperSavePath {
      get { return wallpaperSavePath; }
      set
      {
        if (wallpaperSavePath != value) {
          if (File.Exists(wallpaperSavePath))
            File.Delete(wallpaperSavePath);
          wallpaperSavePath = value;
        }
      }
    }

    public static bool RefreshOnDisplaySettingChanged {
      get { return refreshOnDisplaySettingChanged; }
      set
      {
        if (refreshOnDisplaySettingChanged != value) {
          refreshOnDisplaySettingChanged = value;
          if (refreshOnDisplaySettingChanged) {
            SystemEvents.DisplaySettingsChanged += HandleDisplaySettingsChanged;
            Application.ApplicationExit += HandleApplicationExit;
          }
          else {
            SystemEvents.DisplaySettingsChanged -= HandleDisplaySettingsChanged;
            Application.ApplicationExit -= HandleApplicationExit;
          }
        }
      }
    }

    public static InterpolationMode InterpolationMode {
      get; set;
    }

    public static Wallpaper WholeScreenWallpaper {
      get; private set;
    }

    public static Wallpaper[] Wallpapers {
      get
      {
        if (perScreenWallpapers.Length != Screen.AllScreens.Length)
          Array.Resize(ref perScreenWallpapers, Screen.AllScreens.Length);

        return perScreenWallpapers;
      }
    }

    static WallpaperRenderer()
    {
      InterpolationMode = InterpolationMode.Default;
    }

    private static void HandleDisplaySettingsChanged(object sender, EventArgs e)
    {
      Render();
    }

    private static void HandleApplicationExit(object sender, EventArgs e)
    {
      // static event handler must be detatched
      SystemEvents.DisplaySettingsChanged -= HandleDisplaySettingsChanged;
      Application.ApplicationExit -= HandleApplicationExit;
    }

    public static void Render()
    {
      Render(true);
    }

    private static void Render(bool notifyChanged)
    {
      // wallpaper file must be specified
      if (wallpaperSavePath == null)
        return;

      using (var rendered = RenderWallpapers()) {
        rendered.Save(wallpaperSavePath, ImageFormat.Bmp);
      }

      Desktop.SetWallpaper(wallpaperSavePath, 0, 0, notifyChanged);
    }

    private static Bitmap RenderWallpapers()
    {
      var virtualScreenBounds = Rectangle.Empty;

      foreach (var screen in Screen.AllScreens) {
        virtualScreenBounds = Rectangle.Union(virtualScreenBounds, screen.Bounds);
      }

      var virtualScreenBitmap = new Bitmap(virtualScreenBounds.Width, virtualScreenBounds.Height, PixelFormat.Format24bppRgb);

      using (var virtualScreenGraphics = Graphics.FromImage(virtualScreenBitmap)) {
        virtualScreenGraphics.InterpolationMode = InterpolationMode;
        virtualScreenGraphics.TranslateTransform(-virtualScreenBounds.X, -virtualScreenBounds.Y);

        if (WholeScreenWallpaper == null)
          virtualScreenGraphics.Clear(Color.Black);
        else
          WholeScreenWallpaper.RenderTo(virtualScreenGraphics, virtualScreenBounds);

        for (var index = 0; index < Screen.AllScreens.Length; index++) {
          if (Wallpapers[index] != null)
            Wallpapers[index].RenderTo(virtualScreenGraphics, Screen.AllScreens[index].Bounds);
        }
      }

      return virtualScreenBitmap;
    }

    private static string wallpaperSavePath = null;
    private static bool refreshOnDisplaySettingChanged = false;
    private static Wallpaper[] perScreenWallpapers = new Wallpaper[] {};
  }
}
