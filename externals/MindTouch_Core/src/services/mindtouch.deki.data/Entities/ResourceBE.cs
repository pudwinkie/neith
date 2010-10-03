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

using MindTouch.Dream;
using MindTouch.Xml;

namespace MindTouch.Deki.Data {
    public class ResourceBE {

        // -- Constants --
        public const int HEADREVISION = 0; //Used for lookups for latest revision of a file
        public const int TAILREVISION = 1; //Starting revision number for a file
        public const string ATTRIBUTE_ROOT = "attr";
        public const string META_REVHIDE_USERID = "rev-hidden-user-id";
        public const string META_REVHIDE_TS = "rev-hidden-ts";
        public const string META_REVHIDE_COMMENT = "rev-hidden-comment";

        [Flags]
        public enum ChangeOperations : ushort {
            UNDEFINED = 0,
            CONTENT = 1,    //The content was created or updated
            NAME = 2,       //The name of the resource was changed
            LANGUAGE = 4,   //Language got set or updated
            META = 8,       //The meta attributes of the resource were updates
            DELETEFLAG = 16,//The resource delete flag was changed. Resource is either deleted or restored
            PARENT = 32     //The resource moved to a different parent
        }

        public enum Type : byte {
            UNDEFINED = 0,
            PAGE = 1,
            FILE = 2,
            USER = 3,
            PROPERTY = 4,
            SITE = 5
        };

        // -- Constructors --
        protected ResourceBE() {
            _content = new ResourceContentBE(true);
        }

        protected ResourceBE(ResourceBE sourceRev) {

            ResourceId = sourceRev.ResourceId;
            Revision = sourceRev.Revision;
            ChangeDescription = sourceRev.ChangeDescription;
            Name = sourceRev.Name;
            Timestamp = sourceRev.Timestamp;
            ChangeMask = sourceRev.ChangeMask;
            UserId = sourceRev.UserId;
            ChangeSetId = sourceRev.ChangeSetId;
            Deleted = sourceRev.Deleted;
            ParentId = sourceRev.ParentId;
            ParentPageId = sourceRev.ParentPageId;
            ParentUserId = sourceRev.ParentUserId;
            ParentResource = sourceRev.ParentResource;
            ChildResources = sourceRev.ChildResources;
            
            //Revision contents
            Size = sourceRev.Size;
            MimeType = sourceRev.MimeType;
            Content = sourceRev.Content;
            ContentId = sourceRev.ContentId;
            Meta = sourceRev.Meta;

            //Resource info
            ResourceIsDeleted = sourceRev.ResourceIsDeleted;
            ResourceUpdateUserId = sourceRev.ResourceUpdateUserId;
            ResourceUpdateTimestamp = sourceRev.ResourceUpdateTimestamp;
            ResourceHeadRevision = sourceRev.ResourceHeadRevision;
            ResourceCreateUserId = sourceRev.ResourceCreateUserId;
            ResourceCreateTimestamp = sourceRev.ResourceCreateTimestamp;
            ResourceType = sourceRev.ResourceType;
        }

        // -- Members --
        //Revision related
        int _revision;
        uint _userId;
        ChangeOperations _changeMask;
        string _name;
        string _description;
        DateTime _timestamp;
        uint? _changeSetId;
        bool _deleted;
        string _metaStr;
        uint _size;
        MimeType _mimeType;
        uint? _parentId;
        ulong? _parentPageId;
        uint? _parentUserId;
        uint _contentId;
        string _language;
        bool _isHidden;
        ResourceContentBE _content;
        ResourceBE _parentResource;
        ResourceBE[] _childResources;

        //Resource related
        uint _resourceId;
        ResourceBE.Type _resourceType;
        int _resourceHeadRevision;
        bool _resourceIsDeleted;
        uint _resourceCreateUserId;
        uint _resourceUpdateUserId;
        DateTime _resourceCreateTimestamp;
        DateTime _resourceUpdateTimestamp;

        // -- Properties -- 
        #region Resource related properties
        public uint ResourceId {
            get { return _resourceId; }
            set { _resourceId = value; }
        }

        public ResourceBE.Type ResourceType {
            get { return _resourceType; }
            set { _resourceType = value; }
        }

        public int ResourceHeadRevision {
            get { return _resourceHeadRevision; }
            set { _resourceHeadRevision = value; }
        }

        public bool ResourceIsDeleted {
            get { return _resourceIsDeleted; }
            set { _resourceIsDeleted = value; }
        }

        public uint ResourceCreateUserId {
            get { return _resourceCreateUserId; }
            set { _resourceCreateUserId = value; }
        }

        public uint ResourceUpdateUserId {
            get { return _resourceUpdateUserId; }
            set { _resourceUpdateUserId = value; }
        }

        public DateTime ResourceCreateTimestamp {
            get { return _resourceCreateTimestamp; }
            set { _resourceCreateTimestamp = value; }
        }

