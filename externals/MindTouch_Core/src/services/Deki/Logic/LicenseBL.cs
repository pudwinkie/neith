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
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

using MindTouch.Deki.Data;
using MindTouch.Deki.Util;
using MindTouch.Dream;
using MindTouch.Security.Cryptography;
using MindTouch.Tasking;
using MindTouch.Web;
using MindTouch.Xml;

namespace MindTouch.Deki.Logic {
    public enum LicenseStateType : byte {
        UNDEFINED,
        INVALID,
        INACTIVE,
        COMMUNITY,
        TRIAL,
        COMMERCIAL,
        EXPIRED
    };

    public abstract class MindTouchLicenseException : Exception { }
    public class MindTouchLicenseUserCreationException : MindTouchLicenseException { }
    public class MindTouchLicenseInstanceException : MindTouchLicenseException { }
    public class MindTouchLicenseInvalidOperationException : MindTouchLicenseException {
        
        //--- Fields ---
        public readonly string Operation;

        //--- Constructors ---
        public MindTouchLicenseInvalidOperationException(string operation) {
            Operation = operation;
        }
    }

    public class MindTouchLicenseTransitionException : MindTouchLicenseException {

        //--- Fields ---
        public readonly LicenseStateType CurrentState;
        public readonly LicenseStateType ProposedState;

        //--- Constructors ---
        public MindTouchLicenseTransitionException(LicenseStateType currentState, LicenseStateType proposedState) {
            CurrentState = currentState;
            ProposedState = proposedState;
        }
    }

    public class MindTouchTooManyUsersLicenseException : MindTouchLicenseException {

        //--- Fields ---
        public readonly uint CurrentActiveUsers;
        public readonly uint MaxUsers;
        public readonly uint UserDelta;

        //--- Constructors ---
        public MindTouchTooManyUsersLicenseException(uint currentActiveUsers, uint maxUsers, uint userDelta) {
            CurrentActiveUsers = currentActiveUsers;
            MaxUsers = maxUsers;
            UserDelta = userDelta;
        }
    }


    public class MindTouchNoNewUserLicenseException : MindTouchLicenseException {

        //--- Fields ---
        public readonly LicenseStateType CurrentLicenseState;

        //--- Constructors ---
        public MindTouchNoNewUserLicenseException(LicenseStateType currentLicenseState) {
            CurrentLicenseState = currentLicenseState;
        }
    }


    internal struct LicenseData {

        //--- Fields ---
        public readonly XDoc CurrentLicense;
        public readonly LicenseStateType LicenseState;
        public readonly DateTime LicenseStateChecked;
        public readonly DateTime LicenseExpiration;
        public readonly Permissions AnonymousPermissions;

        //--- Constructors ---
        public LicenseData(XDoc currentLicense, LicenseStateType licenseState, DateTime licenseExpiration, Permissions anonymousPermissions) {
            this.CurrentLicense = currentLicense;
            this.LicenseState = licenseState;
            this.LicenseStateChecked = DateTime.UtcNow;
            this.LicenseExpiration = licenseExpiration;
            this.AnonymousPermissions = anonymousPermissions;
        }
    }

    internal static class LicenseBL {

        //--- Constants ---
        private const int LICENSECHECKINTERVAL = 300; //seconds
        private const int GRACE_PERIOD = 14; // days
        public const string CONTENTRATING = "content-rating";
        public const string CONTENTRATING_ENABLED = "enabled";

        //--- Fields ---
        private static readonly log4net.ILog _log = LogUtils.CreateLog();
        private static readonly object _sync = new object();

        //--- Properties ---
        internal static LicenseStateType LicenseState { get { return DetermineCurrentLicenseState(); } }
        internal static DateTime LicenseExpiration { get { return DekiService.License.LicenseExpiration; } }
        private static Plug LicensePlug { get { return DekiService.Storage.At("license.xml"); } }
        private static DekiWikiService DekiService { get { return ((DekiWikiService)DreamContext.Current.Service); } }

        //--- Methods ---
        internal static string GetCapability(string name) {
            return DekiLicense.GetCapability(DekiService.License.CurrentLicense, name);
        }

