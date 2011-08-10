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

using Smdn.Imaging;
using Smdn.Mathematics;

namespace Smdn.Windows.UserInterfaces {
  public class Wallpaper {
    public Color BackgroundColorNear {
      get; set;
    }

    public Color BackgroundColorFar {
      get; set;
    }

    /// <value>in radian</value>
    public Radian GradientDirection {
      get; set;
    }

    public ImageFillStyle DrawStyle {
      get; set;
    }

    public Wallpaper()
      : this(Color.Black, ImageFillStyle.Center)
    {
    }

    public Wallpaper(Color backgroundColor)
      : this(Color.Black, ImageFillStyle.Center)
    {
    }

    protected Wallpaper(Color backgroundColor, ImageFillStyle drawStyle)
      : this(backgroundColor, backgroundColor, Radian.Zero, drawStyle)
    {
    }

    public Wallpaper(Color backgroundColorNear, Color backgroundColorFar, Radian gradientDirection)
      : this(backgroundColorNear, backgroundColorFar, gradientDirection, ImageFillStyle.Center)
    {
    }

    protected Wallpaper(Color backgroundColorNear, Color backgroundColorFar, Radian gradientDirection, ImageFillStyle drawStyle)
    {
      if (gradientDirection.IsNaN || gradientDirection.IsInfinity)
        throw new ArgumentOutOfRangeException("gradientDirection", "must be a finite number");

      this.BackgroundColorNear = backgroundColorNear;
      this.BackgroundColorFar = backgroundColorFar;
      this.GradientDirection = gradientDirection;
      this.DrawStyle = drawStyle;
    }

    public void RenderTo(Graphics g, Rectangle bounds)
    {
      RenderTo(g, bounds, null);
    }

    public virtual void RenderTo(Graphics g, Rectangle bounds, ImageAttributes attrs)
    {
      CallbackRender(delegate(Bitmap bitmap) {
        using (var brushBackground = CreateBackgroundBrush(bounds)) {
          if (bitmap == null)
            g.FillRectangle(brushBackground, bounds);
          else
            g.FillRectangleWithImage(bitmap, bounds, DrawStyle, brushBackground, attrs);
        }
      });
    }

    private Brush CreateBackgroundBrush(Rectangle bounds)
    {
      if (BackgroundColorNear == BackgroundColorFar)
        return new SolidBrush(BackgroundColorNear);

      var center = bounds.GetCenterF();
      var direction = (double)GradientDirection;

      // FIXME
      var near = new Point((int)(center.X + bounds.Width * 0.5 * Math.Cos(direction)),
                           (int)(center.Y + bounds.Height * 0.5 * Math.Sin(direction)));
      var far  = new Point((int)(center.X + bounds.Width * 0.5 * Math.Cos(direction + Math.PI)),
                           (int)(center.Y + bounds.Height * 0.5 * Math.Sin(direction + Math.PI)));

      return new LinearGradientBrush(near, far, BackgroundColorNear, BackgroundColorFar);
    }

    protected virtual void CallbackRender(Action<Bitmap> render)
    {
      render(null);
    }
  }
}
