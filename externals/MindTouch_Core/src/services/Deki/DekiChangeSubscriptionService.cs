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
using System.Globalization;
using System.IO;
using System.Text;

using log4net;
using MindTouch.Deki.UserSubscription;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Web;
using MindTouch.Xml;

namespace MindTouch.Deki {
    using Yield = IEnumerator<IYield>;

    [DreamService("MindTouch Change Subscription Service", "Copyright (c) 2006-2010 MindTouch Inc.",
       Info = "http://developer.mindtouch.com/Deki/Services/DekiChangeSubscription",
       SID = new[] { "sid://mindtouch.com/deki/2008/11/changesubscription" }
    )]
    public class DekiChangeSubscriptionService : DreamService {

        //--- Types ---
        private class UserException : Exception {

            //--- Constructors ---
            public UserException(string message) : base(message) { }
        }

        //--- Class Fields ---
        private static readonly ILog _log = LogUtils.CreateLog();

        //--- Fields ---
        private Plug _emailer;
        private Plug _deki;
        private Plug _subscriptionLocation;
        private SubscriptionManager _subscriptions;
        private XDoc _baseSubscriptionSet;
        private string _apikey;
        private PlainTextResourceManager _resourceManager;
        private readonly Dictionary<string, string> _wikiLanguageCache = new Dictionary<string, string>();
        private NotificationDelayQueue _notificationQueue;
        private PageChangeCache _cache;

        //--- Features ---
        [DreamFeature("POST:pages/{pageid}", "Subscribe to a resource")]
        [DreamFeatureParam("depth", "string?", "0 for specific page, 'infinity' for sub-tree subscription. Defaults to 0")]
        public Yield SubscribeToChange(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            uint pageId = context.GetParam<uint>("pageid");
            string depth = context.GetParam("depth", "0");
            Result<UserInfo> userResult;
            yield return userResult = Coroutine.Invoke(GetUserInfo, true, request, new Result<UserInfo>()).Catch();
            if(userResult.HasException) {
                ReturnUserError(userResult.Exception, response);
                yield break;
            }
            UserInfo userInfo = userResult.Value;
            DreamMessage pageAuth = null;
            yield return _deki
                .At("pages", pageId.ToString(), "allowed")
                .With("permissions", "read,subscribe")
                .WithHeaders(request.Headers)
                .Post(new XDoc("users").Start("user").Attr("id", userInfo.Id).End(), new Result<DreamMessage>())
                .Set(x => pageAuth = x);
            if(!pageAuth.IsSuccessful || pageAuth.ToDocument()["user/@id"].AsText != userInfo.Id.ToString()) {
                throw new DreamForbiddenException("User not permitted to subscribe to page");
            }
            userInfo.AddResource(pageId, depth);
            userInfo.Save();
            response.Return(DreamMessage.Ok());
            yield break;
        }

        [DreamFeature("DELETE:pages/{pageid}", "Unsubscribe from a resource")]
        public Yield UnsubscribeFromChange(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            uint pageId = context.GetParam<uint>("pageid");
            Result<UserInfo> userResult;
            yield return userResult = Coroutine.Invoke(GetUserInfo, false, request, new Result<UserInfo>()).Catch();
            if(userResult.HasException) {
                ReturnUserError(userResult.Exception, response);
                yield break;
            }
            UserInfo userInfo = userResult.Value;
            if(userInfo != null) {
                userInfo.RemoveResource(pageId);
                userInfo.Save();
            }
            response.Return(DreamMessage.Ok());
            yield break;
        }

