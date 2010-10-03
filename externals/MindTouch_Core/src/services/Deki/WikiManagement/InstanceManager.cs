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
using log4net;

using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Web;
using MindTouch.Xml;

namespace MindTouch.Deki.WikiManagement {
    public abstract class InstanceManager {

        // --- Constants ---
        public const string DEFAULT_WIKI_ID = "default";
        const int MIN_SECS_FROM_ACCESS_TO_SHUTDOWN = 60;

        // --- Class fields ---
        protected static readonly ILog _log = LogUtils.CreateLog();

        // --- Class methods ---
        public static InstanceManager New(DekiWikiService dekiService) {
            InstanceManager mgr = null;
            string srcString = dekiService.Config["wikis/@src"].AsText;
            if(!string.IsNullOrEmpty(srcString)) {
                XUri remoteDirUri = null;
                if(!XUri.TryParse(srcString, out remoteDirUri)) {

                    //TODO: build a specialized exception out of this
                    throw new ApplicationException(string.Format("Configuration is not valid. wikis/@src ({0})is not a valid url!", srcString));
                }
                mgr = new RemoteInstanceManager(dekiService, remoteDirUri);
            } else {
                mgr = new LocalInstanceManager(dekiService);
            }

            mgr.maxInstances = dekiService.Config["wikis/@max"].AsUInt ?? 0;
            uint timeoutSecs = dekiService.Config["wikis/@ttl"].AsUInt ?? uint.MaxValue;
            if(timeoutSecs == 0 || timeoutSecs == uint.MaxValue) {
                mgr._inactiveInstanceTimeOut = TimeSpan.MaxValue;
            } else {
                mgr._inactiveInstanceTimeOut = TimeSpan.FromSeconds(timeoutSecs);
            }

            return mgr;
        }

        // --- Fields ---
        private Dictionary<string, DekiInstance> _instances = new Dictionary<string, DekiInstance>();
        private readonly Dictionary<string, TaskTimer> _instanceExpireTimers = new Dictionary<string, TaskTimer>();
        private uint maxInstances = uint.MaxValue;
        private TimeSpan _inactiveInstanceTimeOut;

        protected DekiWikiService _dekiService;

        // --- Constructors ---
        protected InstanceManager(DekiWikiService dekiService) {
            _dekiService = dekiService;
        }

        // --- Abstract methods ---
        public abstract XDoc GetGlobalConfig();
        protected abstract XDoc GetConfigForWikiId(string wikiId);
        protected abstract string GetWikiIdByHostname(string hostname);

        //--- Properties ---
        public TimeSpan InactiveInstanceTimeOut { get { return _inactiveInstanceTimeOut; } }
        public uint InstancesRunning {
            get {
                lock(_instances) {
                    return (uint)_instances.Count;
                }
            }
        }

        // --- Methods ---
        public IEnumerable<KeyValuePair<string, string>> GetGlobalServices() {
            var config = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("services/mailer", _dekiService.Mailer.Uri.AsPublicUri().ToString()), 
                new KeyValuePair<string, string>("services/luceneindex", _dekiService.LuceneIndex.Uri.AsPublicUri().ToString()), 
                new KeyValuePair<string, string>("services/pagesubscription", _dekiService.PageSubscription.Uri.AsPublicUri().ToString()),
                new KeyValuePair<string, string>("services/packageupdater", _dekiService.PackageUpdater.Uri.AsPublicUri().ToString())
            };
            return config;
        }

