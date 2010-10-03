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

using System.IO;
using NUnit.Framework;

namespace MindTouch.Deki.Tests {

    [TestFixture]
    public class PlainTextResourceManagerTests {

        [Test]
        public void Can_provide_test_resource_set() {
            TestResourceSet resourceSet = new TestResourceSet();
            resourceSet.Add("foo.bar.baz","foo");
            PlainTextResourceManager resourceManager = new PlainTextResourceManager(resourceSet);
            Assert.AreEqual("foo", resourceManager.GetString("foo.bar.baz", null, "baz"));
            Assert.AreEqual("baz", resourceManager.GetString("foo.bar", null, "baz"));
        }

        [Test]
        public void Gets_default_on_missing_key() {
            string path = Path.GetTempPath();
            string resource = Path.Combine(path, "resources.custom.txt");
            using(StreamWriter sw = File.CreateText(resource)) {
                sw.WriteLine("[Test.Section]");
                sw.WriteLine("  foo=bar");
            }
            PlainTextResourceManager resourceManager = new PlainTextResourceManager(path);
            Assert.AreEqual("bar", resourceManager.GetString("Test.Section.foo", null, "baz"));
            Assert.AreEqual("baz", resourceManager.GetString("Test.Section.blah", null, "baz"));
        }
    }
}
