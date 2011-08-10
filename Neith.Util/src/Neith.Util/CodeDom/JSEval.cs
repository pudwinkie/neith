using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom.Compiler;
using System.Reflection;

namespace Neith.Util.CodeDom
{
    /// <summary>
    /// JScriptのEval関数を提供します。
    /// </summary>
    public class JSEval
    {
        /// <summary>
        /// 文字列をJScriptでeval実行します。
        /// </summary>
        /// <param name="expr">JScriptの式</param>
        /// <returns>計算結果</returns>
        public static object Eval(string expr)
        {
            return evalFunc(expr);
        }

        /// <summary>
        /// targetオブジェクトを利用してJScriptによるeval実行を行います。
        /// evalスクリプト内では[target]名でtargetオブジェクトにアクセス可能です。
        /// </summary>
        /// <param name="target">式内で[target]としてアクセスするオブジェクト</param>
        /// <param name="expr">JScriptの式</param>
        /// <returns>計算結果</returns>
        public static object EvalTarget(object target, string expr)
        {
            return evalTargetFunc(target, expr);
        }


        private delegate object EvalFunc(string expr);
        private delegate object EvalTargetFunc(object target, string expr);

        private const string source =
    @"package CSUtil.CodeDom
{
    class EvalJScript
    {
        public function Eval(expr : String) : Object 
        { 
            return eval(expr); 
        }
        public function EvalTarget(target : Object, expr : String) : Object 
        { 
            return eval(expr); 
        }
    }
}";
        private static readonly EvalFunc evalFunc = CreateEvalFunc();
        private static EvalTargetFunc evalTargetFunc;
        private static EvalFunc CreateEvalFunc()
        {
            //コンパイルするための準備
            CodeDomProvider cp = CodeDomProvider.CreateProvider("JScript");
            CompilerParameters cps = new CompilerParameters();
            CompilerResults cres;
            //メモリ内で出力を生成する
            cps.GenerateInMemory = true;
            //コンパイルする
            cres = cp.CompileAssemblyFromSource(cps, source);

            //コンパイルしたアセンブリを取得
            Assembly asm = cres.CompiledAssembly;
            //クラスのTypeを取得
            Type t = asm.GetType("CSUtil.CodeDom.EvalJScript");
            //インスタンスの作成
            object obj = Activator.CreateInstance(t);
            // デリゲートの作成
            evalTargetFunc = (EvalTargetFunc)Delegate.CreateDelegate(
              typeof(EvalTargetFunc), obj, "EvalTarget");
            return (EvalFunc)Delegate.CreateDelegate(
              typeof(EvalFunc), obj, "Eval");
        }

    }
}
