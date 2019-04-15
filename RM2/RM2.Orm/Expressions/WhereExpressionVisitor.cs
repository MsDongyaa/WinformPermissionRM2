using MyMiniOrm.Reflections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MyMiniOrm.Expressions
{
    public class WhereExpressionVisitor<T> : ExpressionVisitor
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

        // 临时全局变量-记录CallExpression中调用的方法名称
        private string _tempMethod;

        // 参数序号，用于生成SqlParameter的Name
        private int _parameterIndex;

        private bool isBinary = false;

        #region 构造函数
        public WhereExpressionVisitor(string prefix = "@")
        {
            _master = MyEntityContainer.Get(typeof(T));
            _prefix = prefix;
        }

        public WhereExpressionVisitor(MyEntity entity, string prefix = "@")
        {
            _master = entity;
            _prefix = prefix;
        }
        #endregion

        #region 返回条件语句、查询参数等
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
        #endregion

        #region 遍历表达式目录树
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            isBinary = true;

            _stringStack.Push(")");

            Visit(node.Right);

            _stringStack.Push(node.NodeType.ToSqlOperator());

            Visit(node.Left);

            _stringStack.Push("(");

            isBinary = false;

            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Not && node.Operand.NodeType == ExpressionType.MemberAccess)
            {
                var rootType = ((MemberExpression)node.Operand).RootExpressionType(out var parameterStack);

                if (rootType == ExpressionType.Parameter)
                {
                    ResolveStackToField(parameterStack);
                }
                else
                {
                    var obj = ResolveValue(node.GetValue());
                    var parameterName = $"{_prefix}__p_{_parameterIndex++}";
                    _parameters.Add(new KeyValuePair<string, object>(parameterName, obj));
                    _stringStack.Push($" {parameterName} ");
                }
            }
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            var rootType = node.RootExpressionType(out var parameterStack);

            if (rootType == ExpressionType.Parameter)
            {
                ResolveStackToField(parameterStack);
            }
            else
            {
                var obj = ResolveValue(node.GetValue());
                var parameterName = $"{_prefix}__p_{_parameterIndex++}";
                _parameters.Add(new KeyValuePair<string, object>(parameterName, obj));
                _stringStack.Push($" {parameterName} ");
            }
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            var parameterName = $"{_prefix}__p_{_parameterIndex++}";
            var value = ResolveValue(node.Value);
            
            _parameters.Add(new KeyValuePair<string, object>(parameterName, value));
            _stringStack.Push($" {parameterName} ");
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
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
                    format = "({0} LIKE {1})";
                }
                else if(node.Method.Name == "IgnoreExpression")
                {
                    return node;
                }
                else
                {
                    throw new NotSupportedException($"不受支持的方法调用 {node.Method.Name}");
                }
                // 解析的时候需要在其他方法内根据方法名拼接字符串，
                // 所以在这里需要一个全局变量保存方法名
                _tempMethod = node.Method.Name;

                Visit(node.Object);
                Visit(node.Arguments[0]);

                var right = _stringStack.Pop();
                var left = _stringStack.Pop();
                _stringStack.Push(string.Format(format, left, right));
            }
            else if (node.Method.Name == "DefaultTrue")
            {
                _stringStack.Push("1=1");
            }
            else if (node.Method.Name == "DefaultFalse")
            {
                _stringStack.Push("1=0");
            }
            else
            {
                var obj = ResolveValue(node.GetValue());

                var parameterName = $"{_prefix}__p_{_parameterIndex++}";
                _parameters.Add(new KeyValuePair<string, object>(parameterName, obj));
                _stringStack.Push($" {parameterName} ");
            }

            return node;
        }
        #endregion

        #region 辅助方法
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

        private void ResolveStackToField(Stack<string> parameterStack)
        {
            if (parameterStack.Count == 2)
            {
                // 调用了导航属性
                var propertyName = parameterStack.Pop();
                var propertyFieldName = parameterStack.Pop();

                _joinProperties.Add(propertyName);

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
        #endregion
    }
}
