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

using System.Xml;
using MindTouch.Deki.Script.Expr;
using MindTouch.Deki.Script.Runtime;
using MindTouch.Xml;

namespace MindTouch.Deki.Script.Compiler {
    internal struct DekiScriptExpressionEvaluationState {

        //--- Fields ---
        public readonly DekiScriptEvalMode Mode;
        public readonly DekiScriptEnv Env;
        public readonly XmlNamespaceManager Namespaces;
        public readonly DekiScriptOutputBuffer Buffer;
        public readonly DekiScriptRuntime Runtime;
        private readonly bool _safe;

        //--- Constructor ---
        public DekiScriptExpressionEvaluationState(DekiScriptEvalMode mode, DekiScriptEnv env, DekiScriptRuntime runtime) {
            this.Mode = mode;
            this.Env = env;
            this.Namespaces = new XmlNamespaceManager(XDoc.XmlNameTable);
            this.Buffer = new DekiScriptOutputBuffer();
            this.Runtime = runtime;
            _safe = (mode == DekiScriptEvalMode.EvaluateSafeMode);
        }

        public DekiScriptExpressionEvaluationState(DekiScriptEvalMode mode, DekiScriptEnv env, DekiScriptRuntime runtime, XmlNamespaceManager namespaces, DekiScriptOutputBuffer buffer) {
            this.Mode = mode;
            this.Env = env;
            this.Namespaces = namespaces;
            this.Buffer = buffer;
            this.Runtime = runtime;
            _safe = (mode == DekiScriptEvalMode.EvaluateSafeMode);
        }

        //--- Properties ---
        public bool SafeMode { get { return _safe; } }

        //--- Methods ---
        public DekiScriptExpressionEvaluationState With(DekiScriptEnv env) {
            return new DekiScriptExpressionEvaluationState(Mode, env, Runtime, Namespaces, Buffer);
        }

        public DekiScriptOutputBuffer.Range Push(DekiScriptLiteral value) {
            return Buffer.Push(value);
        }

        public DekiScriptLiteral Pop(DekiScriptOutputBuffer.Range range) {
            return Buffer.Pop(range, _safe);
        }

        public DekiScriptLiteral PopAll() {
            return Buffer.Pop(new DekiScriptOutputBuffer.Range(0, Buffer.Marker), _safe);
        }
    }
}