using MyMiniOrm.Expressions;
using MyMiniOrm.Reflections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MyMiniOrm
{
    public class ConditionResolver
    {
        // 查询参数
        private readonly List<KeyValuePair<string, object>> _parameters = new List<KeyValuePair<string, object>>();

        // 要关联的属性
        private readonly List<string> _joinProperties = new List<string>();

        // 查询语句
        private readonly Stack<string> _stringStack = new Stack<string>();

        // 主表信息
        private readonly MyEntity _master;

        // 参数前缀
        private readonly string _prefix;
        
        // 参数序号，用于生成SqlParameter的Name
        private int _parameterIndex;

        public ConditionResolver(MyEntity entity, string prefix = "@")
        {
            _master = entity;
            _prefix = prefix;
        }

        public string GetCondition()
        {
            var condition = string.Concat(_stringStack.ToArray());
            _stringStack.Clear();
            return condition;
        }

        public List<KeyValuePair<string, object>> GetParameters()
        {
            return _parameters;
        }

        public List<string> GetJoinPropertyList()
        {
            return _joinProperties;
        }

        public void Resolve(Expression node)
        {
            if (node.NodeType == ExpressionType.AndAlso ||
                node.NodeType == ExpressionType.OrElse)
            {
                var expression = (BinaryExpression) node;
                var right = expression.Right;
                var left = expression.Left;
                
                var rightString = ResolveExpression(right);
                var op = node.NodeType.ToSqlOperator();

                _stringStack.Push(")");
                _stringStack.Push(rightString);
                _stringStack.Push(op);
                Resolve(left);
                _stringStack.Push("(");
            }
            else
            {
                _stringStack.Push(ResolveExpression(node));
            }
        }

        public string ResolveExpression(Expression node, bool isClause = true)
        {
            switch (node.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                {
                    var expression = (BinaryExpression) node;
                    var right = ResolveExpression(expression.Right, false);
                    var op = node.NodeType.ToSqlOperator();
                    var left = ResolveExpression(expression.Left, false);

                    return $"({left} {op} {right})";
                }
                case ExpressionType.MemberAccess:
                {
                    // 参数属性、变量属性等
                    var expression = (MemberExpression) node;
                    var rootType = expression.RootExpressionType(out var stack);
                    if (rootType == ExpressionType.Parameter)
                    {
                        // s.IsActive
                        return isClause ? $"{ResolveStackToField(stack)}=1" : $"{ResolveStackToField(stack)}";
                    }
                    else
                    {
                        var val = node.GetValue();
                        if (isClause)   // var isActive=true; s => isActive
                            {
                            if (val is bool b)
                            {
                                return b ? "1=1" : "1=0";
                            }
                        }
                        var parameterName = GetParameterName();
                        _parameters.Add(new KeyValuePair<string, object>(parameterName, val));
                        return parameterName;
                    }
                }
                case ExpressionType.Call:
                {
                    // 方法调用
                    var expression = (MethodCallExpression) node;
                    var method = expression.Method.Name;

                    if (expression.Object != null &&
                        expression.Object.NodeType == ExpressionType.MemberAccess)
                    {
                        var rootType = ((MemberExpression) expression.Object).RootExpressionType(out var stack);
                        if (rootType == ExpressionType.Parameter)
                        {
                            var value = expression.Arguments[0].GetValue();
                            switch (method)
                            {
                                case "Contains":
                                    value = $"%{value}%";
                                    break;
                                case "StartsWith":
                                    value = $"{value}%";
                                    break;
                                case "EndsWith":
                                    value = $"%{value}";
                                    break;
                            }

                            var parameterName = GetParameterName();
                            _parameters.Add(new KeyValuePair<string, object>(parameterName, value));
                            return $"{ResolveStackToField(stack)} LIKE {parameterName}";
                        }
                    }
                    else
                    {
                        var value = node.GetValue();
                        if (isClause)
                        {
                            if (value is bool b)
                            {
                                return b ? "1=1" : "1=0";
                            }
                        }
                        var parameterName = GetParameterName();
                        _parameters.Add(new KeyValuePair<string, object>(parameterName, value));
                        return $"{parameterName}";
                    }

                    break;
                }
                case ExpressionType.Not:
                {
                    var expression = ((UnaryExpression) node).Operand;
                    if (expression.NodeType == ExpressionType.MemberAccess)
                    {
                        var rootType = ((MemberExpression) expression).RootExpressionType(out var stack);
                        if (rootType == ExpressionType.Parameter)
                        {
                            return $"{ResolveStackToField(stack)}=0";
                        }
                    }

                    break;
                }
                // 常量、本地变量
                case ExpressionType.Constant when !isClause:
                {
                    var val = node.GetValue();
                    var parameterName = GetParameterName();
                    _parameters.Add(new KeyValuePair<string, object>(parameterName, val));
                    return parameterName;
                }
                case ExpressionType.Constant:
                {
                    var expression = (ConstantExpression)node;
                    var value = expression.Value;
                    return value is bool b ? b ? "1=1" : "1=0" : string.Empty;
                }
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                {
                    // 二元操作符，等于、不等于、大于、小于等
                    var expression = (BinaryExpression) node;
                    var right = expression.Right;
                    var left = expression.Left;
                    var op = expression.NodeType.ToSqlOperator();

                    if (op == "=" || op == "<>")
                    {
                        if (right.NodeType == ExpressionType.Constant && right.GetValue() == null)
                        {
                            return op == "="
                                ? $"{ResolveExpression(left, false)} IS NULL"
                                : $"{ResolveExpression(left, false)} IS NOT NULL";
                        }
                    }

                    return $"{ResolveExpression(left, false)} {op} {ResolveExpression(right, false)}";
                }
                default:
                {
                    if (isClause)
                    {
                        var value = node.GetValue();
                        return value is bool b ? b ? "1=1" : "1=0" : string.Empty;
                    }
                    else
                    {
                        var val = node.GetValue();
                        var parameterName = GetParameterName();
                        _parameters.Add(new KeyValuePair<string, object>(parameterName, val));
                        return parameterName;
                    }
                }
            }

            return string.Empty;
        }
        
        private string ResolveStackToField(Stack<string> parameterStack)
        {
            switch (parameterStack.Count)
            {
                case 2:
                {
                    // 调用了导航属性
                    var propertyName = parameterStack.Pop();
                    var propertyFieldName = parameterStack.Pop();

                    _joinProperties.Add(propertyName);

                    if (!_joinProperties.Contains(propertyName))
                    {
                        _joinProperties.Add(propertyName);
                    }

                    var prop = _master.Properties.Single(p => p.Name == propertyName);
                    var propertyEntity = MyEntityContainer.Get(prop.PropertyInfo.PropertyType);
                    var propertyProperty = propertyEntity.Properties.Single(p => p.Name == propertyFieldName);

                    return $"[{propertyName}].[{propertyProperty.FieldName}]";
                }
                case 1:
                {
                    var propertyName = parameterStack.Pop();
                    var propInfo = _master.Properties.Single(p => p.Name == propertyName);
                    return $"[{_master.TableName}].[{propInfo.FieldName}]";
                }
                default:
                    throw new ArgumentException("尚未支持大于2层属性调用。如 student.Clazz.School.Id>10，请使用类似 student.Clazz.SchoolId > 0 替代");
            }
        }

        private string GetParameterName()
        {
            return $"{_prefix}__p_{_parameterIndex++}";
        }
    }
}
