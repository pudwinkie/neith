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
using System.Text;

using MindTouch.Deki.Data;
using MindTouch.Dream;
using MindTouch.Xml;

namespace MindTouch.Deki.Logic {
    
    public static class PermissionsBL {

        #region grant methods

        /// <summary>
        /// This applies permissions in a set/replace approach synonymous with PUT
        /// </summary>
        /// <param name="targetPage">Page to apply permissions to</param>
        /// <param name="restriction">Optional restriction mask to apply to page (and optionally to child pages)</param>
        /// <param name="proposedGrants">List of grants to apply to page and child pages</param>
        /// <param name="cascade">
        /// NONE: Dont apply permissions. 
        /// ABSOLUTE: proposedGrants are applied to root page and child pages. All grants not in the proposedGrants list are removed.
        /// DELTAS: proposedGrants is applied exactly to the root page. Child pages get the differences between the proposedGrants and the grants of the root page thus preserving the grants they had.
        /// </param>
        public static void ReplacePagePermissions(PageBE targetPage, RoleBE restriction, IList<GrantBE> proposedGrants, CascadeType cascade) {
            //Perform validation of grants.

            // Make sure users and groups described in grants exist.
            // this populates user/group object within the grant.
            VerifyValidUsersAndGroups(proposedGrants);

            // Ensure a duplicate grant isn't given for the same role multiple times to a user/grant
            HashGrantsByTypeGranteeIdRoleId(proposedGrants);

            IList<GrantBE> currentGrants, proposedAddedGrants, proposedRemovedGrants;
            ulong userEffectiveRights = (ulong)GetUserPermissions(DekiContext.Current.User);

            switch(cascade) {
            case CascadeType.NONE:

                //No cascading to children. delta(current security of page, proposed security) is applied
                currentGrants = DbUtils.CurrentSession.Grants_GetByPage((uint)targetPage.ID);
                CompareGrantSets(currentGrants, proposedGrants, out proposedAddedGrants, out proposedRemovedGrants);
                ApplyPermissionChange(targetPage, false, userEffectiveRights, null, proposedAddedGrants, proposedRemovedGrants, currentGrants, restriction, true);
                break;
            case CascadeType.ABSOLUTE:

                //Cascade proposed security set to children.
                ApplyPermissionChange(targetPage, true, userEffectiveRights, proposedGrants, null, null, null, restriction, true);
                break;
            case CascadeType.DELTA:

                //Cascade delta(current security of page, proposed security) to page and children
                currentGrants = DbUtils.CurrentSession.Grants_GetByPage((uint)targetPage.ID);
                CompareGrantSets(currentGrants, proposedGrants, out proposedAddedGrants, out proposedRemovedGrants);
                // Note (arnec): even if proposed add & remove are empty, we have to call this method, since restriction may need to be set and propagated.
                ApplyPermissionChange(targetPage, true, userEffectiveRights, null, proposedAddedGrants, proposedRemovedGrants, currentGrants, restriction, true);
                break;
            }
        }

        public static void DeleteAllGrantsForPage(PageBE page) {
            DbUtils.CurrentSession.Grants_DeleteByPage(new List<ulong>() {page.ID});
            RecentChangeBL.AddGrantsRemovedRecentChange(DateTime.UtcNow, page, DekiContext.Current.User, DekiResources.GRANT_REMOVED_ALL);
        }

        /// <summary>
        /// This applies permissions in a add/remove approach synonymous as used by POST.
        /// </summary>
        /// <param name="targetPage">Page to apply permissions to</param>
        /// <param name="restriction">Optional restriction mask to apply to page (and optionally to child pages)</param>
        /// <param name="grantsAdded"></param>
        /// <param name="grantsRemoved"></param>
        /// <param name="cascade">Whether or not to apply permissions to child pages</param>
        /// <remarks>
        /// This preserves existing page permissions by only adding and removing proposed grants
        /// </remarks>
        public static void ApplyDeltaPagePermissions(PageBE targetPage, RoleBE restriction, List<GrantBE> grantsAdded, List<GrantBE> grantsRemoved, bool cascade) {

            // Make sure users and groups described in grants exist.
            // this populates user/group object within the grant.
            List<GrantBE> allGrants = new List<GrantBE>(grantsAdded);
            allGrants.AddRange(grantsRemoved);
            VerifyValidUsersAndGroups(allGrants);

            // Ensure a duplicate grant isn't given for the same role multiple times to a user/grant
            HashGrantsByTypeGranteeIdRoleId(grantsAdded);
            HashGrantsByTypeGranteeIdRoleId(grantsRemoved);

            ulong userEffectiveRights = (ulong)GetUserPermissions(DekiContext.Current.User);
            ApplyPermissionChange(targetPage, cascade, userEffectiveRights, null, grantsAdded, grantsRemoved, null, restriction, true);
        }

