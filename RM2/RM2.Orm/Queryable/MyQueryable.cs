using RM2.Orm.Expressions;
using RM2.Orm.Reflections;
using RM2.Orm.SqlBuilders;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace RM2.Orm.Queryable
{
    public class MyQueryable<T> where T : class , new ()
    {
        private readonly string _connectionString;

        // 要查询的导航属性
        private readonly List<string> _includeProperties = new List<string>();

        // Where子句中包含导航属性
        private List<string> _whereProperties = new List<string>();

        // 导航属性的缓存
        private readonly List<MyEntity> _entityCache = new List<MyEntity>();

        // 主表信息
        private readonly MyEntity _masterEntity;

        // 查询需要的参数
        private List<KeyValuePair<string, object>> _whereParameters = new List<KeyValuePair<string, object>>();

        // 是否已经调用过Where方法
        private bool _hasInitWhere;

        // 拼接好的where子句
        private string _where;

        // 拼接好的order by子句
        private string _orderBy;

        // 构造方法
        public MyQueryable(string connectionString)
        {
            _masterEntity = MyEntityContainer.Get(typeof(T));
            _connectionString = connectionString;
        }

        #region Include
        public MyQueryable<T> Include<TProperty>(Expression<Func<T, TProperty>> expression)
            where TProperty : class, new()
        {
            if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpr = (MemberExpression)expression.Body;
                if (memberExpr.Expression != null &&
                    memberExpr.Expression.NodeType == ExpressionType.Parameter &&
                    memberExpr.Member.GetType().IsClass)
                {
                    if (_includeProperties.All(p => p != memberExpr.Member.Name))
                    {
                        _includeProperties.Add(memberExpr.Member.Name);
                    }
                }
            }

            return this;
        }
        #endregion

        #region Where
        public MyQueryable<T> Where(Expression<Func<T, bool>> expr)
        {
            if (_hasInitWhere)
            {
                throw new ArgumentException("每个查询只能调用一次Where方法");
            }
            _hasInitWhere = true;

            var whereExpressionVisitor = new WhereExpressionVisitor<T>(_masterEntity);
            whereExpressionVisitor.Visit(expr);
            _where = whereExpressionVisitor.GetCondition();
            _whereParameters = whereExpressionVisitor.GetParameters();
            _whereProperties = whereExpressionVisitor.GetJoinPropertyList();

            return this;
        }
        #endregion

        #region OrderBy,ThenOrderBy
        public MyQueryable<T> OrderBy<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                _orderBy = GetOrderByString((MemberExpression)expression.Body);
            }

            return this;
        }

        public MyQueryable<T> OrderByDesc<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                var orderBy = GetOrderByString((MemberExpression)expression.Body);
                if (!string.IsNullOrWhiteSpace(orderBy))
                {
                    _orderBy = orderBy + " DESC";
                }
            }

            return this;
        }

        public MyQueryable<T> ThenOrderBy<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            if (string.IsNullOrWhiteSpace(_orderBy))
            {
                throw new ArgumentNullException(nameof(_orderBy), "排序字段为空，必须先调用OrderBy或OrderByDesc才能调用此方法");
            }
            if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                _orderBy += "," + GetOrderByString((MemberExpression)expression.Body);
            }

            return this;
        }

        public MyQueryable<T> ThenOrderByDesc<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                _orderBy += "," + GetOrderByString((MemberExpression)expression.Body) + " DESC";
            }

            return this;
        }
        #endregion

        #region 输出
        public List<T> ToList()
        {
            var fields = GetFields();
            var from = GetFrom();

            var sqlBuilder = new SqlServerSqlBuilder();
            var sql = sqlBuilder.Select(from, fields, _where, _orderBy);

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(_whereParameters.ToSqlParameters().ToArray());
                var sdr = command.ExecuteReader();

                var handler = new SqlDataReaderConverter<T>(_includeProperties.ToArray());
                return handler.ConvertToEntityList(sdr);
            }
        }

        public List<T> ToPageList(int pageIndex, int pageSize, out int recordCount)
        {
            var fields = GetFields();
            var from = GetFrom();
            recordCount = 0;

            var sqlBuilder = new SqlServerSqlBuilder();
            var sql = sqlBuilder.GetPagingQuerySql(fields, from, _where, _orderBy, pageIndex, pageSize);

            var command = new SqlCommand(sql);
            command.Parameters.AddRange(_whereParameters.ToSqlParameters().ToArray());
            var param = new SqlParameter("@RecordCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
            command.Parameters.Add(param);

            List<T> result;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                command.Connection = conn;
                using (var sdr = command.ExecuteReader())
                {
                    var handler = new SqlDataReaderConverter<T>(_includeProperties.ToArray());
                    result = handler.ConvertToEntityList(sdr);
                }
            }

            recordCount = (int)param.Value;
            return result;
        }

        public T FirstOrDefault()
        {
            var fields = GetFields();
            var from = GetFrom();

            var sqlBuilder = new SqlServerSqlBuilder();
            var sql = sqlBuilder.Select(from, fields, _where, _orderBy, 1);

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(_whereParameters.ToSqlParameters().ToArray());
                var sdr = command.ExecuteReader();

                var handler = new SqlDataReaderConverter<T>(_includeProperties.ToArray());
                return handler.ConvertToEntity2(sdr);
            }
        }
        #endregion

        #region 辅助方法

        /// 把要用到的导航属性的MyEntity缓存到一个List里，不需要每次都要到字典中获取
        private MyEntity GetIncludePropertyEntityInfo(Type type)
        {
            var entity = _entityCache.FirstOrDefault(e => e.Name == type.FullName);

            if (entity != null) return entity;

            entity = MyEntityContainer.Get(type);
            _entityCache.Add(entity);
            return entity;
        }

        // 获取Select子句
        private string GetFields()
        {
            var masterFields = string.Join(
                ",",
                _masterEntity
                    .Properties
                    .Where(p => p.IsMap)
                    .Select(p => $"[{_masterEntity.TableName}].[{p.FieldName}] AS [{p.Name}]")
            );

            if (_includeProperties.Count > 0)
            {
                var sb = new StringBuilder(masterFields);
                sb.Append(",");
                var includeProperties = _includeProperties.OrderBy(i => i);

                foreach (var property in includeProperties)
                {
                    var prop = _masterEntity.Properties.Single(p => p.Name == property);
                    var propEntity = GetIncludePropertyEntityInfo(prop.PropertyInfo.PropertyType);
                    sb.Append(
                        string.Join(",",
                            propEntity.Properties.Where(p => p.IsMap).Select(p =>
                                $"[{propEntity.TableName}].[{p.FieldName}] AS [{property}_{p.Name}]"))
                    );
                }

                return sb.ToString();
            }

            return masterFields;
        }

        // 获取From子句
        public string GetFrom()
        {
            var masterTable = $"[{_masterEntity.TableName}]";
            var allJoinProperties = _includeProperties.Concat(_whereProperties).Distinct().ToList();

            if (allJoinProperties.Any())
            {
                var sb = new StringBuilder(masterTable);
                foreach (var property in allJoinProperties)
                {
                    var prop = _masterEntity.Properties.SingleOrDefault(p => p.Name == property);
                    if (prop != null)
                    {
                        var propEntity = GetIncludePropertyEntityInfo(prop.PropertyInfo.PropertyType);
                        sb.Append($" LEFT JOIN [{propEntity.TableName}] AS [{property}] ON [{_masterEntity.TableName}].[{prop.ForeignKey}]=[{propEntity.TableName}].[{propEntity.KeyColumn}]");
                    }
                }

                return sb.ToString();
            }

            return masterTable;
        }

        // 获取OrderBy子句
        private string GetOrderByString(MemberExpression expression)
        {
            expression.RootExpressionType(out var stack);
            if (stack.Count == 1)
            {
                var propName = stack.Pop();
                var prop = _masterEntity.Properties.Single(p => p.Name == propName);
                return $"[{_masterEntity.TableName}].[{prop.FieldName}]";
            }

            if (stack.Count == 2)
            {
                var slavePropName = stack.Pop();
                var property = stack.Pop();

                var masterProp = _masterEntity.Properties.Single(p => p.Name == property);
                var slaveEntity = GetIncludePropertyEntityInfo(masterProp.PropertyInfo.PropertyType);
                var slaveProperty = slaveEntity.Properties.Single(p => p.Name == slavePropName);

                return $"[{masterProp.Name}].[{slaveProperty.FieldName}]";
            }

            return string.Empty;
        }

        #endregion
    }
}
