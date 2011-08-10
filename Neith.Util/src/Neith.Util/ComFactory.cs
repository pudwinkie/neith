using System;
using System.Collections.Generic;
using System.Text;

namespace Neith.Util
{
    /// <summary>
    /// Com参照ユーティリティ
    /// </summary>
    public static class ComFactory
    {
        /// <summary>
        /// COMオブジェクトへの参照を作成および取得する
        /// </summary>
        /// <param name="progId">作成するオブジェクトのプログラムID</param>
        /// <param name="serverName">
        /// オブジェクトが作成されるネットワーク サーバーの名前
        /// </param>
        /// <returns>作成されたCOMオブジェクト</returns>
        public static object CreateObject(string progId, string serverName)
        {
            Type t;
            if (serverName == null || serverName.Length == 0)
                t = Type.GetTypeFromProgID(progId);
            else
                t = Type.GetTypeFromProgID(progId, serverName, true);
            return Activator.CreateInstance(t);
        }

        /// <summary>
        /// COMオブジェクトへの参照を作成および取得する
        /// </summary>
        /// <param name="progId">作成するオブジェクトのプログラムID</param>
        /// <returns>作成されたCOMオブジェクト</returns>
        public static object CreateObject(string progId)
        {
            return CreateObject(progId, null);
        }
    }
}