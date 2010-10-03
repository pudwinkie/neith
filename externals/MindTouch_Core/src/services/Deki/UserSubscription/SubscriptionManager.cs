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
using MindTouch.Collections;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Xml;

namespace MindTouch.Deki.UserSubscription {

    public class RecordEventArgs : EventArgs {

        //--- Fields ---
        public readonly string WikiId;
        public readonly UserInfo User;
        public readonly bool Delete;

        //--- Constructors ---
        public RecordEventArgs(string wikiId, UserInfo user, bool delete) {
            WikiId = wikiId;
            User = user;
            Delete = delete;
        }
    }

    public class SubscriptionEventArgs : EventArgs {

        //--- Fields ---
        public readonly XDoc[] Subscriptions;

        //--- Constructors ---
        public SubscriptionEventArgs(XDoc[] subscriptions) {
            Subscriptions = subscriptions;
        }
    }

    public class SubscriptionManager {

        //--- Class Fields ---
        private static readonly log4net.ILog _log = LogUtils.CreateLog();

        //--- Fields ---
        private readonly Dictionary<string, SiteInfo> _subscriptions = new Dictionary<string, SiteInfo>();
        private readonly XUri _destination;
        private readonly ProcessingQueue<UserInfo> _recordChangeQueue;
        private readonly ProcessingQueue<Empty> _subscriptionChangeQueue;

        //--- Constructor ---
        public SubscriptionManager(XUri destination, List<Tuplet<string, List<XDoc>>> subscriptions) {
            _destination = destination;
            _recordChangeQueue = new ProcessingQueue<UserInfo>(RecordsChange_Helper, 1);
            _subscriptionChangeQueue = new ProcessingQueue<Empty>(UpdateSubscriptions_Helper, 1);
            if(subscriptions == null) {
                return;
            }
            foreach(Tuplet<string, List<XDoc>> subscription in subscriptions) {
                string wikiId = subscription.Item1;
                SiteInfo siteInfo = new SiteInfo(wikiId);
                _subscriptions.Add(wikiId, siteInfo);
                foreach(XDoc userDoc in subscription.Item2) {
                    UserInfo userInfo = UserInfo.FromXDoc(wikiId, userDoc);
                    if(userInfo == null) {
                        continue;
                    }
                    lock(siteInfo) {
                        siteInfo.Users.Add(userInfo.Id, userInfo);
                    }
                    userInfo.ResourcesChanged += OnSubscriptionChange;
                    userInfo.DataChanged += OnRecordsChange;
                }
            }
        }

        //--- Properties ---
        public IEnumerable<XDoc> Subscriptions {
            get {
                return CalculateSubscriptions();
            }
        }

        //--- Methods ---
        public UserInfo GetUser(string wikiId, uint userId, bool create) {
            SiteInfo siteInfo;
            UserInfo userInfo;
            lock(_subscriptions) {
                if(_subscriptions.TryGetValue(wikiId, out siteInfo)) {
                    lock(siteInfo) {
                        if(siteInfo.Users.TryGetValue(userId, out userInfo)) {
                            return userInfo;
                        }
                    }
                }
                if(!create) {
                    return null;
                }
                if(siteInfo == null) {
                    siteInfo = new SiteInfo(wikiId);
                    _subscriptions.Add(wikiId, siteInfo);
                }
                userInfo = new UserInfo(userId, wikiId);
                lock(siteInfo) {
                    siteInfo.Users.Add(userId, userInfo);
                }
                userInfo.ResourcesChanged += OnSubscriptionChange;
                userInfo.DataChanged += OnRecordsChange;
                return userInfo;
            }
        }

        private void RecordsChange_Helper(UserInfo userInfo) {
            SiteInfo siteInfo;
            lock(_subscriptions) {
                if(!_subscriptions.TryGetValue(userInfo.WikiId, out siteInfo)) {
                    return;
                }
            }
            bool delete = false;
            lock(siteInfo) {
                if(userInfo.Resources.Length == 0 && siteInfo.Users.ContainsKey(userInfo.Id)) {

                    // user has no subscribed resources, remove
                    _log.DebugFormat("purging user '{0}', no subscriptions left", userInfo.Id);
                    siteInfo.Users.Remove(userInfo.Id);
                    delete = true;
                    userInfo.ResourcesChanged -= OnSubscriptionChange;
                    userInfo.DataChanged -= OnRecordsChange;
                }
            }
            if(RecordsChanged != null) {
                _log.Debug("firing RecordsChanged");
                RecordsChanged(this, new RecordEventArgs(userInfo.WikiId, userInfo, delete));
            }
        }

        private void UpdateSubscriptions_Helper(Empty unused) {
            List<XDoc> subscriptions = CalculateSubscriptions();
            if(SubscriptionsChanged != null) {
                _log.Debug("firing SubscriptionsChanged");
                SubscriptionsChanged(this, new SubscriptionEventArgs(subscriptions.ToArray()));
            }
        }