        public DekiInstance GetWikiInstance(DreamMessage request) {
            if(request == null)
                return null;

            DekiInstance dekiInstance = null;

            string hostname = string.Empty;
            bool wikiIdFromHeader = true;
            string wikiId = null;
            string publicUriFromHeader = null;
            var wikiIdentityHeader = HttpUtil.ParseNameValuePairs(request.Headers[DekiWikiService.WIKI_IDENTITY_HEADERNAME] ?? "");
            wikiIdentityHeader.TryGetValue("id", out wikiId);
            wikiIdentityHeader.TryGetValue("uri", out publicUriFromHeader);
            if(string.IsNullOrEmpty(wikiId)) {

                // get requested hostname and normalize
                wikiIdFromHeader = false;
                hostname = request.Headers.Host;
                if(!string.IsNullOrEmpty(hostname)) {
                    if(hostname.EndsWith(":80")) {
                        hostname = hostname.Substring(0, hostname.Length - 3);
                    }

                    hostname = hostname.Trim().ToLowerInvariant();
                }
                wikiId = GetWikiIdByHostname(hostname);
                if(string.IsNullOrEmpty(wikiId))
                    wikiId = DEFAULT_WIKI_ID;
            }
            dekiInstance = GetWikiInstance(wikiId);

            //If an instance doesn't exist or if it was last updated over x minutes ago, refetch the instance config to check for status changes
            if(dekiInstance == null || (InactiveInstanceTimeOut != TimeSpan.MaxValue && dekiInstance.InstanceLastUpdateTime.Add(InactiveInstanceTimeOut) < DateTime.UtcNow)) {

                XDoc instanceConfig = XDoc.Empty;
                try {
                    instanceConfig = GetConfigForWikiId(wikiId);
                    if(instanceConfig.IsEmpty && dekiInstance == null) {
                        if(wikiIdFromHeader) {
                            throw new DreamAbortException(new DreamMessage(DreamStatus.Gone, null, MimeType.TEXT, string.Format("No wiki exists for provided wikiId '{0}'", wikiId)));
                        } else {
                            throw new DreamAbortException(new DreamMessage(DreamStatus.Gone, null, MimeType.TEXT, string.Format("No wiki exists at host '{0}'", hostname)));
                        }
                    }
                } catch(DreamAbortException e) {
                    if(e.Response.Status == DreamStatus.Gone) {
                        ShutdownInstance(wikiId);
                        throw;
                    }
                    if(dekiInstance == null) {
                        throw;
                    }
                } catch {
                    if(dekiInstance == null) {
                        throw;
                    }
                } finally {
                    if(dekiInstance != null)
                        dekiInstance.InstanceLastUpdateTime = DateTime.UtcNow;
                }

                //If a wiki already exists, shut it down if it was updated since it was last created
                if(dekiInstance != null && dekiInstance.InstanceCreationTime < (instanceConfig["@updated"].AsDate ?? DateTime.MinValue)) {
                    ShutdownInstance(wikiId);
                    dekiInstance = null;
                }

                // create instance if none exists
                if(dekiInstance == null) {
                    dekiInstance = CreateWikiInstance(wikiId, instanceConfig);
                }
            }
            if(InactiveInstanceTimeOut != TimeSpan.MaxValue) {
                lock(_instances) {
                    TaskTimer timer;
                    if(_instanceExpireTimers.TryGetValue(wikiId, out timer)) {
                        timer.Change(InactiveInstanceTimeOut, TaskEnv.Clone());
                    }
                }
            }
            if(wikiIdFromHeader) {

                // the request host does not represent a valid host for public uri generation, so we need to alter the context
                XUri publicUriOverride;
                if(string.IsNullOrEmpty(publicUriFromHeader) || !XUri.TryParse(publicUriFromHeader, out publicUriOverride)) {
                    _log.DebugFormat("no public uri provided in wiki header, using canonical uri");
                    publicUriOverride = dekiInstance.CanonicalUri;
                }

                // Note (arnec): Propagating a much hard-coded assumption, i.e. that the Api for any Deki instance can be accessed
                // at the instances' canonical uri plus @api
                publicUriOverride = publicUriOverride.At("@api");
                _log.DebugFormat("switching public uri from {0} to {1} for request", DreamContext.Current.PublicUri, publicUriOverride);
                DreamContext.Current.SetPublicUriOverride(publicUriOverride);
            }
            dekiInstance.InstanceLastAccessedTime = DateTime.UtcNow;
            return dekiInstance;
        }

