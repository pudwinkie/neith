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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using Smdn.Imaging;
using Smdn.Mathematics;
using Smdn.Windows.UserInterfaces;

namespace Smdn.Windows.Forms {
  public partial class SelectWallpaperDialog : Form {
    private class ScreenWallpaperView : ScreenView {
      public Wallpaper VirtualScreenWallpaper {
        get { return virtualScreenWallpaper; }
      }

      public Wallpaper[] AllScreenWallpapers {
        get { return allScreenWallpapers; }
      }

      public Wallpaper SelectedScreenWallpaper {
        get
        {
          var index = SelectedScreenIndex;

          if (index == VirtualScreenIndex)
            return virtualScreenWallpaper;
          else
            return allScreenWallpapers[SelectedScreenIndex];
        }
        set
        {
          var index = SelectedScreenIndex;

          if (index == VirtualScreenIndex)
            virtualScreenWallpaper = value;
          else
            allScreenWallpapers[index] = value;

          Refresh();
        }
      }

      public ScreenWallpaperView()
      {
        InitializeWallpaperFiles();
      }

      protected override void OnDisplaySettingsChanged(EventArgs e)
      {
        InitializeWallpaperFiles();

        base.OnDisplaySettingsChanged(e);
      }

      private void RenderWallpaper(Wallpaper wallpaper, Graphics g, Rectangle bounds, bool isSelected)
      {
        if (wallpaper == null)
          return;

        var imageAttrs = new ImageAttributes();

        imageAttrs.SetColorMatrix(new ColorMatrix(new[] {
          new[] {1.0f, 0.0f, 0.0f, 0.0f, 0.0f},
          new[] {0.0f, 1.0f, 0.0f, 0.0f, 0.0f},
          new[] {0.0f, 0.0f, 1.0f, 0.0f, 0.0f},
          new[] {0.0f, 0.0f, 0.0f, isSelected ? 1.0f : 0.25f, 0.0f},
          new[] {0.0f, 0.0f, 0.0f, 0.0f, 0.0f},
        }));

        wallpaper.BackgroundColorNear = Color.FromArgb(isSelected ? 0xff : 0x40, wallpaper.BackgroundColorNear);
        wallpaper.BackgroundColorFar  = Color.FromArgb(isSelected ? 0xff : 0x40, wallpaper.BackgroundColorFar);

        wallpaper.RenderTo(g, bounds, imageAttrs);

        wallpaper.BackgroundColorNear = Color.FromArgb(0xff, wallpaper.BackgroundColorNear);
        wallpaper.BackgroundColorFar  = Color.FromArgb(0xff, wallpaper.BackgroundColorFar);
      }

      protected override void DrawVirtualScreen(Graphics g, Rectangle bounds)
      {
        var isSelected = (SelectedScreen == null);

        g.FillRectangle(Brushes.Black, bounds);

        RenderWallpaper(virtualScreenWallpaper, g, bounds, isSelected);

        var drawBounds = bounds;

        drawBounds.Inflate(VirtualScreenBoxMargin, VirtualScreenBoxMargin);

        using (var p = new Pen(SystemColors.WindowFrame, isSelected ? 3.0f : 1.0f)) {
          p.DashStyle = DashStyle.Dash;
          g.DrawRectangle(p, drawBounds);
        }
      }

      protected override void DrawScreen(Graphics g, int screenIndex, Rectangle bounds)
      {
        var isSelected = SelectedScreen == Screen.AllScreens[screenIndex];

        RenderWallpaper(allScreenWallpapers[screenIndex], g, bounds, isSelected);

        using (var sf = new StringFormat(StringFormat.GenericDefault)) {
          sf.Alignment = StringAlignment.Center;
          sf.LineAlignment = StringAlignment.Center;

          var fontHeight = bounds.Height * 0.6f;

          if (fontHeight < 0.5f)
            fontHeight = 0.5f;

          using (var f = new Font(Control.DefaultFont.FontFamily, fontHeight, FontStyle.Bold, GraphicsUnit.Pixel)) {
            var text = string.Format("{0}", screenIndex + 1);
            var backgroundColor = (allScreenWallpapers[screenIndex] == null)
              ? Color.Black
              : ColorModel.AverageOf(allScreenWallpapers[screenIndex].BackgroundColorNear, allScreenWallpapers[screenIndex].BackgroundColorFar);
            var textColor = (ColorModel.LuminanceOf(backgroundColor) < 0.5)
              ? Color.White
              : Color.Black;

            using (var b = new SolidBrush(Color.FromArgb(isSelected ? 0xff : 0x80, textColor))) {
              g.DrawString(text, f, b, bounds, sf);
            }

            using (var p = new Pen(SystemColors.WindowFrame, isSelected ? 3.0f : 1.0f)) {
              g.DrawRectangle(p, bounds);
            }
          }
        }
      }

