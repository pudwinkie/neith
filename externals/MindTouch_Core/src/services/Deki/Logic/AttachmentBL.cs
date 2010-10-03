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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using MindTouch.Deki.Data;
using MindTouch.Dream;
using MindTouch.Xml;

namespace MindTouch.Deki.Logic {
    public class AttachmentBL : ResourceBL<AttachmentBE> {

        // -- Constants --
        public const string PROP_DESC = "urn:deki.mindtouch.com#description";
        public const ResourceBE.ChangeOperations DEFAULT_REVISION_FILTER = ResourceBE.ChangeOperations.CONTENT;
        public const int MAX_FILENAME_LENGTH = 255;
        public const int MAX_DESCRIPTION_LENGTH = 1024;

        //--- Class Fields ---
        private static log4net.ILog _log = LogUtils.CreateLog();
        private static string _renameRegexStr = @"( (\((?<num>[\d+])\)))$";
        private static readonly Regex _renameRegex = new Regex(_renameRegexStr, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static Dictionary<char, char> _charReplaceMap = new Dictionary<char, char>();

        //--- Class Constructor ---
        static AttachmentBL() {
            foreach(char c in System.IO.Path.GetInvalidFileNameChars()) {
                _charReplaceMap.Add(c, ' ');
            }
        }

        // -- Constructors --
        protected AttachmentBL(ResourceBE.Type defaultResourceType) : base(defaultResourceType) { }

        public static AttachmentBL Instance {
            get { return new AttachmentBL(ResourceBE.Type.FILE); }
        }

        //--- Class Methods ---


        //--- Instance Methods ---

        public bool IsAllowedForImageMagickPreview(AttachmentBE file) {

            // TODO (steveb): recognize mime-types as well

            if(Array.IndexOf(DekiContext.Current.Instance.ImageMagickExtensions, file.Extension.ToLowerInvariant()) == -1)
                return false;
            if(DekiContext.Current.Instance.MaxImageSize < file.Size)
                return false;
            return true;
        }

        public AttachmentBE AddAttachment(AttachmentBE existingRevision, Stream filestream, long filesize, MimeType mimeType, PageBE targetPage, string userDescription, string fileName) {
            if(DekiContext.Current.Instance.MaxFileSize < filesize) {
                throw new DreamBadRequestException(string.Format(DekiResources.MAX_FILE_SIZE_ALLOWED, DekiContext.Current.Instance.MaxFileSize));
            }

            string saveFileName = ValidateFileName(fileName);

            if(existingRevision != null) {
                if(!StringUtil.EqualsInvariant(saveFileName, existingRevision.Name)) {

                    //An existing file is getting renamed. Make sure no file exists with the new name
                    AttachmentBE existingAttachment = GetResource(targetPage, saveFileName, DeletionFilter.ACTIVEONLY);
                    if(existingAttachment != null) {
                        throw new DreamAbortException(DreamMessage.Conflict(string.Format(DekiResources.ATTACHMENT_EXISTS_ON_PAGE, saveFileName, targetPage.Title.AsUserFriendlyName())));
                    }
                }
            }

            //If file is found but has been deleted, create a new file.
            if(existingRevision != null && existingRevision.ResourceIsDeleted) {
                existingRevision = null;
            }

            AttachmentBE attachment = null;

            ResourceContentBE resourceContents = new ResourceContentBE((uint)filesize, mimeType);
            bool isUpdate = false;
            if(existingRevision == null) {
                attachment = BuildRevForNewResource(new PageWrapperBE(targetPage), saveFileName, mimeType, (uint)filesize, null, ResourceBE.Type.FILE, DekiContext.Current.User.ID, resourceContents);
            } else {
                isUpdate = true;
                attachment = BuildRevForContentUpdate(existingRevision, mimeType, (uint)filesize, null, saveFileName, resourceContents);
            }

            // rewrite mimetype to text/plain for certain extensions
            foreach(string forcedExtensions in DekiContext.Current.Instance.FileExtensionForceAsTextList) {
                if(attachment.Extension == forcedExtensions) {
                    attachment.MimeType = MimeType.TEXT;
                    break;
                }
            }

            //Insert the attachment into the DB
            attachment = SaveResource(attachment);

            try {

                //Save file to storage provider
                DekiContext.Current.Instance.Storage.PutFile(attachment, SizeType.ORIGINAL, new StreamInfo(filestream, filesize, mimeType));
            } catch(Exception x) {
                DekiContext.Current.Instance.Log.WarnExceptionFormat(x, "Failed to save attachment to storage provider");

                //Upon save failure, delete the record from the db.
                DbUtils.CurrentSession.Resources_DeleteRevision(attachment.ResourceId, attachment.Revision);
                throw;
            }

            //Set description property
            if(!string.IsNullOrEmpty(userDescription)) {
                attachment = AttachmentBL.Instance.SetDescription(attachment, userDescription);
            }

            //For images resolve width/height (if not in imagemagick's blacklist)
            attachment = IdentifyUnknownImage(attachment);

            //Pre render thumbnails of images 
            AttachmentPreviewBL.PreSaveAllPreviews(attachment);

            PageBL.Touch(targetPage, DateTime.UtcNow);

            string fileAddedDescription = string.Format(DekiResources.FILE_ADDED, attachment.Name);

            //TODO MaxM: Connect with transaction
            RecentChangeBL.AddFileRecentChange(targetPage.Touched, targetPage, DekiContext.Current.User, fileAddedDescription, 0);
            if(isUpdate) {
                DekiContext.Current.Instance.EventSink.AttachmentUpdate(DreamContext.Current.StartTime, attachment, DekiContext.Current.User);
            } else {
                DekiContext.Current.Instance.EventSink.AttachmentCreate(DreamContext.Current.StartTime, attachment, DekiContext.Current.User);
            }

            return attachment;
        }

        public IList<AttachmentBE> RetrieveAttachments(uint? offset, uint? limit) {

            IList<AttachmentBE> files = GetResources(DeletionFilter.ACTIVEONLY, limit, offset);
            List<AttachmentBE> ret = null;

            //Apply permissions
            if(!ArrayUtil.IsNullOrEmpty(files)) {
                ulong[] distinctPageIds = files.Select(e => e.ParentPageId).Distinct().ToArray();
                ulong[] allowedIds = PermissionsBL.FilterDisallowed(DekiContext.Current.User, distinctPageIds, false, Permissions.READ);
                //NOTE: this does a .contains on a list of unique pageids which *should* be pretty small
                ret = files.Where(f => allowedIds.Contains(f.ParentPageId)).ToList();
            } else {
                ret = new List<AttachmentBE>();
            }
            return ret;
        }

        public AttachmentBE SetDescription(AttachmentBE file, string description) {

            //Description of files use properties
            PropertyBE currentDesc = PropertyBL.Instance.GetResource(file.ResourceId, ResourceBE.Type.UNDEFINED, PROP_DESC);
            if(currentDesc != null) {
                currentDesc.ParentResource = file;
                PropertyBL.Instance.UpdateContent(currentDesc, new ResourceContentBE(description, MimeType.TEXT_UTF8), null, currentDesc.ETag(), AbortEnum.Modified, GetUri(file), ResourceBE.Type.FILE);
            } else {
                PropertyBL.Instance.Create(file.ResourceId, GetUri(file), ResourceBE.Type.FILE, file, PROP_DESC, new ResourceContentBE(description, MimeType.TEXT_UTF8), null, null, AbortEnum.Exists);
            }
            return file;
        }

        public void RemoveAttachments(AttachmentBE[] attachmentToRemove) {
            RemoveAttachments(attachmentToRemove, DateTime.UtcNow, 0);
        }

        public void RemoveAttachmentsFromPages(PageBE[] pages, DateTime timestamp, uint transactionId) {
            //This method used from page deletion

            IList<AttachmentBE> files = GetResources(pages, DeletionFilter.ACTIVEONLY);
            RemoveAttachments(files, timestamp, transactionId);
        }

        public void RemoveAttachments(IList<AttachmentBE> attachmentToRemove, DateTime timestamp, uint transactionId) {

            //TODO MaxM: This batch remove exists solely to mark all files as deleted when a parent page is deleted..
            List<ulong> pageIds = new List<ulong>();
            foreach(AttachmentBE file in attachmentToRemove) {
                file.AssertHeadRevision();
                pageIds.Add(file.ParentPageId);
            }
            Dictionary<ulong, PageBE> pagesById = PageBL.GetPagesByIdsPreserveOrder(pageIds).AsHash(e => e.ID);

            foreach(AttachmentBE file in attachmentToRemove) {
                PageBE parentPage = null;
                if(pagesById.TryGetValue(file.ParentPageId, out parentPage)) {
                    base.Delete(file, parentPage, transactionId);
                }
            }
        }

        public void RestoreAttachment(AttachmentBE attachmentToRestore, PageBE toPage, DateTime timestamp, uint transactionId) {

            if(toPage == null || toPage.ID == 0) {
                ArchiveBE archivesMatchingPageId = DbUtils.CurrentSession.Archive_GetPageHeadById(attachmentToRestore.ParentPageId);
                if(archivesMatchingPageId == null) {
                    throw new DreamAbortException(DreamMessage.InternalError(DekiResources.RESTORE_FILE_FAILED_NO_PARENT));
                } else {
                    toPage = PageBL.GetPageByTitle(archivesMatchingPageId.Title);
                    if(0 == toPage.ID) {
                        PageBL.Save(toPage, DekiResources.RESTOREATTACHMENTNEWPAGETEXT, DekiMimeType.DEKI_TEXT, null);
                    }
                }
            }

            string filename = attachmentToRestore.Name;

            //Check for name conflicts on target page
            AttachmentBE conflictingFile = GetResource(toPage, filename, DeletionFilter.ACTIVEONLY);
            if(conflictingFile != null) {

                //rename the restored file
                filename = string.Format("{0}(restored {1}){2}", attachmentToRestore.NameWithNoExtension, DateTime.Now.ToString("g"), string.IsNullOrEmpty(attachmentToRestore.Extension) ? string.Empty : "." + attachmentToRestore.Extension);
                conflictingFile = GetResource(toPage, filename, DeletionFilter.ACTIVEONLY);
                if(conflictingFile != null) {
                    throw new DreamAbortException(DreamMessage.Conflict(DekiResources.FILE_RESTORE_NAME_CONFLICT));
                }
            }

            //Build new revision for restored file
            attachmentToRestore = BuildRevForRestore(attachmentToRestore, toPage, filename, transactionId);

            //Insert new revision into DB
            attachmentToRestore = SaveResource(attachmentToRestore);

            //Recent Changes
            string logDescription = string.Format(DekiResources.FILE_RESTORED, attachmentToRestore.Name);
            RecentChangeBL.AddFileRecentChange(DreamContext.Current.StartTime, toPage, DekiContext.Current.User, logDescription, transactionId);

            DekiContext.Current.Instance.EventSink.AttachmentRestore(DreamContext.Current.StartTime, attachmentToRestore, DekiContext.Current.User);
        }

        /// <summary>
        /// Move this attachment to the target page. 
        /// </summary>
        /// <remarks>
        /// This will fail if destination page has a file with the same name.
        /// </remarks>
        /// <param name="sourcePage">Current file location</param>
        /// <param name="targetPage">Target file location. May be same as sourcepage for rename</param>
        /// <param name="name">New filename or null for no change</param>
        /// <returns></returns>
        public AttachmentBE MoveAttachment(AttachmentBE attachment, PageBE sourcePage, PageBE targetPage, string name, bool loggingEnabled) {

            //TODO MaxM: Connect with a changeset
            uint changeSetId = 0;
            attachment.AssertHeadRevision();

            bool move = targetPage != null && targetPage.ID != sourcePage.ID;
            bool rename = name != null && !StringUtil.EqualsInvariant(name, attachment.Name);

            //Just return the current revision if no change is being made
            if(!move && !rename) {
                return attachment;
            }

            //validate filename
            if(rename) {
                name = ValidateFileName(name);
            }

            //Check the resource exists on the target (may be same as source page) with new name (if given) or current name
            AttachmentBE existingAttachment = GetResource(targetPage ?? sourcePage, name ?? attachment.Name, DeletionFilter.ACTIVEONLY);
            if(existingAttachment != null) {
                throw new DreamAbortException(DreamMessage.Conflict(string.Format(DekiResources.ATTACHMENT_EXISTS_ON_PAGE, name ?? attachment.Name, (targetPage ?? sourcePage).Title.AsUserFriendlyName())));
            }

            //Performing a move?
            if(move) {
                DekiContext.Current.Instance.Storage.MoveFile(attachment, targetPage);  //Perform the IStorage move (should be a no-op)
            }

            //Build the new revision
            AttachmentBE newRevision = BuildRevForMoveAndRename(attachment, targetPage, name, changeSetId);

            //Insert new revision into DB
            try {
                newRevision = SaveResource(newRevision);
            } catch {

                //failed to save the revision, undo the file move with the IStorage. (Should be a no-op)
                if(move) {
                    DekiContext.Current.Instance.Storage.MoveFile(attachment, sourcePage);
                }
                throw;
                //NOTE MaxM: file rename does not even touch IStorage. No need to undo it
            }

            //Notification for file move
            if(loggingEnabled) {

                if(move) {
                    string logDescription = String.Format(DekiResources.FILE_MOVED_TO, attachment.Name, targetPage.Title.AsPrefixedUserFriendlyPath());
                    RecentChangeBL.AddFileRecentChange(DreamContext.Current.StartTime, sourcePage, DekiContext.Current.User, logDescription, changeSetId);
                    logDescription = String.Format(DekiResources.FILE_MOVED_FROM, attachment.Name, sourcePage.Title.AsPrefixedUserFriendlyPath());
                    RecentChangeBL.AddFileRecentChange(DreamContext.Current.StartTime, targetPage, DekiContext.Current.User, logDescription, changeSetId);
                }
                if(rename) {
                    string logDescription = String.Format(DekiResources.FILE_RENAMED_TO, attachment.Name, name);
                    RecentChangeBL.AddFileRecentChange(DreamContext.Current.StartTime, sourcePage, DekiContext.Current.User, logDescription, changeSetId);
                }
            }

            //Notification for file rename and move use same event
            DekiContext.Current.Instance.EventSink.AttachmentMove(DreamContext.Current.StartTime, attachment, sourcePage, DekiContext.Current.User);

            return newRevision;
        }

        public void WipeAttachments(IList<AttachmentBE> attachments) {
            if(attachments == null) {
                return;
            }

            List<uint> fileids = new List<uint>();
            foreach(AttachmentBE file in attachments) {
                try {

                    // ensure that attachment to be wiped is head revision
                    DekiContext.Current.Instance.Storage.DeleteFile(file, SizeType.ORIGINAL);
                    DekiContext.Current.Instance.Storage.DeleteFile(file, SizeType.THUMB);
                    DekiContext.Current.Instance.Storage.DeleteFile(file, SizeType.WEBVIEW);
                    fileids.Add(file.ResourceId);
                } catch(Exception e) {
                    DekiContext.Current.Instance.Log.WarnExceptionMethodCall(e, "WipeAttachments: delete file failed", file.ResourceId, file.Revision);
                }
            }

            DbUtils.CurrentSession.Resources_Delete(fileids);
        }

        public AttachmentBE[] IdentifyUnknownImages(IEnumerable<AttachmentBE> attachments) {
            List<AttachmentBE> ret = new List<AttachmentBE>();
            foreach(AttachmentBE file in attachments) {
                AttachmentBE updatedFile = file;
                if(file != null && IsAllowedForImageMagickPreview(file) && (file.ImageHeight == null || file.ImageWidth == null)) {
                    StreamInfo fileInfo = DekiContext.Current.Instance.Storage.GetFile(file, SizeType.ORIGINAL, false);
                    if(file != null) {
                        int width;
                        int height;
                        int frames;
                        if(AttachmentPreviewBL.RetrieveImageDimensions(fileInfo, out width, out height, out frames)) {
                            file.ImageWidth = width;
                            file.ImageHeight = height;

                            // check if we need to store the number of frames (default is 1)
                            if(frames > 1) {
                                file.ImageFrames = frames;
                            }
                            updatedFile = UpdateResourceRevision(file);
                        }
                    }
                }
                ret.Add(updatedFile);
            }
            return ret.ToArray();
        }

        public AttachmentBE IdentifyUnknownImage(AttachmentBE image) {
            return IdentifyUnknownImages(new AttachmentBE[] { image })[0];
        }

        public XDoc GetAttachmentRevisionListXml(IList<AttachmentBE> fileList) {
            return GetAttachmentRevisionListXml(fileList, null);
        }

        public XDoc GetAttachmentRevisionListXml(IList<AttachmentBE> fileList, XUri href) {
            XDoc attachmentListDoc = null;

            attachmentListDoc = new XDoc("files");
            attachmentListDoc.Attr("count", fileList == null ? 0 : fileList.Count);
            if(href != null)
                attachmentListDoc.Attr("href", href);

            if(fileList != null) {
                List<AttachmentBE> sortedFiles = new List<AttachmentBE>(fileList);
                sortedFiles = SortFileListByNameAndRevision(sortedFiles);

                //HACK: Convenient place to ensure all images that haven't been identified are looked at
                IdentifyUnknownImages(sortedFiles);

                //Add attachment info to list wrapper xml
                foreach(AttachmentBE att in sortedFiles) {
                    attachmentListDoc.Add(AttachmentBL.Instance.GetFileXml(att, true, null, null));
                }
            }

            return attachmentListDoc;
        }

        public AttachmentBE[] ModifyRevisionVisibility(AttachmentBE res, XDoc request, string comment) {
            List<AttachmentBE> revisionsToHide = new List<AttachmentBE>();
            List<AttachmentBE> revisionsToUnhide = new List<AttachmentBE>();
            List<AttachmentBE> ret = new List<AttachmentBE>();

            foreach(XDoc fileDoc in request["/revisions/file"]) {
                ulong? id = fileDoc["@id"].AsULong;

                //Provided id of all file revision must match the file id
                if(id != null && id.Value != res.FileId) {
                    throw new DreamBadRequestException("TODO: mismatched id");
                }

                int? revNum = fileDoc["@revision"].AsInt;
                if((revNum ?? 0) <= 0) {
                    throw new DreamBadRequestException("TODO: invalid rev");
                }

                //Hiding the head revision is not allowed. Reasons include:
                //* Behavior of search indexing undefined
                //* Behavior of accessing HEAD revision is undefined
                if(revNum == res.ResourceHeadRevision) {
                    throw new DreamBadRequestException("TODO: cannot hide head rev");
                }

                bool? hide = fileDoc["@hidden"].AsBool;
                if(hide == null) {
                    throw new DreamBadRequestException("TODO: hidden attribute invalid or not provided");
                }

                AttachmentBE rev = GetResource(res.ResourceId, revNum.Value);
                if(rev == null) {
                    throw new DreamBadRequestException("TODO: rev not found");
                }

                //Only allow hiding revisions with content changes
                if((rev.ChangeMask & ResourceBE.ChangeOperations.CONTENT) != ResourceBE.ChangeOperations.CONTENT) {
                    throw new DreamAbortException(DreamMessage.Conflict("TODO: rev cannot be hidden since it contained changes to the content"));
                }

                if(hide.Value != rev.IsHidden) {
                    if(hide.Value) {
                        revisionsToHide.Add(rev);
                    } else {
                        revisionsToUnhide.Add(rev);
                    }
                }
            }

            if(revisionsToUnhide.Count == 0 && revisionsToHide.Count == 0) {
                throw new DreamBadRequestException("TODO: no revisions to hide or unhide");
            }

            uint currentUserId = DekiContext.Current.User.ID;
            DateTime currentTs = DateTime.UtcNow;

            foreach(AttachmentBE rev in revisionsToHide) {
                rev.IsHidden = true;
                rev.MetaXml.Elem(ResourceBE.META_REVHIDE_USERID, currentUserId);
                rev.MetaXml.Elem(ResourceBE.META_REVHIDE_TS, currentTs);
                if(!string.IsNullOrEmpty(comment)) {
                    rev.MetaXml.Elem(ResourceBE.META_REVHIDE_COMMENT, comment);
                }

                ret.Add(UpdateResourceRevision(rev));
            }

            if(revisionsToUnhide.Count > 0) {
                PermissionsBL.CheckUserAllowed(DekiContext.Current.User, Permissions.ADMIN);
            }

            foreach(AttachmentBE rev in revisionsToUnhide) {
                rev.IsHidden = false;
                rev.MetaXml[ResourceBE.META_REVHIDE_USERID].Remove();
                rev.MetaXml[ResourceBE.META_REVHIDE_TS].Remove();
                rev.MetaXml[ResourceBE.META_REVHIDE_COMMENT].Remove();
                ret.Add(UpdateResourceRevision(rev));
            }

            return ret.ToArray();
        }

        protected override AttachmentBE SaveResource(AttachmentBE res) {
            AttachmentBE ret = null;
            if(res.IsNewResource()) {

                //New attachments get a legacy fileid mapping.
                uint fileId = ResourceMapBL.GetNewFileId();
                res.FileId = fileId;
                ret = base.SaveResource(res);
                ResourceMapBL.UpdateFileIdMapping(fileId, ret.ResourceId);
            } else {
                ret = base.SaveResource(res);
            }
            return ret;
        }

        protected override AttachmentBE BuildRevForContentUpdate(AttachmentBE currentResource, MimeType mimeType, uint size, string description, string name, ResourceContentBE newContent) {
            AttachmentBE newRev = base.BuildRevForContentUpdate(currentResource, mimeType, size, description, name, newContent);
            newRev.FileId = currentResource.FileId;
            return newRev;
        }

        protected override AttachmentBE BuildRevForMoveAndRename(AttachmentBE currentResource, PageBE targetPage, string name, uint changeSetId) {
            AttachmentBE newRev = base.BuildRevForMoveAndRename(currentResource, targetPage, name, changeSetId);
            newRev.FileId = currentResource.FileId;
            return newRev;
        }

        protected override AttachmentBE BuildRevForRemove(AttachmentBE currentResource, DateTime timestamp, uint changeSetId) {
            AttachmentBE newRev = base.BuildRevForRemove(currentResource, timestamp, changeSetId);
            newRev.FileId = currentResource.FileId;
            return newRev;
        }

        protected override AttachmentBE BuildRevForRestore(AttachmentBE currentResource, PageBE targetPage, string resourceName, uint changeSetId) {
            AttachmentBE newRev = base.BuildRevForRestore(currentResource, targetPage, resourceName, changeSetId);
            newRev.FileId = currentResource.FileId;
            return newRev;
        }

        public IEnumerable<AttachmentBE> GetAllAttachementsChunked() {
            const uint limit = 1000;
            uint offset = 0;
            while(true) {
                var chunk = GetResources(DeletionFilter.ACTIVEONLY, limit, offset);
                _log.DebugFormat("got chunk of {0} attachments", chunk.Count);
                foreach(var attachment in chunk) {
                    yield return attachment;
                }
                if(chunk.Count < limit) {
                    yield break;
                }
                offset += limit;
            }
        }

        #region Uris
        public XUri GetUri(AttachmentBE file) {
            XUri uri = null;
            if(file.ResourceIsDeleted) {
                uri = DekiContext.Current.ApiUri.At("archive", "files", (file.FileId ?? 0).ToString());
            } else {
                uri = DekiContext.Current.ApiUri.At("files", (file.FileId ?? 0).ToString());
            }
            return uri;
        }

        public XUri GetUriContent(AttachmentBE file) {
            return GetUriContent(file, !file.IsHeadRevision());
        }

        public XUri GetUriContent(AttachmentBE file, bool? includeRevision) {
            if(includeRevision == null) {
                includeRevision = !file.IsHeadRevision();
            }

            XUri uri = GetUri(file);

            if(includeRevision.Value) {
                uri = uri.With("revision", file.Revision.ToString());
            }

            uri = uri.At(Title.AsApiParam(file.Name));
            return uri;
        }

        public XUri GetUriInfo(AttachmentBE file) {
            return GetUriInfo(file, !file.IsHeadRevision());
        }

        public XUri GetUriInfo(AttachmentBE file, bool? includeRevision) {
            if(includeRevision == null) {
                includeRevision = !file.IsHeadRevision();
            }

            if(includeRevision.Value) {
                return GetUri(file).At("info").With("revision", file.Revision.ToString());
            } else {
                return GetUri(file).At("info");
            }
        }

        #endregion

        #region XML Helpers

        public XDoc GetFileRevisionsXml(uint resourceId, ResourceBE.ChangeOperations changeFilter, XUri listUri, int? totalcount) {
            IList<AttachmentBE> revisions = GetResourceRevisions(resourceId, changeFilter, SortDirection.ASC, null);
            return GetFileXml(revisions, true, true, null, true, totalcount, listUri);
        }

        public XDoc GetFileXml(AttachmentBE file, bool verbose, string fileSuffix, bool? explicitRevisionInfo) {
            return GetFileXml(new List<AttachmentBE>() { file }, verbose, false, fileSuffix, explicitRevisionInfo, null, null);
        }

        public XDoc GetFileXml(IList<AttachmentBE> files, bool verbose, string fileSuffix, bool? explicitRevisionInfo, XUri listUri) {
            return GetFileXml(files, verbose, true, fileSuffix, explicitRevisionInfo, null, listUri);
        }

        private XDoc GetFileXml(IList<AttachmentBE> files, bool verbose, bool list, string fileSuffix, bool? explicitRevisionInfo, int? totalCount, XUri listUri) {
            Dictionary<uint, UserBE> users = new Dictionary<uint, UserBE>();
            Dictionary<ulong, PageBE> pages = new Dictionary<ulong, PageBE>();
            List<uint> parentIds = new List<uint>();

            //Collect related entity id's 
            foreach(AttachmentBE f in files) {
                users[f.UserId] = null;
                pages[f.ParentPageId] = null;
                parentIds.Add(f.ResourceId);
            }

            //Perform batch lookups of related entities
            users = DbUtils.CurrentSession.Users_GetByIds(users.Keys.ToArray()).AsHash(e => e.ID);
            if(verbose) {
                pages = PageBL.GetPagesByIdsPreserveOrder(pages.Keys.ToArray()).AsHash(e => e.ID);
            }

            //Associate properties with the given attachments
            files = (AttachmentBE[])new ResourceBL<AttachmentBE>(ResourceBE.Type.UNDEFINED).PopulateChildren(files.ToArray(), new ResourceBE.Type[] { ResourceBE.Type.PROPERTY }, explicitRevisionInfo ?? false);

            XDoc ret = XDoc.Empty;
            if(list) {
                List<AttachmentBE> sortedFiles = new List<AttachmentBE>(files);
                files = SortFileListByNameAndRevision(sortedFiles).ToArray();
                ret = new XDoc(string.IsNullOrEmpty(fileSuffix) ? "files" : "files." + fileSuffix);
                ret.Attr("count", files.Count);
                if(totalCount != null) {
                    ret.Attr("totalcount", totalCount.Value);
                }
                if(listUri != null) {
                    ret.Attr("href", listUri);
                }
            }
            foreach(AttachmentBE f in files) {
                UserBE updatedByUser = null;
                PageBE parentPage = null;
                users.TryGetValue(f.UserId, out updatedByUser);
                pages.TryGetValue(f.ParentPageId, out parentPage);
                ret = AppendFileXml(ret, f, fileSuffix, explicitRevisionInfo, updatedByUser, parentPage);
            }

            return ret;
        }

        private XDoc AppendFileXml(XDoc doc, AttachmentBE file, string fileSuffix, bool? explicitRevisionInfo, UserBE updatedByUser, PageBE parentPage) {
            bool requiresEnd = false;
            string fileElement = string.IsNullOrEmpty(fileSuffix) ? "file" : "file." + fileSuffix;
            if(doc == null || doc.IsEmpty) {
                doc = new XDoc(fileElement);
            } else {
                doc.Start(fileElement);
                requiresEnd = true;
            }
            doc.Attr("id", file.FileId ?? 0);
            doc.Attr("revision", file.Revision);
            if(file.IsHidden) {
                doc.Attr("hidden", true);
            }

            doc.Attr("href", GetUriInfo(file, explicitRevisionInfo));

            doc.Start("filename").Value(file.Name).End();

            //Description comes from a property
            string description = string.Empty;

            if(!ArrayUtil.IsNullOrEmpty(file.ChildResources)) {
                ResourceBE descProp = Array.Find<ResourceBE>(file.ChildResources, delegate(ResourceBE p) { return p != null && p.ResourceType == ResourceBE.Type.PROPERTY && StringUtil.EqualsInvariantIgnoreCase(p.Name, PROP_DESC); }) as PropertyBE;
                if(descProp != null) {
                    description = descProp.Content.ToText();
                }
            }

            doc.Start("description").Value(description).End();

            doc.Start("contents")
                .Attr("type", file.MimeType == null ? null : file.MimeType.ToString())
                .Attr("size", file.Size);

            if((file.ImageHeight ?? 0) > 0 && (file.ImageWidth ?? 0) > 0) {
                doc.Attr("width", file.ImageWidth.Value);
                doc.Attr("height", file.ImageHeight.Value);
            }

            doc.Attr("href", GetUriContent(file, explicitRevisionInfo));
            doc.End(); //contents

            if((file.ImageWidth ?? 0) > 0 && (file.ImageHeight ?? 0) > 0) {
                string previewMime = AttachmentPreviewBL.ResolvePreviewMime(file.MimeType).ToString();
                doc.Start("contents.preview")
                    .Attr("rel", "thumb")
                    .Attr("type", previewMime)
                    .Attr("maxwidth", DekiContext.Current.Instance.ImageThumbPixels)
                    .Attr("maxheight", DekiContext.Current.Instance.ImageThumbPixels)
                    .Attr("href", GetUriContent(file, explicitRevisionInfo).With("size", "thumb"));
                if(!file.IsHeadRevision() || (explicitRevisionInfo ?? false)) {
                    doc.Attr("revision", file.Revision);
                }
                doc.End(); //contents.preview: thumb

                doc.Start("contents.preview")
                    .Attr("rel", "webview")
                    .Attr("type", previewMime)
                    .Attr("maxwidth", DekiContext.Current.Instance.ImageWebviewPixels)
                    .Attr("maxheight", DekiContext.Current.Instance.ImageWebviewPixels)
                    .Attr("href", GetUriContent(file, explicitRevisionInfo).With("size", "webview"));
                if(!file.IsHeadRevision() || (explicitRevisionInfo ?? false)) {
                    doc.Attr("revision", file.Revision);
                }
                doc.End(); //contents.preview: webview
            }

            doc.Start("date.created").Value(file.Timestamp).End();
            if(updatedByUser != null) {
                doc.Add(UserBL.GetUserXml(updatedByUser, "createdby", Utils.ShowPrivateUserInfo(updatedByUser)));
            }

            if(file.ResourceIsDeleted && ((file.ChangeMask & ResourceBE.ChangeOperations.DELETEFLAG) == ResourceBE.ChangeOperations.DELETEFLAG)) {
                if(updatedByUser != null) {
                    doc.Add(UserBL.GetUserXml(updatedByUser, "deletedby", Utils.ShowPrivateUserInfo(updatedByUser)));
                }
                doc.Start("date.deleted").Value(file.Timestamp).End();
            }

            if(file.IsHeadRevision() && !(explicitRevisionInfo ?? false) && !file.ResourceIsDeleted) {
                uint filteredCount = GetResourceRevisionCount(file.ResourceId, DEFAULT_REVISION_FILTER);
                doc.Start("revisions");
                doc.Attr("count", filteredCount);
                doc.Attr("totalcount", file.Revision);
                doc.Attr("href", GetUri(file).At("revisions"));
                doc.End();

            } else {

                if(file.ChangeMask != ResourceBE.ChangeOperations.UNDEFINED) {
                    doc.Start("user-action").Attr("type", file.ChangeMask.ToString().ToLowerInvariant()).End();
                }
            }

            //parent page is passed in for verbose output only
            if(parentPage != null) {
                doc.Add(PageBL.GetPageXml(parentPage, "parent"));
            }

            if(file.ChildResources != null) {
                List<PropertyBE> properties = new List<PropertyBE>();
                foreach(PropertyBE p in file.ChildResources) {
                    properties.Add(p);
                }

                doc = PropertyBL.Instance.GetPropertyXml(properties.ToArray(), GetUri(file), null, null, doc);
            }

            if(file.IsHidden) {
                uint? userIdHiddenBy = file.MetaXml[ResourceBE.META_REVHIDE_USERID].AsUInt;
                if(userIdHiddenBy != null) {
                    UserBE userHiddenBy = UserBL.GetUserById(userIdHiddenBy.Value);
                    if(userHiddenBy != null) {
                        doc.Add(UserBL.GetUserXml(userHiddenBy, "hiddenby", Utils.ShowPrivateUserInfo(userHiddenBy)));
                    }
                }
                doc.Elem("date.hidden", file.MetaXml[ResourceBE.META_REVHIDE_TS].AsDate ?? DateTime.MinValue);
                doc.Elem("description.hidden", file.MetaXml[ResourceBE.META_REVHIDE_COMMENT].AsText ?? string.Empty);
            }

            if(requiresEnd) {
                doc.End(); //file
            }

            return doc;
        }

        #endregion

        #region Private Helper Methods

        private bool IsBlockedFileExtension(string extension) {
            bool ret = false;
            string[] blocklist = DekiContext.Current.Instance.FileExtensionBlockList;
            if(blocklist != null && blocklist.Length > 0) {
                if(new List<string>(blocklist).Contains(extension.ToLowerInvariant().Trim()))
                    ret = true;
            }

            return ret;
        }

        private string CleanFileName(string rawfilename) {
            if(rawfilename == null)
                return null;

            StringBuilder sb = new StringBuilder();
            foreach(char c in rawfilename) {
                if(_charReplaceMap.ContainsKey(c))
                    sb.Append(_charReplaceMap[c]);
                else
                    sb.Append(c);
            }

            return sb.ToString();
        }

        private string ValidateFileName(string filename) {
            string ret = CleanFileName(filename).Trim();

            if(Path.GetFileNameWithoutExtension(ret).Length == 0 || filename.Length > MAX_FILENAME_LENGTH) {
                throw new DreamBadRequestException(DekiResources.FILENAME_IS_INVALID);
            }

            string extension = Path.GetExtension(ret).TrimStart('.');

            if(IsBlockedFileExtension(extension)) {
                DekiContext.Current.Instance.Log.WarnMethodCall("attach file not allowed: extension", extension);
                throw new DreamBadRequestException(string.Format(DekiResources.FILE_TYPE_NOT_ALLOWED, extension));
            }

            return ret;
        }

        private List<AttachmentBE> SortFileListByNameAndRevision(List<AttachmentBE> files) {

            files.Sort(delegate(AttachmentBE file1, AttachmentBE file2) {
                int compare = 0;

                //Perform a sort on name if the resource id's are different. Multiple revs of same file should not sort on name.
                if(file1.ResourceId != file2.ResourceId) {
                    compare = StringUtil.CompareInvariant(file1.Name, file2.Name);
                }
                if(compare != 0)
                    return compare;
                else {
                    // compare on revision number
                    return file1.Revision.CompareTo(file2.Revision);
                }
            });
            return files;
        }

        #endregion
    }
}
