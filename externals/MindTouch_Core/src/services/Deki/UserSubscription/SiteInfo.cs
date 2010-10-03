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
using System.Globalization;

namespace MindTouch.Deki.UserSubscription {

    public class SiteInfo {

        //---  Fields ---
        public readonly string WikiId;
        public readonly Dictionary<uint, UserInfo> Users = new Dictionary<uint, UserInfo>();
        private string _sitename;
        private string _emailFromAddress;
        private CultureInfo _culture;
        private DateTime _lastUpdated = DateTime.MinValue;
        private string _emailFormat;

        //--- Constructors ---
        public SiteInfo(string wikiId) {
            WikiId = wikiId;
        }

        //--- Properties ---
        public string Sitename {
            get { return _sitename; }
            set { _sitename = value; _lastUpdated = DateTime.UtcNow; }
        }

        public string EmailFromAddress {
            get { return _emailFromAddress; }
            set { _emailFromAddress = value; _lastUpdated = DateTime.UtcNow; }
        }

        public CultureInfo Culture {
            get { return _culture; }
            set { _culture = value; _lastUpdated = DateTime.UtcNow; }
        }
        
        public string EmailFormat {
            get { return _emailFormat; }
            set { _emailFormat = value; _lastUpdated = DateTime.UtcNow; }
        }

        public bool IsValidated {
            get {
                return !string.IsNullOrEmpty(_sitename)
                    && !string.IsNullOrEmpty(_emailFromAddress)
                    && _culture != null;
            }
        }

        public DateTime LastUpdated {
            get { return _lastUpdated; }
        }
    }
}
