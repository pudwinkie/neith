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
        /// <summary>�^�C���A�E�g�B</summary>
        public const int TIMED_OUT = 200;

        /// <summary>�ʐM��Q�B</summary>
        public const int NETWORK_FAILURE = 201;

        /// <summary>�T�|�[�g���Ȃ��w�b�_���͒l���ݒ肳��܂����B</summary>
        public const int INVALID_REQUEST = 300;

        /// <summary>GNTP���N�G�X�g�ł͂���܂���B</summary>
        public const int UNKNOWN_PROTOCOL = 301;

        /// <summary>���Ή���GNTP�o�[�W�����ł��B</summary>
        public const int UNKNOWN_PROTOCOL_VERSION = 302;

        /// <summary>�w�b�_�̕K�{���ڂ�����܂���B</summary>
        public const int REQUIRED_HEADER_MISSING = 303;

        /// <summary>�F�؂Ɏ��s���܂����B</summary>
        public const int NOT_AUTHORIZED = 400;

        /// <summary>���o�^�̃A�v���P�[�V��������̒ʒm�ł��B</summary>
        public const int UNKNOWN_APPLICATION = 401;

        /// <summary>�A�v���P�[�V�����o�^���Ɏw�肳��Ă��Ȃ��ʒm�ł��B</summary>
        public const int UNKNOWN_NOTIFICATION = 402;

        /// <summary>����ʒm�����Ɏ�M���Ă��܂��B</summary>
        public const int ALREADY_PROCESSED = 403;

        /// <summary>�����G���[���������܂����B</summary>
        public const int INTERNAL_SERVER_ERROR = 500;
    }

    /// <summary>
    /// GNTP���X�|���X�̃G���[�^�C�v�B
    /// </summary>
    public enum ErrorType
    {
        /// <summary>�^�C���A�E�g�B</summary>
        TimedOut = ErrorCode.TIMED_OUT,

        /// <summary>�ʐM��Q�B</summary>
        NetworkFailure = ErrorCode.NETWORK_FAILURE,

        /// <summary>�T�|�[�g���Ȃ��w�b�_���͒l���ݒ肳��܂����B</summary>
        InvalidRequest = ErrorCode.INVALID_REQUEST,

        /// <summary>GNTP���N�G�X�g�ł͂���܂���B</summary>
        UnknownProtocol = ErrorCode.UNKNOWN_PROTOCOL,

        /// <summary>���Ή���GNTP�o�[�W�����ł��B</summary>
        UnknownProtocolVersion = ErrorCode.UNKNOWN_PROTOCOL_VERSION,

        /// <summary>�w�b�_�̕K�{���ڂ�����܂���B</summary>
        RequiredHeaderMissing = ErrorCode.REQUIRED_HEADER_MISSING,

        /// <summary>�F�؂Ɏ��s���܂����B</summary>
        NotAuthorized = ErrorCode.NOT_AUTHORIZED,

        /// <summary>���o�^�̃A�v���P�[�V��������̒ʒm�ł��B</summary>
        UnknownApplication = ErrorCode.UNKNOWN_APPLICATION,

        /// <summary>�A�v���P�[�V�����o�^���Ɏw�肳��Ă��Ȃ��ʒm�ł��B</summary>
        UnknownNotification = ErrorCode.UNKNOWN_NOTIFICATION,

        /// <summary>����ʒm�����Ɏ�M���Ă��܂��B</summary>
        AlreadyProcessed = ErrorCode.ALREADY_PROCESSED,

        /// <summary>�����G���[���������܂����B</summary>
        InternalServerError = ErrorCode.INTERNAL_SERVER_ERROR,
    }
}
