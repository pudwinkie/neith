using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neith.Growl.Connector;

namespace Neith.Logger.GNTP
{
    /// <summary>
    /// GNTPプロトコルでエラー応答を返すための例外
    /// </summary>
    public class GNTPException : Exception
    {
        /// <summary>エラーコード</summary>
        public ErrorType ErrorType { get; private set; }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="errorType"></param>
        public GNTPException(ErrorType errorType)
            : base()
        {
            ErrorType = errorType;
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="errorType"></param>
        /// <param name="message"></param>
        public GNTPException(ErrorType errorType, string message)
            : base(message)
        {
            ErrorType = errorType;
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="errorType"></param>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public GNTPException(ErrorType errorType, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorType = errorType;
        }

    }
}