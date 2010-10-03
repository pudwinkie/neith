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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.IO;
using System.Xml.Xsl;
using Autofac;
using Autofac.Builder;
using log4net;
using MindTouch.Deki.Data;
using MindTouch.Deki.Logic;
using MindTouch.Deki.PubSub;
using MindTouch.Deki.Script;
using MindTouch.Deki.Script.Runtime.Library;
using MindTouch.Deki.Search;
using MindTouch.Deki.WikiManagement;
using MindTouch.Dream;
using MindTouch.Dream.Services.PubSub;
using MindTouch.Tasking;
using MindTouch.Web;
using MindTouch.Xml;

namespace MindTouch.Deki {
    using Yield = IEnumerator<IYield>;

    [DreamService("MindTouch Core Service", "Copyright (c) 2006-2010 MindTouch Inc.",
       Info = "http://developer.mindtouch.com/en/ref/MindTouch_API",
        SID = new[] { 
            "sid://mindtouch.com/2006/11/dekiwiki",
            "http://services.mindtouch.com/deki/draft/2006/11/dekiwiki",
            "http://www.mindtouch.com/services/2006/11/dekiwiki"
        }
    )]
    [DreamServiceConfig("deki-db-server", "string?", "Database host name (default: localhost).")]
    [DreamServiceConfig("deki-db-port", "int?", "Database port (default: 3306).")]
    [DreamServiceConfig("deki-db-catalog", "string?", "Database table name (default: wikidb).")]
    [DreamServiceConfig("deki-db-user", "string?", "Database user name (default: wikiuser).")]
    [DreamServiceConfig("deki-db-password", "string", "Password for database user.")]
    [DreamServiceConfig("deki-db-options", "string", "Optional connection string parameters")]

    [DreamServiceConfig("deki-path", "string", "Application installation folder")]
    [DreamServiceConfig("deki-language", "string?", "Site language (default: \"en-US\").")]
    [DreamServiceConfig("deki-sitename", "string?", "Site name (default: \"MindTouch\").")]
    [DreamServiceConfig("admin-db-user", "string?", "Database administrator user name (default: \"root\").")]
    [DreamServiceConfig("admin-db-password", "string", "Database administrator password.")]
    [DreamServiceConfig("authtoken-salt", "string", "Private key used to generate unique auth tokens")]

    [DreamServiceConfig("deki-resources-path", "string?", "Path to resources folder (default: \"%deki-path%/resources\")")]
    [DreamServiceConfig("imagemagick-convert-path", "string", "Path to ImageMagick converter tool")]
    [DreamServiceConfig("imagemagick-identify-path", "string", "Path to ImageMagick identify tool")]
    [DreamServiceConfig("max-image-size", "int?", "Maximum supported image size in bytes or 0 for no limit (default: 0).")]
    [DreamServiceConfig("banned-words", "string?", "Comma separated list of banned words")]
    [DreamServiceBlueprint("setup/private-storage")]
    public partial class DekiWikiService : DekiExtService {

        //--- Constants ---
        public const string ANON_USERNAME = "Anonymous";
        public const string PARAM_PAGEID = "pageid";      //Represents the ID (int) of a page
        public const string PARAM_REDIRECTS = "redirects";
        internal const string PARAM_TITLE = "title";        //Represents the title of a page (full path)
        internal const string PARAM_FILEID = "fileid";      //Represents the ID (int) of a file attachment
        internal const string PARAM_FILENAME = "filename";  //Represents the filename of an attachment (without path info)
        internal const string PAGENOTFOUNDERROR = "Unable to find requested article";
        internal const string AUTHREALM = "DekiWiki";
        internal const string AUTHTOKEN_URIPARAM = "authtoken";
        internal const string AUTHTOKEN_COOKIENAME = "authtoken";
        internal const string AUTHTOKEN_HEADERNAME = "X-Authtoken";
        internal const string IMPERSONATE_USER_QUERYNAME = "impersonateuserid";
        internal const string WIKI_IDENTITY_HEADERNAME = "X-Deki-Site";
        internal const string DATA_STATS_HEADERNAME = "X-Data-Stats";
        private const string SID_FOR_LUCENE_INDEX = "http://services.mindtouch.com/deki/draft/2007/06/luceneindex";
        private const string SID_FOR_VARNISH_SERVICE = "sid://mindtouch.com/2009/01/varnish";
        public const string GRAVATAR_DEFAULT_PATH = "skins/common/images/default-avatar.png";