        protected DekiInstance GetWikiInstance(string wikiId) {
            DekiInstance di = null;
            lock(_instances) {
                _log.DebugFormat("retrieving instance for wiki id '{0}'", wikiId);
                _instances.TryGetValue(wikiId, out di);
            }
            return di;
        }

        protected DekiInstance CreateWikiInstance(string wikiId, XDoc instanceConfig) {
            List<KeyValuePair<string, DekiInstance>> instanceList = null;
            DekiInstance instance = null;
            int instanceCount = 0;

            //throw exception if licensing does not allow startup of another wiki instance
            MindTouch.Deki.Logic.LicenseBL.IsDekiInstanceStartupAllowed(true);

            lock(_instances) {
                instance = GetWikiInstance(wikiId);
                if(instance == null) {

                    _instances[wikiId] = instance = new DekiInstance(_dekiService, wikiId, instanceConfig);
                    instance.InstanceCreationTime = DateTime.UtcNow;
                }

                //Schedule new instance for shutdown if inactive-instance-timeout enabled.
                if(InactiveInstanceTimeOut != TimeSpan.MaxValue) {
                    TaskTimer timer = new TaskTimer(OnInstanceExpireTimer, wikiId);
                    _instanceExpireTimers[wikiId] = timer;
                }

                instanceCount = _instances.Count;
                if(maxInstances != 0 && instanceCount > maxInstances) {
                    instanceList = new List<KeyValuePair<string, DekiInstance>>(_instances);
                }
            }

            //Hit the instance number limit? Look for least recently accessed wiki and shut it down.
            if(instanceList != null) {
                instanceList.Sort(delegate(KeyValuePair<string, DekiInstance> left, KeyValuePair<string, DekiInstance> right) {
                    return DateTime.Compare(left.Value.InstanceLastAccessedTime, right.Value.InstanceLastAccessedTime);
                });

                List<KeyValuePair<string, DekiInstance>> instancesToExamine =
                    instanceList.GetRange(0, instanceList.Count - (int)maxInstances);

                if(instancesToExamine.Count > 0) {
                    Async.Fork(delegate() {
                        foreach(KeyValuePair<string, DekiInstance> instancePair in instancesToExamine) {
                            if((DateTime.UtcNow - instancePair.Value.InstanceLastAccessedTime).TotalSeconds >= MIN_SECS_FROM_ACCESS_TO_SHUTDOWN) {
                                ShutdownInstance(instancePair.Key);
                            }
                        }
                    }, null);
                }
            }

            return instance;
        }


        private void ShutdownInstance(string wikiId) {
            DekiInstance instance = null;
            TaskTimer timer = null;
            lock(_instances) {
                if(_instances.TryGetValue(wikiId, out instance)) {
                    _instances.Remove(wikiId);
                }
                if(_instanceExpireTimers.TryGetValue(wikiId, out timer)) {
                    _instanceExpireTimers.Remove(wikiId);
                }
            }
            if(instance != null) {
                lock(instance) {
                    if(timer != null)
                        timer.Cancel();

                    ShutdownInstance(wikiId, instance);
                }
            }
        }

        protected virtual void ShutdownInstance(string wikiid, DekiInstance instance) {
            if(instance.Status == DekiInstanceStatus.RUNNING) {
                DekiContext dekiContext = new DekiContext(_dekiService, instance, DreamContext.Current.Request);
                DreamContext.Current.SetState<DekiContext>(dekiContext);
                lock(instance) {
                    instance.Shutdown();
                }
            }
        }

        protected void OnInstanceExpireTimer(TaskTimer timer) {
            _log.InfoMethodCall("instance expired", timer.State);
            ShutdownInstance((string)timer.State);
        }

        // --- Virtual methods ---
        public virtual void Shutdown() {
            lock(_instances) {
                foreach(string wikiId in new List<string>(_instances.Keys)) {
                    ShutdownInstance(wikiId);
                }
            }

            DreamContext.Current.SetState<DekiContext>(null);
            _instances = null;
        }
    }
}
