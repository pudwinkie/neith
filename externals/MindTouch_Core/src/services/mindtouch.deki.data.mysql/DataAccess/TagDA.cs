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
using System.Text;
using System.Data;

using MindTouch.Data;
using MindTouch.Deki.Data;
using MySql.Data.MySqlClient;

namespace MindTouch.Deki.Data.MySql {

    public partial class MySqlDekiDataSession {

        public IList<ulong> Tags_GetPageIds(uint tagid) {
            List<ulong> ids = new List<ulong>();

            // Note that joining on pages is necessary to ensure that the page hasn't been deleted
            Catalog.NewQuery(@" /* Tags_GetPages */
SELECT tag_map.tagmap_page_id FROM pages
JOIN tag_map
    ON page_id = tagmap_page_id
WHERE tagmap_tag_id = ?TAGID;")
            .With("TAGID", tagid)
            .Execute(delegate(IDataReader dr) {
                while(dr.Read()) {
                    ids.Add(SysUtil.ChangeType<ulong>(dr[0]));
                }
            });
            return ids;
        }

        public IList<TagBE> Tags_GetByPageId(ulong pageid) {

            // retrieve the tags associated with a specified page id
            List<TagBE> tags = new List<TagBE>();
            Catalog.NewQuery(@" /* Tags_GetByPageId */
SELECT `tag_id`, `tag_name`, `tag_type`  
FROM `tag_map`  
JOIN `tags` 
    ON `tag_map`.`tagmap_tag_id`=`tags`.`tag_id`  
WHERE `tagmap_page_id` = ?PAGEID; ")
            .With("PAGEID", (uint)pageid)
            .Execute(delegate(IDataReader dr) {
                while(dr.Read()) {
                    TagBE t = Tags_Populate(dr);
                    if(t != null)
                        tags.Add(t);
                }
            });
            return tags;
        }

        public TagBE Tags_GetById(uint tagid) {
            TagBE tag = null;
            Catalog.NewQuery(@" /* Tags_GetById */
SELECT `tag_id`, `tag_name`, `tag_type` 
FROM `tags` 
WHERE `tag_id` = ?TAGID;")
            .With("TAGID", tagid)
            .Execute(delegate(IDataReader dr) {
                while(dr.Read()) {
                    tag = Tags_Populate(dr);
                }
            });
            return tag;
        }

        public TagBE Tags_GetByNameAndType(string tagName, TagType type) {
            TagBE tag = null;
            Catalog.NewQuery(@" /* Tags_GetByNameAndType */
SELECT `tag_id`, `tag_name`, `tag_type`
FROM `tags`
WHERE `tag_name` = ?TAGNAME 
AND `tag_type` = ?TAGTYPE;")
            .With("TAGNAME", tagName)
            .With("TAGTYPE", type)
            .Execute(delegate(IDataReader dr) {
                while(dr.Read()) {
                    tag = Tags_Populate(dr);
                }
            });
            return tag;
        }

        public bool Tags_ValidateDefineTagMapping(TagBE tag) {
            if(tag == null) {
                throw new ArgumentNullException("tag");
            }
            if(tag.Type != TagType.DEFINE) {
                throw new ArgumentException("Tag has to be of type DEFINE");
            }
            return Catalog.NewQuery(@" /* Tags_ValidateDefineTagMapping */
DELETE FROM tag_map WHERE tagmap_tag_id = ?TAGID AND (SELECT COUNT(*) FROM pages WHERE page_id = tagmap_page_id) = 0;
SELECT COUNT(*) FROM tag_map WHERE tagmap_tag_id = ?TAGID;")
                    .With("TAGID", tag.Id).ReadAsInt() > 0;
        }

        public IList<TagBE> Tags_GetByQuery(string partialName, TagType type, DateTime from, DateTime to) {

            // retrieve the tags associated with a specified page id
            List<TagBE> tags = new List<TagBE>();
            bool hasWhere = false;
            StringBuilder query = new StringBuilder();
            query.Append(@" /* Tags_GetByQuery */
SELECT `tag_id`, `tag_name`, `tag_type` 
FROM tags ");
            if(!string.IsNullOrEmpty(partialName)) {
                query.AppendFormat("WHERE tag_name LIKE '{0}%' ", DataCommand.MakeSqlSafe(partialName));
                hasWhere = true;
            }
            if(type != TagType.ALL) {
                if(hasWhere)
                    query.Append("AND ");
                else
                    query.Append("WHERE ");

                query.AppendFormat(" tag_type={0} ", (int)type);
                hasWhere = true;
            }
            if((type == TagType.DATE) && (from != DateTime.MinValue)) {
                if(hasWhere)
                    query.Append("AND ");
                else
                    query.Append("WHERE ");

                query.AppendFormat("tag_name >= '{0}' ", from.ToString("yyyy-MM-dd"));
                hasWhere = true;
            }
            if((type == TagType.DATE) && (to != DateTime.MaxValue)) {
                if(hasWhere)
                    query.Append("AND ");
                else
                    query.Append("WHERE ");

                query.AppendFormat("tag_name <= '{0}' ", to.ToString("yyyy-MM-dd"));
            }

            Catalog.NewQuery(query.ToString())
            .Execute(delegate(IDataReader dr) {
                while(dr.Read()) {
                    TagBE t = Tags_Populate(dr);
                    tags.Add(t);
                }
            });
            return tags;
        }

        public Dictionary<uint, IList<PageBE>> Tags_GetRelatedPages(IList<uint> tagids) {

            // retrieve a map of tag id to pages
            // each define tag maps to a list of related pages and each text tag maps to its defining page (if one exists) 
            Dictionary<uint, IList<PageBE>> result = new Dictionary<uint, IList<PageBE>>();
            if(0 < tagids.Count) {
                string tagIdsText = string.Join(",", DbUtils.ConvertArrayToDelimittedString<uint>(',', tagids));

                //TODO MaxM: This query is causing quite a bit of db load and needs to be optimized

                Catalog.NewQuery(string.Format(@" /* Tags_GetRelatedPages */
SELECT requested_tag_id as tag_id, page_id, page_title, page_namespace, page_display_name
FROM pages
JOIN tag_map
    ON page_id = tagmap_page_id 
JOIN
   (SELECT requestedtags.tag_id as requested_tag_id, tags.tag_id from tags 
    JOIN tags as requestedtags
        ON tags.tag_name = requestedtags.tag_name
    WHERE  (  ((tags.tag_type = 3 AND requestedtags.tag_type = 0) 
            OR (tags.tag_type = 0 AND requestedtags.tag_type = 3)) 
    AND	    requestedtags.tag_id IN ({0}))) relatedtags
ON tagmap_tag_id = tag_id;"
                    , tagIdsText))
                    .Execute(delegate(IDataReader dr) {
                    while(dr.Read()) {

                        // extract the tag to page mapping
                        uint tagid = DbUtils.Convert.To<UInt32>(dr["tag_id"], 0);
                        PageBE page = new PageBE();
                        page.ID = DbUtils.Convert.To<UInt32>(dr["page_id"], 0);
                        page.Title = DbUtils.TitleFromDataReader(dr, "page_namespace", "page_title", "page_display_name");

                        // add the mapping into the collection
                        IList<PageBE> relatedPages;
                        if(!result.TryGetValue(tagid, out relatedPages)) {
                            relatedPages = new List<PageBE>();
                            result[tagid] = relatedPages;
                        }
                        relatedPages.Add(page);
                    }
                });
            }

            return result;
        }

        public uint Tags_Insert(TagBE tag) {
            try {
                return Catalog.NewQuery(@" /* Tags_Insert */
INSERT INTO tags (tag_name, tag_type) VALUES (?TAGNAME, ?TAGTYPE);
SELECT LAST_INSERT_ID();")
                    .With("TAGNAME", tag.Name)
                    .With("TAGTYPE", tag.Type)
                    .ReadAsUInt() ?? 0;
            } catch(MySqlException e) {
                if(e.Number == 1062) {
                    _log.DebugFormat("tag '{0}'({1}) already exists, returning 0",tag.Name,tag.Type);
                    return 0;
                }
                throw;
            }
        }

        public void TagMapping_Delete(ulong pageId, IList<uint> tagids) {

            // deletes the specified page to tag mappings and removes the tag if it is no longer used
            if(0 < tagids.Count) {
                string tagIdsText = string.Join(",", DbUtils.ConvertArrayToDelimittedString<uint>(',', tagids));
                Catalog.NewQuery(String.Format(@" /* Tags_Delete */
DELETE FROM tag_map
WHERE tagmap_page_id = ?PAGEID AND tagmap_tag_id in ({0});

DELETE FROM tags
USING tags
LEFT JOIN tag_map tm ON tags.tag_id = tm.tagmap_tag_id
WHERE tm.tagmap_id IS NULL;", tagIdsText))
                            .With("PAGEID", pageId)
                            .Execute();
            }
        }

        public void TagMapping_Insert(ulong pageId, uint tagId) {
            Catalog.NewQuery(@" /* TagMapping_Insert */
REPLACE INTO tag_map (tagmap_page_id, tagmap_tag_id) VALUES (?PAGEID, ?TAGID);")
                .With("PAGEID", pageId)
                .With("TAGID", tagId)
                .Execute();
        }

        private TagBE Tags_Populate(IDataReader dr) {
            TagBE tag = new TagBE();
            tag._Type = dr.Read<uint>("tag_type");
            tag.Id = dr.Read<uint>("tag_id");
            tag.Name = dr.Read<string>("tag_name");
            return tag;
        }
    }
}
