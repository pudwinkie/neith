//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Windows;
using System.Diagnostics;
using System.ComponentModel;

namespace SampleSupport
{
    public abstract class SampleHarness : IEnumerable<Sample>
    {
        private readonly Dictionary<string, Sample> samples = new Dictionary<string, Sample>();
        private readonly Dictionary<string, Sample> beforeSamples = new Dictionary<string, Sample>();

        public string Title { get; private set; }
        public string Extension { get; private set; }

        public bool HasBeforeSamples { get; private set; }
        public string BeforeString { get; private set; }
        public string AfterString { get; private set; }


        public SampleHarness()
        {
            if (Application.Current == null || Application.Current.MainWindow == null || DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow))
                return;

            Type samplesType = this.GetType();

            this.Title = "Samples";
            string prefix = "Sample";
            this.Extension = "cs";
            this.HasBeforeSamples = false;

            foreach (Attribute a in samplesType.GetCustomAttributes(false))
            {
                if (a is TitleAttribute)
                    this.Title = ((TitleAttribute)a).Title;
                else if (a is PrefixAttribute)
                    prefix = ((PrefixAttribute)a).Prefix;
                else if (a is BeforeAttribute)
                    this.BeforeString = ((BeforeAttribute)a).BeforeString;
                else if (a is AfterAttribute)
                    this.AfterString = ((AfterAttribute)a).AfterString;
                else if (a is ExtensionAttribute)
                    this.Extension = ((ExtensionAttribute)a).Extension;
            }

            string codeFile = String.Format("{0}.{1}", samplesType.Name, this.Extension);

            var allCode = File.ReadAllText(codeFile);

