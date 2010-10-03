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
using MindTouch.Deki.Script.Expr;
using MindTouch.Deki.Script.Runtime.TargetInvocation;
using MindTouch.Dream.Test;
using NUnit.Framework;


namespace MindTouch.Deki.Script.Tests.ScriptTests {
    
    [TestFixture]
    public class DekiScriptLibraryListTests {

        //--- Fields ---
        private DekiScriptTester _t;

        [SetUp]
        public void Setup() {
            _t = new DekiScriptTester();
        }
        
        [Test]
        public void Can_call_ListApply() {
            _t.Test(
                "list.apply([1,2,3,4],\"$+$\")",
                "[ 2, 4, 6, 8 ]",
                typeof(DekiScriptList));
        }

        [Test]
        public void Can_call_ListApply_with_custom_function() {
            _t.Runtime.RegisterFunction("test.double", GetType().GetMethod("TestDouble"), new[] {
                new DekiScriptNativeInvocationTarget.Parameter("i",false),
            });
            _t.Test(
                "list.apply([1,2,3,4],\"test.double($)\")",
                "[ 2, 4, 6, 8 ]",
                typeof(DekiScriptList));
        }

        [Test]
        public void Can_call_ListSort() {
            _t.Test(
                "list.sort{ list: ['2009-12-01','2009-11-01', '2009-11-02', '2009-11-03'], compare: \"date.compare($left, $right)\" };",
                "[ \"2009-11-01\", \"2009-11-02\", \"2009-11-03\", \"2009-12-01\" ]",
                typeof(DekiScriptList));
        }

        [Test]
        public void Can_call_ListSort_with_custom_function() {
            _t.Runtime.RegisterFunction("test.compare", GetType().GetMethod("TestCompare"), new[] {
                new DekiScriptNativeInvocationTarget.Parameter("left",false),
                new DekiScriptNativeInvocationTarget.Parameter("right",false),
            });
           _t.Test(
                "list.sort{list: [1,2,3,4], compare: \"test.compare($left, $right)\"};",
                "[ 4, 3, 2, 1 ]",
                typeof(DekiScriptList));
            
        }
        
        public static int TestDouble(int i) {
            return i*2;
        }

        public static int TestCompare(int left, int right) {
            return right.CompareTo(left);
        }
    }
}
