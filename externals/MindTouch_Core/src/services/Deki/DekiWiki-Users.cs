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

using MindTouch.Deki.Data;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Web;
using MindTouch.Xml;

namespace MindTouch.Deki {
    using Yield = IEnumerator<IYield>;
    using MindTouch.Deki.Logic;

    public partial class DekiWikiService {

        //--- Features ---
        [DreamFeature("POST:users/authenticate", "Authenticate a user given http header Credentials or an auth token. When using external authentication, this will automatically create an account and synchronize groups. Response status 200 implies valid credentials and contains a new auth token.")]
        [DreamFeature("GET:users/authenticate", "Authenticate a user given http header Credentials or an auth token. Response status 200 implies valid credentials and contains a new auth token.")]
        [DreamFeatureParam("authprovider", "int?", "Identifier for the external service to use for authentication.")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Login access is required")]
        [DreamFeatureStatus(DreamStatus.Conflict, "Username conflicts with an existing username")] 
        [DreamFeatureStatus(DreamStatus.Unauthorized, "Authentication failed")]
        public Yield PostUserAuth(DreamContext context, DreamMessage request, Result<DreamMessage> response) {

            uint serviceId = context.GetParam<uint>("authprovider", 0);
            bool altPassword = false;
            //This will internally fail with a 501 response if credentials are invalid.
            //Anonymous accounts (no credentials/authtoken) are not allowed -> 401
            UserBE u = SetContextAndAuthenticate(request, serviceId, context.Verb == Verb.POST, false, out altPassword);
            PermissionsBL.CheckUserAllowed(u, Permissions.LOGIN);
            
            string token = AuthBL.CreateAuthTokenForUser(u);
            
            try {
                PageBL.CreateUserHomePage(DekiContext.Current.User);
            }
            catch {}

            DreamMessage ret = BuildSetAuthTokenResponse(token);
            DekiContext.Current.Instance.EventSink.UserLogin(context.StartTime, DekiContext.Current.User);

            //TODO Max: Set a response header or status to indicate that an alt password was used.
            response.Return(ret);
            yield break;
        }

        [DreamFeature("GET:users", "Retrieve list of users.")]
        [DreamFeatureParam("usernamefilter", "string?", "Search for users name starting with supplied text")]
        [DreamFeatureParam("fullnamefilter", "string?", "Search for users full name starting with supplied text")]
        [DreamFeatureParam("usernameemailfilter", "string?", "Search for users by name and email or part of a name and email")]
        [DreamFeatureParam("authprovider", "int?", "Return users belonging to given authentication service id")]
        [DreamFeatureParam("rolefilter", "string?", "Search for users by a role name")]
        [DreamFeatureParam("activatedfilter", "bool?", "Search for users by their active status")]
        [DreamFeatureParam("limit", "string?", "Maximum number of items to retrieve. Must be a positive number or 'all' to retrieve all items. (default: 100)")]
        [DreamFeatureParam("offset", "int?", "Number of items to skip. Must be a positive number or 0 to not skip any. (default: 0)")]
        [DreamFeatureParam("sortby", "{id, username, nick, email, fullname, date.lastlogin, status, role, service, date.created}?", "Sort field. Prefix value with '-' to sort descending. default: No sorting")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Read access is required")]
        public Yield GetUsers(DreamContext context, DreamMessage request, Result<DreamMessage> response) {

            // TODO (steveb): add 'emailfilter' and use it to obsolete 'usernameemailfilter'; 'usernamefilter', 'fullnamefilter', and 'emailfilter' 
            //                should be OR'ed together when they are present.

            PermissionsBL.CheckUserAllowed(DekiContext.Current.User, Permissions.READ);
            uint totalCount;
            uint queryCount;
            IList<UserBE> users = UserBL.GetUsersByQuery(context, null, out totalCount, out queryCount);
            XDoc result = new XDoc("users");
            result.Attr("count", users.Count);
            result.Attr("querycount", queryCount);
            result.Attr("totalcount", totalCount);
            result.Attr("href", DekiContext.Current.ApiUri.At("users"));
            foreach (UserBE u in users) {
                result.Add(UserBL.GetUserXmlVerbose(u, null, Utils.ShowPrivateUserInfo(u)));
            }
            response.Return(DreamMessage.Ok(result));
            yield break;
        }

        [DreamFeature("GET:users/{userid}", "Retrieve information about a user.")]
        [DreamFeatureParam("{userid}", "string", "either an integer user ID, \"current\", or \"=\" followed by a double uri-encoded user name")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Read access is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested user could not be found")]
        public Yield GetUser(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            UserBE u = GetUserFromUrlMustExist();

            //Perform permission check if not looking yourself up
            if(u.ID != DekiContext.Current.User.ID) {
                PermissionsBL.CheckUserAllowed(DekiContext.Current.User, Permissions.READ);
            }
            response.Return(DreamMessage.Ok(UserBL.GetUserXmlVerbose(u, null, Utils.ShowPrivateUserInfo(u))));
            yield break;
        }

        [DreamFeature("POST:users", "Add or modify a user")]
        [DreamFeatureParam("accountpassword", "string?", "Account password to set (default: do not set/change password)")]
        [DreamFeatureParam("authusername", "string?", "Username to use for verification with external authentication service")]
        [DreamFeatureParam("authpassword", "string?", "Password to use for verification with external authentication service")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Administrator access, apikey, or account owner is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested user could not be found")]
        [DreamFeatureStatus(DreamStatus.Conflict, "Username conflicts with an existing username")]
        public Yield PostUser(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            if(!PermissionsBL.IsUserAllowed(DekiContext.Current.User, Permissions.ADMIN)) {
                throw new DreamForbiddenException("Must provide an apikey or admin authtoken to create a user");
            }
            UserBE user = null;

            //Authorization is performed later.
            string accountPassword = context.GetParam("accountpassword", null);

            //standard user creation/editing
            user = UserBL.PostUserFromXml(request.ToDocument(), null, accountPassword, context.GetParam("authusername", null), context.GetParam("authpassword", null));
            response.Return(DreamMessage.Ok(UserBL.GetUserXmlVerbose(user, null, Utils.ShowPrivateUserInfo(user))));
            yield break;
        }

        [DreamFeature("PUT:users/{userid}", "Modify an existing user")]
        [DreamFeatureParam("{userid}", "string", "either an integer user ID, \"current\", or \"=\" followed by a double uri-encoded user name")]
        [DreamFeatureParam("accountpassword", "string?", "Account password to set (default: do not set/change password)")]
        [DreamFeatureParam("authusername", "string?", "Username to use for verification with external authentication service")]
        [DreamFeatureParam("authpassword", "string?", "Password to use for verification with external authentication service")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Administrator access or account owner is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested user could not be found")]
        [DreamFeatureStatus(DreamStatus.Conflict, "Username conflicts with an existing username")] 
        public Yield PutUser(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            UserBE user = GetUserFromUrl();

            //Authorization is performed later.
            if (user == null) {
                throw new DreamAbortException(DreamMessage.NotFound(DekiResources.GIVEN_USER_NOT_FOUND_USE_POST));
            }
            string accountPassword = context.GetParam("accountpassword", null);
            user = UserBL.PostUserFromXml(request.ToDocument(), user, accountPassword, context.GetParam("authusername", null), context.GetParam("authpassword", null));
            response.Return(DreamMessage.Ok(UserBL.GetUserXmlVerbose(user, null, Utils.ShowPrivateUserInfo(user))));
            yield break;
        }

        [DreamFeature("POST:users/{userid}/allowed", "Check one or more resources if given operation is allowed.")]
        [DreamFeatureParam("{userid}", "string", "either an integer user ID, \"current\", or \"=\" followed by a double uri-encoded user name")]
        [DreamFeatureParam("mask", "long?", "Permission bit mask required for the pages")]
        [DreamFeatureParam("operations", "string?", "Comma separated list of operations to verify")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested user could not be found")]
        public Yield PostUsersAllowed(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            ulong permissionMask = context.GetParam<ulong>("mask", 0);
            string operationList = context.GetParam("operations", "");

            UserBE u = GetUserFromUrlMustExist();

            //Use comma separated permission list or permissionmask from request.
            Permissions p = Permissions.NONE;
            if (permissionMask != 0) {
                p = (Permissions) permissionMask;
            }

            //Convert operation list to mask combined with provided mask
            if (!string.IsNullOrEmpty(operationList)) {
                try {
                    p |= (Permissions) PermissionsBL.MaskFromPermissionList(PermissionsBL.PermissionListFromString(operationList));
                }
                catch {
                    throw new DreamAbortException(DreamMessage.BadRequest(DekiResources.INVALID_OPERATION_LIST));
                }
            }

            if (request.ToDocument().HasName("pages")) {
                
                //Parse provided page list into objects
                IList<PageBE> result = PageBL.ReadPagesXml(request.ToDocument());
                if (p != Permissions.NONE) {
                    result = PermissionsBL.FilterDisallowed(u, result, false, p);
                }

                XDoc responseDoc = new XDoc("pages");
                if (result != null) {
                    foreach (PageBE pg in result) {
                        responseDoc.Add(PageBL.GetPageXml(pg, null));
                    }
                }
                response.Return(DreamMessage.Ok(responseDoc));
            }
            else {
                throw new DreamAbortException(DreamMessage.BadRequest(DekiResources.EXPECTED_ROOT_NODE_PAGES));
            }
            yield break;
        }

        [DreamFeature("PUT:users/{userid}/password", "Set password for a given user.")]
        [DreamFeatureParam("{userid}", "string", "either an integer user ID, \"current\", or \"=\" followed by a double uri-encoded user name")]
        [DreamFeatureParam("currentpassword", "string?", "Current password needed when changing your own password (without admin rights)")]
        [DreamFeatureParam("altpassword", "bool?", "If true, the given password sets a secondary password that can be used for login. The main password is not overwritten. (default: false)")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Administrator access or account owner is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested user could not be found")]
        public Yield PutPasswordChange(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            
            UserBE targetUser = GetUserFromUrlMustExist();

            string password = request.AsText();

            if (string.IsNullOrEmpty(password)) {
                response.Return(DreamMessage.BadRequest(DekiResources.NEW_PASSWORD_NOT_PROVIDED));
                yield break;
            }

            if( password.Length < 4)
                throw new DreamAbortException(DreamMessage.BadRequest(DekiResources.NEW_PASSWORD_TOO_SHORT));

            //Ensure that the password is being set only on local accounts
            ServiceBE s = ServiceBL.GetServiceById(targetUser.ServiceId);
            if( s != null && !ServiceBL.IsLocalAuthService(s)){
                throw new DreamAbortException(DreamMessage.BadRequest(DekiResources.PASSWORD_CHANGE_LOCAL_ONLY));
            }

            if (UserBL.IsAnonymous(targetUser))
                throw new DreamBadRequestException(DekiResources.CANNOT_CHANGE_ANON_PASSWORD);

            //Admins can always change anyones password.
            if (PermissionsBL.IsUserAllowed(DekiContext.Current.User, Permissions.ADMIN)) {

                //For admins a currentpassword is option but if given then it should be validated
                string currentPwd = context.GetParam("currentpassword", string.Empty);

                if (!string.IsNullOrEmpty(currentPwd)) {
                    if (!AuthBL.IsValidAuthenticationForLocalUser(targetUser, currentPwd)) {
                        throw new DreamAbortException(DreamMessage.Forbidden(DekiResources.CURRENTPASSWORD_DOES_NOT_MATCH));
                    }
                }
            }else if (DekiContext.Current.User.ID == targetUser.ID){

                if (context.GetParam<bool>("altpassword", false)) {
                    throw new DreamBadRequestException(DekiResources.CANNOT_CHANGE_OWN_ALT_PASSWORD);
                }

                //User changing their own password requires knowledge of their current password
                string currentPwd = context.GetParam("currentpassword");
                if (!AuthBL.IsValidAuthenticationForLocalUser(DekiContext.Current.User, currentPwd)) {
                    throw new DreamAbortException(DreamMessage.Forbidden(DekiResources.CURRENTPASSWORD_DOES_NOT_MATCH));
                }
            } else {
                response.Return(DreamMessage.Forbidden(DekiResources.MUST_BE_TARGET_USER_OR_ADMIN));
                yield break;
            }
            bool altPassword = context.GetParam<bool>("altpassword", false);
            targetUser = UserBL.SetPassword(targetUser, password, altPassword);
            if(DekiContext.Current.User.ID == targetUser.ID) {
                response.Return(BuildSetAuthTokenResponse(AuthBL.CreateAuthTokenForUser(targetUser)));
            } else {
                response.Return(DreamMessage.Ok());
            }
            yield break;
        }

        [DreamFeature("GET:users/{userid}/favorites", "Retrieves a list of favorite pages for a user.")]
        [DreamFeatureParam("{userid}", "string", "either an integer user ID, \"current\", or \"=\" followed by a double uri-encoded user name")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "BROWSE access is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested user could not be found")]

        public Yield GetFavoritePagesForUser(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            XDoc result = PageBL.GetFavoritePagesForUser(GetUserFromUrlMustExist());
            response.Return(DreamMessage.Ok(result));
            yield break;
        }

        private UserBE GetUserFromUrl() {
            UserBE u = null;
            string userid = DreamContext.Current.GetParam("userid");

            //Double decoding of name is done to work around a mod_proxy issue that strips out slashes
            userid = XUri.Decode(userid);
            if (StringUtil.EqualsInvariantIgnoreCase(userid.Trim(), "current")) {
                u = DekiContext.Current.User;
            }
            else if (userid.StartsWith("=")) {
                string name = userid.Substring(1);
                u = DbUtils.CurrentSession.Users_GetByName(name);
            }
            else {
                uint userIdInt;
                if (!uint.TryParse(userid, out userIdInt)) {
                    throw new DreamAbortException(DreamMessage.BadRequest(DekiResources.USERID_PARAM_INVALID));
                }
                u = UserBL.GetUserById(userIdInt);
            }
            return u;
        }

        private UserBE GetUserFromUrlMustExist() {
            UserBE u = GetUserFromUrl();
            if(u == null) {
                throw new DreamNotFoundException(DekiResources.GIVEN_USER_NOT_FOUND);
            }
            return u;
        }

        private DreamMessage BuildSetAuthTokenResponse(string authToken) {
            DreamMessage responseMsg = DreamMessage.Ok(MimeType.TEXT_UTF8, authToken);

            // set expiration time, if one is provided
            DateTime expires = DateTime.MinValue; // Default to no expiration attribute (session cookie)
            TimeSpan authCookieExpirationTime = DekiContext.Current.Instance.AuthCookieExpirationTime;
            if (authCookieExpirationTime.TotalSeconds > 0) {
                expires = DateTime.UtcNow.Add(authCookieExpirationTime);
            }

            // add 'Set-Cookie' header
            responseMsg.Cookies.Add(DreamCookie.NewSetCookie(AUTHTOKEN_COOKIENAME, authToken, Self.Uri.AsPublicUri().WithoutPathQueryFragment(), expires));

            // add 'P3P' header for IE
            responseMsg.Headers["P3P"] = "CP=\"IDC DSP COR ADM DEVi TAIi PSA PSD IVAi IVDi CONi HIS OUR IND CNT\"";
            return responseMsg;
        }
    }
}
