using System;
using System.Collections.Generic;
using System.Text;

namespace CSUtil.Forms
{
  /// <summary>
  /// ログアイテムを取得するインターフェースを定義します。
  /// </summary>
  public interface ILogItem
  {
    /// <summary>
    /// ログアイテムを取得します。
    /// </summary>
    /// <returns></returns>
    LogItem GetLogItem();
  }
}