        [DreamFeature("GET:subscriptions", "Retrieve page subscriptions for the current user")]
        [DreamFeatureParam("pages", "string?", "A comma separated list of the pages to check. If omitted, returns all subscriptions")]
        public Yield GetSubscriptions(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            Result<UserInfo> userResult;
            yield return userResult = Coroutine.Invoke(GetUserInfo, false, request, new Result<UserInfo>()).Catch();
            if(userResult.HasException) {
                ReturnUserError(userResult.Exception, response);
                yield break;
            }
            UserInfo userInfo = userResult.Value;
            if(userInfo == null) {

                // if userInfo is null, we need to return an empty doc
                response.Return(DreamMessage.Ok(new XDoc("subscriptions")));
                yield break;
            }
            List<uint> pages = new List<uint>();
            string pageList = context.GetParam("pages", "");
            int subscribedPages = 0;
            if(!string.IsNullOrEmpty(pageList)) {
                foreach(string pageId in pageList.Split(',')) {
                    uint id;
                    if(uint.TryParse(pageId, out id)) {
                        subscribedPages++;
                        pages.Add(id);
                    }
                }
            }
            _log.DebugFormat("found {0} subscribed pages for request hierarchy", subscribedPages);
            response.Return(DreamMessage.Ok(userInfo.GetSubscriptionDoc(pages)));
            yield break;
        }

        [DreamFeature("POST:updateuser", "Update user (user pubsub endpoint)")]
        internal Yield UpdateUser(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            XDoc ev = request.ToDocument();
            XUri channel = ev["channel"].AsUri;
            string action = channel.Segments[2];

            uint? userId = ev["userid"].AsUInt;
            if(userId.HasValue) {
                UserInfo userInfo = _subscriptions.GetUser(ev["@wikiid"].AsText, userId.Value, false);
                if(userInfo == null) {
                    response.Return(DreamMessage.Ok());
                    yield break;
                }
                if(StringUtil.EqualsInvariantIgnoreCase(action, "delete")) {

                    // user deletion event, wipe all subscriptions
                    foreach(Tuplet<uint, string> resource in userInfo.Resources) {
                        userInfo.RemoveResource(resource.Item1);
                    }
                    userInfo.Save();
                } else {
                    userInfo.Invalidate();
                }
            }
            response.Return(DreamMessage.Ok());
            yield break;
        }

        [DreamFeature("POST:notify", "receive a notification to be distributed to users")]
        internal Yield Notify(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            string[] recipients = request.Headers.DreamEventRecipients;
            XDoc ev = request.ToDocument();
            string wikiid = ev["@wikiid"].AsText;
            uint pageId = ev["pageid"].AsUInt ?? 0;
            DateTime eventDate = ev["@event-time"].AsDate ?? DateTime.MinValue;
            XUri channel = ev["channel"].AsUri;
            string action = channel.Segments[2];
            bool delete = StringUtil.EqualsInvariantIgnoreCase(action, "delete");
            if(pageId == 0 || eventDate == DateTime.MinValue) {
                _log.DebugFormat("unable to get a pageId or event-time out of the event on channel '{0}'", channel);
                response.Return(DreamMessage.Ok());
                yield break;
            }
            uint eventUserId = ev["user/@id"].AsUInt ?? 0;
            _log.DebugFormat("queueing notifications for channel '{0}' and page '{1}'", channel, pageId);
            foreach(string recipient in recipients) {
                XUri recipientUri = new XUri(recipient);
                uint userId;
                if(!uint.TryParse(recipientUri.LastSegment ?? string.Empty, out userId)) {
                    _log.DebugFormat("can't find a subscription for user {0}, skipping delivery", userId);
                    continue;
                }
                if(userId == eventUserId) {
                    _log.DebugFormat("Not delivering to user {0} since the user generated the event", userId);
                    continue;
                }
                UserInfo userInfo = _subscriptions.GetUser(wikiid, userId, false);
                if(userInfo == null) {
                    continue;
                }
                _log.DebugFormat("queueing userid {0}", userInfo.Id);
                _notificationQueue.Enqueue(wikiid, userInfo.Id, pageId, eventDate, delete);
            }
            response.Return(DreamMessage.Ok());
            yield break;
        }

