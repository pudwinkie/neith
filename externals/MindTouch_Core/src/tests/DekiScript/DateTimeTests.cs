/*
 * MindTouch DekiScript - embeddable web-oriented scripting runtime
 * Copyright (c) 2006-2010 MindTouch Inc.
 * www.mindtouch.com  oss@mindtouch.com
 *
 * For community documentation and downloads visit www.opengarden.org;
 * please review the licensing section.
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 * http://www.gnu.org/copyleft/lesser.html
 */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MindTouch.Deki.Script.Runtime.Library;
using NUnit.Framework;

namespace MindTouch.Deki.Script.Tests {
    
    [TestFixture]
    public class DateTimeTests {

        [Test]
        public void CultureDateTime_with_utc_flagless_signature_assumes_utc_if_no_tz_specified() {
            double offset;
            var time = DekiScriptLibrary.CultureDateTimeParse("12:00pm", CultureInfo.InvariantCulture, out offset);
            Assert.AreEqual(DateTimeKind.Utc, time.Kind);
            Assert.AreEqual(12, time.Hour);
            Assert.AreEqual(0d, offset);
        }

        [Test]
        public void CultureDateTime_with_utc_flagless_signature_adjust_time_to_utc() {
            double offset;
            var time = DekiScriptLibrary.CultureDateTimeParse("12:00pm -03:00", CultureInfo.InvariantCulture, out offset);
            Assert.AreEqual(DateTimeKind.Utc, time.Kind);
            Assert.AreEqual(15, time.Hour);
            Assert.AreEqual(-3d, offset);
        }

        [Test]
        public void CultureDateTime_does_not_adjust_kind_if_no_tz_specified_and_utc_flag_is_false() {
            double offset;
            var time = DekiScriptLibrary.CultureDateTimeParse("12:00pm", CultureInfo.InvariantCulture, false, out offset);
            Assert.AreEqual(DateTimeKind.Unspecified, time.Kind);
            Assert.AreEqual(12, time.Hour);
            Assert.AreEqual(0, offset);
        }

        [Test]
        public void CultureDateTime_adjusts_kind_and_time_to_utc_if_tz_specified_and_utc_flag_is_false() {
            double offset;
            var time = DekiScriptLibrary.CultureDateTimeParse("12:00pm -03:00", CultureInfo.InvariantCulture, false, out offset);
            Assert.AreEqual(DateTimeKind.Utc, time.Kind);
            Assert.AreEqual(15, time.Hour);
            Assert.AreEqual(-3d, offset);
        }

        [Test]
        public void DateParse_with_format_assumes_utc_if_no_default_tz_is_given() {
            Assert.AreEqual(
                "Sun, 10 Oct 2010 12:00:00 GMT",
                DekiScriptLibrary.DateParse("2010/10/10 12:00:00", "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture.IetfLanguageTag,  null));
        }

        [Test]
        public void DateParse_with_format_uses_passed_tz_if_no_default_tz_is_given_and_adjusts_to_utc() {
            Assert.AreEqual(
                "Sun, 10 Oct 2010 15:00:00 GMT",
                DekiScriptLibrary.DateParse("2010/10/10 12:00:00", "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture.IetfLanguageTag, "-3"));
        }

        [Test]
        public void DateParse_without_format_assumes_utc_if_no_default_tz_is_given() {
            Assert.AreEqual(
                "Sun, 10 Oct 2010 12:00:00 GMT",
                DekiScriptLibrary.DateParse("2010/10/10 12:00:00", null, CultureInfo.InvariantCulture.IetfLanguageTag, null));
        }

        [Test]
        public void DateParse_without_format_uses_passed_tz_if_no_default_tz_is_given_and_adjusts_to_utc() {
            Assert.AreEqual(
                "Sun, 10 Oct 2010 15:00:00 GMT",
                DekiScriptLibrary.DateParse("2010/10/10 12:00:00", null, CultureInfo.InvariantCulture.IetfLanguageTag, "-3"));
        }

        [Test]
        public void DateTimeZone_returns_GMT_if_no_tz_in_date() {
            Assert.AreEqual("GMT",DekiScriptLibrary.DateTimeZone("12:00pm",null));
        }

        [Test]
        public void DateTimeZone_returns_provided_tz_if_no_tz_in_date() {
            Assert.AreEqual("-03:00", DekiScriptLibrary.DateTimeZone("12:00pm", "-3"));
        }

        [Test]
        public void DateTimeZone_returns_tz_from_date() {
            Assert.AreEqual("-03:00", DekiScriptLibrary.DateTimeZone("Sun, 10 Oct 2010 12:00:00 -03:00", null));
        }

        [Test]
        public void DateTimeZone_returns_tz_from_date_ignoring_provided_tz_default() {
            Assert.AreEqual("-05:00", DekiScriptLibrary.DateTimeZone("Sun, 10 Oct 2010 12:00:00 -05:00", "-3"));
        }
    }
}
