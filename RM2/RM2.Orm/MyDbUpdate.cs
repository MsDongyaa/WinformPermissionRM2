using MyMiniOrm.Commons;
using MyMiniOrm.Expressions;
using MyMiniOrm.Reflections;
using MyMiniOrm.SqlBuilders;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace MyMiniOrm
{
    public partial class MyDb
    {
        /// <summary>
        /// 通过Id修改指定列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">实体ID</param>
        /// <param name="kvs">属性和值的键值对。用法 DbKvs.New().Add("属性名", 值)</param>
        /// <returns>受影响的记录数</returns>
        public int Update<T>(int id, DbKvs kvs)
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var setProperties = kvs.Where(kv => kv.Key != "Id").Select(kv => kv.Key);
            var includeProperties = entityInfo.Properties.Where(p => setProperties.Contains(p.Name)).ToList();
            if (includeProperties.Count == 0)
            {
                return 0;
            }

            var sql =
                $"UPDATE [{entityInfo.TableName}] SET {string.Join(",", includeProperties.Select(p => $"{p.FieldName}=@{p.Name}"))} WHERE Id=@Id";
            var parameters = kvs.ToSqlParameters();
            parameters.Add(new SqlParameter("@Id", id));
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(parameters.ToArray());
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 通过查询条件修改指定列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="kvs">属性和值的键值对。用法 DbKvs.New().Add("属性名", 值)</param>
        /// <param name="expression">查询条件，注意：不支持导航属性，如 "student => student.School.Id > 0" 将无法解析</param>
        /// <returns>受影响的记录数</returns>
        public int Update<T>(DbKvs kvs, Expression<Func<T, bool>> expression = null)
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var setProperties = kvs.Where(kv => kv.Key != "Id").Select(kv => kv.Key);
            var includeProperties = entityInfo.Properties.Where(p => setProperties.Contains(p.Name)).ToList();
            if (includeProperties.Count == 0)
            {
                return 0;
            }

            string sql;
            List<SqlParameter> parameters;
            if (expression == null)
            {
                sql =
                    $"UPDATE [{entityInfo.TableName}] SET {string.Join(",", includeProperties.Select(p => $"{p.FieldName}=@{p.Name}"))} WHERE Id=@Id";
                parameters = kvs.ToSqlParameters();
            }
            else
            {
                //var whereExpressionVisitor = new WhereExpressionVisitor<T>();
                //whereExpressionVisitor.Visit(expression);
                //var where = whereExpressionVisitor.GetCondition();
                //var whereParameters = whereExpressionVisitor.GetParameters().ToSqlParameters();

                var resolver = new ConditionResolver(entityInfo);
                resolver.Resolve(expression.Body);
                var where = resolver.GetCondition();
                var whereParameters = resolver.GetParameters().ToSqlParameters();

                parameters = kvs.ToSqlParameters();
                parameters.AddRange(whereParameters);

                where = string.IsNullOrWhiteSpace(where) ? "1=1" : where;

                sql =
                    $"UPDATE [{entityInfo.TableName}] SET {string.Join(",", includeProperties.Select(p => $"{p.FieldName}=@{p.Name}"))} WHERE {where}";
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(parameters.ToArray());
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 修改实体的指定属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">要修改的实体</param>
        /// <param name="includes">要修改的属性名称，注意：是实体的属性名而不是数据库字段名</param>
        /// <param name="ignoreAttribute">是否忽略实体的UpdateIgnore描述。默认为true，既includes中包含的所有属性都会被修改</param>
        /// <returns>受影响的记录数</returns>
        public int UpdateInclude<T>(T entity, IEnumerable<string> includes, bool ignoreAttribute = true) where T : IEntity
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var includeProperties = entityInfo.Properties.Where(p => includes.Contains(p.Name) && p.Name != "Id").ToList();

            if (!ignoreAttribute)
            {
                includeProperties = includeProperties.Where(p => !p.UpdateIgnore).ToList();
            }

            if (includeProperties.Count == 0)
            {
                return 0;
            }

            var sql =
                $"UPDATE [{entityInfo.TableName}] SET {string.Join(",", includeProperties.Select(p => $"{p.FieldName}=@{p.Name}"))} WHERE Id=@Id";
            var parameters = new List<SqlParameter> { new SqlParameter("@Id", entity.Id) };

            foreach (var property in includeProperties)
            {
                parameters.Add(new SqlParameter($"@{property.Name}", ResolveParameterValue(property.PropertyInfo.GetValue(entity))));
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(parameters.ToArray());
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 修改实体的指定属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">要修改的实体</param>
        /// <param name="expression">要修改的属性，注意不支持导航属性及其子属性</param>
        /// <param name="ignoreAttribute">是否忽略实体的UpdateIgnore描述。默认为true，既includes中包含的所有属性都会被修改</param>
        /// <returns>受影响的记录数</returns>
        public int UpdateInclude<T>(T entity, Expression<Func<T, object>> expression, bool ignoreAttribute = true) where T : IEntity
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var visitor = new ObjectExpressionVisitor(entityInfo);
            visitor.Visit(expression);
            var include = visitor.GetPropertyList().Select(kv => kv.Key);
            return UpdateInclude(entity, include, ignoreAttribute);
        }

        /// <summary>
        /// 修改实体除指定属性外的其他属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">要修改的实体</param>
        /// <param name="ignore">要忽略的属性，注意：是实体的属性名而不是数据表的列名</param>
        /// <param name="ignoreAttribute">是否忽略实体的UpdateIgnore描述。默认为true，既includes中包含的所有属性都会被修改</param>
        /// <returns>受影响的记录数</returns>
        public int UpdateIgnore<T>(T entity, IEnumerable<string> ignore, bool ignoreAttribute = true) where T : IEntity
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var includeProperties = entityInfo.Properties.Where(p => !ignore.Contains(p.Name) && p.Name != "Id").ToList();

            if (!ignoreAttribute)
            {
                includeProperties = includeProperties.Where(p => !p.UpdateIgnore).ToList();
            }

            if (includeProperties.Count == 0)
            {
                return 0;
            }

            var sql =
                $"UPDATE [{entityInfo.TableName}] SET {string.Join(",", includeProperties.Select(p => $"{p.FieldName}=@{p.Name}"))} WHERE Id=@Id";
            var parameters = new List<SqlParameter> { new SqlParameter("@Id", entity.Id) };

            foreach (var property in includeProperties)
            {
                parameters.Add(new SqlParameter($"@{property.Name}", ResolveParameterValue(property.PropertyInfo.GetValue(entity))));
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(parameters.ToArray());
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">要修改的实体</param>
        /// <param name="expression">要修改的属性，注意不支持导航属性及其子属性</param>
        /// <param name="ignoreAttribute">是否忽略实体的UpdateIgnore描述。默认为true，既includes中包含的所有属性都会被修改</param>
        /// <returns>受影响的记录数</returns>
        public int UpdateIgnore<T>(T entity, Expression<Func<T, object>> expression, bool ignoreAttribute = true) where T : IEntity
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var visitor = new ObjectExpressionVisitor(entityInfo);
            visitor.Visit(expression);
            var include = visitor.GetPropertyList().Select(kv => kv.Key);
            return UpdateIgnore(entity, include, ignoreAttribute);
        }
    }
}
