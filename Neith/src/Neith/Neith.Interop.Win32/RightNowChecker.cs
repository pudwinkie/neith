using System;
using System.Runtime.InteropServices;

namespace Neith.Interop
{
    /// <summary>
    /// 在席チェッカー。キー入力の状況で判定します。
    /// このクラスはマルチスレッド動作に対応しません。
    /// ロックが必要な場合は外部実装で行ってください。
    /// </summary>
    public class RightNowChecker
    {
        private uint LastInputTicks { get; set; }

        /// <summary>在席中はtrueを返します。</summary>
        public bool IsRightNow
        {
            get { return isRightNow; }
            private set
            {
                bool isChange = isRightNow != value;
                isRightNow = value;
                if (isChange) OnValueChanged();
            }
        }
        private bool isRightNow;


        public RightNowChecker()
        {
            Reset();
        }

        /// <summary>
        /// 在席タイマーをクリアします。
        /// 　・状態を無条件に「在席」とします。
        /// 　・最終入力時刻を更新します。
        /// </summary>
        public void Reset()
        {
            LastInputTicks = GetInputTicks();
            IsRightNow = true;
        }

        /// <summary>
        /// 在席確認を行います。
        /// 最終入力時刻の変動を確認します。
        /// </summary>
        public void Check()
        {
            var lastTick = GetInputTicks();
            var check = LastInputTicks != lastTick;
            LastInputTicks = lastTick;
            IsRightNow = check;
        }

        /// <summary>
        /// 値の変更を通知します。
        /// </summary>
        public event EventHandler ValueChanged;

        private void OnValueChanged()
        {
            if (ValueChanged == null) return;
            ValueChanged(this, EventArgs.Empty);
        }

        #region Win32 API

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));

            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dwTime;
        }

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        /// <summary>
        ///  最後に入力があった時刻を返します。
        /// </summary>
        private static uint GetInputTicks()
        {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;
            if (!GetLastInputInfo(ref lastInputInfo)) return 0;
            return lastInputInfo.dwTime;
        }

        #endregion
    }
}