        /// <summary>
        /// Main workflow of applying permissions to pages
        /// </summary>
        /// <param name="targetPage">Page to apply permissions to</param>
        /// <param name="cascade">Whether or not to apply permissions to child pages</param>
        /// <param name="userEffectiveRights">permission mask of user (user + group operations) independant of a page</param>
        /// <param name="proposedGrantSet">Used only for PUT cascade=absolute</param>
        /// <param name="proposedAddedGrants">Grants to add (if they dont already exist)</param>
        /// <param name="proposedRemovedGrants">Grants to remove (if they exist)</param>
        /// <param name="currentGrants">Optional current set of grants for targetPage. This is provided as an optimization to eliminate a db hit</param>
        /// <param name="proposedRestriction">Optional restriction to be applied for page (and child pages)</param>
        /// <param name="atStartPage">Always true when calling from outside this method. Used for recursion to not throw exceptions for errors on child pages</param>
        private static void ApplyPermissionChange(PageBE targetPage, bool cascade, ulong userEffectiveRights, IList<GrantBE> proposedGrantSet, IList<GrantBE> proposedAddedGrants, IList<GrantBE> proposedRemovedGrants, IList<GrantBE> currentGrants, RoleBE proposedRestriction, bool atStartPage ) {

            // TODO (steveb): validate which parameters are null and which are not (i.e. proposedAddedGrants, proposedRemovedGrants, proposedGrantSet)
            
            if(!targetPage.Title.IsEditable) {
                throw new DreamAbortException(DreamMessage.Forbidden(string.Format(DekiResources.PERMISSIONS_NOT_ALLOWED_ON, targetPage.Title.Path, targetPage.Title.Namespace)));
            }

            UserBE currentUser = DekiContext.Current.User;
            bool arePermissionsOkForPage = true;

            // Make sure granter has access to what is being granted.
            // remove already existing grants from new grant list.
            if(currentGrants == null) {
                currentGrants = DbUtils.CurrentSession.Grants_GetByPage((uint)targetPage.ID);
            }

            //proposedGrantSet will be null for descendant pages as well as for setting permissions via POST in a relative fashion
            if(proposedGrantSet != null) {
                CompareGrantSets(currentGrants, proposedGrantSet, out proposedAddedGrants, out proposedRemovedGrants);
            }

            AddRequesterToAddedGrantList(currentGrants, proposedAddedGrants, targetPage);            

            try {

                //Determine the grant perm mask for the current user by using either the proposedGrantSet for the initial page
                //Or by computing it for descendant pages that dont have a proposedGrantSet passed in
                ulong proposedPageGrantMaskForUser;
                if(proposedGrantSet != null) {
                    proposedPageGrantMaskForUser = SumGrantPermissions(proposedGrantSet, currentUser);
                } else {
                    List<GrantBE> proposedGrantSetForCurrentPage = ApplyGrantMerge(currentGrants, proposedAddedGrants, proposedRemovedGrants);
                    proposedPageGrantMaskForUser = SumGrantPermissions(proposedGrantSetForCurrentPage, currentUser);
                }                               

                //Ensure user has the combined permissions granted to the page
                ulong addedGrantMask = SumGrantPermissions(proposedAddedGrants, null);
                CheckUserAllowed(currentUser, targetPage, (Permissions) addedGrantMask);

                //Determine the restriction mask to use. 
                ulong restrictionFlag = ulong.MaxValue;
                RoleBE targetPageRestriction = PermissionsBL.GetRestrictionById(targetPage.RestrictionID);
                if (proposedRestriction == null && targetPageRestriction != null) {
                    restrictionFlag = targetPageRestriction.PermissionFlags;
                }
                else if (proposedRestriction != null) {
                    restrictionFlag = proposedRestriction.PermissionFlags;
                }

                //Ensure the granter is not locking himself out.
                //Calculate the rights the granter would have after the grants are saved
                ulong newEffectivePageRights = PermissionsBL.CalculateEffectivePageRights(new PermissionStruct(userEffectiveRights, (ulong)restrictionFlag, proposedPageGrantMaskForUser));

                //If granter no longer has "CHANGEPERMISSIONS" access after grants, the user is locking self out.
                if (!IsActionAllowed(newEffectivePageRights, false, false, false, Permissions.CHANGEPERMISSIONS)) {
                    throw new DreamAbortException(DreamMessage.BadRequest(DekiResources.USER_WOULD_BE_LOCKED_OUT_OF_PAGE));
                }

            }
            catch (DreamAbortException) { // Validation/permission related exceptions
                arePermissionsOkForPage = false;
                if (atStartPage) //Swallow validation exceptions on 
                    throw;
            }

            if (arePermissionsOkForPage) {
                //All validation steps succeeded: Apply grants/restriction to page based on delta of current grants and proposed additions/removals
                
                List<GrantBE> grantsToRemove = IntersectGrantSets(currentGrants, proposedRemovedGrants);
                List<GrantBE> grantsToAdd = GetExtraGrantsInEndingSet(currentGrants, proposedAddedGrants);

                if (grantsToRemove.Count > 0) {

                    // Contstruct recent change description
                    StringBuilder deleteGrantsDescription = new StringBuilder();
                    List<uint> userGrantIds = new List<uint>();
                    List<uint> groupGrantIds = new List<uint>();
                    foreach(GrantBE g in grantsToRemove) {
                        if(deleteGrantsDescription.Length > 0) {
                            deleteGrantsDescription.Append(", ");
                        }
                        if(g.Type == GrantType.USER) {
                            userGrantIds.Add(g.Id);
                            UserBE user = UserBL.GetUserById(g.UserId);
                            if(user != null) {
                                deleteGrantsDescription.AppendFormat(DekiResources.GRANT_REMOVED, user.Name, g.Role.Name.ToLowerInvariant());
                            }
                        } else if(g.Type == GrantType.GROUP) {
                            groupGrantIds.Add(g.Id);
                            GroupBE group = GroupBL.GetGroupById(g.GroupId);
                            if(group != null) {
                                deleteGrantsDescription.AppendFormat(DekiResources.GRANT_REMOVED, group.Name, g.Role.Name.ToLowerInvariant());
                            }
                        }
                    }
                    DbUtils.CurrentSession.Grants_Delete(userGrantIds, groupGrantIds);
                    RecentChangeBL.AddGrantsRemovedRecentChange(DateTime.UtcNow, targetPage, DekiContext.Current.User, deleteGrantsDescription.ToString());
                }

                if(grantsToAdd.Count > 0) {

                    // Contstruct recent change description
                    Dictionary<uint, PageBE> uniquePages = new Dictionary<uint, PageBE>();
                    StringBuilder addGrantsDescription = new StringBuilder();
                    foreach(GrantBE grant in grantsToAdd) {
                        grant.CreatorUserId = DekiContext.Current.User.ID;
                        if(addGrantsDescription.Length > 0) {
                            addGrantsDescription.Append(", ");
                        }
                        if(grant.Type == GrantType.USER) {
                            UserBE user = UserBL.GetUserById(grant.UserId);
                            if(user != null) {
                                addGrantsDescription.AppendFormat(DekiResources.GRANT_ADDED, user.Name, grant.Role.Name.ToLowerInvariant());
                            }
                        } else if(grant.Type == GrantType.GROUP) {
                            GroupBE group = GroupBL.GetGroupById(grant.GroupId);
                            if(group != null) {
                                addGrantsDescription.AppendFormat(DekiResources.GRANT_ADDED, group.Name, grant.Role.Name.ToLowerInvariant());
                            }
                        }
                        uniquePages[grant.PageId] = PageBL.GetPageById(grant.PageId);
                    }
                    foreach (GrantBE grantToAdd in grantsToAdd) {
                        DbUtils.CurrentSession.Grants_Insert(grantToAdd);
                    }
                    foreach(PageBE p in uniquePages.Values) {
                        RecentChangeBL.AddGrantsAddedRecentChange(DateTime.UtcNow, p, DekiContext.Current.User, addGrantsDescription.ToString());
                    }
                }

                targetPage.Touched = DateTime.UtcNow;

                if (proposedRestriction != null && targetPage.RestrictionID != proposedRestriction.ID) {
                    targetPage.RestrictionID = proposedRestriction.ID;
                    DbUtils.CurrentSession.Pages_Update(targetPage);
                    RecentChangeBL.AddRestrictionUpdatedChange(targetPage.Touched, targetPage, currentUser, string.Format(DekiResources.RESTRICTION_CHANGED, proposedRestriction.Name));
                } else {
                    PageBL.Touch(targetPage, DateTime.UtcNow);
                }                           
            }

            //Cascade into child pages only if current page permissions applied
            if (cascade) {
                if (proposedGrantSet != null) {
                    proposedAddedGrants = proposedRemovedGrants = null;
                }

                ICollection<PageBE> childPages = PageBL.GetChildren(targetPage, true);
                foreach (PageBE p in childPages) {
                    ResetGrants(proposedGrantSet, p);
                    ResetGrants(proposedAddedGrants, p);
                    ResetGrants(proposedRemovedGrants, p);

                    ApplyPermissionChange(p, true, userEffectiveRights, proposedGrantSet, proposedAddedGrants, proposedRemovedGrants, null, proposedRestriction, false);
                }
            }
        }

