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

using MindTouch.Deki.Data;
using MindTouch.Dream;

namespace MindTouch.Deki.Logic {
    public class ResourceBL<ResourceType> where ResourceType : ResourceBE {

        // -- Fields -- 
        ResourceBE.Type _resourceType;

        // -- Constructors --
        public ResourceBL(ResourceBE.Type defaultResourceType) {
            _resourceType = defaultResourceType;
        }

        // -- Properties --
        private ResourceBE.Type[] ResourceTypeArr {
            get {
                return new ResourceBE.Type[] { _resourceType };
            }
        }

        // -- Instance methods --

        #region ResourceDA query wrappers

        private IList<ResourceBE> GetResourcesInternal(IList<uint> parentids, ResourceBE.Type parentType, IList<ResourceBE.Type> resourceTypes, IList<string> names, DeletionFilter deletionStateFilter, bool? populateRevisions, uint? limit, uint? offset) {
            return DbUtils.CurrentSession.Resources_GetByQuery(parentids, parentType, resourceTypes, names, deletionStateFilter, populateRevisions, offset, limit);
        }

        private IList<ResourceType> GetResourcesInternalWithCast(uint[] parentids, ResourceBE.Type parentType, ResourceBE.Type[] resourceTypes, string[] names, DeletionFilter deletionStateFilter, bool? populateRevisions, uint? limit, uint? offset) {
            return Convert(GetResourcesInternal(parentids, parentType, resourceTypes, names, deletionStateFilter, populateRevisions, limit, offset));
        }

        private ResourceBE GetResourceInternal(uint? parentId, ResourceBE.Type parentType, ResourceBE.Type resourceType, string name, DeletionFilter deletionStateFilter) {
            uint[] parentids = parentId == null ? new uint[] { } : new uint[] { parentId.Value };
            return GetResourcesInternal(parentids, parentType, new ResourceBE.Type[] { resourceType }, new string[] { name }, deletionStateFilter, null, null, null).FirstOrDefault();
        }

        /// <summary>
        /// Active child resources of given type
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="parentType"></param>
        /// <returns></returns>
        public IList<ResourceType> GetResources(uint? parentId, ResourceBE.Type parentType) {
            uint[] parentids = parentId == null ? new uint[] { } : new uint[] { parentId.Value };
            return GetResourcesInternalWithCast(parentids, parentType, ResourceTypeArr, null, DeletionFilter.ACTIVEONLY, null, null, null);
        }

        public IList<ResourceType> GetResources(uint? parentId, ResourceBE.Type parentType, string[] names, DeletionFilter deletionStateFilter) {
            uint[] parentids = parentId == null ? new uint[] { } : new uint[] { parentId.Value };
            return GetResourcesInternalWithCast(parentids, parentType, ResourceTypeArr, names, deletionStateFilter, null, null, null);
        }

        public IList<ResourceType> GetResources(uint[] parentids, ResourceBE.Type parentType, string[] names, DeletionFilter deletionStateFilter) {
            return GetResourcesInternalWithCast(parentids, parentType, ResourceTypeArr, names, deletionStateFilter, null, null, null);
        }

        public IList<ResourceType> GetResources(DeletionFilter deletionStateFilter, uint? limit, uint? offset) {
            return GetResourcesInternalWithCast(null, ResourceBE.Type.UNDEFINED, ResourceTypeArr, null, deletionStateFilter, null, limit, offset);
        }
        public IList<ResourceType> GetResources(DeletionFilter deletionStateFilter, string name) {
            return GetResourcesInternalWithCast(null, ResourceBE.Type.UNDEFINED, ResourceTypeArr, new string[] { name }, deletionStateFilter, null, null, null);
        }

        public IList<ResourceType> GetResources(PageBE parentPage, DeletionFilter deletionStateFilter) {
            return GetResources(new PageBE[] { parentPage }, deletionStateFilter);
        }

