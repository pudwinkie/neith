using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;

namespace Neith.Util.CodeDom.ExpressionBuilder
{
    internal class BuilderField : ET
    {
        private readonly ET Target;
        private readonly string Name;

        public BuilderField(ET target, string name)
        {
            this.Target = target;
            this.Name = name;
        }

        public override CodeExpression Expression
        {
            get { return new CodeFieldReferenceExpression(Target.Expression, Name); }
        }
    }
}
