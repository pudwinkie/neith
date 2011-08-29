using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SampleSupport;
using System.Threading.Tasks;
using System.IO;

using Console = SampleSupport.Console;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SampleSupport
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Sample currentSample;
        private Task currentTask;
        private bool before;

        public MainWindow()
        {
            InitializeComponent();

            Console.TextChanged += new EventHandler(Console_TextChanged);

            ChangeSample(null);
        }

        private void Console_TextChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate { OutputTextBox.Text = Console.Text; }));
        }

        private void RootNode_Loaded(object sender, RoutedEventArgs e)
        {
            RootNode.IsSelected = true;

            foreach (var child in RootNode.Items)
            {
                var childItem = (TreeViewItem)SamplesTreeView.ContainerFromItem(child);
                childItem.IsExpanded = true;
            }
        }

        private void SamplesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            currentTask = null;
            CancelButton.IsEnabled = false;

            currentSample = e.NewValue as Sample;

            if (currentSample != null)
            {
                Sample beforeSample;
                if (currentSample.Harness.TryGetSample(currentSample.Key, true, out beforeSample))
                {
                    BeforeRadioButton.Content = currentSample.Harness.BeforeString;
                    BeforeRadioButton.Visibility = Visibility.Visible;
                    AfterRadioButton.Content = currentSample.Harness.AfterString;
                    AfterRadioButton.Visibility = Visibility.Visible;
                }
                else
                {
                    BeforeRadioButton.Visibility = Visibility.Collapsed;
                    AfterRadioButton.Visibility = Visibility.Collapsed;
                }

                AfterRadioButton.IsChecked = false;
                AfterRadioButton.IsChecked = true;
            }
            else
            {
                BeforeRadioButton.Visibility = Visibility.Collapsed;
                AfterRadioButton.Visibility = Visibility.Collapsed;

                CodeRichTextBox.Document.Blocks.Clear();
                ClearOutputTextBox();
                RunButton.IsEnabled = false;
                CancelButton.IsEnabled = false;
                DescriptionTextBox.Text = "Select a sample from the tree to the left.";

                var item = (TreeViewItem)SamplesTreeView.ContainerFromItem(SamplesTreeView.SelectedItem);
                item.IsExpanded = true;
            }
        }

        private void Sample_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if( this.currentSample != null ) {
                    RunCurrentSample();
                }
            }
        }

        static readonly string[] csKeywords = {
            "as", "do", "if", "in", "is", "for", "int", "new", "out", "ref", "try", "base", 
            "bool", "byte", "case", "char", "else", "enum", "goto", "lock", "long", "null", 
            "this", "true", "uint", "void", "break", "catch", "class", "const", "event", "false", 
            "fixed", "float", "sbyte", "short", "throw", "ulong", "using", "where", "while", 
            "yield", "double", "extern", "object", "params", "public", "return", "sealed", 
            "sizeof", "static", "string", "struct", "switch", "typeof", "unsafe", "ushort", 
            "checked", "decimal", "default", "finally", "foreach", "partial", "private", 
            "virtual", "abstract", "continue", "delegate", "explicit", "implicit", "internal", 
            "operator", "override", "readonly", "volatile",  
            "interface", "namespace", "protected", "unchecked",
            "stackalloc", 
            "from", "in", "where", "select", "join", "equals", "let", "on", "group", "by", 
            "into", "orderby", "ascending", "descending", "var",
            "await", "async"
        };

        static readonly string[] vbKeywords = {
            "AddHandler", "AddressOf", "Aggregate", "Alias", "And", "AndAlso", "Ansi", "As",
            "Assembly", "Auto", "Binary", "Boolean", "ByRef", "Byte", "ByVal", "Call", "Case",
            "Catch", "CBool", "CByte", "CChar", "CDate", "CDbl", "CDec", "Char", "CInt", "CLng",
            "CObj", "Compare", "Const", "Continue", "CSByte", "CShort", "CSng", "CStr", "CType",
            "CUInt", "CULng", "CUShort", "Custom", "Date", "Decimal", "Declare", "Default",
            "Delegate", "Dim", "DirectCast", "Distinct", "Do", "Double", "Each", "Else", "ElseIf",
            "End", "EndIf", "Enum", "Equals", "Erase", "Error", "Event", "Exit", "Explicit",
            "False", "Finally", "For", "Friend", "From", "Function", "Get", "GetType", "GetXMLNamespace",
            "Global", "GoSub", "GoTo", "Group By", "Group Join", "Handles", "If", "Implements",
            "Imports", "In", "Inherits", "Integer", "Interface", "Into", "Is", "IsFalse", "IsNot",
            "IsTrue", "Join", "Let", "Lib", "Like", "Long", "Loop", "Me", "Mid", "Mod", "Module",
            "MustInherit", "MustOverride", "MyBase", "MyClass", "Namespace", "Narrowing", "New",
            "Next", "Not", "Nothing", "NotInheritable", "NotOverridable", "Object", "Of", "Off",
            "On", "Operator", "Option", "Optional", "Or", "Order By", "OrElse", "Out", "Overloads",
            "Overridable", "Overrides", "ParamArray", "Partial", "Preserve", "Private", "Property",
            "Protected", "Public", "RaiseEvent", "ReadOnly", "ReDim", "REM", "RemoveHandler",
            "Resume", "Return", "SByte", "Select", "Set", "Shadows", "Shared", "Short", "Single",
            "Skip", "Static", "Step", "Stop", "Strict", "String", "Structure", "Sub", "SyncLock",
            "Take", "Text", "Then", "Throw", "To", "True", "Try", "TryCast", "TypeOf", "UInteger",
            "ULong", "Unicode", "Until", "UShort", "Using", "Variant", "Wend", "When", "Where",
            "While", "Widening", "With", "WithEvents", "WriteOnly", "Xor",
            "Await", "Async", "Iterator"
        };


        private static void addColorizedText(RichTextBox rtb, string text, string extension)
        {
            var keywordBrush = new SolidColorBrush(Colors.Blue);

            string[] keywords;
            if (extension == "cs")
                keywords = csKeywords;
            else
                keywords = vbKeywords;

            var keywordString = @"\b(" + String.Join("|", keywords) + @")\b";
            Regex keywordRegex = new Regex(keywordString, RegexOptions.Compiled);

            int lastEnd = 0;

            FlowDocument doc = new FlowDocument() { PageWidth = 2000 };
            Paragraph para = new Paragraph();

            var matches = keywordRegex.Matches(text).Cast<Match>().ToList();
            foreach (var match in matches)
            {
                var beforeString = text.Substring(lastEnd, match.Index - lastEnd);
                para.Inlines.Add(new Run(beforeString));

                lastEnd = match.Index + match.Length;

                var keywordMatch = match;
                if (keywordMatch.Success)
                {
                    var keywordRun = new Run(keywordMatch.ToString());
                    para.Inlines.Add(keywordRun);

                    int keywordPos = keywordMatch.Index;

                    // Don't match within comments:
                    int commentPos = text.LastIndexOf("//", keywordPos);
                    int newLinePos = text.LastIndexOf("\n", keywordPos);
                    if (newLinePos < commentPos)
                        continue;

                    // Don't match within strings:
                    int quoteCount = 0;
                    int quotePos = text.IndexOf("\"", newLinePos + 1, keywordPos - newLinePos);
                    while (quotePos != -1)
                    {
                        quoteCount++;
                        quotePos = text.IndexOf("\"", quotePos + 1, keywordPos - (quotePos + 1));
                    }
                    if (quoteCount % 2 != 0)
                    {
                        continue;
                    }

                    keywordRun.Foreground = keywordBrush;
                }
                else
                {
                    doc.Blocks.Add(para);
                    para = new Paragraph();
                }
            }

            var afterString = text.Substring(lastEnd);
            para.Inlines.Add(new Run(afterString));


            doc.Blocks.Add(para);

            rtb.Document = doc;
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            RunCurrentSample();
        }

        private async void RunCurrentSample()
        {
            ClearOutputTextBox();
            ProgressBar.Value = 0;
            SampleHarness harness = currentSample.Harness;
            harness.ProgressBar = ProgressBar;
            harness.CancellationTokenSource = null;

            var task = currentSample.InvokeSafe();
            currentTask = task;

            CancelButton.IsEnabled = (harness.CancellationTokenSource != null);
            OutputTextBox.Text = Console.Text;

            if (task != null)
            {
                await task;
                if (currentTask != task) return;
                CancelButton.IsEnabled = false;
            }
        }

        private void ClearOutputTextBox()
        {
            OutputTextBox.Text = "";
            Console.Clear();
        }

        private void BeforeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            before = true;
            ChangeSample(currentSample);
        }

        private void AfterRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            before = false;
            ChangeSample(currentSample);
        }

        private void ChangeSample(Sample sample)
        {
            ClearOutputTextBox();

            if (currentSample != null)
            {
                currentSample = currentSample.Harness[sample.Key, before];

                addColorizedText(CodeRichTextBox, currentSample.Code, currentSample.Harness.Extension);

                RunButton.IsEnabled = true;
                DescriptionTextBox.Text = currentSample.Description;
            }
            else
            {
                CodeRichTextBox.Document.Blocks.Clear();

                RunButton.IsEnabled = false;
                CancelButton.IsEnabled = false;
                DescriptionTextBox.Text = "Select a sample from the tree to the left.";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            currentSample.Harness.CancellationTokenSource.Cancel();
        }
    }


    public static class TreeHelpers
    {
        public static TreeViewItem ContainerFromItem(this TreeView treeView, object item)
        {
            TreeViewItem containerThatMightContainItem = (TreeViewItem)treeView.ItemContainerGenerator.ContainerFromItem(item);
            if (containerThatMightContainItem != null)
                return containerThatMightContainItem;
            else
                return ContainerFromItem(treeView.ItemContainerGenerator, treeView.Items, item);
        }

        private static TreeViewItem ContainerFromItem(ItemContainerGenerator parentItemContainerGenerator, ItemCollection itemCollection, object item)
        {
            foreach (object curChildItem in itemCollection)
            {
                TreeViewItem parentContainer = (TreeViewItem)parentItemContainerGenerator.ContainerFromItem(curChildItem);
                if (parentContainer == null)
                    return null;
                TreeViewItem containerThatMightContainItem = (TreeViewItem)parentContainer.ItemContainerGenerator.ContainerFromItem(item);
                if (containerThatMightContainItem != null)
                    return containerThatMightContainItem;
                TreeViewItem recursionResult = ContainerFromItem(parentContainer.ItemContainerGenerator, parentContainer.Items, item);
                if (recursionResult != null)
                    return recursionResult;
            }
            return null;
        }
    }
}