        public DateTime ResourceUpdateTimestamp {
            get { return _resourceUpdateTimestamp; }
            set { _resourceUpdateTimestamp = value; }
        }

        #endregion

        #region Resource revision related properties
        public int Revision {
            get { return _revision; }
            set { _revision = value; }
        }

        public uint UserId {
            get { return _userId; }
            set { _userId = value; }
        }

        public ChangeOperations ChangeMask {
            get { return _changeMask; }
            set { _changeMask = value; }
        }

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public string ChangeDescription {
            get { return _description; }
            set { _description = value; }
        }

        public DateTime Timestamp {
            get { return _timestamp; }
            set { _timestamp = value; }
        }

        public uint? ChangeSetId {
            get { return _changeSetId; }
            set { _changeSetId = value; }
        }

        public bool Deleted {
            get { return _deleted; }
            set { _deleted = value; }
        }

        public uint Size {
            get { return _size; }
            set { _size = value; }
        }

        public MimeType MimeType {
            get { return _mimeType; }
            set { _mimeType = value; }
        }

        public string Language {
            get { return _language; }
            set { _language = value; }
        }

        public string Meta {
            get {
                if(_metaTempDoc != null) {
                    _metaStr = _metaTempDoc.ToString();
                }
                return _metaStr;
            }
            set {
                _metaStr = value;
                _metaTempDoc = null;
            }
        }

        public bool IsHidden {
            get { return _isHidden; }
            set { _isHidden = value; }
        }

        public uint ContentId {
            get { return _contentId; }
            set { _contentId = value; }
        }

        public ResourceContentBE Content {
            get { return _content; }
            set { _content = value; }
        }

        public uint? ParentId {
            get { return _parentId; }
            set { _parentId = value; }
        }

        public ulong? ParentPageId {
            get { return _parentPageId; }
            set { _parentPageId = value; }
        }

        public uint? ParentUserId {
            get { return _parentUserId; }
            set { _parentUserId = value; }
        }

        public ResourceBE ParentResource {
            get { return _parentResource; }
            set { _parentResource = value; }
        }

        public ResourceBE[] ChildResources {
            get { return _childResources; }
            set { _childResources = value; }
        }

        #endregion

        // -- Derived properties --
        XDoc _metaTempDoc = null;
        public XDoc MetaXml {
            get {
                XDoc ret = null;

                if(_metaTempDoc != null) {
                    ret = _metaTempDoc;
                } else if(string.IsNullOrEmpty(_metaStr)) {
                    ret = _metaTempDoc = new XDoc(ATTRIBUTE_ROOT);
                } else {
                    ret = _metaTempDoc = XDocFactory.From(_metaStr, MimeType.XML);
                }
                return _metaTempDoc;
            }
        }

        // -- Static methods --
        public static ResourceBE New(ResourceBE.Type resourceType) {
            return New(resourceType, null);
        }

        public static ResourceBE NewFrom(ResourceBE res) {
            return New(res.ResourceType, res);
        }

        private static ResourceBE New(ResourceBE.Type resourceType, ResourceBE source) {
            ResourceBE res = null;
            switch(resourceType) {
            case ResourceBE.Type.PROPERTY:

                res = source == null ? new PropertyBE() : new PropertyBE(source);
                break;
            case ResourceBE.Type.FILE:
                res = source == null ? new AttachmentBE() : new AttachmentBE(source);
                break;
            case ResourceBE.Type.PAGE:
                //todo
                break;
            default:
                res = source == null ? new ResourceBE() : new ResourceBE(source);
                break;
            }
            return res;
        }

        // -- Instance methods --
        public bool IsHeadRevision() {
            if(Revision > ResourceHeadRevision) {
                throw new DreamInternalErrorException(string.Format("Revision out of range on resource: [{0}]", ToString()));
            }
            return ResourceHeadRevision == Revision;
        }

        public void AssertHeadRevision() {
            if(!IsHeadRevision()) {
                throw new ArgumentException(string.Format("Expected HEAD revision of resource (revision '{0}') but have revision '{1}'", ResourceHeadRevision, Revision));
            }
        }

        public bool IsNewResource() {
            return ResourceId == 0;
        }

        public virtual string ETag() {
            string etag = string.Format("{0}.r{1}_ts{2}", ResourceId, Revision, Timestamp.ToString(XDoc.RFC_DATETIME_FORMAT));
            return etag;
        }

        protected void SetAttribute(string name, object value) {
            if(value == null) {
                MetaXml[name].Remove();
            } else {
                if(MetaXml[name].IsEmpty) {
                    MetaXml.Elem(name, value);
                } else {
                    MetaXml[name].ReplaceValue(value);
                }
            }
        }

        public override string ToString() {
            string s = string.Format("Res ID:{0} Rev:{1} HeadRev:{2} Type:{3} Name:{4} MimeType:{5} Size:{6}", ResourceId, Revision, ResourceHeadRevision, ResourceType.ToString(), Name, MimeType, Size);
            return s;
        }
    }
}
