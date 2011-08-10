using System;
using Neith.Growl.CoreLibrary;

namespace Neith.Growl.Connector
{
    /// <summary>
    /// ヘッダデータ。
    /// </summary>
    public interface IHeader
    {
        /// <summary>名称</summary>
        string Name { get; }

        /// <summary>値</summary>
        string Value { get; }

        /// <summary>実際のヘッダ名</summary>
        string ActualName { get; }

        /// <summary>有効なヘッダであればtrue</summary>
        bool IsValid { get; }

        /// <summary>空行であるならtrue</summary>
        bool IsBlankLine { get; }

        /// <summary>カスタムヘッダであるならtrue</summary>
        bool IsCustomHeader { get; }

        /// <summary>アプリケーション固有のヘッダであるならtrue</summary>
        bool IsDataHeader { get; }

        /// <summary>識別子ヘッダであればtrue</summary>
        bool IsIdentifier { get; }

        /// <summary>バイナリリソースのヘッダであるならtrue</summary>
        bool IsGrowlResourcePointer { get; }

        /// <summary>リソースID</summary>
        string GrowlResourcePointerID { get; }

        /// <summary>リソース</summary>
        BinaryData GrowlResource { get; set; }
    }
}