        //--- Class Fields ---
        private static readonly ILog _log = LogUtils.CreateLog();
        public static object SyncRoot = new object();
        public static PlainTextResourceManager ResourceManager;
        public static DekiFont ScreenFont;
        private static XslCompiledTransform _extensionRenderXslt;

        //--- Fields ---
        public Dictionary<XUri, XDoc> RemoteExtensionLibraries = new Dictionary<XUri, XDoc>();
        private InstanceManager _instanceManager;
        private NS[] _indexNamespaceWhitelist;
        private Plug _luceneIndex;
        private bool _isLocalLuceneService;
        private bool _isInitialized;
        private string _token;
        private LicenseData _license;
        private Plug _mailer;
        private Plug _pageSubscription;
        private Plug _packageUpdater;

        //--- Properties ---
        public InstanceManager Instancemanager { get { return _instanceManager; } }
        public override string AuthenticationRealm { get { return AUTHREALM; } }
        public string ImageMagickConvertPath { get { return Environment.ExpandEnvironmentVariables(Config["imagemagick-convert-path"].AsText ?? string.Empty); } }
        public string ImageMagickIdentifyPath { get { return Environment.ExpandEnvironmentVariables(Config["imagemagick-identify-path"].AsText ?? string.Empty); } }
        public string DekiPath { get { return Environment.ExpandEnvironmentVariables(Config["deki-path"].AsText); } }
        public string PrinceXmlPath { get { return Environment.ExpandEnvironmentVariables(Config["princexml-path"].AsText ?? string.Empty); } }
        public uint PrinceXmlTimeout { get { return Config["princexml-timeout"].AsUInt ?? 60000; } }
        public string PrinceXmlCssPath { get { return string.Format("{0}/skins/common/prince.css", DekiPath); } }
        public uint ImageMagickTimeout { get { return Config["imagemagick-timeout"].AsUInt ?? 30000; } }
        public string ResourcesPath { get { return Environment.ExpandEnvironmentVariables(Config["deki-resources-path"].AsText ?? Path.Combine(DekiPath, "resources")); } }
        public string MasterApiKey { get { return Config["api-key"].AsText ?? Config["apikey"].AsText; } }
        public NS[] IndexNamespaceWhitelist { get { return _indexNamespaceWhitelist; } }
        public Plug LuceneIndex { get { return _luceneIndex; } }
        public Plug Mailer { get { return _mailer; } }
        public Plug PageSubscription { get { return _pageSubscription; } }
        public Plug PackageUpdater { get { return _packageUpdater; } }

        internal LicenseData License {
            get { return _license; }
            set { _license = value; }
        }

        public override DreamFeatureStage[] Prologues {
            get {
                return new[] { 
                    new DreamFeatureStage("start-stats", this.PrologueStats, DreamAccess.Public),
                    new DreamFeatureStage("set-deki-context", this.PrologueDekiContext, DreamAccess.Public)
                };
            }
        }

        public override DreamFeatureStage[] Epilogues {
            get {
                return new[] {                    
                    new DreamFeatureStage("end-stats", this.EpilogueStats, DreamAccess.Public), 
                    new DreamFeatureStage("identify-instance",this.EpilogueIdentify, DreamAccess.Public), 
                };
            }
        }
        public override ExceptionTranslator[] ExceptionTranslators { get { return new ExceptionTranslator[] { MapDekiDataException }; } }

        internal string Token { get { return _token; } }

        internal DekiScriptRuntime InternalScriptRuntime { get { return ScriptRuntime; } }

        //--- Methods ---
        internal Result<Plug> InternalCreateService(string path, string sid, XDoc config, Result<Plug> result) {
            return CreateService(path, sid, config, result);
        }

