using MyMiniOrm.Commons;
using MyMiniOrm.Expressions;
using MyMiniOrm.Queryable;
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
        private readonly string _connectionString;

        private readonly string _prefix;

        public MyDb(string connectionString, string prefix = "@")
        {
            _connectionString = connectionString;
            _prefix = prefix;
        }

        public MyDb()
        {
            if (string.IsNullOrWhiteSpace(MyMiniOrmConfiguration.GetConnectionString()))
            {
                throw new Exception("MyMiniOrm尚未初始化");
            }

            _connectionString = MyMiniOrmConfiguration.GetConnectionString();
            _prefix = MyMiniOrmConfiguration.GetPrefix();
        }

        /// <summary>
        /// 使用默认配置，返回新MyDb实例
        /// </summary>
        /// <returns></returns>
        public static MyDb New()
        {
            return new MyDb();
        }

        /// <summary>
        /// 返回新MyDb实例
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static MyDb New(string connectionString, string prefix = "@")
        {
            return new MyDb(connectionString, prefix);
        }

        #region 查找
        /// <summary>
        /// 返回MyQueryable实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>实体，若记录为空，返回default(T)</returns>
        public MyQueryable<T> Query<T>() where T : class, IEntity, new()
        {
            return new MyQueryable<T>(_connectionString);
        }

        /// <summary>
        /// 根据ID加载一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">实体ID</param>
        /// <returns>实体，若记录为空，返回default(T)</returns>
        public T Load<T>(int id) where T : class, IEntity, new()
        {
            return new MyQueryable<T>(_connectionString).Where(t => t.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// 根据条件加载一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderBy">排序字段</param>
        /// <param name="dbSort">正序或倒序</param>
        /// <returns>实体，若记录为空，返回default(T)</returns>
        public T Load<T>(Expression<Func<T, bool>> where = null, Expression<Func<T, object>> orderBy = null, MyDbSort dbSort = MyDbSort.Asc) where T : class, new()
        {
            var query = new MyQueryable<T>(_connectionString);
            if (where != null)
            {
                query.Where(where);
            }

            if (orderBy != null)
            {
                if (dbSort == MyDbSort.Desc)
                {
                    query.OrderByDesc(orderBy);
                }
                else
                {
                    query.OrderBy(orderBy);
                }
            }

            return query.FirstOrDefault();
        }

        /// <summary>
        /// 根据条件加载所有实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderBy">排序字段</param>
        /// <param name="dbSort">正序或倒序</param>
        /// <returns>实体列表</returns>
        public List<T> Fetch<T>(Expression<Func<T, bool>> where = null, Expression<Func<T, object>> orderBy = null, MyDbSort dbSort = MyDbSort.Asc) where T : class, new()
        {
            var query = new MyQueryable<T>(_connectionString);
            if (where != null)
            {
                query.Where(where);
            }

            if (orderBy != null)
            {
                if (dbSort == MyDbSort.Desc)
                {
                    query.OrderByDesc(orderBy);
                }
                else
                {
                    query.OrderBy(orderBy);
                }
            }

            return query.ToList();
        }

        /// <summary>
        /// 加载分页列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页条数</param>
        /// <param name="recordCount">记录总数</param>
        /// <param name="where">查询条件</param>
        /// <param name="orderBy">排序字段</param>
        /// <param name="dbSort">正序或倒序</param>
        /// <returns>实体列表，输出记录总数</returns>
        public List<T> PageList<T>(int pageIndex,
            int pageSize,
            out int recordCount,
            Expression<Func<T, bool>> where = null,
            Expression<Func<T, object>> orderBy = null,
            MyDbSort dbSort = MyDbSort.Asc) where T : class, new()
        {
            var query = new MyQueryable<T>(_connectionString);
            if (where != null)
            {
                query.Where(where);
            }

            if (orderBy != null)
            {
                if (dbSort == MyDbSort.Desc)
                {
                    query.OrderByDesc(orderBy);
                }
                else
                {
                    query.OrderBy(orderBy);
                }
            }

            return query.ToPageList(pageIndex, pageSize, out recordCount);
        }
        #endregion

        #region 创建
        /// <summary>
        /// 创建一个实体，新的记录Id将绑定到entity的Id属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">要创建的实体</param>
        /// <returns>新生成记录的ID，若失败返回0</returns>
        public int Insert<T>(T entity) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            var sqlBuilder = new SqlServerSqlBuilder();
            var sql = sqlBuilder.Insert(entityInfo);

            var parameterList = entityInfo
                .Properties
                .Where(p => !p.InsertIgnore)
                .Select(p => new SqlParameter($"{_prefix}{p.Name}", ResolveParameterValue(p.PropertyInfo.GetValue(entity))));

            var command = new SqlCommand(sql);
            command.Parameters.AddRange(parameterList.ToArray());

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                command.Connection = conn;
                var result = command.ExecuteScalar().ToString();
                entity.Id = Convert.ToInt32(string.IsNullOrWhiteSpace(result) ? "0" : result);
                return entity.Id;
            }
        }

        /// <summary>
        /// 如果不满足条件则创建一个实体，
        /// 如限制用户名不能重复 InsertIfNotExist(user, u => u.Name == user.Name)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">要创建的实体</param>
        /// <param name="where">条件</param>
        /// <returns>新生成记录的ID，若失败返回0</returns>
        public int InsertIfNotExists<T>(T entity, Expression<Func<T, bool>> where) where T : class, IEntity, new()
        {
            if (where == null)
            {
                return Insert(entity);
            }
            else
            {
                var entityInfo = MyEntityContainer.Get(typeof(T));
                //var whereExpressionVisitor = new WhereExpressionVisitor<T>(entityInfo);
                //whereExpressionVisitor.Visit(where);
                //var condition = whereExpressionVisitor.GetCondition();
                //var parameters = whereExpressionVisitor.GetParameters().ToSqlParameters();

                var resolver = new ConditionResolver(entityInfo);
                resolver.Resolve(where.Body);
                var condition = resolver.GetCondition();
                var parameters = resolver.GetParameters().ToSqlParameters();

                condition = string.IsNullOrWhiteSpace(condition) ? "1=1" : condition;

                var sqlBuilder = new SqlServerSqlBuilder();
                var sql = sqlBuilder.InsertIfNotExists(entityInfo, condition);

                parameters.AddRange(
                    entityInfo
                        .Properties
                        .Where(p => !p.InsertIgnore)
                        .Select(p => new SqlParameter($"{_prefix}{p.Name}",
                            ResolveParameterValue(p.PropertyInfo.GetValue(entity)))));
                var command = new SqlCommand(sql);
                command.Parameters.AddRange(parameters.ToArray());

                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    command.Connection = conn;
                    var result = command.ExecuteScalar().ToString();
                    entity.Id = Convert.ToInt32(string.IsNullOrWhiteSpace(result) ? "0" : result);
                    return entity.Id;
                }
            }
        }

        /// <summary>
        /// 批量创建实体，注意此方法效率不高
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityList">实体列表</param>
        /// <returns>受影响的记录数</returns>
        public int Insert<T>(List<T> entityList) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            var sqlBuilder = new SqlServerSqlBuilder();
            var sql = sqlBuilder.Insert(entityInfo);

            var count = 0;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (var entity in entityList)
                        {
                            using (var command = new SqlCommand(sql, conn, trans))
                            {
                                command.Parameters.AddRange(entityInfo
                                    .Properties
                                    .Where(p => !p.InsertIgnore)
                                    .Select(p => new SqlParameter($"{_prefix}{p.Name}",
                                        ResolveParameterValue(p.PropertyInfo.GetValue(entity))))
                                    .ToArray());
                                var result = command.ExecuteScalar().ToString();
                                entity.Id = Convert.ToInt32(string.IsNullOrWhiteSpace(result) ? "0" : result);
                                count++;
                            }
                        }
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        count = 0;
                    }
                }
            }

            return count;
        }
        #endregion

        #region 更新
        /// <summary>
        /// 更新一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">要更新的实体</param>
        /// <returns>受影响的记录数</returns>
        public int Update<T>(T entity) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            var sqlBuilder = new SqlServerSqlBuilder();
            var sql = sqlBuilder.Update(entityInfo, "");

            var parameterList = entityInfo
                .Properties
                .Where(p => !p.UpdateIgnore || p.IsKey)
                .Select(p => new SqlParameter($"{_prefix}{p.Name}",
                    ResolveParameterValue(p.PropertyInfo.GetValue(entity))));

            var command = new SqlCommand(sql);
            command.Parameters.AddRange(parameterList.ToArray());

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                command.Connection = conn;
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 更新多个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityList">要更新的实体列表</param>
        /// <returns>受影响的记录数</returns>
        public int Update<T>(List<T> entityList) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            var sqlBuilder = new SqlServerSqlBuilder();
            var sql = sqlBuilder.Update(entityInfo, "");

            var count = 0;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (var entity in entityList)
                        {
                            using (var command = new SqlCommand(sql, conn, trans))
                            {
                                command.Parameters.AddRange(entityInfo
                                    .Properties
                                    .Where(p => !p.UpdateIgnore || p.IsKey)
                                    .Select(p => new SqlParameter($"{_prefix}{p.Name}",
                                        ResolveParameterValue(p.PropertyInfo.GetValue(entity))))
                                    .ToArray());
                                count += command.ExecuteNonQuery();
                            }
                        }
                        trans.Commit();
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                        count = 0;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// 如果不存在，则更新
        /// 如：UpdateIfNotExists(user, u=>u.Name == user.Name && u.Id != user.Id)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="where"></param>
        /// <returns>受影响的记录数</returns>
        public int UpdateIfNotExits<T>(T entity, Expression<Func<T, bool>> where)
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            //var whereExpressionVisitor = new WhereExpressionVisitor<T>(entityInfo);
            //whereExpressionVisitor.Visit(where);
            //var condition = whereExpressionVisitor.GetCondition();
            //var parameters = whereExpressionVisitor.GetParameters().ToSqlParameters();

            var resolver = new ConditionResolver(entityInfo);
            resolver.Resolve(where.Body);
            var condition = resolver.GetCondition();
            var parameters = resolver.GetParameters().ToSqlParameters();

            condition = string.IsNullOrWhiteSpace(condition) ? "1=1" : condition;

            var sqlBuilder = new SqlServerSqlBuilder();
            var sql = sqlBuilder.Update(entityInfo, "");
            sql += $" AND NOT EXISTS (SELECT 1 FROM [{entityInfo.TableName}] WHERE {condition})";

            parameters.AddRange(
                entityInfo
                    .Properties
                    .Where(p => !p.UpdateIgnore || p.IsKey)
                    .Select(p => new SqlParameter($"{_prefix}{p.Name}",
                        ResolveParameterValue(p.PropertyInfo.GetValue(entity)))));

            var command = new SqlCommand(sql);
            command.Parameters.AddRange(parameters.ToArray());

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                command.Connection = conn;
                return command.ExecuteNonQuery();
            }
        }
        #endregion

        #region 删除

        /// <summary>
        /// 根据ID删除记录，如果支持软删除并且非强制删除，则更新IsDel字段为true，否则，删除记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">要删除的实体ID</param>
        /// <param name="isForce">是否强制删除，默认为false</param>
        /// <returns>受影响的记录数</returns>
        public int Delete<T>(int id, bool isForce = false) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            if (isForce || !entityInfo.IsSoftDelete)
            {
                var sql = $"DELETE [{entityInfo.TableName}] WHERE [{entityInfo.KeyColumn}]={_prefix}Id";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var command = new SqlCommand(sql, conn);
                    command.Parameters.AddWithValue($"{_prefix}Id", id);
                    return command.ExecuteNonQuery();
                }
            }
            else
            {
                var sql = $"UPDATE [{entityInfo.TableName}] SET IsDel=1 WHERE [{entityInfo.KeyColumn}]={_prefix}Id";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var command = new SqlCommand(sql, conn);
                    command.Parameters.AddWithValue($"{_prefix}Id", id);
                    return command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 根据ID批量删除记录，如果支持软删除并且非强制删除，则更新IsDel字段为true，否则，删除记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="idList">要删除的ID列表</param>
        /// <param name="isForce">是否强制删除，默认为false</param>
        /// <returns>受影响的记录数</returns>
        public int Delete<T>(IEnumerable<int> idList, bool isForce = false) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            if (isForce || !entityInfo.IsSoftDelete)
            {
                var sql =
                    $"EXEC('DELETE [{entityInfo.TableName}] WHERE [{entityInfo.KeyColumn}] in ('+{_prefix}Ids+')')";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var command = new SqlCommand(sql, conn);
                    command.Parameters.AddWithValue($"{_prefix}Ids", string.Join(",", idList));
                    return command.ExecuteNonQuery();
                }
            }
            else
            {
                var sql = $"EXEC('UPDATE [{entityInfo.TableName}] SET IsDel=1 WHERE [{entityInfo.KeyColumn}] in ('+{_prefix}Ids+')')";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var command = new SqlCommand(sql, conn);
                    command.Parameters.AddWithValue($"{_prefix}Id", idList);
                    return command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 根据条件删除记录，如果支持软删除并且非强制删除，则更新IsDel字段为true，否则，删除记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">条件，注意不支持导航属性及其子属性</param>
        /// <param name="isForce">是否强制删除</param>
        /// <returns>受影响的记录数</returns>
        public int Delete<T>(Expression<Func<T, bool>> expression, bool isForce) where T : IEntity
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            //var whereExpressionVisitor = new WhereExpressionVisitor<T>();
            //whereExpressionVisitor.Visit(expression);
            //var where = whereExpressionVisitor.GetCondition();
            //var whereParameters = whereExpressionVisitor.GetParameters().ToSqlParameters();

            var resolver = new ConditionResolver(entityInfo);
            resolver.Resolve(expression.Body);
            var condition = resolver.GetCondition();
            var parameters = resolver.GetParameters().ToSqlParameters();

            condition = string.IsNullOrWhiteSpace(condition) ? "1=1" : condition;
            string sql;
            if (isForce || !entityInfo.IsSoftDelete)
            {
                sql =
                    $"DELETE [{entityInfo.TableName}] WHERE {condition}";
            }
            else
            {
                sql =
                    $"UPDATE [{entityInfo.TableName}] SET IsDel=1 WHERE {condition}";
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(parameters.ToArray());
                return command.ExecuteNonQuery();
            }
        }
        #endregion

        #region 数量

        public int GetCount<T>(Expression<Func<T, bool>> expression = null) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            if (expression == null)
            {
                var sql = $"SELECT COUNT(0) FROM [{entityInfo.TableName}]";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var command = new SqlCommand(sql, conn);
                    return (int)command.ExecuteScalar();
                }
            }
            else
            {
                //var whereExpressionVisitor = new WhereExpressionVisitor<T>();
                //whereExpressionVisitor.Visit(expression);
                //var condition = whereExpressionVisitor.GetCondition();
                //var parameters = whereExpressionVisitor.GetParameters().ToSqlParameters();

                var resolver = new ConditionResolver(entityInfo);
                resolver.Resolve(expression.Body);
                var condition = resolver.GetCondition();
                var parameters = resolver.GetParameters().ToSqlParameters();

                condition = string.IsNullOrWhiteSpace(condition) ? "1=1" : condition;

                var sql = $"SELECT COUNT(0) FROM [{entityInfo.TableName}] WHERE [{condition}]";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var command = new SqlCommand(sql, conn);
                    command.Parameters.AddRange(parameters.ToArray());
                    return (int)command.ExecuteScalar();
                }
            }
        }
        #endregion

        #region 私有方法

        private object ResolveParameterValue(object val)
        {
            if (val is null)
            {
                val = DBNull.Value;
            }

            return val;
        }
        #endregion
    }
}