      private void InitializeWallpaperFiles()
      {
        Array.Resize(ref allScreenWallpapers, Screen.AllScreens.Length);
      }

      private Wallpaper virtualScreenWallpaper = null;
      private Wallpaper[] allScreenWallpapers = new Wallpaper[] {};
    }

    private class ImageFillStyleDropDownList : FormattingDropDownList<ImageFillStyle> {
      public ImageFillStyleDropDownList()
      {
        Items.Add(ImageFillStyle.Center);
        Items.Add(ImageFillStyle.Fill);
        Items.Add(ImageFillStyle.Fit);
        Items.Add(ImageFillStyle.Tile);
        Items.Add(ImageFillStyle.TileCenter);
        Items.Add(ImageFillStyle.Zoom);

        this.StringFormatter = delegate(ImageFillStyle style) {
          switch (style) {
            case ImageFillStyle.Center:     return "中央";
            case ImageFillStyle.Fill:       return "フルスクリーン";
            case ImageFillStyle.Fit:        return "サイズ変更";
            case ImageFillStyle.Tile:       return "並べる";
            case ImageFillStyle.TileCenter: return "中央から並べる";
            case ImageFillStyle.Zoom:       return "ズーム";
            default: return style.ToString();
          }
        };
      }
    }

    private class DirectoryWallapaperSelectionOrderDropDownList : FormattingDropDownList<DirectoryWallpaper.SelectionOrder> {
      public DirectoryWallapaperSelectionOrderDropDownList()
      {
        Items.Add(DirectoryWallpaper.SelectionOrder.ByFileName);
        Items.Add(DirectoryWallpaper.SelectionOrder.ByCreationTime);
        Items.Add(DirectoryWallpaper.SelectionOrder.ByRandom);

        this.StringFormatter = delegate(DirectoryWallpaper.SelectionOrder order) {
          switch (order) {
            case DirectoryWallpaper.SelectionOrder.ByFileName:      return "ファイル名";
            case DirectoryWallpaper.SelectionOrder.ByCreationTime:  return "作成日時";
            case DirectoryWallpaper.SelectionOrder.ByRandom:        return "ランダム";
            default: return order.ToString();
          }
        };
      }
    }

    private static Regex stringToTimeSpanRegex = new Regex(@"\s*((?<hours>\d+)\s*(時間|:))?\s*(?<mins>\d+)\s*(分|:)\s*(?<secs>\d+)\s*(秒)?", RegexOptions.Singleline);

    public SelectWallpaperDialog()
    {
      InitializeComponent();
      Initialize();
    }

    private void Initialize()
    {
      screenWallpaperView.SelectedScreenChanged += screenWallpaperView_SelectedScreenChanged;
      screenWallpaperView.ScreenToStringConverter = ConvertScreenToString;

      tabControlWallpaper.SelectedIndexChanged += tabControlWallpaper_SelectedIndexChanged;

      labelBackgroundColorNear.Click += labelBackgroundColor_Click;
      labelBackgroundColorFar.Click += labelBackgroundColor_Click;

      labelBackgroundColorNear.BackColorChanged += labelBackgroundColor_BackColorChanged;
      labelBackgroundColorFar.BackColorChanged += labelBackgroundColor_BackColorChanged;

      numericUpDownGradientDirection.ValueChanged += numericUpDownGradientDirection_ValueChanged;

      buttonChangeFile.Click += buttonChangeFile_Click;
      buttonChangeDirectory.Click += buttonChangeDirectory_Click;

      fileSelectionOrderDropDownList.SelectedIndexChanged += fileSelectionOrderDropDownList_SelectedIndexChanged;

      imageFillStyleDropDownList.SelectedIndexChanged += imageFillStyleDropDownList_SelectedIndexChanged;

      timeSpanUpdateChangeInterval.StringToTimeSpanConverter = delegate(string val) {
        var match = stringToTimeSpanRegex.Match(val);

        if (!match.Success)
          return TimeSpan.Parse(val);

        var hours = 0;
        var mins = 0;
        var secs = 0;

        if (match.Groups["hours"].Success)
          hours = int.Parse(match.Groups["hours"].Value);
        if (match.Groups["mins"].Success)
          mins = int.Parse(match.Groups["mins"].Value);
        if (match.Groups["secs"].Success)
          secs = int.Parse(match.Groups["secs"].Value);

        return new TimeSpan(hours, mins, secs);
      };
      timeSpanUpdateChangeInterval.TimeSpanToStringConverter = delegate(TimeSpan val) {
        return string.Format("{0}時間 {1:D2}分 {2:D2}秒", (int)Math.Floor(val.TotalHours), val.Minutes, val.Seconds);
      };

      checkBoxDisableChangeInterval.CheckedChanged += delegate {
        timeSpanUpdateChangeInterval.Enabled = !checkBoxDisableChangeInterval.Checked;
      };

      screenWallpaperView.SelectedScreen = null;
      tabControlWallpaper.SelectedIndex = 0;

      timeSpanUpdateChangeInterval.Text = timeSpanUpdateChangeInterval.TimeSpanToStringConverter(TimeSpan.Zero);

      checkBoxDisableChangeInterval.Checked = false;

      Reset();

      screenWallpaperView_SelectedScreenChanged(null, EventArgs.Empty);
      tabControlWallpaper_SelectedIndexChanged(null, EventArgs.Empty);
    }

