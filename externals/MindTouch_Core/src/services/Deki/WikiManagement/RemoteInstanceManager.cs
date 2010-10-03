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

using MindTouch.Dream;
using MindTouch.Xml;

namespace MindTouch.Deki.WikiManagement {
    public class RemoteInstanceManager: InstanceManager{

        // --- Constants ---
        const int HOST_WIKIID_TIMEOUT = 5 * 60;

        // --- Fields ---
        Plug _directory;
        Dictionary<string, Tuplet<string, DateTime>> _hostsToWikiIds = new Dictionary<string, Tuplet<string, DateTime>>();

        // --- Constructors ---
        public RemoteInstanceManager(DekiWikiService dekiService, XUri directoryUri) : base(dekiService) {
            _directory = Plug.New(directoryUri);

            DreamMessage testMsg = _directory.GetAsync().Wait();
            if (!testMsg.IsSuccessful)
                throw new DreamInternalErrorException(string.Format("Error validating remote deki portal service at '{0}'", directoryUri.ToString()));
        }

        public override XDoc GetGlobalConfig() {
            return _dekiService.Config["wikis/globalconfig"];
        }

        protected override XDoc GetConfigForWikiId(string wikiId) {
            XDoc configDoc = XDoc.Empty;

            DreamMessage p = _directory.At(wikiId).GetAsync().Wait();
            if (p.IsSuccessful && p.ContentType.IsXml) {
                XDoc wikiDoc = p.ToDocument();
                configDoc = wikiDoc["config"];
                
                List<string> hosts = new List<string>();
                foreach (XDoc hostDoc in configDoc["host"]) {
                    string host = hostDoc.AsText;
                    hosts.Add(host);
                    lock (_hostsToWikiIds) {
                        _hostsToWikiIds[host] = new Tuplet<string, DateTime>(wikiId, DateTime.UtcNow);
                    }
                }

                configDoc["host"].RemoveAll();
                configDoc.Elem("hosts", string.Join(",", hosts.ToArray()));

                string status = wikiDoc["status"].AsText;
                if (!string.IsNullOrEmpty(status)) {
                    switch (status.ToLowerInvariant()) {
                    case "active":
                        break;
                    default: //TODO: define custom errors for more statuses
                        throw new DreamAbortException(new DreamMessage(DreamStatus.Gone, null, MimeType.TEXT, string.Format("Site is currently unavailable. Status: {0}", status)));
                    }
                }
                DateTime updated = wikiDoc["date.updated"].AsDate ?? DateTime.MinValue;
                if(updated != DateTime.MinValue) {
                    configDoc.Attr("updated", updated);
                }
            } else {
                _log.WarnFormat("Unable to lookup config for site '{0}'. Return status: '{1}'", wikiId, p.Status);
            }
            return configDoc;
        }

        protected override string GetWikiIdByHostname(string hostname) {
            Tuplet<string, DateTime> wikiId = null;
            lock(_hostsToWikiIds) {
                _hostsToWikiIds.TryGetValue(hostname, out wikiId);
                
                //Associations between a hostname and a wiki id should timeout at least every 5 minutes to allow hostnames to be switched.
                if (wikiId != null && wikiId.Item2.Add(TimeSpan.FromSeconds(Math.Min((int) InactiveInstanceTimeOut.TotalSeconds, HOST_WIKIID_TIMEOUT))) < DateTime.UtcNow) {
                    _hostsToWikiIds.Remove(hostname);
                    wikiId = null;
                }
            }
            if (wikiId == null) {
                DreamMessage p = _directory.At("="+hostname).GetAsync().Wait();
                if (p.IsSuccessful) {
                    XDoc wikiDoc = p.ToDocument();
                    wikiId = new Tuplet<string, DateTime>(wikiDoc["@id"].AsText, DateTime.UtcNow);
                    lock (_hostsToWikiIds) {
                        _hostsToWikiIds[hostname] = wikiId;
                    }
                }
            }
            
            if( wikiId == null)
                return null;
            else
                return wikiId.Item1;
        }

        protected override void ShutdownInstance(string wikiid, DekiInstance instance) {
         
            //On instance shutdown, remove host to wikiId association.
            string hosts = instance.Config["hosts"].AsText;
            if (!string.IsNullOrEmpty(hosts)) {
                lock (_hostsToWikiIds) {
                    foreach (string host in hosts.Split(',')) {
                        _hostsToWikiIds.Remove(host);
                    }
                }
            }

            base.ShutdownInstance(wikiid, instance);
        }
    }
}
