using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace StickyWindowLibrary
{
    /// <summary>
    /// 汎用のHWND窓のキャプチャアダプタ。位置の取得のみ可能。
    /// </summary>
    public class HWNDAdapter : IFormAdapter
    {
        private IntPtr handle;
        public HWNDAdapter(IntPtr handle)
        {
            this.handle = handle;
        }

        #region WIN32API

        //        typedef struct _RECT 
        //                { 
        //                    LONG left; 
        //                    LONG top; 
        //                    LONG right; 
        //                    LONG bottom; 
        //                } RECT, *PRECT; 
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public int X { get { return left; } }
            public int Y { get { return top; } }
            public int Width { get { return right - left; } }
            public int Height { get { return bottom - top; } }

            public Rectangle ToRectangle()
            {
                return new Rectangle(X, Y, Width, Height);
            }

        }

        //        BOOL GetWindowRect(
        //            HWND hWnd,      // ウィンドウのハンドル
        //            LPRECT lpRect   // ウィンドウの座標値
        //            );        
        [DllImport("User32.Dll")]
        private static extern int GetWindowRect(
            IntPtr hWnd,    // ウィンドウのハンドル
            out RECT rect   // ウィンドウの座標値
            );


        //        BOOL MoveWindow(
        //            HWND hWnd,      // ウィンドウのハンドル
        //            int X,          // 横方向の位置
        //            int Y,          // 縦方向の位置
        //            int nWidth,     // 幅
        //            int nHeight,    // 高さ
        //            BOOL bRepaint   // 再描画オプション
        //            );
        [DllImport("User32.dll")]
        private static extern int MoveWindow(
            IntPtr hWnd,
            int x,
            int y,
            int nWidth,
            int nHeight,
            int bRepaint
            );


        #endregion
        #region IFormAdapter メンバー

        public object FormObject { get { return handle; } }

        public IntPtr Handle { get { return handle; } }

        public Rectangle Bounds
        {
            get
            {
                RECT rect;
                GetWindowRect(Handle, out rect);
                return rect.ToRectangle();
            }
            set
            {
                MoveWindow(Handle, value.X, value.Y, value.Width, value.Height, 1);
            }
        }

        public System.Drawing.Size MaximumSize
        {
            get { return Bounds.Size; }
            set { throw new NotImplementedException(); }
        }

        public System.Drawing.Size MinimumSize
        {
            get { return Bounds.Size; }
            set { throw new NotImplementedException(); }
        }

        public bool Capture
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public void Activate()
        {
            throw new NotImplementedException();
        }

        public System.Drawing.Point PointToScreen(System.Drawing.Point point)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}