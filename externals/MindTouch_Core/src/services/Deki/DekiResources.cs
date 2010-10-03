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

namespace MindTouch.Deki {
    public sealed class DekiResources {

        // --- Constants ---
        public static string DEFAULT_SITE_NAME { get { return GetString("System.API.default-site-name"); } }
        public static string DEL_WITH_CHILDREN_PLACEHOLDER { get { return "{{wiki.localize('System.API.page-placeholder-for-children')}}"; } }
        public static string EMPTY_PARENT_ARTICLE_TEXT { get { return "{{wiki.localize('System.API.page-generated-for-subpage')}}"; } }
        public static string FILE_ADDED { get { return GetString("System.API.added-file"); } }
        public static string FILE_MOVED_FROM { get { return GetString("System.API.moved-file-from"); } }
        public static string FILE_MOVED_TO { get { return GetString("System.API.moved-file-to"); } }
        public static string FILE_RENAMED_TO { get { return GetString("System.API.renamed-file-to"); } }
        public static string FILE_REMOVED { get { return GetString("System.API.removed-file"); } }
        public static string FILE_RESTORED { get { return GetString("System.API.restored-file"); } }
        public static string FILE_DESCRIPTION_CHANGED { get { return GetString("System.API.user-changed-file-description"); } }
        public static string GRANT_ADDED { get { return DekiResources.GetString("System.API.user-grant-added"); } }
        public static string GRANT_REMOVED { get { return DekiResources.GetString("System.API.user-grant-removed"); } }
        public static string GRANT_REMOVED_ALL { get { return DekiResources.GetString("System.API.user-grant-removed-all"); } }
        public static string MAIN_PAGE { get { return GetString("System.API.main-page"); } }
        public static string MISSING_ARTICLE { get { return GetString("System.API.Error.missing-article"); } }
        public static string NEW_ARTICLE_TEXT { get { return GetString("System.API.new-article-text"); } }
        public static string NO_ARTICLE_TEXT { get { return GetString("System.API.no-text-in-page"); } }
        public static string NO_HEADINGS { get { return GetString("System.API.no-headers"); } }
        public static string NO_TOC_HERE { get { return GetString("System.API.page-has-no-toc"); } }
        public static string ONE_MOVED_TO_TWO { get { return GetString("System.API.one-moved-to-two"); } }
        public static string REVERTED { get { return GetString("System.API.reverted-to-earlier-version"); } }
        public static string USER_ADDED { get { return GetString("System.API.user-added"); } }
        public static string USER_INVITED { get { return GetString("System.API.user-invited-for-days"); } }
        public static string TABLE_OF_CONTENTS { get { return GetString("System.API.table-of-contents"); } }
        public static string AND { get { return GetString("System.API.and"); } }
        public static string EDIT_SUMMARY_ONE { get { return GetString("System.API.edited-once-by"); } }
        public static string EDIT_SUMMARY_TWO { get { return GetString("System.API.edited-twice-by"); } }
        public static string EDIT_SUMMARY_MANY { get { return GetString("System.API.edited-times-by"); } }
        public static string EDIT_MULTIPLE { get { return GetString("System.API.edited-multiple"); } }
        public static string NEW_PAGE { get { return GetString("System.API.new-page"); } }
        public static string REMOVE_FROM_WATCHLIST { get { return GetString("System.API.remove-from-watchlist"); } }
        public static string ADD_TO_WATCHLIST { get { return GetString("System.API.add-to-watchlist"); } }
        public static string WHATS_NEW { get { return GetString("System.API.whats-new"); } }
        public static string PAGE_NEWS { get { return GetString("System.API.page-changes"); } }
        public static string USER_NEWS { get { return GetString("System.API.user-contributions"); } }
        public static string USER_FAVORITES { get { return GetString("System.API.user-favorites"); } }
        public static string PAGES_EQUAL { get { return GetString("System.API.page-versions-identical"); } }
        public static string PAGE_DIFF_ERROR { get { return GetString("System.API.Error.page-diff"); } }
        public static string PAGE_DIFF_TOO_LARGE { get { return GetString("System.API.Error.page-diff-too-large"); } }
        public static string MORE_DOT_DOT_DOT { get { return GetString("System.API.more-dot-dot-dot"); } }
        public static string REDIRECTED_TO { get { return GetString("System.API.page-content-located-at"); } }
        public static string REDIRECTED_TO_BROKEN { get { return GetString("System.API.page-redirect-no-longer-exists"); } }
        public static string PAGE_DIFF_SUMMARY { get { return GetString("System.API.page-diff-added-removed"); } }
        public static string PAGE_DIFF_SUMMARY_ADDED { get { return GetString("System.API.page-diff-added"); } }
        public static string PAGE_DIFF_SUMMARY_NOT_VISIBLE { get { return GetString("System.API.page-diff-no-visible-changes"); } }
        public static string PAGE_DIFF_SUMMARY_NOTHING { get { return GetString("System.API.page-diff-no-changes"); } }
        public static string PAGE_DIFF_SUMMARY_REMOVED { get { return GetString("System.API.page-diff-words-removed"); } }
        public static string PAGE_DIFF_OTHER_CHANGES { get { return GetString("System.API.page-diff-other-changes"); } }
        public static string PAGE_DIFF_NOTHING { get { return GetString("System.API.page-diff-nothing"); } }
        public static string PAGE_DISPLAYNAME_CHANGED { get { return GetString("System.API.page-displayname-changed"); } }
        public static string PAGE_DISPLAYNAME_RESET { get { return GetString("System.API.page-displayname-reset"); } }
        public static string PAGE_CONTENTTYPE_CHANGED { get { return GetString("System.API.page-content-type-changed"); } }
        public static string PAGE_CREATED { get { return GetString("System.API.page-created"); } }
        public static string PAGE_LANGUAGE_CHANGED { get { return GetString("System.API.page-language-changed"); } }
        public static string LOGINEXTERNALUSERCONFLICT { get { return GetString("System.API.Error.user-name-exists-provider"); } }
        public static string LOGINEXTERNALUSERCONFLICTUNKNOWN { get { return GetString("System.API.Error.user-name-exists"); } }
        public static string UNSUPPORTED_TYPE { get { return GetString("System.API.Error.unsupported-type"); } }
        public static string BAD_TYPE { get { return GetString("System.API.Error.bad-type"); } }
        public static string PARSER_ERROR { get { return GetString("System.API.Error.parser-details"); } }
        public static string UNDEFINED_NAME { get { return GetString("System.API.Error.reference-to-undefined-name"); } }
        public static string INVOKE_ERROR { get { return GetString("System.API.Error.function-failed"); } }
        public static string DELETED_ARTICLE { get { return GetString("System.API.deleted-article"); } }
        public static string SERVICE_CHECK_SETTINGS { get { return GetString("System.API.check-service-settings"); } }
        public static string UNDELETED_ARTICLE { get { return GetString("System.API.restored-article"); } }
        public static string RESTOREATTACHMENTNEWPAGETEXT { get { return "{{wiki.localize('System.API.page-created-restored-attachment')}}"; } }
        public static string OR { get { return GetString("System.API.or"); } }
        public static string RESTRICT_MESSAGE { get { return GetString("System.API.page-is-restricted"); } }
        public static string PAGEMOVECONFLICTHOMEPAGE { get { return GetString("System.API.Error.cannot-move-to-home-page"); } }
        public static string PAGEMOVECONFLICTEXISTINGTITLE { get { return GetString("System.API.Error.title-conflicts-existing-title"); } }
        public static string PAGEMOVECONFLICTTITLE { get { return GetString("System.API.Error.title-same-as-current"); } }
        public static string PAGEMOVECONFLICTTEMPLATE { get { return GetString("System.API.Error.cannot-move-in-out-templates"); } }
        public static string PAGEMOVECONFLICTSPECIAL { get { return GetString("System.API.Error.cannot-move-in-out-special"); } }
        public static string PAGEMOVECONFLICTTITLENOTEDITABLE { get { return GetString("System.API.Error.cannot-move-to-namespace"); } }
        public static string PAGEMOVECONFLICTMOVEHOMEPAGE { get { return GetString("System.API.Error.cannot-move-home-page"); } }
        public static string PAGEMOVECONFLICTMOVEROOTUSER { get { return GetString("System.API.Error.cannot-move-user-page"); } }
        public static string PAGEMOVECONFLICTSOURCENAMESPACE { get { return GetString("System.API.Error.cannot-move-from-namespace"); } }
        public static string PAGEMOVECONFLICTMOVETODESCENDANT { get { return GetString("System.API.Error.cannot-move-page-to-child"); } }
        public static string COMMENT_ADDED { get { return GetString("System.API.comment-added"); } }
        public static string COMMENT_EDITED { get { return GetString("System.API.comment-edited"); } }
        public static string COMMENT_DELETED { get { return GetString("System.API.comment-deleted"); } }
        public static string NEWUSERPAGETEXT { get { return GetString("System.API.new-user-page-content"); } }
        public static string INVALID_TITLE { get { return GetString("System.API.Error.invalid-title"); } }
        public static string INVALID_REDIRECT { get { return GetString("System.API.Error.invalid-redirect"); } }
        public static string INVALID_REDIRECT_OPERATION { get { return GetString("System.API.Error.invalid-redirect-operation"); } } 
        public static string INTERNAL_ERROR { get { return GetString("System.API.Error.internal-error"); } }
        public static string OPENSEARCH_SHORTNAME { get { return GetString("System.API.opensearch-shortname"); } }
        public static string OPENSEARCH_DESCRIPTION { get { return GetString("System.API.opensearch-description"); } }
        public static string EDIT_PAGE { get { return GetString("Skin.Common.edit-page"); } }
        public static string CREATE_PAGE { get { return GetString("Skin.Common.new-page"); } }
        public static string VIEW_PAGE { get { return GetString("System.API.page-diff-view-page"); } }
        public static string VIEW_PAGE_DIFF { get { return GetString("System.API.page-diff-view-page-diff"); } }
        public static string VIEW_PAGE_HISTORY { get { return GetString("System.API.page-diff-view-page-history"); } }
        public static string BAN_USER { get { return GetString("System.API.page-diff-ban-user"); } }
        public static string PAGE_NOT_AVAILABLE { get { return GetString("System.API.page-diff-page-not-available"); } }
        public static string COMMENT_NOT_AVAILABLE { get { return GetString("System.API.page-diff-comment-not-available"); } }
        public static string RESTRICTION_CHANGED { get { return GetString("System.API.restriction-changed"); } }
        public static string PROTECTED { get { return GetString("System.API.protected"); } }

