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
using System.Drawing.Imaging;
using System.Windows.Forms;

using Smdn.Imaging.Interop;
using Smdn.Windows.Forms.Interop;

using ImagingInterop = Smdn.Imaging.Interop;
using FormsInterop = Smdn.Windows.Forms.Interop;

namespace Smdn.Windows.Forms {
  /*
   * Window Features - Layered Windows: http://msdn.microsoft.com/en-us/library/ms632599(VS.85).aspx#layered
   * Using Windows - Using Layered Windows: http://msdn.microsoft.com/en-us/library/ms632598(VS.85).aspx#layered_win
   * UpdateLayeredWindow Function: http://msdn.microsoft.com/en-us/library/ms633556(VS.85).aspx]
   * SetLayeredWindowAttributes Function: http://msdn.microsoft.com/en-us/library/ms633540(VS.85).aspx
   * BLENDFUNCTION Structure: http://msdn.microsoft.com/en-us/library/dd183393(VS.85).aspx
   */
  public class LayeredWindow : Form {
    public static bool IsSupported {
      get { return OSFeature.Feature.IsPresent(OSFeature.LayeredWindows); }
    }

    protected override CreateParams CreateParams {
      get
      {
        var createParams = base.CreateParams;

        createParams.ExStyle |= (int)WS_EX.LAYERED;

        if (passThroughMouseEvents)
          createParams.ExStyle |= (int)WS_EX.TRANSPARENT;

        return createParams;
      }
    }

    public bool UseColorKey {
      get { return useColorKey; }
      set
      {
        if (useColorKey != value) {
          useColorKey = value;
          Refresh();
        }
      }
    }

    public Color ColorKey {
      get { return colorKey; }
      set
      {
        if (colorKey != value) {
          colorKey = value;
          if (useColorKey)
            Refresh();
        }
      }
    }

    public byte Alpha {
      get { return blendFunction.SourceConstantAlpha; }
      set
      {
        if (blendFunction.SourceConstantAlpha != value) {
          blendFunction.SourceConstantAlpha = value;
          if (!useColorKey)
            Refresh();
        }
      }
    }

    public bool PassThroughMouseEvents {
      get
      {
        if (IsHandleCreated)
          return ((FormsInterop.user32.GetWindowLong(Handle, GWL.EXSTYLE) & (int)WS_EX.TRANSPARENT) != 0);
        else
          return passThroughMouseEvents;
      }
      set
      {
        if (PassThroughMouseEvents != value) {
          passThroughMouseEvents = value;
          SetWindowStyle();
        }
      }
    }

    public LayeredWindow()
    {
      if (!Runtime.IsRunningOnWindows || !IsSupported)
        throw new PlatformNotSupportedException("Layered windows feature is not supported or not installed");

      ControlBox      = false;
      MaximizeBox     = false;
      MinimizeBox     = false;
      Text            = string.Empty;
      FormBorderStyle = FormBorderStyle.None;

      blendFunction = new BLENDFUNCTION();
      blendFunction.BlendOp               = FormsInterop.Consts.AC_SRC_OVER;
      blendFunction.BlendFlags            = 0; // Must be zero
      blendFunction.SourceConstantAlpha   = 0xff;
      blendFunction.AlphaFormat           = FormsInterop.Consts.AC_SRC_ALPHA;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
        DisposeLayerImage();

      base.Dispose(disposing);
    }

    private void SetWindowStyle()
    {
      if (!IsHandleCreated)
        return;

      if (passThroughMouseEvents)
        FormsInterop.user32.SetWindowLong(Handle, GWL.EXSTYLE, FormsInterop.user32.GetWindowLong(Handle, GWL.EXSTYLE) | (int)WS_EX.TRANSPARENT);
      else
        FormsInterop.user32.SetWindowLong(Handle, GWL.EXSTYLE, FormsInterop.user32.GetWindowLong(Handle, GWL.EXSTYLE) & ~(int)WS_EX.TRANSPARENT);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
      Refresh();

      base.OnHandleCreated(e);
    }

    protected override void OnShown(EventArgs e)
    {
      DisposeLayerImage();
      Refresh();

      base.OnShown(e);
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
      Refresh();

      base.OnVisibleChanged(e);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
      DisposeLayerImage();
      Refresh();

      base.OnSizeChanged(e);
    }

    private void DisposeLayerImage()
    {
      if (layerImage != null) {
        layerImage.Dispose();
        layerImage = null;
      }
    }

    private void EnsureLayerImageCreated()
    {
      if (layerImage == null)
        layerImage = new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format32bppArgb);
    }

    public new void Refresh()
    {
      Invalidate();
      Update();
    }

    public new void Invalidate()
    {
      Invalidate(new Rectangle(new Point(0, 0), ClientSize));
    }

    public new void Invalidate(Rectangle rect)
    {
      if (rect == Rectangle.Empty)
        clipRectangle = Rectangle.Empty;
      else
        clipRectangle = Rectangle.Union(clipRectangle, rect);
    }

    public new void Update()
    {
      if (!Visible)
        return;

      var hDCScreen   = IntPtr.Zero;
      var hDC         = IntPtr.Zero;
      var hBitmap     = IntPtr.Zero;
      var hBitmapOld  = IntPtr.Zero;

      try {
        hDCScreen = ImagingInterop.user32.GetDC(IntPtr.Zero);
        hDC       = ImagingInterop.gdi32.CreateCompatibleDC(hDCScreen);

        if (hDC == IntPtr.Zero)
          return;

        EnsureLayerImageCreated();

        hBitmap = layerImage.GetHbitmap(Color.FromArgb(0));

        if (hBitmap == IntPtr.Zero)
          return;

        hBitmapOld = ImagingInterop.gdi32.SelectObject(hDC, hBitmap);

        using (var pev = new PaintEventArgs(Graphics.FromHdc(hDC), clipRectangle)) {
          pev.Graphics.Clear(Color.Transparent);

          OnPaint(pev);

          var size      = (SIZE)ClientSize;
          var ptDest    = (POINT)Location;
          var ptSource  = POINT.Zero;

          if (useColorKey)
            FormsInterop.user32.UpdateLayeredWindow(Handle, hDCScreen, ref ptDest, ref size, hDC, ref ptSource, (uint)ColorTranslator.ToWin32(colorKey), ref blendFunction, FormsInterop.Consts.ULW_COLORKEY);
          else
            FormsInterop.user32.UpdateLayeredWindow(Handle, hDCScreen, ref ptDest, ref size, hDC, ref ptSource, 0, ref blendFunction, FormsInterop.Consts.ULW_ALPHA);
        }

        clipRectangle = Rectangle.Empty;
      }
      finally {
        if (hBitmap != IntPtr.Zero) {
          if (hBitmapOld != IntPtr.Zero)
            ImagingInterop.gdi32.SelectObject(hDC, hBitmapOld);
          ImagingInterop.gdi32.DeleteObject(hBitmap);
        }
        if (hDC != IntPtr.Zero)
          ImagingInterop.gdi32.DeleteDC(hDC);
        if (hDCScreen != IntPtr.Zero)
          ImagingInterop.user32.ReleaseDC(IntPtr.Zero, hDCScreen);
      }
    }

    private bool useColorKey = false;
    private Color colorKey = Color.Transparent;
    private bool passThroughMouseEvents = false;
    private BLENDFUNCTION blendFunction;
    private Bitmap layerImage = null;
    private Rectangle clipRectangle = Rectangle.Empty;
  }
}