        //--- Methods ---
        protected override Yield Start(XDoc config, Result result) {
            yield return Coroutine.Invoke(base.Start, config, new Result());

            // set up plug for phpscript that will handle the notifications
            _emailer = Plug.New(config["uri.emailer"].AsUri);

            // set up plug deki, so we can validate users
            _deki = Plug.New(config["uri.deki"].AsUri);

            // get the apikey, which we will need as a subscription auth token for subscriptions not done on behalf of a user
            _apikey = config["apikey"].AsText;
            _cache = new PageChangeCache(_deki.With("apikey", _apikey), TimeSpan.FromSeconds(config["page-cache-ttl"].AsInt ?? 2));

            // resource manager for email template
            string resourcePath = Config["resources-path"].AsText;
            if(!string.IsNullOrEmpty(resourcePath)) {
                _resourceManager = new PlainTextResourceManager(Environment.ExpandEnvironmentVariables(resourcePath));
            } else {

                // creating a test resource manager
                _log.WarnFormat("'resource-path' was not defined in Config, using a test resource manager for email templating");
                TestResourceSet testSet = new TestResourceSet();
                testSet.Add("Notification.Page.email-subject", "Page Modified");
                testSet.Add("Notification.Page.email-header", "The following pages have changed:");
                _resourceManager = new PlainTextResourceManager(testSet);
            }

            // get persisted subscription storage
            List<Tuplet<string, List<XDoc>>> allWikiSubs = new List<Tuplet<string, List<XDoc>>>();
            Result<DreamMessage> storageCatalog;
            yield return storageCatalog = Storage.At("subscriptions").GetAsync();
            foreach(XDoc wikiSubs in storageCatalog.Value.ToDocument()["folder/name"]) {
                string wikihost = wikiSubs.AsText;
                Tuplet<string, List<XDoc>> wikiDoc = new Tuplet<string, List<XDoc>>(wikihost, new List<XDoc>());
                allWikiSubs.Add(wikiDoc);
                Result<DreamMessage> wikiUsers;
                yield return wikiUsers = Storage.At("subscriptions", wikihost).GetAsync();
                foreach(XDoc userDocname in wikiUsers.Value.ToDocument()["file/name"]) {
                    string userFile = userDocname.AsText;
                    if(!userFile.EndsWith(".xml")) {
                        _log.WarnFormat("Found stray file '{0}' in wiki '{1}' store, ignoring", userFile, wikihost);
                        continue;
                    }
                    Result<DreamMessage> userDoc;
                    yield return userDoc = Storage.At("subscriptions", wikihost, userFile).GetAsync();
                    try {
                        wikiDoc.Item2.Add(userDoc.Value.ToDocument());
                    } catch(InvalidDataException e) {
                        _log.Error(string.Format("Unable to retrieve subscription store for user {0}/{1}", wikihost, userFile), e);
                    }
                }
            }
            _subscriptions = new SubscriptionManager(Self.Uri.AsServerUri().At("notify"), allWikiSubs);
            _subscriptions.RecordsChanged += PersistSubscriptions;
            _subscriptions.SubscriptionsChanged += PushSubscriptionSetUpstream;

            // set up subscription for pubsub
            _baseSubscriptionSet = new XDoc("subscription-set")
                .Elem("uri.owner", Self.Uri.AsServerUri().ToString())
                .Start("subscription")
                    .Elem("channel", "event://*/deki/users/*")
                    .Add(DreamCookie.NewSetCookie("service-key", InternalAccessKey, Self.Uri).AsSetCookieDocument)
                    .Start("recipient")
                        .Attr("authtoken", _apikey)
                        .Elem("uri", Self.Uri.AsServerUri().At("updateuser").ToString())
                    .End()
              .End();
            XDoc subSet = _baseSubscriptionSet.Clone();
            foreach(XDoc sub in _subscriptions.Subscriptions) {
                subSet.Add(sub);
            }
            Result<DreamMessage> subscribe;
            yield return subscribe = PubSub.At("subscribers").PostAsync(subSet);
            string accessKey = subscribe.Value.ToDocument()["access-key"].AsText;
            XUri location = subscribe.Value.Headers.Location;
            Cookies.Update(DreamCookie.NewSetCookie("access-key", accessKey, location), null);
            _subscriptionLocation = Plug.New(location.AsLocalUri().WithoutQuery());
            _log.DebugFormat("set up initial subscription location at {0}", _subscriptionLocation.Uri);

            // set up notification accumulator queue
            TimeSpan accumulationMinutes = TimeSpan.FromSeconds(config["accumulation-time"].AsInt ?? 10 * 60);
            _log.DebugFormat("Initializing queue with {0:0.00} minute accumulation", accumulationMinutes.TotalMinutes);
            _notificationQueue = new NotificationDelayQueue(accumulationMinutes, SendEmail);
            result.Return();
        }