        #region DekiWiki-Comments
        public static string FAILED_EDIT_COMMENT { get { return GetString("System.API.Error.failed_edit_comment"); } }
        public static string FAILED_POST_COMMENT { get { return GetString("System.API.Error.failed_post_comment"); } }
        public static string COMMENT_NOT_FOUND { get { return GetString("System.API.Error.comment_not_found"); } }
        public static string FILTER_PARAM_INVALID { get { return GetString("System.API.Error.filter_param_invalid"); } }
        #endregion

        #region DekiWiki-Files
        public static string CANNOT_PARSE_NUMFILES { get { return GetString("System.API.Error.cannot_parse_numfiles"); } }
        public static string INVALID_FILE_RATIO { get { return GetString("System.API.Error.invalid_file_ratio"); } }
        public static string INVALID_FILE_SIZE { get { return GetString("System.API.Error.invalid_file_size"); } }
        public static string INVALID_FILE_FORMAT { get { return GetString("System.API.Error.invalid_file_format"); } }
        public static string COULD_NOT_RETRIEVE_FILE { get { return GetString("System.API.Error.could_not_retrieve_file"); } }
        public static string CANNOT_UPLOAD_TO_TEMPLATE { get { return GetString("System.API.Error.cannot_upload_to_template"); } }
        public static string FAILED_TO_SAVE_UPLOAD { get { return GetString("System.API.Error.failed_to_save_upload"); } }
        //public static string ATTACHMENT_EXISTS_ON_PAGE { get { return GetString("Attachment already exists on target page"); } }
        public static string FILE_ALREADY_REMOVED { get { return GetString("System.API.Error.file_already_removed"); } }
        public static string REVISION_HEAD_OR_INT { get { return GetString("System.API.Error.revision_head_or_int"); } }
        public static string REVISION_NOT_SUPPORTED { get { return GetString("System.API.Error.revision_not_supported"); } }
        public static string MAX_REVISIONS_ALLOWED { get { return GetString("System.API.Error.max_revisions_allowed"); } }
        public static string COULD_NOT_FIND_FILE { get { return GetString("System.API.Error.could_not_find_file"); } }
        public static string FILE_HAS_BEEN_REMOVED { get { return GetString("System.API.Error.file_has_been_removed"); } }
        public static string MISSING_FILENAME { get { return GetString("System.API.Error.missing_filename"); } }
        #endregion