        private static void AddRequesterToAddedGrantList(IList<GrantBE> currentGrants, IList<GrantBE> proposedAddedGrants, PageBE page) {

            UserBE currentUser = DekiContext.Current.User;
            List<GrantBE> temp = new List<GrantBE>(currentGrants);
            temp.AddRange(proposedAddedGrants);

            bool grantForSelfExists = false;
            foreach(GrantBE g in temp) {
                if(g.Type == GrantType.USER && g.UserId == currentUser.ID) {
                    grantForSelfExists = true;
                    break;
                }
            }

            if (!grantForSelfExists) {
                GrantBE requesterGrant = new GrantBE();
                requesterGrant.PageId = (uint) page.ID;
                requesterGrant.UserId = currentUser.ID;
                requesterGrant.Type = GrantType.USER;

                //TODO (MaxM): BUG 7054.  This is the last hardcoded role name. 
                // returns null if the role is not found
                RoleBE requesterRole = PermissionsBL.GetRoleByName(Role.CONTRIBUTOR);

                if(requesterRole == null)
                    throw new MissingFieldException(string.Format(DekiResources.CANNOT_RETRIEVE_REQUIRED_ROLE, Role.CONTRIBUTOR));
                requesterGrant.RoleId = requesterRole.ID;
                requesterGrant.Role = requesterRole;

                proposedAddedGrants.Add(requesterGrant);
            }
        }

        /// <summary>
        /// Returns a bitmask sum of grants applying to a given user
        /// </summary>
        /// <param name="grants"></param>
        /// <param name="user">If null all grants are summed independant of user</param>
        /// <returns></returns>
        private static ulong SumGrantPermissions(IList<GrantBE> grants, UserBE user) {
            ulong grantPermissionSum = 0;
            foreach (GrantBE grant in grants) {
                GrantBE currentGrant = grant;
                if (user != null) {
                    IList<GroupBE> groups = DbUtils.CurrentSession.Groups_GetByUser(user.ID);
                    if(((GrantType.USER == grant.Type) && (user.ID == grant.UserId)) || ((GrantType.GROUP == grant.Type) && (Array.Exists(groups.ToArray(), delegate(GroupBE g) { return g != null && g.Id == currentGrant.GroupId; })))) {
                        grantPermissionSum |= grant.Role.PermissionFlags;
                    }
                } else {
                    grantPermissionSum |= grant.Role.PermissionFlags;                    
                }
            }
            return grantPermissionSum;
        }

        /// <summary>
        /// Performs a subtraction of grants (in both directions) to determine which grants were added and removed from one set to another
        /// </summary>
        /// <param name="current"></param>
        /// <param name="proposed"></param>
        /// <param name="added"></param>
        /// <param name="removed"></param>
        private static void CompareGrantSets(IList<GrantBE> current, IList<GrantBE> proposed, out IList<GrantBE> added, out IList<GrantBE> removed) {
            added = GetExtraGrantsInEndingSet(current, proposed);
            removed = GetExtraGrantsInEndingSet(proposed, current);
        }

        /// <summary>
        /// Returns list of grants that are in both sets
        /// </summary>
        /// <param name="setA"></param>
        /// <param name="setB"></param>
        /// <returns></returns>
        private static List<GrantBE> IntersectGrantSets(IList<GrantBE> setA, IList<GrantBE> setB) {
            List<GrantBE> removed = GetExtraGrantsInEndingSet(setB, setA);
            List<GrantBE> intersection = GetExtraGrantsInEndingSet(removed, setA);
            return intersection;
        }

        /// <summary>
        /// Set the pageId for grants while resetting their primary key id. This is used for cascading grants to child pages.
        /// </summary>
        /// <param name="grants"></param>
        /// <param name="targetPage"></param>
        private static void ResetGrants(IList<GrantBE> grants, PageBE targetPage) {
            if (grants != null) {
                foreach (GrantBE g in grants) {
                    g.PageId = (uint) targetPage.ID;
                    g.Id = 0;
                }
            }
        }

        /// <summary>
        /// Compare two grant lists and determine grants that exist in endingSet that dont exist in startingSet
        /// </summary>
        /// <param name="startingSet"></param>
        /// <param name="endingSet"></param>
        /// <returns>The difference between endingSet and startingSet (endingSet - startingSet)</returns>
        private static List<GrantBE> GetExtraGrantsInEndingSet(IList<GrantBE> startingSet, IList<GrantBE> endingSet) {
        	
        	// NOTE (steveb): consider renaming to 'SubtractGrantsFrom(endingSet, startingSet)'
        	
            List<GrantBE> extraGrants = new List<GrantBE>();

            Dictionary<GrantType, Dictionary<uint, Dictionary<uint, GrantBE>>> startingSetHash =
                HashGrantsByTypeGranteeIdRoleId(startingSet);

            foreach (GrantBE g in endingSet) {
                uint id = g.Type == GrantType.USER ? g.UserId : g.GroupId;
                if (!startingSetHash.ContainsKey(g.Type) || !startingSetHash[g.Type].ContainsKey(id) || !startingSetHash[g.Type][id].ContainsKey(g.RoleId))
                    extraGrants.Add(g);
            }

            return extraGrants;
        }

