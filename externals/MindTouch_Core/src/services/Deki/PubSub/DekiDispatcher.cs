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
using MindTouch.Dream;
using MindTouch.Dream.Services.PubSub;
using MindTouch.Tasking;
using MindTouch.Xml;

namespace MindTouch.Deki.PubSub {
    using Yield = IEnumerator<IYield>;

    public class DekiDispatcher : Dispatcher {

        //--- Class Fields ---
        private static readonly log4net.ILog _log = LogUtils.CreateLog();

        //--- Fields ---
        private readonly Plug _deki;
        private readonly string _authtoken;
        private Dictionary<uint, List<Tuplet<PubSubSubscription, bool>>> _subscriptionsByPage = new Dictionary<uint, List<Tuplet<PubSubSubscription, bool>>>();

        //--- Constructors ---
        public DekiDispatcher(DispatcherConfig config)
            : base(config) {
            _authtoken = config.ServiceConfig["authtoken"].AsText;
            var dekiUri = config.ServiceConfig["uri.deki"].AsUri;
            if(config.ServiceCookies != null) {
                _cookieJar.Update(config.ServiceCookies.Fetch(dekiUri), null);
            }
            _deki = Plug.New(dekiUri).WithCookieJar(_cookieJar);
        }

        //--- Methods ---
        protected override PubSubSubscription[] CalculateCombinedSubscriptions() {
            PubSubSubscription[] allSubs = base.CalculateCombinedSubscriptions();
            var tempSubLookup = new Dictionary<uint, List<Tuplet<PubSubSubscription, bool>>>();
            foreach(PubSubSubscription sub in allSubs) {

                // this is only for page subs, so we check the channels
                bool hasPageChannel = false;
                foreach(XUri channel in sub.Channels) {
                    if(channel.Segments.Length > 1 && StringUtil.EqualsInvariantIgnoreCase(channel.Segments[1], "pages")) {
                        hasPageChannel = true;
                    }
                }
                if(hasPageChannel) {
                    foreach(XUri resource in sub.Resources) {
                        bool infiniteDepth = (resource.Fragment != null && resource.Fragment.Contains("depth=infinity"));

                        // we treat page subscriptions special, so we can assume that we can pull the page id off them
                        // the expect page resource uri looks like deki://{wikiid}/pages/{pageid}
                        uint pageId;
                        if(resource.Segments.Length != 2 || !StringUtil.EqualsInvariantIgnoreCase(resource.Segments[0], "pages") || !uint.TryParse(resource.Segments[1], out pageId)) {
                            continue;
                        }
                        List<Tuplet<PubSubSubscription, bool>> subs;
                        if(!tempSubLookup.TryGetValue(pageId, out subs)) {
                            subs = new List<Tuplet<PubSubSubscription, bool>>();
                            tempSubLookup.Add(pageId, subs);
                        }
                        subs.Add(new Tuplet<PubSubSubscription, bool>(sub, infiniteDepth));
                    }
                }
            }
            _subscriptionsByPage = tempSubLookup;
            return allSubs;
        }

