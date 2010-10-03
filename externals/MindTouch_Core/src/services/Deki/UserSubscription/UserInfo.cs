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

using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Xml;

namespace MindTouch.Deki.UserSubscription {
    public class UserInfo {

        //--- Class Fields ---
        public static TimeSpan ValidationExpireTime = TimeSpan.FromHours(2);

        //--- Class Methods ---
        public static UserInfo FromXDoc(string wikiId, XDoc userDoc) {
            uint? userId = userDoc["@userid"].AsUInt;
            if(!userId.HasValue) {
                return null;
            }
            List<Tuplet<uint, string>> resources = new List<Tuplet<uint, string>>();
            foreach(XDoc sub in userDoc["subscription.page"]) {
                resources.Add(new Tuplet<uint, string>(sub["@id"].AsUInt.GetValueOrDefault(), sub["@depth"].AsText));
            }
            return new UserInfo(userId.Value, wikiId, resources);
        }

        //--- Fields ---
        public readonly uint Id;
        public readonly string WikiId;
        private readonly List<Tuplet<uint, string>> _resources = new List<Tuplet<uint, string>>();
        private string _email;
        private readonly TaskTimer _expire;
        private bool _isValidated;
        private bool _isDirty;
        private CultureInfo _culture = CultureInfo.InvariantCulture;
        private string _timezone = null;

        //--- Constructors ---
        public UserInfo(uint id, string wikiid)
            : this() {
            Id = id;
            WikiId = wikiid;
        }

        private UserInfo(uint id, string wikiid, IEnumerable<Tuplet<uint, string>> resources)
            : this() {
            Id = id;
            WikiId = wikiid;
            _resources.AddRange(resources);
        }

        private UserInfo() {
            _expire = new TaskTimer(delegate { IsValidated = false; }, null);
            _isDirty = true;
        }

        //--- Properties ---
        public Tuplet<uint, string>[] Resources { get { return _resources.ToArray(); } }

        public CultureInfo Culture {
            get { return _culture; }
            set {
                if(_culture == value) {
                    return;
                }
                _culture = value;
                _isDirty = true;
            }
        }

        public string Email {
            get { return _email; }
            set {
                if(_email == value) {
                    return;
                }
                _email = value;
                _isDirty = true;
            }
        }

        public string Timezone {
            get { return _timezone; }
            set {
                if(_timezone == value) {
                    return;
                }
                _timezone = value;
                _isDirty = true;
            }
        }

        public bool IsValidated {
            get { return _isValidated; }
            private set {
                _isValidated = value;
                if(_isValidated) {
                    _expire.Change(ValidationExpireTime, TaskEnv.None);
                }
            }
        }

        public XDoc AsDocument {
            get {
                XDoc doc = new XDoc("user").Attr("userid", Id);
                lock(_resources) {
                    foreach(Tuplet<uint, string> resource in _resources) {
                        doc.Start("subscription.page").Attr("depth", resource.Item2).Attr("id", resource.Item1).End();
                    }
                }
                return doc;
            }
        }

        //--- Methods ---
        public void Invalidate() {
            _isDirty = true;
            IsValidated = false;
        }

        public void AddResource(uint pageId, string depth) {
            if(string.IsNullOrEmpty(depth)) {
                depth = "0";
            }
            lock(_resources) {

                // since there shouldn't be two subscriptions for the same page with different depths
                // we just search for the page
                Tuplet<uint, string> existing = _resources.Find(delegate(Tuplet<uint, string> x) {
                    return pageId == x.Item1;
                });
                if(existing == null) {
                    _resources.Add(new Tuplet<uint, string>(pageId, depth));
                } else if(depth != existing.Item2) {
                    existing.Item2 = depth;
                } else {
                    return;
                }
                _isDirty = true;
                OnResourcesChanged();
            }
        }

        public void RemoveResource(uint pageId) {
            lock(_resources) {

                // since there shouldn't be two subscriptions for the same resource with different depths
                // we just search for the resource
                Tuplet<uint, string> existing = _resources.Find(delegate(Tuplet<uint, string> x) {
                    return pageId == x.Item1;
                });
                if(existing == null) {
                    return;
                }
                _resources.Remove(existing);
                _isDirty = true;
                OnResourcesChanged();
            }
        }

        private void OnResourcesChanged() {
            if(ResourcesChanged != null) {
                ResourcesChanged(this, EventArgs.Empty);
            }
        }

        //--- Events ---
        public event EventHandler<EventArgs> ResourcesChanged;
        public event EventHandler<EventArgs> DataChanged;

        public void Save() {
            IsValidated = true;
            if(_isDirty && DataChanged != null) {
                DataChanged(this, EventArgs.Empty);
            }
            _isDirty = false;
        }

        public XDoc GetSubscriptionDoc(List<uint> pages) {
            XDoc subscription = new XDoc("subscriptions");
            bool all = pages.Count == 0;
            foreach(Tuplet<uint, string> tuple in Resources) {
                if(all || pages.Contains(tuple.Item1)) {
                    subscription.Start("subscription.page").Attr("id", tuple.Item1).Attr("depth", tuple.Item2).End();
                }
            }
            return subscription;
        }
    }
}