    public void Reset()
    {
      labelBackgroundColorNear.BackColor = Color.Black;
      labelBackgroundColorFar.BackColor = Color.Black;
      numericUpDownGradientDirection.Value = 0.0m;
      imageFillStyleDropDownList.SelectedItem = ImageFillStyle.Fit;

      textBoxFileName.Text = null;
      textBoxDirectoryName.Text = null;
      fileSelectionOrderDropDownList.SelectedItem = DirectoryWallpaper.SelectionOrder.ByFileName;
    }

    private void tabControlWallpaper_SelectedIndexChanged(object sender, EventArgs e)
    {
      tabControlWallpaper.SelectedTab.Controls.Add(panelCommonConfiguration);

      panelCommonConfiguration.Top = panelCommonConfiguration.Parent.ClientSize.Height - panelCommonConfiguration.Height;

      imageFillStyleDropDownList.Enabled = (tabControlWallpaper.SelectedTab != tabPageWallpaper);

      if (lockChanged)
        return;

      try {
        lockChanged = true;

        var selectedWallpaper = CreateSelectedTabPageWallpaper();

        if (selectedWallpaper is DirectoryWallpaper)
          (selectedWallpaper as DirectoryWallpaper).SelectNextFile();

        screenWallpaperView.SelectedScreenWallpaper = selectedWallpaper;
      }
      finally {
        lockChanged = false;
      }
    }

    private Wallpaper CreateSelectedTabPageWallpaper()
    {
      Wallpaper wallpaper = null;

      var backColorNear = labelBackgroundColorNear.BackColor;
      var backColorFar = labelBackgroundColorFar.BackColor;
      var gradientDirection = Radian.FromDegree((float)numericUpDownGradientDirection.Value);
      var drawStyle = imageFillStyleDropDownList.SelectedItem;

      if (tabControlWallpaper.SelectedTab == tabPageFileWallpaper) {
        wallpaper = new FileWallpaper(textBoxFileName.Text,
                                      backColorNear,
                                      backColorFar,
                                      gradientDirection,
                                      drawStyle);
      }
      else if (tabControlWallpaper.SelectedTab == tabPageDirectoryWallpaper) {
        wallpaper = new DirectoryWallpaper(textBoxDirectoryName.Text,
                                           fileSelectionOrderDropDownList.SelectedItem,
                                           backColorNear,
                                           backColorFar,
                                           gradientDirection,
                                           drawStyle);
      }
      else if (tabControlWallpaper.SelectedTab == tabPageWallpaper) {
        wallpaper = new Wallpaper(backColorNear,
                                  backColorFar,
                                  gradientDirection);
        wallpaper.DrawStyle = drawStyle;
      }

      return wallpaper;
    }

    private void screenWallpaperView_SelectedScreenChanged(object sender, EventArgs e)
    {
      labelScreenWallpaper.Text = string.Format("{0}の壁紙", ConvertScreenToString(screenWallpaperView.SelectedScreen));

      if (lockChanged)
        return;

      try {
        lockChanged = true;

        Reset();

        var wallpaper = screenWallpaperView.SelectedScreenWallpaper;

        if (wallpaper == null) {
          labelBackgroundColorNear.BackColor = Color.Black;
          labelBackgroundColorFar.BackColor = Color.Black;
          numericUpDownGradientDirection.Value = 0.0m;
          textBoxFileName.Text = null;

          tabControlWallpaper.SelectedTab = tabPageWallpaper;
        }
        else {
          labelBackgroundColorNear.BackColor = wallpaper.BackgroundColorNear;
          labelBackgroundColorFar.BackColor = wallpaper.BackgroundColorFar;
          numericUpDownGradientDirection.Value = (decimal)wallpaper.GradientDirection.Regularized.ToDegree();
          imageFillStyleDropDownList.SelectedItem = wallpaper.DrawStyle;

          if (wallpaper is DirectoryWallpaper) {
            var directoryWallpaper = wallpaper as DirectoryWallpaper;

            textBoxDirectoryName.Text = directoryWallpaper.Directory;
            fileSelectionOrderDropDownList.SelectedItem = directoryWallpaper.FileSelectionOrder;

            tabControlWallpaper.SelectedTab = tabPageDirectoryWallpaper;
          }
          else if (wallpaper is FileWallpaper) {
            textBoxFileName.Text = (wallpaper as FileWallpaper).File;

            tabControlWallpaper.SelectedTab = tabPageFileWallpaper;
          }
          else {
            tabControlWallpaper.SelectedTab = tabPageWallpaper;
          }
        }
      }
      finally {
        lockChanged = false;
      }
    }

