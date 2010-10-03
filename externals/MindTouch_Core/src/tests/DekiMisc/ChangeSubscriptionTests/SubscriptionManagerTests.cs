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

using System.Collections.Generic;
using System.Threading;

using MindTouch.Deki.UserSubscription;
using MindTouch.Dream;
using MindTouch.Xml;

using NUnit.Framework;

namespace MindTouch.Deki.Tests.ChangeSubscriptionTests {

    [TestFixture]
    public class SubscriptionManagerTests {

        private static readonly log4net.ILog _log = LogUtils.CreateLog();
        
        [Test]
        public void SubscriptionManager_with_initial_subscriptions() {
            List<Tuplet<string, List<XDoc>>> subs = new List<Tuplet<string, List<XDoc>>>();
            List<XDoc> x = new List<XDoc>();
            Tuplet<string, List<XDoc>> xSubs = new Tuplet<string, List<XDoc>>("x", x);
            subs.Add(xSubs);
            x.Add(new XDoc("user")
                      .Attr("userid", 1)
                      .Elem("email", "foo")
                      .Start("subscription.page").Attr("id", 1).Attr("depth", 0).End());
            x.Add(new XDoc("user")
                      .Attr("userid", 2)
                      .Elem("email", "foo")
                      .Start("subscription.page").Attr("id", 1).Attr("depth", 0).End()
                      .Start("subscription.page").Attr("id", 2).Attr("depth", 0).End());
            x.Add(new XDoc("user")
                      .Attr("userid", 3)
                      .Elem("email", "foo")
                      .Start("subscription.page").Attr("id", 2).Attr("depth", 0).End());
            List<XDoc> y = new List<XDoc>();
            Tuplet<string, List<XDoc>> ySubs = new Tuplet<string, List<XDoc>>("y", y);
            subs.Add(ySubs);
            y.Add(new XDoc("user")
                      .Attr("userid", 10)
                      .Elem("email", "foo")
                      .Start("subscription.page").Attr("id", 1).Attr("depth", 0).End());
            SubscriptionManager subscriptionManager = new SubscriptionManager(new XUri("test://"), subs);
            List<XDoc> subscriptions = new List<XDoc>(subscriptionManager.Subscriptions);
            Assert.AreEqual(3, subscriptions.Count);
            bool foundXa = false;
            bool foundXb = false;
            bool foundYa = false;
            foreach(XDoc sub in subscriptions) {
                switch(sub["channel"].AsText) {
                case "event://x/deki/pages/create":
                    switch(sub["uri.resource"].AsText) {
                    case "deki://x/pages/1#depth=0":
                        Assert.AreEqual(2, sub["recipient"].ListLength);
                        foundXa = true;
                        break;
                    case "deki://x/pages/2#depth=0":
                        Assert.AreEqual(2, sub["recipient"].ListLength);
                        foundXb = true;
                        break;
                    default:
                        Assert.Fail("bad resource for deki X");
                        break;
                    }
                    break;
                case "event://y/deki/pages/create":
                    if(sub["uri.resource"].AsText != "deki://y/pages/1#depth=0") {
                        Assert.Fail("bad resource for deki Y");
                    }
                    XDoc recipient = sub["recipient"];
                    Assert.AreEqual(1, recipient.ListLength);
                    Assert.AreEqual("10", recipient["@userid"].AsText);
                    foundYa = true;
                    break;
                }
            }
            Assert.IsTrue(foundXa);
            Assert.IsTrue(foundXb);
            Assert.IsTrue(foundYa);
            Assert.IsNotNull(subscriptionManager.GetUser("x", 1, false));
            Assert.IsNotNull(subscriptionManager.GetUser("x", 2, false));
            Assert.IsNotNull(subscriptionManager.GetUser("x", 3, false));
            Assert.IsNull(subscriptionManager.GetUser("x", 10, false));
            Assert.IsNotNull(subscriptionManager.GetUser("y", 10, false));
        }

        [Test]
        public void SubscriptionManager_collapses_subscriptions_to_favor_infinite_depth() {
            SubscriptionManager subscriptionManager = new SubscriptionManager(null, null);
            UserInfo userInfo = subscriptionManager.GetUser("a", 1, true);
            userInfo.AddResource(1, "0");
            userInfo.AddResource(1, "infinity");
            List<XDoc> subscriptions = new List<XDoc>(subscriptionManager.Subscriptions);
            Assert.AreEqual(1, subscriptions.Count);
            Assert.AreEqual("deki://a/pages/1#depth=infinity", subscriptions[0]["uri.resource"].AsText);
        }

