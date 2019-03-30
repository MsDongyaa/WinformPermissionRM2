using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RM2.Orm.Expressions
{
    public static class ExpressionExtensions
    {
        public static string ToSqlOperator(this ExpressionType type)
        {
            switch (type)
            {
                case (ExpressionType.AndAlso):
                case (ExpressionType.And):
                    return " AND ";
                case (ExpressionType.OrElse):
                case (ExpressionType.Or):
                    return " OR ";
                case (ExpressionType.Not):
                    return " NOT ";
                case (ExpressionType.NotEqual):
                    return "<>";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case (ExpressionType.Equal):
                    return "=";
                default:
                    throw new Exception("不支持该方法");
            }
        }

        public static bool IsDerivedFromParameter(this MemberExpression exp)
        {
            return IsDerivedFromParameter(exp, out var p);
        }

        public static bool IsDerivedFromParameter(this MemberExpression exp, out ParameterExpression p)
        {
            p = null;
            var prevExp = exp.Expression;
            var memberExp = prevExp as MemberExpression;
            while (memberExp != null)
            {
                prevExp = memberExp.Expression;
                memberExp = prevExp as MemberExpression;
            }

            if (prevExp == null)/* 静态属性访问 */
                return false;

            if (prevExp.NodeType == ExpressionType.Parameter)
            {
                p = (ParameterExpression)prevExp;
                return true;
            }

            /* 当实体继承于某个接口或类时，会有这种情况 */
            if (prevExp.NodeType == ExpressionType.Convert)
            {
                prevExp = ((UnaryExpression)prevExp).Operand;
                if (prevExp.NodeType == ExpressionType.Parameter)
                {
                    p = (ParameterExpression)prevExp;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取MemberExpress的根节点类型，并返回 各个级别的 Member 名称
        /// </summary>
        /// <param name="memberExpression"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static ExpressionType RootExpressionType(this MemberExpression memberExpression, out Stack<string> stack)
        {
            var memberExpr = memberExpression;
            var parentExpr = memberExpression.Expression;

            stack = new Stack<string>();
            stack.Push(memberExpression.Member.Name);

            while (parentExpr != null && parentExpr.NodeType == ExpressionType.MemberAccess)
            {
                memberExpr = (MemberExpression)parentExpr;
                parentExpr = ((MemberExpression)parentExpr).Expression;
                stack.Push(memberExpr.Member.Name);
            }

            return parentExpr?.NodeType ?? memberExpr.NodeType;
        }

        /// <summary>
        /// 获取MemberExpression的根节点类型
        /// </summary>
        /// <param name="memberExpression"></param>
        /// <returns></returns>
        public static ExpressionType RootExpressionType(this MemberExpression memberExpression)
        {
            var memberExpr = memberExpression;
            var parentExpr = memberExpression.Expression;

            while (parentExpr != null && parentExpr.NodeType == ExpressionType.MemberAccess)
            {
                memberExpr = (MemberExpression)parentExpr;
                parentExpr = ((MemberExpression)parentExpr).Expression;
            }

            return parentExpr?.NodeType ?? memberExpr.NodeType;
        }

        /// <summary>
        /// 执行Expression，获取值
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static object GetValue(this Expression expression)
        {
            if (expression.NodeType == ExpressionType.Constant)
            {
                return ((ConstantExpression)expression).Value;
            }
            else
            {
                var cast = Expression.Convert(expression, typeof(object));
                return Expression.Lambda<Func<object>>(cast).Compile().Invoke();
            }
        }
    }
}