        /// <summary>
        /// Ensure a duplicate grant isn't given for the same role multiple times to a user/grant.
        /// </summary>
        /// <param name="grants"></param>
        /// <returns>Returns the hash used for the calculation</returns>
        private static Dictionary<GrantType, Dictionary<uint, Dictionary<uint, GrantBE>>> HashGrantsByTypeGranteeIdRoleId(IList<GrantBE> grants) {
            Dictionary<GrantType, Dictionary<uint, Dictionary<uint, GrantBE>>> grantHash =
                new Dictionary<GrantType, Dictionary<uint, Dictionary<uint, GrantBE>>>();

            if (grants == null)
                throw new ArgumentNullException("grants");

            grantHash[GrantType.USER] = new Dictionary<uint, Dictionary<uint, GrantBE>>();
            grantHash[GrantType.GROUP] = new Dictionary<uint, Dictionary<uint, GrantBE>>();

            foreach (GrantBE grant in grants) {
                GrantType type = grant.Type;

                uint id = grant.Type == GrantType.USER ? grant.UserId : grant.GroupId;

                if (!grantHash[type].ContainsKey(id)) {
                    grantHash[type][id] = new Dictionary<uint, GrantBE>();
                    if (!grantHash[type][id].ContainsKey(grant.RoleId)) {
                        grantHash[type][id][grant.RoleId] = grant;
                    }
                    else {
                        throw new DreamAbortException(DreamMessage.BadRequest(DekiResources.DUPLICATE_ROLE));
                    }
                }
                else {
                    throw new DreamAbortException(DreamMessage.BadRequest(DekiResources.DUPLICATE_GRANT_FOR_USER_GROUP));
                }
            }
            return grantHash;
        }

        /// <summary>
        /// Verifies that groups and users described by the grants exist. Populates user/group object within grant.
        /// </summary>
        /// <param name="grants"></param>
        private static void VerifyValidUsersAndGroups(IList<GrantBE> grants) {
            List<uint> userIds = new List<uint>();
            List<uint> groupIds = new List<uint>();

            //Collect all group/userids from grants
            foreach (GrantBE grant in grants) {
                if (grant.Type == GrantType.USER) {
                    userIds.Add(grant.UserId);
                }
                else if (grant.Type == GrantType.GROUP) {
                    groupIds.Add(grant.GroupId);
                }
            }
            Dictionary<uint, GroupBE> groupsById = DbUtils.CurrentSession.Groups_GetByIds(groupIds).AsHash(e => e.Id);
            Dictionary<uint, UserBE> usersById = DbUtils.CurrentSession.Users_GetByIds(userIds).AsHash(e => e.ID);

            //Verify each user/group exists in db and assign the user/group object within the grant.
            foreach (GrantBE grant in grants) {
                if (grant.Type == GrantType.USER) {
                    if (!usersById.ContainsKey(grant.UserId) || null == usersById[grant.UserId])
                        throw new DreamAbortException(DreamMessage.BadRequest(string.Format(DekiResources.CANNOT_FIND_USER_WITH_ID, grant.UserId)));
                }
                else if (grant.Type == GrantType.GROUP) {
                    if (!groupsById.ContainsKey(grant.GroupId) || null == groupsById[grant.GroupId])
                        throw new DreamAbortException(DreamMessage.BadRequest(string.Format(DekiResources.CANNOT_FIND_GROUP_WITH_ID, grant.GroupId)));
                }
            }
        }

        private static List<GrantBE> ApplyGrantMerge(IList<GrantBE> currentGrants, IList<GrantBE> grantsToAdd, IList<GrantBE> grantsToRemove) {
            
            //Start out with a copy of the current grants
            List<GrantBE> currentWithAdded = new List<GrantBE>(currentGrants);
            
            //Add the grants to be added
            currentWithAdded.AddRange(grantsToAdd);

            //Subtract the (intersecting) grants that need to be removed
            return GetExtraGrantsInEndingSet(grantsToRemove, currentWithAdded);
        }

        public static RoleBE GetRoleByName(string roleName) {
            return DbUtils.CurrentSession.RolesRestrictions_GetRoles().FirstOrDefault(e => StringUtil.EqualsInvariantIgnoreCase(e.Name, roleName));
        }

        public static RoleBE GetRoleById(uint id) {
            return DbUtils.CurrentSession.RolesRestrictions_GetRoles().FirstOrDefault(e => e.ID == id);
        }

        public static RoleBE GetRestrictionByName(string restrictionName) {
            return DbUtils.CurrentSession.RolesRestrictions_GetRestrictions().FirstOrDefault(e => StringUtil.EqualsInvariantIgnoreCase(e.Name, restrictionName));
        }

        public static RoleBE GetRestrictionById(uint id) {
            return DbUtils.CurrentSession.RolesRestrictions_GetRestrictions().FirstOrDefault(e => e.ID == id);
        }

        #endregion

        #region IsUserAllowed overloads
        public static bool IsUserAllowed(UserBE user, params Permissions [] actions) {
            return CheckUserAllowed(user,false,actions);
        }

        public static bool IsUserAllowed(UserBE user, PageBE page, params Permissions[] actions) {
            return CheckUserAllowed(user, page, false, actions);
        }

        public static void CheckUserAllowed(UserBE user, params Permissions[] actions) {
            CheckUserAllowed(user, true, actions);
        }

        public static void CheckUserAllowed(UserBE user, PageBE page, params Permissions[] actions) {
            CheckUserAllowed(user, page, true, actions);
        }

        private static bool CheckUserAllowed(UserBE user, bool throwException, params Permissions[] actions) {
            if(user == null)
                return false;

            return IsActionAllowed((ulong)GetUserPermissions(user), throwException, true, true, actions);
        }

        private static bool CheckUserAllowed(UserBE user, PageBE page, bool throwException, params Permissions[] actions) {
            if (user == null || page == null)
                return false;

            PageBE[] allowedPages = FilterDisallowed(user, new PageBE[] { page }, throwException, true, true, actions);
            return (allowedPages == null || allowedPages.Length == 0) ? false : true;  
            
        }

        public static PageBE[] FilterDisallowed(UserBE user, ICollection<PageBE> pages, bool throwException, params Permissions[] actions) {
            PageBE[] filteredOutPages;
            return FilterDisallowed(user, pages, throwException, true, true, out filteredOutPages, actions);
        }

        public static PageBE[] FilterDisallowed(UserBE user, ICollection<PageBE> pages, bool throwException, bool allowApiKey, bool applyBanMask, params Permissions[] actions) {
            PageBE[] filteredOutPages;
            return FilterDisallowed(user, pages, throwException, allowApiKey, applyBanMask, out filteredOutPages, actions);
        }