        protected override Yield GetListenersByChannelResourceMatch(DispatcherEvent ev, Result<Dictionary<XUri, List<DispatcherRecipient>>> result) {
            if(!StringUtil.EqualsInvariantIgnoreCase(ev.Channel.Segments[1], "pages")) {

                // not a page DispatcherEvent or a page delete DispatcherEvent, use default matcher
                Result<Dictionary<XUri, List<DispatcherRecipient>>> baseResult;
                yield return baseResult = Coroutine.Invoke(base.GetListenersByChannelResourceMatch, ev, new Result<Dictionary<XUri, List<DispatcherRecipient>>>());
                result.Return(baseResult);
                yield break;
            }
            List<PubSubSubscription> matches = new List<PubSubSubscription>();
            if(ev.Channel.Segments.Length <= 2 || !StringUtil.EqualsInvariantIgnoreCase(ev.Channel.Segments[2], "delete")) {

                // dispatch to all PubSubSubscriptions that listen for this DispatcherEvent and its contents
                XDoc evDoc = ev.AsDocument();
                uint? pageid = evDoc["pageid"].AsUInt;
                string wikiId = evDoc["@wikiid"].AsText;
                bool first = true;
                _log.DebugFormat("trying dispatch based on channel & page PubSubSubscriptions for page '{0}' from wiki '{1}'", pageid, wikiId);

                // fetch parent page id's for this page so that we can resolve infinite depth PubSubSubscriptions
                Result<DreamMessage> pageHierarchyResult;
                yield return pageHierarchyResult = _deki.At("pages", pageid.ToString()).WithHeader("X-Deki-Site", "id=" + wikiId).GetAsync();
                DreamMessage pageHierarchy = pageHierarchyResult.Value;
                if(pageHierarchy.IsSuccessful) {
                    XDoc pageDoc = pageHierarchy.ToDocument();
                    while(pageid.HasValue) {
                        List<Tuplet<PubSubSubscription, bool>> subs;
                        _subscriptionsByPage.TryGetValue(pageid.Value, out subs);
                        if(subs != null) {

                            // only the first pageId (the one from the event) triggers on non-infinite depth subs
                            foreach(var sub in subs) {
                                if((sub.Item2 || first) && !matches.Contains(sub.Item1)) {
                                    matches.Add(sub.Item1);
                                }
                            }
                        }

                        // get parent id and then set pageDoc to the parent's subdoc, so we can descend the ancesstor tree further
                        pageid = pageDoc["page.parent/@id"].AsUInt;
                        pageDoc = pageDoc["page.parent"];
                        first = false;
                    }
                } else {
                    _log.WarnFormat("unable to retrieve page doc for page '{0}': {1}", pageid, pageHierarchy.Status);
                }
            }
            ICollection<PubSubSubscription> listeningPubSubSubscriptions;
            lock(_channelMap) {

                // get all the PubSubSubscriptions that are wild card matches (which is basically those that didn't
                // have any resources in their PubSubSubscription) and add them to the above matches
                foreach(PubSubSubscription sub in _resourceMap.GetMatches(new XUri("http://dummy/dummy"))) {
                    if(!matches.Contains(sub)) {
                        matches.Add(sub);
                    }
                }
                listeningPubSubSubscriptions = _channelMap.GetMatches(ev.Channel, matches);
            }

            // build a lookup of destinations and their recipients
            Dictionary<XUri, Dictionary<DispatcherRecipient, object>> listenersTemp = new Dictionary<XUri, Dictionary<DispatcherRecipient, object>>();
            foreach(PubSubSubscription sub in listeningPubSubSubscriptions) {
                Dictionary<DispatcherRecipient, object> recipients;
                if(!listenersTemp.TryGetValue(sub.Destination, out recipients)) {
                    recipients = new Dictionary<DispatcherRecipient, object>();
                    listenersTemp.Add(sub.Destination, recipients);
                }
                foreach(DispatcherRecipient recipient in sub.Recipients) {
                    recipients[recipient] = null;
                }
            }

            // collapse the lookup used to build the unique recipient list to a simple list of recipient list
            Dictionary<XUri, List<DispatcherRecipient>> listeners = new Dictionary<XUri, List<DispatcherRecipient>>();
            foreach(KeyValuePair<XUri, Dictionary<DispatcherRecipient, object>> kvp in listenersTemp) {
                List<DispatcherRecipient> recipients = new List<DispatcherRecipient>(kvp.Value.Count);
                recipients.AddRange(kvp.Value.Keys);
                listeners.Add(kvp.Key, recipients);
            }
            result.Return(listeners);
            yield break;
        }

        protected override Yield DetermineRecipients(DispatcherEvent ev, DispatcherRecipient[] recipients, Result<DispatcherEvent> result) {
            List<DispatcherRecipient> recipients2 = new List<DispatcherRecipient>();

            Dictionary<int, DispatcherRecipient> userIds = new Dictionary<int, DispatcherRecipient>();
            foreach(DispatcherRecipient r in recipients) {
                string authtoken = r.Doc["@authtoken"].AsText;
                if(string.IsNullOrEmpty(authtoken)) {

                    // if the reciepient has no authtoken, but has a userid, collect the Id so we can authorize it against the page
                    int? userId = r.Doc["@userid"].AsInt;
                    if(userId.HasValue) {
                        userIds.Add(userId.Value, r);
                    }
                } else if(authtoken == _authtoken) {

                    // authtoken means the recipient doesn't need page level authorization (such as lucene)
                    recipients2.Add(r);
                }
            }
            if(userIds.Count > 0 && (ev.Channel.Segments.Length <= 2 || !StringUtil.EqualsInvariantIgnoreCase(ev.Channel.Segments[2], "delete"))) {

                // check all userId's against the page to prune set to authorized users
                XDoc users = new XDoc("users");
                foreach(int userid in userIds.Keys) {
                    users.Start("user").Attr("id", userid).End();
                }
                XDoc changeDoc = ev.AsDocument();
                uint? pageid = changeDoc["pageid"].AsUInt;
                string wikiId = changeDoc["@wikiid"].AsText;
                if(pageid.HasValue) {
                    Result<DreamMessage> userAuthResult;
                    yield return userAuthResult = _deki.At("pages", pageid.Value.ToString(), "allowed")
                        .With("permissions", "read,subscribe")
                        .With("filterdisabled", true)
                        .WithHeader("X-Deki-Site", "id=" + wikiId)
                        .PostAsync(users);
                    DreamMessage userAuth = userAuthResult.Value;
                    if(userAuth.IsSuccessful) {
                        int authorized = 0;
                        foreach(XDoc userid in userAuth.ToDocument()["user/@id"]) {
                            DispatcherRecipient recipient;
                            if(!userIds.TryGetValue(userid.AsInt.GetValueOrDefault(), out recipient)) {
                                continue;
                            }
                            authorized++;
                            recipients2.Add(recipient);
                        }
                        if(authorized != userIds.Count) {
                            _log.DebugFormat("requested auth on {0} users, received auth on {1} for page {2}", userIds.Count, authorized, pageid.Value);
                        }
                    } else {
                        _log.WarnFormat("unable to retrieve user auth for page '{0}': {1}", pageid, userAuth.Status);
                    }
                }
            }
            result.Return(recipients2.Count == 0 ? null : ev.WithRecipient(true, recipients2.ToArray()));
            yield break;
        }
    }
}
