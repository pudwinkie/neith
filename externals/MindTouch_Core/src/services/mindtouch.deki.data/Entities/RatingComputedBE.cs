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

    [Serializable]
    public class RatingComputedBE {

        // -- Constructors --
        public RatingComputedBE() { }

        // -- Members --
        uint _id;
        ulong _resourceId;
        ResourceBE.Type _resourceType;
        float _score;
        float _scoreTrend;
        uint _count;
        DateTime _timestamp;

        // -- Properties --
        public uint Id {
            get { return _id; }
            set { _id = value; }
        }

        public ulong ResourceId {
            get { return _resourceId; }
            set { _resourceId = value; }
        }

        public ResourceBE.Type ResourceType {
            get { return _resourceType; }
            set { _resourceType = value; }
        }

        public float Score {
            get { return _score; }
            set { _score = value; }
        }

        public float ScoreTrend {
            get { return _scoreTrend; }
            set { _scoreTrend = value; }
        }

        public uint Count {
            get { return _count; }
            set { _count = value; }
        }

        public DateTime Timestamp {
            get { return _timestamp; }
            set { _timestamp = value; }
        }

        //--- Methods ---
        public virtual RatingComputedBE Copy() {
            RatingComputedBE r = new RatingComputedBE();
            r.Id = Id;
            r.Count = Count;
            r.ResourceId = ResourceId;
            r.ResourceType = ResourceType;
            r.Score = Score;
            r.ScoreTrend = ScoreTrend;
            r.Timestamp = Timestamp;
            return r;
        }
    }
}