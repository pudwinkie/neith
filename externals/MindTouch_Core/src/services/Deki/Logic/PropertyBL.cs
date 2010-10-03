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
using System;

using MindTouch.Deki.Data;
using MindTouch.Dream;
using MindTouch.IO;
using MindTouch.Xml;

namespace MindTouch.Deki.Logic {
    public enum AbortEnum {
        Never,
        Exists,
        Modified
    }

    public class PropertyBL : ResourceBL<PropertyBE> {

        // -- Constants --
        public const uint DEFAULT_CONTENT_CUTOFF = 1024 * 2; //Maximum length of content to be previewed when looking at multiple properties

        // -- Constructors --
        protected PropertyBL(ResourceBE.Type defaultResourceType) : base(defaultResourceType) { }

        public static PropertyBL Instance {
            get { return new PropertyBL(ResourceBE.Type.PROPERTY); }
        }

        // -- Methods --
        public PropertyBE UpdateContent(PropertyBE prop, ResourceContentBE content, string changeDescription, string eTag, AbortEnum abort, XUri parentUri, ResourceBE.Type parentType) {
            if(abort == AbortEnum.Modified) {
                ValidateEtag(eTag, prop, true);
            }
            prop = BuildRevForContentUpdate(prop, content.MimeType, content.Size, changeDescription, null, content);
            prop = SaveResource(prop);
            DekiContext.Current.Instance.EventSink.PropertyUpdate(DreamContext.Current.StartTime, prop, DekiContext.Current.User, parentType, parentUri);
            return prop;
        }

        public PropertyBE Create(uint? parentId, XUri parentUri, ResourceBE.Type parentType, ResourceBE parentResource, string name, ResourceContentBE content, string description, string etag, AbortEnum abort) {

            //TODO: The parent resource isn't verified when the resource name and the parent info is given
            PropertyBE prop = GetResource(parentId, parentType, name);
            if(prop != null) {
                if(abort == AbortEnum.Exists) {
                    throw new DreamAbortException(DreamMessage.Conflict(string.Format(DekiResources.PROPERTY_ALREADY_EXISTS, name)));
                } else if(abort == AbortEnum.Modified) {
                    ValidateEtag(etag, prop, true);
                }
                prop = BuildRevForContentUpdate(prop, content.MimeType, content.Size, description, null, content);
                prop = SaveResource(prop);
                DekiContext.Current.Instance.EventSink.PropertyUpdate(DreamContext.Current.StartTime, prop, DekiContext.Current.User, parentType, parentUri);
            } else {
                if((abort == AbortEnum.Modified) && !string.IsNullOrEmpty(etag)) {
                    throw new DreamAbortException(DreamMessage.Conflict(DekiResources.PROPERTY_UNEXPECTED_ETAG));
                }
                if(parentResource == null) {
                    prop = BuildRevForNewResource(parentId, parentType, name, content.MimeType, content.Size, description, ResourceBE.Type.PROPERTY, DekiContext.Current.User.ID, content);
                } else {
                    prop = BuildRevForNewResource(parentResource, name, content.MimeType, content.Size, description, ResourceBE.Type.PROPERTY, DekiContext.Current.User.ID, content);
                }
                prop = SaveResource(prop);
                DekiContext.Current.Instance.EventSink.PropertyCreate(DreamContext.Current.StartTime, prop, DekiContext.Current.User, parentType, parentUri);
            }
            return prop;
        }

