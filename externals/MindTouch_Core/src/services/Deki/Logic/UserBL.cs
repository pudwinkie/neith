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
using System.Linq;
using System.Text.RegularExpressions;

using MindTouch.Deki.Data;
using MindTouch.Dream;
using MindTouch.Xml;

namespace MindTouch.Deki.Logic {
    public static class UserBL {

        public static UserBE GetAdmin() {
            return GetUserById(1);
        }

        public static PageBE GetHomePage(UserBE user) {
            return PageBL.GetPageByTitle(Title.FromUIUsername(user.Name));
        }

        public static UserBE CreateNewUser(UserBE newUser) {
            if(newUser == null)
                return null;

            //throw exception if licensing does not allow creation of another user
            LicenseBL.IsUserCreationAllowed(true);

            if(newUser.RoleId == 0) {
                RoleBE defaultRole = PermissionsBL.RetrieveDefaultRoleForNewAccounts();
                if(defaultRole != null)
                    newUser.RoleId = defaultRole.ID;
            }

            ValidateUser(newUser);
            newUser.CreateTimestamp = DateTime.UtcNow;
            uint userId = DbUtils.CurrentSession.Users_Insert(newUser);
            if(userId == 0) {
                return null;
            }

            newUser.ID = userId;

            PageBE userHomepage = null;
            try {
                // User homepages are created upon user creation (an attempt to create user homepages may also be done upon login)
                userHomepage = PageBL.CreateUserHomePage(newUser);
            } catch { }

            RecentChangeBL.AddUserCreatedRecentChange(DateTime.UtcNow, userHomepage, DekiContext.Current.User ?? newUser, String.Format(DekiResources.USER_ADDED, newUser.Name));
            DekiContext.Current.Instance.EventSink.UserCreate(DreamContext.Current.StartTime, newUser);


            return newUser;
        }

        public static void UpdateUser(UserBE user) {

            // Note (maxm): The user 'touched' timestamp is updated:
            // * at authentication time
            // * any user object changes (including group membership)
            // It's exposed in the user xml as 'date.lastlogin'
            user.Touched = DateTime.UtcNow;

            ValidateUser(user);
            DbUtils.CurrentSession.Users_Update(user);

            DekiContext.Current.Instance.EventSink.UserUpdate(DreamContext.Current.StartTime, user);
        }

        public static UserBE CreateOrUpdateUser(UserBE user) {
            return CreateOrUpdateUser(user, null);
        }

        public static UserBE CreateOrUpdateUser(UserBE user, string newPassword) {

            if(user.ID > 0) {
                UpdateUser(user);
            } else {
                //TODO consider logic here to confirm that the user does not yet exist.

                user = CreateNewUser(user);
            }

            if(!string.IsNullOrEmpty(newPassword) && ServiceBL.IsLocalAuthService(user.ServiceId)) {
                user = UserBL.SetPassword(user, newPassword, false);
                DekiContext.Current.Instance.EventSink.UserChangePassword(DreamContext.Current.StartTime, user);
            }

            return user;
        }

        public static void UpdateUsersGroups(UserBE user, GroupBE[] groups) {
            if(user == null || groups == null)
                return;

            IList<GroupBE> groupsWithIds = DbUtils.CurrentSession.Groups_GetByNames(groups.Select(e => e.Name).ToList());
            DbUtils.CurrentSession.GroupMembers_UpdateGroupsForUser(user.ID, groupsWithIds.Select(e => e.Id).ToList());
            UpdateUser(user);
        }

        public static string NormalizeExternalNameToWikiUsername(string externalUserName) {
            uint suffix = 0;
            UserBE userWithMatchingName = null;
            string newUserName;
            do {
                newUserName = Title.FromUIUsername(externalUserName).Path;
                if(suffix > 0)
                    newUserName = newUserName + suffix.ToString();

                userWithMatchingName = DbUtils.CurrentSession.Users_GetByName(newUserName);
                suffix++;
            } while(userWithMatchingName != null);

            return newUserName;
        }


        public static void ValidateUser(UserBE user) {
            if(string.IsNullOrEmpty(user.Name) || user.Name.EndsWith(".", true, CultureInfo.InvariantCulture) || !Title.FromUIUsername(user.Name).IsValid)
                throw new DreamBadRequestException(string.Format(DekiResources.USER_VALIDATION_FAILED, user.Name));
        }

