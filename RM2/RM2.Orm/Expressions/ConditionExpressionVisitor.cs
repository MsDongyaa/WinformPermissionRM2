using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MyMiniOrm.Reflections;

namespace MyMiniOrm.Expressions
{
    public class ConditionExpressionVisitor : ExpressionVisitor
    {
        private readonly Queue<ConditionClause> _queue = new Queue<ConditionClause>();

        public Queue<ConditionClause> GetStack()
        {
            return _queue;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.AndAlso ||
                node.NodeType == ExpressionType.OrElse)
            {
                _queue.Enqueue(new ConditionClause() { Type = node.NodeType, Expression = node.Right });
            }
            else
            {
                _queue.Enqueue(new ConditionClause() { Type = null, Expression = node });
            }

            Visit(node.Left);
            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            _queue.Enqueue(new ConditionClause() { Type = null, Expression = node });
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _queue.Enqueue(new ConditionClause() { Type = null, Expression = node });
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _queue.Enqueue(new ConditionClause() { Type = null, Expression = node });
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            _queue.Enqueue(new ConditionClause() { Type = null, Expression = node });
            return node;
        }
    }

    public class ConditionClause
    {
        public ExpressionType? Type { get; set; }

        public Expression Expression { get; set; }
    }

    public class ExpressionConverter
    {
        private readonly MyEntity _master;

        public List<KeyValuePair<string, object>> _parameters = new List<KeyValuePair<string, object>>();

        public List<string> JoinPropertyList = new List<string>();

        private readonly Stack<string> _stringStack = new Stack<string>();

        private readonly string _prefix;

        private int _parameterIndex;

        private string _tempMethod;

        public ExpressionConverter(MyEntity master, string prefix = "@")
        {
            _master = master;
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

        public void Resolve(Queue<ConditionClause> clauses)
        {
            _stringStack.Push(")");
            var count = clauses.Count;
            for (int i = 0; i < count; i++)
            {
                var clause = clauses.Dequeue();
                var expression = clause.Expression;
                if (expression.NodeType == ExpressionType.Constant)
                {
                    // 如果表达式为常量表达式，如 true，false
                    var constant = (ConstantExpression) expression;
                    var value = constant.Value;
                    if (value is true)
                    {
                        _stringStack.Push("1=1");
                        ResolveConditionType(clause.Type);
                    }
                    else if (value is false)
                    {
                        _stringStack.Push("1=0");
                        ResolveConditionType(clause.Type);
                    }
                }
                else if (expression.NodeType == ExpressionType.MemberAccess)
                {
                    var memberExpression = (MemberExpression) expression;
                    if (memberExpression.RootExpressionType() == ExpressionType.Parameter)
                    {
                        var fieldName = ConvertMemberToString(memberExpression);
                        _stringStack.Push($"{fieldName} = 1");
                        ResolveConditionType(clause.Type);
                    }
                }
                else if (expression.NodeType == ExpressionType.Not)
                {
                    var notExpression = (UnaryExpression) expression;
                    var operand = notExpression.Operand;
                    if (operand.NodeType == ExpressionType.MemberAccess && 
                        ((MemberExpression)operand).RootExpressionType() == ExpressionType.Parameter)
                    {
                        // 如果操作的是参数的属性，如!s.IsDel这种形式，那么拼接sql为[表名].[字段名]=0；
                        _stringStack.Push("=0");
                        _stringStack.Push(ConvertMemberToString((MemberExpression)operand));

                        ResolveConditionType(clause.Type);
                    }

                    // 如果操作的不是参数的属性，如本地变量 !isActive 这种形式，那么将表达式取值，然后拼接为 1=1 或 1=0
                    var value = notExpression.GetValue();
                    if (value is true)
                    {
                        _stringStack.Push("1=1");
                        ResolveConditionType(clause.Type);
                    }
                    else if (value is false)
                    {
                        _stringStack.Push("1=0");
                        ResolveConditionType(clause.Type);
                    }
                }
                else if (expression.NodeType == ExpressionType.Call)
                {
                    // 只支持参数类型的方法调用，如s.Name.Contains("关键字")
                    var callExpression = (MethodCallExpression)expression;
                    if (callExpression.Object != null &&
                        callExpression.Object.NodeType == ExpressionType.MemberAccess &&
                        ((MemberExpression) callExpression.Object).RootExpressionType() == ExpressionType.Parameter)
                    {
                        if(callExpression.Method.Name == "Contains" ||
                           callExpression.Method.Name == "StartsWith" ||
                           callExpression.Method.Name == "EndsWith")
                        { 
                            var parameterName = GetParameterName();
                            var fieldName = ConvertMemberToString(((MemberExpression) callExpression.Object));
                            _stringStack.Push(")");
                            _stringStack.Push(parameterName);
                            _stringStack.Push(" LIKE ");
                            _stringStack.Push(fieldName);
                            _stringStack.Push("(");
                            ResolveConditionType(clause.Type);

                            var value = callExpression.Arguments[0].GetValue();
                            switch (callExpression.Method.Name)
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
                            _parameters.Add(new KeyValuePair<string, object>(parameterName, value));
                        }
                    }
                }
                else if (expression.NodeType == ExpressionType.Equal ||
                         expression.NodeType == ExpressionType.NotEqual ||
                         expression.NodeType == ExpressionType.GreaterThan ||
                         expression.NodeType == ExpressionType.GreaterThanOrEqual ||
                         expression.NodeType == ExpressionType.LessThan ||
                         expression.NodeType == ExpressionType.LessThanOrEqual)
                {
                    var binaryExpression = (BinaryExpression) clause.Expression;
                    var left = binaryExpression.Left;
                    if (left.NodeType == ExpressionType.MemberAccess)
                    {
                        var fieldName = ConvertMemberToString((MemberExpression)left);
                        var op = binaryExpression.NodeType.ToSqlOperator();
                        var value = binaryExpression.Right.GetValue();

                        var parameterName = GetParameterName();
                        _stringStack.Push($"({fieldName} {op} {parameterName})");
                        ResolveConditionType(clause.Type);
                        _parameters.Add(new KeyValuePair<string, object>(parameterName, value));
                    }
                }
            }
            _stringStack.Push("(");
        }

        public void ResolveMemberAccessExpression(MemberExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var rootType = expression.RootExpressionType(out var parameterStack);

            if (rootType == ExpressionType.Parameter)
            {
                if (parameterStack.Count == 2)
                {
                    // 调用了导航属性
                    var propertyName = parameterStack.Pop();
                    var propertyFieldName = parameterStack.Pop();

                    JoinPropertyList.Add(propertyName);

                    var prop = _master.Properties.Single(p => p.Name == propertyName);
                    var propertyEntity = MyEntityContainer.Get(prop.PropertyInfo.PropertyType);
                    var propertyProperty = propertyEntity.Properties.Single(p => p.Name == propertyFieldName);

                    _stringStack.Push($"[{propertyName}].[{propertyProperty.FieldName}]");
                }
                else if (parameterStack.Count == 1)
                {
                    var propertyName = parameterStack.Pop();
                    var propInfo = _master.Properties.Single(p => p.Name == propertyName);
                    _stringStack.Push($"[{_master.TableName}].[{propInfo.FieldName}]");
                }
                else
                {
                    throw new ArgumentException("尚未支持大于2层属性调用。如 student.Clazz.School.Id>10，请使用类似 student.Clazz.SchoolId > 0 替代");
                }
            }
            else
            {
                var obj = ResolveValue(expression.GetValue());
                var parameterName = $"{_prefix}__p_{_parameterIndex++}";
                _parameters.Add(new KeyValuePair<string, object>(parameterName, obj));
                _stringStack.Push($" {parameterName} ");
            }
        }

        public void ResolveConstantExpression(ConstantExpression node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            var parameterName = $"{_prefix}__p_{_parameterIndex++}";
            var value = ResolveValue(node.Value);
            if (value.ToString() == "1=1" || value.ToString() == "1=0")
            {
                _stringStack.Push($" {value} ");
            }
            else
            {
                _parameters.Add(new KeyValuePair<string, object>(parameterName, value));
                _stringStack.Push($" {parameterName} ");
            }
        }

        public void ResolveMethodCallExpression(MethodCallExpression node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            if (node.Object != null &&
                node.Object.NodeType == ExpressionType.MemberAccess &&
                ((MemberExpression)node.Object).RootExpressionType() == ExpressionType.Parameter)
            {
                string format;

                if (node.Method.Name == "StartsWith" ||
                    node.Method.Name == "Contains" ||
                    node.Method.Name == "EndsWith")
                {
                    format = "{0} LIKE {1}";
                }
                else
                {
                    throw new NotSupportedException($"不受支持的方法调用 {node.Method.Name}");
                }
                // 解析的时候需要在其他方法内根据方法名拼接字符串，
                // 所以在这里需要一个全局变量保存方法名
                _tempMethod = node.Method.Name;
                
                ResolveMemberAccessExpression((MemberExpression)node.Object);
                ResolveExpression(node.Arguments[0]);

                var right = _stringStack.Pop();
                var left = _stringStack.Pop();
                _stringStack.Push(string.Format(format, left, right));
            }
            else
            {
                var obj = ResolveValue(node.GetValue());

                var parameterName = $"{_prefix}__p_{_parameterIndex++}";
                _parameters.Add(new KeyValuePair<string, object>(parameterName, obj));
                _stringStack.Push($" {parameterName} ");
            }
        }

        public void ResolveUnaryExpression(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Not && node.Operand.NodeType == ExpressionType.MemberAccess)
            {
                var rootType = ((MemberExpression)node.Operand).RootExpressionType(out var parameterStack);

                if (rootType == ExpressionType.Parameter)
                {
                    if (parameterStack.Count == 2)
                    {
                        // 调用了导航属性
                        var propertyName = parameterStack.Pop();
                        var propertyFieldName = parameterStack.Pop();

                        JoinPropertyList.Add(propertyName);

                        var prop = _master.Properties.Single(p => p.Name == propertyName);
                        var propertyEntity = MyEntityContainer.Get(prop.PropertyInfo.PropertyType);
                        var propertyProperty = propertyEntity.Properties.Single(p => p.Name == propertyFieldName);

                        _stringStack.Push($"[{propertyName}].[{propertyProperty.FieldName}]=0");
                    }
                    else if (parameterStack.Count == 1)
                    {
                        var propertyName = parameterStack.Pop();
                        var propInfo = _master.Properties.Single(p => p.Name == propertyName);
                        _stringStack.Push($"[{_master.TableName}].[{propInfo.FieldName}]=0");
                    }
                    else
                    {
                        throw new ArgumentException("尚未支持大于2层属性调用。如 student.Clazz.School.Id>10，请使用类似 student.Clazz.SchoolId > 0 替代");
                    }
                }
                else
                {
                    var obj = ResolveValue(node.GetValue());
                    var parameterName = $"{_prefix}__p_{_parameterIndex++}";
                    _parameters.Add(new KeyValuePair<string, object>(parameterName, obj));
                    _stringStack.Push($" {parameterName} ");
                }
            }
        }

        private void ResolveExpression(Expression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.MemberAccess:
                    ResolveMemberAccessExpression((MemberExpression)node);
                    break;
                case ExpressionType.Constant:
                    ResolveConstantExpression((ConstantExpression)node);
                    break;
                case ExpressionType.Call:
                    ResolveMethodCallExpression((MethodCallExpression)node);
                    break;
                case ExpressionType.Not:
                    ResolveUnaryExpression((UnaryExpression)node);
                    break;
            }
        }

        private object ResolveValue(object obj)
        { 
            switch (_tempMethod)
            {
                case "Contains":
                    obj = $"%{obj}%";
                    break;
                case "StartsWith":
                    obj = $"{obj}%";
                    break;
                case "EndsWith":
                    obj = $"%{obj}";
                    break;
            }

            _tempMethod = "";
            return obj;
        }

        // 成员表达式
        public string ConvertMemberToString(MemberExpression node)
        {
            var rootType = node.RootExpressionType(out var stack);
            if (rootType == ExpressionType.Parameter)
            {
                return GetFieldFullName(stack);
            }

            return string.Empty;
        }

        public string GetFieldFullName(Stack<string> stack)
        {
            if (stack.Count == 1)
            {
                var fieldName = stack.Pop();
                var property = _master.Properties.Single(p => p.Name == fieldName);
                return $"[{_master.TableName}].[{property.FieldName}]";
            }
            else if (stack.Count == 2)
            {
                var propertyName = stack.Pop();
                var fieldName = stack.Pop();

                var property = _master.Properties.Single(p => p.Name == propertyName);
                var propertyEntity = MyEntityContainer.Get(property.PropertyInfo.PropertyType);
                var field = propertyEntity.Properties.Single(p => p.Name == fieldName);

                return $"[{propertyName}].[{field.FieldName}]";
            }
            return string.Empty;
        }

        private string GetParameterName()
        {
            return $"{_prefix}__p_{_parameterIndex++}";
        }

        private void ResolveBoolValueAsExpression(object value)
        {
            if (value is true)
            {
                _stringStack.Push("1=1");
            }
            else if (value is false)
            {
                _stringStack.Push("1=0");
            }
        }

        private void ResolveConditionType(ExpressionType? type)
        {
            if (type != null)
            {
                switch (type.Value)
                {
                    case ExpressionType.AndAlso:
                        _stringStack.Push(" AND ");
                        break;
                    case ExpressionType.OrElse:
                        _stringStack.Push(" OR ");
                        break;
                }
            }
        }
    }
}
