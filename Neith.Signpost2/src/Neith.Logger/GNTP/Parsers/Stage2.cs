using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Neith.Growl.Connector;

namespace Neith.Logger.GNTP.Parsers
{
    /// <summary>
    /// パーサ：ステージ２。構文チェックを行う。
    /// </summary>
    public class Stage2
    {
        /// <summary>
        /// GNTPヘッダ（ローカルリクエスト）
        /// </summary>
        private static readonly Regex RegHeaderLocal = new Regex(@"GNTP/(?<Version>.\..)\s+(?<Directive>\S+)\s+(((?<EncryptionAlgorithm>\S+):(?<IV>\S+))|((?<EncryptionAlgorithm>\S+)))\s*[\r\n]", RegexOptions.Compiled);

        /// <summary>
        /// GNTPヘッダ（リモートリクエスト。パスワード必須）
        /// </summary>
        private static readonly Regex RegHeaderRemote = new Regex(@"GNTP/(?<Version>.\..)\s+(?<Directive>\S+)\s+(((?<EncryptionAlgorithm>\S+):(?<IV>\S+))\s+|((?<EncryptionAlgorithm>\S+)\s+))(?<KeyHashAlgorithm>(\S+)):(?<KeyHash>(\S+))\.(?<Salt>(\S+))\s*[\r\n]", RegexOptions.Compiled);

        /// <summary>
        /// テキスト行
        /// </summary>
        private static readonly Regex RegTextBlock = new Regex(TEXT_BLOCK, RegexOptions.Compiled);
        private const string CRLF = @"\r\n";
        private const string KEY_VALUE = @"(?<Key>[^\r\n]+?): (?<Value>.+?)" + CRLF;
        private const string TEXT_BLOCK = "^(" + KEY_VALUE + ")+" + CRLF + "$";

        /// <summary>
        /// バイナリブロックヘッダ。
        /// </summary>
        private static readonly Regex RegBinHeader = new Regex(BIN_HEADER, RegexOptions.Compiled);
        private const string BIN_HEADER = @"^Identifier: (?<Key>.+?)" + CRLF
            + @"^Length: (?<Length>\d+?)" + CRLF + "$";

        private readonly Stage1 Reader;
        private readonly Regex RegHeader;

        /// <summary>GNTPバージョン</summary>
        public string Version { get; private set; }

        /// <summary>リクエストタイプ</summary>
        public string Directive { get; private set; }

        /// <summary>暗号アルゴリズム</summary>
        public string EncryptionAlgorithm { get; private set; }

        /// <summary>暗号IV</summary>
        public string IV { get; private set; }

        /// <summary>hashアルゴリズム</summary>
        public string KeyHashAlgorithm { get; private set; }

        /// <summary>hash</summary>
        public string KeyHash { get; private set; }

        /// <summary>Salt</summary>
        public string Salt { get; private set; }

        public Stage2(Stage1 stage1Reader,bool isLocal)
        {
            Reader = stage1Reader;
            RegHeader = isLocal ? RegHeaderLocal : RegHeaderRemote;
        }

        /// <summary>
        /// ヘッダを解釈します。
        /// </summary>
        public async Task<Stage2> ReadHeader()
        {
            var text = await Reader.ReadHeader();
            var m = RegHeader.Match(text);
            if (!m.Success) throw new ParserException(ErrorType.InvalidRequest);
            Version = m.Groups["Version"].Value;
            Directive = m.Groups["Directive"].Value;
            EncryptionAlgorithm = m.Groups["EncryptionAlgorithm"].Value;
            IV = m.Groups["IV"].Value;
            KeyHashAlgorithm = m.Groups["KeyHashAlgorithm"].Value;
            KeyHash = m.Groups["KeyHash"].Value;
            Salt = m.Groups["Salt"].Value.ToUpper();
            return this;
        }

        /// <summary>
        /// テキストブロックを解釈します。
        /// </summary>
        public async Task<IEnumerable<KeyValuePair<string,string>>> ReadTextBlock()
        {
            var lines = await Reader.ReadTextBlock();
            return EnLine(lines);
        }
        private IEnumerable<KeyValuePair<string, string>> EnLine(string lines)
        {
            var m = RegTextBlock.Match(lines);
            if (!m.Success) throw new ParserException(ErrorType.InvalidRequest, "Missing TextBlock Format");
            var Keys = m.Groups["Key"].Captures;
            var Values = m.Groups["Value"].Captures;
            for (int i = 0; i < Keys.Count; i++)
                yield return new KeyValuePair<string, string>(Keys[i].Value, Values[i].Value);
        }

        /// <summary>
        /// バイナリブロックを解釈します。
        /// </summary>
        /// <returns></returns>
        public async Task<KeyValuePair<string, byte[]>> ReadBinBlock()
        {
            var header = await Reader.ReadHeader();
            header += await Reader.ReadHeader();
            var m = RegBinHeader.Match(header);
            if (!m.Success) throw new ParserException(ErrorType.InvalidRequest, "Missing BinBlock Header");
            var Key = m.Groups["Key"].Value;
            var Length = int.Parse(m.Groups["Length"].Value);
            if (Length <= 0) throw new ParserException(ErrorType.InvalidRequest, "Invalid Length");
            var data = await Reader.ReadBinBlock(Length);
            return new KeyValuePair<string, byte[]>(Key, data);
        }

    }
}