        /// <summary>
        /// Updates the given user in the db with current timestamp
        /// </summary>
        /// <param name="user"></param>
        public static UserBE UpdateUserTimestamp(UserBE user) {
            if(user == null)
                return null;

            //Update user's last logged time column to now if it's more than a minute old.
            if(user.Touched <= DateTime.UtcNow.AddMinutes(-1) && user.UserActive) {

                // Note (maxm): This does not call UserBL.UserUpdate to avoid frequent reindexing.
                user.Touched = DateTime.UtcNow;
                DbUtils.CurrentSession.Users_Update(user);
            }
            return user;
        }

        public static UserBE PostUserFromXml(XDoc userDoc, UserBE userToProcess, string accountpassword, string externalusername, string externalpassword) {
            List<GroupBE> externalGroups = null;
            uint? id;
            bool? active;
            string username, fullname, email, language, timezone;
            ServiceBE authService;
            RoleBE role;

            //Parse the standard user XML doc
            ParseUserXml(userDoc, out id, out username, out email, out fullname, out authService, out role, out active, out language, out timezone);

            //new user
            if(userToProcess == null && (id == null || id == 0)) {
                userToProcess = ReadUserXml(userDoc, username, email, fullname, authService, role, language, timezone);

                //External accounts should be confirmed, username normalized, groups retrieved
                if(authService != null && !ServiceBL.IsLocalAuthService(authService)) {

                    if(!string.IsNullOrEmpty(accountpassword))
                        throw new DreamBadRequestException(DekiResources.CANNOT_SET_EXTERNAL_ACCOUNT_PASSWORD);

                    //Only admins can create external accounts for others. Anyone can create their own external account
                    if(externalusername != userToProcess.Name || externalusername == string.Empty)
                        PermissionsBL.CheckUserAllowed(DekiContext.Current.User, Permissions.ADMIN);

                    //username+password from request query params are used here
                    userToProcess = ExternalServiceSA.BuildUserFromAuthService(authService, userToProcess, username, true, externalusername, externalpassword, out externalGroups);

                    if(userToProcess == null) {
                        throw new DreamAbortException(DreamMessage.NotFound(string.Format(DekiResources.EXTERNAL_USER_NOT_FOUND, username)));
                    }

                    //Does the external account already exist?
                    UserBE matchingExternalAccount = DbUtils.CurrentSession.Users_GetByExternalName(userToProcess.ExternalName, userToProcess.ServiceId);
                    if(matchingExternalAccount != null) {
                        throw new DreamAbortException(DreamMessage.Conflict(string.Format(DekiResources.USER_EXISTS_WITH_EXTERNAL_NAME, matchingExternalAccount.Name, userToProcess.ExternalName, userToProcess.ServiceId)));
                    }
                } else { //Creating local account

                    //User creation requires admin rights unless the config flag allows it
                    //Anonymous users are not allowed to set role
                    if(!DekiContext.Current.Instance.AllowAnonymousLocalAccountCreation || role != null) {
                        PermissionsBL.CheckUserAllowed(DekiContext.Current.User, Permissions.ADMIN);
                    }
                }

                //Sanity check for already existing user
                UserBE existingUser = DbUtils.CurrentSession.Users_GetByName(userToProcess.Name);
                if(existingUser != null) {
                    throw new DreamAbortException(DreamMessage.Conflict(string.Format(DekiResources.USER_EXISTS_WITH_ID, existingUser.Name, existingUser.ID)));
                }

                //if (UserDA.RetrieveUserRegistrations(userToProcess.Name)) {
                //    throw new DreamAbortException(DreamMessage.Conflict(string.Format("User '{0}' has been reserved", userToProcess.Name)));
                //}

                userToProcess = UserBL.CreateOrUpdateUser(userToProcess, accountpassword);
                if(null != externalGroups) {
                    UserBL.UpdateUsersGroups(userToProcess, externalGroups.ToArray());
                }
            }

            //update existing user
            else {

                if(userToProcess == null) {

                    //Modifying a user with POST
                    if(id == null || id == 0) {
                        throw new DreamAbortException(DreamMessage.BadRequest(DekiResources.USER_ID_ATTR_INVALID));
                    }

                    userToProcess = UserBL.GetUserById(id.Value);
                    if(userToProcess == null) {
                        throw new DreamAbortException(DreamMessage.NotFound(string.Format(DekiResources.USER_ID_NOT_FOUND, id)));
                    }
                }

                if(!string.IsNullOrEmpty(accountpassword))
                    throw new DreamBadRequestException(DekiResources.USE_PUT_TO_CHANGE_PASSWORDS);

                //Permission check if not modifying self
                if(userToProcess.ID != DekiContext.Current.User.ID)
                    PermissionsBL.CheckUserAllowed(DekiContext.Current.User, Permissions.ADMIN);

                userToProcess = UpdateUserFromXml(userToProcess, userDoc, username, email, fullname, authService, role, active, externalusername, externalpassword, language, timezone, out externalGroups);
                userToProcess = CreateOrUpdateUser(userToProcess);
                if(null != externalGroups) {
                    UserBL.UpdateUsersGroups(userToProcess, externalGroups.ToArray());
                }

                if(UserBL.IsAnonymous(userToProcess) && DekiContext.Current.Instance.CacheAnonymousOutput) {
                    DekiContext.Current.Deki.EmptyResponseCacheInternal();
                }
            }

            return userToProcess;
        }