        protected override Yield Stop(Result result) {
            yield return _subscriptionLocation.DeleteAsync().Catch();
            _subscriptions.RecordsChanged -= PersistSubscriptions;
            _subscriptions.SubscriptionsChanged -= PushSubscriptionSetUpstream;
            _subscriptions = null;
            _wikiLanguageCache.Clear();
            yield return Coroutine.Invoke(base.Stop, new Result());
            result.Return();
        }

        public override DreamFeatureStage[] Prologues {
            get { return new[] { new DreamFeatureStage("ensure-wiki-id-header", PrologueSiteIdHeader, DreamAccess.Public), }; }
        }

        public override DreamFeatureStage[] Epilogues {
            get { return new[] { new DreamFeatureStage("log-called-feature", this.EpilogueLog, DreamAccess.Public), }; }
        }

        private Yield PrologueSiteIdHeader(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            if(context.Feature.MainStage.Access == DreamAccess.Public) {
                if(string.IsNullOrEmpty(request.Headers["X-Deki-Site"])) {
                    string wikiId = context.GetParam("siteid", null);
                    if(string.IsNullOrEmpty(wikiId)) {
                        throw new DreamBadRequestException("request must contain either an X-Deki-Site header or siteid query parameter");
                    }
                    request.Headers.Add("X-Deki-Site", "id=" + wikiId);
                }
            }
            response.Return(request);
            yield break;
        }

        private void ReturnUserError(Exception exception, Result<DreamMessage> response) {
            var dreamException = exception as DreamResponseException;
            if(dreamException != null) {
                response.Throw(new DreamAbortException(dreamException.Response));
            } else if(exception is UserException) {
                response.Throw(new DreamBadRequestException(exception.Message));
            } else {
                response.Throw(exception);
            }
        }

        private Yield EpilogueLog(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            _log.InfoFormat("Feature [{0}] completed", context.Uri.Path);
            response.Return(request);
            yield break;
        }

        private Yield GetUserInfo(bool createUser, DreamMessage request, Result<UserInfo> result) {

            // can assume that the header has a value, since our prologue would have barfed already otherwise
            XDoc userDoc = null;
            string wikiId = HttpUtil.ParseNameValuePairs(request.Headers["X-Deki-Site"])["id"];
            yield return _deki.At("users", "current").WithHeaders(request.Headers).Get(new Result<XDoc>()).Set(x => userDoc = x);
            result.Return(GetUserInfo(userDoc, wikiId, createUser));
        }

