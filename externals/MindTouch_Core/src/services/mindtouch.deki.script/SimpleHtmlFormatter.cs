/*
 * MindTouch DekiScript - embeddable web-oriented scripting runtime
 * Copyright (c) 2006-2010 MindTouch Inc.
 * www.mindtouch.com  oss@mindtouch.com
 *
 * For community documentation and downloads visit wiki.developer.mindtouch.com;
 * please review the licensing section.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using MindTouch.Dream;
using MindTouch.Xml;

namespace MindTouch.Deki.Script {
    public class SimpleHtmlFormatter {

        private static Regex _prep = new Regex("\\r", RegexOptions.Compiled);
        private static Regex _whitespace = new Regex("[\t ][\t ]", RegexOptions.Compiled);
        private static Regex _paragraphBreaks = new Regex("\n\n+", RegexOptions.Compiled);
        private static Dictionary<string, string> entityMap = new Dictionary<string, string>();

        public static XDoc Format(string text) {
            return new SimpleHtmlFormatter(text).ToDocument();
        }

        static SimpleHtmlFormatter() {
            entityMap.Add("&", "&amp;");
            entityMap.Add("\"", "&quot;");
            //entityMap.Add("'", "&apos;");     // xdoc decodes this again anyway
            entityMap.Add("<", "&lt;");
            entityMap.Add(">", "&gt;");
        }

        private string _text;

        private SimpleHtmlFormatter(string text) {
            _text = _prep.Replace(text ?? "", "");
            EncodeEntities();
            while(_whitespace.IsMatch(_text)) {
                _text = _whitespace.Replace(_text, "&#160; ");
            }
            FormatBlocks();
        }

        private void FormatBlocks() {
            string[] paragraphs = _paragraphBreaks.Split(_text);
            StringBuilder builder = new StringBuilder();
            foreach(string p in paragraphs) {
                string paragraphh = p.Replace("\n", "<br />\n");
                builder.AppendLine("<p>" + paragraphh + "</p>");
            }
            _text = builder.ToString();
        }

        private void EncodeEntities() {
            StringBuilder builder = new StringBuilder(_text);
            foreach(KeyValuePair<string, string> kvp in entityMap) {
                builder.Replace(kvp.Key, kvp.Value);
            }
            _text = builder.ToString();
        }

        private XDoc ToDocument() {
            return XDocFactory.From("<html><body>" + _text + "</body></html>", MimeType.XHTML);
        }
    }
}