        private static UserBE UpdateUserFromXml(UserBE userToUpdate, XDoc userDoc, string username, string email, string fullname, ServiceBE authservice, RoleBE role, bool? active, string externalusername, string externalpassword, string language, string timezone, out List<GroupBE> externalGroups) {
            externalGroups = null;
            if(userToUpdate.Name != username && !string.IsNullOrEmpty(username)) {
                if(UserBL.IsAnonymous(userToUpdate)) {
                    throw new DreamBadRequestException(DekiResources.ANONYMOUS_USER_EDIT);
                }
                userToUpdate = RenameUser(userToUpdate, username);
            }

            //Modify a user's authentication service
            if(authservice != null && authservice.Id != userToUpdate.ServiceId) {
                if(UserBL.IsAnonymous(userToUpdate)) {
                    throw new DreamBadRequestException(DekiResources.ANONYMOUS_USER_EDIT);
                }

                if(ServiceBL.IsLocalAuthService(authservice)) {

                    //external to local
                    userToUpdate.ExternalName = null;
                    userToUpdate.ServiceId = authservice.Id;

                } else {

                    //(local or external) to external
                    userToUpdate = ExternalServiceSA.BuildUserFromAuthService(authservice, userToUpdate, userToUpdate.Name, true, externalusername, externalpassword, out externalGroups);
                    if(userToUpdate == null) {
                        throw new DreamInternalErrorException(DekiResources.USER_AUTHSERVICE_CHANGE_FAIL);
                    }

                    //Does the external account already exist?
                    UserBE matchingExternalAccount = DbUtils.CurrentSession.Users_GetByExternalName(userToUpdate.ExternalName, userToUpdate.ServiceId);
                    if(matchingExternalAccount != null) {
                        throw new DreamAbortException(DreamMessage.Conflict(string.Format(DekiResources.USER_EXISTS_WITH_EXTERNAL_NAME, matchingExternalAccount.Name, matchingExternalAccount.ExternalName, matchingExternalAccount.ServiceId)));
                    }
                }
            }

            if(email != null) {
                if(UserBL.IsAnonymous(userToUpdate) && email != userToUpdate.Email) {
                    throw new DreamBadRequestException(DekiResources.ANONYMOUS_USER_EDIT);
                }

                userToUpdate.Email = email;
            }

            if(!string.IsNullOrEmpty(fullname))
                userToUpdate.RealName = fullname;

            if(active != null) {
                if(UserBL.IsAnonymous(userToUpdate) && userToUpdate.UserActive && !active.Value) {
                    throw new DreamBadRequestException(DekiResources.DEACTIVATE_ANONYMOUS_NOT_ALLOWED);
                }

                //throw exception if licensing does not allow activating a user
                if(!userToUpdate.UserActive && active.Value) {
                    LicenseBL.IsUserCreationAllowed(true);
                }
                userToUpdate.UserActive = active.Value;
            }

            if(role != null && role.ID != userToUpdate.RoleId) {
                PermissionsBL.CheckUserAllowed(DekiContext.Current.User, Permissions.ADMIN);
                userToUpdate.RoleId = role.ID;
            }

            if(language != null) {
                userToUpdate.Language = language;
            }

            if(timezone != null) {
                userToUpdate.Timezone = timezone;
            }

            return userToUpdate;
        }

