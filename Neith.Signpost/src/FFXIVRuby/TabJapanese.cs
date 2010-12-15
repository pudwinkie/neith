using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Data;

namespace FFXIVRuby
{
public class TabJapanese
{
    // Fields
    private FixedFormSentenceLibraryDataSet dataset = new FixedFormSentenceLibraryDataSet();
    public static string TabStringEnd = "》";
    public static string TabStringStart = "《";

    // Methods
    public string GetJapanese(string text)
    {
        Regex regex = new Regex(string.Format("{0}[0-9a-fA-F]+{1}", TABConvertor.TabStartString, TABConvertor.TabTerminalString));
        MatchCollection matchs = regex.Matches(text);
        string[] strArray = regex.Split(text);
        StringBuilder builder = new StringBuilder();
        builder.Append(strArray[0]);
        for (int i = 0; i < matchs.Count; i++)
        {
            builder.Append(this.GetJapaneseFromTabCode(matchs[i].Value));
            builder.Append(strArray[i + 1]);
        }
        return builder.ToString();
    }

    public string GetJapaneseFromTabCode(string tabcodehex)
    {
        string tabCode = tabcodehex.Replace(TABConvertor.TabStartString, "").Replace(TABConvertor.TabTerminalString, "");
        FixedFormSentenceLibraryDataSet.TabStringRow row = this.dataset.TabString.FindByTabCode(tabCode);
        if (row != null)
        {
            return string.Format("{0}{1}{2}", TabStringStart, row.Japanese, TabStringEnd);
        }
        return tabcodehex;
    }

    public string GetTabCode(string text)
    {
        Regex regex = new Regex(string.Format("{0}.+?{1}", TabStringStart, TabStringEnd));
        MatchCollection matchs = regex.Matches(text);
        string[] strArray = regex.Split(text);
        StringBuilder builder = new StringBuilder();
        builder.Append(strArray[0]);
        for (int i = 0; i < matchs.Count; i++)
        {
            builder.Append(this.GetTabCodeFormJapanese(matchs[i].Value));
            builder.Append(strArray[i + 1]);
        }
        return builder.ToString();
    }

    public string GetTabCodeFormJapanese(string japanese)
    {
        string str = japanese.Replace(TabStringStart, "").Replace(TabStringEnd, "");
        DataRow[] rowArray = this.dataset.TabString.Select(string.Format("Japanese='{0}'", str));
        if (rowArray.Length > 0)
        {
            return string.Format("{0}{1}{2}", TABConvertor.TabStartString, ((FixedFormSentenceLibraryDataSet.TabStringRow) rowArray[0]).TabCode, TABConvertor.TabTerminalString);
        }
        return japanese;
    }

    public void ReadXml(Stream st)
    {
        this.dataset.Clear();
        this.dataset.ReadXml(st, XmlReadMode.Auto);
    }

    public void ReadXml(string path)
    {
        this.dataset.Clear();
        this.dataset.ReadXml(path);
    }
}

}