        internal static void UpdateLicense(XDoc license) {

            // verify that new license is valid
            DekiLicense.Validate(license);

            // check if new license can be accepted. An exception is thrown for invalid transitions
            DateTime expiration;
            Permissions anonymousPermissions;
            LicenseStateType state = ValidateNewLicenseTransition(license, out expiration, out anonymousPermissions);

            lock(_sync) {

                //Save the license to Deki's storage service
                LicensePlug.Put(DreamMessage.Ok(license));

                // update in-memory license information
                DekiService.License = new LicenseData(license, state, expiration, anonymousPermissions);
                ConfigBL.ClearConfigCache();
            }
        }

        internal static bool IsUserCreationAllowed(bool throwException) {
            bool ret = true;
            uint? maxUsers = null;

            if (!IsLicenseValid()) {
                if (throwException) {
                    throw new MindTouchNoNewUserLicenseException(DetermineCurrentLicenseState());
                }
                ret = false;
            } else {

                //Active-users not defined implies unlimited users
                maxUsers = RetrieveLicense(false)["/license.private/grants/active-users"].AsUInt ?? uint.MaxValue;
            }

            uint currentUsers = DbUtils.CurrentSession.Users_GetCount();
            if (currentUsers >= (maxUsers ?? 0)) {
                ret = false;
                if (throwException) {
                    throw new MindTouchLicenseUserCreationException();
                }
            }

            return ret;
        }

        internal static bool IsDekiInstanceStartupAllowed(bool throwException) {
            bool ret = true;
            uint maxInstances;

            switch (DetermineCurrentLicenseState()) {
            case LicenseStateType.COMMERCIAL:

                //Full license may have a limit on number of instances
                maxInstances = RetrieveLicense(false)["/license.private/grants/active-sites"].AsUInt ?? uint.MaxValue;
                break;
            default:

                //Invalid and expired license allows starting any number of (invalid/expired) instances.                
                maxInstances = uint.MaxValue;
                break;
            }

            uint currentSites = ((DekiWikiService) DreamContext.Current.Service).Instancemanager.InstancesRunning;
            if (currentSites >= maxInstances) {
                ret = false;
                if (throwException) {
                    throw new MindTouchLicenseInstanceException();
                }
            }

            return ret;
        }

        internal static ulong LicensePermissionRevokeMask() {
            if (IsLicenseValid()) {
                return ~0UL;
            }
            return ~(ulong)PermissionSets.INVALID_LICENSE_REVOKE_LIST;
        }

        internal static ulong AnonymousUserMask(UserBE user) {
            if(!UserBL.IsAnonymous(user)) {
                return ~0UL;
            }
            return (ulong)DekiService.License.AnonymousPermissions;
        }

        internal static XDoc RetrieveCurrentLicense(bool publicOnly) {
            if(DekiService.License.CurrentLicense == null || DekiService.License.CurrentLicense.IsEmpty) {
                return XDoc.Empty;
            }
            return publicOnly ? DekiService.License.CurrentLicense["license.public"] : DekiService.License.CurrentLicense;
        }

        private static XDoc RetrieveLicense(bool publicOnly) {
            XDoc license = XDoc.Empty;

            // load license
            DreamMessage msg = LicensePlug.GetAsync().Wait();
            if (msg.IsSuccessful) {
                try {
                    license = msg.ToDocument();
                } catch(Exception x) {
                    _log.WarnExceptionFormat(x, "The commercial license could not be loaded");
                    license = XDoc.Empty;
                }
            }          

            // check if a license was found
            if(license.IsEmpty) {
                msg = Plug.New("resource://mindtouch.deki/MindTouch.Deki.Resources.license-community.xml").With(DreamOutParam.TYPE, MimeType.XML.ToString()).GetAsync().Wait();
                if(msg.IsSuccessful) {
                    try {
                        license = msg.ToDocument();
                    } catch (Exception x) {
                        _log.WarnExceptionFormat(x, "The community license could not be loaded");
                        license = XDoc.Empty;
                    }

                } else {

                    // unable to retrieve the license
                    _log.Warn("unable to retrieve the license built in community license");
                }
            }

            // check if only the public part is required
            if(publicOnly) {
                license = license["license.public"];
            }
            return license;
        }