        public IList<ResourceType> GetResources(PageBE[] parentPages, DeletionFilter deletionStateFilter) {
            uint[] pageIds = parentPages.Select(e => (uint)e.ID).ToArray();
            IList<ResourceType> resources = GetResourcesInternalWithCast(pageIds, ResourceBE.Type.PAGE, ResourceTypeArr, null, deletionStateFilter, null, null, null);
            if(!ArrayUtil.IsNullOrEmpty(resources)) {

                Dictionary<ulong, PageBE> pagesById = parentPages.AsHash(e => e.ID);
                foreach(ResourceType res in resources) {
                    PageBE p;
                    if(pagesById.TryGetValue((ulong) (res.ParentPageId ?? 0), out p)) {
                        res.ParentResource = new PageWrapperBE(p);
                    }
                }
            }

            return resources;
        }

        public IList<ResourceType> GetResourcesByChangeSet(uint changeSetId) {
            IList<ResourceBE> ret = DbUtils.CurrentSession.Resources_GetByChangeSet(changeSetId, _resourceType).ToArray();
            return Convert(ret);
        }

        public ResourceType GetResource(PageBE parentPage, string name, DeletionFilter deletionStateFilter) {
            ResourceType res = Convert(GetResourceInternal((uint) parentPage.ID, ResourceBE.Type.PAGE, _resourceType, name, deletionStateFilter));
            if(res != null) {
                res.ParentResource = new PageWrapperBE(parentPage);
            }
            return res;
        }

        public ResourceType GetResource(PageBE parentPage, string name, DeletionFilter deletionStateFilter, int revision) {
            ResourceBE res = GetResourceInternal((uint) parentPage.ID, ResourceBE.Type.PAGE, _resourceType, name, deletionStateFilter);
            if(res != null && revision != ResourceBE.HEADREVISION) {
                res = GetResource(res.ResourceId, revision);
            }

            if(res != null) {
                res.ParentResource = new PageWrapperBE(parentPage);
            }

            return Convert(res);
        }


        public ResourceType GetResource(uint? parentId, ResourceBE.Type parentType, string name) {
            return Convert(GetResourceInternal(parentId, parentType, _resourceType, name, DeletionFilter.ACTIVEONLY));
        }

        public ResourceType GetResource(uint? parentid, ResourceBE.Type parentType, string name, int revision) {
            ResourceType res = Convert(GetResourceInternal(parentid, parentType, _resourceType, name, DeletionFilter.ACTIVEONLY));

            //Requested head revision already retrieved
            if(revision == ResourceBE.HEADREVISION || res == null) {
                return res;
            } else {

                //Lookup specific revision
                return Convert(GetResource(res.ResourceId, revision));
            }
        }

        public ResourceType GetResource(uint resourceid) {
            ResourceBE ret = DbUtils.CurrentSession.Resources_GetByIdAndRevision(resourceid, ResourceBE.HEADREVISION);
            return Convert(ret);
        }

        public ResourceType GetResource(uint resourceid, int revision) {
            ResourceBE ret = DbUtils.CurrentSession.Resources_GetByIdAndRevision(resourceid, revision);
            return Convert(ret);
        }

        public IList<ResourceType> GetResourceRevisions(uint resourceid, ResourceBE.ChangeOperations changeTypesFilter, SortDirection sortRevisions, uint? limit) {
            IList<ResourceBE> ret = DbUtils.CurrentSession.Resources_GetRevisions(resourceid, changeTypesFilter, sortRevisions, limit).ToArray();
            return Convert(ret);
        }

        public Dictionary<Title, AttachmentBE> GetFileResourcesByTitlesWithMangling(Title[] titles) {
            if(ArrayUtil.IsNullOrEmpty(titles)) {
                return new Dictionary<Title, AttachmentBE>();
            }
            return DbUtils.CurrentSession.Resources_GetFileResourcesByTitlesWithMangling(titles);
        }

        public Dictionary<ulong, IList<ResourceType>> GetResourcesGroupedByParent(uint[] parentIds, ResourceBE.Type parentType) {
            IList<ResourceBE> resources = GetResourcesInternal(parentIds, parentType, new ResourceBE.Type[] { _resourceType }, null, DeletionFilter.ACTIVEONLY, null, null, null);
            return GroupByParentIdWithCast(resources, parentType);
        }

