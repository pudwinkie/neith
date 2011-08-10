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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Microsoft.Win32;

namespace Smdn.Windows.Forms {
  public class ScreenView : Control {
    private class ScreenDropDownListItem {
      public Screen Screen {
        get; private set;
      }

      public ScreenDropDownListItem(ScreenView owner, Screen screen)
      {
        this.owner = owner;
        this.Screen = screen;
      }

      public override string ToString()
      {
        if (owner.ScreenToStringConverter == null) {
          var index = Array.IndexOf(Screen.AllScreens, this.Screen);

          if (0 <= index)
            return string.Format("Screen {0} ({1}x{2})", index + 1, this.Screen.Bounds.Width, this.Screen.Bounds.Height);
          else
            return "Desktop";
        }
        else {
          return owner.ScreenToStringConverter(Screen);
        }
      }

      private ScreenView owner;
    }

    public EventHandler SelectedScreenChanged;
    public EventHandler DisplaySettingsChanged;

    public const int VirtualScreenIndex = -1;

    public Screen SelectedScreen {
      get { return screenDropDownList.SelectedItem.Screen; }
      set
      {
        if (screenDropDownList.SelectedItem.Screen == value)
          return;

        foreach (var item in screenDropDownList.Items) {
          if (item.Screen == value) {
            screenDropDownList.SelectedItem = item;
            break;
          }
        }
      }
    }