        private static bool IsLicenseValid() {
            switch (DetermineCurrentLicenseState()) {
            case LicenseStateType.INVALID:
            case LicenseStateType.EXPIRED:
            case LicenseStateType.INACTIVE:
                return false;
            default:
                return true;
            }
        }

        private static LicenseStateType DetermineCurrentLicenseState() {
            LicenseData licenseData = DekiService.License;
            LicenseStateType state = licenseData.LicenseState;

            // Re-evaluate license after interval or if it's undefined
            if((state == LicenseStateType.UNDEFINED) || ((DateTime.UtcNow - DekiService.License.LicenseStateChecked).TotalSeconds > LICENSECHECKINTERVAL)) {
                lock(_sync) {
                    DateTime expiration;
                    XDoc license = RetrieveLicense(false);
                    Permissions anonymousPermissions;
                    DetermineLicenseState(license, out state, out expiration, out anonymousPermissions);
                    DekiService.License = new LicenseData(license, state, expiration, anonymousPermissions);
                }
            }
            return state;
        }

        private static void DetermineLicenseState(XDoc license, out LicenseStateType state, out DateTime expiration, out Permissions anonymousPermissions) {
            expiration = DateTime.MaxValue;
            state = LicenseStateType.INVALID;
            anonymousPermissions = PermissionSets.MINIMAL_ANONYMOUS_PERMISSIONS;

            // check if a valid license was passed in
            if((license == null) || license.IsEmpty) {
                return;
            }

            // check if the deki assembly is signed
            Assembly assembly = typeof(DekiWikiService).Assembly;
            if(ArrayUtil.IsNullOrEmpty(assembly.GetName().GetPublicKey())) {

                // no signature, default to community
                _log.Warn("Unable to validate signature of license since the MindTouch Core service was not signed by MindTouch. Reverting to community edition.");
                state = LicenseStateType.COMMUNITY;
                anonymousPermissions = PermissionSets.ALL;
                return;
            }

            // assembly is signed: validate xml signature
            RSACryptoServiceProvider rsa = RSAUtil.ProviderFrom(assembly);
            if((rsa == null) || !license.HasValidSignature(rsa)) {
                _log.Warn("License failed XML validation");
                state = LicenseStateType.INVALID;
                return;
            }

            // check license matched product key
            string productKey = license["licensee/product-key"].AsText;
            if((productKey != null) && !productKey.EqualsInvariantIgnoreCase(StringUtil.ComputeHashString(DekiService.MasterApiKey, Encoding.UTF8))) {
                _log.Warn("Invalid product-key in license");
                state = LicenseStateType.INVALID;
                return;
            }

            // determine license type
            switch(license["@type"].AsText ?? "inactive") {
            case "trial":
                state = LicenseStateType.TRIAL;
                break;
            case "inactive":
                state = LicenseStateType.INACTIVE;
                break;
            case "community":
                state = LicenseStateType.COMMUNITY;
                break;
            case "commercial":
                state = LicenseStateType.COMMERCIAL;
                break;
            default:
                _log.Warn("Unknown license type");
                state = LicenseStateType.INVALID;
                break;
            }

            // check expiration
            expiration = license["date.expiration"].AsDate ?? DateTime.MaxValue;
            if(state == LicenseStateType.COMMERCIAL) {

                // check if license is passed grace period
                if(expiration <= DateTime.UtcNow.AddDays(-GRACE_PERIOD)) {
                    state = LicenseStateType.EXPIRED;
                    return;
                }
            } else if(expiration <= DateTime.UtcNow) {
                state = LicenseStateType.EXPIRED;
                return;
            }

            // check version
            var licenseVersion = (license["version"].AsText ?? "*").Split('.');
            var assemblyVersion = typeof(LicenseBL).Assembly.GetName().Version;
            var appVersion = new[] { assemblyVersion.Major, assemblyVersion.Minor, assemblyVersion.Revision, assemblyVersion.Build };
            for(int i = 0; (i < licenseVersion.Length) && (i < appVersion.Length); ++i) {
                string pattern = licenseVersion[i];
                int value;
                if(!pattern.Equals("*") && (!int.TryParse(pattern, out value) || (value < appVersion[i]))) {
                    state = LicenseStateType.EXPIRED;
                    return;
                } 
            }

            // determine permissions for anonymous user
            anonymousPermissions = PermissionsBL.MaskFromString(DekiLicense.GetCapability(license, "anonymous-permissions")) | PermissionSets.MINIMAL_ANONYMOUS_PERMISSIONS;
        }

