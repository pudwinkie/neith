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

namespace MindTouch.Deki.UserSubscription {

    // Note: this class is not thread-safe. It is assumed that the using code (NotificationDelayQueue) handles the appropriate
    //       thread locks when manipulating it
    public class NotificationUpdateRecord {

        //--- Fields ---
        public readonly string WikiId;
        public readonly uint UserId;
        private readonly Dictionary<uint, Tuplet<DateTime, bool>> _pages;

        //--- Constructors ---
        public NotificationUpdateRecord(string wikiId, uint userId) {
            WikiId = wikiId;
            UserId = userId;
            _pages = new Dictionary<uint, Tuplet<DateTime, bool>>();
        }

        //--- Properties ---
        public IEnumerable<Tuplet<uint, DateTime, bool>> Pages {
            get {
                foreach(KeyValuePair<uint, Tuplet<DateTime, bool>> kvp in _pages) {
                    yield return new Tuplet<uint, DateTime, bool>(kvp.Key, kvp.Value.Item1, kvp.Value.Item2);
                }
            }
        }

        //--- Methods ---
        public void Add(uint pageId, DateTime modificationDate, bool delete) {
            Tuplet<DateTime, bool> record;
            if(!_pages.TryGetValue(pageId, out record)) {
                record = new Tuplet<DateTime, bool>(modificationDate, delete);
                _pages.Add(pageId, record);
            }
            if(!record.Item2 && delete) {
                record.Item2 = true;
            }
        }
    }
}