    public int SelectedScreenIndex {
      get
      {
        if (SelectedScreen == null)
          return VirtualScreenIndex;
        else
          return Array.IndexOf(Screen.AllScreens, SelectedScreen);
      }
      set
      {
        if (value == VirtualScreenIndex)
          SelectedScreen = null;
        else
          SelectedScreen = Screen.AllScreens[value];
      }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [LocalizableAttribute(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Converter<Screen, string> ScreenToStringConverter {
      get; set;
    }

    protected Rectangle VirtualScreenBounds {
      get; private set;
    }

    protected Rectangle VirtualScreenBoxBounds {
      get; private set;
    }

    protected float ScreenToClientScale {
      get; set;
    }

    protected const int VirtualScreenBoxMargin = 6;

    public ScreenView()
    {
      this.SetStyle(ControlStyles.DoubleBuffer, true);
      this.SetStyle(ControlStyles.UserPaint, true);
      this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);

      base.ResizeRedraw = true;

      screenDropDownList = new DropDownList<ScreenDropDownListItem>();
      screenDropDownList.Location = new Point(0, 0);
      screenDropDownList.Width = this.Width;
      screenDropDownList.SelectedValueChanged += screenDropDownList_SelectedValueChanged;

      Controls.Add(screenDropDownList);
      components.Add(screenDropDownList);

      SystemEvents.DisplaySettingsChanged += HandleDisplaySettingsChanged;

      UpdateScreenDropDownList();
      UpdateScreenBounds();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing) {
        if (components != null)
          components.Dispose();

        SystemEvents.DisplaySettingsChanged -= HandleDisplaySettingsChanged;
      }

      base.Dispose(disposing);
    }

    private void HandleDisplaySettingsChanged(object sender, EventArgs e)
    {
      UpdateScreenDropDownList();

      OnDisplaySettingsChanged(EventArgs.Empty);
    }

    protected virtual void OnDisplaySettingsChanged(EventArgs e)
    {
      UpdateScreenBounds();

      var ev = this.DisplaySettingsChanged;

      if (ev != null)
        ev(this, e);
    }

    private void screenDropDownList_SelectedValueChanged(object sender, EventArgs e)
    {
      OnSelectedScreenChanged(EventArgs.Empty);

      Refresh();
    }

    private void UpdateScreenDropDownList()
    {
      screenDropDownList.Items.Clear();
      screenDropDownList.Items.Add(new ScreenDropDownListItem(this, null)); // virtual screen

      foreach (var screen in Screen.AllScreens) {
        screenDropDownList.Items.Add(new ScreenDropDownListItem(this, screen));
      }

      screenDropDownList.SelectedIndex = 0;
    }

    private void UpdateScreenBounds()
    {
      VirtualScreenBounds = Rectangle.Empty;

      foreach (var screen in Screen.AllScreens) {
        VirtualScreenBounds = Rectangle.Union(VirtualScreenBounds, screen.Bounds);
      }
    }

    protected override void OnSizeChanged(EventArgs e)
    {
      var clientSize = ClientSize;

      clientSize.Width  = clientSize.Width  - 2 * VirtualScreenBoxMargin;
      clientSize.Height = clientSize.Height - 2 * VirtualScreenBoxMargin - (screenDropDownList.Height + 5);

      var aspectScreen = VirtualScreenBounds.Height / (float)VirtualScreenBounds.Width;
      var aspectControl = clientSize.Height / (float)clientSize.Width;

      ScreenToClientScale = (aspectScreen < aspectControl)
        ? clientSize.Width / (float)VirtualScreenBounds.Width
        : clientSize.Height / (float)VirtualScreenBounds.Height;

      var w = (int)((VirtualScreenBounds.Width) * ScreenToClientScale);
      var h = (int)((VirtualScreenBounds.Height) * ScreenToClientScale);

      VirtualScreenBoxBounds = new Rectangle(VirtualScreenBoxMargin + (clientSize.Width - w) / 2,
                                             VirtualScreenBoxMargin + (clientSize.Height - h) / 2 + (screenDropDownList.Height + 5),
                                             w,
                                             h);

      screenBoxBounds.Clear();

      foreach (var screen in Screen.AllScreens) {
        screenBoxBounds.Add(screen, new Rectangle(ConvertToClient(new Point(screen.Bounds.X - VirtualScreenBounds.X, screen.Bounds.Y - VirtualScreenBounds.Y)),
                                                  ConvertToClient(screen.Bounds.Size)));
      }

      screenDropDownList.Location = new Point(0, 0);
      screenDropDownList.Width = this.Width;

      base.OnSizeChanged(e);
    }

    protected virtual void DrawVirtualScreen(Graphics g, Rectangle bounds)
    {
      var isSelected = SelectedScreen == null;
      var drawBounds = bounds;

      drawBounds.Inflate(VirtualScreenBoxMargin, VirtualScreenBoxMargin);

      g.FillRectangle(isSelected ? SystemBrushes.Highlight : SystemBrushes.Window, drawBounds);

      using (var p = new Pen(SystemColors.WindowFrame, isSelected ? 3.0f : 1.0f)) {
        p.DashStyle = DashStyle.Dash;
        g.DrawRectangle(p, drawBounds);
      }
    }

    protected virtual void DrawScreen(Graphics g, int screenIndex, Rectangle bounds)
    {
      var isSelected = SelectedScreen == Screen.AllScreens[screenIndex];

      g.FillRectangle(isSelected ? SystemBrushes.Highlight : SystemBrushes.Window, bounds);

      using (var sf = new StringFormat(StringFormat.GenericDefault)) {
        sf.Alignment = StringAlignment.Center;
        sf.LineAlignment = StringAlignment.Center;

        using (var f = new Font(Control.DefaultFont.FontFamily, bounds.Height * 0.6f, FontStyle.Bold, GraphicsUnit.Pixel)) {
          var text = (screenIndex + 1).ToString();

          g.DrawString(text, f, isSelected ? SystemBrushes.HighlightText : SystemBrushes.WindowText, bounds, sf);

          using (var p = new Pen(SystemColors.WindowFrame, isSelected ? 2.0f : 1.0f)) {
            g.DrawRectangle(p, bounds);
          }
        }
      }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      try {
        e.Graphics.Clear(SystemColors.Control);

        DrawVirtualScreen(e.Graphics, VirtualScreenBoxBounds);

        foreach (var pair in screenBoxBounds) {
          DrawScreen(e.Graphics, Array.IndexOf(Screen.AllScreens, pair.Key), pair.Value);
        }
      }
      finally {
        base.OnPaint(e);
      }
    }

    protected override void OnClick(EventArgs e)
    {
      SelectedScreen = FindScreenFromPoint(PointToClient(Cursor.Position));

      base.OnClick(e);
    }

    protected virtual void OnSelectedScreenChanged(EventArgs e)
    {
      var ev = this.SelectedScreenChanged;

      if (ev != null)
        ev(this, e);
    }

    protected bool IsInVirtualScreen(Point pt)
    {
      return VirtualScreenBoxBounds.Contains(pt);
    }

    protected Screen FindScreenFromPoint(Point pt)
    {
      foreach (var pair in screenBoxBounds) {
        if (pair.Value.Contains(pt))
          return pair.Key;
      }

      return null;
    }

    protected Rectangle ConvertToScreen(Rectangle rect)
    {
      return new Rectangle(ConvertToScreen(rect.Location), ConvertToScreen(rect.Size));
    }

    protected Point ConvertToScreen(Point pt)
    {
      return new Point((int)((pt.X - VirtualScreenBoxBounds.Left) / ScreenToClientScale),
                       (int)((pt.Y - VirtualScreenBoxBounds.Top) / ScreenToClientScale));
    }

    protected Size ConvertToScreen(Size sz)
    {
      return new Size((int)(sz.Width / ScreenToClientScale),
                      (int)(sz.Height / ScreenToClientScale));
    }

    protected Rectangle ConvertToClient(Rectangle rect)
    {
      return new Rectangle(ConvertToClient(rect.Location), ConvertToClient(rect.Size));
    }

    protected Point ConvertToClient(Point pt)
    {
      return new Point(VirtualScreenBoxBounds.X + (int)(pt.X * ScreenToClientScale),
                       VirtualScreenBoxBounds.Y + (int)(pt.Y * ScreenToClientScale));
    }

    protected Size ConvertToClient(Size sz)
    {
      return new Size((int)(sz.Width * ScreenToClientScale),
                      (int)(sz.Height * ScreenToClientScale));
    }

    private Container components = new Container();
    private Dictionary<Screen, Rectangle> screenBoxBounds = new Dictionary<Screen, Rectangle>();
    private DropDownList<ScreenDropDownListItem> screenDropDownList;
  }
}