        private static LicenseStateType ValidateNewLicenseTransition(XDoc licenseDoc, out DateTime expiration, out Permissions anonymousPermissions) {

            /*
            * State transitions:
            *   community   -> community
            *   community   -> commercial
            *   trial       -> commercial
            *   trial       -> trial
            *   commercial  -> commercial
            *   expired     -> trial
            *   expired     -> commercial
            *   inactive    -> trial
            *   inactive    -> commercial
            *   invalid     -> *
            */

            // retrieve current license state
            LicenseStateType currentState = DetermineCurrentLicenseState();

            // retrieve desired license state
            LicenseStateType proposedState;
            DetermineLicenseState(licenseDoc, out proposedState, out expiration, out anonymousPermissions);

            bool exception = true;

            // check if the license transition is valid
            if (proposedState == LicenseStateType.INVALID) {

                // cannot switch to an invalid license                
                throw new DreamBadRequestException(DekiResources.LICENSE_UPDATE_INVALID);
            }
            if (proposedState == LicenseStateType.EXPIRED){

                // cannot switch to an expired license
                throw new DreamBadRequestException(string.Format(DekiResources.LICENSE_UPDATE_EXPIRED, expiration.ToString(DreamContext.Current.Culture)));
            }
            switch(currentState) {
            case LicenseStateType.COMMUNITY:
                switch(proposedState) {
                case LicenseStateType.COMMUNITY:
                case LicenseStateType.COMMERCIAL:
                    exception = false;
                    break;
                }
                break;
            case LicenseStateType.INACTIVE:
                switch(proposedState) {
                case LicenseStateType.COMMERCIAL:
                case LicenseStateType.TRIAL:
                    exception = false;
                    break;
                }
                break;
            case LicenseStateType.TRIAL:
                switch(proposedState) {
                case LicenseStateType.COMMERCIAL:
                case LicenseStateType.TRIAL:
                    exception = false;
                    break;
                }
                break;
            case LicenseStateType.COMMERCIAL:
                switch(proposedState) {
                case LicenseStateType.COMMERCIAL:
                    exception = false;
                    break;
                }
                break;
            case LicenseStateType.EXPIRED:
                switch(proposedState) {
                case LicenseStateType.TRIAL:
                case LicenseStateType.COMMERCIAL:
                    exception = false;
                    break;
                }
                break;
            case LicenseStateType.INVALID:
                exception = false;
                break;
            default:
                throw new DreamBadRequestException(DekiResources.LICENSE_UPDATE_INVALID);
            }

            // verify that this license of for this installations
            if (exception) {
                throw new MindTouchLicenseTransitionException(currentState, proposedState);
            }

            //Extra validation when updating or transitioning to commerical license
            if(proposedState == LicenseStateType.COMMERCIAL) {

                //user count
                uint? maxUsers = licenseDoc["/license.private/grants/active-users"].AsUInt;
                if(maxUsers != null) {
                        
                    //Reject license if its user limit is lower than current number of users
                    uint currentActiveUsers = DbUtils.CurrentSession.Users_GetCount();
                    if(currentActiveUsers > maxUsers.Value) {
                        uint userDelta = currentActiveUsers - maxUsers.Value;
                        throw new MindTouchTooManyUsersLicenseException(currentActiveUsers, maxUsers.Value, userDelta);
                    }
                }
            }
            return proposedState;
        }
    }
}