        #region DekiWiki-Groups
        public static string GROUPID_PARAM_INVALID { get { return GetString("System.API.Error.groupid_param_invalid"); } }
        public static string GROUP_NOT_FOUND { get { return GetString("System.API.Error.group_not_found"); } }
        //public static string GROUP_ID_NOT_FOUND { get { return GetString("Group id '{0}' not found"); } }
        #endregion

        #region DekiWiki-Nav
        public static string OUTPUT_PARAM_INVALID { get { return GetString("System.API.Error.output_param_invalid"); } }
        #endregion

        #region DekiWiki-News
        public static string GIVEN_USER_NOT_FOUND { get { return GetString("System.API.Error.given_user_not_found"); } }
        public static string SINCE_PARAM_INVALID { get { return GetString("System.API.Error.since_param_invalid"); } }
        public static string MAX_PARAM_INVALID { get { return GetString("System.API.Error.max_param_invalid"); } }
        public static string OFFSET_PARAM_INVALID { get { return GetString("System.API.Error.offset_param_invalid"); } }
        public static string FORMAT_PARAM_INVALID { get { return GetString("System.API.Error.format_param_invalid"); } }
        #endregion

        #region DekiWiki-Pages
        public static string FORMAT_PARAM_MUST_BE { get { return GetString("System.API.Error.format_param_must_be"); } }
        public static string UNABLE_TO_EXPORT_PAGEID { get { return GetString("System.API.Error.unable_to_export_pageid"); } }
        public static string UNABLE_TO_EXPORT_PAGE_PRINCE_ERROR { get { return GetString("System.API.Error.unable_to_export_page_prince_error"); } }
        public static string DIR_IS_NOT_VALID { get { return GetString("System.API.Error.dir_is_not_valid"); } }
        public static string COULD_NOT_FIND_REVISION { get { return GetString("System.API.Error.could_not_find_revision"); } }
        //public static string MAX_PARAM_INVALID { get { return GetString("'max' parameter is not valid"); } }
        //public static string OFFSET_PARAM_INVALID { get { return GetString("'offset' parameter is not valid"); } }
        public static string MISSING_FUNCTIONALITY { get { return GetString("System.API.Error.missing_functionality"); } }
        public static string SECTION_PARAM_INVALID { get { return GetString("System.API.Error.section_param_invalid"); } }
        public static string CONTENT_TYPE_NOT_SUPPORTED { get { return GetString("System.API.Error.content_type_not_supported"); } }
        public static string PAGE_ALREADY_EXISTS { get { return GetString("System.API.Error.page_already_exists"); } }
        public static string EDITTIME_PARAM_INVALID { get { return GetString("System.API.Error.edittime_param_invalid"); } }
        public static string PAGE_WAS_MODIFIED { get { return GetString("System.API.Error.page_was_modified"); } }
        public static string HEADING_PARAM_INVALID { get { return GetString("System.API.Error.heading_param_invalid"); } }
        public static string INVALID_FORMAT_GIVEN { get { return GetString("System.API.Error.invalid_format_given"); } }
        public static string RESTRICTION_INFO_MISSING { get { return GetString("System.API.Error.restriction_info_missing"); } }
        public static string RESTRICITON_NOT_FOUND { get { return GetString("System.API.Error.restriciton_not_found"); } }
        public static string CASCADE_PARAM_INVALID { get { return GetString("System.API.Error.cascade_param_invalid"); } }
        public static string CANNOT_MODIFY_TALK { get { return GetString("System.API.Error.cannot_modify_talk"); } }
        public static string CANNOT_CREATE_TALK { get { return GetString("System.API.Error.cannot_create_talk"); } }
        public static string CANNOT_RELTO_TALK { get { return GetString("System.API.Error.cannot_relto_talk"); } } 

