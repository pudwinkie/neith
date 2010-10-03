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
using System.IO;
using System.Linq;
using System.Threading;
using MindTouch.Dream;
using MindTouch.Dream.Test;
using MindTouch.Tasking;
using MindTouch.Xml;

using NUnit.Framework;

namespace MindTouch.Deki.Tests {

    [TestFixture]
    public class DekiInstanceTests {

        [Ignore("Stress test")]
        [Test]
        public void Multiple_simultaneous_requests() {
            var signal = new ManualResetEvent(false);
            var apihits = new List<Result<Result<DreamMessage>>>();
            var n = 50;
            var core = CreateCoreService();
            for(var i = 0; i < n; i++) {
                apihits.Add(Async.ForkThread(() => {
                    signal.WaitOne();
                    return core.AtLocalHost.At("pages", "1").GetAsync();
                },
                new Result<Result<DreamMessage>>()));
            }
            signal.Set();
            foreach(Result<Result<DreamMessage>> r in apihits) {
                r.Wait();
                r.Value.Wait();
                Assert.IsTrue(r.Value.Value.IsSuccessful, r.Value.Value.ToString());
            }
            var services = core.AtLocalHost.At("site", "services").With("limit","all").With("apikey", Utils.Settings.ApiKey).GetAsync().Wait();
            Assert.IsTrue(services.IsSuccessful, services.ToText());
            foreach(var service in services.ToDocument()["service[status='disabled']"]) {
                var error = service["lasterror"].Contents;
                if(string.IsNullOrEmpty(error)) {
                    continue;
                }
                Console.WriteLine(service.ToPrettyString());
            }
        }

        [Ignore("Stress test")]
        [Test]
        public void Start_Stop_services_concurrently() {
            var core = CreateCoreService();
            int retry = 10;
            while(true) {
                var status = core.AtLocalHost.At("site", "status").With("apikey", Utils.Settings.ApiKey).GetAsync().Wait();
                if(status.IsSuccessful && status.ToDocument()["state"].Contents == "RUNNING") {
                    break;
                }
                retry--;
                if(retry < 0) {
                    throw new Exception("host never stared up");
                }
                Thread.Sleep(1000);
            }
            var services = core.AtLocalHost.At("site", "services").With("limit", "all").With("apikey", Utils.Settings.ApiKey).GetAsync().Wait();
            Assert.IsTrue(services.IsSuccessful, services.ToText());
            var servicesToRestart = from service in services.ToDocument()["service[status='enabled']"]
                                    let id = service["@id"].AsInt ?? 0
                                    where !string.IsNullOrEmpty(service["uri"].Contents) && id != 1
                                    select service;
            var signal = new ManualResetEvent(false);
            var apihits = new List<Result<DreamMessage>>();
            foreach(var service in servicesToRestart) {
                var localId = service["@id"].AsInt ?? 0;
                apihits.Add(Async.ForkThread(() => {
                    signal.WaitOne();
                    DreamMessage response = null;
                    for(var k = 0; k < 10; k++) {
                        response = core.AtLocalHost.At("site", "services", localId.ToString(), "stop").With("apikey", Utils.Settings.ApiKey).PostAsync().Wait();
                        if(response.IsSuccessful) {
                            response = core.AtLocalHost.At("site", "services", localId.ToString(), "start").With("apikey", Utils.Settings.ApiKey).PostAsync().Wait();
                        }
                    }
                    return response;
                },
                new Result<DreamMessage>()));
            }
            signal.Set();
            foreach(var r in apihits) {
                r.Wait();
                Assert.IsTrue(r.Value.IsSuccessful, r.Value.ToString());
            }
        }