        protected override Yield Start(XDoc config, IContainer container, Result result) {
            yield return Coroutine.Invoke(base.Start, config, new Result());

            // ensure imagemagick is setup correctly.            
            if(string.IsNullOrEmpty(ImageMagickConvertPath)) {
                throw new NotImplementedException("Please set 'imagemagick-convert-path' in config to path of ImageMagick's 'convert'");
            }
            if(!File.Exists(ImageMagickIdentifyPath)) {
                throw new FileNotFoundException("Cannot find ImagicMagick 'identify' binary: ", ImageMagickIdentifyPath);
            }
            if(string.IsNullOrEmpty(ImageMagickIdentifyPath)) {
                throw new NotImplementedException("Please set 'imagemagick-identify-path' in config to path of ImageMagick's 'identify'");
            }
            if(!File.Exists(ImageMagickConvertPath)) {
                throw new FileNotFoundException("Cannot find ImagicMagick 'convert' binary: ", ImageMagickConvertPath);
            }

            // check for 'apikey'
            if(string.IsNullOrEmpty(MasterApiKey)) {
                throw new ArgumentNullException("apikey", "The global apikey is not defined. Please ensure that you have a global <apikey> defined in the MindTouch Core service settings xml file.");
            }

            // TODO (arnec): how to prevent injection of replacements that have license checking removed,
            // but still allow tests to do it (check that the replacement comes from a signed assembly

            // make sure we have an ISearchBL registered
            var builder = new ContainerBuilder();
            builder.Register(c => DekiContext.Current).RequestScoped();
            if(!container.IsRegistered<ISearchBL>()) {
                builder.Register(c => {
                    var dekiContext = c.Resolve<DekiContext>();
                    var licenseState = LicenseBL.LicenseState;
                    return new SearchBL(

                        // Note (arnec): Need to figure out how to get the data session into the container
                        DbUtils.CurrentSession,
                        dekiContext.Instance.SearchCache,
                        dekiContext.Instance.Id,
                        dekiContext.Deki.Self.Uri,
                        dekiContext.Deki.LuceneIndex,
                        dekiContext.User,
                        new DekiInstanceSettings(),
                        new SearchQueryParser(),
                        () => (licenseState == LicenseStateType.TRIAL || licenseState == LicenseStateType.COMMERCIAL) && LicenseBL.GetCapability("search-engine") == "adaptive"
                    );
                }).As<ISearchBL>().RequestScoped();
            }
            builder.Build(container);

            // intialize instance manager
            _instanceManager = InstanceManager.New(this);

            // compute deki token
            ulong folded_productkey_md5 = 0;
            byte[] md5_bytes = StringUtil.ComputeHash(MasterApiKey, Encoding.UTF8);
            for(int i = 0; i < md5_bytes.Length; ++i) {
                folded_productkey_md5 ^= (ulong)md5_bytes[i] << (i & 7) * 8;
            }
            _token = folded_productkey_md5.ToString("X");

            // setup resource manager
            lock(SyncRoot) {
                if(ResourceManager == null) {
                    ResourceManager = new PlainTextResourceManager(ResourcesPath);
                    ScreenFont = new DekiFont(Plug.New("resource://mindtouch.deki/MindTouch.Deki.Resources.Arial.mtdf").Get().AsBytes());
                }
            }

            // initialize scripting engine
            XDoc scripting = Config["scripting"];
            DekiScriptLibrary.InsertTextLimit = scripting["max-web-response-length"].AsLong ?? DekiScriptLibrary.InsertTextLimit;
            DekiScriptLibrary.MinCacheTtl = scripting["min-web-cache-ttl"].AsDouble ?? DekiScriptLibrary.MinCacheTtl;

            // set up deki pub sub (by default we override uri.publish with our own service, unless @must-use=true is specified)
            if(!(Config["uri.publish/@must-use"].AsBool ?? false)) {
                Result<Plug> pubsubResult;
                XDoc pubsubConfig = new XDoc("config")
                    .Elem("uri.deki", Self.Uri.With("apikey", MasterApiKey))
                    .Start("downstream")
                        .Elem("uri", PubSub.At("publish").Uri.WithoutLastSegment().At("subscribers"))
                    .End()
                    .Start("components")
                        .Start("component")
                            .Attr("type", typeof(IPubSubDispatcher).AssemblyQualifiedName)
                            .Attr("implementation", typeof(DekiDispatcher).AssemblyQualifiedName)
                        .End()
                    .End()
                    .Elem("authtoken", MasterApiKey);
                foreach(var cookie in Cookies.Fetch(PubSub.Uri)) {
                    pubsubConfig.Add(cookie.AsSetCookieDocument);
                }
                yield return pubsubResult = CreateService(
                    "pubsub",
                    "sid://mindtouch.com/dream/2008/10/pubsub",
                    pubsubConfig,
                    new Result<Plug>());
                PubSub = pubsubResult.Value;
            }

            // set up package updater service (unless is was passed in)
            if(config["packageupdater/@uri"].IsEmpty) {
                var packageConfig = config["packageupdater"];
                packageConfig = packageConfig.IsEmpty ? new XDoc("config") : packageConfig.Clone();
                if(packageConfig["package-path"].IsEmpty) {
                    packageConfig.Elem("package-path", Path.Combine(Path.Combine(config["deki-path"].AsText, "packages"), "$1"));
                }
                yield return CreateService(
                    "packageupdater",
                    "sid://mindtouch.com/2010/04/packageupdater",
                    new XDoc("config")
                        .Elem("apikey", MasterApiKey)
                        .AddNodes(packageConfig),
                    new Result<Plug>()
                    );
            }

            // set up emailer service (unless it was passed in)
            XUri mailerUri;
            if(config["uri.mailer"].IsEmpty) {
                yield return CreateService(
                    "mailer",
                    "sid://mindtouch.com/2009/01/dream/email",
                    new XDoc("config")
                        .Elem("apikey", MasterApiKey)
                        .AddAll(Config["smtp/*"]),
                    new Result<Plug>()
                );
                mailerUri = Self.Uri.At("mailer");
            } else {
                mailerUri = config["uri.mailer"].AsUri;
            }
            _mailer = Plug.New(mailerUri);

            // set up the email subscription service (unless it was passed in)
            XUri pageSubscription;
            if(config["uri.page-subscription"].IsEmpty) {

                XDoc pagesubserviceConfig = new XDoc("config")
                    .Elem("uri.deki", Self.Uri)
                    .Elem("uri.emailer", mailerUri.At("message"))
                    .Elem("resources-path", ResourcesPath)
                    .Elem("apikey", MasterApiKey)
                    .AddAll(Config["page-subscription/*"]);
                foreach(var cookie in Cookies.Fetch(mailerUri)) {
                    pagesubserviceConfig.Add(cookie.AsSetCookieDocument);
                }
                yield return CreateService(
                    "pagesubservice",
                    "sid://mindtouch.com/deki/2008/11/changesubscription",
                    pagesubserviceConfig,
                    new Result<Plug>()
                );
                pageSubscription = Self.Uri.At("pagesubservice");
                config.Elem("uri.page-subscription", pageSubscription);
            } else {
                pageSubscription = config["uri.page-subscription"].AsUri;
            }
            _pageSubscription = Plug.New(pageSubscription);

            // set up package importer, if not provided
            XUri packageUpdater;
            if(config["uri.package"].IsEmpty) {
                yield return CreateService(
                    "package",
                    "sid://mindtouch.com/2009/07/package",
                    new XDoc("config").Elem("uri.deki", Self.Uri),
                    new Result<Plug>());
                packageUpdater = Self.Uri.At("package");
                config.Elem("uri.package", packageUpdater);
            } else {
                packageUpdater = config["uri.package"].AsUri;
            }
            _packageUpdater = Plug.New(packageUpdater);

            // set up lucene
            _luceneIndex = Plug.New(Config["indexer/@src"].AsUri);
            if(_luceneIndex == null) {

                // create the indexer service
                XDoc luceneIndexConfig = new XDoc("config").AddNodes(Config["indexer"]);
                if(MasterApiKey != null) {
                    luceneIndexConfig.Start("apikey").Attr("hidden", true).Value(MasterApiKey).End();
                }
                if(luceneIndexConfig["path.store"].IsEmpty) {
                    luceneIndexConfig.Elem("path.store", Path.Combine(Path.Combine(config["deki-path"].AsText, "luceneindex"), "$1"));
                }
                yield return CreateService("luceneindex", SID_FOR_LUCENE_INDEX, luceneIndexConfig, new Result<Plug>()).Set(v => _luceneIndex = v);
                _isLocalLuceneService = true;
            } else {

                // push our host's pubsub service to lucene, to keep it up to date on our changes
                var pubsub = new XDoc("pubsub").Attr("href", PubSub);
                foreach(var cookie in PubSub.CookieJar.Fetch(PubSub.Uri)) {
                    pubsub.Add(cookie.AsSetCookieDocument);
                }
                yield return _luceneIndex.At("subscriptions").PostAsync(pubsub);
            }

            _indexNamespaceWhitelist = new[] { NS.MAIN, NS.PROJECT, NS.USER, NS.TEMPLATE, NS.HELP, NS.MAIN_TALK, NS.PROJECT_TALK, NS.USER_TALK, NS.TEMPLATE_TALK, NS.HELP_TALK, NS.SPECIAL, NS.SPECIAL_TALK };
            if(!string.IsNullOrEmpty(Config["indexer/namespace-whitelist"].AsText)) {
                List<NS> customWhitelist = new List<NS>();
                foreach(string item in Config["indexer/namespace-whitelist"].AsText.Split(',')) {
                    NS ns;
                    if(SysUtil.TryParseEnum(item, out ns)) {
                        customWhitelist.Add(ns);
                    }
                }
                _indexNamespaceWhitelist = customWhitelist.ToArray();
            }

            if(!Config["wikis/globalconfig/cache/varnish"].IsEmpty) {
                // create the varnish service

                // TODO (petee): getting the varnish config from wikis/globalconfig/cache is a hack
                // The frontend needs to get the max-age to send out the cache headers but we currently have no way
                // of getting the DekiWikiService config so we'll hack it so it comes back in GET:site/settings.
                XDoc varnishConfig = new XDoc("config")
                    .Elem("uri.deki", Self.Uri.With("apikey", MasterApiKey))
                    .Elem("uri.varnish", Config["wikis/globalconfig/cache/varnish"].AsUri)
                    .Elem("varnish-purge-delay", Config["wikis/globalconfig/cache/varnish-purge-delay"].AsInt ?? 10)
                    .Elem("varnish-max-age", Config["wikis/globalconfig/cache/varnish-max-age"].AsInt ?? 300)
                    .Start("apikey").Attr("hidden", true).Value(MasterApiKey).End();
                yield return CreateService("varnish", SID_FOR_VARNISH_SERVICE, varnishConfig, new Result<Plug>());
            }
            _isInitialized = true;
            result.Return();
        }