        #endregion

        #region DekiWiki-Ratings
        public static string RATING_INVALID_SCORE { get { return GetString("System.API.Error.rating_invalid_score"); } }        
        #endregion

        #region DekiWiki-RecycleBin
        public static string FILE_NOT_DELETED { get { return GetString("System.API.Error.file_not_deleted"); } }
        //public static string COULD_NOT_RETRIEVE_FILE { get { return GetString("Could not retrieve attachment fileid {0} rev {1}"); } }
        public static string LIMIT_PARAM_INVALID { get { return GetString("System.API.Error.limit_param_invalid"); } }
        public static string TITLE_PARAM_INVALID { get { return GetString("System.API.Error.title_param_invalid"); } }
        #endregion

        #region DekiWiki-Services
        public static string SERVICE_NOT_FOUND { get { return GetString("System.API.Error.service_not_found"); } }
        #endregion

        #region DekiWiki-Site
        public static string ERROR_DELETING_INDEX { get { return GetString("System.API.Error.error_deleting_index"); } }
        public static string MUST_BE_LOGGED_IN { get { return GetString("System.API.Error.must_be_logged_in"); } }
        public static string ERROR_PARSING_SEARCH_QUERY { get { return GetString("System.API.Error.error_parsing_search_query"); } }
        public static string ERROR_QUERYING_SEARCH_INDEX { get { return GetString("System.API.Error.error_querying_search_index"); } }
        public static string EXPECTED_IMAGE_MIMETYPE { get { return GetString("System.API.Error.expected_image_mimetype"); } }
        public static string EXPECTED_XML_CONTENT_TYPE { get { return GetString("System.API.Error.expected_xml_content_type"); } }
        public static string CANNOT_PROCESS_LOGO_IMAGE { get { return GetString("System.API.Error.cannot_process_logo_image"); } }
        public static string ERROR_NO_SUCH_RESOURCE { get { return GetString("System.API.Error.no_such_resource"); } }
        #endregion

        #region DekiWiki-SiteRoles
        public static string ROLEID_PARAM_INVALID { get { return GetString("System.API.Error.roleid_param_invalid"); } }
        public static string ROLE_NAME_NOT_FOUND { get { return GetString("System.API.Error.role_name_not_found"); } }
        public static string ROLE_ID_NOT_FOUND { get { return GetString("System.API.Error.role_id_not_found"); } }
        #endregion

        #region DekiWiki-Users
        //public static string GIVEN_USER_NOT_FOUND { get { return GetString("Given user was not found"); } }
        public static string ACCOUNTPASSWORD_PARAM_INVALID { get { return GetString("System.API.Error.accountpassword_param_invalid"); } }
        public static string GIVEN_USER_NOT_FOUND_USE_POST { get { return GetString("System.API.Error.given_user_not_found_use_post"); } }
        public static string INVALID_OPERATION_LIST { get { return GetString("System.API.Error.invalid_operation_list"); } }
        public static string EXPECTED_ROOT_NODE_PAGES { get { return GetString("System.API.Error.expected_root_node_pages"); } }
        public static string NEW_PASSWORD_NOT_PROVIDED { get { return GetString("System.API.Error.new_password_not_provided"); } }
        public static string NEW_PASSWORD_TOO_SHORT { get { return GetString("System.API.Error.new_password_too_short"); } }
        public static string UNABLE_TO_FIND_USER { get { return GetString("System.API.Error.unable_to_find_user"); } }
        public static string PASSWORD_CHANGE_LOCAL_ONLY { get { return GetString("System.API.Error.password_change_local_only"); } }
        public static string CANNOT_CHANGE_ANON_PASSWORD { get { return GetString("System.API.Error.cannot_change_anon_password"); } }
        public static string CURRENTPASSWORD_DOES_NOT_MATCH { get { return GetString("System.API.Error.currentpassword_does_not_match"); } }
        public static string MUST_BE_TARGET_USER_OR_ADMIN { get { return GetString("System.API.Error.must_be_target_user_or_admin"); } }
        public static string CANNOT_CHANGE_OWN_ALT_PASSWORD { get { return GetString("System.API.Error.cannot_change_own_alt_password"); } }
        public static string USERID_PARAM_INVALID { get { return GetString("System.API.Error.userid_param_invalid"); } }
        #endregion

        #region DekiXmlParser
        public static string CONTENT_CANNOT_BE_PARSED { get { return GetString("System.API.Error.content_cannot_be_parsed"); } }
        public static string XPATH_PARAM_INVALID { get { return GetString("System.API.Error.xpath_param_invalid"); } }
        //public static string SECTION_PARAM_INVALID { get { return GetString("'section' parameter is not valid"); } }
        public static string INFINITE_PAGE_INCLUSION { get { return GetString("System.API.Error.infinite_page_inclusion"); } }
        public static string PAGE_FORMAT_INVALID { get { return GetString("System.API.Error.page_format_invalid"); } }
        public static string MISSING_FILE { get { return GetString("System.API.Error.missing_file"); } }
        #endregion

