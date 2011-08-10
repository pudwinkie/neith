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
using System.Windows.Forms;

using Smdn.Imaging;
using Smdn.Mathematics;

using RgbColor = System.Drawing.Color;

namespace Smdn.Windows.Forms {
  public class HsvColorPicker : Control {
    public event EventHandler<HsvColorChangedEventArgs> ColorChanged;

    public HsvColor Color {
      get { return color; }
      set
      {
        if (color == value)
          return;

        var e = new HsvColorChangedEventArgs(color, value);

        color = value;

        OnColorChanged(e);
      }
    }

    protected virtual float RadiusOuter {
      get; private set;
    }

    protected virtual float RadiusInner {
      get; private set;
    }

    protected virtual PointF Center {
      get; private set;
    }

    protected virtual RectangleF OuterCircumscribedRect {
      get; private set;
    }

    protected virtual RectangleF InnerCircumscribedRect {
      get; private set;
    }

    protected virtual PointF[] SaturationValueTrianglePoints {
      get; private set;
    }

    protected RectangleF SaturationValueTriangleBounds {
      get; private set;
    }

    protected float SaturationValueTriangleLength {
      get; private set;
    }

    public HsvColorPicker()
    {
      this.SetStyle(ControlStyles.DoubleBuffer, true);
      this.SetStyle(ControlStyles.UserPaint, true);
      this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing) {
        if (hueCircleBitmap != null) {
          hueCircleBitmap.Dispose();
          hueCircleBitmap = null;
        }

        if (saturationValueTriangleBitmap != null) {
          saturationValueTriangleBitmap.Dispose();
          saturationValueTriangleBitmap = null;
        }
      }

      base.Dispose(disposing);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
      UpdateMetrics();
      UpdateHueCircleBitmap();
      UpdateSaturationValueTriangleBitmap(false);

      base.OnSizeChanged(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
      mouseDownPoint = PointToPolar(e);

      base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
      SetColorByMouse(e);

      base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
      SetColorByMouse(e);

      base.OnMouseUp(e);
    }

    private void SetColorByMouse(MouseEventArgs e)
    {
      if ((e.Button & MouseButtons.Left) != 0) {
        if (RadiusInner < mouseDownPoint.Radius && mouseDownPoint.Radius <= RadiusOuter) {
          var point = PointToPolar(e);

          Color = new HsvColor((int)-point.Angle.ToDegree(),
                               Color.S,
                               Color.V);
        }
        else if (mouseDownPoint.Radius <= RadiusInner) {
          var x = e.X - SaturationValueTriangleBounds.X;
          var y = e.Y - SaturationValueTriangleBounds.Y;
          var rot = Radian.FromDegree(-Color.H);
          byte s, v;

          if (PointToSV(x, y, rot, out s, out v))
            Color = new HsvColor(Color.H, s, v);
        }
      }
    }

    private void UpdateMetrics()
    {
      RadiusOuter = ((ClientSize.Width < ClientSize.Height) ? ClientSize.Width : ClientSize.Height) * 0.5f;
      RadiusInner = RadiusOuter * 0.8f;
      Center = new PointF(ClientRectangle.X + ClientRectangle.Width * 0.5f,
                          ClientRectangle.Y + ClientRectangle.Height * 0.5f);
      OuterCircumscribedRect = RectangleFExtensions.CreateCircumscribed(Center, RadiusOuter);
      InnerCircumscribedRect = RectangleFExtensions.CreateCircumscribed(Center, RadiusInner);
      SaturationValueTrianglePoints = new[] {
        PolarExtensions.ToPointF(new Polar(RadiusInner, 0.0f), Center),
        PolarExtensions.ToPointF(new Polar(RadiusInner, Radian.PI * 2.0f / 3.0f), Center),
        PolarExtensions.ToPointF(new Polar(RadiusInner, Radian.PI * 4.0f / 3.0f), Center),
      };
      SaturationValueTriangleBounds = new RectangleF(SaturationValueTrianglePoints[2].X,
                                                     SaturationValueTrianglePoints[2].Y,
                                                     SaturationValueTrianglePoints[0].X - SaturationValueTrianglePoints[1].X,
                                                     SaturationValueTrianglePoints[1].Y - SaturationValueTrianglePoints[2].Y);
      SaturationValueTriangleLength = SaturationValueTriangleBounds.Height;
    }