        protected override Yield Stop(Result result) {
            _isInitialized = false;
            RemoteExtensionLibraries.Clear();
            if(_instanceManager != null) {
                _instanceManager.Shutdown();
            }
            if(_isLocalLuceneService) {
                _luceneIndex.DeleteAsync().Wait();
                _isLocalLuceneService = false;
            }
            _indexNamespaceWhitelist = null;
            _token = null;
            _mailer = null;
            yield return Coroutine.Invoke(base.Stop, new Result());
            result.Return();
        }

        private T Resolve<T>(DreamContext context) {
            var instance = context.Container.Resolve<T>();
            var public_key = instance.GetType().Assembly.GetName().GetPublicKey();
            if(public_key.Length <= 0) {
                throw new DreamAbortException(DreamMessage.InternalError("missing assembly signature"));
            }
            if(!public_key.SequenceEqual(GetType().Assembly.GetName().GetPublicKey())) {
                throw new DreamAbortException(DreamMessage.InternalError("signature mismatch for DekiWikiService"));
            }
            if(!public_key.SequenceEqual(typeof(DreamService).Assembly.GetName().GetPublicKey())) {
                throw new DreamAbortException(DreamMessage.InternalError("signature mismatch for DreamService"));
            }
            return instance;
        }

        internal void CheckResponseCache(DreamContext context, bool longTimeout) {
            DekiContext deki = DekiContext.Current;
            if(UserBL.IsAnonymous(deki.User) && deki.Instance.CacheAnonymousOutput) {
                string key = string.Format("{0}.{1}", deki.User.ID, context.Uri);
                CheckResponseCache(key);
                context.CacheKeyAndTimeout = new Tuplet<object, TimeSpan>(key, longTimeout ? deki.Instance.CacheAnonymousOutputLong : deki.Instance.CacheAnonymousOutputShort);
            }
        }