        private UserInfo GetUserInfo(XDoc userDoc, string wikiId, bool createUser) {
            if(string.IsNullOrEmpty(userDoc["email"].AsText)) {
                throw new UserException("no email for user");
            }
            var canSubscribe = false;
            var perms = (userDoc["permissions.user/operations"].AsText ?? "").Split(',');
            foreach(var perm in perms) {
                if("SUBSCRIBE".EqualsInvariantIgnoreCase(perm.Trim())) {
                    canSubscribe = true;
                }
            }
            if(!canSubscribe) {
                throw new UserException("user does not have subscribe role");
            }
            var userId = userDoc["@id"].AsUInt.Value;
            var userInfo = _subscriptions.GetUser(wikiId, userId, createUser);
            if(userInfo != null) {
                var email = userDoc["email"].AsText;
                var name = userDoc["fullname"].AsText.IfNullOrEmpty(userDoc["username"].AsText);
                userInfo.Email = string.IsNullOrEmpty(name) ? email : string.Format("\"{0}\" <{1}>", name.EscapeString(), email);
                var language = userDoc["language"].AsText;
                if(!string.IsNullOrEmpty(language)) {
                    userInfo.Culture = CultureUtil.GetNonNeutralCulture(language);
                }
                var timezone = userDoc["timezone"].AsText;
                if(!string.IsNullOrEmpty(timezone)) {

                    // only update timezone if the user has it defined
                    userInfo.Timezone = timezone;
                }
            }
            return userInfo;
        }