        private static Regex timeZoneRegex = new Regex(@"([\+\-]?\d\d\:\d\d)", RegexOptions.Compiled);

        public static IList<UserBE> GetUsersByQuery(DreamContext context, uint? groupId, out uint totalCount, out uint queryCount) {
            uint limit, offset;
            SortDirection sortDir;
            string sortFieldString;
            Utils.GetOffsetAndCountFromRequest(context, 100, out limit, out offset, out sortDir, out sortFieldString);

            // Attempt to read the sort field.  If a parsing error occurs, default to undefined.
            UsersSortField sortField = UsersSortField.UNDEFINED;
            if(!String.IsNullOrEmpty(sortFieldString)) {
                try { sortField = SysUtil.ChangeType<UsersSortField>(sortFieldString.Replace('.', '_')); } catch { }
            }
            uint? serviceid = context.GetParam<uint>("authprovider", 0);
            if((serviceid ?? 0) == 0) {
                serviceid = null;
            }

            string usernamefilter = context.GetParam("usernamefilter", null);
            string realnamefilter = context.GetParam("fullnamefilter", null);
            string usernameemailfilter = context.GetParam("usernameemailfilter", null);
            string rolefilter = context.GetParam("rolefilter", null);
            bool? activatedfilter = null;
            bool parsedActivatedFilter;
            if(bool.TryParse(context.GetParam("activatedfilter", null), out parsedActivatedFilter)) {
                activatedfilter = parsedActivatedFilter;
            }

            return DbUtils.CurrentSession.Users_GetByQuery(usernamefilter, realnamefilter, usernameemailfilter, rolefilter, activatedfilter, groupId, serviceid, sortDir, sortField, offset, limit, out totalCount, out queryCount);
        }

        public static UserBE GetUserById(uint userId) {
            return DbUtils.CurrentSession.Users_GetByIds(new List<uint>() { userId }).FirstOrDefault();
        }

        public static UserBE GetUserByName(string name) {
            return DbUtils.CurrentSession.Users_GetByName(name);
        }

        public static UserBE SetPassword(UserBE user, string password, bool altPassword) {
            string pwhash = Logic.AuthBL.EncryptPassword(user, password);

            if(altPassword) {

                //Set the alternate password only while keeping the main password untouched.
                user.NewPassword = pwhash;
            } else {

                //Set the main password and clear the alternate password.
                user.Password = pwhash;
                user.NewPassword = string.Empty;
            }

            user.Touched = DateTime.UtcNow;
            DbUtils.CurrentSession.Users_Update(user);
            return user;
        }

        private static UserBE RenameUser(UserBE user, string newUserName) {

            //Renaming requires admin rights.
            PermissionsBL.CheckUserAllowed(DekiContext.Current.User, Permissions.ADMIN);

            if(!ServiceBL.IsLocalAuthService(user.ServiceId)) {

                //TODO MaxM: allow renaming of external users
                throw new DreamAbortException(DreamMessage.NotImplemented(DekiResources.EXTERNAL_USER_RENAME_NOT_ALLOWED));
            }

            //Check for already existing user with same name
            UserBE existingUser = DbUtils.CurrentSession.Users_GetByName(newUserName);
            if(existingUser != null) {
                throw new DreamAbortException(DreamMessage.Conflict(string.Format(DekiResources.USER_EXISTS_WITH_ID, existingUser.Name, existingUser.ID)));
            }

            PageBE existingTargetUserHomePage = PageBL.GetPageByTitle(Title.FromUIUsername(newUserName));
            if(existingTargetUserHomePage != null && existingTargetUserHomePage.ID != 0) {
                throw new DreamAbortException(DreamMessage.Conflict(DekiResources.USER_RENAME_HOMEPAGE_CONFLICT));
            }

            //Try to move the homepage.
            PageBE userHomePage = GetHomePage(user);
            if(userHomePage != null && userHomePage.ID != 0) {
                PageBL.MovePage(userHomePage, Title.FromUIUsername(newUserName));
            }

            //Rename the user
            user.Name = newUserName;
            UserBL.UpdateUser(user);
            return user;
        }