        internal void EmptyResponseCacheInternal() {
            EmptyResponseCache();
        }

        protected override DreamAccess DetermineAccess(DreamContext context, string key) {
            if(DekiContext.CurrentOrNull != null && DekiContext.Current.HasInstance) {

                //For features considered 'private' or 'internal', having a correct api-key or admin rights is required
                if(!string.IsNullOrEmpty(key) && (DekiContext.Current.Instance.ApiKey == key || MasterApiKey == key)) {
                    return DreamAccess.Internal;
                }
                if(PermissionsBL.IsUserAllowed(DekiContext.Current.User, Permissions.ADMIN)) {
                    return DreamAccess.Internal;
                }
            }
            return base.DetermineAccess(context, key);
        }

        protected override string TryGetServiceLicense(XUri sid) {
            string license;
            DateTime? expiration;
            TryGetServiceLicense(sid, out license, out expiration);
            return license;
        }

        internal bool TryGetServiceLicense(XUri sid, out string license, out DateTime? expiration) {
            license = null;
            expiration = null;
            if((sid != null) && (License.CurrentLicense != null)) {
                foreach(XDoc service in License.CurrentLicense["grants/service-license"]) {
                    string text = service.AsText;

                    // check if the licensed SID matches the requested SID
                    XUri licensedSID = service["@sid"].AsUri;
                    if(licensedSID == null) {

                        // parse service-license contents for the SID
                        Dictionary<string, string> values = HttpUtil.ParseNameValuePairs(text);

                        // check if the licensed SID matches the requested SID
                        string licensedSIDText;
                        if(values.TryGetValue("sid", out licensedSIDText) && XUri.TryParse(licensedSIDText, out licensedSID) && sid.HasPrefix(licensedSID)) {

                            // check if the licensed SID has an expiration date
                            string licenseExpireText;
                            DateTime licenseExpire;
                            if(values.TryGetValue("expire", out licenseExpireText) && DateTime.TryParse(licenseExpireText, out licenseExpire)) {
                                if(licenseExpire >= DateTime.UtcNow) {
                                    license = text;
                                    expiration = licenseExpire;
                                    return true;
                                }
                            } else {
                                license = text;
                                return true;
                            }
                            return false;
                        }
                    } else if(sid.HasPrefix(licensedSID)) {
                        DateTime? expire = service["@date.expire"].AsDate;

                        // check if the licensed SID has an expiration date
                        if((expire == null) || (expire >= DateTime.UtcNow)) {
                            license = text;
                            expiration = expire;
                            return true;
                        }
                        return false;
                    }
                }
            }
            return false;
        }

