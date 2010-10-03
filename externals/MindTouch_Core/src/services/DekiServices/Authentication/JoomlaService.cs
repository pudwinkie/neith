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

using MindTouch.Data;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Xml;

namespace MindTouch.Deki.Services {
    using Yield = IEnumerator<IYield>;

    [DreamService("MindTouch Joomla Authentication Service", "Copyright (c) 2006-2010 MindTouch Inc.",
       Info = "http://doc.opengarden.org/Deki_Wiki/Authentication/JoomlaAuthentication",
        SID = new string[] { 
            "sid://mindtouch.com/2007/07/joomla-authentication",
            "http://services.mindtouch.com/deki/draft/2007/07/joomla-authentication", 
        }
    )]
    [DreamServiceConfig("db-server", "string?", "Database host name (default: localhost).")]
    [DreamServiceConfig("db-port", "int?", "Database port (default: 3306)")]
    [DreamServiceConfig("db-catalog", "string", "Database name")]
    [DreamServiceConfig("db-user", "string", "Database user name")]
    [DreamServiceConfig("db-password", "string", "Password for database user")]
    [DreamServiceConfig("db-options", "string?", "Connection string parameters (default: \"\")")]
    [DreamServiceConfig("db-tableprefix", "string?", "Prefix for table names (default: \"jos_\")")]
    [DreamServiceBlueprint("deki/service-type", "authentication")]
    public class JoomlaAuthenticationService : DekiAuthenticationService {

        //--- Fields ---
        private DataFactory _factory;
        private DataCatalog _catalog;
        private string _prefix;
        
        //--- Properties ---
        public override string AuthenticationRealm {
            get {
                return "Joomla";
            }
        }

        protected override Yield Start(XDoc config, Result result) {
            yield return Coroutine.Invoke(base.Start, config, new Result());

            _factory = new DataFactory(MySql.Data.MySqlClient.MySqlClientFactory.Instance, "?");
            _catalog = new DataCatalog(_factory, config);
            _prefix = config["db-tableprefix"].AsText ?? "jos_";

            try {
                _catalog.TestConnection();
            } catch(Exception) {
                throw new Exception(string.Format("Unable to connect to joomla instance with connectionString: {0}", _catalog.ConnectionString));
            }
            result.Return();
        }

        public override bool CheckUserPassword(string user, string password) {
            DataCommand cmd = _catalog.NewQuery(string.Format("SELECT count(*) FROM {2}users WHERE username='{0}' AND password=md5('{1}') AND block=0 LIMIT 1", DataCommand.MakeSqlSafe(user), DataCommand.MakeSqlSafe(password), _prefix));
            long count = cmd.ReadAsLong() ?? 0;
            if(count > 0)
                return true;

            return false;
        }

        public override User GetUser(string user) {
            string name = "", email = "";
            DataCommand cmd = _catalog.NewQuery(string.Format("SELECT username, email, usertype FROM {1}users WHERE username='{0}' AND block=0 LIMIT 1", DataCommand.MakeSqlSafe(user), _prefix));
            XDoc result = cmd.ReadAsXDoc("users", "user");

            if(result["user"].IsEmpty)
                return null;

            name = result["user/username"].AsText;
            email = result["user/email"].AsText;
            Group group = new Group(result["user/usertype"].AsText);

            return new User(name, email, new Group[] { group });
        }

        public override Group[] GetGroups() {
            IList<Group> groupList = new List<Group>();
            DataCommand cmd = _catalog.NewQuery(string.Format("SELECT name FROM {0}usertypes", _prefix));
            XDoc groupsXDoc = cmd.ReadAsXDoc("usertypes", "usertype");
            foreach(XDoc group in groupsXDoc["usertype/name"])
                groupList.Add(new Group(group.AsText));

            Group[] groups = new Group[groupList.Count];
            groupList.CopyTo(groups, 0);
            return groups;
        }

        public override Group GetGroup(string group) {
            DataCommand cmd = _catalog.NewQuery(string.Format("SELECT name FROM {1}usertypes where name='{0}' LIMIT 1", DataCommand.MakeSqlSafe(group), _prefix));
            XDoc groupXDoc = cmd.ReadAsXDoc("groups", "group");
            if(groupXDoc["group/name"].IsEmpty)
                return null;

            return new Group(groupXDoc["group/name"].AsText);
        }
    }
}
