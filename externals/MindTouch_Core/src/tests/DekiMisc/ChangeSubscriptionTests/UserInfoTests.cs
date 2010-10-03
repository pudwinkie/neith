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
using MindTouch.Xml;

using NUnit.Framework;

namespace MindTouch.Deki.Tests.ChangeSubscriptionTests {
    
    [TestFixture]
    public class UserInfoTests {

        [Test]
        public void UserInfo_add_remove_resources() {
            UserInfo userInfo = new UserInfo(1, "wicked");
            int resourcesChanged = 0;
            userInfo.ResourcesChanged += delegate { resourcesChanged++; };
            userInfo.AddResource(1, "0");
            Assert.AreEqual(1, resourcesChanged);
            Assert.AreEqual(1, userInfo.Resources.Length);
            Assert.AreEqual(1, userInfo.Resources[0].Item1);
            userInfo.AddResource(2, "0");
            userInfo.AddResource(3, "0");
            userInfo.AddResource(4, "0");
            Assert.AreEqual(4, resourcesChanged);
            Assert.AreEqual(4, userInfo.Resources.Length);
            List<uint> resources = new List<uint>();
            foreach(Tuplet<uint, string> tuple in userInfo.Resources) {
                resources.Add(tuple.Item1);
            }
            Assert.IsTrue(resources.Contains(2));
            userInfo.RemoveResource(2);
            Assert.AreEqual(5, resourcesChanged);
            Assert.AreEqual(3, userInfo.Resources.Length);
            resources.Clear();
            foreach(Tuplet<uint, string> tuple in userInfo.Resources) {
                resources.Add(tuple.Item1);
            }
            Assert.IsFalse(resources.Contains(2));
        }

        [Test]
        public void UserInfo_self_invalidates_on_expire_time() {
            TimeSpan defaultExpire = UserInfo.ValidationExpireTime;
            try {
                UserInfo.ValidationExpireTime = TimeSpan.FromMilliseconds(200);
                UserInfo userInfo = new UserInfo(1, "wicked");
                Assert.IsFalse(userInfo.IsValidated);
                userInfo.Save();
                Assert.IsTrue(userInfo.IsValidated);
                Thread.Sleep(1000);
                Assert.IsFalse(userInfo.IsValidated);
            } finally {
                UserInfo.ValidationExpireTime = defaultExpire;
            }
        }

        [Test]
        public void UserInfo_retrieve_subscription_for_a_page() {
            SubscriptionManager subscriptionManager = new SubscriptionManager(null, null);
            UserInfo userInfo1 = subscriptionManager.GetUser("a", 1, true);
            userInfo1.AddResource(10, "infinity");
            userInfo1.AddResource(11, "0");
            userInfo1.AddResource(12, "0");
            userInfo1.AddResource(13, "infinity");

            List<uint> pages = new List<uint>();
            pages.Add(10);
            pages.Add(12);
            pages.Add(14);
            pages.Add(16);
            XDoc subscriptions = userInfo1.GetSubscriptionDoc(pages);
            Assert.AreEqual(2, subscriptions["subscription.page"].ListLength);
            XDoc page10 = subscriptions["subscription.page[@id='10']"];
            Assert.IsFalse(page10.IsEmpty);
            Assert.AreEqual("infinity", page10["@depth"].AsText);
            XDoc page12 = subscriptions["subscription.page[@id='12']"];
            Assert.IsFalse(page12.IsEmpty);
            Assert.AreEqual("0", page12["@depth"].AsText);
        }
    }
}
