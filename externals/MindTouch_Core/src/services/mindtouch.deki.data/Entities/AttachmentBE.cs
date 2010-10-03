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
using MindTouch.Dream;

namespace MindTouch.Deki.Data {
    public class AttachmentBE : ResourceBE {

        // -- Constants --
        const string META_IMAGEWIDTH = "width";
        const string META_IMAGEHEIGHT = "height";
        const string META_IMAGEFRAMES = "frames";
        const string META_FILEID = "fileid";

        // -- Constructors --
        public AttachmentBE()
            : base() {
            ResourceType = Type.FILE;
        }

        public AttachmentBE(ResourceBE res)
            : base(res) { }

        // -- Properties -- 

        //For convenience the ParentPageId is exposed as not nullable for AttachmentBE's
        public new ulong ParentPageId {
            get {
                return base.ParentPageId ?? 0;
            }
            set {
                base.ParentPageId = value;
            }
        }

        public int? ImageWidth {
            get { return MetaXml[META_IMAGEWIDTH].AsInt; }
            set { SetAttribute(META_IMAGEWIDTH, value); }
        }

        public int? ImageHeight {
            get { return MetaXml[META_IMAGEHEIGHT].AsInt; }
            set { SetAttribute(META_IMAGEHEIGHT, value); }
        }

        public int? ImageFrames {
            get { return MetaXml[META_IMAGEFRAMES].AsInt; }
            set { SetAttribute(META_IMAGEFRAMES, value); }
        }

        public uint? FileId {
            get { return MetaXml[META_FILEID].AsUInt; }
            set { SetAttribute(META_FILEID, value); }
        }

        // -- Derived properties --
        public string Extension {
            get { return System.IO.Path.GetExtension(Name).TrimStart('.'); }
        }

        public string NameWithNoExtension {
            get {
                if(Extension.Length == 0) {
                    return Name;
                } else {
                    return Name.Substring(0, Name.Length - Extension.Length - 1);
                }
            }
        }
    }
}
