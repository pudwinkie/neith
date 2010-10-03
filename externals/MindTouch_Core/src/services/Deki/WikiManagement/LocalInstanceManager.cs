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

using System.Collections.Generic;

using MindTouch.Xml;

namespace MindTouch.Deki.WikiManagement {
    public class LocalInstanceManager : InstanceManager {

        // --- Fields ---
        private Dictionary<string, string> _hostNamesToWikiIds = new Dictionary<string, string>();

        // --- Constructors ---
        public LocalInstanceManager(DekiWikiService dekiService) : base(dekiService) {
            XDoc dekiServiceConfig = dekiService.Config;

            Dictionary<string, string> wikiIds = new Dictionary<string, string>();
            if (dekiServiceConfig["wikis/config"].IsEmpty) {

                //Not in cluster mode (no other wikis defined): run all hosts under default wiki.

                AssociateHostnameWithWiki("*", DEFAULT_WIKI_ID);
            } else {
                foreach (XDoc wikiDoc in dekiServiceConfig["wikis/config"]) {
                    string wikiId = wikiDoc["@id"].AsText;

                    List<string> hostNameList = new List<string>();
                    foreach (XDoc hostDoc in wikiDoc["host"]) {
                        hostNameList.Add(hostDoc.Contents);
                    }

                    AssociateHostnameWithWiki(hostNameList.ToArray(), wikiId);
                }
            }
        }

        // --- Methods ---
        public override XDoc GetGlobalConfig() {
            return _dekiService.Config["wikis/globalconfig"];
        }

        protected override XDoc GetConfigForWikiId(string wikiId) {
            string configXpath = string.Format("wikis/config[@id='{0}']", wikiId);
            XDoc instanceConfig = _dekiService.Config[configXpath];

            if (instanceConfig.IsEmpty && wikiId == DEFAULT_WIKI_ID && _dekiService.Config["wikis"].IsEmpty) {

                //For backwards compatibility with older style config doc, just extract the db settings from the service startup xml.
                //All other settings are either environmental(service-wide) or in the config table.
                instanceConfig = new XDoc("config");
                foreach (string s in new string[] { "db-server", "db-port", "db-catalog", "db-user", "db-password", "db-options" }) {
                    instanceConfig.Elem(s, _dekiService.Config[s].AsText);
                }
            }

            return instanceConfig;
        }

        protected override string GetWikiIdByHostname(string hostname) {

            //Resolve hostname to a wiki name
            string wikiName = null;

            lock (_hostNamesToWikiIds) {
                if (!_hostNamesToWikiIds.TryGetValue(hostname, out wikiName))
                    _hostNamesToWikiIds.TryGetValue("*", out wikiName);
            }
            return wikiName;
        }

        public override void Shutdown() {
            base.Shutdown();
            _hostNamesToWikiIds.Clear();
        }

        private void AssociateHostnameWithWiki(string hostname, string wikiId) {
            AssociateHostnameWithWiki(new string[] { hostname }, wikiId);
        }

        private void AssociateHostnameWithWiki(string[] hostnames, string wikiId) {
            lock (_hostNamesToWikiIds) {
                foreach (string host in hostnames) {
                    _hostNamesToWikiIds[host] = wikiId;
                }
            }
        }
    }
}
