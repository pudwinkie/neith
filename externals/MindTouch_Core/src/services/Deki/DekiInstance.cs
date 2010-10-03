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
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

using log4net;
using MindTouch.Cache;
using MindTouch.Deki.Caching;
using MindTouch.Deki.Data;
using MindTouch.Deki.Logic;
using MindTouch.Deki.Search;
using MindTouch.Deki.Storage;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Xml;

namespace MindTouch.Deki {

    /// <summary>
    /// Used as a basis of building dekicontexts tied to a specific instance
    /// </summary>
    public class DekiInstance {

        //--- Constants ---
        public const int RUNNING_WAIT_TIMEOUT = 30;

        //--- Static Fields ---
        private static readonly ILog _log = LogUtils.CreateLog();

        //--- Fields ---
        public readonly string Id;
        public readonly XDoc Config;

        // TODO (arnec): Refactor all ICacheProvider users to use SearchCache and make sure they can be serialized
        [Obsolete("Cache will soon be replaced with SearchCache.")]
        public readonly ICacheProvider Cache;
        public readonly IKeyValueCache SearchCache;
        public readonly ILog Log;
        public readonly TaskTimerFactory TimerFactory;
        public long RequestCounter;
        public long RequestCounter_Success;
        public TimeSpan ExecutionTime;
        public string StatusDescription;
        public readonly ServiceRepository RunningServices = new ServiceRepository();
        public DateTime SettingsLastModified = DateTime.UtcNow;
        public DateTime InstanceLastUpdateTime = DateTime.UtcNow;
        public DateTime InstanceCreationTime = DateTime.UtcNow;
        public DateTime InstanceLastAccessedTime = DateTime.UtcNow;
        public ulong HomePageId;
        public object SettingsSyncRoot = new object();
        private XUri _canonicalUri;
        private IStorageProvider _storage;
        private Dictionary<string, XDoc> _configCache = new Dictionary<string, XDoc>();
        private DekiInstanceStatus _status;
        private DekiWikiService _deki;
        private object CounterSyncRoot = new object();
        private Regex _bannedWords;
        private DSACryptoServiceProvider _privateDigitalSignature;
        private System.Threading.ManualResetEvent _runningEvent = new System.Threading.ManualResetEvent(false);
        private DekiChangeSink _eventSink;
        private IDekiDataSessionFactory _sessionFactory;
        private List<MimeType> _inlineDispositionBlacklist;
        private List<MimeType> _inlineDispositionWhitelist;
        private string[] _fileExtensionForcedAsText;
        private bool? _pageViewEventsEnabled = null;
        private Dictionary<uint, TaskTimer> _serviceExpirations = new Dictionary<uint, TaskTimer>();

        //--- Constructor ---
        public DekiInstance(DekiWikiService deki, string id, XDoc instanceConfig) {
            if(deki == null) {
                throw new ArgumentNullException("deki");
            }
            if(string.IsNullOrEmpty(id)) {
                throw new ArgumentNullException("id");
            }

            this.Id = id;
            this.TimerFactory = TaskTimerFactory.Create(this);
            this.Cache = new DreamCache(TimerFactory);
            var cacheFactory = new InMemoryKeyValueCacheFactory(TimerFactory);
            var searchSerializer = new SearchSerializer();
            cacheFactory.SetSerializer<SearchResult>(searchSerializer);
            cacheFactory.SetSerializer<SearchResultDetail>(searchSerializer);
            this.SearchCache = cacheFactory.Create();
            this.Config = instanceConfig;
            this.Log = LogManager.GetLogger(deki.GetType().Name + "_" + id);
            _deki = deki;
            _status = DekiInstanceStatus.CREATED;
            foreach(XDoc hostDoc in Config["host"]) {
                string host = hostDoc.Contents;
                if(!StringUtil.EqualsInvariantIgnoreCase(host, "*")) {
                    string port = hostDoc["@port"].AsText;
                    string scheme = hostDoc["@https"].AsBool.GetValueOrDefault() ? "https://" : "http://";
                    string uri = scheme + host + (string.IsNullOrEmpty(port) ? "" : ":" + port);
                    _canonicalUri = new XUri(uri);
                    _log.DebugFormat("divined canonical use from hosts as {0}", _canonicalUri);
                    break;
                }
            }
            if(_canonicalUri == null) {

                // Note (arnec): this is a best guess fallback. It will only work in these scenarios:
                // a) The host was set up with a uri.public that has ends in @api and with the @api points to the site uri, or
                // b) The api lives on the same machine as the site, so that deriving uri.public for the host from the machine
                // IP happens to point to the same machine
                // Either way it relies on the hard-coded assumption that the api is accessible via {site}/@api
                _canonicalUri = DreamContext.Current.ServerUri;
                if(_canonicalUri.LastSegment.EqualsInvariantIgnoreCase("@api")) {
                    _canonicalUri = _canonicalUri.WithoutLastSegment();
                }
                _log.DebugFormat("using server uri as canonical uri: {0}", _canonicalUri);
            } else {

                // Note (arnec): Propagating a much hard-coded assumption, i.e. that the Api for any Deki instance can be accessed
                // at the instances' canonical uri plus @api

                // register the api uri with the dream host so that requests originating from within Dream are guaranteed to be locally routed
                _deki.Env.At("status", "aliases").Post(new XDoc("aliases").Elem("uri.alias", _canonicalUri.At("@api")), new Result<DreamMessage>());
            }
        }