        #region GrantBE
        public static string CANNOT_PARSE_GRANTS { get { return GetString("System.API.Error.cannot_parse_grants"); } }
        public static string USER_OR_GROUP_ID_NOT_GIVEN { get { return GetString("System.API.Error.user_or_group_id_not_given"); } }
        public static string ROLE_NOT_GIVEN { get { return GetString("System.API.Error.role_not_given"); } }
        public static string ROLE_UNRECOGNIZED { get { return GetString("System.API.Error.role_unrecognized"); } }
        public static string CANNOT_PARSE_EXPIRY { get { return GetString("System.API.Error.cannot_parse_expiry"); } }
        #endregion

        #region AttachmentBL
        public static string MAX_FILE_SIZE_ALLOWED { get { return GetString("System.API.Error.max_file_size_allowed"); } }
        public static string FILENAME_IS_INVALID { get { return GetString("System.API.Error.filename_is_invalid"); } }
        public static string FILE_TYPE_NOT_ALLOWED { get { return GetString("System.API.Error.file_type_not_allowed"); } }
        // public static string MAX_REVISIONS_ALLOWED { get { return GetString("A maximum of {0} revisions is allowed per file"); } }
        public static string RESTORE_FILE_FAILED_NO_PARENT { get { return GetString("System.API.Error.restore_file_failed_no_parent"); } }
        public static string ATTACHMENT_EXISTS_ON_PAGE { get { return GetString("System.API.Error.attachment_exists_on_page"); } }
        public static string FILE_RESTORE_NAME_CONFLICT { get { return GetString("System.API.Error.file_restore_name_conflict"); } }
        public static string ATTACHMENT_MOVE_INVALID_PARAM { get { return GetString("System.API.Error.attachment_move_invalid_param"); } }
        
        
        #endregion

        #region AttachmentPreviewBL
        public static string FAILED_WITH_MIME_TYPE { get { return GetString("System.API.Error.failed_with_mime_type"); } }
        public static string FORMAT_CONVERSION_WITH_SIZE_UNSUPPORTED { get { return GetString("System.API.Error.format_conversion_with_size_unsupported"); } }
        public static string IMAGE_REQUEST_TOO_LARGE { get { return GetString("System.API.Error.image_request_too_large"); } }
        public static string CANNOT_CREATE_THUMBNAIL { get { return GetString("System.API.Error.cannot_create_thumbnail"); } }
        #endregion

        #region AuthBL
        public static string INVALID_SERVICE_ID { get { return GetString("System.API.Error.invalid_service_id"); } }
        public static string NOT_AUTH_SERVICE { get { return GetString("System.API.Error.not_auth_service"); } }
        public static string AUTHENTICATION_FAILED { get { return GetString("System.API.Error.authentication_failed"); } }
        public static string USER_DISABLED { get { return GetString("System.API.Error.user_disabled"); } }
        public static string CANNOT_RETRIEVE_USER_FOR_TOKEN { get { return GetString("System.API.Error.cannot_retrieve_user_for_token"); } }
        #endregion

        #region CommentBL
        public static string COMMENT_MIMETYPE_UNSUPPORTED { get { return GetString("System.API.Error.comment_mimetype_unsupported"); } }
        public static string COMMENT_FOR { get { return GetString("System.API.comment_for"); } }
        public static string COMMENT_BY_TO { get { return GetString("System.API.comment_by_to"); } }
        public static string COMMENT_CONCURRENCY_ERROR { get { return GetString("System.API.Error.comment_concurrency_error"); } }
        #endregion

        #region ConfigBL
        public static string MISSING_REQUIRED_CONFIG_KEY { get { return GetString("System.API.Error.missing_required_config_key"); } }
        public static string ERROR_UPDATE_CONFIG_SETTINGS { get { return GetString("System.API.Error.error_update_config_settings"); } }
        #endregion

        #region ExternalServicesSA
        public static string UNABLE_TO_AUTH_WITH_SERVICE { get { return GetString("System.API.Error.unable_to_auth_with_service"); } }
        public static string SERVICE_NOT_STARTED { get { return GetString("System.API.Error.service_not_started"); } }
        public static string UNEXPECTED_EXTERNAL_USERNAME { get { return GetString("System.API.Error.unexpected_external_username"); } }
        public static string AUTHENTICATION_FAILED_FOR { get { return GetString("System.API.Error.authentication_failed_for"); } }
        public static string GROUP_DETAILS_LOOKUP_FAILED { get { return GetString("System.API.Error.group_details_lookup_failed"); } }
        public static string SERVICE_INFO_LOOKUP_FAILED { get { return GetString("System.API.Error.service_info_lookup_failed"); } }
        #endregion

