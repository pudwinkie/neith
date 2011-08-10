using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;

namespace Neith.Util.CodeDom.ExpressionBuilder
{
    internal class BuilderMethodReference : ET
    {
        private readonly ET Target;
        private readonly string Name;
        private readonly CodeTypeReference[] GenericTypes;

        public BuilderMethodReference(ET target, string name, params CodeTypeReference[] genericTypes)
        {
            this.Target = target;
            this.Name = name;
            this.GenericTypes = genericTypes;
        }

        public override CodeExpression Expression
        {
            get
            {
                return new CodeMethodReferenceExpression(Target.Expression, Name, GenericTypes);
            }
        }
    }
}
