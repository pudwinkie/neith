using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;

namespace Neith.Util.CodeDom.ExpressionBuilder
{
    internal class BuilderBinaryOperator : ET
    {
        private readonly ET Left;
        private readonly CodeBinaryOperatorType Operation;
        private readonly ET Right;

        public BuilderBinaryOperator(ET l, CodeBinaryOperatorType op, ET r)
        {
            this.Left = l;
            this.Operation = op;
            this.Right = r;
        }

        public override CodeExpression Expression
        {
            get { return new CodeBinaryOperatorExpression(Left.Expression, Operation, Right.Expression); }
        }
    }
}
