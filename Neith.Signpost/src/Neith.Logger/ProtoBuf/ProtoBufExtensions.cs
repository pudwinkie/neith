using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using ProtoBuf;

namespace Neith.Logger
{
    public static class ProtoBufExtensions
    {
        public static void Serialize<T>(this Stream st, T item)
        {
            Serializer.SerializeWithLengthPrefix(st, item, PrefixStyle.Base128, 0);
        }

        public static void SerializeAll<T>(this IEnumerable<T> items, Stream st)
        {
            foreach (var item in items) st.Serialize(item);
        }

        public static void SerializeAll<T>(this IEnumerable<T> items, string path)
        {
            using (var st = File.Create(path)) {
                items.SerializeAll(st);
            }
        }

        /// <summary>
        /// ストリームより指定型のオブジェクトをすべて列挙し、ストリームを閉じます。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="st"></param>
        /// <returns></returns>
        public static T Deserialize<T>(this Stream st)
        {
            return Serializer
                .DeserializeWithLengthPrefix<T>(st, PrefixStyle.Base128, 0);
        }

        /// <summary>
        /// ストリームより指定型のオブジェクトをすべて列挙し、ストリームを閉じます。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="st"></param>
        /// <returns></returns>
        public static IEnumerable<T> EnDeserialize<T>(this Stream st)
        {
            using (st) {
                var items = Serializer.DeserializeItems<T>(st, PrefixStyle.Base128, 0);
                foreach (var item in items) yield return item;
            }
        }

        /// <summary>
        /// ファイル名より指定型のオブジェクトをすべて列挙します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEnumerable<T> EnDeserialize<T>(this string path)
        {
            var st = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            st.Seek(0, SeekOrigin.Begin);
            return st.EnDeserialize<T>();
        }



    }
}
