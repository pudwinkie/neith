using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Neith.Util.Runtime
{
    /// <summary>
    /// オブジェクトのシリアライズに関するユーティリティクラスです。
    /// </summary>
    public static class SerializeUtil
    {
        /// <summary>
        /// 圧縮バイナリ形式で読み書きします。
        /// </summary>
        public static class PackedBinary
        {
            /// <summary>
            /// オブジェクトを圧縮バイナリ形式で保存します。
            /// </summary>
            /// <param name="target">保存対象</param>
            /// <param name="path">保存場所</param>
            public static void Save<T>(T target, string path)
            {
                using (MemoryStream mem = new MemoryStream()) {
                    BinaryFormatter fomatter = new BinaryFormatter();
                    fomatter.Serialize(mem, target);
                    using (Stream fs = File.OpenWrite(path))
                    using (Stream pack = new GZipStream(fs, CompressionMode.Compress)) {
                        pack.Write(mem.GetBuffer(), 0, (int)mem.Length);
                    }
                }
            }

            /// <summary>
            /// オブジェクトを圧縮バイナリ形式で読み込みます。
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static T Load<T>(string path)
            {
                using (Stream fs = File.OpenRead(path))
                using (Stream unpack = new GZipStream(fs, CompressionMode.Decompress)) {
                    BinaryFormatter fomatter = new BinaryFormatter();
                    return (T)fomatter.Deserialize(unpack);
                }
            }
        }

        /// <summary>
        /// XML形式で読み書きします。
        /// </summary>
        public static class XML
        {
            /// <summary>
            /// オブジェクトをファイルに書き込みます。
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="target"></param>
            /// <param name="path"></param>
            public static void Save<T>(T target, string path)
            {
                XmlSerializer fomatter = new XmlSerializer(typeof(T));
                using (Stream fs = File.OpenWrite(path)) {
                    fomatter.Serialize(fs, target);
                }
            }

            /// <summary>
            /// ファイルを読み込み、オブジェクトに変換します。
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="path"></param>
            /// <returns></returns>
            public static T Load<T>(string path)
            {
                XmlSerializer fomatter = new XmlSerializer(typeof(T));
                using (Stream fs = File.OpenRead(path)) {
                    return (T)fomatter.Deserialize(fs);
                }
            }
        }



    }
}