        [Ignore("Stress test")]
        [Test]
        public void Multi_instance_service_start_stop() {
            var core1 = CreateCoreService();
            var core2 = CreateCoreService();
            int retry = 10;
            while(true) {
                var status1 = core1.AtLocalHost.At("site", "status").With("apikey", Utils.Settings.ApiKey).GetAsync().Wait();
                var status2 = core2.AtLocalHost.At("site", "status").With("apikey", Utils.Settings.ApiKey).GetAsync().Wait();
                if(status1.IsSuccessful && status1.ToDocument()["state"].Contents == "RUNNING" &&
                    status2.IsSuccessful && status2.ToDocument()["state"].Contents == "RUNNING") {
                    break;
                }
                retry--;
                if(retry < 0) {
                    throw new Exception("hosts never stared up");
                }
                Thread.Sleep(1000);
            }
            var services = core1.AtLocalHost.At("site", "services").With("limit", "all").With("apikey", Utils.Settings.ApiKey).GetAsync().Wait();
            Assert.IsTrue(services.IsSuccessful, services.ToText());
            var serviceToRestart = (from service in services.ToDocument()["service[status='enabled']"]
                                    let id = service["@id"].AsInt ?? 0
                                    where !string.IsNullOrEmpty(service["uri"].Contents) && id != 1
                                    select service).FirstOrDefault();
            var signal = new ManualResetEvent(false);
            var apihits = new List<Result<DreamMessage>>();
            foreach(var core in new[] { core1, core2 }) {
                var localCore = core;
                apihits.Add(Async.ForkThread(() => {
                    var id = serviceToRestart["@id"].AsInt ?? 0;
                    signal.WaitOne();
                    DreamMessage response = null;
                    for(var k = 0; k < 20; k++) {
                        DreamMessage stopResponse = localCore.AtLocalHost.At("site", "services", id.ToString(), "stop").With("apikey", Utils.Settings.ApiKey).PostAsync().Wait();
                        if(stopResponse.IsSuccessful) {
                            int retryStart = 3;
                            while(retryStart > 0) {
                                response = localCore.AtLocalHost.At("site", "services", id.ToString(), "start").With("apikey", Utils.Settings.ApiKey).PostAsync().Wait();
                                if(response.IsSuccessful) {
                                    break;
                                }
                                retryStart--;
                            }
                        }
                    }
                    return response;
                },
                new Result<DreamMessage>()));
            }
            signal.Set();
            foreach(var r in apihits) {
                r.Wait();
                Assert.IsTrue(r.Value.IsSuccessful, r.Value.ToString());
            }
        }

        private DreamServiceInfo CreateCoreService() {
            var lucenePath = Path.Combine(Path.Combine(Path.GetTempPath(), StringUtil.CreateAlphaNumericKey(4)), "luceneindex");
            Directory.CreateDirectory(lucenePath);
            var dekiConfig = new XDoc("config")
                .Elem("apikey", Utils.Settings.ApiKey)
                .Elem("path", "deki")
                .Elem("sid", "http://services.mindtouch.com/deki/draft/2006/11/dekiwiki")
                .Elem("deki-path", Utils.Settings.DekiPath)
                .Elem("imagemagick-convert-path", Utils.Settings.ImageMagickConvertPath)
                .Elem("imagemagick-identify-path", Utils.Settings.ImageMagickIdentifyPath)
                .Elem("princexml-path", Utils.Settings.PrinceXmlPath)
                .Elem("deki-resources-path", Utils.Settings.DekiResourcesPath)
                .Start("page-subscription")
                    .Elem("accumulation-time", "0")
                .End()
                .Start("wikis")
                    .Start("config")
                        .Attr("id", "default")
                        .Elem("host", "*")
                        .Start("page-subscription")
                            .Elem("from-address", "foo@bar.com")
                        .End()
                        .Elem("db-server", Utils.Settings.DbServer)
                        .Elem("db-port", "3306")
                        .Elem("db-catalog", "wikidb")
                        .Elem("db-user", "wikiuser")
                        .Start("db-password").Attr("hidden", "true").Value(Utils.Settings.DbPassword).End()
                        .Elem("db-options", "pooling=true; Connection Timeout=5; Protocol=socket; Min Pool Size=2; Max Pool Size=50; Connection Reset=false;character set=utf8;ProcedureCacheSize=25;Use Procedure Bodies=true;")
                    .End()
                .End()
                .Start("indexer")
                    .Elem("path.store", lucenePath)
                    .Elem("namespace-whitelist", "main, main_talk, user, user_talk")
                .End();
            var hostInfo = DreamTestHelper.CreateRandomPortHost(new XDoc("config").Elem("apikey", Utils.Settings.ApiKey));
            hostInfo.Host.Self.At("load").With("name", "mindtouch.deki").Post(DreamMessage.Ok());
            hostInfo.Host.Self.At("load").With("name", "mindtouch.deki.services").Post(DreamMessage.Ok());
            hostInfo.Host.Self.At("load").With("name", "mindtouch.indexservice").Post(DreamMessage.Ok());
            return DreamTestHelper.CreateService(hostInfo, dekiConfig);
        }
    }

}