        private UserBE SetContextAndAuthenticate(DreamMessage request, uint serviceid, bool autoCreateExternalUser, bool allowAnon, out bool altPassword) {
            UserBE user = AuthBL.Authenticate(DreamContext.Current, request, serviceid, autoCreateExternalUser, allowAnon, out altPassword);
            user = UserBL.UpdateUserTimestamp(user);
            DekiContext.Current.User = user;

            // check that a user token is set (it might not be set if a user logs-in directly using HTTP authentication)
            if(!UserBL.IsAnonymous(user) && (DekiContext.Current.AuthToken == null)) {
                DekiContext.Current.AuthToken = AuthBL.CreateAuthTokenForUser(user);
            }
            BanningBL.PerformBanCheck();
            return user;
        }

        #region --- Prologues and Epilogues ---
        protected Yield PrologueDekiContext(DreamContext context, DreamMessage request, Result<DreamMessage> response) {

            // check if we need to skip this feature
            if(context.Feature.PathSegments.Length > 1 && context.Feature.PathSegments[context.Feature.ServiceUri.Segments.Length].StartsWith("@")) {
                response.Return(request);
                yield break;
            }

            // check if service has initialized
            if(!_isInitialized) {
                throw new DreamInternalErrorException("service not initialized");
            }

            //Build the dekicontext out of current request details and info from this wiki instance's details
            DekiInstance instance = _instanceManager.GetWikiInstance(request);
            DekiContext dekiContext = new DekiContext(this, instance, request);
            DreamContext.Current.SetState(dekiContext);

            // check if instance has already been initialized
            if(instance != null) {
                if(instance.Status == DekiInstanceStatus.CREATED) {
                    bool created;
                    try {
                        lock(instance) {
                            created = (instance.Status == DekiInstanceStatus.CREATED);
                            if(created) {

                                // initialize instance
                                instance.Startup(dekiContext);
                            }
                        }

                        // BUGBUGBUG (steveb): we startup the services AFTER the lock, because of race conditions, but this needs to be fixed
                        if(created) {
                            instance.StartServices();
                        }
                    } catch(Exception e) {
                        created = false;
                        instance.StatusDescription = "Initialization exception: " + e.GetCoroutineStackTrace();
                        instance.Log.Error("Error initializing instance", e);
                    }
                    if(created) {

                        // Note (arnec) this has to happen down here, since yield cannot exist inside a try/catch
                        // send instance settings to mailer
                        yield return Coroutine.Invoke(ConfigureMailer, ConfigBL.GetInstanceSettingsAsDoc(false), new Result()).CatchAndLog(_log);

                        // check whether we have an index
                        XDoc lucenestate = null;
                        yield return LuceneIndex.At("initstate").With("wikiid", instance.Id).Get(new Result<XDoc>()).Set(x => lucenestate = x);

                        // Note (arnec): defaulting to true, to avoid accidental re-index on false positive
                        if(!(lucenestate["@exists"].AsBool ?? true)) {
                            _log.DebugFormat("instance '{0}' doesn't have an index yet, forcing a rebuild", instance.Id);
                            yield return Self.At("site", "search", "rebuild").With("apikey", instance.ApiKey).Post(new Result<DreamMessage>());
                        }
                    }
                }
                instance.CheckInstanceIsReady();
                if(instance.Status == DekiInstanceStatus.ABANDONED) {

                    //If instance was abandoned (failed to initialize), error out.
                    throw new DreamInternalErrorException(string.Format("wiki '{0}' has failed to initialize or did not start up properly: {1}", instance.Id, instance.StatusDescription));
                }
            }

            // intialize culture/language + user
            if(instance != null) {

                // if not already started, start the database session for the current request 
                if(DbUtils.CurrentSession == null) {
                    DbUtils.CurrentSession = instance.SessionFactory.CreateSession();
                }

                if(context.Culture.IsNeutralCulture || context.Culture.Equals(System.Globalization.CultureInfo.InvariantCulture)) {
                    try {
                        context.Culture = new System.Globalization.CultureInfo(instance.SiteLanguage);
                    } catch {

                        // in case the site language is invalid, default to US English
                        context.Culture = new System.Globalization.CultureInfo("en-US");
                    }
                }
                if(!StringUtil.EqualsInvariantIgnoreCase(context.Feature.Signature, "users/authenticate")) {
                    bool allowAnon = StringUtil.EqualsInvariantIgnoreCase(context.Uri.GetParam("authenticate", "false"), "false");
                    bool altPassword;
                    SetContextAndAuthenticate(request, 0, false, allowAnon, out altPassword);
                }

                // TODO (steveb): we should update the culture based on the user's preferences
            }

            // continue processing
            response.Return(request);
            yield break;
        }