        public IList<ResourceBE> PopulateChildren(ResourceBE[] resources, ResourceBE.Type[] resourceTypes, bool associateRevisions) {
            if(ArrayUtil.IsNullOrEmpty(resources)) {
                return resources;
            }

            Dictionary<uint, object> resourceIdsHash = new Dictionary<uint, object>();
            foreach(ResourceType r in resources) {
                resourceIdsHash[r.ResourceId] = null;
            }
            uint[] resourceIds = new List<uint>(resourceIdsHash.Keys).ToArray();

            IList<ResourceBE> children = GetResourcesInternal(resourceIds, ResourceBE.Type.UNDEFINED, resourceTypes, null, DeletionFilter.ACTIVEONLY, associateRevisions, null, null);
            Dictionary<ulong, IList<ResourceBE>> childrenByParentId = GroupByParentId(children, ResourceBE.Type.UNDEFINED);
            if(!associateRevisions) {

                //Head revisions of all children of the resource are associated                                 
                foreach(ResourceBE parent in resources) {
                    if(childrenByParentId.TryGetValue(parent.ResourceId, out children)) {
                        parent.ChildResources = children.ToArray();
                    }
                }
            } else {

                //Corresponding revisions of all children of the resource are associated

                foreach(ResourceBE parent in resources) {
                    if(childrenByParentId.TryGetValue(parent.ResourceId, out children)) {
                        List<ResourceBE> childrenAtRevs = new List<ResourceBE>();
                        Dictionary<uint, ResourceBE[]> revsForChild = GroupByResourceId(children);
                        foreach(KeyValuePair<uint, ResourceBE[]> childRevSet in revsForChild) {
                            ResourceBE[] childRevisions = childRevSet.Value;
                            
                            //Ensure that the revisions are sorted by rev# desc. Index 
                            Array.Sort<ResourceBE>(childRevisions, delegate(ResourceBE left, ResourceBE right) {
                                return right.Revision.CompareTo(left.Revision);
                            });

                            if(parent.IsHeadRevision()) {
                                //Head revision of parent should use head revision of children

                                if(childRevisions.Length > 0) {
                                    childrenAtRevs.Add(childRevisions[0]);
                                }

                            } else {
                                //Determine the highest timestamp a child property revision can have
                                DateTime timestampOfNextRev = DateTime.MaxValue;
                                
                                //Revision HEAD - 1 can use the HEAD resources timestamp
                                if(parent.Revision + 1 == parent.ResourceHeadRevision) {
                                    timestampOfNextRev = parent.ResourceUpdateTimestamp;
                                } else {
                                    //Determine timestamp of next revision

                                    ResourceBE nextRev = null;
                                    
                                    //Look to see if current list of resources contains the next revision
                                    foreach(ResourceBE r in resources) {
                                        if(parent.ResourceId == r.ResourceId && parent.Revision + 1 == r.Revision) {
                                            nextRev = r;
                                            break;
                                        }
                                    }

                                    //Perform DB call to retrieve next revision
                                    if(nextRev == null){
                                        nextRev = GetResource(parent.ResourceId, parent.Revision + 1);
                                    }

                                    if(nextRev != null) {
                                        timestampOfNextRev = nextRev.Timestamp;
                                    }
                                }

                                //Get most recent revision of child before the timestamp of the next revision of parent.
                                for(int i = 0; i < childRevisions.Length; i++) {
                                    if(timestampOfNextRev > childRevisions[i].Timestamp) {
                                        childrenAtRevs.Add(childRevisions[i]);
                                        break;
                                    }
                                }
                            }
                        }
                        parent.ChildResources = childrenAtRevs.ToArray();
                    }
                }                
            }

            return resources;
        }

        public uint GetResourceRevisionCount(uint resourceid, ResourceBE.ChangeOperations changeTypesFilter) {
            return DbUtils.CurrentSession.Resources_GetRevisionCount(resourceid, changeTypesFilter);
        }

        
        #endregion

        #region Actions on resources

        protected virtual ResourceType Delete(ResourceType resource) {
            return Delete(resource, null, 0);
        }