        private Yield SendEmail(NotificationUpdateRecord updateRecord, Result result) {
            bool userChanged = false;
            Plug deki = _deki.With("apikey", _apikey).WithCookieJar(Cookies);
            _log.DebugFormat("trying to dispatch email to user {0} for wiki '{1}'", updateRecord.UserId, updateRecord.WikiId);
            bool createUser = false;
            UserInfo userInfo = _subscriptions.GetUser(updateRecord.WikiId, updateRecord.UserId, false);
            if(userInfo == null) {
                createUser = true;
                _log.DebugFormat("user is gone from subscriptions. Trying to re-fetch", updateRecord.UserId, updateRecord.WikiId);
            }
            if(userInfo == null || !userInfo.IsValidated) {

                // need to refetch user info to make sure we have
                DreamMessage userMsg = null;
                yield return deki.At("users", updateRecord.UserId.ToString()).WithHeader("X-Deki-Site", "id=" + updateRecord.WikiId)
                    .Get(new Result<DreamMessage>())
                    .Set(x => userMsg = x);
                if(!userMsg.IsSuccessful) {
                    _log.DebugFormat("unable to fetch user {0}, skipping delivery: {1}", updateRecord.UserId, userMsg.Status);
                    result.Return();
                    yield break;
                }
                var userDoc = userMsg.ToDocument();
                try {
                    userInfo = GetUserInfo(userDoc, updateRecord.WikiId, createUser);
                } catch(UserException e) {
                    _log.DebugFormat("unable to re-validate user {0}, skipping delivery: {1}", updateRecord.UserId, e.Message);
                    result.Return();
                    yield break;
                }
                userInfo.Save();
            }
            SiteInfo siteInfo = _subscriptions.GetSiteInfo(updateRecord.WikiId);
            if(!siteInfo.IsValidated) {

                // lazy loading site information
                Result<DreamMessage> siteResult;
                yield return siteResult = deki.At("site", "settings").WithHeader("X-Deki-Site", "id=" + updateRecord.WikiId).GetAsync();
                DreamMessage site = siteResult.Value;
                if(!site.IsSuccessful) {
                    _log.WarnFormat("unable to fetch site data for deki '{0}', skipping delivery: {1}", updateRecord.WikiId, site.Status);
                    result.Return();
                    yield break;
                }
                XDoc siteDoc = site.ToDocument();
                siteInfo.Sitename = siteDoc["ui/sitename"].AsText;
                siteInfo.EmailFromAddress = siteDoc["page-subscription/from-address"].AsText;
                siteInfo.EmailFormat = siteDoc["page-subscription/email-format"].AsText;
                if(string.IsNullOrEmpty(siteInfo.EmailFromAddress)) {
                    siteInfo.EmailFromAddress = siteDoc["admin/email"].AsText;
                }
                siteInfo.Culture = CultureUtil.GetNonNeutralCulture(siteDoc["ui/language"].AsText) ?? CultureInfo.GetCultureInfo("en-us");
                if(!siteInfo.IsValidated) {
                    _log.WarnFormat("unable to get required data from site settings, cannot send email");
                    if(string.IsNullOrEmpty(siteInfo.Sitename)) {
                        _log.WarnFormat("missing ui/sitename");
                    }
                    if(string.IsNullOrEmpty(siteInfo.EmailFromAddress)) {
                        _log.WarnFormat("missing page-subscription/from-address");
                    }
                    result.Return();
                    yield break;
                }
            }
            CultureInfo culture = CultureUtil.GetNonNeutralCulture(userInfo.Culture, siteInfo.Culture);
            string subject = string.Format("[{0}] {1}", siteInfo.Sitename, _resourceManager.GetString("Notification.Page.email-subject", culture, "Site Modified"));
            XDoc email = new XDoc("email")
                .Attr("configuration", siteInfo.WikiId)
                .Elem("to", userInfo.Email)
                .Elem("from", siteInfo.EmailFromAddress)
                .Elem("subject", subject)
                .Start("pages");
            string header = _resourceManager.GetString("Notification.Page.email-header", culture, "The following pages have changed:");
            StringBuilder plainBody = new StringBuilder();
            plainBody.AppendFormat("{0}\r\n\r\n", header);
            XDoc htmlBody = new XDoc("body")
                .Attr("html", true)
                .Elem("h2", header);
            foreach(Tuplet<uint, DateTime, bool> record in updateRecord.Pages) {

                // TODO (arnec): Need to revalidate that the user is still allowed to see that page
                // TODO (arnec): Should check that the user is still subscribed to this page
                uint pageId = record.Item1;
                email.Elem("pageid", pageId);
                Result<PageChangeData> dataResult;
                yield return dataResult = Coroutine.Invoke(_cache.GetPageData, pageId, userInfo.WikiId, record.Item2, culture, userInfo.Timezone, new Result<PageChangeData>());
                PageChangeData data = dataResult.Value;
                if(data == null) {
                    _log.WarnFormat("Unable to fetch page change data for page {0}", pageId);
                    continue;
                }
                htmlBody.AddAll(data.HtmlBody.Elements);
                plainBody.Append(data.PlainTextBody);
                if(!record.Item3) {
                    continue;
                }
                userInfo.RemoveResource(pageId);
                userChanged = true;
            }
            email.End();
            if(!StringUtil.EqualsInvariantIgnoreCase(siteInfo.EmailFormat, "html")) {
                email.Elem("body", plainBody.ToString());
            }
            if(!StringUtil.EqualsInvariantIgnoreCase(siteInfo.EmailFormat, "plaintext")) {
                email.Add(htmlBody);
            }
            _log.DebugFormat("dispatching email for user '{0}'", userInfo.Id);
            yield return _emailer.WithCookieJar(Cookies).PostAsync(email).Catch();
            if(userChanged) {
                userInfo.Save();
            }
            result.Return();
            yield break;
        }

        private void PersistSubscriptions(object sender, RecordEventArgs e) {

            // This is an event handler coming from a worker thread, so sync call is fine
            if(e.Delete) {
                Storage.At("subscriptions", e.WikiId, "user_" + e.User.Id + ".xml").WithCookieJar(Cookies).Delete();
            } else {
                Storage.At("subscriptions", e.WikiId, "user_" + e.User.Id + ".xml").WithCookieJar(Cookies).Put(e.User.AsDocument);
            }
        }

        private void PushSubscriptionSetUpstream(object sender, SubscriptionEventArgs e) {
            XDoc subSet = _baseSubscriptionSet.Clone();
            foreach(XDoc sub in e.Subscriptions) {
                subSet.Add(sub);
            }

            // This is an event handler coming from a worker thread, so sync call is fine
            _log.DebugFormat("pushing subscription set to {0}", _subscriptionLocation.Uri);
            _subscriptionLocation.WithCookieJar(Cookies).Put(subSet);
        }
    }
}