        public PropertyBE[] SaveBatch(uint? parentId, XUri parentUri, ResourceBE.Type parentType, ResourceBE parentResource, XDoc doc, out string[] failedNames, out Dictionary<string, DreamMessage> saveStatusByName) {
            //This is a specialized method that saves a batch of property updates in one request and connects them with a transaction.
            //Successful updates are returned
            //Status/description of each property update is returned as well as a hash of dreammessages by name

            saveStatusByName = new Dictionary<string, DreamMessage>();
            List<string> failedNamesList = new List<string>();
            Dictionary<string, PropertyBE> resourcesByName = new Dictionary<string, PropertyBE>();
            List<PropertyBE> ret = new List<PropertyBE>();

            //Get list of names and perform dupe check
            foreach(XDoc propDoc in doc["/properties/property"]) {
                string name = propDoc["@name"].AsText ?? string.Empty;
                if(resourcesByName.ContainsKey(name)) {
                    throw new DreamAbortException(DreamMessage.BadRequest(string.Format(DekiResources.PROPERTY_DUPE_EXCEPTION, name)));
                }

                resourcesByName[name] = null;
            }

            //Retrieve current properties with given name
            resourcesByName = GetResources(parentId, parentType, new List<string>(resourcesByName.Keys).ToArray(), DeletionFilter.ACTIVEONLY).AsHash(e => e.Name);

            //extract property info, build resource revisions, save resource, and maintain statuses for each save
            foreach(XDoc propDoc in doc["/properties/property"]) {
                PropertyBE res = null;
                string content;
                uint contentLength = 0;
                string description = string.Empty;
                string etag = null;
                MimeType mimeType;
                string name = string.Empty;

                try {
                    name = propDoc["@name"].AsText ?? string.Empty;
                    if(resourcesByName.TryGetValue(name, out res)) {

                        //All existing properties on this batch operation have the same parent
                        res.ParentResource = parentResource;
                    }

                    if(propDoc["contents"].IsEmpty) {
                        if(res == null) {
                            throw new DreamBadRequestException(string.Format(DekiResources.PROPERTY_DOESNT_EXIST_DELETE, name));
                        } else {
                            res = Delete(res, parentType, parentUri);
                        }
                    } else {

                        //parse content from xml
                        etag = propDoc["@etag"].AsText;
                        description = propDoc["description"].AsText;
                        content = propDoc["contents"].AsText;
                        contentLength = (uint) (content ?? string.Empty).Length;
                        string mimeTypeStr = propDoc["contents/@type"].AsText;
                        if(string.IsNullOrEmpty(mimeTypeStr) || !MimeType.TryParse(mimeTypeStr, out mimeType)) {
                            throw new DreamBadRequestException(string.Format(DekiResources.PROPERTY_INVALID_MIMETYPE, name, mimeTypeStr));
                        }
                        ResourceContentBE resourceContent = new ResourceContentBE(content, mimeType);

                        if(res == null) {

                            //new property
                            res = Create(parentId, parentUri, parentType, parentResource, name, resourceContent, description, etag, AbortEnum.Exists);
                        } else {

                            //new revision
                            res = UpdateContent(res, resourceContent, description, etag, AbortEnum.Modified, parentUri, parentType);
                        }
                    }

                    ret.Add(res);
                    saveStatusByName[name] = DreamMessage.Ok();
                } catch(DreamAbortException x) {

                    //Unexpected errors fall through while business logic errors while saving a property continues processing
                    saveStatusByName[name] = x.Response;
                    failedNamesList.Add(name);
                }
            }

            failedNames = failedNamesList.ToArray();
            return ret.ToArray();
        }

        public PropertyBE Delete(PropertyBE prop, ResourceBE.Type parentType, XUri parentUri) {
            DekiContext.Current.Instance.EventSink.PropertyDelete(DreamContext.Current.StartTime, prop, DekiContext.Current.User, parentType, parentUri);
            return base.Delete(prop);
        }

        public ResourceContentBE CreateDbSerializedContentFromStream(Stream stream, long length, MimeType type) {
            var memorystream = new ChunkedMemoryStream();
            stream.CopyTo(memorystream, length);
            return new ResourceContentBE(memorystream, type);
        }

        #region XML Helpers

        public XDoc GetPropertyXml(PropertyBE property, XUri parentResourceUri, string propSuffix, uint? contentCutoff) {
            return GetPropertyXml(new PropertyBE[] { property }, parentResourceUri, false, propSuffix, null, contentCutoff, null);
        }

        public XDoc GetPropertyXml(IList<PropertyBE> properties, XUri parentResourceUri, string propSuffix, uint? contentCutoff) {
            return GetPropertyXml(properties, parentResourceUri, true, propSuffix, null, contentCutoff, null);
        }

        public XDoc GetPropertyXml(IList<PropertyBE> properties, XUri parentResourceUri, string propSuffix, uint? contentCutoff, XDoc docToModify) {
            return GetPropertyXml(properties, parentResourceUri, true, propSuffix, null, contentCutoff, docToModify);
        }