        protected virtual ResourceType Delete(ResourceType resource, PageBE parentPage, uint changeSetId) {

            //Build the new revision
            ResourceType res = BuildRevForRemove(resource, DateTime.UtcNow, changeSetId);

            //Update db
            res = SaveResource(res);

            //Update indexes and parent page's timestamp

            //TODO MaxM: Changesink needs to accept a resource
            if(res.ResourceType == ResourceBE.Type.FILE) {
                DekiContext.Current.Instance.EventSink.AttachmentDelete(DreamContext.Current.StartTime, res as AttachmentBE, DekiContext.Current.User);

                // Recent changes
                string logDescription = string.Format(DekiResources.FILE_REMOVED, res.Name);
                RecentChangeBL.AddFileRecentChange(DreamContext.Current.StartTime, parentPage, DekiContext.Current.User, logDescription, changeSetId);
            }

            if(parentPage != null) {
                PageBL.Touch(parentPage, DateTime.UtcNow);
            }

            return res;
        }

        protected virtual ResourceType SaveResource(ResourceType res) {
            ResourceType ret = Convert(DbUtils.CurrentSession.Resources_SaveRevision(res));
            ReAssociateResourceState(res, ret);
            return ret;
        }

        protected virtual ResourceType UpdateResourceRevision(ResourceType res) {
            ResourceType ret = Convert(DbUtils.CurrentSession.Resources_UpdateRevision(res));
            ReAssociateResourceState(res, ret);
            return ret;
        }

