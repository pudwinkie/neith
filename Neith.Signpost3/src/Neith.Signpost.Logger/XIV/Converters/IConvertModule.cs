using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.ComponentModel.Composition;

namespace Neith.Signpost.Logger.XIV.Converters
{
    public interface IConvertModule
    {
        Regex Regex { get; }
        int CallCount { get; }
        SrcItem LastItem { get; }
        XElement Calc(SrcItem src, Match m);
    }

    public interface IConvertMetadata
    {
        /// <summary>優先度。大きい番号の項目を優先する。</summary>
        int Priority { get; }

        /// <summary>ID。このモジュールが利用されるIDを列挙する。</summary>
        int[] Id { get; }

    }


    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ConverterMetadataAttribute : ExportAttribute
    {
        public ConverterMetadataAttribute(int priority, params int[] id)
            : base(typeof(IConvertMetadata))
        {
            if (id == null) throw new ArgumentException("idは必ず１つ以上指定してください");
            Priority = priority;
            Id = id;
        }

        /// <summary>優先度。大きい番号の項目を優先する。</summary>
        public int Priority { get; set; }

        /// <summary>ID。このモジュールが利用されるIDを列挙する。</summary>
        public int[] Id { get; set; }
    }



}
