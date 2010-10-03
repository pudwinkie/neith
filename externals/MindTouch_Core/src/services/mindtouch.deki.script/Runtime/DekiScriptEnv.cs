/*
 * MindTouch DekiScript - embeddable web-oriented scripting runtime
 * Copyright (c) 2006-2010 MindTouch Inc.
 * www.mindtouch.com  oss@mindtouch.com
 *
 * For community documentation and downloads visit wiki.developer.mindtouch.com;
 * please review the licensing section.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using MindTouch.Deki.Script.Expr;

namespace MindTouch.Deki.Script.Runtime {
    public class DekiScriptEnv {

        //--- Constants ---
        public const string SAFEMODE = "__safe";
        public const string SETTINGS = "__settings";
        public const string CALLSTACK = "__callstack";

        //--- Types ---
        public delegate DekiScriptLiteral ResolverDelegate(string name);

        //--- Fields ---
        public readonly DekiScriptMap Vars;
        private DekiScriptMap _magicIds;

        //--- Constructors ---
        public DekiScriptEnv() {
            this.Vars = new DekiScriptMap();
        }

        public DekiScriptEnv(
            DekiScriptMap vars
        ) {
            if(vars == null) {
                throw new ArgumentNullException("vars");
            }
            this.Vars = vars;
        }

        //--- Properties ---
        public bool IsSafeMode {
            get {

                // TODO (steveb): we need to move SAFEMODE into a different location where it doesn't run the risk of being modified

                DekiScriptLiteral result;
                if(Vars.TryGetValue(SAFEMODE, out result)) {
                    return result.AsBool() ?? true;
                }
                return true;
            }
        }

        //--- Methods ---
        public DekiScriptEnv NewScope() {
            return new DekiScriptEnv(new DekiScriptMap(Vars));
        }

        public DekiScriptLiteral GetMagicId(string id) {
            DekiScriptLiteral result = DekiScriptNil.Value;

            // check if magic IDs map already exists; if not, create one
            if(_magicIds == null) {
                _magicIds = new DekiScriptMap();
            } else {
                result = _magicIds[id];
            }

            // check if a magic ID was found; if not, create one
            if(result.IsNil) {
                result = DekiScriptExpression.Constant(id + "_" + StringUtil.CreateAlphaNumericKey(8));
                _magicIds.Add(id, result);
            }
            return result;
        }
    }
}