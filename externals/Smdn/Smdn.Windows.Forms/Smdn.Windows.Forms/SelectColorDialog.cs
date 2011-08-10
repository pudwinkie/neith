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
using System.Windows.Forms;

using Smdn.Imaging;

namespace Smdn.Windows.Forms {
  public partial class SelectColorDialog : Form {
    public HsvColor HsvColor {
      get { return hsvColorPicker.Color; }
      set { hsvColorPicker.Color = value; }
    }

    public Color RgbColor {
      get { return ColorModel.ToRgb(HsvColor); }
      set { HsvColor = ColorModel.ToHsv(value); }
    }

    public SelectColorDialog()
    {
      InitializeComponent();
      Initialize();
    }

    private void Initialize()
    {
      hsvColorPicker.Color = HsvColor.Black;

      labelHue.BackColor = Color.Black;
      labelColor.BackColor = Color.Black;

      hsvColorPicker.ColorChanged += hsvColorPicker_ColorChanged;

      numericUpDownHsvHue.ValueChanged        += numericUpDownHsv_ValueChanged;
      numericUpDownHsvSaturation.ValueChanged += numericUpDownHsv_ValueChanged;
      numericUpDownHsvValue.ValueChanged      += numericUpDownHsv_ValueChanged;

      numericUpDownRgbR.ValueChanged += numericUpDownRgb_ValueChanged;
      numericUpDownRgbG.ValueChanged += numericUpDownRgb_ValueChanged;
      numericUpDownRgbB.ValueChanged += numericUpDownRgb_ValueChanged;
    }

    private void numericUpDownHsv_ValueChanged(object sender, EventArgs e)
    {
      if (lockChanged)
        return;

      try {
        lockChanged = true;

        hsvColorPicker.Color = new HsvColor((int)numericUpDownHsvHue.Value, (byte)(numericUpDownHsvSaturation.Value * 2.55m), (byte)(numericUpDownHsvValue.Value * 2.55m));

        var rgbColor = ColorModel.ToRgb(hsvColorPicker.Color);

        SetRgbValue(rgbColor);
        SetPreview(rgbColor, hsvColorPicker.Color);
      }
      finally {
        lockChanged = false;
      }
    }

    private void numericUpDownRgb_ValueChanged(object sender, EventArgs e)
    {
      if (lockChanged)
        return;

      try {
        lockChanged = true;

        var rgbColor = Color.FromArgb((int)numericUpDownRgbR.Value, (int)numericUpDownRgbG.Value, (int)numericUpDownRgbB.Value);
        var hsvColor = ColorModel.ToHsv(rgbColor);

        SetHsvValue(hsvColor);
        SetPreview(rgbColor, hsvColor);
      }
      finally {
        lockChanged = false;
      }
    }

    private void hsvColorPicker_ColorChanged(object sender, HsvColorChangedEventArgs e)
    {
      if (lockChanged)
        return;

      try {
        lockChanged = true;

        SetHsvValue(e.NewColor);

        var rgbColor = ColorModel.ToRgb(e.NewColor);

        SetRgbValue(rgbColor);
        SetPreview(rgbColor, e.NewColor);
      }
      finally {
        lockChanged = false;
      }
    }

    private void SetRgbValue(Color rgbColor)
    {
      numericUpDownRgbR.Value = rgbColor.R;
      numericUpDownRgbG.Value = rgbColor.G;
      numericUpDownRgbB.Value = rgbColor.B;
    }

    private void SetHsvValue(HsvColor hsvColor)
    {
      hsvColorPicker.Color = hsvColor;

      numericUpDownHsvHue.Value         = (decimal)hsvColor.H;
      numericUpDownHsvSaturation.Value  = 100.0m * hsvColor.S / 255.0m;
      numericUpDownHsvValue.Value       = 100.0m * hsvColor.V / 255.0m;
    }

    private void SetPreview(Color rgbColor, HsvColor hsvColor)
    {
      textBoxRgb.Text = ColorTranslator.ToHtml(rgbColor);

      labelHue.BackColor = ColorModel.ToRgb(new HsvColor(hsvColor.H, 0xff, 0xff));
      labelColor.BackColor = rgbColor;
    }

    private bool lockChanged = false;
  }
}
