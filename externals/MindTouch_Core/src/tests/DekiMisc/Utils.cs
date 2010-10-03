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

using System;
using System.Collections.Generic;
using System.Text;

using MindTouch.Dream;
using MindTouch.Dream.Test;
using MindTouch.Tasking;
using MindTouch.Xml;

using NUnit.Framework;

namespace MindTouch.Deki.Tests {
    public static class Utils {

        public static string ToErrorString(this DreamMessage response) {
            if(response == null || response.IsSuccessful) {
                return null;
            }
            var responseText = response.HasDocument ? response.ToDocument().ToPrettyString() : response.ToString();
            return string.Format("Status: {0}\r\nMessage:\r\n{1}", response.Status, responseText);
        }

        public class TestSettings {

            public static readonly TestSettings Instance = new TestSettings();
            private readonly object padlock = new object();
            private XDoc _dekiConfig;
            private DreamHostInfo _hostInfo;
            private readonly XDoc _xdoc = null;
            public readonly XUri LuceneMockUri = new XUri("mock://mock/testlucene");

            private TestSettings() {
                MockPlug.DeregisterAll();
                string configfile = "mindtouch.deki.tests.xml";
                if(System.IO.File.Exists(configfile)) {
                    _xdoc = XDocFactory.LoadFrom(configfile, MimeType.XML);
                } else {
                    _xdoc = new XDoc("config");
                }
            }

            public Plug Server {
                get {
                    return HostInfo.LocalHost.At("deki");
                }
            }

            public DreamHostInfo HostInfo {
                get {
                    if(_hostInfo == null) {
                        lock(padlock) {
                            if(_hostInfo == null) {
                                _hostInfo = DreamTestHelper.CreateRandomPortHost(new XDoc("config").Elem("apikey", Settings.ApiKey).Elem("storage-dir", Settings.StorageDir));
                                _hostInfo.Host.Self.At("load").With("name", "mindtouch.deki").Post(DreamMessage.Ok());
                                _hostInfo.Host.Self.At("load").With("name", "mindtouch.deki.services").Post(DreamMessage.Ok());
                                _hostInfo.Host.Self.At("load").With("name", "mindtouch.indexservice").Post(DreamMessage.Ok());
                                DreamServiceInfo deki = DreamTestHelper.CreateService(_hostInfo, DekiConfig);
                            }
                        }
                    }
                    return _hostInfo;
                }
            }

            public void ShutdownHost() {
                lock(padlock) {
                    if(_hostInfo != null) {
                        _hostInfo.Dispose();
                        _hostInfo = null;
                    }
                }
            }

            public string ApiKey { get { return GetString("/config/apikey", "123"); } }
            public string ProductKey { get { return GetString("/config/productkey", "badkey"); } }
            public string AssetsPath { get { return GetString("/config/assets-path", @"C:\mindtouch\assets"); } }
            public string DekiPath { get { return GetString("/config/deki-path", @"C:\mindtouch\public\dekiwiki\trunk\web"); } }
            public string DekiResourcesPath { get { return GetString("/config/deki-resources-path", @"C:\mindtouch\public\dekiwiki\trunk\web\resources"); } }
            public string ImageMagickConvertPath { get { return GetString("/config/imagemagick-convert-path", @"C:\mindtouch\public\dekiwiki\trunk\src\tools\mindtouch.dekihost.setup\convert.exe"); } }
            public string ImageMagickIdentifyPath { get { return GetString("/config/imagemagick-identify-path", @"C:\mindtouch\public\dekiwiki\trunk\src\tools\mindtouch.dekihost.setup\identify.exe"); } }
            public string PrinceXmlPath { get { return GetString("/config/princexml-path", @"C:\Program Files\Prince\Engine\bin\prince.exe"); } }
            public string HostAddress { get { return GetString("/config/host-address", "testdb.mindtouch.com"); } }
            public string DbServer { get { return GetString("/config/db-server", "testdb.mindtouch.com"); } }
            public string DbCatalog { get { return GetString("/config/db-catalog", "wikidb"); } }
            public string DbUser { get { return GetString("/config/db-user", "wikiuser"); } }
            public string DbPassword { get { return GetString("/config/db-password", "password"); } }
            public string UserName { get { return GetString("/config/UserName", "Admin"); } }
            public string Password { get { return GetString("/config/Password", "password"); } }
            public string StorageDir { get { return GetString("/config/storage-dir", null); } }
            public int CountOfRepeats { get { return GetInt("/config/CountOfRepeats", 5); } }
            public int SizeOfBigContent { get { return GetInt("/config/SizeOfBigContent", 4096); } }
            public int SizeOfSmallContent { get { return GetInt("/config/SizeOfSmallContent", 256); } }

