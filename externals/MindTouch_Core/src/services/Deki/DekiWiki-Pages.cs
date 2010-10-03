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

using MindTouch.Deki.Data;
using MindTouch.Deki.Export;
using MindTouch.Deki.Logic;
using MindTouch.Deki.Script.Runtime.Library;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Xml;

namespace MindTouch.Deki {
    using Yield = IEnumerator<IYield>;

    public partial class DekiWikiService {

        //--- Features ---
        [DreamFeature("GET:pages/{pageid}", "Retrieve aggregate page information including attachments")]
        [DreamFeatureParam("{pageid}", "string", "either an integer page ID, \"home\", or \"=\" followed by a double uri-encoded page title")]
        [DreamFeatureParam("redirects", "int?", "If zero, do not follow page redirects.")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Read access to the page is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested page could not be found")]
        public Yield GetPage(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            CheckResponseCache(context, false);
            PageBE page = PageBL.GetPageFromUrl(true);
            page = PageBL.AuthorizePage(DekiContext.Current.User, Permissions.READ, page, false);
            response.Return(DreamMessage.Ok(PageBL.GetPageXmlVerbose(page, null)));
            yield break;
        }

        [DreamFeature("POST:pages/{pageid}/allowed", "Filter a list of user ids based on access to the page")]
        [DreamFeatureParam("{pageid}", "int", "integer page ID")]
        [DreamFeatureParam("permissions", "string?", "A comma separated list of permissions that must be satisfied (e.g read, etc.). Defaults to read, if not provided")]
        [DreamFeatureParam("filterdisabled", "bool?", "Consider disabled users to be disallowed, regardless of permissions (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Read access to the page is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested page could not be found")]
        public Yield GetPageAllowedUsers(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            List<uint> userids = new List<uint>();
            if(request.HasDocument) {
                foreach(XDoc userid in request.ToDocument()["user/@id"]) {
                    uint? id = userid.AsUInt;
                    if(id.HasValue) {
                        userids.Add(id.Value);
                    } else {
                        throw new DreamBadRequestException(string.Format("'{0}' is not a valid userid", userid.AsText));
                    }
                }
            }
            if(userids.Count == 0) {
                throw new DreamBadRequestException("must provide at least one userid");
            }
            string permissionsList = context.GetParam("permissions");
            bool filterDisabled = context.GetParam("filterdisabled", false);
            if(filterDisabled) {
                List<uint> activeUsers = new List<uint>();
                foreach(UserBE user in DbUtils.CurrentSession.Users_GetByIds(userids)) {
                    if(user.UserActive) {
                        activeUsers.Add(user.ID);
                    }
                }
                userids = activeUsers;
                if(userids.Count == 0) {
                    response.Return(DreamMessage.Ok(new XDoc("users")));
                    yield break;
                }
            }
            Permissions permissions = Permissions.READ;
            if(!string.IsNullOrEmpty(permissionsList)) {
                bool first = true;
                foreach(string perm in permissionsList.Split(',')) {
                    Permissions p;
                    if(!SysUtil.TryParseEnum(perm, out p)) {
                        throw new DreamBadRequestException(string.Format("'{0}' is not a valid permission value", perm));
                    }
                    if(first) {
                        permissions = p;
                    } else {
                        permissions |= p;
                    }
                    first = false;
                }
            }
            uint[] filteredIds = PermissionsBL.FilterDisallowed(userids.ToArray(), context.GetParam<uint>("pageid"), false, permissions);
            XDoc msg = new XDoc("users");
            foreach(int userid in filteredIds) {
                msg.Start("user").Attr("id", userid).End();
            }
            response.Return(DreamMessage.Ok(msg));
            yield break;
        }

        [DreamFeature("GET:pages/{pageid}/diff", "Show changes between revisions")]
        [DreamFeatureParam("{pageid}", "string", "either an integer page ID, \"home\", or \"=\" followed by a double uri-encoded page title")]
        [DreamFeatureParam("previous", "string?", "Previous page revision to retrieve. 'head' by default will retrieve latest revision. Positive integer will retrieve specific revision")]
        [DreamFeatureParam("redirects", "int?", "If zero, do not follow page redirects.")]
        [DreamFeatureParam("revision", "string?", "Page revision to retrieve. 'head' by default will retrieve latest revision. Positive integer will retrieve specific revision")]
        [DreamFeatureParam("mode", "{edit, raw, view}?", "which rendering mode to use when diffing; default is 'edit'")]
        [DreamFeatureParam("diff", "{combined, all}?", "Result format; 'combined' shows changes to the page contents, 'all' shows in addition the before and after versions of the page with highlighted changes; default is 'combined'")]
        [DreamFeatureParam("format", "{html, xhtml}?", "Result format (default: html)")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Read access to the page is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested page could not be found")]
        public Yield GetPageDiff(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            string afterRev = DreamContext.Current.GetParam("revision", "head");
            string beforeRev = DreamContext.Current.GetParam("previous");
            ParserMode mode = ParserMode.EDIT;
            try {
                mode = SysUtil.ChangeType<ParserMode>(context.GetParam("mode", "edit").ToUpperInvariant());
            } catch { }

            PageBE page = PageBL.AuthorizePage(DekiContext.Current.User, Permissions.READ, false);
            PageBL.ResolvePageRev(page, afterRev);
            ParserResult parserResult = DekiXmlParser.Parse(page, mode);

            // check if the same revision is being requested
            bool xhtml = StringUtil.EqualsInvariantIgnoreCase(context.GetParam("format", "html"), "xhtml");
            XDoc result = new XDoc("content");
            result.Attr("type", parserResult.ContentType);
            if(StringUtil.EqualsInvariant(afterRev, beforeRev)) {
                if(!xhtml) {
                    result.Value(parserResult.BodyText);
                } else {
                    result.AddNodes(parserResult.MainBody);
                }
            } else {
                PageBE previous = PageBL.AuthorizePage(DekiContext.Current.User, Permissions.READ, false);
                PageBL.ResolvePageRev(previous, beforeRev);
                ParserResult previousParserResult = DekiXmlParser.Parse(previous, mode);
                string summary;
                XDoc invisibleDiff;
                XDoc beforeChanges;
                XDoc afterChanges;
                XDoc combinedChanges = Utils.GetPageDiff(previousParserResult.MainBody, parserResult.MainBody, true, out invisibleDiff, out summary, out beforeChanges, out afterChanges);
                if(combinedChanges.IsEmpty) {

                    // if there are no visible changes and we requested the compact form, we will receive an empty document, which breaks subsequent code
                    combinedChanges = new XDoc("body");
                }
                if(!invisibleDiff.IsEmpty) {
                    combinedChanges.Start("p").Elem("strong", DekiResources.PAGE_DIFF_OTHER_CHANGES).End();
                    combinedChanges.Add(invisibleDiff);
                }
                switch(context.GetParam("diff", "combined").ToLowerInvariant()) {
                case "all":
                    if(!xhtml) {
                        result.Elem("before", beforeChanges.ToInnerXHtml());
                        result.Elem("combined", combinedChanges.ToInnerXHtml());
                        result.Elem("after", afterChanges.ToInnerXHtml());
                    } else {
                        result.Start("before").AddNodes(beforeChanges).End();
                        result.Start("combined").AddNodes(combinedChanges).End();
                        result.Start("after").AddNodes(afterChanges).End();
                    }
                    break;
                default:
                    if(!xhtml) {
                        result.Value(combinedChanges.ToInnerXHtml());
                    } else {
                        result.AddNodes(combinedChanges);
                    }
                    break;
                }
            }
            response.Return(DreamMessage.Ok(result));
            yield break;
        }

        [DreamFeature("GET:pages/{pageid}/info", "Retrieve page information")]
        [DreamFeatureParam("{pageid}", "string", "either an integer page ID, \"home\", or \"=\" followed by a double uri-encoded page title")]
        [DreamFeatureParam("redirects", "int?", "If zero, do not follow page redirects.")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Read access to the page is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested page could not be found")]
        public Yield GetPageInfo(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            PageBE page = PageBL.AuthorizePage(DekiContext.Current.User, Permissions.READ, false);
            response.Return(DreamMessage.Ok(PageBL.GetPageXml(page, string.Empty)));
            yield break;
        }

        [DreamFeature("GET:pages/{pageid}/pdf", "Export a page to PDF")]
        [DreamFeatureParam("{pageid}", "string", "either an integer page ID, \"home\", or \"=\" followed by a double uri-encoded page title")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Read access to the page is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested page could not be found")]
        public Yield GetPageExportPDF(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            PageBE page = PageBL.AuthorizePage(DekiContext.Current.User, Permissions.READ, false);

            // prepare content
            string title = page.Title.AsUserFriendlyName();
            ParserResult parser = DekiXmlParser.Parse(page, ParserMode.VIEW);
            XDoc ret = new XDoc("html")
                .Start("head")
                    .Elem("title", title)
                    .AddNodes(parser.Head)
                .End()
                .Start("body")
                    .Elem("h1", title)
                    .AddNodes(parser.MainBody)
                .End();
            // convert document
            Stream stream = Export.PDFExport.Export(ret);
            if(stream == null) {
                throw new DreamAbortException(DreamMessage.InternalError(string.Format(DekiResources.UNABLE_TO_EXPORT_PAGE_PRINCE_ERROR, page.ID)));
            }
            DreamMessage responseMessage = new DreamMessage(DreamStatus.Ok, null, MimeType.PDF, stream.Length, stream);
            responseMessage.Headers.ContentDisposition = new ContentDisposition(true, page.TimeStamp, null, null, string.Format("{0}.pdf", page.Title.AsUserFriendlyName()), null, request.Headers.UserAgent);
            response.Return(responseMessage);
            yield break;
        }

        [DreamFeature("GET:pages/{pageid}/links", "Retrieve list of inbound or outbound page links")]
        [DreamFeatureParam("{pageid}", "string", "either an integer page ID, \"home\", or \"=\" followed by a double uri-encoded page title")]
        [DreamFeatureParam("dir", "{from, to}", "links pointing to a page or from a page")]
        [DreamFeatureParam("redirects", "int?", "If zero, do not follow page redirects.")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Read access to the page is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested page could not be found")]
        public Yield GetPageLinks(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            PageBE page = PageBL.AuthorizePage(DekiContext.Current.User, Permissions.READ, false);

            // build response
            XDoc result;

            // get links
            switch(context.Uri.GetParam("dir", "").ToLowerInvariant()) {
            case "from":
            case "out":
                result = PageBL.GetLinksXml(DbUtils.CurrentSession.Links_GetOutboundLinks(page.ID), "outbound");
                break;
            case "to":
            case "in":
                result = PageBL.GetLinksXml(DbUtils.CurrentSession.Links_GetInboundLinks(page.ID), "inbound");
                break;
            default:
                response.Return(DreamMessage.BadRequest(string.Format(DekiResources.DIR_IS_NOT_VALID, context.Uri.GetParam("dir", ""))));
                yield break;
            }

            response.Return(DreamMessage.Ok(result));
            yield break;
        }

        [DreamFeature("GET:pages/{pageid}/subpages", "Retrieve list of sub-pages")]
        [DreamFeatureParam("{pageid}", "string", "either an integer page ID, \"home\", or \"=\" followed by a double uri-encoded page title")]
        [DreamFeatureParam("limit", "string?", "Maximum number of items to retrieve. Must be a positive number or 'all' to retrieve all items. (default: 100)")]
        [DreamFeatureParam("offset", "int?", "Number of items to skip. Must be a positive number or 0 to not skip any. (default: 0)")]
        [DreamFeatureParam("redirects", "int?", "If zero, do not follow page redirects.")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Read access to the page is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested page could not be found")]
        public Yield GetPageSubpages(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            PageBE page = PageBL.AuthorizePage(DekiContext.Current.User, Permissions.READ, false);
            uint offset;
            uint limit;
            Utils.GetOffsetAndCountFromRequest(context, uint.MaxValue, out limit, out offset);

            // build response
            XDoc result = PageBL.GetSubpageXml(page, limit, offset);
            response.Return(DreamMessage.Ok(result));
            yield break;
        }

        [DreamFeature("GET:pages/{pageid}/aliases", "Retrieve list of page aliases")]
        [DreamFeatureParam("{pageid}", "string", "either an integer page ID, \"home\", or \"=\" followed by a double uri-encoded page title")]
        [DreamFeatureParam("redirects", "int?", "If zero, do not follow page redirects.")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Read access to the page is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested page could not be found")]
        public Yield GetPageAliases(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            PageBE page = PageBL.AuthorizePage(DekiContext.Current.User, Permissions.READ, false);
            XUri href = DekiContext.Current.ApiUri.At("pages", page.ID.ToString(), "aliases");
            XDoc doc = PageBL.GetPageListXml(PageBL.GetRedirectsApplyPermissions(page), "aliases", href);
            response.Return(DreamMessage.Ok(doc));
            yield break;
        }

        [DreamFeature("GET:pages/{pageid}/revisions", "Retrieve revision history of a given title")]
        [DreamFeatureParam("{pageid}", "string", "either an integer page ID, \"home\", or \"=\" followed by a double uri-encoded page title")]
        [DreamFeatureParam("limit", "string?", "Maximum number of items to retrieve. Must be a positive number or 'all' to retrieve all items. (default: 50)")]
        [DreamFeatureParam("offset", "int?", "Number of items to skip. Must be a positive number or 0 to not skip any. (default: 0)")]
        [DreamFeatureParam("redirects", "int?", "If zero, do not follow page redirects.")]
        [DreamFeatureParam("revision", "int?", "Page revision to retrieve")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Read access to the page is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested page could not be found")]
        public Yield GetPageRevisions(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            PageBE page = PageBL.AuthorizePage(DekiContext.Current.User, Permissions.READ, false);
            XDoc result = null;

            // extract parameters
            int rev = context.GetParam<int>("revision", int.MinValue);
            if(rev != int.MinValue) {
                OldBE oldRev = PageBL.GetOldRevisionForPage(page, rev);

                if(oldRev != null) {
                    result = PageBL.GetOldXml(page, oldRev, null);
                } else {
                    throw new DreamAbortException(DreamMessage.NotFound(string.Format(DekiResources.COULD_NOT_FIND_REVISION, rev, page.ID)));
                }
            } else {
                uint max, offset;
                Utils.GetOffsetAndCountFromRequest(context, 50, out max, out offset);
                result = PageBL.GetOldListXml(page, DbUtils.CurrentSession.Old_GetOldsByQuery(page.ID, true, offset, max), "pages");
            }

            response.Return(DreamMessage.Ok(result));
            yield break;
        }

        [DreamFeature("GET:pages/{pageid}/files", "Retrieves a list of files for a given page")]
        [DreamFeatureParam("{pageid}", "string", "either an integer page ID, \"home\", or \"=\" followed by a double uri-encoded page title")]
        [DreamFeatureParam("redirects", "int?", "If zero, do not follow page redirects.")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Read access to the page is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested page could not be found")]
        public Yield GetPageFiles(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            PageBE page = PageBL.AuthorizePage(DekiContext.Current.User, Permissions.READ, false);
            IList<AttachmentBE> files = AttachmentBL.Instance.GetResources(page, DeletionFilter.ACTIVEONLY);
            XUri href = DekiContext.Current.ApiUri.At("pages", page.ID.ToString(), "files");
            response.Return(DreamMessage.Ok(AttachmentBL.Instance.GetAttachmentRevisionListXml(files, href)));
            yield break;
        }

        [DreamFeature("GET:pages/{pageid}/files,subpages", "Retrieves a list of files and subpages for a given page")]
        [DreamFeatureParam("{pageid}", "string", "either an integer page ID, \"home\", or \"=\" followed by a double uri-encoded page title")]
        [DreamFeatureParam("redirects", "int?", "If zero, do not follow page redirects.")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Read access to the page is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested page could not be found")]
        public Yield GetPageSubpagesAndFiles(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            PageBE page = PageBL.AuthorizePage(DekiContext.Current.User, Permissions.READ, false);
            IList<AttachmentBE> files = AttachmentBL.Instance.GetResources(page, DeletionFilter.ACTIVEONLY);
            XDoc ret = PageBL.GetPageXml(page, null);
            ret.Add(PageBL.GetSubpageXml(page, uint.MaxValue, 0));
            ret.Add(AttachmentBL.Instance.GetAttachmentRevisionListXml(files));
            response.Return(DreamMessage.Ok(ret));
            yield break;
        }

        [DreamFeature("POST:pages/{pageid}/revert", "Revert page to an earlier revision")]
        [DreamFeatureParam("{pageid}", "string", "either an integer page ID, \"home\", or \"=\" followed by a double uri-encoded page title")]
        [DreamFeatureParam("redirects", "int?", "If zero, do not follow page redirects.")]
        [DreamFeatureParam("fromrevision", "int", "Revision number of page that will become the new head revision")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Update access to the page is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested page could not be found")]
        public Yield PostPageRevert(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            PageBE page = PageBL.AuthorizePage(DekiContext.Current.User, Permissions.UPDATE, false);

            // Note (arnec): has to be an int instead of ulong, since negative numbers have special meaning a number of layers down
            int rev = context.GetParam<int>("fromrevision");
            PageBL.RevertPageFromRevision(page, rev);
            DekiContext.Current.Instance.EventSink.PageRevert(context.StartTime, page, DekiContext.Current.User, rev);
            response.Return(DreamMessage.Ok());
            yield break;
        }

        [DreamFeature("POST:pages/{pageid}/move", "Move page to a new location")]
        [DreamFeatureParam("{pageid}", "string", "either an integer page ID, \"home\", or \"=\" followed by a double uri-encoded page title")]
        [DreamFeatureParam("to", "string?", "new page location including the path and name of the page")]
        [DreamFeatureParam("name", "string?", "Move the page to the given name while keeping it under the same parent page")]
        [DreamFeatureParam("parentid", "int?", "Relocate the page under a given parent page")]
        [DreamFeatureParam("title", "string?", "Set the title of the page. The name of a page is also modified unless it's provided")]
        [DreamFeatureParam("redirects", "int?", "If zero, do not follow page redirects.")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Update access to the page is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested page could not be found")]
        [DreamFeatureStatus(DreamStatus.Conflict, "Page move would conflict with an existing page")]
        public Yield PostPageMove(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            PageBE page = PageBL.AuthorizePage(DekiContext.Current.User, Permissions.UPDATE, false);

            string to = context.GetParam("to", null);
            string name = context.GetParam("name", null);
            string title = context.GetParam("title", null);
            ulong parentId = context.GetParam<ulong>("parentid", 0);
            IList<PageBE> movedPages = null;

            // Validate title
            if(title != null) {
                try {
                    title = title.Trim();
                    Title.ValidateDisplayName(title);
                } catch(ArgumentException x) {
                    throw new DreamBadRequestException("Title parameter is invalid");
                }
            }

            // Validate name by renaming
            if(name != null){
                try {
                    Title newTitle = page.Title.Rename(name, title);
                } catch(ArgumentException x) {
                    throw new DreamBadRequestException(string.Format("Given name is invalid: {0}", x.Message));
                }
            }

            // Validate parentid
            PageBE parentPage = null;
            if(parentId > 0) {
                parentPage = PageBL.GetPageById(parentId);
                if(parentPage == null) {
                    throw new DreamBadRequestException("Page given by parentid does not exist");
                }
            }

            if(!string.IsNullOrEmpty(to)) {

                // If 'to' is defined ensure that name and parentid are not.
                if(!string.IsNullOrEmpty(name) || parentId != 0) {
                    throw new DreamBadRequestException("To parameter cannot be combined with name, title, or parentid");
                } else {
                    movedPages = PageBL.MovePage(page, to, title);
                }
            } else {
                movedPages = PostPageMove_Helper(page, parentPage, name, title);
            }

            XDoc ret = PageBL.GetPageListXml(movedPages, "pages.moved");
            response.Return(DreamMessage.Ok(ret));
            yield break;
        }

        private IList<PageBE> PostPageMove_Helper(PageBE page, PageBE parentPage, string name, string title) {
            try {
                return PageBL.MovePage(page, parentPage, name, title);
            } catch(Exceptions.InvalidTitleException x) {
                throw new DreamBadRequestException(string.Format("Name parameter is invalid: {0}", x.Message));
            }
        }

        [DreamFeature("GET:pages/{pageid}/contents", "Retrieve the contents of a page.")]
        [DreamFeatureParam("{pageid}", "string", "either an integer page ID, \"home\", or \"=\" followed by a double uri-encoded page title")]
        [DreamFeatureParam("redirects", "int?", "If zero, do not follow page redirects.")]
        [DreamFeatureParam("mode", "{edit, raw, view}", "render content for different uses; default is 'view'")]
        [DreamFeatureParam("revision", "string?", "Page revision to retrieve. 'head' by default will retrieve latest revision. positive integer will retrieve specific revision")]
        [DreamFeatureParam("highlight", "string?", "Comma separated list of terms to highlight (default: empty)")]
        [DreamFeatureParam("format", "{html, xhtml}?", "Result format (default: html)")]
        [DreamFeatureParam("section", "int?", "The section number (default: none)")]
        [DreamFeatureParam("include", "bool?", "Treat page as an include (default: false)")]
        [DreamFeatureParam("pageid", "int?", "For template pages, use specified page ID as context for template invocation (default: none)")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureParam("relto", "int?", "Page used for path normalization (default: none)")]
        [DreamFeatureParam("reltopath", "string?", "Page used for path normalization. Ignored if relto parameter is defined. (default: none)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Update access to the page is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested page could not be found")]
        [DreamFeatureStatus(DreamStatus.NonAuthoritativeInformation, "Page contents could not be parsed in its native format and was returned in an alternative format instead")]
        public Yield GetPageContents(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            CheckResponseCache(context, false);

            PageBE page = PageBL.GetPageFromUrl(true);
            page = PageBL.AuthorizePage(DekiContext.Current.User, Permissions.READ, page, false);

            // Retrieve the title used for path normalization (if any)
            Title relToTitle = Utils.GetRelToTitleFromUrl(context);

            PageBL.ResolvePageRev(page, DreamContext.Current.GetParam("revision", "HEAD"));

            if(page.IsHidden) {
                PermissionsBL.CheckUserAllowed(DekiContext.Current.User, Permissions.ADMIN);
            }

            int section = context.GetParam<int>("section", -1);
            if((0 == section) || (section < -1)) {
                throw new DreamAbortException(DreamMessage.BadRequest(DekiResources.SECTION_PARAM_INVALID));
            }

            // check if page should be processed as an include (used by the 'Template Dialog')
            bool isInclude = !StringUtil.EqualsInvariantIgnoreCase(context.GetParam("include", "false"), "false");
            string language = context.GetParam("lang", null);

            // extract the parser mode
            ParserMode mode = ParserMode.VIEW;
            switch(context.GetParam("mode", "view").ToLowerInvariant()) {
            case "edit":
                mode = ParserMode.EDIT;
                break;
            case "raw":
                mode = ParserMode.RAW;
                break;
            case "view":
                mode = ParserMode.VIEW;
                if(!isInclude && DekiContext.Current.Instance.StatsPageHitCounter) {
                    PageBL.IncrementViewCount(page);
                }
                break;
            case "viewnoexecute":
                mode = ParserMode.VIEW_NO_EXECUTE;
                if(!isInclude && DekiContext.Current.Instance.StatsPageHitCounter) {
                    PageBL.IncrementViewCount(page);
                }
                break;
            }

            // retrieve the page xdoc
            ParserResult parserResult;
            uint contextPageId = context.Uri.GetParam<uint>("pageid", uint.MaxValue);
            if((mode == ParserMode.VIEW) && (contextPageId != uint.MaxValue) && page.Title.IsTemplate) {

                // NOTE (steveb): page being rendered is a template and a contextual page was specified; this means we're rendering a global template page
                PageBE contextPage = PageBL.GetPageById(contextPageId);
                if(contextPage == null) {
                    response.Return(DreamMessage.BadRequest(string.Format("no page exists for pageid={0}", contextPageId)));
                    yield break;
                }
                parserResult = DekiXmlParser.ParseGlobalTemplate(contextPage, page);
            } else {
                parserResult = DekiXmlParser.Parse(page, page.ContentType, language ?? page.Language, page.GetText(DbUtils.CurrentSession), mode, isInclude, section, null, relToTitle);
            }
            if(page.Title.IsTemplate && isInclude) {
                DekiXmlParser.PostProcessTemplateInsertBody(parserResult, page);
            }

            // post process tail element
            DekiXmlParser.PostProcessParserResults(parserResult);

            // BUGBUGBUG (steveb): we cannot properly restore an old title unless it had a display title set

            // wrap the result in a content tag and return it to the user
            XDoc result = new XDoc("content")
                .Attr("type", parserResult.ContentType)
                .Attr("etag",page.Etag)
                .Attr("title", page.CustomTitle ?? page.Title.AsUserFriendlyName());

            // check if page contains unsafe content
            if(mode == ParserMode.EDIT) {
                result.Attr("unsafe", !DekiScriptLibrary.VerifyXHtml(parserResult.MainBody, true));
            }

            // check if the content should be returned as structured XML or inlined as text in an XML envelope
            if(StringUtil.EqualsInvariant(context.GetParam("format", "html"), "xhtml")) {
                result.AddNodes(parserResult.Content);
            } else {

                // encode the result as nodes of text
                foreach(XDoc entry in parserResult.Content.Elements) {
                    if(entry.HasName("body")) {
                        result.Start("body").Attr("target", entry["@target"].AsText).Value(entry.ToInnerXHtml()).End();
                    } else {
                        result.Elem(entry.Name, entry.ToInnerXHtml());
                    }
                }
            }

            // check if we hit a snag, which is indicated by a plain-text response
            DreamMessage msg;
            if((parserResult.ContentType == MimeType.TEXT.FullType) && (page.ContentType != MimeType.TEXT.FullType)) {

                // something happened during parsing
                msg = new DreamMessage(DreamStatus.NonAuthoritativeInformation, null, result);
            } else {
                msg = DreamMessage.Ok(result);
            }

            response.Return(msg);
            yield break;
        }

        [DreamFeature("POST:pages/{pageid}/contents", "Update contents of a page")]
        [DreamFeatureParam("{pageid}", "string", "either an integer page ID, \"home\", or \"=\" followed by a double uri-encoded page title")]
        [DreamFeatureParam("edittime", "string", "the edit timestamp (yyyyMMddHHmmss or yyyy-MM-ddTHH:mm:ssZ)")]
        [DreamFeatureParam("comment", "string?", "the edit comment")]
        [DreamFeatureParam("language", "string?", "the page language (default: determine culture from parent)")]
        [DreamFeatureParam("title", "string?", "the display title (default: use existing title or determine from page path.)")]
        [DreamFeatureParam("section", "int?", "the section number.  If zero, append as a new section")]
        [DreamFeatureParam("xpath", "string?", "identifies the portion of the page to update; this parameter is ignored if section is specified")]
        [DreamFeatureParam("abort", "{never, modified, exists}?", "specifies condition under which to prevent the save; default is never")]
        [DreamFeatureParam("redirects", "int?", "If zero, do not follow page redirects.")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureParam("tidy", "{remove, convert}?", "Determines if invalid content is converted to text or removed (default: 'convert')")]
        [DreamFeatureParam("relto", "int?", "Page used for path normalization (default: none)")]
        [DreamFeatureParam("reltopath", "string?", "Page used for path normalization. Ignored if relto parameter is defined. (default: none)")]
        [DreamFeatureParam("overwrite", "bool?", "New page revision is created when no changes are detected when overwrite is true (default: false)")]
        [DreamFeatureParam("importtime", "string?", "If this is an import, the edit timestamp of the imported content (yyyyMMddHHmmss or yyyy-MM-ddTHH:mm:ssZ)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Update access to the page is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested page could not be found")]
        public Yield PostPageContents(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            PageBE cur = PageBL.GetPageFromUrl(false);

            // load page contents based on mime type
            string contents;
            MimeType mimeType = request.ContentType;
            if(mimeType.IsXml) {
                XDoc contentsDoc = request.ToDocument();
                if(contentsDoc == null || contentsDoc.IsEmpty || !contentsDoc.HasName("content")) {
                    throw new DreamBadRequestException(string.Format(DekiResources.INVALID_POSTED_DOCUMENT_1, "content"));
                }
                contents = contentsDoc["body"].ToInnerXHtml();
            } else if(MimeType.TEXT.Match(mimeType) || MimeType.FORM_URLENCODED.Match(mimeType)) {
                contents = request.AsText();
            } else {
                throw new DreamBadRequestException(string.Format(DekiResources.CONTENT_TYPE_NOT_SUPPORTED, mimeType.ToString(), MimeType.TEXT.ToString(), MimeType.FORM_URLENCODED.ToString()));
            }

            bool isExistingPage = cur.ID != 0 && !cur.IsRedirect;
            string abort = context.GetParam("abort", "never").ToLowerInvariant();
            if(isExistingPage && "exists" == abort) {
                throw new DreamAbortException(DreamMessage.Conflict(DekiResources.PAGE_ALREADY_EXISTS));
            }

            // Retrieve the title used for path normalization (if any)
            Title relToTitle = Utils.GetRelToTitleFromUrl(context);

            DateTime editTime = DbUtils.ToDateTime(context.GetParam("edittime", null));
            string comment = context.GetParam("comment", String.Empty);
            string language = context.GetParam("language", null);
            string displayName = context.GetParam("title", null);
            int section = context.GetParam<int>("section", -1);
            if((section < -1) || ((!isExistingPage) && (0 < section))) {
                throw new DreamAbortException(DreamMessage.BadRequest(DekiResources.SECTION_PARAM_INVALID));
            }

            // determin how unsafe/invalid content should be handled
            bool removeIllegalElements = StringUtil.EqualsInvariantIgnoreCase(context.GetParam("tidy", "convert"), "remove");

            // a new revision is created when no changes are detected when overwrite is enabled
            bool overwrite = context.GetParam<bool>("overwrite", false);

            // check whether the page exists and is not a redirect
            DateTime pageLastEditTime = cur.TimeStamp;
            OldBE baseOld = null;
            OldBE overwrittenOld = null;
            if(isExistingPage) {
                PageBL.AuthorizePage(DekiContext.Current.User, Permissions.UPDATE, cur, false);

                // ensure that 'edittime' is set
                if(DateTime.MinValue == editTime) {
                    throw new DreamAbortException(DreamMessage.BadRequest(DekiResources.EDITTIME_PARAM_INVALID));
                }

                // check if page was modified since since the specified time
                if(pageLastEditTime > editTime) {

                    // ensure we're allowed to save a modified page
                    if("modified" == abort) {
                        throw new DreamAbortException(DreamMessage.Conflict(DekiResources.PAGE_WAS_MODIFIED));
                    }

                    // if an edit has occurred since the specified edit time, retrieve the revision upon which it is based
                    // NOTE: this can be null if someone created the page after the specified edit time (ie. no common base revision)
                    baseOld = DbUtils.CurrentSession.Old_GetOldByTimestamp(cur.ID, editTime);

                    // if editing a particular section, use the page upon which the section edits were based.
                    if(0 < section && null == baseOld) {
                        throw new DreamAbortException(DreamMessage.BadRequest(DekiResources.HEADING_PARAM_INVALID));
                    }
                }
            }

            // save page
            bool conflict;
            overwrittenOld = PageBL.Save(cur, baseOld, comment, contents, DekiMimeType.DEKI_TEXT, displayName, language, section, context.GetParam("xpath", null), DateTime.UtcNow, 0, true, removeIllegalElements, relToTitle, overwrite, out conflict);

            // check if this post is part of an import action
            var importTimeStr = context.GetParam("importtime", null);
            if(!string.IsNullOrEmpty(importTimeStr)) {
                var dateModified = DbUtils.ToDateTime(importTimeStr);
                var lastImport = PropertyBL.Instance.GetResource((uint)cur.ID, ResourceBE.Type.PAGE, SiteImportBuilder.LAST_IMPORT);
                var lastImportDoc = new XDoc("last-import").Elem("etag", cur.Etag).Elem("date.modified", dateModified);
                var content = new ResourceContentBE(lastImportDoc);
                if(lastImport == null) {
                    PropertyBL.Instance.Create((uint)cur.ID, PageBL.GetUri(cur), ResourceBE.Type.PAGE, new PageWrapperBE(cur), SiteImportBuilder.LAST_IMPORT, content, string.Format("import at revision {0}", cur.Revision), content.ComputeHashString(), AbortEnum.Never);
                } else {
                    PropertyBL.Instance.UpdateContent(lastImport, content, string.Format("updated import at revision {0}", cur.Revision), content.ComputeHashString(), AbortEnum.Never, PageBL.GetUri(cur), ResourceBE.Type.PAGE);
                }
            }

            // generate xml output
            XDoc editXml = new XDoc("edit");
            editXml.Add(PageBL.GetPageXml(cur, String.Empty));

            // if a non-redirect was overwritten, report it
            if((overwrittenOld != null) && (pageLastEditTime != editTime) && isExistingPage && conflict) {
                editXml.Attr("status", "conflict");
                editXml.Add(baseOld == null ? new XDoc("page.base") : PageBL.GetOldXml(cur, baseOld, "base"));
                editXml.Add(PageBL.GetOldXml(cur, overwrittenOld, "overwritten"));
            } else {
                editXml.Attr("status", "success");
            }

            response.Return(DreamMessage.Ok(editXml));
            yield break;
        }

        [DreamFeature("POST:pages/{pageid}/revisions", "Performs operations such as hide/unhide for revisions of pages")]
        [DreamFeatureParam("{pageid}", "string", "either an integer page ID, \"home\", or \"=\" followed by a double uri-encoded page title")]
        [DreamFeatureParam("comment", "string?", "Reason for hiding revisions")]
        [DreamFeatureParam("redirects", "int?", "If zero, do not follow page redirects.")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "DELETE access is required to hide a revision and ADMIN access to unhide")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested page could not be found")]
        public Yield PostPageRevisions(DreamContext context, DreamMessage request, Result<DreamMessage> response) {

            // Note (arnec):Page revision hiding requires DELETE permission which by default is never authorized on the home page,
            // so we need to pass in a flag to allow the Homepage authorization exception to be ignored.
            PageBE page = PageBL.AuthorizePage(DekiContext.Current.User, Permissions.DELETE, true);

            OldBE[] olds = PageBL.ModifyRevisionVisibility(page, request.ToDocument(), context.GetParam("comment", string.Empty));
            XDoc ret = PageBL.GetOldListXml(page, olds, "pages");
            response.Return(DreamMessage.Ok(ret));
            yield break;
        }

        [DreamFeature("POST:pages/{pageid}/index", "re-index a page and it's attributes")]
        [DreamFeatureParam("{pageid}", "string", "either an integer page ID, \"home\", or \"=\" followed by a double uri-encoded page title")]
        [DreamFeatureParam("redirects", "int?", "If zero, do not follow page redirects.")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "MindTouch API key or Administrator access is required.")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested page could not be found")]
        internal Yield IndexPage(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            PageBE page = PageBL.AuthorizePage(DekiContext.Current.User, Permissions.NONE, false);
            DekiContext.Current.Instance.EventSink.PagePoke(context.StartTime, page, DekiContext.Current.User);
            uint commentCount;
            foreach(CommentBE comment in DbUtils.CurrentSession.Comments_GetByPage(page, CommentFilter.NONDELETED, false, null, SortDirection.UNDEFINED, 0, uint.MaxValue, out commentCount)) {
                DekiContext.Current.Instance.EventSink.CommentPoke(context.StartTime, comment, page, DekiContext.Current.User);
            }
            response.Return(DreamMessage.Ok());
            yield break;
        }

        [DreamFeature("DELETE:pages/{pageid}", "Deletes a page and optionally descendant pages by moving them to the archive")]
        [DreamFeatureParam("{pageid}", "string", "either an integer page ID, \"home\", or \"=\" followed by a double uri-encoded page title")]
        [DreamFeatureParam("recursive", "bool?", "only delete page or delete page and descendants. Default: false")]
        [DreamFeatureParam("redirects", "int?", "If zero, do not follow page redirects.")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "Request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Update/delete access to the page is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested page could not be found")]
        public Yield DeletePage(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            PageBE pageToDelete = PageBL.AuthorizePage(DekiContext.Current.User, Permissions.UPDATE | Permissions.DELETE, false);
            bool recurse = context.GetParam<bool>("recursive", false);
            PageBE[] deletedPages = PageBL.DeletePage(pageToDelete, recurse);

            XDoc responseDoc = PageBL.GetPageListXml(deletedPages, "deletedpages");
            response.Return(DreamMessage.Ok(responseDoc));
            yield break;
        }

        [DreamFeature("GET:pages", "Builds a site map starting from 'home' page.")]
        [DreamFeature("GET:pages/{pageid}/tree", "Builds a site map starting from a given page.")]
        [DreamFeatureParam("{pageid}", "string", "either an integer page ID, \"home\", or \"=\" followed by a double uri-encoded page title")]
        [DreamFeatureParam("format", "{xml, html, google}?", "Result format (default: xml)")]
        [DreamFeatureParam("startpage", "bool?", "For HTML sitemap, indicates if the start page should be included (default: true)")]
        [DreamFeatureParam("language", "string?", "Filter results by language (default: all languages)")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Browse access to the page is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested page could not be found")]
        public Yield GetPages(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            CheckResponseCache(context, true);

            PageBE page = null;
            string pageParam = context.GetParam("pageid", string.Empty);
            if(pageParam == string.Empty) {
                page = PageBL.GetHomePage();
            } else {
                page = PageBL.GetPageFromUrl();
            }
            PageBL.AuthorizePage(DekiContext.Current.User, Permissions.BROWSE, page, false);
            string format = context.GetParam("format", "xml");
            bool includeStartPage = context.GetParam<bool>("startpage", true);

            // extract the filter language
            string language = context.GetParam("language", null);
            if(null != language) {
                PageBL.ValidatePageLanguage(language);
            }

            XDoc retXml = null;
            switch(format.ToLowerInvariant()) {
            case "sitemap":
            case "sitemap.xml":
            case "google":
                retXml = PageSiteMapBL.BuildGoogleSiteMap(page, language);
                response.Return(DreamMessage.Ok(retXml));
                break;
            case "html":
                retXml = PageSiteMapBL.BuildHtmlSiteMap(page, language, int.MaxValue, false);
                XDoc html = includeStartPage ? retXml : retXml["li/ul"];
                response.Return(DreamMessage.Ok(MimeType.HTML, html.ToXHtml()));
                break;
            case "xml":
                retXml = PageSiteMapBL.BuildXmlSiteMap(page, language);
                response.Return(DreamMessage.Ok(retXml));
                break;
            default:
                throw new DreamBadRequestException(DekiResources.INVALID_FORMAT_GIVEN);
            }
            yield break;
        }

        [DreamFeature("GET:pages/popular", "Retrieves a list of popular pages.")]
        [DreamFeatureParam("limit", "string?", "Maximum number of items to retrieve. Must be a positive number or 'all' to retrieve all items. (default: 50)")]
        [DreamFeatureParam("offset", "int?", "Number of items to skip. Must be a positive number or 0 to not skip any. (default: 0)")]
        [DreamFeatureParam("language", "string?", "Filter results by language (default: all languages)")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        public Yield GetPopularPages(DreamContext context, DreamMessage request, Result<DreamMessage> response) {

            //Language filtering done in GetPopularPagesXml
            XDoc result = PageBL.GetPopularPagesXml(context);
            response.Return(DreamMessage.Ok(result));
            yield break;
        }

        #region security

        [DreamFeature("GET:pages/{pageid}/security", "Retrieve page security info")]
        [DreamFeatureParam("{pageid}", "string", "either an integer page ID, \"home\", or \"=\" followed by a double uri-encoded page title")]
        [DreamFeatureParam("redirects", "int?", "If zero, do not follow page redirects.")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Browse access to the page is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested page could not be found")]
        public Yield GetPageSecurity(DreamContext context, DreamMessage request, Result<DreamMessage> response) {

            //NOTE MaxM: This features requires either BROWSE or READ access but not both. Permissions don't currently support this sort of query so two checks are performed instead.
            // First user is checked for having READ access and if not then BROWSE access. The second call will throw an exception. The exception message is misleading since it only describes
            // that BROWSE is required.
            PageBE page = PageBL.GetPageFromUrl();
            UserBE user = DekiContext.Current.User;
            if(!PermissionsBL.IsUserAllowed(user, page, Permissions.READ)) {
                PermissionsBL.CheckUserAllowed(user, page, Permissions.BROWSE);
            }

            XDoc result = PageBL.GetSecurityXml(page);
            response.Return(DreamMessage.Ok(result));
            yield break;
        }

        [DreamFeature("DELETE:pages/{pageid}/security", "Reset page restricts and grants")]
        [DreamFeatureParam("{pageid}", "string", "either an integer page ID, \"home\", or \"=\" followed by a double uri-encoded page title")]
        [DreamFeatureParam("redirects", "int?", "If zero, do not follow page redirects.")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Change permissions access to the page is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "The requested page could not be found")]
        public Yield DeletePageSecurity(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            PageBE page = PageBL.AuthorizePage(DekiContext.Current.User, Permissions.CHANGEPERMISSIONS, false);
            PermissionsBL.DeleteAllGrantsForPage(page);

            //Clear page restriction.
            page.RestrictionID = 0;
            DbUtils.CurrentSession.Pages_Update(page);

            DekiContext.Current.Instance.EventSink.PageSecurityDelete(context.StartTime, page);
            response.Return(DreamMessage.Ok());
            yield break;
        }

        [DreamFeature("PUT:pages/{pageid}/security", "Set page security info")]
        [DreamFeatureParam("{pageid}", "string", "either an integer page ID, \"home\", or \"=\" followed by a double uri-encoded page title")]
        [DreamFeatureParam("redirects", "int?", "If zero, do not follow page redirects.")]
        [DreamFeatureParam("cascade", "{none,delta,absolute}?", "none: Permissions are not cascaded to child pages; deltas: Changes between given page's security and proposed security cascaded to child nodes; absolute: Proposed security is set on child pages. Default: none")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Change permissions access to the page is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested page could not be found")]
        public Yield PutPageSecurity(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            PageBE page = PageBL.AuthorizePage(DekiContext.Current.User, Permissions.CHANGEPERMISSIONS, false);

            CascadeType cascade = CascadeType.NONE;

            switch(context.GetParam("cascade", "none").ToLowerInvariant().Trim()) {
            case "delta":
                cascade = CascadeType.DELTA;
                break;
            case "absolute":
                cascade = CascadeType.ABSOLUTE;
                break;
            case "none":
                cascade = CascadeType.NONE;
                break;
            default:
                throw new DreamBadRequestException(DekiResources.CASCADE_PARAM_INVALID);
            }

            //Parse out the page restriction attribute
            string pageRestrictionStr = request.ToDocument()["permissions.page/restriction"].Contents;
            if(string.IsNullOrEmpty(pageRestrictionStr))
                throw new DreamAbortException(DreamMessage.BadRequest(DekiResources.RESTRICTION_INFO_MISSING));

            //Retrieve role from the db.
            RoleBE restriction = PermissionsBL.GetRestrictionByName(pageRestrictionStr);
            if(restriction == null)
                throw new DreamAbortException(DreamMessage.BadRequest(string.Format(DekiResources.RESTRICITON_NOT_FOUND, pageRestrictionStr)));

            //Parse given xml to GrantBE's. This throws a bad request exception on parse problems.
            List<GrantBE> grants = PermissionsBL.ReadGrantsXml(request.ToDocument()["grants"], page, false);

            //Apply validation logic and add grants
            PermissionsBL.ReplacePagePermissions(page, restriction, grants, cascade);

            DekiContext.Current.Instance.EventSink.PageSecuritySet(context.StartTime, page, cascade);

            //Return the parsed grants to client
            response.Return(DreamMessage.Ok(PageBL.GetSecurityXml(page)));
            yield break;
        }

        [DreamFeature("POST:pages/{pageid}/security", "Modify page security by adding and removing grants")]
        [DreamFeatureParam("{pageid}", "string", "either an integer page ID, \"home\", or \"=\" followed by a double uri-encoded page title")]
        [DreamFeatureParam("redirects", "int?", "If zero, do not follow page redirects.")]
        [DreamFeatureParam("cascade", "{none, delta}", "Apply proposed security to child pages. default: none")]
        [DreamFeatureParam("authenticate", "bool?", "Force authentication for request (default: false)")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "Change permissions access to the page is required")]
        [DreamFeatureStatus(DreamStatus.NotFound, "Requested page could not be found")]
        public Yield PostPageSecurity(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            PageBE page = PageBL.AuthorizePage(DekiContext.Current.User, Permissions.CHANGEPERMISSIONS, false);
            CascadeType cascade = CascadeType.NONE;
            switch(context.GetParam("cascade", "none").ToLowerInvariant().Trim()) {
            case "delta":
                cascade = CascadeType.DELTA;
                break;
            case "none":
                cascade = CascadeType.NONE;
                break;
            default:
                throw new DreamBadRequestException(DekiResources.CASCADE_PARAM_INVALID);
            }

            RoleBE restriction = null;
            //Parse out the OPTIONAL page restriction attribute
            string pageRestrictionStr = request.ToDocument()["permissions.page/restriction"].Contents;

            //Retrieve role from the db.
            if(!string.IsNullOrEmpty(pageRestrictionStr)) {
                restriction = PermissionsBL.GetRestrictionByName(pageRestrictionStr);
                if(restriction == null)
                    throw new DreamAbortException(DreamMessage.BadRequest(string.Format(DekiResources.RESTRICITON_NOT_FOUND, pageRestrictionStr)));
            }

            //Parse given xml to GrantBE's. This throws a bad request exception on parse problems.
            List<GrantBE> grantsAdded = PermissionsBL.ReadGrantsXml(request.ToDocument()["grants.added"], page, false);
            List<GrantBE> grantsRemoved = PermissionsBL.ReadGrantsXml(request.ToDocument()["grants.removed"], page, false);

            if(grantsAdded.Count != 0 || grantsRemoved.Count != 0 || restriction != null) {

                //Apply validation logic and add grants.
                PermissionsBL.ApplyDeltaPagePermissions(page, restriction, grantsAdded, grantsRemoved, cascade == CascadeType.DELTA);
            }

            DekiContext.Current.Instance.EventSink.PageSecurityUpdated(context.StartTime, page, cascade);

            //Return the parsed grants to client
            response.Return(DreamMessage.Ok(PageBL.GetSecurityXml(page)));
            yield break;
        }

        [DreamFeature("POST:pages/{pageid}/message/*//*", "Post a custom page event into the pubsub bus (limited to 128KB)")]
        [DreamFeatureParam("{pageid}", "string", "either an integer page ID, \"home\", or \"=\" followed by a double uri-encoded page title")]
        [DreamFeatureStatus(DreamStatus.Ok, "The request completed successfully")]
        [DreamFeatureStatus(DreamStatus.BadRequest, "Invalid input parameter or request body")]
        [DreamFeatureStatus(DreamStatus.Forbidden, "A logged-in user is required")]
        public Yield PostPageMessage(DreamContext context, DreamMessage request, Result<DreamMessage> response) {
            if(UserBL.IsAnonymous(DekiContext.Current.User)) {
                response.Return(DreamMessage.Forbidden("A logged-in user is required"));
                yield break;
            }
            if(request.ContentLength > 128 * 1024) {
                response.Return(DreamMessage.BadRequest("Content-length cannot exceed 128KB)"));
                yield break;
            }
            PageBE page = PageBL.AuthorizePage(DekiContext.Current.User, Permissions.READ, false);
            XDoc body = new XDoc("body");
            switch(request.ContentType.FullType) {
            case "text/plain":
                body.Attr("content-type", request.ContentType.ToString())
                    .Value(request.AsText());
                break;
            default:
                body.Attr("content-type", request.ContentType.ToString())
                    .Add(request.ToDocument());
                break;
            }
            string[] path = context.GetSuffixes(UriPathFormat.Original);
            path = ArrayUtil.SubArray(path, 1);
            DekiContext.Current.Instance.EventSink.PageMessage(DreamContext.Current.StartTime, page, DekiContext.Current.User, body, path);
            response.Return(DreamMessage.Ok());
            yield break;
        }

        #endregion grants

    }
}
