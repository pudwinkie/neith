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
using MindTouch.Dream;

namespace MindTouch.Deki.Script.Runtime.TargetInvocation {
    public class DekiScriptHttpGetInvocationTarget : ADekiScriptInvocationTarget {
        
        //--- Fields ---
        private readonly XUri _endpoint;

        //--- Constructors ---
        public DekiScriptHttpGetInvocationTarget(XUri endpoint) {
            if(endpoint == null) {
                throw new ArgumentNullException("endpoint");
            }
            _endpoint = endpoint;
        }
        
        //--- Methods ---
        public override DekiScriptLiteral InvokeList(DekiScriptRuntime runtime, DekiScriptList args) {
            throw new DekiScriptBadTypeException(Location.None, args.ScriptType, new[] { DekiScriptType.MAP });
        }

        public override DekiScriptLiteral InvokeMap(DekiScriptRuntime runtime, DekiScriptMap args) {

            // prepare uri for invocation
            Plug plug = Plug.New(_endpoint);
            plug = runtime.PreparePlug(plug);

            // invoke function
            DreamMessage response = plug.Get();

            // convert response to literal
            DekiScriptLiteral list;
            try {
                list = DekiScriptLiteral.FromXml(response.ToDocument());
            } catch(ArgumentException) {
                throw new DekiScriptUnsupportedTypeException(Location.None, string.Format("<{0}>", response.ToDocument().Name));
            }
            if(list.ScriptType != DekiScriptType.LIST) {
                throw new DekiScriptBadReturnTypeException(Location.None, list.ScriptType, new[] { DekiScriptType.LIST });
            }
            return ((DekiScriptList)list)[0];
        }
    }
}