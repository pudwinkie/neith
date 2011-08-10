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
using System.Windows.Forms;

namespace Smdn.Windows.Forms {
  [DefaultEvent("ValueChanged")]
  [DefaultProperty("Value")]
  [DefaultBindingProperty ("Value")]
  public class TimeSpanUpDown : UpDownBase, ISupportInitialize {
    public event EventHandler ValueChanged;

    /*
     * properties
     */
    public TimeSpan Value {
      get
      {
        if (UserEdit)
          ValidateEditText();
        return val;
      }
      set
      {
        if (val == value)
          return;

        if (!suppressValidation && (value < minimum || maximum < value))
          throw new ArgumentOutOfRangeException("Value");

        val = value;

        OnValueChanged(EventArgs.Empty);

        UpdateEditText();
      }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Localizable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Converter<TimeSpan, string> TimeSpanToStringConverter {
      get { return timeSpanToStringConverter; }
      set
      {
        if (timeSpanToStringConverter == value)
          return;

        timeSpanToStringConverter = value;

        UpdateEditText();
      }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Localizable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Converter<string, TimeSpan> StringToTimeSpanConverter {
      get { return stringToTimeSpanConverter; }
      set
      {
        if (stringToTimeSpanConverter == value)
          return;

        stringToTimeSpanConverter = value;

        UpdateEditText();
      }
    }

    public TimeSpan Increment {
      get { return increment; }
      set
      {
        if (value < TimeSpan.Zero)
          throw new ArgumentOutOfRangeException("Increment", "Increment cannot be negative");

        increment = value;
      }
    }

    public TimeSpan Maximum {
      get { return maximum; }
      set
      {
        maximum = value;

        if (maximum < minimum)
          minimum = maximum;
        if (maximum < val)
          Value = maximum;
      }
    }

    public TimeSpan Minimum {
      get { return minimum; }
      set
      {
        minimum = value;

        if (maximum < minimum)
          minimum = maximum;
        if (val < minimum)
          Value = minimum;
      }
    }

    /*
     * methods
     */
    public static string DefaultConverter(TimeSpan val)
    {
      var sign = (val == TimeSpan.Zero)
        ? string.Empty
        : (TimeSpan.Zero < val)
          ? "+"
          : "-";

      return string.Format("{0}{1}:{2:D2}:{3:D2}.{4:D3}", sign, (int)Math.Abs(val.TotalHours), Math.Abs(val.Minutes), Math.Abs(val.Seconds), Math.Abs(val.Milliseconds));
    }

    public static TimeSpan DefaultConverter(string val)
    {
      return TimeSpan.Parse(val);
    }

    public TimeSpanUpDown()
    {
      timeSpanToStringConverter = DefaultConverter;
      stringToTimeSpanConverter = DefaultConverter;
      suppressValidation = false;
      increment = new TimeSpan(0, 1, 0);
      maximum = TimeSpan.MaxValue;
      minimum = TimeSpan.MinValue;

      UpdateEditText();
    }

    public void BeginInit()
    {
      suppressValidation = true;
    }

    public void EndInit()
    {
      suppressValidation = false;

      Value = GetRanged(val);

      UpdateEditText();
    }

    public override void DownButton()
    {
      if (UserEdit)
        ParseEditText();

      var newValue = val - increment;

      Value = (newValue < minimum) ? minimum : newValue;
    }

    public override void UpButton()
    {
      if (UserEdit)
        ParseEditText();

      var newValue = val + increment;

      Value = (maximum < newValue) ? maximum : newValue;
    }

    public void SelectAll()
    {
      if (Text == null)
        return;
      else
        Select(0, Text.Length);
    }

    protected override void ValidateEditText()
    {
      ParseEditText();
      UpdateEditText();
    }

    private void ParseEditText()
    {
      try {
        Value = (stringToTimeSpanConverter == null)
          ? DefaultConverter(Text)
          : stringToTimeSpanConverter(Text);
      }
      catch {
        // ignore exceptions
      }
      finally {
        UserEdit = false;
      }
    }

    protected override void UpdateEditText()
    {
      if (suppressValidation)
        return;

      if (UserEdit)
        ParseEditText();

      ChangingText = true;

      Text = (timeSpanToStringConverter == null)
        ? DefaultConverter(val)
        : timeSpanToStringConverter(val);
    }

    private TimeSpan GetRanged(TimeSpan t)
    {
      if (maximum < t)
        return maximum;
      else if (t < minimum)
        return minimum;
      else
        return t;
    }

    protected virtual void OnValueChanged(EventArgs e)
    {
      var ev = this.ValueChanged;

      if (ev != null)
        ev(this, e);
    }

    private Converter<TimeSpan, string> timeSpanToStringConverter = null;
    private Converter<string, TimeSpan> stringToTimeSpanConverter = null;
    private TimeSpan val;
    private TimeSpan increment;
    private TimeSpan minimum;
    private TimeSpan maximum;
    private bool suppressValidation;
  }
}
