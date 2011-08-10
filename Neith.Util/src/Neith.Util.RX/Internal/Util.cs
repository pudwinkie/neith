using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace System
{
    internal static class Util
    {

        internal static string ToPropertyName<TObj, TRet>(this Expression<Func<TObj, TRet>> Property)
        {
            string prop_name = null;

            try {
                var prop_expr = Property.Body as MemberExpression;
                if (prop_expr.Expression.NodeType != ExpressionType.Parameter) {
                    throw new ArgumentException("Property expression must be of the form 'x => x.SomeProperty'");
                }

                prop_name = prop_expr.Member.Name;
            }
            catch (NullReferenceException) {
                throw new ArgumentException("Property expression must be of the form 'x => x.SomeProperty'");
            }
            return prop_name;
        }



    }
}