        public static bool IsAnonymous(UserBE user) {
            return user.Name == DekiWikiService.ANON_USERNAME;
        }

        #region Uris
        public static XUri GetUri(UserBE user) {
            return DekiContext.Current.ApiUri.At("users", user.ID.ToString());
        }

        public static XUri GetUriUiHomePage(UserBE user) {
            return XUri.TryParse(Utils.AsPublicUiUri(Title.FromDbPath(NS.USER, user.Name, null)));
        }
        #endregion

        #region XML Helpers

        private static UserBE ReadUserXml(XDoc userDoc, string username, string email, string fullname, ServiceBE authService, RoleBE role, string language, string timezone) {

            UserBE user = new UserBE();

            if(string.IsNullOrEmpty(username))
                throw new DreamBadRequestException(DekiResources.USERNAME_PARAM_INVALID);

            //TODO (MaxM) Consider validation of fullname, email, username

            //Retrieve default auth service for new user if authservice not given
            if(authService == null) {
                authService = ServiceBL.RetrieveLocalAuthService();
            }

            user.Name = username;

            //Default role will be applied if one is not given
            if(role != null)
                user.RoleId = role.ID;

            user.RealName = fullname ?? string.Empty;
            user.ServiceId = authService.Id;
            user.UserActive = true;
            user.Email = email ?? string.Empty;
            user.Language = language;
            user.Timezone = timezone;

            return user;
        }

        private static void ParseUserXml(XDoc userDoc, out uint? id, out string username, out string email, out string fullname, out ServiceBE authService, out RoleBE role, out bool? active, out string language, out string timezone) {

            username = userDoc["username"].AsText;
            email = userDoc["email"].AsText;
            fullname = userDoc["fullname"].AsText;
            language = userDoc["language"].AsText;
            timezone = userDoc["timezone"].AsText;
            string authserviceidstr = userDoc["service.authentication/@id"].AsText;
            string rolestr = userDoc["permissions.user/role"].AsText;
            string statusStr = userDoc["status"].AsText;
            authService = null;
            role = null;

            id = null;

            if(!userDoc["@id"].IsEmpty) {
                uint id_temp;
                if(!uint.TryParse(userDoc["@id"].Contents, out id_temp))
                    throw new DreamBadRequestException(DekiResources.USER_ID_ATTR_INVALID);
                id = id_temp;
            }

            if(!string.IsNullOrEmpty(authserviceidstr)) {
                uint serviceid;
                if(!uint.TryParse(authserviceidstr, out serviceid))
                    throw new DreamBadRequestException(DekiResources.SERVICE_AUTH_ID_ATTR_INVALID);

                authService = ServiceBL.GetServiceById(serviceid);
                if(authService == null)
                    throw new DreamBadRequestException(string.Format(DekiResources.SERVICE_DOES_NOT_EXIST, serviceid));
            }

            if(!string.IsNullOrEmpty(rolestr)) {
                role = PermissionsBL.GetRoleByName(rolestr);
                if(role == null)
                    throw new DreamBadRequestException(string.Format(DekiResources.ROLE_DOES_NOT_EXIST, rolestr));
            }

            if(!string.IsNullOrEmpty(statusStr)) {
                switch(statusStr.ToLowerInvariant()) {
                case "active":
                    active = true;
                    break;
                case "inactive":
                    active = false;
                    break;
                default:
                    throw new DreamBadRequestException(DekiResources.USER_STATUS_ATTR_INVALID);
                }
            } else {
                active = null;
            }

            if(!string.IsNullOrEmpty(timezone)) {
                if(!timeZoneRegex.Match(timezone).Success) {
                    throw new DreamBadRequestException(DekiResources.INVALID_TIMEZONE_VALUE);
                }
            }

            if(!string.IsNullOrEmpty(language)) {
                string[] validLanguages = DekiContext.Current.Instance.Languages.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string tempLanguage = language;
                if(!Array.Exists<string>(validLanguages, delegate(string temp) { return StringUtil.EqualsInvariantIgnoreCase(temp, tempLanguage); })) {
                    throw new DreamBadRequestException(DekiResources.INVALID_LANGUAGE_VALUE);
                }
            }
        }