        #region GroupBL
        public static string EXTERNAL_GROUP_NOT_FOUND { get { return GetString("System.API.Error.external_group_not_found"); } }
        public static string GROUP_EXISTS_WITH_SERVICE { get { return GetString("System.API.Error.group_exists_with_service"); } }
        public static string GROUP_ID_NOT_FOUND { get { return GetString("System.API.Error.group_id_not_found"); } }
        public static string GROUP_CREATE_UPDATE_FAILED { get { return GetString("System.API.Error.group_create_update_failed"); } }
        public static string EXPECTED_ROOT_NODE_USERS { get { return GetString("System.API.Error.expected_root_node_users"); } }
        public static string GROUP_MEMBERS_REQUIRE_SAME_AUTH { get { return GetString("System.API.Error.group_members_require_same_auth"); } }
        public static string GROUP_ID_ATTR_INVALID { get { return GetString("System.API.Error.group_id_attr_invalid"); } }
        public static string SERVICE_AUTH_ID_ATTR_INVALID { get { return GetString("System.API.Error.service_auth_id_attr_invalid"); } }
        public static string SERVICE_DOES_NOT_EXIST { get { return GetString("System.API.Error.service_does_not_exist"); } }
        public static string ROLE_DOES_NOT_EXIST { get { return GetString("System.API.Error.role_does_not_exist"); } }
        //public static string USER_ID_ATTR_INVALID { get { return GetString("/user/@id not specified or invalid"); } }
        public static string COULD_NOT_FIND_USER { get { return GetString("System.API.Error.could_not_find_user"); } }
        public static string EXTERNAL_GROUP_RENAME_NOT_ALLOWED { get { return GetString("System.API.Error.group_external_rename_not_allowed"); } }
        public static string GROUP_EXTERNAL_CHANGE_MEMBERS { get { return GetString("System.API.Error.group_external_change_members"); } }
        #endregion

        #region PageArchiveBL
        public static string CANNOT_RESTORE_PAGE_NAMED { get { return GetString("System.API.Error.cannot_restore_page_named"); } }
        public static string RESTORE_PAGE_ID_NOT_FOUND { get { return GetString("System.API.Error.restore_page_id_not_found"); } }        

        #endregion

        #region PageBL
        public static string INVALID_POSTED_DOCUMENT { get { return GetString("System.API.Error.invalid_posted_document"); } }
        public static string INVALID_POSTED_DOCUMENT_1 { get { return GetString("System.API.Error.invalid_posted_document_1"); } }
        public static string INVALID_PAGE_ID { get { return GetString("System.API.Error.invalid_page_id"); } }
        public static string UNABLE_TO_PARSE_PAGES_FROM_XML { get { return GetString("System.API.Error.unable_to_parse_pages_from_xml"); } }
        public static string UNABLE_TO_FIND_HOME_PAGE { get { return GetString("System.API.Error.unable_to_find_home_page"); } }
        public static string UNABLE_TO_FIND_OLD_PAGE_FOR_ID { get { return GetString("System.API.Error.unable_to_find_old_page_for_id"); } }
        public static string UNABLE_TO_RETRIEVE_PAGE_FOR_ID { get { return GetString("System.API.Error.unable_to_retrieve_page_for_id"); } }
        public static string SECTION_EDIT_EXISTING_PAGES_ONLY { get { return GetString("System.API.Error.section_edit_existing_pages_only"); } }
        public static string CANNOT_FIND_PAGE_WITH_REVISION { get { return GetString("System.API.Error.cannot_find_page_with_revision"); } }
        public static string PAGES_ALREADY_EXIST_AT_DEST { get { return GetString("System.API.Error.pages_already_exist_at_dest"); } }
        public static string CANNOT_MODIFY_SPECIAL_PAGES { get { return GetString("System.API.Error.cannot_modify_special_pages"); } }
        public static string HOMEPAGE_CANNOT_BE_DELETED { get { return GetString("System.API.Error.homepage_cannot_be_deleted"); } }
        //public static string REVISION_HEAD_OR_INT { get { return GetString("Revision may be HEAD or a positive integer"); } }
        public static string LANGUAGE_PARAM_INVALID { get { return GetString("System.API.Error.language_param_invalid"); } }
        public static string LANGUAGE_SET_TALK { get { return GetString("System.API.Error.language_set_talk"); } }        
        public static string PAGE_ID_PARAM_INVALID { get { return GetString("System.API.Error.page_id_param_invalid"); } }
        public static string CANNOT_FIND_REQUESTED_PAGE { get { return GetString("System.API.Error.cannot_find_requested_page"); } }
        public static string PAGE_CONCURRENCY_ERROR { get { return GetString("System.API.Error.page_concurrency_error"); } }
        #endregion

        #region PersmissionBL
        public static string PERMISSIONS_NOT_ALLOWED_ON { get { return GetString("System.API.Error.permissions_not_allowed_on"); } }
        public static string USER_WOULD_BE_LOCKED_OUT_OF_PAGE { get { return GetString("System.API.Error.user_would_be_locked_out_of_page"); } }
        public static string CANNOT_RETRIEVE_REQUIRED_ROLE { get { return GetString("System.API.Error.cannot_retrieve_required_role"); } }
        public static string DUPLICATE_ROLE { get { return GetString("System.API.Error.duplicate_role"); } }
        public static string DUPLICATE_GRANT_FOR_USER_GROUP { get { return GetString("System.API.Error.duplicate_grant_for_user_group"); } }
        public static string CANNOT_FIND_USER_WITH_ID { get { return GetString("System.API.Error.cannot_find_user_with_id"); } }
        public static string CANNOT_FIND_GROUP_WITH_ID { get { return GetString("System.API.Error.cannot_find_group_with_id"); } }
        public static string ACCESS_DENIED_TO { get { return GetString("System.API.Error.access_denied_to"); } }
        public static string ACCESS_DENIED_TO_FOR_PAGE { get { return GetString("System.API.Error.access_denied_to_for_page"); } }
        public static string OPERATION_DENIED_FOR_ANONYMOUS { get { return GetString("System.API.Error.operation_denied_for_anonymous"); } }
        public static string ROLD_NAME_PARAM_INVALID { get { return GetString("System.API.Error.rold_name_param_invalid"); } }
        public static string ANONYMOUS_USER_EDIT { get { return GetString("System.API.Error.anonymous_user_edit"); } }
        #endregion

