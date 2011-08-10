using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;

namespace Neith.Util.IO
{
    /// <summary>
    /// ファイルの更新を確認します。
    /// 現在の実装はタイムスタンプとファイルサイズを比較します。
    /// </summary>
    [Serializable]
    public class FileUpdateChecker
    {
        /// <summary>ファイルパスを取得します。</summary>
        public readonly string FilePath;
        private DateTime lastWriteTime;
        private long length;

        /// <summary>
        /// 判定したかどうかを管理するフラグです。
        /// </summary>
        public enum CheckFlag
        {
            /// <summary>未判定</summary>
            NotCheck,
            /// <summary>true</summary>
            True,
            /// <summary>false</summary>
            False,
        }
        [NonSerialized]
        private CheckFlag checkFlag = CheckFlag.NotCheck;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="path"></param>
        public FileUpdateChecker(string path)
        {
            FilePath = Path.GetFullPath(path);
            if (!File.Exists(path)) {
                lastWriteTime = DateTime.MinValue;
                length = -1;
                return;
            }
            FileInfo info = new FileInfo(path);
            lastWriteTime = info.LastWriteTime;
            length = info.Length;
        }

        /// <summary>
        /// 最新状況で更新されていればtrue。
        /// </summary>
        public bool IsUpdate
        {
            get
            {
                if (checkFlag != CheckFlag.NotCheck) return checkFlag == CheckFlag.True;
                FileUpdateChecker nowFile = new FileUpdateChecker(FilePath);
                while (true) {
                    if (lastWriteTime != nowFile.lastWriteTime) break;
                    if (length != nowFile.length) break;
                    checkFlag = CheckFlag.False;
                    return false;
                }
                lastWriteTime = nowFile.lastWriteTime;
                length = nowFile.length;
                checkFlag = CheckFlag.True;
                return true;
            }
        }

        /// <summary>
        /// チェック状態をリセットします。
        /// </summary>
        public void Reset()
        {
            checkFlag = CheckFlag.NotCheck;
        }

    }
}
