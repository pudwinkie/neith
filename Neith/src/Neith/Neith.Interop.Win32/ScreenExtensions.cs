using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;

namespace Neith.Interop
{
    /// <summary>
    /// 画面情報を返します。
    /// </summary>
    public static class ScreenExtensions
    {

        public static Rect GetScreenRect(this Window window)
        {
            var rect = Screen.PrimaryScreen.WorkingArea;
            return new Rect(rect.Top, rect.Left, rect.Width, rect.Height);
        }


    }
}
