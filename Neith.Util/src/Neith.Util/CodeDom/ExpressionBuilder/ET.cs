using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;

namespace Neith.Util.CodeDom.ExpressionBuilder
{
    /// <summary>
    /// CodeDom式を生成するためのビルドクラス。
    /// </summary>
    public abstract class ET
    {
        #region 暗黙的型変換
        /// <summary>
        /// CodeExpressionへのcastを行います。
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static implicit operator CodeExpression(ET target)
        {
            return target.Expression;
        }

        /// <summary>
        /// CodeExpressionStatementへのcastを行います。
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static implicit operator CodeExpressionStatement(ET target)
        {
            return target.Statement;
        }

        #endregion
        #region 式の取得
        /// <summary>
        /// 式を取得します。
        /// </summary>
        public abstract CodeExpression Expression { get;}

        /// <summary>
        /// 式を取得します。
        /// </summary>
        /// <param name="trees"></param>
        /// <returns></returns>
        protected static CodeExpression[] GetExpresions(ET[] trees)
        {
            CodeExpression[] codes = new CodeExpression[trees.Length];
            for (int i = 0; i < codes.Length; i++) codes[i] = trees[i].Expression;
            return codes;
        }

        /// <summary>
        /// 式文字列を返します。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "ET:(" + Expression + ")";
        }

        #endregion
        #region ステートメント生成

        /// <summary>
        /// 現在の式からなるステートメントを取得します。
        /// </summary>
        public CodeExpressionStatement Statement
        {
            get
            {
                return new CodeExpressionStatement(Expression);
            }
        }

        /// <summary>
        /// 代入式ステートメントを作成します。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static CodeAssignStatement Assign(ET target, ET value)
        {
            return new CodeAssignStatement(target.Expression, value.Expression);
        }

        /// <summary>
        /// 代入式ステートメントを作成します。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static CodeAssignStatement Assign(ET target, object value)
        {
            return Assign(target, V(value));
        }

        /// <summary>
        /// プロパティに代入するステートメントを作成します。
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public CodeAssignStatement AssignP(string propertyName, ET value)
        {
            return Assign(THIS.P(propertyName), value);
        }

        /// <summary>
        /// プロパティに代入するステートメントを作成します。
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public CodeAssignStatement AssignP(string propertyName, object value)
        {
            return Assign(THIS.P(propertyName), value);
        }

        /// <summary>
        /// フィールドに代入するステートメントを作成します。
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public CodeAssignStatement AssignF(string fieldName, ET value)
        {
            return Assign(THIS.F(fieldName), value);
        }

        /// <summary>
        /// フィールドに代入するステートメントを作成します。
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public CodeAssignStatement AssignF(string fieldName, object value)
        {
            return Assign(THIS.F(fieldName), value);
        }

        /// <summary>
        /// 現在の式に値を代入する代入式ステートメントを作成します。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public CodeAssignStatement Assign(ET value)
        {
            return Assign(this, value);
        }

        #endregion
        #region 式生成

        /// <summary>
        /// this参照。
        /// </summary>
        public static ET THIS { get { return new BuilderThis(); } }


        /// <summary>
        /// フィールドへの参照を作成します。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ET F(ET target, string name)
        {
            return new BuilderField(target, name);
        }

        /// <summary>
        /// フィールドへの参照を作成します。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ET F(string name)
        {
            return F(this, name);
        }


        /// <summary>
        /// フィールドへの参照を作成します。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ET P(ET target, string name)
        {
            return new BuilderProperty(target, name);
        }

        /// <summary>
        /// フィールドへの参照を作成します。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ET P(string name)
        {
            return P(this, name);
        }

        /// <summary>
        /// メソッドへの参照を作成します。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ET RefMethod(ET target, string name)
        {
            return new BuilderMethodReference(target, name);
        }

        /// <summary>
        /// メソッドへの参照を作成します。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ET RefMethod(string name)
        {
            return RefMethod(this, name);
        }