    private void labelBackgroundColor_Click(object sender, EventArgs e)
    {
      var label = sender as Label;

      using (var dialog = new SelectColorDialog()) {
        dialog.RgbColor = label.BackColor;

        if (dialog.ShowDialog() != DialogResult.OK)
          return;

        label.BackColor = dialog.RgbColor;
      }
    }

    private void labelBackgroundColor_BackColorChanged(object sender, EventArgs e)
    {
      var label = sender as Label;

      if (ColorModel.LuminanceOf(label.BackColor) < 0.5)
        label.ForeColor = Color.White;
      else
        label.ForeColor = Color.Black;

      label.Text = ColorTranslator.ToHtml(label.BackColor);

      if (lockChanged)
        return;

      try {
        lockChanged = true;

        var wallpaper = screenWallpaperView.SelectedScreenWallpaper;

        if (wallpaper == null) {
          wallpaper = CreateSelectedTabPageWallpaper();
          screenWallpaperView.SelectedScreenWallpaper = wallpaper;
        }

        if (label == labelBackgroundColorNear)
          wallpaper.BackgroundColorNear = label.BackColor;
        else
          wallpaper.BackgroundColorFar = label.BackColor;

        screenWallpaperView.Refresh();
      }
      finally {
        lockChanged = false;
      }
    }

    private void numericUpDownGradientDirection_ValueChanged(object sender, EventArgs e)
    {
      if (lockChanged)
        return;

      try {
        lockChanged = true;

        var wallpaper = screenWallpaperView.SelectedScreenWallpaper;

        if (wallpaper == null) {
          wallpaper = CreateSelectedTabPageWallpaper();
          screenWallpaperView.SelectedScreenWallpaper = wallpaper;
        }

        wallpaper.GradientDirection = Radian.FromDegree((float)numericUpDownGradientDirection.Value);

        screenWallpaperView.Refresh();
      }
      finally {
        lockChanged = false;
      }
    }

    private void fileSelectionOrderDropDownList_SelectedIndexChanged(object sender, EventArgs e)
    {
      var wallpaper = screenWallpaperView.SelectedScreenWallpaper as DirectoryWallpaper;

      if (wallpaper == null)
        return;

      wallpaper.FileSelectionOrder = fileSelectionOrderDropDownList.SelectedItem;
      wallpaper.SelectNextFile();

      screenWallpaperView.Refresh();
    }

    private void imageFillStyleDropDownList_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (lockChanged)
        return;

      try {
        var wallpaper = screenWallpaperView.SelectedScreenWallpaper;

        if (wallpaper == null) {
          wallpaper = CreateSelectedTabPageWallpaper();
          screenWallpaperView.SelectedScreenWallpaper = wallpaper;
        }

        wallpaper.DrawStyle = imageFillStyleDropDownList.SelectedItem;

        screenWallpaperView.Refresh();
      }
      finally {
        lockChanged = false;
      }
    }

    private void buttonChangeFile_Click(object sender, EventArgs e)
    {
      var wallpaper = screenWallpaperView.SelectedScreenWallpaper as FileWallpaper;

      using (var dialog = OpenFileDialogExtensions.CreateOpenImageDialog(wallpaper.File)) {
        dialog.RestoreDirectory = true;

        if (dialog.ShowDialog(this) == DialogResult.OK) {
          wallpaper.File = dialog.FileName;
          textBoxFileName.Text = dialog.FileName;

          screenWallpaperView.Refresh();
        }
      }
    }

    private void buttonChangeDirectory_Click(object sender, EventArgs e)
    {
      using (var dialog = new FolderBrowserDialog()) {
        var wallpaper = screenWallpaperView.SelectedScreenWallpaper as DirectoryWallpaper;

        dialog.Description = string.Empty;
        dialog.SelectedPath = wallpaper.Directory;

        if (dialog.ShowDialog(this) == DialogResult.OK) {
          wallpaper.Directory = dialog.SelectedPath;
          textBoxDirectoryName.Text = dialog.SelectedPath;

          wallpaper.SelectNextFile();

          screenWallpaperView.Refresh();
        }
      }
    }

    private string ConvertScreenToString(Screen screen)
    {
      if (screen == null)
        return "デスクトップ全体";
      else
        return string.Format("スクリーン{0} ({1}x{2})", Array.IndexOf(Screen.AllScreens, screen), screen.Bounds.Width, screen.Bounds.Height);
    }

    private bool lockChanged = false;
  }
}
