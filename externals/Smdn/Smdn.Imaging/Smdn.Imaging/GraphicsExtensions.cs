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

namespace Smdn.Imaging {
  public static class GraphicsExtensions {
    /*
    public static void DrawCircumcircle(this Graphics g, Pen p, Rectangle rect)
    {
      throw new NotImplementedException();
    }

    public static void DrawCircumcircle(this Graphics g, Pen p, RectangleF rect)
    {
      throw new NotImplementedException();
    }
    */

    /*
    public static void DrawIncircle(this Graphics g, Pen p, Rectangle rect)
    {
      DrawCircle(g, p, (RectangleF)rect);
    }

    public static void DrawIncircle(this Graphics g, Pen p, RectangleF rect)
    {
      throw new NotImplementedException();
    }
    */

    public static void DrawCircle(this Graphics g, Pen pen, Point center, int radius)
    {
      g.DrawEllipse(pen, RectangleExtensions.CreateCircumscribed(center, radius));
    }

    public static void DrawCircle(this Graphics g, Pen pen, int x, int y, int radius)
    {
      g.DrawEllipse(pen, RectangleExtensions.CreateCircumscribed(x, y, radius));
    }

    public static void DrawCircle(this Graphics g, Pen pen, PointF center, float radius)
    {
      g.DrawEllipse(pen, RectangleFExtensions.CreateCircumscribed(center, radius));
    }

    public static void DrawCircle(this Graphics g, Pen pen, float x, float y, float radius)
    {
      g.DrawEllipse(pen, RectangleFExtensions.CreateCircumscribed(x, y, radius));
    }

    public static void FillRectangleWithImage(this Graphics g, Image image, Rectangle rect, ImageFillStyle style)
    {
      FillRectangleWithImage(g, image, rect, style, null, null);
    }

    public static void FillRectangleWithImage(this Graphics g, Image image, Rectangle rect, ImageFillStyle style, ImageAttributes imageAttrs)
    {
      FillRectangleWithImage(g, image, rect, style, null, imageAttrs);
    }

    public static void FillRectangleWithImage(this Graphics g, Image image, Rectangle rect, ImageFillStyle style, Color colorBackground)
    {
      FillRectangleWithImage(g, image, rect, style, colorBackground, null);
    }

    public static void FillRectangleWithImage(this Graphics g, Image image, Rectangle rect, ImageFillStyle style, Color colorBackground, ImageAttributes imageAttrs)
    {
      using (var b = new SolidBrush(colorBackground)) {
        FillRectangleWithImage(g, image, rect, style, b, imageAttrs);
      }
    }

    public static void FillRectangleWithImage(this Graphics g, Image image, Rectangle rect, ImageFillStyle style, Brush brushBackground)
    {
      FillRectangleWithImage(g, image, rect, style, brushBackground, null);
    }

    public static void FillRectangleWithImage(this Graphics g, Image image, Rectangle rect, ImageFillStyle style, Brush brushBackground, ImageAttributes imageAttrs)
    {
      if (image == null)
        throw new ArgumentNullException("image");

      var prevClip = g.Clip;

      try {
        using (var regionClip = new Region(rect)) {
          g.Clip = regionClip;

          if (brushBackground != null)
            g.FillRectangle(brushBackground, rect);

          switch (style) {
            /*
             * unscaled
             */
            case ImageFillStyle.Center: {
              g.DrawImageUnscaled(image,
                                  rect.X + (rect.Width  - image.Width)  / 2,
                                  rect.Y + (rect.Height - image.Height) / 2);
              break;
            }

            case ImageFillStyle.Tile:
            case ImageFillStyle.TileCenter: {
              var offset = (style == ImageFillStyle.Tile)
                ? new Point(0, 0)
                : new Point(((rect.Width - image.Width) / 2) - image.Width * (1 + (rect.Width / image.Width) / 2),
                             ((rect.Height - image.Height) / 2) - image.Height * (1 + (rect.Height / image.Height) / 2));

              for (var y = rect.Top + offset.Y; y <= rect.Bottom; y += image.Height) {
                for (var x = rect.Left + offset.X; x <= rect.Right; x += image.Width) {
                  if (imageAttrs == null)
                    g.DrawImageUnscaled(image, x, y);
                  else
                    g.DrawImage(image,
                                new Rectangle(x, y, image.Width, image.Height),
                                0,
                                0,
                                image.Width,
                                image.Height,
                                GraphicsUnit.Pixel,
                                imageAttrs);
                }
              }
              break;
            }

            /*
             * scaled
             */
            case ImageFillStyle.Fill: {
              g.DrawImage(image, rect);
              break;
            }

            case ImageFillStyle.Zoom:
            case ImageFillStyle.Fit: {
              var aspectImage = image.Height / (float)image.Width;
              var aspectRect = rect.Height / (float)rect.Width;
              float scaledImageWidth, scaledImageHeight;

              if ((style == ImageFillStyle.Zoom && (aspectRect < aspectImage)) ||
                  (style == ImageFillStyle.Fit && (aspectImage < aspectRect))) {
                scaledImageWidth  = (float)rect.Width;
                scaledImageHeight = (float)image.Height * (rect.Width / (float)image.Width);
              }
              else {
                scaledImageWidth  = (float)image.Width * (rect.Height / (float)image.Height);
                scaledImageHeight = (float)rect.Height;
              }

              g.DrawImage(image,
                          Rectangle.Truncate(new RectangleF((float)rect.X + (rect.Width  - scaledImageWidth)  * 0.5f,
                                                            (float)rect.Y + (rect.Height - scaledImageHeight) * 0.5f,
                                                            scaledImageWidth,
                                                            scaledImageHeight)),
                          0,
                          0,
                          image.Width,
                          image.Height,
                          GraphicsUnit.Pixel,
                          imageAttrs);

              break;
            }

            default:
              throw ExceptionUtils.CreateNotSupportedEnumValue(style);
          } // switch style
        } // using clip
      }
      finally {
        g.Clip = prevClip;
      }
    }
  }
}
