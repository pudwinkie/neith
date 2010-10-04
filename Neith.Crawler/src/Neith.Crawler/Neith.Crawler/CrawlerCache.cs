using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Neith.Crawler
{
    /// <summary>
    /// クロウラーのファイルキャッシュへのアクセス。
    /// </summary>
    public class CrawlerCache
    {
        /// <summary>Key値</summary>
        public string Key { get; private set; }

        /// <summary>Hash値</summary>
        public string Hash { get; private set; }

        /// <summary>基準ディレクトリ</summary>
        public string BaseDir { get; private set; }

        /// <summary>基準BasePath</summary>
        public string BasePath { get; private set; }


        /// <summary>基準ディレクトリ</summary>
        public bool IsCreateBaseDir { get; private set; }

        /// <summary>
        /// キャッシュインスタンスを作成します。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static CrawlerCache Create(string key)
        {
            return new CrawlerCache(key);
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="key"></param>
        private CrawlerCache(string key)
        {
            Key = key;
            Hash = Key.ToHashString();
            BaseDir = Const.CacheDir
                .PathConbine(Hash.Substring(0, 1))
                .PathConbine(Hash.Substring(1, 1))
                .ToFullPath();
            BasePath = BaseDir
                .PathConbine(Hash)
                .ToFullPath();
            IsCreateBaseDir = false;
        }

        /// <summary>
        /// 該当キャッシュをすべて削除します。
        /// ファイルが存在しない場合は何もしません。
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public void Clear()
        {
            if (!Directory.Exists(BaseDir)) return;
            foreach (var path in Directory.GetFiles(BaseDir, Hash + "*.*")) {
                File.Delete(path);
            }
            IsCreateBaseDir = false;
        }

        /// <summary>
        /// BaseDirが存在しなければ作成します。
        /// </summary>
        public void CreateBaseDir()
        {
            if (IsCreateBaseDir) return;
            Directory.CreateDirectory(BaseDir);
            IsCreateBaseDir = true;
        }

        /// <summary>
        /// ファイル名を返します。
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetPath(string fileName)
        {
            return (BasePath + "." + fileName).ToFullPath();
        }

        /// <summary>
        /// テキストをファイルに書き出します。
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="contents"></param>
        public void WriteAllText(string fileName, string contents)
        {
            CreateBaseDir();
            var path = GetPath(fileName);
            File.WriteAllText(path, contents);
        }

        /// <summary>
        /// ファイルを読み込み、テキストを返します。
        /// ファイルが存在しない場合はnullを返します。
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string ReadAllText(string fileName)
        {
            var path = GetPath(fileName);
            if (!File.Exists(path)) return null;
            return File.ReadAllText(path);
        }



    }
}
