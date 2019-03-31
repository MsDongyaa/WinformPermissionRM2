using RM2.Orm.Commons;
using RM2.Orm.Expressions;
using RM2.Orm.Queryable;
using RM2.Orm.Reflections;
using RM2.Orm.SqlBuilders;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace RM2.Orm
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

        public static MyDb New()
        {
            return new MyDb();
        }

        public static MyDb New(string connectionString, string prefix = "@")
        {
            return new MyDb(connectionString, prefix);
        }

        #region 查找
        public MyQueryable<T> Query<T>() where T : class, IEntity, new()
        {
            return new MyQueryable<T>(_connectionString);
        }

        public T Load<T>(int id) where T : class, IEntity, new()
        {
            return new MyQueryable<T>(_connectionString).Where(t => t.Id == id).FirstOrDefault();
        }

        public T Load<T>(Expression<Func<T, bool>> where = null, params Expression<Func<T, object>>[] orderBy) where T : class, new()
        {
            var query = new MyQueryable<T>(_connectionString);
            if (where != null)
            {
                query.Where(where);
            }

            if (orderBy.Length > 0)
            {
                foreach (var ob in orderBy)
                {
                    query.OrderBy(ob);
                }
            }

            return query.FirstOrDefault();
        }

        public List<T> Fetch<T>(Expression<Func<T, bool>> where = null, params Expression<Func<T, object>>[] orderBy) where T : class, new()
        {
            var query = new MyQueryable<T>(_connectionString);
            if (where != null)
            {
                query.Where(where);
            }

            if (orderBy.Length > 0)
            {
                foreach (var ob in orderBy)
                {
                    query.OrderBy(ob);
                }
            }

            return query.ToList();
        }

        public List<T> PageList<T>(int pageIndex,
            int pageSize,
            out int recordCount,
            Expression<Func<T, bool>> where = null,
            params Expression<Func<T, object>>[] orderBy) where T : class, new()
        {
            var query = new MyQueryable<T>(_connectionString);
            if (where != null)
            {
                query.Where(where);
            }

            if (orderBy.Length > 0)
            {
                foreach (var ob in orderBy)
                {
                    query.OrderBy(ob);
                }
            }

            return query.ToPageList(pageIndex, pageSize, out recordCount);
        }
        #endregion

        #region 创建
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

        public int InsertIfNotExist<T>(T entity, Expression<Func<T, bool>> where) where T : class, IEntity, new()
        {
            if (where == null)
            {
                return Insert<T>(entity);
            }
            else
            {
                var entityInfo = MyEntityContainer.Get(typeof(T));
                var whereExpressionVisitor = new WhereExpressionVisitor<T>(entityInfo);
                whereExpressionVisitor.Visit(where);

                var condition = whereExpressionVisitor.GetCondition();
                var parameters = whereExpressionVisitor.GetParameters().ToSqlParameters();

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
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        count = 0;
                    }
                }
            }

            return count;
        }

        public int UpdateIfNotExit<T>(T entity, Expression<Func<T, bool>> where)
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var whereExpressionVisitor = new WhereExpressionVisitor<T>(entityInfo);
            whereExpressionVisitor.Visit(where);

            var condition = whereExpressionVisitor.GetCondition();
            var parameters = whereExpressionVisitor.GetParameters().ToSqlParameters();

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

        public int Delete<T>(int id) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var sql = $"DELETE [{entityInfo.TableName}] WHERE [{entityInfo.KeyColumn}]={_prefix}Id";
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddWithValue($"{_prefix}Id", id);
                return command.ExecuteNonQuery();
            }
        }

        public int Delete<T>(IEnumerable<int> idList) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var sql = $"EXEC('DELETE [{entityInfo.TableName}] WHERE [{entityInfo.KeyColumn}] in ('+{_prefix}Ids+')')";
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddWithValue($"{_prefix}Ids", string.Join(",", idList));
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
                var whereExpressionVisitor = new WhereExpressionVisitor<T>();
                whereExpressionVisitor.Visit(expression);
                var condition = whereExpressionVisitor.GetCondition();
                var parameters = whereExpressionVisitor.GetParameters();

                condition = string.IsNullOrWhiteSpace(condition) ? "1=1" : condition;

                var sql = $"SELECT COUNT(0) FROM [{entityInfo.TableName}] WHERE [{condition}]";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var command = new SqlCommand(sql, conn);
                    command.Parameters.AddRange(parameters.ToSqlParameters().ToArray());
                    return (int)command.ExecuteScalar();
                }
            }
        }
        #endregion

        #region 私有方法

        private TResult Exec<TResult>(Func<SqlConnection, TResult> func)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                return func.Invoke(conn);
            }
        }

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