            public XDoc DekiConfig {
                get {
                    if(_dekiConfig == null) {
                        lock(padlock) {
                            _dekiConfig = new XDoc("config")
                            .Elem("apikey", ApiKey)
                            .Elem("path", "deki")
                            .Elem("sid", "http://services.mindtouch.com/deki/draft/2006/11/dekiwiki")
                            .Elem("deki-path", DekiPath)
                            .Elem("deki-resources-path", DekiResourcesPath)
                            .Elem("imagemagick-convert-path", ImageMagickConvertPath)
                            .Elem("imagemagick-identify-path", ImageMagickIdentifyPath)
                            .Elem("princexml-path", PrinceXmlPath)
                            .Start("page-subscription")
                                .Elem("accumulation-time", "0")
                            .End()
                            .Start("packageupdater")
                                .Attr("uri", "mock://mock/packageupdater")
                            .End()
                            .Start("wikis")
                                .Start("config")
                                    .Attr("id", "default")
                                    .Elem("host", "*")
                                    .Start("page-subscription")
                                        .Elem("from-address", "foo@bar.com")
                                    .End()
                                    .Elem("db-server", DbServer)
                                    .Elem("db-port", "3306")
                                    .Elem("db-catalog", DbCatalog)
                                    .Elem("db-user", DbUser)
                                    .Start("db-password").Attr("hidden", "true").Value(DbPassword).End()
                                    .Elem("db-options", "pooling=true; Connection Timeout=5; Protocol=socket; Min Pool Size=2; Max Pool Size=50; Connection Reset=false;character set=utf8;ProcedureCacheSize=25;Use Procedure Bodies=true;")
                                .End()
                            .End()
                            .Start("indexer").Attr("src", LuceneMockUri).End();
                        }
                    }
                    return _dekiConfig;
                }
            }

            private int GetInt(string path, int defaultValue) {
                return _xdoc[path].AsInt != null ? _xdoc[path].AsInt.Value : defaultValue;
            }

            private string GetString(string path, string defaultValue) {
                return _xdoc[path].AsText ?? defaultValue;
            }
        }

        public static readonly TestSettings Settings = TestSettings.Instance;

        public static Plug BuildPlugForAnonymous() {
            return BuildPlugForUser(null, null);
        }

        public static Plug BuildPlugForAdmin() {
            return BuildPlugForUser(Settings.UserName, Settings.Password);
        }

        public static Plug BuildPlugForUser(string username) {
            return BuildPlugForUser(username, UserUtils.DefaultUserPsw);
        }

        public static Plug BuildPlugForUser(string username, string password) {
            Plug.GlobalCookies.Clear();
            Plug p = Settings.Server;

            if(!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password)) {
                DreamMessage msg = p.WithCredentials(username, password).At("users", "authenticate").PostAsync().Wait();
                Assert.AreEqual(DreamStatus.Ok, msg.Status, "Failed to authenticate.");
            }
            return p;
        }

        public static string GenerateUniqueName() {
            return Guid.NewGuid().ToString().Replace("-", string.Empty);
        }

