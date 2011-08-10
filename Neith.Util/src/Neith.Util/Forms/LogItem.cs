using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSUtil.Forms
{
  /// <summary>
  /// アイテム情報構造体。
  /// </summary>
  public struct LogItem
  {
    /// <summary>
    /// タイムスタンプ。
    /// </summary>
    public readonly DateTime TimeStamp;

    /// <summary>
    /// 表示テキスト。
    /// </summary>
    public readonly string Text;

    /// <summary>
    /// 文字色。
    /// </summary>
    public readonly Color ForeColor;

    /// <summary>
    /// 背景色。
    /// </summary>
    public readonly Color BackColor;

    /// <summary>
    /// コンストラクタ。
    /// </summary>
    /// <param name="timeStamp"></param>
    /// <param name="text"></param>
    /// <param name="foreColor"></param>
    /// <param name="backColor"></param>
    public LogItem(DateTime timeStamp, string text, Color foreColor, Color backColor)
    {
      TimeStamp = timeStamp;
      Text = text;
      ForeColor = foreColor;
      BackColor = backColor;
    }

    /// <summary>
    /// 表示内容。
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return Text;
    }
  }
}