        //--- Properties ---
        public DekiInstanceStatus Status { get { return _status; } }
        public IStorageProvider Storage { get { return _storage; } }

        public bool EnableUnsafeIEContentInlining {
            get {
                bool enableUnsafeIEContentInlining;
                Boolean.TryParse(ConfigBL.GetInstanceSettingsValue("files/enable-unsafe-ie-inlining", "false"), out enableUnsafeIEContentInlining);
                return enableUnsafeIEContentInlining;
            }
        }

        public bool PageViewEventsEnabled {
            get {
                // Note (arnec): have to lazy initialize because at constructor time ConfigBL can't be used since it assumes the context to already exists
                if(!_pageViewEventsEnabled.HasValue) {
                    bool pageViewEventsEnabled;
                    Boolean.TryParse(ConfigBL.GetInstanceSettingsValue("events/enable-page-view-events", "false"), out pageViewEventsEnabled);
                    _pageViewEventsEnabled = pageViewEventsEnabled;
                }
                return _pageViewEventsEnabled.GetValueOrDefault();
            }
        }

        public string[] FileExtensionBlockList {
            get {

                //setup attachment blocklist
                return ConfigBL.GetInstanceSettingsValue("files/blocked-extensions", string.Empty).ToLowerInvariant().Split(new char[] { ',', ' ', '.' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public string[] FileExtensionForceAsTextList {
            get {
                if(_fileExtensionForcedAsText == null) {
                    _fileExtensionForcedAsText = ConfigBL.GetInstanceSettingsValue("files/force-text-extensions", string.Empty).ToLowerInvariant().Split(new char[] { ',', ' ', '.' }, StringSplitOptions.RemoveEmptyEntries);
                }
                return _fileExtensionForcedAsText;
            }
        }

        public string[] ImageMagickExtensions {
            get {

                //setup imagemagick whitelist
                return ConfigBL.GetInstanceSettingsValue("files/imagemagick-extensions", string.Empty).ToLowerInvariant().Split(new char[] { ',', ' ', '.' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public string Languages { get { return ConfigBL.GetInstanceSettingsValue("languages", string.Empty); } }
        public string SiteLanguage { get { return ConfigBL.GetInstanceSettingsValue("ui/language", "en-US"); } }
        public string SiteTimezone { get { return ConfigBL.GetInstanceSettingsValue("ui/timezone", "GMT"); } }
        public string AuthTokenSalt { get { return this.ApiKey + ConfigBL.GetInstanceSettingsValue("security/authtokensalt", string.Empty); } }
        public ulong MaxImageSize { get { return ConfigBL.GetInstanceSettingsValueAs<ulong>("files/imagemagick-max-size") ?? ulong.MaxValue; } }
        public uint ImageThumbPixels { get { return ConfigBL.GetInstanceSettingsValueAs<uint>("files/imagemagick-thumbnail-pixels") ?? 160; } }
        public uint ImageWebviewPixels { get { return ConfigBL.GetInstanceSettingsValueAs<uint>("files/imagemagick-webview-pixels") ?? 550; } }
        public long MaxFileSize { get { return ConfigBL.GetInstanceSettingsValueAs<long>("files/max-file-size") ?? long.MaxValue; } }
        public string SiteName { get { return ConfigBL.GetInstanceSettingsValue("ui/sitename", "MindTouch"); } }
        public bool AllowAnonymousLocalAccountCreation { get { return ConfigBL.GetInstanceSettingsValueAs<bool>("security/allow-anon-account-creation") ?? true; } }
        public string NewAccountRole { get { return ConfigBL.GetInstanceSettingsValue("security/new-account-role", null); } }
        public string HomePageGrantRole { get { return ConfigBL.GetInstanceSettingsValue("security/homepage-grant-role", null); } }
        public string ApiKey { get { return ConfigBL.GetInstanceSettingsValue("security/api-key", null); } }
        public TimeSpan AuthCookieExpirationTime { get { return TimeSpan.FromSeconds(ConfigBL.GetInstanceSettingsValueAs<int>("security/cookie-expire-secs") ?? 604800); } }
        public string AdminUserForImpersonation { get { return ConfigBL.GetInstanceSettingsValue("security/admin-user-for-impersonation", null); } }
        public bool SafeHtml { get { return ConfigBL.GetInstanceSettingsValueAs<bool>("editor/safe-html") ?? true; } }
        public bool WebLinkNoFollow { get { return ConfigBL.GetInstanceSettingsValueAs<bool>("editor/web-link-nofollow") ?? true; } }
        public uint LogoWidth { get { return ConfigBL.GetInstanceSettingsValueAs<uint>("ui/logo-maxwidth") ?? 280; } }
        public uint LogoHeight { get { return ConfigBL.GetInstanceSettingsValueAs<uint>("ui/logo-maxheight") ?? 72; } }
        public bool LimitedAdminPermissions { get { return ConfigBL.GetInstanceSettingsValueAs<bool>("site/limited-admin-permissions") ?? false; } }
        public int NavMaxItems { get { return ConfigBL.GetInstanceSettingsValueAs<int>("ui/nav-max-items") ?? 25; } }
        public bool CacheAnonymousOutput { get { return ConfigBL.GetInstanceSettingsValueAs<bool>("cache/anonymous-output") ?? false; } }
        public TimeSpan CacheAnonymousOutputShort { get { return TimeSpan.FromSeconds(ConfigBL.GetInstanceSettingsValueAs<double>("cache/anonymous-output-short") ?? 30); } }
        public TimeSpan CacheAnonymousOutputLong { get { return TimeSpan.FromSeconds(ConfigBL.GetInstanceSettingsValueAs<double>("cache/anonymous-output-long") ?? (15 * 60)); } }
        public bool CacheBans { get { return ConfigBL.GetInstanceSettingsValueAs<bool>("cache/bans") ?? true; } }
        public bool StatsPageHitCounter { get { return ConfigBL.GetInstanceSettingsValueAs<bool>("stats/page-hit-counter") ?? true; } }
        public string ContentNewUser { get { return ConfigBL.GetInstanceSettingsValue("content/new-user", string.Empty); } }
        public string GravatarRating { get { return ConfigBL.GetInstanceSettingsValue("gravatar/rating", null); } }
        public string GravatarSize { get { return ConfigBL.GetInstanceSettingsValue("gravatar/size", null); } }
        public string GravatarDefault { get { return ConfigBL.GetInstanceSettingsValue("gravatar/default", DekiContext.Current.UiUri.AtPath(DekiWikiService.GRAVATAR_DEFAULT_PATH).ToString()); } }
        public string GravatarSalt { get { return ConfigBL.GetInstanceSettingsValue("gravatar/secure", null); } }
        public string ExternalLinkTarget { get { return ConfigBL.GetInstanceSettingsValue("ui/external-link-target", null); } }
        public int MaxHeadingLevelForTableOfContents { get { return ConfigBL.GetInstanceSettingsValueAs<int>("ui/toc-max-heading") ?? 6; } }
        public bool PrivacyExposeUserEmail { get { return ConfigBL.GetInstanceSettingsValueAs<bool>("privacy/expose-user-email") ?? false; } }
        public int RecentChangesScanSize { get { return ConfigBL.GetInstanceSettingsValueAs<int>("feed/recent-changes-scan-size") ?? 10000; } }
        public bool RecentChangesDiffCaching { get { return ConfigBL.GetInstanceSettingsValueAs<bool>("feed/diff-cache") ?? false; } }
        public TimeSpan RecentChangesFeedCachingTtl { get { return TimeSpan.FromSeconds(ConfigBL.GetInstanceSettingsValueAs<double>("feed/cache-ttl") ?? 0); } }

        public DekiChangeSink EventSink { get { return _eventSink; } }
        public IDekiDataSessionFactory SessionFactory { get { return _sessionFactory; } }

        public Regex BannedWords {
            get {
                if(_bannedWords == null) {

                    // parse line
                    string list = ConfigBL.GetInstanceSettingsValue("ui/banned-words", null);
                    if(list == null) {
                        return null;
                    }
                    string[] words = list.Split(new char[] { ',', ';', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if(words.Length == 0) {
                        return null;
                    }
                    Array.Sort(words, delegate(string left, string right) {
                        return right.Length - left.Length;
                    });

                    // create regular expression
                    StringBuilder pattern = new StringBuilder();
                    for(int i = 0; i < words.Length; ++i) {
                        if(pattern.Length > 0) {
                            pattern.Append("|");
                        }
                        pattern.Append(Regex.Escape(words[i]));
                    }
                    _bannedWords = new Regex(pattern.ToString(), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                }
                return _bannedWords;
            }
            set {
                _bannedWords = value;
            }
        }

        public DSACryptoServiceProvider PrivateDigitalSignature {
            get {
                if(_privateDigitalSignature != null) {
                    return _privateDigitalSignature;
                }
                string dsaKey = ConfigBL.GetInstanceSettingsValue("security/digital-signature", null);
                if(dsaKey != null) {
                    try {
                        DSACryptoServiceProvider dsa = new DSACryptoServiceProvider();
                        dsa.ImportCspBlob(Convert.FromBase64String(dsaKey));
                        _privateDigitalSignature = dsa;
                    } catch {

                        // something happened, maybe the key is invalid
                    }
                }
                return _privateDigitalSignature;
            }
            set {
                _privateDigitalSignature = value;
            }
        }

        public XUri CanonicalUri { get { return _canonicalUri; } }

        //--- Methods ---
        public ServiceRepository.IServiceInfo RegisterRemoteService(ServiceBE service, XUri serviceUri) {
            return RunningServices.RegisterService(service, serviceUri, false);
        }

        public ServiceRepository.IServiceInfo CreateLocalService(ServiceBE service, string servicePath, XDoc config) {
            var deki = DekiContext.Current.Deki;
            Plug location = deki.InternalCreateService(servicePath, service.SID, config, new Result<Plug>()).Wait();
            XUri sid = XUri.TryParse(service.SID);
            string license;
            DateTime? expiration;
            if(deki.TryGetServiceLicense(sid, out license, out expiration) && (expiration != null)) {
                lock(_serviceExpirations) {
                    TaskTimer timer;
                    if(_serviceExpirations.TryGetValue(service.Id, out timer)) {
                        timer.Cancel();
                    }
                    _serviceExpirations[service.Id] = TimerFactory.New(expiration.Value, _ => ServiceBL.StopService(service), null, TaskEnv.Clone());
                }
            }
            return RunningServices.RegisterService(service, location.Uri, true);
        }

        public void DeregisterService(uint serviceId) {
            lock(_serviceExpirations) {
                TaskTimer timer;
                if(_serviceExpirations.TryGetValue(serviceId, out timer)) {
                    timer.Cancel();
                    _serviceExpirations.Remove(serviceId);
                }
            }
            ServiceRepository.IServiceInfo serviceInfo = RunningServices[serviceId];
            if(serviceInfo == null) {
                _log.DebugFormat("cannot destroy service {0}, already destroyed", serviceId);
                return;
            }
            if(serviceInfo.IsLocal) {
                Plug location = Plug.New(serviceInfo.ServiceUri);
                if(location != null) {
                    location.DeleteAsync().Wait();
                }
            }
            RunningServices.DeregisterService(serviceInfo);
        }

        /// <summary>
        /// Calls methods to perform initial init of a wiki instance including db updates, etc
        /// </summary>
        public void Startup(DekiContext context) {
            if(context == null) {
                throw new ArgumentNullException("context");
            }
            if(_status != DekiInstanceStatus.CREATED) {
                throw new InvalidOperationException("bad state");
            }
            _status = DekiInstanceStatus.INITIALIZING;

            // run startup code
            try {

                // create the IDekiDataSessionFactory for this instance
                Type typeMySql = Type.GetType("MindTouch.Deki.Data.MySql.MySqlDekiDataSessionFactory, mindtouch.deki.data.mysql", true);
                Type typeCaching = null;
                try {
                    typeCaching = Type.GetType("MindTouch.Deki.Data.Caching.CachingDekiDataSessionFactory, mindtouch.data.caching", false);
                } catch(Exception x) {
                    Log.Warn("The caching library was found but could not be loaded. Check that its version matches the version of your MindTouch API", x);
                }

                IDekiDataSessionFactory factoryMySql = (IDekiDataSessionFactory)typeMySql.GetConstructor(Type.EmptyTypes).Invoke(null);
                IDekiDataSessionFactory factoryLogging = new LoggingDekiDataSessionFactory(factoryMySql);

                if(typeCaching != null) {
                    IDekiDataSessionFactory factoryCaching = (IDekiDataSessionFactory)typeCaching.GetConstructor(new[] { typeof(IDekiDataSessionFactory) }).Invoke(new object[] { factoryLogging });
                    _sessionFactory = factoryCaching;
                } else {
                    _sessionFactory = factoryLogging;
                }

                _sessionFactory.Initialize(Config ?? _deki.Config, new DekiInstanceSettings());

                try {
                    DbUtils.CurrentSession = _sessionFactory.CreateSession();

                    // check for 'api-key'
                    if(string.IsNullOrEmpty(ApiKey) && string.IsNullOrEmpty(_deki.MasterApiKey)) {
                        throw new ArgumentNullException("apikey", "Missing apikey for wiki instance. Please ensure that you have a global <apikey> defined (in the service settings xml file) or an instance specific key in the config table as 'security/api-key'.");
                    }

                    // check if a storage config section was provided (default storage is filesystem provider)
                    XDoc storageConfig;
                    switch(ConfigBL.GetInstanceSettingsValue("storage/@type", ConfigBL.GetInstanceSettingsValue("storage/type", "default"))) {
                    case "default":
                        string defaultAttachPath = Path.Combine(_deki.DekiPath, "attachments");
                        storageConfig = new XDoc("config")
                            .Elem("path", defaultAttachPath)
                            .Elem("cache-path", Path.Combine(defaultAttachPath, ".cache"));
                        _storage = new FSStorage(storageConfig);
                        break;
                    case "fs":
                        string fsPath = ConfigBL.GetInstanceSettingsValue("storage/fs/path", null);

                        //Replace a $1 with the wiki name                
                        fsPath = string.Format(Dream.PhpUtil.ConvertToFormatString(fsPath ?? string.Empty), Id);
                        storageConfig = new XDoc("config")
                            .Elem("path", fsPath)
                            .Elem("cache-path", ConfigBL.GetInstanceSettingsValue("storage/fs/cache-path", null));
                        _storage = new FSStorage(storageConfig);
                        break;
                    case "s3":
                        storageConfig = new XDoc("config")
                            .Elem("publickey", ConfigBL.GetInstanceSettingsValue("storage/s3/publickey", null))
                            .Elem("privatekey", ConfigBL.GetInstanceSettingsValue("storage/s3/privatekey", null))
                            .Elem("bucket", ConfigBL.GetInstanceSettingsValue("storage/s3/bucket", null))
                            .Elem("prefix", string.Format(Dream.PhpUtil.ConvertToFormatString(ConfigBL.GetInstanceSettingsValue("storage/s3/prefix", string.Empty)), DekiContext.Current.Instance.Id))
                            .Elem("timeout", ConfigBL.GetInstanceSettingsValue("storage/s3/timeout", null))
                            .Elem("allowredirects", ConfigBL.GetInstanceSettingsValue("storage/s3/allowredirects", null))
                            .Elem("redirecttimeout", ConfigBL.GetInstanceSettingsValue("storage/s3/redirecttimeout", null));
                        _storage = new S3Storage(storageConfig);
                        break;
                    default:
                        throw new ArgumentException("Storage provider unknown or not defined (key: storage/type)", "storage/type");
                    }

                    HomePageId = DbUtils.CurrentSession.Pages_HomePageId;
                } finally {
                    if(null != DbUtils.CurrentSession) {
                        DbUtils.CurrentSession.Dispose();
                        DbUtils.CurrentSession = null;
                    }
                }
                _eventSink = new DekiChangeSink(Id, DekiContext.Current.ApiUri, DekiContext.Current.Deki.PubSub.At("publish").WithCookieJar(DekiContext.Current.Deki.Cookies));
                _eventSink.InstanceStarting(DreamContext.Current.StartTime);
            } catch {

                // we failed to initialize
                _status = DekiInstanceStatus.ABANDONED;
                throw;
            }

            // set state to initializing
            _status = DekiInstanceStatus.INITIALIZING;
        }

        public void StartServices() {
            _runningEvent.Set();
            _status = DekiInstanceStatus.STARTING_SERVICES;
            DbUtils.CurrentSession = _sessionFactory.CreateSession();
            ServiceBL.StartServices();
            _status = DekiInstanceStatus.RUNNING;
            _eventSink.InstanceStarted(DreamContext.Current.StartTime);
        }

        public void CheckInstanceIsReady() {
            if(_status == DekiInstanceStatus.CREATED || _status == DekiInstanceStatus.INITIALIZING) {
                if(!_runningEvent.WaitOne(TimeSpan.FromSeconds(RUNNING_WAIT_TIMEOUT), false)) {
                    throw new TimeoutException("instance initialization timed out");
                }
            }
        }

        public void Shutdown() {
            if(_status != DekiInstanceStatus.RUNNING) {
                throw new InvalidOperationException("bad state");
            }
            TimerFactory.Dispose();
            _storage.Shutdown();

            // run shudown code
            ServiceBL.StopServices();

            // reset instance fields
            _status = DekiInstanceStatus.STOPPED;
            RunningServices.Clear();
            _configCache = null;
            _storage = null;
            Cache.Dispose();
            SearchCache.Dispose();
            _eventSink.InstanceShutdown(DreamContext.Current.StartTime);
            _eventSink = null;

            // unregister the instance to database mapping
            if(null != _sessionFactory) {
                _sessionFactory.Dispose();
                _sessionFactory = null;
            }
        }

        public void IncreasetHitCounter(bool success, TimeSpan executionTime) {
            lock(CounterSyncRoot) {
                ++RequestCounter;
                if(success) {
                    ++RequestCounter_Success;
                }
                ExecutionTime.Add(executionTime);
            }
        }

        public bool MimeTypeCanBeInlined(MimeType mimeType) {
            if(_inlineDispositionBlacklist == null) {
                string[] blackliststrings = ConfigBL.GetInstanceSettingsValue("files/blacklisted-disposition-mimetypes", string.Empty).ToLowerInvariant().Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                List<MimeType> blacklist = new List<MimeType>();
                foreach(string v in blackliststrings) {
                    MimeType mt = MimeType.New(v);
                    if(mt != null) {
                        blacklist.Add(mt);
                    }
                }
                string[] whiteliststrings = ConfigBL.GetInstanceSettingsValue("files/whitelisted-disposition-mimetypes", string.Empty).ToLowerInvariant().Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                List<MimeType> whitelist = new List<MimeType>();
                foreach(string v in whiteliststrings) {
                    MimeType mt = MimeType.New(v);
                    if(mt != null) {
                        whitelist.Add(mt);
                    }
                }
                _inlineDispositionBlacklist = blacklist;
                _inlineDispositionWhitelist = whitelist;
            }
            foreach(MimeType bad in _inlineDispositionBlacklist) {
                if(mimeType.Match(bad)) {
                    return false;
                }
            }
            foreach(MimeType good in _inlineDispositionWhitelist) {
                if(mimeType.Match(good)) {
                    return true;
                }
            }
            return false;
        }


        private XDoc LookupSystemSetting(string xpath) {
            XDoc result;
            if(!_configCache.TryGetValue(xpath, out result)) {
                result = XDoc.Empty;
                lock(_configCache) {
                    if(Config != null) {
                        result = Config[xpath];
                    }

                    if(result.IsEmpty) {
                        result = _deki.Config[xpath];
                    }
                    _configCache[xpath] = result;
                }
            }
            return result;
        }
    }
}