    private static readonly Radian angleTriangle = Radian.StraightAngle / 3.0f; // 60deg
    private static readonly Radian halfAngleTriangle = angleTriangle / 2.0f; // 30deg
    private static readonly float tan30 = (float)Math.Tan(Math.PI / 6);

    private bool PointToSV(float x, float y, Radian rotation, out byte s, out byte v)
    {
      var pt = Polar.FromCartecian((x + SaturationValueTriangleBounds.X) - Center.X,
                                   (y + SaturationValueTriangleBounds.Y) - Center.Y);

      pt = pt.Rotate(-rotation);

      x = pt.X + Center.X - SaturationValueTriangleBounds.X;
      y = pt.Y + Center.Y - SaturationValueTriangleBounds.Y;

      return PointToSV(x, y, out s, out v);
    }

    private bool PointToSV(float x, float y, out byte s, out byte v)
    {
      s = 0;
      v = 0;

      var xx = x / SaturationValueTriangleLength;
      var yy = y / SaturationValueTriangleLength;

      var p = Polar.FromCartecian(xx, yy);

      if (halfAngleTriangle <= p.Angle && p.Angle < Radian.RightAngle)
        s = (byte)(256.0f * (Radian.RightAngle - p.Angle) / angleTriangle);
      else
        return false;

      var vv = yy + xx * tan30;

      if (0.0f <= vv && vv < 1.0f) {
        v = (byte)(256.0f * vv);
        return true;
      }
      else {
        return false;
      }
    }

    private PointF SVToPoint(byte s, byte v)
    {
      var vv = v / 256.0f;
      var ss = s / 256.0f;

      // FIXME
      var xx = (float)(vv * Math.Cos((double)(Radian.RightAngle - angleTriangle * ss)));
      var yy = vv - xx * tan30;

      return new PointF(xx * SaturationValueTriangleLength, yy * SaturationValueTriangleLength);
    }

    private Polar PointToPolar(MouseEventArgs e)
    {
      return Polar.FromCartecian(e.X - Center.X, e.Y - Center.Y);
    }

    private void UpdateHueCircleBitmap()
    {
      if (hueCircleBitmap != null &&
          (hueCircleBitmap.Width == (int)OuterCircumscribedRect.Width) &&
          (hueCircleBitmap.Height == (int)OuterCircumscribedRect.Height))
        return;

      if (hueCircleBitmap != null) {
        hueCircleBitmap.Dispose();
        hueCircleBitmap = null;
      }

      hueCircleBitmap = new Bitmap((int)OuterCircumscribedRect.Width, (int)OuterCircumscribedRect.Height, PixelFormat.Format32bppArgb);

      using (var g = Graphics.FromImage(hueCircleBitmap)) {
        var shiftMatrix = new Matrix();

        shiftMatrix.Translate(-OuterCircumscribedRect.X, -OuterCircumscribedRect.Y);

        g.Transform = shiftMatrix;

        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.InterpolationMode = InterpolationMode.HighQualityBilinear;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using (var locked = new LockedBitmap(hueCircleBitmap, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb)) {
          const uint transparent = 0x00000000;
          var centerX = OuterCircumscribedRect.Width * 0.5f;
          var centerY = OuterCircumscribedRect.Height * 0.5f;

          unsafe {
            locked.ForEachScanLine(delegate(void* scanline, int y, int width) {
              var bgra = (byte*)scanline;
              var pointY = (float)y - centerY;

              for (var x = 0; x < width; x++) {
                var point = Polar.FromCartecian((float)x - centerX, pointY);

                if (RadiusInner <= point.Radius && point.Radius <= RadiusOuter) {
                  var h = -point.Angle.ToDegree();
                  var col = ColorModel.ToRgb(new HsvColor((int)h, 0xff, 0xff));

                  *(bgra++) = col.B;
                  *(bgra++) = col.G;
                  *(bgra++) = col.R;
                  *(bgra++) = 0xff;
                }
                else {
                  *(uint*)bgra = transparent;

                  bgra += 4;
                }
              }
            });
          } // unsafe
        } // using LockedBitmap

        g.DrawEllipse(Pens.Black, OuterCircumscribedRect);
        g.DrawEllipse(Pens.Black, InnerCircumscribedRect);
      } // using Graphics
    }