        #region ServiceBL
        public static string EXPECTED_SERVICE_TO_HAVE_SID { get { return GetString("System.API.Error.expected_service_to_have_sid"); } }
        public static string SERVICE_ADMINISTRATION_DISABLED { get { return GetString("System.API.Error.service_administration_disabled"); } }
        public static string SERVICE_CREATE_SID_MISSING { get { return GetString("System.API.Error.service_create_sid_missing"); } }
        public static string SERVICE_CREATE_TYPE_MISSING { get { return GetString("System.API.Error.service_create_type_missing"); } }
        public static string SERVICE_UPDATE_TYPE_INVALID { get { return GetString("System.API.Error.service_update_type_invalid"); } }
        public static string SERVICE_UNEXPECTED_INIT { get { return GetString("System.API.Error.service_unexpected_init"); } }
        public static string SERVICE_MISSING_DESCRIPTION { get { return GetString("System.API.Error.service_missing_description"); } }
        public static string SERVICE_INVALID_STATUS { get { return GetString("System.API.Error.service_invalid_status"); } }
        #endregion

        #region SiteBL
        public static string CANNOT_RETRIEVE_ADMIN_ACCOUNT { get { return GetString("System.API.Error.cannot_retrieve_admin_account"); } }
        public static string SMTP_SERVER_NOT_CONFIGURED { get { return GetString("System.API.Error.smtp_server_not_configured"); } }
        public static string ADMIN_EMAIL_NOT_SET { get { return GetString("System.API.Error.admin_email_not_set"); } }
        #endregion

        #region UserBL
        public static string USER_VALIDATION_FAILED { get { return GetString("System.API.Error.user_validation_failed"); } }
        public static string CANNOT_SET_EXTERNAL_ACCOUNT_PASSWORD { get { return GetString("System.API.Error.cannot_set_external_account_password"); } }
        public static string EXTERNAL_USER_NOT_FOUND { get { return GetString("System.API.Error.external_user_not_found"); } }
        public static string USER_EXISTS_WITH_EXTERNAL_NAME { get { return GetString("System.API.Error.user_exists_with_external_name"); } }
        public static string USER_EXISTS_WITH_ID { get { return GetString("System.API.Error.user_exists_with_id"); } }
        public static string USER_ID_ATTR_INVALID { get { return GetString("System.API.Error.user_id_attr_invalid"); } }
        public static string USER_ID_NOT_FOUND { get { return GetString("System.API.Error.user_id_not_found"); } }
        public static string USE_PUT_TO_CHANGE_PASSWORDS { get { return GetString("System.API.Error.use_put_to_change_passwords"); } }
        public static string UPDATE_USER_AUTH_SERVICE_NOT_ALLOWED { get { return GetString("System.API.Error.update_user_auth_service_not_allowed"); } }
        public static string DEACTIVATE_ANONYMOUS_NOT_ALLOWED { get { return GetString("System.API.Error.deactivate_anonymous_not_allowed"); } }
        public static string USERNAME_PARAM_INVALID { get { return GetString("System.API.Error.username_param_invalid"); } }
        //public static string SERVICE_AUTH_ID_ATTR_INVALID { get { return GetString("'/user/service.authentication/@id' not provided or invalid"); } }
        //public static string SERVICE_DOES_NOT_EXIST { get { return GetString("service {0} does not exist"); } }
        //public static string ROLE_DOES_NOT_EXIST { get { return GetString("role '{0}' does not exist"); } }
        public static string USER_STATUS_ATTR_INVALID { get { return GetString("System.API.Error.user_status_attr_invalid"); } }
        public static string NO_REGISTRATION_FOUND { get { return GetString("System.API.Error.no_registration_found"); } }
        public static string REGISTRATION_EXPIRED { get { return GetString("System.API.Error.registration_expired"); } }
        public static string USER_ALREADY_EXISTS { get { return GetString("System.API.Error.user_already_exists"); } }
        public static string EXTERNAL_USER_RENAME_NOT_ALLOWED { get { return GetString("System.API.Error.user_external_rename_not_allowed"); } }
        public static string USER_RENAME_HOMEPAGE_CONFLICT { get { return GetString("System.API.Error.user_rename_homepage_conflict"); } }
        public static string INVALID_TIMEZONE_VALUE { get { return GetString("System.API.Error.invalid_timezone_value"); } }
        public static string INVALID_LANGUAGE_VALUE { get { return GetString("System.API.Error.invalid_language_value"); } }
        public static string USER_AUTHSERVICE_CHANGE_FAIL { get { return GetString("System.API.Error.user_authservice_change_fail"); } }
        
        #endregion

