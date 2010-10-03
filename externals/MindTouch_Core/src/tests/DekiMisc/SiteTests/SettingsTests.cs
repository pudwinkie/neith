/*
 * MindTouch Core - open source enterprise collaborative networking
 * Copyright (c) 2006-2010 MindTouch Inc.
 * www.mindtouch.com  oss@mindtouch.com
 *
 * For community documentation and downloads visit www.opengarden.org;
 * please review the licensing section.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with this program; if not, write to the Free Software Foundation, Inc.,
 * 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 * http://www.gnu.org/copyleft/gpl.html
 */

using System.Text;
using System.Collections.Generic;
using NUnit.Framework;
using MindTouch.Dream;
using MindTouch.Xml;


namespace MindTouch.Deki.Tests.SiteTests
{
    [TestFixture]
    public class SettingsTests
    {
        [Test]
        public void GetSettings()
        {
            // GET:site/settings
            // http://developer.mindtouch.com/Deki/API_Reference/GET%3asite%2f%2fsettings

            Plug p = Utils.BuildPlugForAdmin();

            DreamMessage msg = p.At("site", "settings").Get();
            Assert.AreEqual(DreamStatus.Ok, msg.Status);
        }

        [Test]
        public void PutSettings()
        {
            // PUT:site/settings
            // http://developer.mindtouch.com/Deki/API_Reference/PUT%3asite%2f%2fsettings

            Plug p = Utils.BuildPlugForAdmin();

            DreamMessage msg = p.At("site", "settings").Get();
            Assert.AreEqual(DreamStatus.Ok, msg.Status);

            XDoc settingsDoc = msg.ToDocument();
            Assert.IsFalse(settingsDoc.IsEmpty);
            XDoc safeHtml = settingsDoc["editor/safe-html"];
            safeHtml.ReplaceValue(false);

            msg = p.At("site", "settings").Put(settingsDoc);
            Assert.AreEqual(DreamStatus.Ok, msg.Status);

            msg = p.At("site", "settings").Get();
            Assert.AreEqual(DreamStatus.Ok, msg.Status);
            Assert.IsFalse(msg.ToDocument()["editor/safe-html"].AsBool.Value);

            settingsDoc = msg.ToDocument();
            safeHtml = settingsDoc["editor/safe-html"];
            safeHtml.ReplaceValue(true);

            msg = p.At("site", "settings").Put(settingsDoc);
            Assert.AreEqual(DreamStatus.Ok, msg.Status);

            msg = p.At("site", "settings").Get();
            Assert.AreEqual(DreamStatus.Ok, msg.Status);
            Assert.IsTrue(msg.ToDocument()["editor/safe-html"].AsBool.Value);
        }

        private string GenerateBigRandomCss()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < Utils.Settings.SizeOfSmallContent; i++)
            {
                builder.AppendFormat(".note{0}", Utils.GenerateUniqueName());
                builder.AppendLine("{ color: red; background: yellow; font-weight: bold; }");
            }