        public static PageBE[] FilterDisallowed(UserBE user, ICollection<PageBE> pages, bool throwException, out PageBE[] filteredOutPages, params Permissions[] actions) {
            return FilterDisallowed(user, pages, throwException, true, true, out filteredOutPages, actions);
        }

        public static PageBE[] FilterDisallowed(UserBE user, ICollection<PageBE> pages, bool throwException, bool allowApiKey, bool applyBanMask, out PageBE[] filteredOutPages, params Permissions[] actions) {
            List<PageBE> allowedPages = new List<PageBE>();
            List<PageBE> filteredOutPagesList = new List<PageBE>();

            if (user == null || user.ID == 0 || pages == null)
                throw new ArgumentException("Null or unexpected parameter for PermissionsBL::FilterDisallowed");

            // Collect unique page id's
            Dictionary<ulong, PageBE> pageHash = new Dictionary<ulong,PageBE>();
            Dictionary<ulong, PageBE> pagesToCheck = new Dictionary<ulong, PageBE>();
            Dictionary<uint, RoleBE> restrictionsById = new Dictionary<uint, RoleBE>();

            bool validApiKey = allowApiKey && DekiContext.Current.IsValidApiKeyInRequest;

            Permissions userPermissions = GetUserPermissions(user);

            foreach (PageBE p in pages) {
                pageHash[p.ID] = p;

                if (!validApiKey) {
                    //Determine if page is protected and a db lookup is necessary to determine user rights to page.
                    ulong pageRestrictionFlags = ulong.MaxValue;
                    
                    //Cache unique restrictions by id to not have to rely on cache/roles to be enabled.
                    RoleBE pageRestriction;
                    if (!restrictionsById.TryGetValue(p.RestrictionID, out pageRestriction)) {
                        pageRestriction = PermissionsBL.GetRestrictionById(p.RestrictionID);
                        if (pageRestriction != null) {
                            restrictionsById[p.RestrictionID] = pageRestriction;
                        }
                    }

                    if (pageRestriction != null) {
                        pageRestrictionFlags = pageRestriction.PermissionFlags;
                    }

                    //If not allowed to given access to page with no grants, perform permissions check with grants
                    ulong effectivePageRights = CalculateEffectivePageRights(new PermissionStruct((ulong)userPermissions, pageRestrictionFlags, 0));
                    if (!IsActionAllowed(effectivePageRights, false, allowApiKey, applyBanMask, actions))
                        pagesToCheck[p.ID] = p;
                }
            }            

            //Get effective permisions for each pageid for the user from the db.
            Dictionary<ulong, PermissionStruct> pagePerms = null;
            if(pagesToCheck.Count > 0) {
                pagePerms = CalculateEffectiveForPages(user, pagesToCheck.Keys);
            } else {
                pagePerms = new Dictionary<ulong, PermissionStruct>();
            }

            if(pagePerms == null) {
                throw new ArgumentNullException("Error occurred in PermissionsDA::EffectivePermissionForUserPages");
            }

            //Foreach page/permission returned, add to list if queried and allowed
            foreach (uint pageId in pageHash.Keys) {
                PageBE currentPage;
                if (!pageHash.TryGetValue(pageId, out currentPage))
                    continue;

                if (pagesToCheck.ContainsKey(pageId)) {
                    PermissionStruct pagePerm;
                    if (!pagePerms.TryGetValue(pageId, out pagePerm))
                        continue;

                    if(IsActionAllowed(CalculateEffectivePageRights(pagePerm), throwException, currentPage, allowApiKey, applyBanMask, actions)) {
                        if (currentPage != null)
                            allowedPages.Add(currentPage);
                    } else {
                        if (currentPage != null)
                            filteredOutPagesList.Add(currentPage);
                    }
                } else
                    allowedPages.Add(currentPage);
            }

            filteredOutPages = filteredOutPagesList.ToArray();
            return allowedPages.ToArray();
        }

        public static ulong[] FilterDisallowed(UserBE user, ulong[] pageIds, bool throwException, params Permissions[] actions) {
            ulong[] filteredOutPages;
            return FilterDisallowed(user, pageIds, throwException, out filteredOutPages, actions);
        }

        public static ulong[] FilterDisallowed(UserBE user, ulong[] pageIds, bool throwException, out ulong[] filteredOutPages, params Permissions[] actions) {
            List<ulong> allowedPages = new List<ulong>();
            List<ulong> filteredOutPagesList = new List<ulong>();

            if (user == null || user.ID == 0 || pageIds == null)
                throw new ArgumentException("Null or unexpected parameter for PermissionsBL::FilterDisallowed");

            //Get effective permissions for each pageid for the user from the db.
            Dictionary<ulong, PermissionStruct> pagePerms = CalculateEffectiveForPages(user, pageIds);

            if(pagePerms == null) {
                throw new ArgumentNullException("Error occurred in PermissionsDA::EffectivePermissionForUserPages");
            }

            //Foreach page/permission returned, add to list if queried and allowed
            foreach (KeyValuePair<ulong, PermissionStruct> pagePerm in pagePerms) {
                if (IsActionAllowed(CalculateEffectivePageRights(pagePerm.Value), throwException, actions)) {
                    allowedPages.Add(pagePerm.Key);
                }  else {
                    filteredOutPagesList.Add(pagePerm.Key);
                }
            }

            filteredOutPages = filteredOutPagesList.ToArray();
            return allowedPages.ToArray();
        }

        public static uint[] FilterDisallowed(uint[] userIds, ulong pageId, bool throwException, params Permissions[] actions) {
            List<uint> allowedUsers = new List<uint>();
            Dictionary<uint, PermissionStruct> perms = DbUtils.CurrentSession.Grants_CalculateEffectiveForUsers(pageId, userIds);
            foreach(KeyValuePair<uint, PermissionStruct> pagePerm in perms) {

                // FilterDisallowed for single page, many users disregards apikey, because it's recognition would always
                // result in true, defeating the point of filtering out users
                if(IsActionAllowed(PermissionsBL.CalculateEffectivePageRights(pagePerm.Value), throwException, false, true, actions)) {
                    allowedUsers.Add(pagePerm.Key);
                } 
            }
            return allowedUsers.ToArray();
        }
        #endregion IsUserAllowed overloads

