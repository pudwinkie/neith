using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neith.Growl.Connector;
using Neith.Growl.CoreLibrary;

namespace Neith.Logger.GNTP.Parsers
{
    /// <summary>
    /// パーサ：ステージ３。意味解析を行なう。
    /// 暗号キーの認証も行なう。
    /// </summary>
    public class Stage3
    {
        private readonly Stage1 Stage1;
        private readonly Stage2 Reader;
        private readonly bool PasswordRequired;
        private readonly PasswordManager PasswordManager;




        /// <summary>リクエスト種別</summary>
        public RequestType Directive { get { return _Directive.Value; } }
        private readonly Lazy<RequestType> _Directive;

        /// <summary>ハッシュアルゴリズム</summary>
        public Cryptography.HashAlgorithmType KeyHashAlgorithm { get { return _KeyHashAlgorithm.Value; } }
        private readonly Lazy<Cryptography.HashAlgorithmType> _KeyHashAlgorithm;

        /// <summary>暗号アルゴリズム</summary>
        public Cryptography.SymmetricAlgorithmType EncryptionAlgorithm { get { return _EncryptionAlgorithm.Value; } }
        private readonly Lazy<Cryptography.SymmetricAlgorithmType> _EncryptionAlgorithm;

        /// <summary>パスワードを設定します。</summary>
        public Password Password { get; private set; }

        /// <summary>Keyを設定します。</summary>
        public Key Key { get { return Stage1.Key; } private set { Stage1.Key = value; } }

        public Stage3(Stage1 stage1, Stage2 stage2Reader, bool passwordRequired, PasswordManager passwordManager)
        {
            Stage1 = stage1;
            Reader = stage2Reader;
            PasswordRequired = passwordRequired;
            PasswordManager = passwordManager;
            _Directive = new Lazy<RequestType>(() =>
            {
                try {
                    return (RequestType)Enum.Parse(typeof(RequestType), Reader.Directive);
                }
                catch (Exception ex) {
                    throw new ParserException(ErrorType.InvalidRequest, "Directive", ex);
                }
            });
            _KeyHashAlgorithm = new Lazy<Cryptography.HashAlgorithmType>(() =>
            {
                try {
                    return Cryptography.GetKeyHashType(Reader.KeyHashAlgorithm);
                }
                catch (Exception ex) {
                    throw new ParserException(ErrorType.InvalidRequest, "KeyHashAlgorithm", ex);
                }
            });
            _EncryptionAlgorithm = new Lazy<Cryptography.SymmetricAlgorithmType>(() =>
            {
                try {
                    return Cryptography.GetEncryptionType(Reader.EncryptionAlgorithm);
                }
                catch (Exception ex) {
                    throw new ParserException(ErrorType.InvalidRequest, "EncryptionAlgorithm", ex);
                }
            });

        }

        /// <summary>
        /// ヘッダを解釈します。暗号keyを作成します。
        /// </summary>
        public async void ReadHeader()
        {
            var header = await Reader.ReadHeader();
            SetEncryptionKey();
        }

        private void SetEncryptionKey()
        {
            if (PasswordRequired || Directive == RequestType.SUBSCRIBE || KeyHashAlgorithm != Cryptography.HashAlgorithmType.NONE) {
                if (KeyHashAlgorithm == Cryptography.HashAlgorithmType.NONE) {
                    throw new ParserException(ErrorType.NotAuthorized, "required KeyHashAlgorithm");
                }
                Key key;
                var authorized = PasswordManager.IsValid(
                    Reader.KeyHash,
                    Reader.Salt,
                    KeyHashAlgorithm,
                    EncryptionAlgorithm,
                    out key);
                if (!authorized) throw new ParserException(ErrorType.NotAuthorized, "unmatch");
                Key = key;
            }
            else {
                Key = Neith.Growl.Connector.Key.None;
            }
        }


    }
}