        public static XDoc GetUserXml(UserBE user, string relation, bool showPrivateInfo) {
            XDoc userXml = new XDoc(string.IsNullOrEmpty(relation) ? "user" : "user." + relation);
            userXml.Attr("id", user.ID);
            userXml.Attr("href", DekiContext.Current.ApiUri.At("users", user.ID.ToString()));
            userXml.Elem("nick", user.Name);
            userXml.Elem("username", user.Name);
            userXml.Elem("fullname", user.RealName ?? String.Empty);

            // check if we can add the email address
            if(showPrivateInfo) {
                userXml.Elem("email", user.Email);
            } else {
                userXml.Start("email").Attr("hidden", true).End();
            }

            // add gravatar
            if(!IsAnonymous(user) && !string.IsNullOrEmpty(user.Email)) {
                DekiContext context = DekiContext.CurrentOrNull;
                XUri gravatar = new XUri("http://www.gravatar.com/avatar");
                string hash = string.Empty;
                if(context != null) {
                    DekiInstance deki = context.Instance;
                    string secure = context.Instance.GravatarSalt ?? string.Empty;
                    if(!secure.EqualsInvariantIgnoreCase("hidden")) {
                        hash = StringUtil.ComputeHashString(secure + (user.Email ?? string.Empty).Trim().ToLowerInvariant(), System.Text.Encoding.UTF8);
                    }

                    // add size, if any
                    string size = deki.GravatarSize;
                    if(size != null) {
                        gravatar = gravatar.With("s", size);
                    }

                    // add rating, if any
                    string rating = deki.GravatarRating;
                    if(rating != null) {
                        gravatar = gravatar.With("r", rating);
                    }

                    // add default icon, if any
                    string def = deki.GravatarDefault;
                    if(def != null) {
                        gravatar = gravatar.With("d", def);
                    }
                }
                if(!string.IsNullOrEmpty(hash)) {
                    userXml.Elem("hash.email", hash);
                    userXml.Elem("uri.gravatar", gravatar.At(hash + ".png"));
                } else {
                    userXml.Elem("hash.email", string.Empty);
                    userXml.Elem("uri.gravatar", gravatar.At("no-email.png"));
                }
            }
            return userXml;
        }

        public static XDoc GetUserXmlVerbose(UserBE user, string relationAttr, bool showPrivateInfo) {
            XDoc userXml = GetUserXml(user, relationAttr, showPrivateInfo);

            userXml.Elem("date.created", user.CreateTimestamp);

            PageBE homePage = GetHomePage(user);
            if(homePage != null && homePage.ID != 0)
                userXml.Add(PageBL.GetPageXml(homePage, "home"));

            userXml.Start("status").Value(user.UserActive ? "active" : "inactive").End();
            userXml.Start("date.lastlogin").Value(user.Touched).End();
            userXml.Start("language").Value(user.Language).End();
            userXml.Start("timezone").Value(user.Timezone).End();

            ServiceBE authService = ServiceBL.GetServiceById(user.ServiceId);
            if(authService != null)
                userXml.Add(ServiceBL.GetServiceXml(authService, "authentication"));

            //Permissions for the user from user role
            userXml.Add(PermissionsBL.GetRoleXml(PermissionsBL.GetRoleById(user.RoleId), "user"));

            ulong effectivePermissions = PermissionsBL.CalculateEffectiveUserRights(user);

            //Effective permissions for the user from the role + group roles.
            userXml.Add(PermissionsBL.GetPermissionXml(effectivePermissions, "effective"));

            userXml.Start("groups");
            IList<GroupBE> groups = DbUtils.CurrentSession.Groups_GetByUser(user.ID);
            if(null != groups) {
                foreach(GroupBE g in groups) {
                    userXml.Add(GroupBL.GetGroupXmlVerbose(g, null));
                }
            }
            userXml.End();

            //Retrieve properties for current user while providing an href for other users.            
            if(DekiContext.Current.User.ID == user.ID) {

                IList<PropertyBE> props = PropertyBL.Instance.GetResources(user.ID, ResourceBE.Type.USER);
                userXml = PropertyBL.Instance.GetPropertyXml(props, GetUri(user), null, null, userXml);
            } else {
                userXml.Start("properties").Attr("href", GetUri(user).At("properties")).End();
            }

            // TODO Max: get <subscriptions> (watchlist) not implemented
            return userXml;
        }
        #endregion
    }
}