        public static Permissions GetUserPermissions(UserBE user) {
            Dictionary<ulong, PermissionStruct> permissionByUserId = CalculateEffectiveForPages(user, new ulong[] { 0 });
            PermissionStruct permission;
            if (permissionByUserId != null && permissionByUserId.TryGetValue(0, out permission)) {
                return permission.UserPermissions;
            } else {
                return Permissions.NONE;
            }
        }

        private static ulong CalculateEffectiveForUserPage(UserBE user, ulong pageId) {
            Dictionary<ulong, PermissionStruct> perms = DbUtils.CurrentSession.Grants_CalculateEffectiveForPages(user.ID, new ulong[] { pageId });
            ulong permissionMask = 0;
            if (perms != null && perms.ContainsKey(pageId)) {
                permissionMask = CalculateEffectivePageRights(perms[pageId]);
            }
            return permissionMask;
        }

        /// <summary>
        /// Calculates the rights that a user effectively has for a page. 
        /// </summary>
        /// <param name="permissions">Struct consisting of the following:
        /// Rights of specific to user is added to rights given to groups to which to user belongs
        /// Restriction flags associated with a page. These work to counter the effectiveUserGroupRights
        /// Effective grants associated with a page as a combinatation of user and group grants
        /// </param>
        /// <returns>The effective rights represented as a bitmask to a page</returns>
        public static ulong CalculateEffectivePageRights(PermissionStruct permissions) {

            //If a user's effective rights (independent of page) has admin flag, the effective page rights are same as user's effective rights.
            if ((permissions.UserPermissions & Permissions.ADMIN) == Permissions.ADMIN) {
                return (ulong)permissions.UserPermissions;
            }
            return (ulong)(((permissions.UserPermissions & permissions.PageRestrictionsMask) | permissions.PageGrantPermissions) | (permissions.UserPermissions & PermissionSets.PAGE_INDEPENDENT));
        }

        public static ulong CalculateEffectivePageRights(PageBE page, UserBE user) {
            //TODO (MaxM): This should be optimized to not make a direct db call but should instead calculate effective permissions based on
            // Page.Grants, Page.Restriction, user.PermissionMask

            ulong effectivePermissions = CalculateEffectiveForUserPage(user, page.ID);
            return CalculatePermissionReduction(user, effectivePermissions);
        }

        public static ulong CalculateEffectiveUserRights(UserBE user) {
            Permissions effectivePermissions = GetUserPermissions(user);
            return CalculatePermissionReduction(user, (ulong)effectivePermissions);
        }

        private static ulong CalculatePermissionReduction(UserBE user, ulong effectivePermissions) {

            //Reduce permissions due to bans.
            //Note: this will only reduce for current user
            if (DekiContext.Current.User.ID == user.ID && DekiContext.Current.BanPermissionRevokeMask != null) {
                effectivePermissions = effectivePermissions & (~DekiContext.Current.BanPermissionRevokeMask.Value);
            }

            //Reduce permissions due to licensing.
            effectivePermissions = effectivePermissions & LicenseBL.LicensePermissionRevokeMask();
            return effectivePermissions;
        }

        public static bool IsActionAllowed(ulong permissionMask, bool throwException, params Permissions[] actions) {
            return IsActionAllowed(permissionMask, throwException, null, true, true, actions);
        }

        public static bool IsActionAllowed(ulong permissionMask, bool throwException, bool allowApiKey, bool applyBanMask, params Permissions[] actions) {
            return IsActionAllowed(permissionMask, throwException, null, allowApiKey, applyBanMask, actions);
        }

        public static bool IsActionAllowed(ulong permissionMask, bool throwException, PageBE optionalPage, bool allowApiKey, bool applyBanMask, params Permissions[] actions) {
            if (allowApiKey && DekiContext.Current.IsValidApiKeyInRequest)
                return true;

            foreach (Permissions action in actions) {
                
                ulong banPreserveMask = ulong.MaxValue;
                if (applyBanMask && DekiContext.Current.BanPermissionRevokeMask != null) {
                    banPreserveMask = ~DekiContext.Current.BanPermissionRevokeMask.Value;
                }

                ulong licenseRevokeMask = LicenseBL.LicensePermissionRevokeMask();

                bool isActionAllowed =            ((permissionMask & (ulong) Permissions.ADMIN) == (ulong) Permissions.ADMIN) || ((permissionMask & (ulong) action) == (ulong) action);
                bool isActionAllowedWithLicense = ((permissionMask & (ulong) Permissions.ADMIN) == (ulong) Permissions.ADMIN) || ((permissionMask & licenseRevokeMask & (ulong) action) == (ulong) action);
                bool isActionAllowedWithBans =    ((permissionMask & banPreserveMask & (ulong) Permissions.ADMIN) == (ulong) Permissions.ADMIN) || ((permissionMask & banPreserveMask & (ulong) action) == (ulong) action);

                if (!isActionAllowed || !isActionAllowedWithLicense || !isActionAllowedWithBans) {
                    if (throwException) {

                        //TODO MaxM: if ban mask is applied and ban reasons exist in DekiContext.Current.BanReasons then consider including them as part of the exception message.

                        string errorMsg = null;

                        //If access denied due to licensing, set a custom error message
                        if (isActionAllowed && isActionAllowedWithBans /*&& !isActionAllowedWithLicense*/) {                            
                            errorMsg = string.Format(DekiResources.LICENSE_OPERATION_NOT_ALLOWED, PermissionsToString(actions));
                        } else {

                            if (optionalPage == null || optionalPage.ID == 0 || ((permissionMask & (ulong) Permissions.BROWSE) != (ulong) Permissions.BROWSE))
                                errorMsg = string.Format(DekiResources.ACCESS_DENIED_TO, DekiContext.Current.User.Name, PermissionsToString(actions), PermissionsToString(permissionMask));
                            else
                                errorMsg = string.Format(DekiResources.ACCESS_DENIED_TO_FOR_PAGE, DekiContext.Current.User.Name, PermissionsToString(actions), PermissionsToString(permissionMask), optionalPage.Title.AsPrefixedUserFriendlyPath());
                        }

                        if (UserBL.IsAnonymous(DekiContext.Current.User)) {
                            throw new DreamAbortException(DreamMessage.AccessDenied(DekiWikiService.AUTHREALM, string.Format(DekiResources.OPERATION_DENIED_FOR_ANONYMOUS, errorMsg)));
                        }
                        throw new DreamAbortException(DreamMessage.Forbidden(errorMsg));
                    }
                    return false;
                }
            }
            return true;
        }

