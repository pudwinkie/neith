using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;

namespace Neith.Util.CodeDom.ExpressionBuilder
{
    internal class BuilderMethodInvoke : ET
    {
        private BuilderMethodReference Method;
        private ET[] Parameters;

        public BuilderMethodInvoke(BuilderMethodReference method, params ET[] parameters)
        {
            this.Method = method;
            this.Parameters = parameters;
        }

        public override CodeExpression Expression
        {
            get
            {
                return new CodeMethodInvokeExpression(
                  (CodeMethodReferenceExpression)Method.Expression,
                  GetExpresions(Parameters));
            }
        }
    }
}
