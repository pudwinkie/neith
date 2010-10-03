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
using System.Threading;

using MindTouch.Deki.UserSubscription;
using MindTouch.Dream;
using MindTouch.Dream.Test;
using MindTouch.Extensions.Time;
using MindTouch.Tasking;
using MindTouch.Xml;

using NUnit.Framework;

namespace MindTouch.Deki.Tests.ChangeSubscriptionTests {

    [TestFixture]
    public class DekiChangeSubscriptionTests {

        private static readonly log4net.ILog _log = LogUtils.CreateLog();

        private DreamHostInfo _hostInfo;

        [SetUp]
        public void PerTestSetup() {
            _hostInfo = DreamTestHelper.CreateRandomPortHost();
        }

        [TearDown]
        public void PerTestCleanup() {
            MockPlug.DeregisterAll();
            _hostInfo = null;
            _log.Debug("cleaned up");
        }

        [Test]
        public void Initialize_service_with_persisted_subscriptions() {
            XUri email = new XUri("http://mock/email");
            XUri deki = new XUri("http://mock/deki");
            MockPlug.Register(deki, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("deki: {0}", u);
                DreamMessage msg = DreamMessage.Ok(new XDoc("user")
                    .Attr("id", "1")
                    .Elem("email", "a@b.com")
                    .Start("permissions.user")
                        .Elem("operations", "READ,SUBSCRIBE,LOGIN")
                    .End());
                msg.Headers.Add("X-Deki-Site", "id=wicked");
                r2.Return(msg);
            });
            XUri subscribe = new XUri("http://mock/sub");
            int subscribeCalled = 0;
            MockPlug.Register(subscribe, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("subscribe: {0}", u);
                subscribeCalled++;
                DreamMessage msg = DreamMessage.Ok(new XDoc("foo"));
                msg.Headers.Location = subscribe;
                r2.Return(msg);
            });
            XUri storage = new XUri("http://mock/store");
            int storageListCalled = 0;
            int storageFileCalled = 0;
            int storageBadCalled = 0;
            MockPlug.Register(storage, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("storage: {0}", u);
                if(v == "GET") {
                    if(u == storage.At("subscriptions")) {
                        storageListCalled++;
                        r2.Return(DreamMessage.Ok(new XDoc("files")
                                                      .Start("folder").Elem("name", "wicked").End()
                                      ));
                        return;
                    } else if(u == storage.At("subscriptions", "wicked")) {
                        storageListCalled++;
                        r2.Return(DreamMessage.Ok(new XDoc("files")
                                                      .Start("file").Elem("name", "user_1.xml").End()
                                                      .Start("file").Elem("name", "bar.txt").End()
                                      ));
                        return;
                    } else if(u == storage.At("subscriptions", "wicked", "user_1.xml")) {
                        storageFileCalled++;
                        r2.Return(DreamMessage.Ok(new XDoc("user")
                                                      .Attr("userid", 1)
                                                      .Elem("email", "foo")
                                                      .Start("subscription.page").Attr("id", 1).Attr("depth", 0).End()
                                      ));
                        return;
                    }
                }
                storageBadCalled++;
                throw new DreamBadRequestException("unexpected call");
            });
            DreamServiceInfo serviceInfo = DreamTestHelper.CreateService(
                _hostInfo,
                typeof(DekiChangeSubscriptionService),
                "email",
                new XDoc("config")
                    .Elem("uri.emailer", email)
                    .Elem("uri.deki", deki)
                    .Elem("uri.pubsub", subscribe)
                    .Elem("uri.storage", storage)
                );
            Assert.AreEqual(1, subscribeCalled);
            Assert.AreEqual(2, storageListCalled);
            Assert.AreEqual(1, storageFileCalled);
            Assert.AreEqual(0, storageBadCalled);
            _log.Debug("get all subscriptions");
            DreamMessage response = serviceInfo.AtLocalHost
                .At("subscriptions")
                .WithHeader("X-Deki-Site", "id=wicked")
                .GetAsync()
                .Wait();
            Assert.IsTrue(response.IsSuccessful);
            XDoc subscriptions = response.ToDocument();
            XDoc sub = subscriptions["subscription.page"];
            Assert.AreEqual(1, sub.ListLength);
            Assert.AreEqual("1", sub["@id"].AsText);
            Assert.AreEqual("0", sub["@depth"].AsText);
        }

        [Test]
        public void Request_without_valid_user_headers_results_in_not_authorized() {

            // set up mocks for all the support service calls
            XUri deki = new XUri("http://mock/deki");
            MockPlug.Register(deki, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("deki: {0}", u);
                r2.Return(DreamMessage.AccessDenied("deki", "bad puppy"));
            });
            XUri subscribe = new XUri("http://mock/sub");
            MockPlug.Register(subscribe, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                if(u == subscribe.At("subscribers")) {
                    _log.Debug("creating subscription");
                    DreamMessage msg = DreamMessage.Ok(new XDoc("x"));
                    msg.Headers.Location = subscribe.At("testsub");
                    r2.Return(msg);
                } else {
                    _log.Debug("updating subscription");
                    r2.Return(DreamMessage.Ok());
                }
            });
            XUri storage = new XUri("http://mock/store");
            MockPlug.Register(storage, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("storage: {0}", u);
                r2.Return(DreamMessage.Ok(new XDoc("foo")));
            });

            // set up service
            _log.Debug("set up service");
            DreamServiceInfo serviceInfo = DreamTestHelper.CreateService(
                _hostInfo,
                typeof(DekiChangeSubscriptionService),
                "email",
                new XDoc("config")
                    .Elem("uri.emailer", new XUri("http://mock/email"))
                    .Elem("uri.deki", deki)
                    .Elem("uri.pubsub", subscribe)
                    .Elem("uri.storage", storage)
                );
            Plug service = serviceInfo.WithInternalKey().AtLocalHost;

            // post a subscription
            _log.Debug("post page 10 subscription");
            DreamMessage response = service
                .At("pages", "10")
                .With("depth", "infinity")
                .WithHeader("X-Deki-Site", "id=wicked")
                .PostAsync()
                .Wait();
            Assert.IsFalse(response.IsSuccessful);
            Assert.AreEqual(DreamStatus.Unauthorized, response.Status);
        }

        [Test]
        public void Request_for_user_without_email_results_in_special_bad_request() {

            // set up mocks for all the support service calls
            XUri deki = new XUri("http://mock/deki");
            MockPlug.Register(deki, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("deki: {0}", u);
                DreamMessage msg = DreamMessage.Ok(new XDoc("user")
                    .Attr("id", "1")
                    .Elem("email", "")
                    .Start("permissions.user")
                        .Elem("operations", "READ,SUBSCRIBE,LOGIN")
                    .End());
                msg.Headers.Add("X-Deki-Site", "id=wicked");
                r2.Return(msg);
            });
            XUri subscribe = new XUri("http://mock/sub");
            MockPlug.Register(subscribe, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                if(u == subscribe.At("subscribers")) {
                    _log.Debug("creating subscription");
                    DreamMessage msg = DreamMessage.Ok(new XDoc("x"));
                    msg.Headers.Location = subscribe.At("testsub");
                    r2.Return(msg);
                } else {
                    _log.Debug("updating subscription");
                    r2.Return(DreamMessage.Ok());
                }
            });
            XUri storage = new XUri("http://mock/store");
            MockPlug.Register(storage, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("storage: {0}", u);
                r2.Return(DreamMessage.Ok(new XDoc("foo")));
            });

            // set up service
            _log.Debug("set up service");
            DreamServiceInfo serviceInfo = DreamTestHelper.CreateService(
                _hostInfo,
                typeof(DekiChangeSubscriptionService),
                "email",
                new XDoc("config")
                    .Elem("uri.emailer", new XUri("http://mock/email"))
                    .Elem("uri.deki", deki)
                    .Elem("uri.pubsub", subscribe)
                    .Elem("uri.storage", storage)
                );
            Plug service = serviceInfo.WithInternalKey().AtLocalHost;

            // post a subscription
            _log.Debug("post page 10 subscription");
            DreamMessage response = service
                .At("pages", "10")
                .With("depth", "infinity")
                .WithHeader("X-Deki-Site", "id=wicked")
                .PostAsync()
                .Wait();
            Assert.IsFalse(response.IsSuccessful);
            Assert.AreEqual(DreamStatus.BadRequest, response.Status);
            Assert.AreEqual("no email for user", response.ToDocument()["message"].AsText);
        }

        [Test]
        public void Request_for_user_without_subscribe_role_results_in_special_bad_request() {

            // set up mocks for all the support service calls
            XUri deki = new XUri("http://mock/deki");
            MockPlug.Register(deki, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("deki: {0}", u);
                DreamMessage msg = DreamMessage.Ok(new XDoc("user")
                    .Attr("id", "1")
                    .Elem("email", "a@b.com")
                    .Start("permissions.user")
                        .Elem("operations", "READ,LOGIN")
                    .End());
                msg.Headers.Add("X-Deki-Site", "id=wicked");
                r2.Return(msg);
            });
            XUri subscribe = new XUri("http://mock/sub");
            MockPlug.Register(subscribe, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                if(u == subscribe.At("subscribers")) {
                    _log.Debug("creating subscription");
                    DreamMessage msg = DreamMessage.Ok(new XDoc("x"));
                    msg.Headers.Location = subscribe.At("testsub");
                    r2.Return(msg);
                } else {
                    _log.Debug("updating subscription");
                    r2.Return(DreamMessage.Ok());
                }
            });
            XUri storage = new XUri("http://mock/store");
            MockPlug.Register(storage, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("storage: {0}", u);
                r2.Return(DreamMessage.Ok(new XDoc("foo")));
            });

            // set up service
            _log.Debug("set up service");
            DreamServiceInfo serviceInfo = DreamTestHelper.CreateService(
                _hostInfo,
                typeof(DekiChangeSubscriptionService),
                "email",
                new XDoc("config")
                    .Elem("uri.emailer", new XUri("http://mock/email"))
                    .Elem("uri.deki", deki)
                    .Elem("uri.pubsub", subscribe)
                    .Elem("uri.storage", storage)
                );
            Plug service = serviceInfo.WithInternalKey().AtLocalHost;

            // post a subscription
            _log.Debug("post page 10 subscription");
            DreamMessage response = service
                .At("pages", "10")
                .With("depth", "infinity")
                .WithHeader("X-Deki-Site", "id=wicked")
                .PostAsync()
                .Wait();
            Assert.IsFalse(response.IsSuccessful);
            Assert.AreEqual(DreamStatus.BadRequest, response.Status);
            Assert.AreEqual("user does not have subscribe role", response.ToDocument()["message"].AsText);
        }

        [Test]
        public void Subscription_end_to_end() {

            // set up mocks for all the support service calls
            string apikey = "abc";
            XUri deki = new XUri("http://mock/deki");
            XUri dekiAuth = deki.At("users");
            var dekiUserResetEvent = new ManualResetEvent(false);
            var dekiPageResetEvent = new ManualResetEvent(false);
            var dekiSiteResetEvent = new ManualResetEvent(false);
            var subscribeResetEvent = new ManualResetEvent(false);
            var storagePutResetEvent = new ManualResetEvent(false);
            var storageGetResetEvent = new ManualResetEvent(false);
            var storageDeleteResetEvent = new ManualResetEvent(false);
            var emailResetEvent = new ManualResetEvent(false);
            Action reset = () => {
                dekiUserResetEvent.Reset();
                dekiPageResetEvent.Reset();
                dekiSiteResetEvent.Reset();
                subscribeResetEvent.Reset();
                storagePutResetEvent.Reset();
                storageGetResetEvent.Reset();
                storageDeleteResetEvent.Reset();
                emailResetEvent.Reset();
            };
            XDoc dekiUserResponse = null;
            XDoc dekiPageResponse = null;
            XDoc dekiFeedResponse = null;
            XDoc dekiSiteResponse = null;
            XDoc dekiPageRequest = null;
            XUri dekiCalledUri = null;
            MockPlug.Register(deki, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("deki: {0}", u);
                DreamMessage msg;
                if(u.Path.StartsWith("/deki/pages")) {
                    if(v != "GET") {
                        dekiPageRequest = r.ToDocument();
                    }
                    if(u.Path.EndsWith("feed")) {
                        msg = DreamMessage.Ok(dekiFeedResponse);
                    } else {
                        msg = DreamMessage.Ok(dekiPageResponse);
                    }
                    dekiPageResetEvent.Set();
                } else if(u.Path.StartsWith("/deki/site/settings")) {
                    msg = DreamMessage.Ok(dekiSiteResponse);
                    dekiSiteResetEvent.Set();
                } else {
                    msg = DreamMessage.Ok(dekiUserResponse);
                    dekiUserResetEvent.Set();
                    dekiCalledUri = u;
                }
                msg.Headers.Add("X-Deki-Site", "id=wicked");
                r2.Return(msg);
            });
            XUri email = new XUri("http://mock/email").With("apikey", "123");
            List<XDoc> emailPosted = new List<XDoc>();
            int emailsExpected = 0;
            MockPlug.Register(email, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("email: {0}", u);
                emailPosted.Add(r.ToDocument());
                if(emailPosted.Count >= emailsExpected) {
                    emailResetEvent.Set();
                }
                r2.Return(DreamMessage.Ok());
            });
            XUri subscribe = new XUri("http://mock/sub");
            XUri subscriptionLocation = subscribe.At("testsub");
            XUri subscribeCalledUri = null;
            XDoc subscribePosted = null;
            MockPlug.Register(subscribe, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                subscribeCalledUri = u;
                if(u == subscribe.At("subscribers")) {
                    _log.Debug("creating subscription");
                    DreamMessage msg = DreamMessage.Ok(new XDoc("x"));
                    msg.Headers.Location = subscriptionLocation;
                    subscribeResetEvent.Set();
                    r2.Return(msg);
                } else {
                    _log.Debug("updating subscription");
                    subscribePosted = r.ToDocument();
                    subscribeResetEvent.Set();
                    r2.Return(DreamMessage.Ok());
                }
            });
            XUri storage = new XUri("http://mock/store");
            XDoc storageResponse = null;
            List<Tuplet<string, XDoc>> storagePuts = new List<Tuplet<string, XDoc>>();
            int storagePutsExpected = 1;
            int deleteCalled = 0;
            MockPlug.Register(storage, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("storage: {0} - {1}", v, u);
                if(v == "PUT") {
                    string wikihost = u.Segments[u.Segments.Length - 2];
                    storagePuts.Add(new Tuplet<string, XDoc>(wikihost, r.ToDocument()));
                    if(storagePuts.Count >= storagePutsExpected) {
                        storagePutResetEvent.Set();
                    }
                } else if(v == "DELETE") {
                    deleteCalled++;
                    storageDeleteResetEvent.Set();
                } else if(v == "GET") {
                    storageGetResetEvent.Set();
                }

                r2.Return(DreamMessage.Ok(storageResponse));
            });
            // set up service
            _log.Debug("set up service");
            storageResponse = new XDoc("files");
            DreamServiceInfo serviceInfo = DreamTestHelper.CreateService(
                _hostInfo,
                typeof(DekiChangeSubscriptionService),
                "email",
                new XDoc("config")
                    .Elem("uri.emailer", email)
                    .Elem("uri.deki", deki)
                    .Elem("uri.pubsub", subscribe)
                    .Elem("uri.storage", storage)
                    .Elem("accumulation-time", 0)
                    .Elem("from-address", "foo@bar.com")
                    .Elem("apikey", apikey)
                );
            Plug service = serviceInfo.WithInternalKey().AtLocalHost;

            // expect:
            // - storage was queried
            // - subscription was created on subscribe
            Assert.IsTrue(Wait.For(() => storageGetResetEvent.WaitOne(100, true), 10.Seconds()));
            Assert.IsTrue(Wait.For(() => subscribeResetEvent.WaitOne(100, true), 10.Seconds()));
            Assert.AreEqual(subscribe.At("subscribers"), subscribeCalledUri);
            reset();

            // post a subscription
            _log.Debug("post a subscription");
            dekiUserResponse = new XDoc("user")
                .Attr("id", "1")
                .Elem("email", "a@b.com")
                .Elem("language", "en")
                .Start("permissions.user")
                    .Elem("operations", "READ,SUBSCRIBE,LOGIN")
                .End();
            dekiPageResponse = new XDoc("users").Start("user").Attr("id", 1).End();
            DreamMessage response = service
                .At("pages", "10")
                .With("depth", "infinity")
                .WithHeader("X-Deki-Site", "id=wicked")
                .PostAsync()
                .Wait();
            Assert.IsTrue(response.IsSuccessful, response.AsText());

            // expect:
            // - deki was queried for user info
            // - subscription location was updated with new sub
            // - storage was updated with new wiki subscription set
            Assert.IsTrue(Wait.For(() => storagePutResetEvent.WaitOne(100, true), 10.Seconds()));
            Assert.IsTrue(Wait.For(() => subscribeResetEvent.WaitOne(100, true), 10.Seconds()));
            Assert.IsTrue(Wait.For(() => dekiUserResetEvent.WaitOne(100, true), 10.Seconds()));
            Assert.IsTrue(Wait.For(() => dekiPageResetEvent.WaitOne(100, true), 10.Seconds()));
            Assert.AreEqual(1, dekiPageRequest["user/@id"].AsUInt.Value);
            Assert.AreEqual(dekiAuth.At("current"), dekiCalledUri);
            Assert.AreEqual(1, storagePuts.Count);
            Assert.AreEqual("wicked", storagePuts[0].Item1);
            Assert.AreEqual(subscriptionLocation, subscribeCalledUri);
            Assert.AreEqual(2, subscribePosted["subscription"].ListLength);
            Assert.AreEqual("deki://wicked/pages/10#depth=infinity", subscribePosted["subscription[channel='event://wicked/deki/pages/create']/uri.resource"].AsText);
            reset();

            //post another subscription
            _log.Debug("post another subscription");
            storagePuts.Clear();
            subscribePosted = null;
            dekiCalledUri = null;
            subscribeCalledUri = null;
            dekiPageRequest = null;
            dekiUserResponse = new XDoc("user")
                .Attr("id", "2")
                .Elem("email", "c@d.com")
                .Elem("language", "en")
                .Start("permissions.user")
                    .Elem("operations", "READ,SUBSCRIBE,LOGIN")
                .End();
            dekiPageResponse = new XDoc("users").Start("user").Attr("id", 2).End();
            response = service
                .At("pages", "10")
                .With("depth", "infinity")
                .WithHeader("X-Deki-Site", "id=wicked")
                .PostAsync()
                .Wait();
            Assert.IsTrue(response.IsSuccessful);

            // expect:
            // - deki was queried for user info
            // - subscription location was updated with new sub
            // - storage was updated with new wiki subscription set
            Assert.IsTrue(Wait.For(() => storagePutResetEvent.WaitOne(100, true), 10.Seconds()));
            Assert.IsTrue(Wait.For(() => subscribeResetEvent.WaitOne(100, true), 10.Seconds()));
            Assert.IsTrue(Wait.For(() => dekiUserResetEvent.WaitOne(100, true), 10.Seconds()));
            Assert.IsTrue(Wait.For(() => dekiPageResetEvent.WaitOne(100, true), 10.Seconds()));
            Assert.AreEqual(2, dekiPageRequest["user/@id"].AsUInt.Value);
            Assert.AreEqual(dekiAuth.At("current"), dekiCalledUri);
            Assert.AreEqual("wicked", storagePuts[0].Item1);
            Assert.AreEqual(1, storagePuts.Count);
            Assert.AreEqual(subscriptionLocation, subscribeCalledUri);
            Assert.AreEqual(2, subscribePosted["subscription"].ListLength);
            Assert.AreEqual(2, subscribePosted["subscription[channel='event://wicked/deki/pages/create']/recipient"].ListLength);
            reset();

            // post a page event
            _log.Debug("posting a page event");
            dekiCalledUri = null;
            subscribeCalledUri = null;
            string channel = "event://wicked/deki/pages/update";
            emailPosted.Clear();
            emailsExpected = 2;
            dekiPageResponse = new XDoc("page")
                .Elem("uri.ui", "http://foo.com/@api/deki/pages/10")
                .Elem("title", "foo")
                .Elem("path", "foo/bar");
            dekiFeedResponse = new XDoc("table")
                .Start("change")
                .Elem("rc_summary", "Two edits")
                .Elem("rc_comment", "edit 1")
                .Elem("rc_comment", "edit 2")
                .End();
            dekiSiteResponse = new XDoc("config")
                .Start("ui")
                    .Elem("sitename", "Test Site")
                    .Elem("language", "de-de")
                .End()
                .Start("page-subscription")
                    .Elem("from-address", "foo@test.com")
                .End();
            response = service.At("notify")
                .WithHeader(DreamHeaders.DREAM_EVENT_CHANNEL, channel)
                .WithHeader(DreamHeaders.DREAM_EVENT_RECIPIENT, "deki://wicked/user/1")
                .WithHeader(DreamHeaders.DREAM_EVENT_RECIPIENT, "deki://wicked/user/2")
                .PostAsync(CreateDekiEvent().Elem("channel", channel).Elem("pageid", 10))
                .Wait();
            Assert.IsTrue(response.IsSuccessful);

            // expect:
            // - email service is called for both users
            Assert.IsTrue(Wait.For(() => emailResetEvent.WaitOne(100, true), 10.Seconds()));
            Assert.IsTrue(Wait.For(() => dekiSiteResetEvent.WaitOne(100, true), 10.Seconds()));
            Assert.IsNull(dekiCalledUri);
            Assert.IsNull(subscribeCalledUri);
            Assert.AreEqual(2, emailPosted.Count);
            bool found1 = false;
            bool found2 = false;
            foreach(XDoc emailDoc in emailPosted) {
                XDoc para = emailDoc["body[@html='true']/p"];
                Assert.AreEqual("http://foo.com/@api/deki/pages/10", para["b/a/@href"].AsText);
                para = para.Next;
                Assert.AreEqual("<li>edit 1 ( <a href=\"http://foo.com/@api/deki/pages/10?revision\">Mon, 01 Jan 0001 00:00:00 GMT</a> by <a href=\"http://foo.com/User%3a\" /> )</li>", para["ol/li"].ToString());
                if(emailDoc["to"].AsText == "a@b.com") {
                    found1 = true;
                } else if(emailDoc["to"].AsText == "c@d.com") {
                    found2 = true;
                }
            }
            Assert.IsTrue(found1);
            Assert.IsTrue(found2);
            reset();

            // post a user update event
            _log.Debug("posting a user update event");
            dekiCalledUri = null;
            storagePuts.Clear();
            subscribeCalledUri = null;
            emailPosted.Clear();
            channel = "event://wicked/deki/users/update";
            response = service.At("updateuser")
                .WithHeader(DreamHeaders.DREAM_EVENT_CHANNEL, channel)
                .PostAsync(CreateDekiEvent().Elem("channel", channel).Elem("userid", 1))
                .Wait();
            Assert.IsTrue(response.IsSuccessful);

            // expect:
            // - nothing, user should be invalidated but no action should be taken on it
            Thread.Sleep(500); // give 'nothing' a chance to happen anyhow
            Assert.IsNull(dekiCalledUri);
            Assert.IsNull(subscribeCalledUri);
            Assert.IsEmpty(emailPosted);
            reset();

            // post another page event
            _log.Debug("posting another page event");
            dekiUserResponse = new XDoc("user")
                .Attr("id", "1")
                .Elem("email", "new@b.com")
                .Elem("language", "en")
                .Start("permissions.user")
                    .Elem("operations", "READ,SUBSCRIBE,LOGIN")
                .End();
            channel = "event://wicked/deki/pages/update";
            emailPosted.Clear();
            emailsExpected = 2;
            response = service.At("notify")
                .WithHeader(DreamHeaders.DREAM_EVENT_CHANNEL, channel)
                .WithHeader(DreamHeaders.DREAM_EVENT_RECIPIENT, "http://wicked/user/1")
                .WithHeader(DreamHeaders.DREAM_EVENT_RECIPIENT, "http://wicked/user/2")
                .PostAsync(CreateDekiEvent().Elem("channel", channel).Elem("pageid", 10))
                .Wait();
            Assert.IsTrue(response.IsSuccessful);

            // expect:
            // - deki is queried for invalidated user info
            // - storage was updated with new wiki subscription set
            // - email service is called for both users
            Assert.IsTrue(Wait.For(() => emailResetEvent.WaitOne(100, true), 10.Seconds()));
            Assert.IsTrue(Wait.For(() => dekiUserResetEvent.WaitOne(100, true), 10.Seconds()));
            Assert.IsTrue(Wait.For(() => storagePutResetEvent.WaitOne(100, true), 10.Seconds()));
            Assert.IsNull(subscribeCalledUri);
            Assert.AreEqual(dekiAuth.At("1").With("apikey", apikey), dekiCalledUri);
            Assert.AreEqual("wicked", storagePuts[0].Item1);
            Assert.AreEqual(1, storagePuts.Count);
            Assert.AreEqual(2, emailPosted.Count);
            found1 = false;
            found2 = false;
            foreach(XDoc emailDoc in emailPosted) {
                if(emailDoc["to"].AsText == "new@b.com") {
                    found1 = true;
                } else if(emailDoc["to"].AsText == "c@d.com") {
                    found2 = true;
                }
            }
            Assert.IsTrue(found1);
            Assert.IsTrue(found2);
            reset();

            // remove a subscription
            _log.Debug("remove a subscription");
            dekiCalledUri = null;
            storagePuts.Clear();
            subscribeCalledUri = null;
            dekiUserResponse = new XDoc("user")
                .Attr("id", "1")
                .Elem("email", "new@b.com")
                .Elem("language", "en")
                .Start("permissions.user")
                    .Elem("operations", "READ,SUBSCRIBE,LOGIN")
                .End();
            response = service.At("pages", "10")
                .WithHeader("X-Deki-Site", "id=wicked")
                .DeleteAsync()
                .Wait();
            Assert.IsTrue(response.IsSuccessful);

            // expect:
            // - deki was queried to get user info
            // - subscription location was updated with new sub
            // - storage was updated with new wiki subscription set
            Assert.IsTrue(Wait.For(() => storageDeleteResetEvent.WaitOne(100, true), 10.Seconds()));
            Assert.IsTrue(Wait.For(() => subscribeResetEvent.WaitOne(100, true), 10.Seconds()));
            Assert.IsTrue(Wait.For(() => dekiUserResetEvent.WaitOne(100, true), 10.Seconds()));
            Assert.AreEqual(dekiAuth.At("current"), dekiCalledUri);
            Assert.AreEqual(1, deleteCalled);
            Assert.AreEqual(subscriptionLocation, subscribeCalledUri);
            Assert.AreEqual(2, subscribePosted["subscription"].ListLength);
            Assert.AreEqual(1, subscribePosted["subscription[channel='event://wicked/deki/pages/create']/recipient"].ListLength);
            Assert.AreEqual("2", subscribePosted["subscription/recipient/@userid"].AsText);
            reset();

            // post a page event
            _log.Debug("posting a final page event");
            dekiCalledUri = null;
            subscribeCalledUri = null;
            emailPosted.Clear();
            channel = "event://wicked/deki/pages/update";
            emailsExpected = 1;
            response = service.At("notify")
                .WithHeader(DreamHeaders.DREAM_EVENT_CHANNEL, channel)
                .WithHeader(DreamHeaders.DREAM_EVENT_RECIPIENT, "http://wicked/user/2")
                .PostAsync(CreateDekiEvent().Elem("channel", channel).Elem("pageid", 10))
                .Wait();
            Assert.IsTrue(response.IsSuccessful);

            // expect:
            // - email service is called for remaining user
            Assert.IsTrue(Wait.For(() => emailResetEvent.WaitOne(100, true), 10.Seconds()));
            Assert.IsNull(dekiCalledUri);
            Assert.IsNull(subscribeCalledUri);
            Assert.AreEqual(1, emailPosted.Count);
            Assert.AreEqual("c@d.com", emailPosted[0]["to"].AsText);
            reset();
        }

        [Test]
        public void Retrieve_subscriptions() {
            // set up mocks for all the support service calls
            XUri deki = new XUri("http://mock/deki");
            MockPlug.Register(deki, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("deki: {0}", u);
                DreamMessage msg;
                if(u.Path.StartsWith("/deki/pages")) {
                    msg = DreamMessage.Ok(new XDoc("users").Start("user").Attr("id", "1").End());
                } else {
                    msg = DreamMessage.Ok(new XDoc("user")
                        .Attr("id", "1")
                        .Elem("email", "a@b.com")
                        .Elem("language", "en")
                        .Start("permissions.user")
                            .Elem("operations", "READ,SUBSCRIBE,LOGIN")
                        .End());
                }
                msg.Headers.Add("X-Deki-Site", "id=wicked");
                r2.Return(msg);
            });
            XUri subscribe = new XUri("http://mock/sub");
            MockPlug.Register(subscribe, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                if(u == subscribe.At("subscribers")) {
                    _log.Debug("creating subscription");
                    DreamMessage msg = DreamMessage.Ok(new XDoc("x"));
                    msg.Headers.Location = subscribe.At("testsub");
                    r2.Return(msg);
                } else {
                    _log.Debug("updating subscription");
                    r2.Return(DreamMessage.Ok());
                }
            });
            XUri storage = new XUri("http://mock/store");
            MockPlug.Register(storage, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("storage: {0}", u);
                r2.Return(DreamMessage.Ok(new XDoc("foo")));
            });

            // set up service
            _log.Debug("set up service");
            DreamServiceInfo serviceInfo = DreamTestHelper.CreateService(
                _hostInfo,
                typeof(DekiChangeSubscriptionService),
                "email",
                new XDoc("config")
                    .Elem("uri.emailer", new XUri("http://mock/email"))
                    .Elem("uri.deki", deki)
                    .Elem("uri.pubsub", subscribe)
                    .Elem("uri.storage", storage)
                );
            Plug service = serviceInfo.WithInternalKey().AtLocalHost;

            // post a subscription
            _log.Debug("post page 10 subscription");
            DreamMessage response = service
                .At("pages", "10")
                .With("depth", "infinity")
                .WithHeader("X-Deki-Site", "id=wicked")
                .PostAsync()
                .Wait();
            Assert.IsTrue(response.IsSuccessful);

            // post a subscription
            _log.Debug("post page 11 subscription");
            response = service
                .At("pages", "11")
                .WithHeader("X-Deki-Site", "id=wicked")
                .PostAsync()
                .Wait();
            Assert.IsTrue(response.IsSuccessful);

            // post a subscription
            _log.Debug("post page 12 subscription");
            response = service
                .At("pages", "12")
                .With("depth", "0")
                .WithHeader("X-Deki-Site", "id=wicked")
                .PostAsync()
                .Wait();
            Assert.IsTrue(response.IsSuccessful);

            // post a subscription
            _log.Debug("post page 13 subscription");
            response = service
                .At("pages", "13")
                .With("depth", "infinity")
                .WithHeader("X-Deki-Site", "id=wicked")
                .PostAsync()
                .Wait();
            Assert.IsTrue(response.IsSuccessful);

            // get subscriptions for some pages
            _log.Debug("get some subscriptions");
            response = service
                .At("subscriptions")
                .With("pages", "10,12,14,16")
                .WithHeader("X-Deki-Site", "id=wicked")
                .GetAsync()
                .Wait();
            Assert.IsTrue(response.IsSuccessful);
            XDoc subscriptions = response.ToDocument();
            Assert.AreEqual(2, subscriptions["subscription.page"].ListLength);
            XDoc page10 = subscriptions["subscription.page[@id='10']"];
            Assert.IsFalse(page10.IsEmpty);
            Assert.AreEqual("infinity", page10["@depth"].AsText);
            XDoc page12 = subscriptions["subscription.page[@id='12']"];
            Assert.IsFalse(page12.IsEmpty);
            Assert.AreEqual("0", page12["@depth"].AsText);

            // get all subscriptions
            _log.Debug("get all subscriptions");
            response = service
                .At("subscriptions")
                .WithHeader("X-Deki-Site", "id=wicked")
                .GetAsync()
                .Wait();
            Assert.IsTrue(response.IsSuccessful);
            subscriptions = response.ToDocument();
            Assert.AreEqual(4, subscriptions["subscription.page"].ListLength);
        }

        [Test]
        public void Retrieve_subscriptions_with_wikiid_query_arg() {
            // set up mocks for all the support service calls
            XUri deki = new XUri("http://mock/deki");
            MockPlug.Register(deki, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("deki: {0}", u);
                DreamMessage msg;
                if(u.Path.StartsWith("/deki/pages")) {
                    msg = DreamMessage.Ok(new XDoc("users").Start("user").Attr("id", "1").End());
                } else {
                    msg = DreamMessage.Ok(new XDoc("user")
                        .Attr("id", "1")
                        .Elem("email", "a@b.com")
                        .Elem("language", "en")
                        .Start("permissions.user")
                            .Elem("operations", "READ,SUBSCRIBE,LOGIN")
                        .End());
                }
                msg.Headers.Add("X-Deki-Site", "id=wicked");
                r2.Return(msg);
            });
            XUri subscribe = new XUri("http://mock/sub");
            MockPlug.Register(subscribe, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                if(u == subscribe.At("subscribers")) {
                    _log.Debug("creating subscription");
                    DreamMessage msg = DreamMessage.Ok(new XDoc("x"));
                    msg.Headers.Location = subscribe.At("testsub");
                    r2.Return(msg);
                } else {
                    _log.Debug("updating subscription");
                    r2.Return(DreamMessage.Ok());
                }
            });
            XUri storage = new XUri("http://mock/store");
            MockPlug.Register(storage, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("storage: {0}", u);
                r2.Return(DreamMessage.Ok(new XDoc("foo")));
            });

            // set up service
            _log.Debug("set up service");
            DreamServiceInfo serviceInfo = DreamTestHelper.CreateService(
                _hostInfo,
                typeof(DekiChangeSubscriptionService),
                "email",
                new XDoc("config")
                    .Elem("uri.emailer", new XUri("http://mock/email"))
                    .Elem("uri.deki", deki)
                    .Elem("uri.pubsub", subscribe)
                    .Elem("uri.storage", storage)
                );
            Plug service = serviceInfo.WithInternalKey().AtLocalHost;

            // post a subscription
            _log.Debug("post page 10 subscription");
            DreamMessage response = service
                .At("pages", "10")
                .With("depth", "infinity")
                .WithHeader("X-Deki-Site", "id=wicked")
                .PostAsync()
                .Wait();
            Assert.IsTrue(response.IsSuccessful);

            // post a subscription
            _log.Debug("post page 11 subscription");
            response = service
                .At("pages", "11")
                .With("siteid", "wicked")
                .PostAsync()
                .Wait();
            Assert.IsTrue(response.IsSuccessful);
            // post a subscription
            _log.Debug("post page 12 subscription");
            response = service
                .At("pages", "12")
                .With("depth", "0")
                .With("siteid", "wicked")
                .PostAsync()
                .Wait();
            Assert.IsTrue(response.IsSuccessful);
            // post a subscription
            _log.Debug("post page 13 subscription");
            response = service
                .At("pages", "13")
                .With("depth", "infinity")
                .With("siteid", "wicked")
                .PostAsync()
                .Wait();
            Assert.IsTrue(response.IsSuccessful);

            _log.Debug("get some subscriptions");
            response = service
                .At("subscriptions")
                .With("pages", "10,12,14,16")
                .With("siteid", "wicked")
                .GetAsync()
                .Wait();
            Assert.IsTrue(response.IsSuccessful);
            XDoc subscriptions = response.ToDocument();
            Assert.AreEqual(2, subscriptions["subscription.page"].ListLength);
            XDoc page10 = subscriptions["subscription.page[@id='10']"];
            Assert.IsFalse(page10.IsEmpty);
            Assert.AreEqual("infinity", page10["@depth"].AsText);
            XDoc page12 = subscriptions["subscription.page[@id='12']"];
            Assert.IsFalse(page12.IsEmpty);
            Assert.AreEqual("0", page12["@depth"].AsText);

            _log.Debug("get all subscriptions");
            response = service
                .At("subscriptions")
                .With("siteid", "wicked")
                .GetAsync()
                .Wait();
            Assert.IsTrue(response.IsSuccessful);
            subscriptions = response.ToDocument();
            Assert.AreEqual(4, subscriptions["subscription.page"].ListLength);

        }

        [Test]
        public void User_without_proper_page_permission_gets_forbidden_on_subscribe_attempt() {

            // set up mocks for all the support service calls
            XUri deki = new XUri("http://mock/deki");
            MockPlug.Register(deki, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("deki: {0}", u);
                DreamMessage msg;
                if(u.Path.StartsWith("/deki/pages")) {
                    msg = DreamMessage.Ok(new XDoc("users"));
                } else {
                    msg = DreamMessage.Ok(new XDoc("user")
                        .Attr("id", "1")
                        .Elem("email", "a@b.com")
                        .Elem("language", "en")
                        .Start("permissions.user")
                            .Elem("operations", "READ,SUBSCRIBE,LOGIN")
                        .End());
                }
                msg.Headers.Add("X-Deki-Site", "id=wicked");
                r2.Return(msg);
            });
            XUri subscribe = new XUri("http://mock/sub");
            MockPlug.Register(subscribe, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                if(u == subscribe.At("subscribers")) {
                    _log.Debug("creating subscription");
                    DreamMessage msg = DreamMessage.Ok(new XDoc("x"));
                    msg.Headers.Location = subscribe.At("testsub");
                    r2.Return(msg);
                } else {
                    _log.Debug("updating subscription");
                    r2.Return(DreamMessage.Ok());
                }
            });
            XUri storage = new XUri("http://mock/store");
            MockPlug.Register(storage, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("storage: {0}", u);
                r2.Return(DreamMessage.Ok(new XDoc("foo")));
            });

            // set up service
            _log.Debug("set up service");
            DreamServiceInfo serviceInfo = DreamTestHelper.CreateService(
                _hostInfo,
                typeof(DekiChangeSubscriptionService),
                "email",
                new XDoc("config")
                    .Elem("uri.emailer", new XUri("http://mock/email"))
                    .Elem("uri.deki", deki)
                    .Elem("uri.pubsub", subscribe)
                    .Elem("uri.storage", storage)
                );
            Plug service = serviceInfo.WithInternalKey().AtLocalHost;

            // post a subscription
            _log.Debug("post page 10 subscription");
            DreamMessage response = service
                .At("pages", "10")
                .With("depth", "infinity")
                .WithHeader("X-Deki-Site", "id=wicked")
                .PostAsync()
                .Wait();
            Assert.IsFalse(response.IsSuccessful);
            Assert.AreEqual(DreamStatus.Forbidden, response.Status);
        }

        [Test]
        public void User_update_removing_subscribe_role_wipes_subscriptions() {
            ManualResetEvent dekiResetEvent = new ManualResetEvent(false);
            ManualResetEvent emailResetEvent = new ManualResetEvent(false);
            ManualResetEvent subscribeResetEvent = new ManualResetEvent(false);
            ManualResetEvent storagePutResetEvent = new ManualResetEvent(false);
            ManualResetEvent storageGetResetEvent = new ManualResetEvent(false);
            ManualResetEvent storageDeleteResetEvent = new ManualResetEvent(false);
            Action resetAll = () => {
                dekiResetEvent.Reset();
                emailResetEvent.Reset();
                subscribeResetEvent.Reset();
                storagePutResetEvent.Reset();
                storageGetResetEvent.Reset();
                storageDeleteResetEvent.Reset();
            };

            // set up mocks for all the support service calls
            XUri deki = new XUri("http://mock/deki");
            XUri dekiAuth = deki.At("users");
            XDoc dekiResponse = null;
            XUri dekiCalledUri = null;
            MockPlug.Register(deki, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("deki: {0}", u);
                DreamMessage msg;
                if(u.Path.StartsWith("/deki/pages")) {
                    msg = DreamMessage.Ok(new XDoc("users").Start("user").Attr("id", "1").End());
                } else {
                    dekiCalledUri = u;
                    dekiResetEvent.Set();
                    msg = DreamMessage.Ok(dekiResponse);
                }
                msg.Headers.Add("X-Deki-Site", "id=wicked");
                r2.Return(msg);
            });
            XUri email = new XUri("http://mock/email").With("apikey", "123");
            List<XDoc> emailPosted = new List<XDoc>();
            MockPlug.Register(email, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("email: {0}", u);
                emailPosted.Add(r.ToDocument());
                emailResetEvent.Set();
                r2.Return(DreamMessage.Ok());
            });
            XUri subscribe = new XUri("http://mock/sub");
            XUri subscriptionLocation = subscribe.At("testsub");
            XUri subscribeCalledUri = null;
            XDoc subscribePosted = null;
            MockPlug.Register(subscribe, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                subscribeCalledUri = u;
                if(u == subscribe.At("subscribers")) {
                    _log.Debug("creating subscription");
                    DreamMessage msg = DreamMessage.Ok(new XDoc("x"));
                    msg.Headers.Location = subscriptionLocation;
                    subscribeResetEvent.Set();
                    r2.Return(msg);
                } else {
                    _log.Debug("updating subscription");
                    subscribePosted = r.ToDocument();
                    subscribeResetEvent.Set();
                    r2.Return(DreamMessage.Ok());
                }
            });
            XUri storage = new XUri("http://mock/store");
            XDoc storageResponse = null;
            List<Tuplet<string, XDoc>> storagePuts = new List<Tuplet<string, XDoc>>();
            int storagePutsExpected = 0;
            int deleteCalled = 0;
            MockPlug.Register(storage, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("storage: {0} - {1}", v, u);
                if(v == "PUT") {
                    string wikihost = u.Segments[u.Segments.Length - 2];
                    storagePuts.Add(new Tuplet<string, XDoc>(wikihost, r.ToDocument()));
                    if(storagePuts.Count >= storagePutsExpected) {
                        storagePutResetEvent.Set();
                    }
                } else if(v == "DELETE") {
                    deleteCalled++;
                    storageDeleteResetEvent.Set();
                } else if(v == "GET") {
                    storageGetResetEvent.Set();
                }

                r2.Return(DreamMessage.Ok(storageResponse));
            });

            // set up service
            _log.Debug("set up service");
            resetAll();
            storageResponse = new XDoc("files");
            DreamServiceInfo serviceInfo = DreamTestHelper.CreateService(
                _hostInfo,
                typeof(DekiChangeSubscriptionService),
                "email",
                new XDoc("config")
                    .Elem("uri.emailer", email)
                    .Elem("uri.deki", deki)
                    .Elem("uri.pubsub", subscribe)
                    .Elem("uri.storage", storage)
                );
            Plug service = serviceInfo.WithInternalKey().AtLocalHost;

            // expect:
            // - storage was queried
            // - subscription was created on subscribe
            Assert.IsTrue(storageGetResetEvent.WaitOne(100, true));
            Assert.IsTrue(subscribeResetEvent.WaitOne(100, true));
            Assert.AreEqual(subscribe.At("subscribers"), subscribeCalledUri);

            // post a subscription
            resetAll();
            storagePutsExpected = 1;
            _log.Debug("post a subscription");
            dekiResponse = new XDoc("user")
                .Attr("id", "1")
                .Elem("email", "a@b.com")
                .Elem("language", "en")
                .Start("permissions.user")
                    .Elem("operations", "READ,SUBSCRIBE,LOGIN")
                .End();
            DreamMessage response = service
                .At("pages", "10")
                .With("depth", "infinity")
                .WithHeader("X-Deki-Site", "id=wicked")
                .PostAsync()
                .Wait();
            Assert.IsTrue(response.IsSuccessful);

            // expect:
            // - deki was queried for user info
            // - subscription location was updated with new sub
            // - storage was updated with new wiki subscription set
            Assert.IsTrue(storagePutResetEvent.WaitOne(1000, true));
            Assert.IsTrue(subscribeResetEvent.WaitOne(1000, true));
            Assert.IsTrue(dekiResetEvent.WaitOne(1000, true));
            Thread.Sleep(100);
            Assert.AreEqual(dekiAuth.At("current"), dekiCalledUri);
            Assert.AreEqual("wicked", storagePuts[0].Item1);
            Assert.AreEqual(1, storagePuts.Count);
            Assert.AreEqual(subscriptionLocation, subscribeCalledUri);
            Assert.AreEqual(2, subscribePosted["subscription"].ListLength);
            Assert.AreEqual("deki://wicked/pages/10#depth=infinity", subscribePosted["subscription[channel='event://wicked/deki/pages/create']/uri.resource"].AsText);

            // make sure user has a subscription
            resetAll();
            response = service
               .At("subscriptions")
               .WithHeader("X-Deki-Site", "id=wicked")
               .GetAsync()
               .Wait();
            Assert.IsTrue(response.IsSuccessful);
            var subscriptions = response.ToDocument();
            Assert.AreEqual(1, subscriptions["subscription.page"].ListLength);

            // post a user update event
            _log.Debug("posting a user update event");
            resetAll();
            storagePuts.Clear();
            subscribeCalledUri = null;
            emailPosted.Clear();
            deleteCalled = 0;
            string channel = "event://wicked/deki/users/update";
            response = service.At("updateuser")
                .WithHeader(DreamHeaders.DREAM_EVENT_CHANNEL, channel)
                .PostAsync(CreateDekiEvent().Elem("channel", channel).Elem("userid", 1))
                .Wait();
            Assert.IsTrue(response.IsSuccessful);

            // expect:
            // - deki was not queried for user info
            Assert.IsFalse(dekiResetEvent.WaitOne(1000, true));

            // check for subscriptions
            _log.Debug("get all subscriptions");
            resetAll();
            dekiCalledUri = null;
            dekiResponse = new XDoc("user")
                .Attr("id", "1")
                .Elem("email", "a@b.com")
                .Elem("language", "en")
                .Start("permissions.user")
                    .Elem("operations", "READ,LOGIN")
                .End();
            response = service
                .At("subscriptions")
                .WithHeader("X-Deki-Site", "id=wicked")
                .GetAsync()
                .Wait();

            // expect:
            // - deki was queried for user info
            // - subscription is empty
            Assert.IsFalse(response.IsSuccessful);
            Assert.AreEqual(DreamStatus.BadRequest, response.Status);
            Assert.AreEqual("user does not have subscribe role", response.ToDocument()["message"].AsText);
            Assert.IsTrue(dekiResetEvent.WaitOne(1000, true));
            Assert.AreEqual(dekiAuth.At("current"), dekiCalledUri);
        }

        [Test]
        public void User_delete_event_wipes_subscriptions() {

            // set up mocks for all the support service calls
            XUri deki = new XUri("http://mock/deki");
            XUri dekiAuth = deki.At("users");
            AutoResetEvent dekiResetEvent = new AutoResetEvent(false);
            XDoc dekiResponse = null;
            XUri dekiCalledUri = null;
            MockPlug.Register(deki, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("deki: {0}", u);
                DreamMessage msg;
                if(u.Path.StartsWith("/deki/pages")) {
                    msg = DreamMessage.Ok(new XDoc("users").Start("user").Attr("id", "1").End());
                } else {
                    dekiCalledUri = u;
                    dekiResetEvent.Set();
                    msg = DreamMessage.Ok(dekiResponse);
                }
                msg.Headers.Add("X-Deki-Site", "id=wicked");
                r2.Return(msg);
            });
            XUri email = new XUri("http://mock/email").With("apikey", "123");
            AutoResetEvent emailResetEvent = new AutoResetEvent(false);
            List<XDoc> emailPosted = new List<XDoc>();
            MockPlug.Register(email, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("email: {0}", u);
                emailPosted.Add(r.ToDocument());
                emailResetEvent.Set();
                r2.Return(DreamMessage.Ok());
            });
            XUri subscribe = new XUri("http://mock/sub");
            XUri subscriptionLocation = subscribe.At("testsub");
            AutoResetEvent subscribeResetEvent = new AutoResetEvent(false);
            XUri subscribeCalledUri = null;
            XDoc subscribePosted = null;
            MockPlug.Register(subscribe, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                subscribeCalledUri = u;
                if(u == subscribe.At("subscribers")) {
                    _log.Debug("creating subscription");
                    DreamMessage msg = DreamMessage.Ok(new XDoc("x"));
                    msg.Headers.Location = subscriptionLocation;
                    subscribeResetEvent.Set();
                    r2.Return(msg);
                } else {
                    _log.Debug("updating subscription");
                    subscribePosted = r.ToDocument();
                    subscribeResetEvent.Set();
                    r2.Return(DreamMessage.Ok());
                }
            });
            XUri storage = new XUri("http://mock/store");
            AutoResetEvent storagePutResetEvent = new AutoResetEvent(false);
            AutoResetEvent storageGetResetEvent = new AutoResetEvent(false);
            AutoResetEvent storageDeleteResetEvent = new AutoResetEvent(false);
            XDoc storageResponse = null;
            List<Tuplet<string, XDoc>> storagePuts = new List<Tuplet<string, XDoc>>();
            int storagePutsExpected = 0;
            int deleteCalled = 0;
            MockPlug.Register(storage, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("storage: {0} - {1}", v, u);
                if(v == "PUT") {
                    string wikihost = u.Segments[u.Segments.Length - 2];
                    storagePuts.Add(new Tuplet<string, XDoc>(wikihost, r.ToDocument()));
                    if(storagePuts.Count >= storagePutsExpected) {
                        storagePutResetEvent.Set();
                    }
                } else if(v == "DELETE") {
                    deleteCalled++;
                    storageDeleteResetEvent.Set();
                } else if(v == "GET") {
                    storageGetResetEvent.Set();
                }

                r2.Return(DreamMessage.Ok(storageResponse));
            });

            // set up service
            _log.Debug("set up service");
            storageResponse = new XDoc("files");
            DreamServiceInfo serviceInfo = DreamTestHelper.CreateService(
                _hostInfo,
                typeof(DekiChangeSubscriptionService),
                "email",
                new XDoc("config")
                    .Elem("uri.emailer", email)
                    .Elem("uri.deki", deki)
                    .Elem("uri.pubsub", subscribe)
                    .Elem("uri.storage", storage)
                );
            Plug service = serviceInfo.WithInternalKey().AtLocalHost;

            // expect:
            // - storage was queried
            // - subscription was created on subscribe
            Assert.IsTrue(storageGetResetEvent.WaitOne(100, true));
            Assert.IsTrue(subscribeResetEvent.WaitOne(100, true));
            Assert.AreEqual(subscribe.At("subscribers"), subscribeCalledUri);

            // post a subscription
            storagePutsExpected = 1;
            _log.Debug("post a subscription");
            dekiResponse = new XDoc("user")
                .Attr("id", "1")
                .Elem("email", "a@b.com")
                .Elem("language", "en")
                .Start("permissions.user")
                    .Elem("operations", "READ,SUBSCRIBE,LOGIN")
                .End();
            DreamMessage response = service
                .At("pages", "10")
                .With("depth", "infinity")
                .WithHeader("X-Deki-Site", "id=wicked")
                .PostAsync()
                .Wait();
            Assert.IsTrue(response.IsSuccessful);

            // expect:
            // - deki was queried for user info
            // - subscription location was updated with new sub
            // - storage was updated with new wiki subscription set
            Assert.IsTrue(storagePutResetEvent.WaitOne(1000, true));
            Assert.IsTrue(subscribeResetEvent.WaitOne(1000, true));
            Assert.IsTrue(dekiResetEvent.WaitOne(1000, true));
            Thread.Sleep(100);
            Assert.AreEqual(dekiAuth.At("current"), dekiCalledUri);
            Assert.AreEqual("wicked", storagePuts[0].Item1);
            Assert.AreEqual(1, storagePuts.Count);
            Assert.AreEqual(subscriptionLocation, subscribeCalledUri);
            Assert.AreEqual(2, subscribePosted["subscription"].ListLength);
            Assert.AreEqual("deki://wicked/pages/10#depth=infinity", subscribePosted["subscription[channel='event://wicked/deki/pages/create']/uri.resource"].AsText);

            // post a user delete event
            _log.Debug("posting a user delete event");
            dekiCalledUri = null;
            storagePuts.Clear();
            subscribeCalledUri = null;
            emailPosted.Clear();
            string channel = "event://wicked/deki/users/delete";
            response = service.At("updateuser")
                .WithHeader(DreamHeaders.DREAM_EVENT_CHANNEL, channel)
                .PostAsync(CreateDekiEvent().Elem("channel", channel).Elem("userid", 1))
                .Wait();
            Assert.IsTrue(response.IsSuccessful);

            // expect:
            // - storage should be called and user is no longer in set
            // - new sub set should be pushed upstream
            Assert.IsTrue(storageDeleteResetEvent.WaitOne(1000, true));
            Assert.IsTrue(subscribeResetEvent.WaitOne(1000, true));
            Assert.IsNull(dekiCalledUri);
            Assert.IsEmpty(emailPosted);
            Assert.AreEqual(1, deleteCalled);
            Assert.AreEqual(subscriptionLocation, subscribeCalledUri);
            Assert.AreEqual(1, subscribePosted["subscription"].ListLength);
        }

        [Test]
        public void Deleted_page_event_wipes_subscriptions_for_page() {

            // set up mocks for all the support service calls
            XUri deki = new XUri("http://mock/deki");
            XUri dekiAuth = deki.At("users");
            AutoResetEvent dekiUserResetEvent = new AutoResetEvent(false);
            AutoResetEvent dekiPageResetEvent = new AutoResetEvent(false);
            AutoResetEvent dekiSiteResetEvent = new AutoResetEvent(false);
            XDoc dekiUserResponse = null;
            XDoc dekiPageResponse = null;
            XDoc dekiFeedResponse = null;
            XDoc dekiSiteResponse = null;
            XDoc dekiPageRequest = null;
            XUri dekiCalledUri = null;
            MockPlug.Register(deki, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("deki: {0}", u);
                DreamMessage msg;
                if(u.Path.StartsWith("/deki/pages")) {
                    if(v != "GET") {
                        dekiPageRequest = r.ToDocument();
                    }
                    if(u.Path.EndsWith("feed")) {
                        msg = DreamMessage.Ok(dekiFeedResponse);
                    } else {
                        msg = DreamMessage.Ok(dekiPageResponse);
                    }
                    dekiPageResetEvent.Set();
                } else if(u.Path.StartsWith("/deki/site/settings")) {
                    msg = DreamMessage.Ok(dekiSiteResponse);
                    dekiSiteResetEvent.Set();
                } else {
                    msg = DreamMessage.Ok(dekiUserResponse);
                    dekiUserResetEvent.Set();
                    dekiCalledUri = u;
                }
                msg.Headers.Add("X-Deki-Site", "id=wicked");
                r2.Return(msg);
            });
            XUri email = new XUri("http://mock/email").With("apikey", "123");
            AutoResetEvent emailResetEvent = new AutoResetEvent(false);
            List<XDoc> emailPosted = new List<XDoc>();
            MockPlug.Register(email, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("email: {0}", u);
                emailPosted.Add(r.ToDocument());
                emailResetEvent.Set();
                r2.Return(DreamMessage.Ok());
            });
            XUri subscribe = new XUri("http://mock/sub");
            XUri subscriptionLocation = subscribe.At("testsub");
            AutoResetEvent subscribeResetEvent = new AutoResetEvent(false);
            XUri subscribeCalledUri = null;
            XDoc subscribePosted = null;
            MockPlug.Register(subscribe, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                subscribeCalledUri = u;
                if(u == subscribe.At("subscribers")) {
                    _log.Debug("creating subscription");
                    DreamMessage msg = DreamMessage.Ok(new XDoc("x"));
                    msg.Headers.Location = subscriptionLocation;
                    subscribeResetEvent.Set();
                    r2.Return(msg);
                } else {
                    _log.Debug("updating subscription");
                    subscribePosted = r.ToDocument();
                    subscribeResetEvent.Set();
                    r2.Return(DreamMessage.Ok());
                }
            });
            XUri storage = new XUri("http://mock/store");
            AutoResetEvent storagePutResetEvent = new AutoResetEvent(false);
            AutoResetEvent storageGetResetEvent = new AutoResetEvent(false);
            AutoResetEvent storageDeleteResetEvent = new AutoResetEvent(false);
            XDoc storageResponse = null;
            List<Tuplet<string, XDoc>> storagePuts = new List<Tuplet<string, XDoc>>();
            int storagePutsExpected = 0;
            XUri storageGetCalled = null;
            int deleteCalled = 0;
            MockPlug.Register(storage, delegate(Plug p, string v, XUri u, DreamMessage r, Result<DreamMessage> r2) {
                _log.DebugFormat("storage: {0} - {1}", v, u);
                if(v == "PUT") {
                    string wikihost = u.Segments[u.Segments.Length - 2];
                    storagePuts.Add(new Tuplet<string, XDoc>(wikihost, r.ToDocument()));
                    if(storagePuts.Count >= storagePutsExpected) {
                        storagePutResetEvent.Set();
                    }
                } else if(v == "DELETE") {
                    deleteCalled++;
                    storageDeleteResetEvent.Set();
                } else if(v == "GET") {
                    storageGetCalled = u;
                    storageGetResetEvent.Set();
                }
                r2.Return(DreamMessage.Ok(storageResponse));
            });

            // set up service
            _log.Debug("set up service");
            storageResponse = new XDoc("files");
            DreamServiceInfo serviceInfo = DreamTestHelper.CreateService(
                _hostInfo,
                typeof(DekiChangeSubscriptionService),
                "email",
                new XDoc("config")
                    .Elem("uri.emailer", email)
                    .Elem("uri.deki", deki)
                    .Elem("uri.pubsub", subscribe)
                    .Elem("uri.storage", storage)
                    .Elem("accumulation-time", 0)
                    .Elem("from-address", "foo@bar.com")
                );
            Plug service = serviceInfo.WithInternalKey().AtLocalHost;

            // expect:
            // - storage was queried
            // - subscription was created on subscribe
            Assert.IsTrue(storageGetResetEvent.WaitOne(100, true));
            Assert.IsTrue(subscribeResetEvent.WaitOne(100, true));
            Assert.AreEqual(storage.At("subscriptions"), storageGetCalled);
            Assert.AreEqual(subscribe.At("subscribers"), subscribeCalledUri);

            // post a subscription
            _log.Debug("post a subscription");
            storagePutsExpected = 1;
            dekiUserResponse = new XDoc("user")
                .Attr("id", "1")
                .Elem("email", "a@b.com")
                .Elem("language", "en")
                .Start("permissions.user")
                    .Elem("operations", "READ,SUBSCRIBE,LOGIN")
                .End();
            dekiPageResponse = new XDoc("users").Start("user").Attr("id", "1").End();
            DreamMessage response = service
                .At("pages", "10")
                .With("depth", "infinity")
                .WithHeader("X-Deki-Site", "id=wicked")
                .PostAsync()
                .Wait();
            Assert.IsTrue(response.IsSuccessful);

            // expect:
            // - deki was queried for user info
            // - subscription location was updated with new sub
            // - storage was updated with new wiki subscription set
            Assert.IsTrue(storagePutResetEvent.WaitOne(1000, true));
            Assert.IsTrue(subscribeResetEvent.WaitOne(1000, true));
            Assert.IsTrue(dekiUserResetEvent.WaitOne(1000, true));
            Assert.AreEqual(dekiAuth.At("current"), dekiCalledUri);
            Assert.AreEqual("wicked", storagePuts[0].Item1);
            Assert.AreEqual(1, storagePuts.Count);
            Assert.AreEqual(subscriptionLocation, subscribeCalledUri);
            Assert.AreEqual(2, subscribePosted["subscription"].ListLength);
            Assert.AreEqual("deki://wicked/pages/10#depth=infinity", subscribePosted["subscription[channel='event://wicked/deki/pages/create']/uri.resource"].AsText);

            // post a page deleted event
            _log.Debug("posting a page deleted event");
            dekiCalledUri = null;
            storagePutsExpected = 1;
            subscribeCalledUri = null;
            string channel = "event://wicked/deki/pages/delete";
            dekiPageResponse = new XDoc("page")
                .Elem("uri.ui", "http://foo.com/@api/deki/pages/10")
                .Elem("title", "foo")
                .Elem("path", "foo/bar");
            dekiFeedResponse = new XDoc("table")
                .Start("change")
                .Elem("rc_summary", "Two edits")
                .Elem("rc_comment", "edit 1")
                .Elem("rc_comment", "edit 2")
                .End();
            dekiSiteResponse = new XDoc("config")
                .Start("ui")
                    .Elem("sitename", "Test Site")
                    .Elem("language", "de-de")
                .End()
                .Start("page-subscription")
                    .Elem("from-address", "foo@test.com")
                .End();
            response = service.At("notify")
                .WithHeader(DreamHeaders.DREAM_EVENT_CHANNEL, channel)
                .WithHeader(DreamHeaders.DREAM_EVENT_RECIPIENT, "deki://wicked/user/1")
                .WithHeader(DreamHeaders.DREAM_EVENT_RECIPIENT, "deki://wicked/user/2")
                .PostAsync(CreateDekiEvent().Elem("channel", channel).Elem("pageid", 10))
                .Wait();
            Assert.IsTrue(response.IsSuccessful);

            // expect:
            // - email service is called for user
            // - storage is called with subscription (and user) removed
            // - subscription is pushed upstream
            Assert.IsTrue(emailResetEvent.WaitOne(1000, true));
            Assert.IsTrue(storageDeleteResetEvent.WaitOne(1000, true));
            Assert.IsTrue(subscribeResetEvent.WaitOne(1000, true));
            Assert.IsTrue(dekiSiteResetEvent.WaitOne(1000, true));
            Assert.IsNull(dekiCalledUri);
            Assert.AreEqual(1, emailPosted.Count);
            Assert.AreEqual("a@b.com", emailPosted[0]["to"].AsText);
            Assert.AreEqual(1, deleteCalled);
            Assert.AreEqual(subscriptionLocation, subscribeCalledUri);
            Assert.AreEqual(1, subscribePosted["subscription"].ListLength);
        }

        [Test]
        public void UserInfo_from_XDoc_and_back() {
            XDoc userDoc = new XDoc("user")
                .Attr("userid", 1)
                .Start("subscription.page").Attr("depth", "0").Attr("id", 1).End()
                .Start("subscription.page").Attr("depth", "0").Attr("id", 2).End()
                .Start("subscription.page").Attr("depth", "0").Attr("id", 3).End()
                .Start("subscription.page").Attr("depth", "infinity").Attr("id", 4).End();
            UserInfo userInfo = UserInfo.FromXDoc("wicked", userDoc);
            Assert.AreEqual(1, userInfo.Id);
            Assert.AreEqual("wicked", userInfo.WikiId);
            Assert.AreEqual(4, userInfo.Resources.Length);
            Assert.AreEqual(1, userInfo.Resources[0].Item1);
            Assert.AreEqual("0", userInfo.Resources[0].Item2);
            Assert.AreEqual(4, userInfo.Resources[3].Item1);
            Assert.AreEqual("infinity", userInfo.Resources[3].Item2);
            XDoc userDoc2 = userInfo.AsDocument;
            XDoc expected = new XDoc("user")
                .Attr("userid", 1)
                .Start("subscription.page").Attr("depth", "0").Attr("id", 1).End()
                .Start("subscription.page").Attr("depth", "0").Attr("id", 2).End()
                .Start("subscription.page").Attr("depth", "0").Attr("id", 3).End()
                .Start("subscription.page").Attr("depth", "infinity").Attr("id", 4).End();
            Assert.AreEqual(expected, userDoc2);
        }

        private XDoc CreateDekiEvent() {
            return new XDoc("deki-event").Attr("wikiid", "wicked").Attr("event-time", DateTime.Parse("2009/01/01 12:00:00"));
        }
    }
}