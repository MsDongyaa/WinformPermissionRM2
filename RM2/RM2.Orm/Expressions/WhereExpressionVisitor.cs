using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using RM2.Orm.Reflections;

namespace RM2.Orm.Expressions
{
    public class WhereExpressionVisitor<T> : ExpressionVisitor
    {
        private readonly List<KeyValuePair<string, object>> _parameters = new List<KeyValuePair<string, object>>();

        private readonly List<string> _joinTables = new List<string>();

        private readonly Stack<string> _stringStack = new Stack<string>();

        private readonly MyEntity _master;

        private readonly string _prefix;

        private string _tempMethod;

        private int _parameterIndex;

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

        public List<string> GetJoinTables()
        {
            return _joinTables;
        }

        /// <inheritdoc />
        /// <summary>
        /// 二元表达式
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            _stringStack.Push(")");
            //解析右边
            Visit(node.Right);

            _stringStack.Push(node.NodeType.ToSqlOperator());

            //解析左边
            Visit(node.Left);
            _stringStack.Push("(");

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            var rootType = node.RootExpressionType(out var parameterStack);

            if (rootType == ExpressionType.Parameter)
            {
                if (parameterStack.Count == 2)
                {
                    var propertyName = parameterStack.Pop();
                    var propertyFieldName = parameterStack.Pop();

                    _joinTables.Add(propertyName);

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
                var obj = ResolveValue(node.GetValue());
                var parameterName = $"{_prefix}__p_{_parameterIndex++}";
                _parameters.Add(new KeyValuePair<string, object>(parameterName, obj));
                _stringStack.Push($" {parameterName} ");
                _tempMethod = "";
            }
            return node;
        }

        /// <summary>
        /// 常量表达式
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            var parameterName = $"{_prefix}__p_{_parameterIndex++}";
            var value = node.Value;
            switch (_tempMethod)
            {
                case "Contains":
                    value = "%" + value + "%";
                    break;
                case "StartsWith":
                    value = value + "%";
                    break;
                case "EndsWith":
                    value = "%" + value;
                    break;
            }
            _parameters.Add(new KeyValuePair<string, object>(parameterName, value));
            _stringStack.Push($" {parameterName} ");
            _tempMethod = "";
            return node;
        }

        /// <summary>
        /// 方法表达式
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));

            if (m.Object != null &&
                m.Object.NodeType == ExpressionType.MemberAccess &&
                ((MemberExpression)m.Object).RootExpressionType() == ExpressionType.Parameter)
            {
                string format;

                if (m.Method.Name == "StartsWith" ||
                    m.Method.Name == "Contains" ||
                    m.Method.Name == "EndsWith")
                {
                    format = "({0} LIKE {1})";
                }
                else
                {
                    throw new NotSupportedException($"不受支持的方法调用 {m.Method.Name}");
                }
                // 解析的时候需要在其他方法内根据方法名拼接字符串，
                // 所以在这里需要一个全局变量保存方法名
                _tempMethod = m.Method.Name;

                Visit(m.Object);
                Visit(m.Arguments[0]);

                var right = _stringStack.Pop();
                var left = _stringStack.Pop();
                _stringStack.Push(string.Format(format, left, right));
            }
            else
            {
                var obj = m.GetValue();

                var parameterName = $"{_prefix}__p_{_parameterIndex++}";
                _parameters.Add(new KeyValuePair<string, object>(parameterName, obj));
                _stringStack.Push($" {parameterName} ");
            }

            return m;
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

            return obj;
        }
    }
}