        /// <summary>
        /// メソッド呼び出しを作成します。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static ET M(ET target, string name, params ET[] parameters)
        {
            return new BuilderMethodInvoke(
              (BuilderMethodReference)target.RefMethod(name),
              parameters);
        }

        /// <summary>
        /// メソッド呼び出しを作成します。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public ET M(string name, params ET[] parameters)
        {
            return M(this, name, parameters);
        }



        /// <summary>
        /// フィールドへの参照を作成します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="op"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private static ET BinaryOperator(ET l, CodeBinaryOperatorType op, ET r)
        {
            return new BuilderBinaryOperator(l, op, r);
        }

        /// <summary>
        /// 足し算。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator +(ET l, ET r)
        {
            return BinaryOperator(l, CodeBinaryOperatorType.Add, r);
        }

        /// <summary>
        /// 引き算。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator -(ET l, ET r)
        {
            return BinaryOperator(l, CodeBinaryOperatorType.Subtract, r);
        }

        /// <summary>
        /// 掛け算。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator *(ET l, ET r)
        {
            return BinaryOperator(l, CodeBinaryOperatorType.Multiply, r);
        }

        /// <summary>
        /// 割り算。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator /(ET l, ET r)
        {
            return BinaryOperator(l, CodeBinaryOperatorType.Divide, r);
        }

        /// <summary>
        /// あまり。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator %(ET l, ET r)
        {
            return BinaryOperator(l, CodeBinaryOperatorType.Modulus, r);
        }

        /// <summary>
        /// 論理 OR。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator |(ET l, ET r)
        {
            return BinaryOperator(l, CodeBinaryOperatorType.BitwiseOr, r);
        }

        /// <summary>
        /// 論理 AND。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator &(ET l, ET r)
        {
            return BinaryOperator(l, CodeBinaryOperatorType.BitwiseAnd, r);
        }

        /// <summary>
        /// 条件 OR。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="rights"></param>
        /// <returns></returns>
        public static ET Or(ET left, params ET[] rights)
        {
            return EnumOp(CodeBinaryOperatorType.BooleanOr, left, rights);
        }

        private static ET EnumOp(CodeBinaryOperatorType opType, ET left, params ET[] rights)
        {
            if (rights.Length == 0) return left;
            return EnumOpImpl(opType, left, rights, 0);
        }
        private static ET EnumOpImpl(CodeBinaryOperatorType opType, ET l, ET[] rights, int index)
        {
            ET r;
            if (index == rights.Length - 1) r = rights[index];
            else r = EnumOpImpl(opType, rights[index], rights, index + 1);
            return new BuilderBinaryOperator(l, opType, r);
        }

        /// <summary>
        /// 条件 OR。
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public ET Or(params ET[] r)
        {
            return Or(this, r);
        }

        /// <summary>
        /// 条件 AND。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="rights"></param>
        /// <returns></returns>
        public static ET And(ET left, params ET[] rights)
        {
            return EnumOp(CodeBinaryOperatorType.BooleanAnd, left, rights);
        }

        /// <summary>
        /// And式を返します。
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public ET And(params ET[] r)
        {
            return And(this, r);
        }

        /// <summary>
        /// 値が等しい。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator ==(ET l, ET r)
        {
            return BinaryOperator(l, CodeBinaryOperatorType.ValueEquality, r);
        }