            return builder.ToString();
        }

        [Test]
        public void PutSettingsManyTimes()
        {
            Plug p = Utils.BuildPlugForAdmin();

            XDoc settingsDoc = null;

            DreamMessage msg = p.At("site", "settings").GetAsync().Wait();
            Assert.AreEqual(DreamStatus.Ok, msg.Status);
            XDoc originalSettings = msg.ToDocument();
            Assert.IsFalse(originalSettings.IsEmpty);

            string bodycss1 = GenerateBigRandomCss();
            string bodycss2 = GenerateBigRandomCss();
            string css1 = GenerateBigRandomCss();
            string css2 = GenerateBigRandomCss();
            string html01 = Utils.GetRandomTextByAlphabet(Utils.Settings.SizeOfBigContent);
            string html02 = Utils.GetRandomTextByAlphabet(Utils.Settings.SizeOfBigContent);
            string html11 = Utils.GetRandomTextByAlphabet(Utils.Settings.SizeOfBigContent);
            string html12 = Utils.GetRandomTextByAlphabet(Utils.Settings.SizeOfBigContent);

            try
            {
                for (int i = 0; i < Utils.Settings.CountOfRepeats; i++)
                {
                    settingsDoc = new XDoc("config")
                        .Start("cache")
                            .Elem("pages", true)
                            .Elem("permissions", true)
                            .Elem("roles", true)
                            .Elem("services", true)
                            .Elem("users", true)
                        .End()
                        .Start("editor")
                            .Elem("safe-html", true)
                        .End()
                        .Start("storage")
                            .Start("fs")
                                .Elem("path", "/var/www/deki-hayes-trunk/attachments")
                            .End()
                            .Elem("type", "fs")
                        .End()
                        .Start("files")
                            .Elem("blocked-extensions", "html, htm, exe, vbs, scr, reg, bat, comhtml, htm, exe, vbs, scr, reg, bat, com, cmd")
                        .End()
                        .Start("ui")
                            .Start("custom")
                                .Elem("bodycss", bodycss1)
                                .Elem("css", css1)
                                .Start("html")
                                    .Start("ace")
                                        .Start("neutral")
                                            .Elem("html0", html01)
                                            .Elem("html1", html11)
                                        .End()
                                    .End()
                                .End()
                            .End()
                            .Elem("language", "en-us")
                            .Elem("sitename", "Deki Wiki")
                        .End()
                        .Start("security")
                            .Elem("new-account-role", "Contributor")
                        .End();

                    msg = p.At("site", "settings").Put(settingsDoc);
                    Assert.AreEqual(DreamStatus.Ok, msg.Status);

                    msg = p.At("site", "settings").Get();
                    Assert.AreEqual(DreamStatus.Ok, msg.Status);

                    XDoc msgDoc = msg.ToDocument();

                    Assert.AreEqual(msgDoc["cache/pages"].AsBool, true);
                    Assert.AreEqual(msgDoc["cache/permissions"].AsBool, true);
                    Assert.AreEqual(msgDoc["cache/roles"].AsBool, true);
                    Assert.AreEqual(msgDoc["cache/services"].AsBool, true);
                    Assert.AreEqual(msgDoc["cache/users"].AsBool, true);
                    Assert.AreEqual(msgDoc["editor/safe-html"].AsBool, true);
                    Assert.AreEqual(msgDoc["ui/custom/bodycss"].AsText, bodycss1);
                    Assert.AreEqual(msgDoc["ui/custom/css"].AsText, css1);
                    Assert.AreEqual(msgDoc["ui/custom/html/ace/neutral/html0"].AsText, html01);
                    Assert.AreEqual(msgDoc["ui/custom/html/ace/neutral/html1"].AsText, html11);

                    settingsDoc = new XDoc("config")
                        .Start("cache")
                            .Elem("pages", false)
                            .Elem("permissions", false)
                            .Elem("roles", false)
                            .Elem("services", false)
                            .Elem("users", false)
                        .End()
                        .Start("editor")
                            .Elem("safe-html", false)
                        .End()
                        .Start("storage")
                            .Start("fs")
                                .Elem("path", "/var/www/deki-hayes-trunk/attachments")
                            .End()
                            .Elem("type", "fs")
                        .End()
                        .Start("files")
                            .Elem("blocked-extensions", "html, htm, exe, vbs, scr, reg, bat, comhtml, htm, exe, vbs, scr, reg, bat, com, cmd")
                        .End()
                        .Start("ui")
                            .Start("custom")
                                .Elem("bodycss", bodycss2)
                                .Elem("css", css2)
                                .Start("html")
                                    .Start("ace")
                                        .Start("neutral")
                                            .Elem("html0", html02)
                                            .Elem("html1", html12)
                                        .End()
                                    .End()
                                .End()
                            .End()
                            .Elem("language", "en-us")
                            .Elem("sitename", "Deki Wiki")
                        .End()
                        .Start("security")
                            .Elem("new-account-role", "Contributor")
                        .End();

                    msg = p.At("site", "settings").Put(settingsDoc);
                    Assert.AreEqual(DreamStatus.Ok, msg.Status);

                    msg = p.At("site", "settings").Get();
                    Assert.AreEqual(DreamStatus.Ok, msg.Status);

                    msgDoc = msg.ToDocument();

                    Assert.AreEqual(msgDoc["cache/pages"].AsBool, false);
                    Assert.AreEqual(msgDoc["cache/permissions"].AsBool, false);
                    Assert.AreEqual(msgDoc["cache/roles"].AsBool, false);
                    Assert.AreEqual(msgDoc["cache/services"].AsBool, false);
                    Assert.AreEqual(msgDoc["cache/users"].AsBool, false);
                    Assert.AreEqual(msgDoc["editor/safe-html"].AsBool, false);
                    Assert.AreEqual(msgDoc["ui/custom/bodycss"].AsText, bodycss2);
                    Assert.AreEqual(msgDoc["ui/custom/css"].AsText, css2);
                    Assert.AreEqual(msgDoc["ui/custom/html/ace/neutral/html0"].AsText, html02);
                    Assert.AreEqual(msgDoc["ui/custom/html/ace/neutral/html1"].AsText, html12);
                }
            }
            //catch (DreamResponseException ex)
            //{
            //    throw;
            //}
            finally
            {
                msg = p.At("site", "settings").PutAsync(originalSettings).Wait();
                Assert.IsTrue(msg.Status == DreamStatus.Ok, "Put original settings is failed: {0}", msg.ToString());
            }
        }

        [Test]
        public void StripLeafNodes() {
            XDoc doc = null;
            doc = new XDoc("root").Start("outer").Attr("someattr", "myval").Start("inner").End().End();
            MindTouch.Deki.Utils.RemoveEmptyNodes(doc);
            XDocAssertEqual(
                new XDoc("root").Start("outer").Attr("someattr", "myval").End(),
                doc);
            doc = new XDoc("root").Start("outer").Start("inner").End().End();
            MindTouch.Deki.Utils.RemoveEmptyNodes(doc);
            XDocAssertEqual(new XDoc("root"), doc);

            doc = new XDoc("root").Start("outer").Start("inner").Value("zzz").End().End();
            MindTouch.Deki.Utils.RemoveEmptyNodes(doc);
            XDocAssertEqual(new XDoc("root").Start("outer").Start("inner").Value("zzz").End().End(), doc);
        }

        private void XDocAssertEqual(XDoc expected, XDoc actual) {
            Assert.IsNotNull(expected);
            Assert.IsNotNull(actual);
            KeyValuePair<string, string>[] e = expected.ToKeyValuePairs();
            KeyValuePair<string, string>[] a = actual.ToKeyValuePairs();

            Assert.AreEqual(e.Length, a.Length, "Number of items in XDoc mismatched");
            for(int i = 0; i < e.Length; i++) {
                Assert.AreEqual(e[i].Key, a[i].Key);
                Assert.AreEqual(e[i].Value, a[i].Value);
            } 
        }
    }
}