        private XDoc GetPropertyXml(IList<PropertyBE> properties, XUri parentResourceUri, bool collection, string propSuffix, bool? explicitRevisionInfo, uint? contentCutoff, XDoc doc) {
            bool requiresEnd = false;
            if(collection) {
                string rootPropertiesNode = string.IsNullOrEmpty(propSuffix) ? "properties" : "properties." + propSuffix;
                if(doc == null) {
                    doc = new XDoc(rootPropertiesNode);
                } else {
                    doc.Start(rootPropertiesNode);
                    requiresEnd = true;
                }

                doc.Attr("count", properties.Count);

                if(parentResourceUri != null) {

                    //Note: this assumes that the property collection of a resource is always accessed by appending "properties" to the parent URI
                    doc.Attr("href", parentResourceUri.At("properties"));
                }

            } else {
                doc = XDoc.Empty;
            }

            //Batch retrieve users for user.modified and user.deleted
            Dictionary<uint, UserBE> usersById = new Dictionary<uint, UserBE>();
            foreach(ResourceBE r in properties) {
                usersById[r.UserId] = null;
            }
            if(!ArrayUtil.IsNullOrEmpty(properties)) {
                usersById = DbUtils.CurrentSession.Users_GetByIds(usersById.Keys.ToArray()).AsHash(e => e.ID);
            }

            foreach(PropertyBE p in properties) {
                doc = AppendPropertyXml(doc, p, parentResourceUri, propSuffix, explicitRevisionInfo, contentCutoff, usersById);
            }

            if(requiresEnd) {
                doc.End();
            }

            return doc;
        }

        private XDoc AppendPropertyXml(XDoc doc, PropertyBE property, XUri parentResourceUri, string propSuffix, bool? explicitRevisionInfo, uint? contentCutoff, Dictionary<uint, UserBE> usersById) {
            bool requiresEnd = false;
            explicitRevisionInfo = explicitRevisionInfo ?? false;
            string propElement = string.IsNullOrEmpty(propSuffix) ? "property" : "property." + propSuffix;
            if(doc == null || doc.IsEmpty) {
                doc = new XDoc(propElement);
            } else {
                doc.Start(propElement);
                requiresEnd = true;
            }

            //Build the base uri to the property

            bool includeContents = property.Size <= (contentCutoff ?? DEFAULT_CONTENT_CUTOFF) &&
                                    (property.MimeType.Match(MimeType.ANY_TEXT)
                                    );

            //TODO: contents null check
            doc.Attr("name", property.Name)
               .Attr("href", /*explicitRevisionInfo.Value ? property.UriInfo(parentResourceUri, true) : */property.UriInfo(parentResourceUri));

            if(property.IsHeadRevision()) {
                doc.Attr("etag", property.ETag());
            }

            /* PROPERTY REVISIONS: if(!property.IsHeadRevision() || explicitRevisionInfo.Value) {
                revisions not currently exposed.
                doc.Attr("revision", property.Revision);
            }
            */

            /* PROPERTY REVISIONS: doc.Start("revisions")
               .Attr("count", property.ResourceHeadRevision)
               .Attr("href", property.UriRevisions())
               .End();
            */
            string content = null;
            if(includeContents) {
                content = property.Content.ToText();
            }

            doc.Start("contents")
               .Attr("type", property.MimeType.ToString())
               .Attr("size", property.Size)
               .Attr("href", /*PROPERTY REVISIONS: explicitRevisionInfo.Value ? property.UriContent(true) : */property.UriContent(parentResourceUri))
               .Value(content)
               .End();

            doc.Elem("date.modified", property.Timestamp);
            UserBE userModified;
            usersById.TryGetValue(property.UserId, out userModified);
            if(userModified != null) {
                doc.Add(UserBL.GetUserXml(userModified, "modified", Utils.ShowPrivateUserInfo(userModified)));
            }

            doc.Elem("change-description", property.ChangeDescription);

            if(property.Deleted) {
                UserBE userDeleted;
                usersById.TryGetValue(property.UserId, out userDeleted);
                if(userDeleted != null) {
                    doc.Add(UserBL.GetUserXml(userDeleted, "deleted", Utils.ShowPrivateUserInfo(userDeleted)));
                }

                doc.Elem("date.deleted", property.Timestamp);
            }

            if(requiresEnd) {
                doc.End(); //property
            }

            return doc;
        }

        #endregion
    }
}