        [Test]
        public void SubscriptionManager_user_subscription_management() {
            var subscriptionManager = new SubscriptionManager(null, null);
            var recordEvents = new List<RecordEventArgs>();
            var recordsEvent = new ManualResetEvent(false);
            subscriptionManager.RecordsChanged += delegate(object sender, RecordEventArgs e) {
                recordEvents.Add(e);
                recordsEvent.Set();
            };
            SubscriptionEventArgs subscriptionEventArgs = null;
            var subscriptionsFired = 0;
            var subscriptionsEvent = new ManualResetEvent(false);
            subscriptionManager.SubscriptionsChanged += delegate(object sender, SubscriptionEventArgs e) {
                subscriptionEventArgs = e;
                subscriptionsFired++;
                subscriptionsEvent.Set();
            };
            _log.Debug("adding resource 1 to user 1");
            recordsEvent.Reset();
            subscriptionsEvent.Reset();
            UserInfo userInfo1 = subscriptionManager.GetUser("a", 1, true);
            userInfo1.AddResource(1, "0");
            userInfo1.Save();
            _log.Debug("waiting on events");
            Assert.IsTrue(recordsEvent.WaitOne(2000, true));
            Assert.IsTrue(subscriptionsEvent.WaitOne(2000, true));
            Assert.IsNotNull(subscriptionManager.GetUser("a", 1, false));
            Assert.AreEqual(1, recordEvents.Count);
            Assert.AreEqual(1, subscriptionsFired);
            Assert.AreEqual("a", recordEvents[0].WikiId);
            Assert.AreEqual(1, recordEvents[0].User.Id);
            Assert.AreEqual(1, subscriptionEventArgs.Subscriptions.Length);
            Assert.AreEqual("deki://a/pages/1#depth=0", subscriptionEventArgs.Subscriptions[0]["uri.resource"].AsText);
            _log.Debug("adding resource 1 to user 2");
            recordsEvent.Reset();
            subscriptionsEvent.Reset();
            UserInfo userInfo2 = subscriptionManager.GetUser("a", 2, true);
            userInfo2.AddResource(1, "0");
            userInfo2.Save();
            _log.Debug("waiting on events");
            Assert.IsTrue(recordsEvent.WaitOne(2000, true));
            Assert.IsTrue(subscriptionsEvent.WaitOne(2000, true));
            Assert.IsNotNull(subscriptionManager.GetUser("a", 2, false));
            Assert.AreEqual(2, recordEvents.Count);
            Assert.AreEqual(2, subscriptionsFired);
            Assert.AreEqual("a", recordEvents[1].WikiId);
            Assert.AreEqual(2, recordEvents[1].User.Id);
            Assert.AreEqual(1, subscriptionEventArgs.Subscriptions.Length);
            Assert.AreEqual("deki://a/pages/1#depth=0", subscriptionEventArgs.Subscriptions[0]["uri.resource"].AsText);
            Assert.AreEqual(2, subscriptionEventArgs.Subscriptions[0]["recipient"].ListLength);
            _log.Debug("adding resource 2 to user 2");
            recordsEvent.Reset();
            subscriptionsEvent.Reset();
            UserInfo userInfo2a = subscriptionManager.GetUser("a", 2, false);
            userInfo2a.AddResource(2, "0");
            userInfo2a.Save();
            _log.Debug("waiting on events");
            Assert.IsTrue(recordsEvent.WaitOne(2000, true));
            Assert.IsTrue(subscriptionsEvent.WaitOne(2000, true));
            UserInfo userInfo = subscriptionManager.GetUser("a", 2, false);
            Assert.AreEqual(2, userInfo.Resources.Length);
            Assert.AreEqual(3, recordEvents.Count);
            Assert.AreEqual(3, subscriptionsFired);
            Assert.AreEqual("a", recordEvents[2].WikiId);
            Assert.AreEqual(2, recordEvents[2].User.Id);
            Assert.AreEqual(2, subscriptionEventArgs.Subscriptions.Length);
            Assert.AreEqual("deki://a/pages/2#depth=0", subscriptionEventArgs.Subscriptions[1]["uri.resource"].AsText);
            Assert.AreEqual(1, subscriptionEventArgs.Subscriptions[1]["recipient"].ListLength);
            _log.Debug("removing resource 1 from user 1");
            recordsEvent.Reset();
            subscriptionsEvent.Reset();
            UserInfo userInfo1a = subscriptionManager.GetUser("a", 1, false);
            userInfo1a.RemoveResource(1);
            userInfo1a.Save();
            _log.Debug("waiting on events");
            Assert.IsTrue(recordsEvent.WaitOne(2000, true));
            Assert.IsTrue(subscriptionsEvent.WaitOne(2000, true));
            Assert.IsNull(subscriptionManager.GetUser("a", 1, false));
            Assert.AreEqual(4, subscriptionsFired);
            Assert.AreEqual(4, recordEvents.Count);
            Assert.AreEqual("a", recordEvents[3].WikiId);
            Assert.AreEqual(1, recordEvents[3].User.Id);
            Assert.AreEqual(2, subscriptionEventArgs.Subscriptions.Length);
            Assert.AreEqual("deki://a/pages/1#depth=0", subscriptionEventArgs.Subscriptions[0]["uri.resource"].AsText);
            Assert.AreEqual(1, subscriptionEventArgs.Subscriptions[0]["recipient"].ListLength);
        }
    }
}