        public static string GenerateUniqueName(string prefix) {
            return prefix + GenerateUniqueName();
        }

        public static bool ByteArraysAreEqual(byte[] one, byte[] two) {
            if(one == null || two == null)
                throw new ArgumentException();

            if(one.Length != two.Length)
                return false;
            for(int i = 0; i < one.Length; i++)
                if(one[i] != two[i])
                    return false;

            return true;
        }

        private static char[] _alphabet = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'l', 'm', 'n', 'o', 'p', 'r', 's', 't', (char)224, (char)225, (char)226, (char)227, (char)228, (char)229, (char)230, (char)231, (char)232, (char)233, (char)234, (char)235, (char)236, (char)237, (char)238, (char)239, (char)240, (char)241, (char)242 };

        public static string GetRandomTextByAlphabet(int countOfSymbols) {
            System.Text.StringBuilder builder = new StringBuilder(countOfSymbols);
            Random rnd = new Random();
            for(int i = 0; i < countOfSymbols; i++)
                builder.Append(_alphabet[rnd.Next(_alphabet.Length - 1)]);

            return builder.ToString();
        }

        public static string GetRandomText(int countOfSymbols) {
            System.Text.StringBuilder builder = new StringBuilder(countOfSymbols);
            Random rnd = new Random();
            for(int i = 0; i < countOfSymbols; i++) {
                try {
                    int symbolAsInt = rnd.Next(0x10ffff);
                    while(0xD800 <= symbolAsInt && symbolAsInt <= 0xDFFF)
                        symbolAsInt = rnd.Next(0x10ffff);
                    char symbol = char.ConvertFromUtf32(symbolAsInt)[0];
                    if(char.IsDigit(symbol) || char.IsLetter(symbol) || char.IsPunctuation(symbol))
                        builder.Append(symbol);
                    else
                        i--;
                } catch(System.ArgumentOutOfRangeException) {
                    i--;
                }
            }

            return builder.ToString();
        }

        public static string GetBigRandomText() {
            return GetRandomTextByAlphabet(Utils.Settings.SizeOfBigContent);
        }

        public static string GetSmallRandomText() {
            return GetRandomTextByAlphabet(Utils.Settings.SizeOfSmallContent);
        }

        public static string DateToString(DateTime time) {
            return time == DateTime.MinValue ? null : time.ToUniversalTime().ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
        }

        public static Dictionary<string, string> GetDictionaryFromDoc(XDoc doc, string element, string key, string value) {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach(XDoc node in doc[element])
                result[node[key].AsText] = node[value].AsText;

            return result;
        }

        public static void TestSortingOfDocByField(XDoc doc, string resourceName, string element, bool ascorder) {
            TestSortingOfDocByField(doc, resourceName, element, ascorder, null);
        }

        public static void TestSortingOfDocByField(XDoc doc, string resourceName, string element, bool ascorder, Dictionary<string, string> dictData) {
            string previousValue = string.Empty;
            foreach(XDoc node in doc[resourceName]) {
                string currentValue = dictData == null ? node[element].AsText : dictData[node[element].AsText];
                if(!string.IsNullOrEmpty(previousValue) && !string.IsNullOrEmpty(currentValue)) {
                    int x = StringUtil.CompareInvariantIgnoreCase(previousValue, currentValue) * (ascorder ? 1 : -1);
                    Assert.IsTrue(x <= 0, string.Format("Sort assertion failed for '{0}': '{1}' and '{2}'", element, previousValue, currentValue));
                }
                previousValue = currentValue;
                currentValue = string.Empty;
            }
        }

        public static void PingServer() {
            var host = Settings.HostInfo;
        }
    }


    // Note (arnec): this has to exist for nunit-console to pick up the log4net configuration
    [SetUpFixture]
    public class LogSetup {
        [SetUp]
        public void Setup() {
            log4net.Config.XmlConfigurator.Configure();
        }
    }

}