        private void ReAssociateResourceState(ResourceType oldRev, ResourceType newRev) {
            if(oldRev == null || newRev == null) {
                return;
            }

            //Reassociate parent resource on new revision
            newRev.ParentResource = oldRev.ParentResource;
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Returns a hash by parentid's based on parentType of lists of child resources
        /// </summary>
        /// <param name="resources"></param>
        /// <param name="parentType"></param>
        /// <returns></returns>
        public Dictionary<ulong, IList<ResourceType>> GroupByParentIdWithCast(IEnumerable<ResourceBE> resources, ResourceBE.Type parentType) {

            Dictionary<ulong, List<ResourceBE>> temp = GroupByParentIdInternal(resources, parentType);
            Dictionary<ulong, IList<ResourceType>> ret = new Dictionary<ulong, IList<ResourceType>>();

            //Convert lists to arrays
            foreach(KeyValuePair<ulong, List<ResourceBE>> kvp in temp) {
                ret[kvp.Key] = Convert(kvp.Value);
            }

            return ret;
        }

        public Dictionary<ulong, IList<ResourceBE>> GroupByParentId(IEnumerable<ResourceBE> resources, ResourceBE.Type parentType) {

            Dictionary<ulong, List<ResourceBE>> temp = GroupByParentIdInternal(resources, parentType);
            Dictionary<ulong, IList<ResourceBE>> ret = new Dictionary<ulong, IList<ResourceBE>>();

            //Convert lists to arrays
            foreach(KeyValuePair<ulong, List<ResourceBE>> kvp in temp) {
                ret[kvp.Key] = kvp.Value;
            }

            return ret;
        }

        private Dictionary<ulong, List<ResourceBE>> GroupByParentIdInternal(IEnumerable<ResourceBE> resources, ResourceBE.Type parentType) {
            Dictionary<ulong, List<ResourceBE>> temp = new Dictionary<ulong, List<ResourceBE>>();
            foreach(ResourceBE res in resources) {
                ulong? id = null;
                switch(parentType) {
                case ResourceBE.Type.PAGE:
                    id = res.ParentPageId;
                    break;
                case ResourceBE.Type.USER:
                    id = res.ParentUserId;
                    break;
                default:
                    id = res.ParentId;
                    break;
                }

                if((id ?? 0) != 0) {
                    if(!temp.ContainsKey(id.Value)) {
                        temp[id.Value] = new List<ResourceBE>();
                    }

                    temp[id.Value].Add(res);
                }
            }

            return temp;
        }

        public Dictionary<uint, ResourceBE[]> GroupByResourceId(IEnumerable<ResourceBE> resources) {
            Dictionary<uint, List<ResourceBE>> temp = GroupByResourceIdInternal(resources);
            Dictionary<uint, ResourceBE[]> ret = new Dictionary<uint, ResourceBE[]>();

            //Convert lists to arrays
            foreach(KeyValuePair<uint, List<ResourceBE>> kvp in temp) {
                ret[kvp.Key] = kvp.Value.ToArray();
            }

            return ret;
        }

        private Dictionary<uint, List<ResourceBE>> GroupByResourceIdInternal(IEnumerable<ResourceBE> resources) {
            Dictionary<uint, List<ResourceBE>> temp = new Dictionary<uint, List<ResourceBE>>();
            foreach(ResourceBE res in resources) {
                if(!temp.ContainsKey(res.ResourceId)) {
                    temp[res.ResourceId] = new List<ResourceBE>();
                }

                temp[res.ResourceId].Add(res);
            }

            return temp;
        }

        public bool ValidateEtag(string etag, ResourceBE headResource, bool throwException) {
            if(!headResource.IsHeadRevision()) {
                throw new ArgumentException(string.Format("Given resource (id: {0} rev: {1}) for ETag evaluation is not HEAD revisions.", headResource.ResourceId, headResource.Revision));
            }

            bool isValid = false;
            if(etag != null && StringUtil.EqualsInvariant(etag, headResource.ETag())) {
                isValid = true;
            }

            if(!isValid && throwException) {
                throw new DreamAbortException(DreamMessage.Conflict(string.Format("Given ETag '{0}' for resource id '{1}' does not match ETag for HEAD revision.", etag ?? string.Empty, headResource.ResourceId)));
            } else {
                return isValid;
            }
        }

        private IList<ResourceType> Convert(IList<ResourceBE> resources) {
            return resources.Cast<ResourceType>().ToList();
        }

        private ResourceType Convert(ResourceBE res) {

            // TODO (steveb): throw exception if conversion fails
            return res as ResourceType;
        }

        #endregion

        #region Protected resource revision building methods

        protected virtual ResourceType BuildRevForMoveAndRename(ResourceType currentResource, PageBE targetPage, string name, uint changeSetId) {
            ResourceType newRev = BuildResourceRev(currentResource);

            //NOTE MaxM: This logic exists here since BuildResourceRev clears out fields preventing chaining of entity building for separate actions on one revision
            if(targetPage != null && (uint) targetPage.ID != newRev.ParentPageId.Value) {
                newRev.ParentPageId = (uint) targetPage.ID;
                newRev.ChangeMask |= ResourceBE.ChangeOperations.PARENT;
            }

            if(name != null && !StringUtil.EqualsInvariant(name, currentResource.Name)) {
                newRev.Name = name;
                newRev.ChangeMask |= ResourceBE.ChangeOperations.NAME;
            }

            newRev.ChangeSetId = changeSetId;
            return newRev;
        }

        protected virtual ResourceType BuildRevForRemove(ResourceType currentResource, DateTime timestamp, uint changeSetId) {
            ResourceType newRev = BuildResourceRev(currentResource);
            newRev.ResourceIsDeleted = true;
            newRev.Deleted = true;
            newRev.ChangeSetId = changeSetId;
            newRev.Timestamp = timestamp;
            newRev.ChangeMask = newRev.ChangeMask | ResourceBE.ChangeOperations.DELETEFLAG;
            return newRev;
        }

        protected virtual ResourceType BuildRevForRestore(ResourceType currentResource, PageBE targetPage, string resourceName, uint changeSetId) {
            ResourceType newRev = BuildResourceRev(currentResource);
            newRev.ResourceIsDeleted = false;
            newRev.ChangeSetId = changeSetId;
            newRev.ParentPageId = (uint) targetPage.ID;
            newRev.ParentResource = new PageWrapperBE(targetPage);
            newRev.Name = resourceName;
            newRev.ChangeMask = newRev.ChangeMask | ResourceBE.ChangeOperations.DELETEFLAG;
            return newRev;
        }

        public virtual ResourceType BuildRevForNewResource(ResourceBE parentResource, string resourcename, MimeType mimeType, uint size, string description, ResourceBE.Type resourceType, uint userId, ResourceContentBE content) {
            uint parentId;
            switch(parentResource.ResourceType) {
            case ResourceBE.Type.PAGE:
                parentId = (uint) ((PageWrapperBE) parentResource).Page.ID;
                break;
            default:
                parentId = parentResource.ResourceId;
                break;
            }
            ResourceType res = BuildRevForNewResource(parentId, parentResource.ResourceType, resourcename, mimeType, size, description, resourceType, userId, content);
            res.ParentResource = parentResource;
            return res;
        }

        public virtual ResourceType BuildRevForNewResource(uint? parentId, ResourceBE.Type parentType, string resourcename, MimeType mimeType, uint size, string description, ResourceBE.Type resourceType, uint userId, ResourceContentBE content) {
            ResourceType newResource = BuildRevForNewResource(resourcename, mimeType, size, description, resourceType, userId, content);
            switch(parentType) {
            case ResourceBE.Type.PAGE:
                newResource.ParentPageId = parentId;
                break;
            case ResourceBE.Type.USER:
                newResource.ParentUserId = parentId;
                break;
            default:
                newResource.ParentId = parentId;
                break;
            }
            newResource.ChangeMask = newResource.ChangeMask | ResourceBE.ChangeOperations.PARENT;
            return newResource;
        }

        private ResourceType BuildRevForNewResource(string resourcename, MimeType mimeType, uint size, string description, ResourceBE.Type resourceType, uint userId, ResourceContentBE content) {
            ResourceType newResource = Convert(ResourceBE.New(resourceType));
            newResource.ResourceType = resourceType;
            newResource.Name = resourcename;
            newResource.ChangeMask = newResource.ChangeMask | ResourceBE.ChangeOperations.NAME;
            newResource.Size = size;
            newResource.MimeType = mimeType;
            newResource.ChangeDescription = description;
            newResource.ResourceCreateUserId = newResource.ResourceUpdateUserId = newResource.UserId = userId;
            newResource.ChangeSetId = 0;
            newResource.Timestamp = newResource.ResourceCreateTimestamp = newResource.ResourceUpdateTimestamp = DateTime.UtcNow;
            newResource.Content = content;
            newResource.ChangeMask = newResource.ChangeMask | ResourceBE.ChangeOperations.CONTENT;
            newResource.ResourceHeadRevision = ResourceBE.TAILREVISION;
            newResource.Revision = ResourceBE.TAILREVISION;
            newResource.IsHidden = false;
            return newResource;
        }

        private ResourceType BuildRevForExistingResource(ResourceType currentResource, MimeType mimeType, uint size, string description) {
            ResourceType newRev = BuildResourceRev(currentResource);
            newRev.MimeType = mimeType;
            newRev.Size = size;
            newRev.ChangeDescription = description;
            newRev.Content = currentResource.Content;
            newRev.ContentId = currentResource.ContentId;
            return newRev;
        }

        protected virtual ResourceType BuildRevForContentUpdate(ResourceType currentResource, MimeType mimeType, uint size, string description, string name, ResourceContentBE newContent) {
            ResourceType newRev = BuildRevForExistingResource(currentResource, mimeType, size, description);
            newRev.Content = newContent;
            newRev.ContentId = 0;
            newRev.ChangeMask |= ResourceBE.ChangeOperations.CONTENT;
            if(name != null && !StringUtil.EqualsInvariant(name, newRev.Name)) {                
                newRev.ChangeMask |= ResourceBE.ChangeOperations.NAME;
                newRev.Name = name;
            }
            return newRev;
        }

        protected virtual ResourceType BuildResourceRev(ResourceType currentRevision) {
            currentRevision.AssertHeadRevision();

            //Clone the resource revision            
            ResourceType newRev = Convert(ResourceBE.NewFrom(currentRevision));

            //Initialize for new revision (clear anything that shouldn't be carried over from current rev)
            newRev.ChangeMask = ResourceBE.ChangeOperations.UNDEFINED;
            newRev.Timestamp = DateTime.UtcNow;
            newRev.UserId = DekiContext.Current.User.ID;
            newRev.ChangeSetId = 0;
            newRev.Deleted = false;
            newRev.ChangeDescription = null;
            newRev.IsHidden = false;
            newRev.Meta = string.Empty;
            return newRev;
        }

        #endregion
    }
}
