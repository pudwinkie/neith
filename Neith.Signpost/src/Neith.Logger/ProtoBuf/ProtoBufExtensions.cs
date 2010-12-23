using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace ProtoBuf
{
    public static class ProtoBufExtensions
    {


        /// <summary>
        /// ストリームより指定型のオブジェクトをすべて列挙し、ストリームを閉じます。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="st"></param>
        /// <returns></returns>
        public static IEnumerable<T> EnDeserialize<T>(this Stream st)
        {
            using (st) {
                while (st.Length != st.Position) {
                    var obj = Serializer.Deserialize<T>(st);
                    if (obj == null) yield break;
                    yield return obj;
                }
            }
        }

        public static IEnumerable<T> EnDeserialize<T>(this string path)
        {
            var st = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return EnDeserialize<T>(st);
        }



    }
}