        private List<XDoc> CalculateSubscriptions() {
            var subscriptions = new List<XDoc>();
            var wikiis = new List<Tuplet<string, SiteInfo>>();
            lock(_subscriptions) {
                foreach(KeyValuePair<string, SiteInfo> wiki in _subscriptions) {
                    wikiis.Add(new Tuplet<string, SiteInfo>(wiki.Key, wiki.Value));
                }
            }
            int pageSubscriptions = 0;
            int subscribedUsers = 0;
            foreach(Tuplet<string, SiteInfo> wiki in wikiis) {
                var subscriptionLookup = new Dictionary<uint, Tuplet<string, List<uint>>>();
                var key = wiki.Item1;
                var siteInfo = wiki.Item2;
                lock(siteInfo) {
                    foreach(UserInfo info in siteInfo.Users.Values) {
                        subscribedUsers++;
                        foreach(Tuplet<uint, string> resource in info.Resources) {
                            Tuplet<string, List<uint>> subs;
                            if(!subscriptionLookup.TryGetValue(resource.Item1, out subs)) {
                                subs = new Tuplet<string, List<uint>>(resource.Item2, new List<uint>());
                                subscriptionLookup.Add(resource.Item1, subs);
                            } else if(resource.Item2 == "infinity") {
                                subs.Item1 = resource.Item2;
                            }
                            subs.Item2.Add(info.Id);
                        }
                    }
                }
                foreach(KeyValuePair<uint, Tuplet<string, List<uint>>> kvp in subscriptionLookup) {
                    XDoc subscription = new XDoc("subscription")
                        .Elem("channel", string.Format("event://{0}/deki/pages/create", key))
                        .Elem("channel", string.Format("event://{0}/deki/pages/update", key))
                        .Elem("channel", string.Format("event://{0}/deki/pages/delete", key))
                        .Elem("channel", string.Format("event://{0}/deki/pages/revert", key))
                        .Elem("channel", string.Format("event://{0}/deki/pages/move", key))
                        .Elem("channel", string.Format("event://{0}/deki/pages/tags/update", key))
                        .Elem("channel", string.Format("event://{0}/deki/pages/dependentschanged/comments/create", key))
                        .Elem("channel", string.Format("event://{0}/deki/pages/dependentschanged/comments/update", key))
                        .Elem("channel", string.Format("event://{0}/deki/pages/dependentschanged/comments/delete", key))
                        .Elem("channel", string.Format("event://{0}/deki/pages/dependentschanged/files/create", key))
                        .Elem("channel", string.Format("event://{0}/deki/pages/dependentschanged/files/update", key))
                        .Elem("channel", string.Format("event://{0}/deki/pages/dependentschanged/files/delete", key))
                        .Elem("channel", string.Format("event://{0}/deki/pages/dependentschanged/files/properties/*", key))
                        .Elem("channel", string.Format("event://{0}/deki/pages/dependentschanged/files/restore", key))
                        .Elem("uri.resource", string.Format("deki://{0}/pages/{1}#depth={2}", key, kvp.Key, kvp.Value.Item1))
                        .Elem("uri.proxy", _destination);
                    pageSubscriptions++;
                    foreach(int userId in kvp.Value.Item2) {
                        subscription
                            .Start("recipient")
                                .Attr("userid", userId)
                                .Elem("uri", string.Format("deki://{0}/users/{1}", key, userId))
                            .End();
                    }
                    subscriptions.Add(subscription);
                }
            }
            _log.DebugFormat("calculated subscription set with {0} page subscriptions for {1} users", pageSubscriptions, subscribedUsers);
            return subscriptions;
        }

        //--- Event Handlers ---
        private void OnSubscriptionChange(object sender, EventArgs e) {
            if(SubscriptionsChanged == null) {

                // no one cares about the subscription set, don't bother calculating it 
                return;
            }

            // TODO (arnec): this should be a timed queue that accumulates updates instead of firing on every change
            if(!_subscriptionChangeQueue.TryEnqueue(null)) {
                throw new ShouldNeverHappenException("Enqueue operations failed.");
            }
        }

        private void OnRecordsChange(object sender, EventArgs e) {
            if(RecordsChanged == null) {

                // no serializer listening, so we don't need to bother calculating the record set
                return;
            }
            UserInfo userInfo = sender as UserInfo;
            if(!_recordChangeQueue.TryEnqueue(userInfo)) {
                throw new ShouldNeverHappenException("Enqueue operations failed.");
            }
        }

        //--- Events ---
        public event EventHandler<RecordEventArgs> RecordsChanged;
        public event EventHandler<SubscriptionEventArgs> SubscriptionsChanged;

        public SiteInfo GetSiteInfo(string wikiId) {
            SiteInfo siteInfo;
            _subscriptions.TryGetValue(wikiId, out siteInfo);
            return siteInfo;
        }
    }
}