        private Yield PrologueStats(DreamContext context, DreamMessage request, Result<DreamMessage> response) {

            // initialize stopwatch timer
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            context.SetState("stats-stopwatch", sw);

            // continue processing
            response.Return(request);
            yield break;
        }

        private Yield EpilogueStats(DreamContext context, DreamMessage request, Result<DreamMessage> response) {

            // check if we need to skip this feature
            if(context.Feature.PathSegments.Length > 1 && context.Feature.PathSegments[context.Feature.ServiceUri.Segments.Length].StartsWith("@")) {
                response.Return(request);
                yield break;
            }

            // check if the epilogue was called without a deki instance (e.g. during initialization or with an invalid hostname)
            if(DekiContext.CurrentOrNull == null || !DekiContext.Current.HasInstance) {
                response.Return(request);
                yield break;
            }

            // compute execution time
            TimeSpan executionTime = TimeSpan.Zero;
            System.Diagnostics.Stopwatch elapsedTimeSw = context.GetState<System.Diagnostics.Stopwatch>("stats-stopwatch");
            if(elapsedTimeSw != null) {
                elapsedTimeSw.Stop();
                executionTime = TimeSpan.FromMilliseconds(elapsedTimeSw.ElapsedMilliseconds);
            }

            // increate instance hit counter
            DekiContext.Current.Instance.IncreasetHitCounter(request.IsSuccessful, executionTime);

            // if logging is enabled, grab the response text if the request was not successful
            string exception = string.Empty;
            XDoc activeConfig = DekiContext.Current.Instance.Config ?? Config;
            bool loggingEnabled = !String.IsNullOrEmpty(activeConfig["dblogging-conn-string"].AsText);
            if(loggingEnabled) {
                if(!request.IsSuccessful) {
                    exception = request.AsText();
                }
            }

            //Build overall request stats header
            StringBuilder statsHeaderSb = new StringBuilder();
            statsHeaderSb.AppendFormat("{0}={1}; ", "request-time-ms", (int)executionTime.TotalMilliseconds);

            //Append data stats
            IDekiDataStats sessionStats = DbUtils.CurrentSession as IDekiDataStats;
            Dictionary<string, string> stats;
            if(sessionStats != null) {
                stats = sessionStats.GetStats();
                if(stats != null) {
                    foreach(KeyValuePair<string, string> kvp in stats) {
                        statsHeaderSb.AppendFormat("{0}={1}; ", kvp.Key, kvp.Value);
                    }
                }
            }

            //Append context stats
            stats = DekiContext.Current.Stats;
            if(stats.Count > 0) {
                foreach(KeyValuePair<string, string> kvp in stats) {
                    statsHeaderSb.AppendFormat("{0}={1}; ", kvp.Key, kvp.Value);
                }
            }

            string requestStats = statsHeaderSb.ToString();
            request.Headers.Add(DATA_STATS_HEADERNAME, requestStats);
            DekiContext.Current.Instance.Log.InfoFormat("Finished [{0}:{1}] [{2}] {3}", context.Verb, context.Uri.Path, request.Status.ToString(), requestStats);


            // check if there is a catalog to record per-request information
            if(loggingEnabled) {
                try {
                    //Write request/response info to stats table after sending response back to client
                    UserBE u = DekiContext.Current.User;
                    string username = u == null ? string.Empty : u.Name;
                    DreamMessage initialRequest = DekiContext.Current.Request;
                    string hostheader = (initialRequest == null) ? string.Empty : initialRequest.Headers.Host ?? string.Empty;
                    DbUtils.CurrentSession.RequestLog_Insert(context.Uri, context.Verb, hostheader, context.Request.Headers.DreamOrigin, DekiContext.Current.Instance.Id, context.Feature.Signature, request.Status, username, (uint)executionTime.TotalMilliseconds, exception);
                } catch(Exception x) {
                    DekiContext.Current.Instance.Log.Error(string.Format("Failed to write request to db log. [Instance:{0}; Feature:{1}; Verb:{2}; Status:{3}; Duration:{4};]", DekiContext.Current.Instance.Id, context.Feature.Signature, context.Verb, (int)request.Status, executionTime), x);
                }
            }

            // continue processing
            response.Return(request);
            yield break;
        }