            Regex methodRegex;
            if (this.Extension == "cs")
                methodRegex = new Regex(@"^\s*public (?<async>async )?(?<type>void|Task) (?<name>" + prefix + @"(?<key>\w+?)(?<before>Before)?)\(\)\s*$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            else
                methodRegex = new Regex(@"^\s*Public (?<async>Async )?(?<kind>Sub|Function) (?<name>" + prefix + @"(?<key>\w+?)(?<before>Before)?)\(\)( As (?<type>Task))?\s*$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            var codeReader = new StringReader(allCode);
            string line;
            while ((line = codeReader.ReadLine()) != null)
            {
                var match = methodRegex.Match(line);
                if (match.Success)
                {
                    string type = match.Groups["type"].Value;
                    string kind = match.Groups["kind"].Value;
                    string name = match.Groups["name"].Value;
                    string key = match.Groups["key"].Value;
                    bool before = match.Groups["before"].Success;

                    string methodCategory = "Miscellaneous";
                    string methodTitle = key;
                    string methodDescription = "See code.";
                    var linkedMethods = new List<MethodInfo>();
                    var linkedFields = new List<FieldInfo>();
                    var linkedClasses = new List<Type>();

                    MethodInfo method = samplesType.GetMethod(name);

                    foreach (Attribute a in method.GetCustomAttributes(false))
                    {
                        if (a is CategoryAttribute)
                            methodCategory = ((CategoryAttribute)a).Category;
                        else if (a is TitleAttribute)
                            methodTitle = ((TitleAttribute)a).Title;
                        else if (a is DescriptionAttribute)
                            methodDescription = ((DescriptionAttribute)a).Description;
                        else if (a is LinkedMethodAttribute)
                        {
                            foreach (string methodName in ((LinkedMethodAttribute)a).MethodNames)
                            {
                                MethodInfo linked = samplesType.GetMethod(methodName,
                                                                          (BindingFlags.Public | BindingFlags.NonPublic) |
                                                                          (BindingFlags.Static | BindingFlags.Instance));

                                if (linked == null)
                                    continue;

                                linkedMethods.Add(linked);
                            }
                        }
                        else if (a is LinkedFieldAttribute)
                        {
                            FieldInfo linked = samplesType.GetField(((LinkedFieldAttribute)a).DeclarationName,
                                                                     (BindingFlags.Public | BindingFlags.NonPublic) |
                                                                     (BindingFlags.Static | BindingFlags.Instance));

                            if (linked == null)
                                continue;

                            linkedFields.Add(linked);
                        }
                        else if (a is LinkedClassAttribute)
                        {
                            Type linked = samplesType.GetNestedType(((LinkedClassAttribute)a).ClassName, BindingFlags.Public);

                            if (linked == null)
                                continue;

                            linkedClasses.Add(linked);
                        }
                    }

                    var methodCode = new StringBuilder();

                    foreach (FieldInfo lf in linkedFields)
                    {
                        string fieldBlock;
                        if (this.Extension == "cs")
                            fieldBlock = String.Format("{0} {1}", shortTypeName(lf.FieldType.FullName ?? getGenericFullName(lf.FieldType)), lf.Name);
                        else
                            fieldBlock = String.Format("{1} As {0}", shortTypeName(lf.FieldType.FullName ?? getGenericFullName(lf.FieldType)), lf.Name);

                        methodCode.Append(getCodeBlock(allCode, fieldBlock, false, kind));
                        methodCode.Append(Environment.NewLine);
                    }

                    methodCode.Append(getMethodCodeBlock(allCode, method));
                    
                    foreach (MethodInfo lm in linkedMethods)
                    {
                        methodCode.Append(Environment.NewLine);
                        methodCode.Append(getMethodCodeBlock(allCode, lm));
                    }

                    foreach (Type lc in linkedClasses)
                    {
                        string classKind;
                        if (this.Extension == "cs")
                            classKind = "class";
                        else
                            classKind = "Class";
                        methodCode.Append(Environment.NewLine);
                        methodCode.Append(getCodeBlock(allCode, classKind + " " + lc.Name, true, classKind));
                    }
                    
                    Sample sample = new Sample(this, method, key, methodCategory, methodTitle, methodDescription, methodCode.ToString());

                    if (!before)
                    {
                        this.samples.Add(key, sample);
                    }
                    else
                    {
                        this.beforeSamples.Add(key, sample);
                        this.HasBeforeSamples = true;
                    }
                }
            }
        }

        private static string getGenericFullName(Type t)
        {
            string fullName = t.Name + "[";
            foreach (Type arg in t.GetGenericArguments())
            {
                fullName += "[" + arg.Name + "]";
            }
            return fullName + "]";
        }

        private static string shortTypeName(string typeName)
        {
            bool isGeneric = typeName.Contains("`");
            if (isGeneric)
                return ">";  // match just the closing bracket

            bool isAssemblyQualified = typeName[0] == '[';
            if (isAssemblyQualified)
            {
                int commaPos = typeName.IndexOf(',');
                if (commaPos != -1)
                    return shortTypeName(typeName.Substring(1, commaPos - 1));
                else
                    return shortTypeName(typeName.Substring(1, typeName.Length - 2));
            }
            else
            {
                switch (typeName)
                {
                    case "System.Void":     return "void";
                    case "System.Int16":    return "short";
                    case "System.Int32":    return "int";
                    case "System.Int64":    return "long";
                    case "System.Single":   return "float";
                    case "System.Double":   return "double";
                    case "System.String":   return "string";
                    case "System.Char":     return "char";
                    case "System.Boolean":  return "bool";
                        
                    /* other primitive types omitted */

                    default:
                        int lastDotPos = typeName.LastIndexOf('.');
                        int lastPlusPos = typeName.LastIndexOf('+');
                        int startPos = Math.Max(lastDotPos, lastPlusPos) + 1;
                        return typeName.Substring(startPos, typeName.Length - startPos);
                }
            }
        }

        private static string shortKindName(string typename)
        {
            if (typename == "System.Void")
                return "Sub";
            else
                return "Function";
        }

        private string getMethodCodeBlock(string allCode, MethodInfo method)
        {
            string kind = shortKindName(method.ReturnType.FullName);
            string methodBlock;
            if (this.Extension == "cs")
                methodBlock = String.Format("{0} {1}{2}", shortTypeName(method.ReturnType.FullName ?? getGenericFullName(method.ReturnType)), method.Name, method.IsGenericMethod ? "<" : "(");
            else
                methodBlock = String.Format("{0} {1}(", shortKindName(method.ReturnType.FullName), method.Name);

            return getCodeBlock(allCode, methodBlock, true, kind);
        }

        private string getCodeBlock(string allCode, string blockName, bool hasBody, string kind)
        {
            int blockStartTokenPos = allCode.IndexOf(blockName, StringComparison.OrdinalIgnoreCase);
            if (blockStartTokenPos == -1)
                return blockName + " code not found.";

            int blockStart = allCode.LastIndexOf(Environment.NewLine, blockStartTokenPos, StringComparison.OrdinalIgnoreCase);
            if (blockStart == -1)
                blockStart = 0;
            else
                blockStart += Environment.NewLine.Length;

            int blockEnd;

            if (this.Extension == "cs")
            {
                int pos = blockStart;
                int braceCount = 0;
                char c;
                do
                {
                    pos++;

                    c = allCode[pos];
                    switch (c)
                    {
                        case '{':
                            braceCount++;
                            break;

                        case '}':
                            braceCount--;
                            break;
                    }
                } while (pos < allCode.Length &&
                            (hasBody && !(c == '}' && braceCount == 0)) ||
                            (!hasBody && (c != ';')));

                blockEnd = pos;
            }
            else
            {
                if (hasBody)
                {
                    int depth = 1;
                    int kindPos = blockStartTokenPos;

                    do
                    {
                        kindPos = allCode.IndexOf(kind, kindPos + kind.Length, StringComparison.OrdinalIgnoreCase);

                        if (!IsWholeWord(allCode, kindPos, kind.Length))
                            continue;

                        if (allCode.Substring(kindPos - 4, 3) != "End")
                            depth++;
                        else
                            depth--;
                    } while (depth > 0);

                    blockEnd = kindPos + kind.Length;
                }
                else
                {
                    blockEnd = allCode.IndexOf("\r\n", blockStartTokenPos) - 1;
                }
            }
            
            string blockCode = allCode.Substring(blockStart, blockEnd - blockStart + 1);

            return removeIndent(blockCode);
        }

        internal static bool IsWholeWord(string text, int startPos, int length)
        {
            return ((startPos - 1 < 0) || !char.IsLetterOrDigit(text[startPos - 1])) &&
                   ((startPos + length >= text.Length) || !char.IsLetterOrDigit(text[startPos + length]));
        }

        private static string removeIndent(string code)
        {
            int indentSpaces = 0;
            while (code[indentSpaces] == ' ')
            {
                indentSpaces++;
            }

            StringBuilder builder = new StringBuilder();
            string[] codeLines = code.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (string line in codeLines)
            {
                if (indentSpaces < line.Length)
                    builder.AppendLine(line.Substring(indentSpaces));
                else
                    builder.AppendLine();
            }

            return builder.ToString();
        }


        public virtual void InitSample() {}

        public virtual void HandleException(Exception e) {
            Console.Write(e);
        }

        public virtual CancellationTokenSource CancellationTokenSource { get { return null; } set { } }
        public ProgressBar ProgressBar { get; set; }


        public Sample this[string key]
        {
            get { return this[key, false]; }
        }

        public Sample this[string key, bool before]
        {
            get
            {
                if (!before)
                    return samples[key];
                else
                    return beforeSamples[key];
            }
        }

        public bool TryGetSample(string key, bool before, out Sample sample)
        {
            if (!before)
                return samples.TryGetValue(key, out sample);
            else
                return beforeSamples.TryGetValue(key, out sample);
        }

        public IEnumerable<string> Keys
        {
            get
            {
                return samples.Keys;
            }
        }

        public IEnumerable<SampleCategory> Categories
        {
            get
            {
                var cats = from s in samples.Values
                           group s by s.Category into g
                           select new SampleCategory { Category = g.Key, Samples = g };
                return cats;
            }
        }

        public struct SampleCategory
        {
            public string Category { get; set; }
            public IEnumerable<Sample> Samples { get; set; }
        }

        public IEnumerator<Sample> GetEnumerator()
        {
            return samples.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Sample
    {
        private readonly SampleHarness harness;
        private readonly MethodInfo method;
        private readonly string key;
        private readonly string category;
        private readonly string title;
        private readonly string description;
        private readonly string code;


        public Sample(SampleHarness harness, MethodInfo method, string key, string category,
                      string title, string description, string code)
        {
            this.harness = harness;
            this.method = method;
            this.key = key;
            this.category = category;
            this.title = title;
            this.description = description;
            this.code = code;
        }
        

        public SampleHarness Harness
        {
            get { return harness; }
        }
        
        public MethodInfo Method
        {
            get { return method; }
        }

        public string Key
        {
            get { return key; }
        }

        public string Category
        {
            get { return category; }
        }
        
        public string Title
        {
            get { return title; }
        }
        
        public string Description
        {
            get { return description; }
        }
        
        public string Code
        {
            get { return code; }
        }
        

        public Task Invoke()
        {
            harness.InitSample();
            var task = (method.Invoke(this.harness, null)) as Task;
            return task;
        }

        public Task InvokeSafe()
        {
            try
            {
                return Invoke();
            }
            catch (TargetInvocationException e)
            {
                harness.HandleException(e.InnerException);
                return null;
            }
        }

        public override string ToString()
        {
            return Title;
        }
    }

    [global::System.AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class TitleAttribute : Attribute
    {
        public TitleAttribute(string title)
        {
            this.Title = title;
        }

        public string Title {get; set;}
    }

    [global::System.AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PrefixAttribute : Attribute
    {
        public PrefixAttribute(string prefix)
        {
            this.Prefix = prefix;
        }

        public string Prefix {get; set;}
    }

    [global::System.AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class BeforeAttribute : Attribute
    {
        public BeforeAttribute(string before)
        {
            this.BeforeString = before;
        }

        public string BeforeString { get; set; }
    }

    [global::System.AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class AfterAttribute : Attribute
    {
        public AfterAttribute(string after)
        {
            this.AfterString = after;
        }

        public string AfterString { get; set; }
    }

    [global::System.AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ExtensionAttribute : Attribute
    {
        public ExtensionAttribute(string extension)
        {
            this.Extension = extension;
        }

        public string Extension { get; set; }
    }

    [global::System.AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class CategoryAttribute : Attribute
    {
        public CategoryAttribute(string category)
        {
            this.Category = category;
        }

        public string Category { get; set; }
    }

    [global::System.AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class DescriptionAttribute : Attribute
    {
        public DescriptionAttribute(string description)
        {
            this.Description = description;
        }

        public string Description {get; set;}
    }

    [global::System.AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class LinkedMethodAttribute : Attribute
    {

        public LinkedMethodAttribute(params string[] methodNames)
        {
            this.MethodNames = methodNames;
        }


        public string[] MethodNames { get; set; }
    }

    [global::System.AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class LinkedClassAttribute : Attribute
    {
        public LinkedClassAttribute(string className)
        {
            this.ClassName = className;
        }

        public string ClassName { get; set; }
    }

    [global::System.AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class LinkedFieldAttribute : Attribute
    {
        public LinkedFieldAttribute(string declName)
        {
            this.DeclarationName = declName;
        }

        public string DeclarationName { get; set; }
    }

    public static class Console
    {
        private static string text;
        public static string Text
        {
            get
            {
                return text;
            }
            private set
            {
                text = value;
                TextChanged(null, new EventArgs());
            }
        }

        public static event EventHandler TextChanged;

        public static void Clear()
        {
            Text = "";
        }

        public static void Write(object value)
        {
            Text += value.ToString();
        }

        public static void Write(string format, params object[] arg)
        {
            Text += String.Format(format, arg);
        }

        public static void WriteLine()
        {
            Text += "\n";
        }

        public static void WriteLine(object value)
        {
            Text += (value.ToString() + "\n");
        }

        public static void WriteLine(string format, params object[] arg)
        {
            Text += (String.Format(format, arg) + "\n");
        }
    }
}