        public static RoleBE RetrieveDefaultRoleForNewAccounts() {
            RoleBE defaultRole = null;
            
            string roleName = DekiContext.Current.Instance.NewAccountRole;
            if (!string.IsNullOrEmpty(roleName)) {

                //May be null.
                defaultRole = GetRoleByName(roleName);
            }

            return defaultRole;
        }

        public static RoleBE PutRole(RoleBE role, DreamMessage request, DreamContext context) {
            if (role == null) {
                string roleName = context.GetParam("roleid");
                
                //role name is double encoded.
                roleName = XUri.Decode(roleName);
                if (!roleName.StartsWith("=") || roleName.Substring(1).Length == 0)
                    throw new DreamBadRequestException(DekiResources.ROLD_NAME_PARAM_INVALID);
                roleName = roleName.Substring(1);
                role = new RoleBE();
                role.Name = roleName;
                role.Type = RoleType.ROLE;
            }

            List<Permissions> operations = PermissionListFromString(request.ToDocument()["operations"].AsText);
            role.PermissionFlags = MaskFromPermissionList(operations);
            role.CreatorUserId = DekiContext.Current.User.ID;
            role.TimeStamp = DateTime.UtcNow;

            uint roleId;
            if (null == GetRoleByName(role.Name)) {
                roleId = DbUtils.CurrentSession.RolesRestrictions_InsertRole(role);
                if (roleId == 0) {
                    role = null;
                } else {
                    role.ID = roleId;
                }
            } else {
                DbUtils.CurrentSession.RolesRestrictions_UpdateRole(role);
            }

            // Clear output caching due to role change
            if(DekiContext.Current.Instance.CacheAnonymousOutput) {
                DekiContext.Current.Deki.EmptyResponseCacheInternal();
            }
            return role;
        }

        public static bool ValidateRequestApiKey() {
            bool ret = false;

            string requestKey = DreamContext.Current.GetParam("apikey", string.Empty);
            if (!string.IsNullOrEmpty(requestKey)) {

                if (DekiContext.Current.Instance.ApiKey == requestKey || DekiContext.Current.Deki.MasterApiKey == requestKey) {
                    ret = true;
                }
            }

            return ret;
        }

        #region XML Helpers

        public static List<GrantBE> ReadGrantsXml(XDoc grantsXml, PageBE page, bool ignoreInvalid) {

            List<GrantBE> grants = new List<GrantBE>();
            GrantBE g = null;

            if (!grantsXml.IsEmpty) {
                if (grantsXml.HasName("grant")) {
                    try {
                        g = ReadGrantXml(grantsXml, page);
                    } catch (ArgumentException x) {
                        if (!ignoreInvalid)
                            throw new DreamAbortException(DreamMessage.BadRequest(string.Format(DekiResources.CANNOT_PARSE_GRANTS, x.Message)));
                    }
                    if (g != null) {
                        grants.Add(g);
                    }
                }
                    //If rootnode is grants, pawn off inner xml to GrantFromXml in a loop.
                else if (grantsXml.HasName("grants") || grantsXml.HasName("grants.added") || grantsXml.HasName("grants.removed")) {
                    foreach (XDoc grantXml in grantsXml["grant"]) {
                        try {
                            g = ReadGrantXml(grantXml, page);
                        } catch (ArgumentException x) {
                            if (!ignoreInvalid)
                                throw new DreamAbortException(DreamMessage.BadRequest(string.Format(DekiResources.CANNOT_PARSE_GRANTS, x.Message)));
                        }
                        if (g != null) {
                            grants.Add(g);
                        }
                    }
                }
            }
            return grants;
        }

        private static GrantBE ReadGrantXml(XDoc grantXml, PageBE pg) {
            GrantBE grant = new GrantBE();

            if (grantXml["user/@id"].Contents != string.Empty) {
                grant.UserId = DbUtils.Convert.To<uint>(grantXml["user/@id"].Contents, 0);
                grant.Type = GrantType.USER;
            }
            if (grantXml["group/@id"].Contents != string.Empty) {
                grant.GroupId = DbUtils.Convert.To<uint>(grantXml["group/@id"].Contents, 0);
                grant.Type = GrantType.GROUP;
            }
            //Userid nor Groupid given or both given is invalid.
            if ((grant.UserId == 0 && grant.GroupId == 0) || (grant.UserId != 0 && grant.GroupId != 0)) {
                throw new ArgumentException(DekiResources.USER_OR_GROUP_ID_NOT_GIVEN);
            }
            if (grantXml["permissions/role"].Contents == string.Empty)
                throw new ArgumentException(DekiResources.ROLE_NOT_GIVEN);

            RoleBE r = GetRoleByName(grantXml["permissions/role"].Contents);
            if (r == null)
                throw new ArgumentException(DekiResources.ROLE_UNRECOGNIZED);

            grant.Role = r;
            grant.RoleId = r.ID;

            //Optional datetime expire field. If provided but unparsable, return null.
            string expireString = grantXml["date.expires"].Contents;
            if (expireString != string.Empty) {
                DateTime expirationDate;
                if (!DateTime.TryParse(expireString, out expirationDate)) {
                    throw new ArgumentException(DekiResources.CANNOT_PARSE_EXPIRY);
                } else {
                    grant.ExpirationDate = expirationDate;
                }
            }

            grant.PageId = (uint)pg.ID;
            grant.CreatorUserId = DekiContext.Current.User.ID;

            return grant;
        }

        public static XDoc GetRoleXml(RoleBE role, string relation) {
            XDoc roleXml = null;
            if (role == null)
                roleXml = GetPermissionXml(0, relation);
            else {
                roleXml = GetPermissionXml(role.PermissionFlags, relation);
                roleXml.Start(role.Type.ToString().ToLowerInvariant());
                roleXml.Attr("id", role.ID);
                roleXml.Attr("href", DekiContext.Current.ApiUri.At("site", "roles", role.ID.ToString()));
                roleXml.Value(role.Name);
                roleXml.End();

            }

            return roleXml;
        }

        public static XDoc GetPermissionXml(ulong PermissionFlags, string relation) {
            XDoc roleXml = string.IsNullOrEmpty(relation) ? new XDoc("permissions") : new XDoc("permissions." + relation);

            string[] sPerms = PermissionsToArray(PermissionFlags);
            roleXml.Start("operations").Attr("mask", PermissionFlags.ToString()).Value(string.Join(",", sPerms)).End();

            return roleXml;
        }

