using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;

namespace Neith.Util.CodeDom.ExpressionBuilder
{
    internal class BuilderArrayIndex : ET
    {
        private readonly ET Target;
        private readonly ET[] Indexer;

        public BuilderArrayIndex(ET target, params ET[] indexer)
        {
            this.Target = target;
            this.Indexer = indexer;
        }

        public override CodeExpression Expression
        {
            get
            {
                return new CodeIndexerExpression(Target.Expression, GetExpresions(Indexer));
            }
        }

    }
}