        private Yield EpilogueIdentify(DreamContext context, DreamMessage request, Result<DreamMessage> response) {

            // check if the epilogue was called without a deki instance (e.g. during initialization or with an invalid hostname)
            if(DekiContext.CurrentOrNull == null || !DekiContext.Current.HasInstance) {
                response.Return(request);
                yield break;
            }

            // end the database session for the current request
            if(null != DbUtils.CurrentSession) {
                DbUtils.CurrentSession.Dispose();
                DbUtils.CurrentSession = null;
            }

            // attach our wikiid
            request.Headers.Add(WIKI_IDENTITY_HEADERNAME, "id=" + StringUtil.QuoteString(DekiContext.Current.Instance.Id));
            response.Return(request);
            yield break;
        }

        #endregion

        private Yield ConfigureMailer(XDoc settings, Result result) {
            if(string.IsNullOrEmpty(settings["mail/smtp-servers"].AsText)) {
                yield return _mailer.At("configuration", DekiContext.Current.Instance.Id).DeleteAsync();
            } else {
                string port = settings["mail/smtp-port"].AsText;
                yield return _mailer.At("configuration", DekiContext.Current.Instance.Id).PutAsync(new XDoc("smtp")
                    .Elem("smtp-host", settings["mail/smtp-servers"].AsText)
                    .Elem("use-ssl", StringUtil.EndsWithInvariantIgnoreCase(settings["mail/smtp-secure"].AsText ?? string.Empty, "ssl") || StringUtil.EndsWithInvariantIgnoreCase(settings["mail/smtp-secure"].AsText ?? string.Empty, "tls"))
                    .Elem("smtp-port", string.IsNullOrEmpty(port) ? null : port)
                    .Elem("smtp-auth-user", settings["mail/smtp-username"].AsText)
                    .Elem("smtp-auth-password", settings["mail/smtp-password"].AsText));
            }
            result.Return();
            yield break;
        }

        private DreamMessage MapDekiDataException(DreamContext context, Exception exception) {
            return DekiExceptionMapper.Map(exception);
        }
    }
}