        public static XDoc GetGrantXml(GrantBE grant) {
            XDoc doc = new XDoc("grant");
            //Permissions for the user from user role
            doc.Add(PermissionsBL.GetRoleXml(grant.Role, null));

            if (grant.Type == GrantType.USER) {
                UserBE user = UserBL.GetUserById(grant.UserId);
                if (user != null)
                    doc.Add(UserBL.GetUserXml(user, null, Utils.ShowPrivateUserInfo(user)));
            } else if (grant.Type == GrantType.GROUP) {
                GroupBE group = GroupBL.GetGroupById(grant.GroupId);
                if (group != null)
                    doc.Add(GroupBL.GetGroupXml(group, null));
            }

            if (grant.ExpirationDate != DateTime.MaxValue)
                doc.Start("date.expires").Value(grant.ExpirationDate).End();

            doc.Start("date.modified").Value(grant.TimeStamp).End();

            UserBE creatorUser = UserBL.GetUserById(grant.CreatorUserId);
            if (creatorUser != null)
                doc.Add(UserBL.GetUserXml(creatorUser, "modifiedby", Utils.ShowPrivateUserInfo(creatorUser)));

            return doc;
        }

        public static XDoc GetGrantListXml(IList<GrantBE> grants) {
            XDoc doc = new XDoc("grants");
            if (grants != null) {
                foreach (GrantBE g in grants) {
                    doc.Add(GetGrantXml(g));
                }
            }
            return doc;
        }

        #endregion

        #region User impersonation

        const string IMPERSONATED_USER_KEY = "impersonated_user";

        public static bool ImpersonationBegin(UserBE userToImpersonateAs) {
            if (userToImpersonateAs == null) {
                DekiContext.Current.Instance.Log.WarnMethodCall("ImpersonationBegin", "Aborted user impersonation because no user was given.");
                return false;
            }

            //Cannot impersonate when already impersonating.
            if(DreamContext.Current.GetState<UserBE>(IMPERSONATED_USER_KEY) != null){
                DekiContext.Current.Instance.Log.WarnMethodCall("ImpersonationBegin", "Aborted user impersonation because a user is already being impersonated.");
                return false;
            }

            DreamContext.Current.SetState(IMPERSONATED_USER_KEY, DekiContext.Current.User);
            DekiContext.Current.User = userToImpersonateAs;
            return true;
        }

        public static bool ImpersonationBeginOfAdmin() {
            UserBE userToImpersonateAs = null;
            string[] usersToLookup = null;
            if (string.IsNullOrEmpty(DekiContext.Current.Instance.AdminUserForImpersonation)) {
                usersToLookup = new string[] { "sysop", "admin" };
            } else {
                usersToLookup = new string[] { DekiContext.Current.Instance.AdminUserForImpersonation };
            }

            foreach (string u in usersToLookup) {
                userToImpersonateAs = DbUtils.CurrentSession.Users_GetByName(u);
                if (userToImpersonateAs != null) {
                    break;
                }            
            }

            if (userToImpersonateAs == null) {
                throw new DreamInternalErrorException(string.Format("Failed to lookup an administrator for impersonation. Please set 'security/admin-user-for-impersonation' to the name of the admin user to impersonate. Tried: {0}", string.Join(",", usersToLookup)));
            }

            return ImpersonationBegin(userToImpersonateAs);
        }

        public static void ImpersonationEnd() {
            UserBE impersonatedUser = DreamContext.Current.GetState<UserBE>(IMPERSONATED_USER_KEY);
            if (impersonatedUser != null) {
                DreamContext.Current.SetState<UserBE>(IMPERSONATED_USER_KEY, null);
                DekiContext.Current.User = impersonatedUser;
            }
        }

        #endregion

        #region Helper methods for debugging

        private static string PermissionsToString(params Permissions[] actions) {
            ulong mask = MaskFromPermissionList(actions);
            List<Permissions> pList = PermissionListFromMask(mask);
            List<string> sList = new List<string>();
            foreach (Permissions action in pList) {
                sList.Add(action.ToString());
            }

            return "{" + string.Join(",", sList.ToArray()) + "}";
        }

        private static string PermissionsToString(ulong permissionMask) {
            List<string> sList = new List<string>();
            foreach (Permissions action in PermissionListFromMask(permissionMask)) {
                sList.Add(action.ToString());
            }

            return "{" + string.Join(",", sList.ToArray()) + "}";
        }

        public static string[] PermissionsToArray(ulong permissionMask) {
            List<string> sList = new List<string>();
            foreach (Permissions action in PermissionListFromMask(permissionMask)) {
                sList.Add(action.ToString());
            }

            return sList.ToArray();
        }

        public static List<Permissions> PermissionListFromMask(ulong permissionMask) {
            List<Permissions> ret = new List<Permissions>();
            if (permissionMask == ulong.MinValue)
                return ret;

            foreach (Permissions p in Enum.GetValues(typeof(Permissions))) {
                if ((permissionMask & (ulong) p) != 0 && p != Permissions.NONE)
                    ret.Add(p);
            }

            return ret;
        }

        public static List<Permissions> PermissionListFromString(string operations) {
            if (string.IsNullOrEmpty(operations))
                return new List<Permissions>();

            List<Permissions> pList = new List<Permissions>();

            foreach(string operation in operations.Split(',')){
                string op = operation.Trim();
                if(op.EqualsInvariantIgnoreCase("ALL")) {
                    pList.Add(PermissionSets.ALL);
                } else {
                    pList.Add(SysUtil.ParseEnum<Permissions>(op));                    
                }
            }

            return pList;
        }

        public static ulong MaskFromPermissionList(IList<Permissions> permissionList) {
            Permissions ret = Permissions.NONE;

            if (permissionList != null) {
                foreach (Permissions p in permissionList) {
                    ret |= p;
                }
            }
            return (ulong) ret;
        }

        public static Permissions MaskFromString(string operations) {
            return (Permissions)MaskFromPermissionList(PermissionListFromString(operations));
        }

        #endregion

        private static Dictionary<ulong, PermissionStruct> CalculateEffectiveForPages(UserBE user, IEnumerable<ulong> pageIds) {
            Dictionary<ulong, PermissionStruct> result = DbUtils.CurrentSession.Grants_CalculateEffectiveForPages(user.ID, pageIds);
            if(UserBL.IsAnonymous(user) && (result != null)) {
                ulong mask = LicenseBL.AnonymousUserMask(user);
                foreach(var check in result.ToArray()) {
                    var value = check.Value;
                    value.UserPermissions = (Permissions)((ulong)check.Value.UserPermissions & mask);
                    result[check.Key] = value;
                }
            }
            return result;
        }
    }
}
