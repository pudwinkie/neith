﻿/*
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
using MindTouch.Deki.Script.Expr;
using MindTouch.Dream;
using MindTouch.Dream.Test;
using MindTouch.Dream.Test.Mock;
using NUnit.Framework;

namespace MindTouch.Deki.Script.Tests.ScriptTests {

    [TestFixture]
    public class DekiScriptLibraryWebTests {

        [SetUp]
        public void Setup() {
            MockPlug.DeregisterAll();
        }

        [Test]
        public void Can_call_WebHtml_on_uri() {
            var mock = MockPlug.Setup(new XUri("mock://mock"))
                .Verb("GET")
                .Returns(DreamMessage.Ok(MimeType.TEXT, "<strong>hi</strong>"))
                .ExpectCalls(Times.Once());
            DekiScriptTester.Default.Test(
                "web.html(\"mock://mock\")", 
                "<html><body><strong>hi</strong></body></html>",
                typeof(DekiScriptXml));
            mock.Verify();
        }

        [Test]
        public void Can_call_WebHtml_on_string() {
            DekiScriptTester.Default.Test(
                "web.html(\"<strong>hi</strong>\")",
                "<html><body><strong>hi</strong></body></html>",
                typeof(DekiScriptXml));
        }

        [Test]
        public void Can_call_WebHtml_on_WebHtml_output() {
            
            // Fixed Regression Bug: 8477
            DekiScriptTester.Default.Test(
                "web.html(web.html(\"<strong>hi</strong>\"))",
                "<html><body><strong>hi</strong></body></html>", typeof(DekiScriptXml));
        }
    }
}