        /// <summary>
        /// 同じ式であればtrueを返します。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            ET other = obj as ET;
            object o = other;
            if (o == null) return false;
            return this.Expression == other.Expression;
        }

        /// <summary>
        /// ハッシュ値を返します。
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Expression.GetHashCode();
        }

        /// <summary>
        /// 値の否定
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static ET operator !(ET target)
        {
            return target == false;
        }

        /// <summary>
        /// 値が異なる
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator !=(ET l, ET r)
        {
            return !(l == r);
        }

        /// <summary>
        /// 超過。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator >(ET l, ET r)
        {
            return BinaryOperator(l, CodeBinaryOperatorType.GreaterThan, r);
        }

        /// <summary>
        /// 以上。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator >=(ET l, ET r)
        {
            return BinaryOperator(l, CodeBinaryOperatorType.GreaterThanOrEqual, r);
        }

        /// <summary>
        /// 未満。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator <(ET l, ET r)
        {
            return BinaryOperator(l, CodeBinaryOperatorType.LessThan, r);
        }

        /// <summary>
        /// 以下。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator <=(ET l, ET r)
        {
            return BinaryOperator(l, CodeBinaryOperatorType.LessThanOrEqual, r);
        }

        /// <summary>
        /// マイナス
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static ET operator -(ET target)
        {
            return 0 - target;
        }

        #region 片側がobject型の場合、値として扱う

        /// <summary>
        /// オブジェクトを値としてCodeDomに変換するビルダーを返します。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ET V(object value)
        {
            return new BuilderPrimitive(value);
        }


        /// <summary>
        /// +演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator +(ET l, object r) { return l + V(r); }

        /// <summary>
        /// -演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator -(ET l, object r) { return l - V(r); }

        /// <summary>
        /// *演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator *(ET l, object r) { return l * V(r); }

        /// <summary>
        /// /演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator /(ET l, object r) { return l / V(r); }

        /// <summary>
        /// %演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator %(ET l, object r) { return l % V(r); }

        /// <summary>
        /// |演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator |(ET l, object r) { return l | V(r); }

        /// <summary>
        /// ＆演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator &(ET l, object r) { return l & V(r); }

        /// <summary>
        /// ==演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator ==(ET l, object r) { return l == V(r); }

        /// <summary>
        /// !=演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator !=(ET l, object r) { return l != V(r); }

        /// <summary>
        /// ＜演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator <(ET l, object r) { return l < V(r); }

        /// <summary>
        /// ≦演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator <=(ET l, object r) { return l <= V(r); }

        /// <summary>
        /// ＞演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator >(ET l, object r) { return l > V(r); }

        /// <summary>
        /// ≧演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator >=(ET l, object r) { return l >= V(r); }


        /// <summary>
        /// +演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator +(object l, ET r) { return V(l) + r; }

        /// <summary>
        /// -演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator -(object l, ET r) { return V(l) - r; }

        /// <summary>
        /// *演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator *(object l, ET r) { return V(l) * r; }

        /// <summary>
        /// /演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator /(object l, ET r) { return V(l) / r; }

        /// <summary>
        /// %演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator %(object l, ET r) { return V(l) % r; }

        /// <summary>
        /// |演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator |(object l, ET r) { return V(l) | r; }

        /// <summary>
        /// ＆演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator &(object l, ET r) { return V(l) & r; }

        /// <summary>
        /// ==演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator ==(object l, ET r) { return V(l) == r; }

        /// <summary>
        /// !=演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator !=(object l, ET r) { return V(l) != r; }

        /// <summary>
        /// ＜演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator <(object l, ET r) { return V(l) < r; }

        /// <summary>
        /// ≦演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator <=(object l, ET r) { return V(l) <= r; }

        /// <summary>
        /// ＞演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator >(object l, ET r) { return V(l) > r; }

        /// <summary>
        /// ≧演算を変換します。
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ET operator >=(object l, ET r) { return V(l) >= r; }

        #endregion


        /// <summary>
        /// 配列参照を作成します。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="indexer"></param>
        /// <returns></returns>
        public static ET ArrayIndex(ET target, params ET[] indexer)
        {
            return new BuilderArrayIndex(target, indexer);
        }




        /// <summary>
        /// 配列参照を作成します。
        /// </summary>
        /// <param name="indexer"></param>
        /// <returns></returns>
        public ET ArrayIndex(params ET[] indexer)
        {
            return ArrayIndex(this, indexer);
        }




        #endregion
    }
}
