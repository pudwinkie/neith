using System;
using System.Collections.Generic;
using System.Text;

namespace Neith.Growl.Connector
{
    /// <summary>
    /// Contains the list of error codes that can be returned in error responses
    /// </summary>
    public static class ErrorCode
    {
        /// <summary>タイムアウト。</summary>
        public const int TIMED_OUT = 200;

        /// <summary>通信障害。</summary>
        public const int NETWORK_FAILURE = 201;

        /// <summary>サポートしないヘッダ又は値が設定されました。</summary>
        public const int INVALID_REQUEST = 300;

        /// <summary>GNTPリクエストではありません。</summary>
        public const int UNKNOWN_PROTOCOL = 301;

        /// <summary>未対応のGNTPバージョンです。</summary>
        public const int UNKNOWN_PROTOCOL_VERSION = 302;

        /// <summary>ヘッダの必須項目が足りません。</summary>
        public const int REQUIRED_HEADER_MISSING = 303;

        /// <summary>認証に失敗しました。</summary>
        public const int NOT_AUTHORIZED = 400;

        /// <summary>未登録のアプリケーションからの通知です。</summary>
        public const int UNKNOWN_APPLICATION = 401;

        /// <summary>アプリケーション登録時に指定されていない通知です。</summary>
        public const int UNKNOWN_NOTIFICATION = 402;

        /// <summary>同一通知を既に受信しています。</summary>
        public const int ALREADY_PROCESSED = 403;

        /// <summary>内部エラーが発生しました。</summary>
        public const int INTERNAL_SERVER_ERROR = 500;
    }

    /// <summary>
    /// GNTPレスポンスのエラータイプ。
    /// </summary>
    public enum ErrorType
    {
        /// <summary>タイムアウト。</summary>
        TimedOut = ErrorCode.TIMED_OUT,

        /// <summary>通信障害。</summary>
        NetworkFailure = ErrorCode.NETWORK_FAILURE,

        /// <summary>サポートしないヘッダ又は値が設定されました。</summary>
        InvalidRequest = ErrorCode.INVALID_REQUEST,

        /// <summary>GNTPリクエストではありません。</summary>
        UnknownProtocol = ErrorCode.UNKNOWN_PROTOCOL,

        /// <summary>未対応のGNTPバージョンです。</summary>
        UnknownProtocolVersion = ErrorCode.UNKNOWN_PROTOCOL_VERSION,

        /// <summary>ヘッダの必須項目が足りません。</summary>
        RequiredHeaderMissing = ErrorCode.REQUIRED_HEADER_MISSING,

        /// <summary>認証に失敗しました。</summary>
        NotAuthorized = ErrorCode.NOT_AUTHORIZED,

        /// <summary>未登録のアプリケーションからの通知です。</summary>
        UnknownApplication = ErrorCode.UNKNOWN_APPLICATION,

        /// <summary>アプリケーション登録時に指定されていない通知です。</summary>
        UnknownNotification = ErrorCode.UNKNOWN_NOTIFICATION,

        /// <summary>同一通知を既に受信しています。</summary>
        AlreadyProcessed = ErrorCode.ALREADY_PROCESSED,

        /// <summary>内部エラーが発生しました。</summary>
        InternalServerError = ErrorCode.INTERNAL_SERVER_ERROR,
    }
}
