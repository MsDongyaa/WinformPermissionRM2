﻿using RM2.Orm.Commons;
using RM2.Orm.Expressions;
using RM2.Orm.Reflections;
using RM2.Orm.SqlBuilders;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace RM2.Orm
{
    public partial class MyDb
    {
        public int Update<T>(T entity) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            var sb = new StringBuilder("UPDATE ");
            sb.Append($"[{entityInfo.TableName}] SET ");
            sb.Append(string.Join(",",
                entityInfo.Properties.Where(p => !p.UpdateIgnore).Select(p => $"[{p.FieldName}]=@{p.Name}")));
            sb.Append($" WHERE [{entityInfo.KeyColumn}]=@Id");

            var parameterList = entityInfo
                .Properties
                .Where(p => !p.UpdateIgnore)
                .Select(p => new SqlParameter($"@{p.Name}", p.PropertyInfo.GetValue(entity)));

            var command = new SqlCommand(sb.ToString());
            command.Parameters.AddRange(parameterList.ToArray());
            command.Parameters.AddWithValue("@Id", entity.Id);

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

            var sb = new StringBuilder("UPDATE ");
            sb.Append($"[{entityInfo.TableName}] SET ");
            sb.Append(string.Join(",",
                entityInfo.Properties.Where(p => !p.UpdateIgnore).Select(p => $"[{p.FieldName}]=@{p.Name}")));
            sb.Append($" WHERE [{entityInfo.KeyColumn}]=@Id");

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
                            using (var command = new SqlCommand(sb.ToString(), conn, trans))
                            {
                                command.Parameters.AddRange(entityInfo
                                    .Properties
                                    .Where(p => !p.UpdateIgnore)
                                    .Select(p => new SqlParameter($"@{p.Name}", p.PropertyInfo.GetValue(entity)))
                                    .ToArray());
                                command.Parameters.AddWithValue("@Id", entity.Id);
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
                var whereExpressionVisitor = new WhereExpressionVisitor<T>();
                whereExpressionVisitor.Visit(expression);
                var where = whereExpressionVisitor.GetCondition();
                var whereParameters = whereExpressionVisitor.GetParameters().ToSqlParameters();
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

        public int Update<T>(T entity, IEnumerable<string> includes) where T : IEntity
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var includeProperties = entityInfo.Properties.Where(p => includes.Contains(p.Name) && p.Name != "Id").ToList();
            if (includeProperties.Count == 0)
            {
                return 0;
            }

            var sql =
                $"UPDATE [{entityInfo.TableName}] SET {string.Join(",", includeProperties.Select(p => $"{p.FieldName}=@{p.Name}"))} WHERE Id=@Id";
            var parameters = new List<SqlParameter> { new SqlParameter("@Id", entity.Id) };

            foreach (var property in includeProperties)
            {
                parameters.Add(new SqlParameter($"@{property.Name}", property.PropertyInfo.GetValue(entity)));
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(parameters.ToArray());
                return command.ExecuteNonQuery();
            }
        }

        public int UpdateIgnore<T>(T entity, IEnumerable<string> ignore) where T : IEntity
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var includeProperties = entityInfo.Properties.Where(p => !ignore.Contains(p.Name) && p.Name != "Id").ToList();
            if (includeProperties.Count == 0)
            {
                return 0;
            }

            var sql =
                $"UPDATE [{entityInfo.TableName}] SET {string.Join(",", includeProperties.Select(p => $"{p.FieldName}=@{p.Name}"))} WHERE Id=@Id";
            var parameters = new List<SqlParameter> { new SqlParameter("@Id", entity.Id) };

            foreach (var property in includeProperties)
            {
                parameters.Add(new SqlParameter($"@{property.Name}", property.PropertyInfo.GetValue(entity)));
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(parameters.ToArray());
                return command.ExecuteNonQuery();
            }
        }
    }
}