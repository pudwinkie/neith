using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace Blue.Windows
{
    public class StickyWindowWPF
    {
        private readonly StickyWindow impl;

        #region コンストラクタ

        // <summary>
        /// Make the window Sticky
        /// </summary>
        /// <param name="window">Window to be made sticky</param>
        public StickyWindowWPF(Window window)
        {
            impl = new StickyWindow(window);
        }

        #endregion
        #region 公開プロパティ

        /// <summary>スナップするピクセル距離を指定します。既定値は20pxです。</summary>
        public int StickGap
        {
            get { return impl.StickGap; }
            set { impl.StickGap = value; }
        }

        /// <summary>trueの時、サイズ変更時にスナップします。既定値はtrueです。</summary>
        public bool StickOnResize
        {
            get { return impl.StickOnResize; }
            set { impl.StickOnResize = value; }
        }

        /// <summary>trueの時、移動時にスナップします。既定値はtrueです。</summary>
        public bool StickOnMove
        {
            get { return impl.StickOnMove; }
            set { impl.StickOnMove = value; }
        }

        /// <summary>trueの時、画面端にスナップします。既定値はtrueです。</summary>
        public bool StickToScreen
        {
            get { return impl.StickToScreen; }
            set { impl.StickToScreen = value; }
        }

        /// <summary>trueの時、他のスナップ窓にスナップします。既定値はtrueです。</summary>
        public bool StickToOther
        {
            get { return impl.StickToOther; }
            set { impl.StickToOther = value; }
        }


        #endregion
        #region 公開メソッド
        #region WPF Window
        /// <summary>
        /// 外部窓をスナップ対象に登録します。
        /// </summary>
        /// <param name="winExternal">System.Windows.Window</param>
        public static void Register(Window winExternal)
        {
            StickyWindow.Register(winExternal);
        }

        /// <summary>
        /// スナップ対象の外部フォームを削除します。
        /// </summary>
        /// <param name="winExternal">System.Windows.Window</param>
        public static void Unregister(Window winExternal)
        {
            StickyWindow.Unregister(winExternal);
        }


        #endregion
        #region HWND Window
        /// <summary>
        /// 外部窓をスナップ対象に登録します。
        /// </summary>
        /// <param name="hWnd">Window Handle</param>
        public static void Register(IntPtr hWnd)
        {
            StickyWindow.Register(hWnd);
        }

        /// <summary>
        /// スナップ対象の外部フォームを削除します。
        /// </summary>
        /// <param name="hWnd">Window Handle</param>
        public static void Unregister(IntPtr hWnd)
        {
            StickyWindow.Unregister(hWnd);
        }


        #endregion
        #region IWin32Window
        /// <summary>
        /// 外部窓をスナップ対象に登録します。
        /// </summary>
        /// <param name="hWnd">Window Handle</param>
        public static void Register(IWin32Window win)
        {
            Register(win.Handle);
        }

        /// <summary>
        /// スナップ対象の外部フォームを削除します。
        /// </summary>
        /// <param name="hWnd">Window Handle</param>
        public static void Unregister(IWin32Window win)
        {
            Unregister(win.Handle);
        }


        #endregion

        #endregion
    }
}