    private void UpdateSaturationValueTriangleBitmap(bool refreshHue)
    {
      if (!refreshHue &&
          saturationValueTriangleBitmap != null &&
          (hueCircleBitmap.Width == (int)InnerCircumscribedRect.Width) &&
          (hueCircleBitmap.Height == (int)InnerCircumscribedRect.Height))
        return;

      if (saturationValueTriangleBitmap != null) {
        saturationValueTriangleBitmap.Dispose();
        saturationValueTriangleBitmap = null;
      }

      saturationValueTriangleBitmap = new Bitmap((int)InnerCircumscribedRect.Width, (int)InnerCircumscribedRect.Height, PixelFormat.Format32bppArgb);

      using (var g = Graphics.FromImage(saturationValueTriangleBitmap)) {
        using (var locked = new LockedBitmap(saturationValueTriangleBitmap, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb)) {
          const uint transparent = 0x00000000;

          unsafe {
            locked.ForEachScanLine(delegate(void* scanline, int y, int width) {
              var bgra = (byte*)scanline;
              var yy = InnerCircumscribedRect.Y + y - SaturationValueTriangleBounds.Y;
              var offsetX = InnerCircumscribedRect.X - SaturationValueTriangleBounds.X;

              for (var x = 0; x < width; x++) {
                byte s, v;

                if (PointToSV(x + offsetX, yy, Radian.Zero, out s, out v)) {
                  var col = ColorModel.ToRgb(new HsvColor(Color.H, s, v));

                  *(bgra++) = col.B;
                  *(bgra++) = col.G;
                  *(bgra++) = col.R;
                  *(bgra++) = 0xff;
                }
                else {
                  *(uint*)bgra = transparent;

                  bgra += 4;
                }
              }
            });
          } // unsafe
        } // using LockedBitmap
      } // using Graphics
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      try {
        var g = e.Graphics;

        g.Clear(SystemColors.Control);
        g.InterpolationMode = InterpolationMode.HighQualityBilinear;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        g.DrawImageUnscaled(hueCircleBitmap, (int)OuterCircumscribedRect.X, (int)OuterCircumscribedRect.Y);

        var rotationMatrix = new Matrix();

        rotationMatrix.RotateAt((float)-Color.H, Center);

        g.Transform = rotationMatrix;

        g.DrawImageUnscaled(saturationValueTriangleBitmap, (int)InnerCircumscribedRect.X, (int)InnerCircumscribedRect.Y);

#if false
        g.DrawPolygon(Pens.Black, SaturationValueTrianglePoints);
        g.DrawRectangle(Pens.Black, Rectangle.Round(SaturationValueTriangleBounds));
#endif

        var widthPen = RadiusOuter * 0.025f;

        using (var p = new Pen(((ColorModel.LuminanceOf(new HsvColor(Color.H, 0xff, 0xff)) < 0.5f) ? RgbColor.White : RgbColor.Black), widthPen)) {
          g.DrawLine(p, Center.X + RadiusInner, Center.Y, Center.X + RadiusOuter, Center.Y);
        }

        using (var p = new Pen(((ColorModel.LuminanceOf(Color) < 0.5) ? RgbColor.White : RgbColor.Black), widthPen)) {
          var pointSV = SVToPoint(Color.S, Color.V);

          g.DrawCircle(p,
                       pointSV.X + SaturationValueTriangleBounds.X,
                       pointSV.Y + SaturationValueTriangleBounds.Y,
                       widthPen * 2.0f);
        }
      }
      finally {
        base.OnPaint(e);
      }
    }

    protected virtual void OnColorChanged(HsvColorChangedEventArgs e)
    {
      var ev = this.ColorChanged;

      if (ev != null)
        ev(this, e);

      UpdateSaturationValueTriangleBitmap(e.LastColor.H != e.NewColor.H);

      Refresh();
    }

    private HsvColor color = HsvColor.Black;
    private Bitmap hueCircleBitmap = null;
    private Bitmap saturationValueTriangleBitmap = null;
    private Polar mouseDownPoint = Polar.Zero;
  }
}