        #region FSStorage
        public static string PATH_CONFIG_MISSING { get { return GetString("System.API.Error.path_config_missing"); } }
        public static string CAN_ONLY_MOVE_HEAD_REVISION { get { return GetString("System.API.Error.can_only_move_head_revision"); } }
        public static string DEST_PAGE_HAS_FILE_WITH_SAME_NAME { get { return GetString("System.API.Error.dest_page_has_file_with_same_name"); } }
        public static string CANNOT_CREATE_FILE_DIRECTORY { get { return GetString("System.API.Error.cannot_create_file_directory"); } }
        public static string CANNOT_MOVE_FILE_DELETED_EXISTS { get { return GetString("System.API.Error.cannot_move_file_deleted_exists"); } }
        public static string ERROR_MOVING_FILE_REVISIONS { get { return GetString("System.API.Error.error_moving_file_revisions"); } }
        public static string CAN_ONLY_DELETE_HEAD_REVISION { get { return GetString("System.API.Error.can_only_delete_head_revision"); } }
        public static string CANNOT_SAVE_FILE_TO { get { return GetString("System.API.Error.cannot_save_file_to"); } }
        public static string CANNOT_SET_PERMISSIONS_ON_FILE { get { return GetString("System.API.Error.cannot_set_permissions_on_file"); } }
        public static string SOURCE_FILES_MISSING { get { return GetString("System.API.Error.source_files_missing"); } }
        #endregion

        #region HttpCacheHelpers
        public static string UNABLE_TO_PARSE_VAL_HEADER_VAL { get { return GetString("System.API.Error.unable_to_parse_val_header_val"); } }
        #endregion

        #region DekiContext
        public static string NO_INSTANCE_FOR_HOSTNAME { get { return GetString("System.API.Error.no_instance_for_hostname"); } }
        #endregion

        #region LicenseBL
        public static string LICENSE_NO_NEW_USER_CREATION { get { return GetString("System.API.Error.license_no_new_user_creation"); } }
        public static string LICENSE_LIMIT_USER_CREATION { get { return GetString("System.API.Error.license_limit_user_creation"); } }
        public static string LICENSE_LIMIT_TOO_MANY_USERS { get { return GetString("System.API.Error.license_limit_too_many_users"); } }        
        public static string LICENSE_LIMIT_INSTANCE_CREATION { get { return GetString("System.API.Error.license_limit_instance_creation"); } }
        public static string LICENSE_UPDATE_INVALID { get { return GetString("System.API.Error.license_update_invalid"); } }
        public static string LICENSE_UPDATE_EXPIRED { get { return GetString("System.API.Error.license_update_expired"); } }
        public static string LICENSE_TRANSITION_INVALID { get { return GetString("System.API.Error.license_transition_invalid"); } }
        public static string LICENSE_UPDATE_PRODUCTKEY_INVALID { get { return GetString("System.API.Error.license_update_productkey_invalid"); } }
        public static string LICENSE_OPERATION_NOT_ALLOWED { get { return GetString("System.API.Error.license_operation_not_allowed"); } }
        
        #endregion

        #region PropertyBL
        public static string PROPERTY_DUPE_EXCEPTION { get { return GetString("System.API.Error.property_dupe_exception"); } }
        public static string PROPERTY_UNEXPECTED_ETAG { get { return GetString("System.API.Error.property_unexpected_etag"); } }
        public static string PROPERTY_ALREADY_EXISTS { get { return GetString("System.API.Error.property_already_exists"); } }
        public static string PROPERTY_DOESNT_EXIST_DELETE { get { return GetString("System.API.Error.property_doesnt_exist_delete"); } }
        public static string PROPERTY_INVALID_MIMETYPE { get { return GetString("System.API.Error.property_invalid_mimetype"); } }
        public static string PROPERTY_CREATE_MISSING_SLUG { get { return GetString("System.API.Error.property_create_missing_slug"); } }
        public static string PROPERTY_EDIT_NONEXISTING_CONFLICT { get { return GetString("System.API.Error.property_edit_nonexisting_conflict"); } }
        public static string PROPERTY_EXISTS_CONFLICT { get { return GetString("System.API.Error.property_exists_conflict"); } }
        public static string PROPERTY_CONCURRENCY_ERROR { get { return GetString("System.API.Error.property_concurrency_error"); } }        
        #endregion

        #region Banning
        public static string BANNING_NOT_FOUND_ID { get { return GetString("System.API.Error.banning_not_found_id"); } }
        public static string BANNING_EMPTY_BAN { get { return GetString("System.API.Error.banning_empty_ban"); } }
        public static string BANNING_NO_PERMS { get { return GetString("System.API.Error.banning_no_perms"); } }        
        #endregion

        #region TagBL
        public static string TAG_ADDED { get { return GetString("System.API.added-tags"); } }
        public static string TAG_REMOVED { get { return GetString("System.API.removed-tags"); } }
        public static string CANNOT_FIND_REQUESTED_TAG { get { return GetString("System.API.Error.cannot_find_requested_tag"); } }
        public static string TAG_INVALID { get { return GetString("System.API.Error.invalid-tag"); } }
        #endregion

        //--- Class Methods ---
        public static String GetString(string name) {

            // NOTE (royk): it's possible to pass in a key with embedded javascript from $_GET, which 
            //              creates a XSS vulnerability, so let's not pass back this information

            return GetString(name, null) ?? string.Format("[MISSING: {0}]", name.ReplaceAll("&", "&amp;", "<", "&lt;", ">", "&gt;")).EscapeString();
        }

        public static String GetString(string name, string def) {
            return DekiWikiService.ResourceManager.GetString(name, DreamContext.Current.Culture, def);
        }
    }
}
