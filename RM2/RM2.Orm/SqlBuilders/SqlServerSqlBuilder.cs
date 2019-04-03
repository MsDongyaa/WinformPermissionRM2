using MyMiniOrm.Commons;
using MyMiniOrm.Reflections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyMiniOrm.SqlBuilders
{
    public class SqlServerSqlBuilder
    {
        protected readonly string Prefix = "@";

        public string Select(string table, string columns, string where, string sort, int top = 0)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));

            var sb = new StringBuilder("SELECT ");
            if (top > 0)
            {
                sb.Append("TOP ").Append(top.ToString()).Append(" ");
            }

            sb.Append(string.IsNullOrWhiteSpace(columns) ? "*" : columns);

            sb.Append(" FROM ").Append(table);

            if (!string.IsNullOrWhiteSpace(where))
            {
                sb.Append(" WHERE ").Append(where);
            }

            if (!string.IsNullOrWhiteSpace(sort))
            {
                sb.Append(" ORDER BY ").Append(sort);
            }

            return sb.ToString();
        }

        public string PagingSelect(string table, string columns, string where, string sort, int pageIndex, int pageSize)
        {
            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            pageSize = pageSize <= 0 ? 20 : pageSize;
            where = string.IsNullOrWhiteSpace(where) ? "1=1" : where;

            if (pageIndex == 1)
            {
                var sql = Select(table, columns, where, sort, pageSize);
                sql += $";SELECT @RecordCount=COUNT(0) FROM {table} WHERE {where}";
                return sql;
            }

            columns = string.IsNullOrWhiteSpace(columns) ? "*" : columns;
            sort = string.IsNullOrWhiteSpace(sort) ? "(SELECT 1)" : sort;

            var sb = new StringBuilder();
            sb.Append("SELECT ")
                .Append(columns)
                .Append(" FROM ")
                .Append(table)
                .Append(" WHERE ")
                .Append(where)
                .Append(" ORDER BY ")
                .Append(sort)
                .Append(" OFFSET ")
                .Append((pageIndex - 1) * pageSize)
                .Append(" ROWS FETCH NEXT ")
                .Append(pageSize)
                .Append(" ROWS ONLY;");
            sb.Append("SELECT @RecordCount=COUNT(0) FROM ")
                .Append(table)
                .Append(string.IsNullOrWhiteSpace(where) ? "" : " WHERE " + where);

            return sb.ToString();
        }

        public string GetPagingQuerySql(string cols, string tables, string condition, string orderBy, int index, int size)
        {
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                orderBy = "(select 1)";
            }

            if (string.IsNullOrWhiteSpace(condition))
            {
                condition = "1=1";
            }

            if (index == 1)
            {
                var sql =
                    $"SELECT TOP {size} {cols} FROM {tables} WHERE {condition} ORDER BY {orderBy};SELECT {Prefix}RecordCount=COUNT(0) FROM {tables} WHERE {condition}";

                return sql;
            }
            else
            {
                var sb = new StringBuilder();
                sb.Append("FROM ").Append(tables);
                sb.Append(" WHERE ").Append(condition);
                var sql = $@"  WITH PAGEDDATA AS
					    (
						    SELECT TOP 100 PERCENT {cols}, ROW_NUMBER() OVER (ORDER BY {@orderBy}) AS FLUENTDATA_ROWNUMBER
						    {sb}
					    )
					    SELECT *
					    FROM PAGEDDATA
					    WHERE FLUENTDATA_ROWNUMBER BETWEEN {(index - 1) * size + 1} AND {index * size};
                        SELECT {Prefix}RecordCount=COUNT(0) FROM {tables} WHERE {condition}";
                return sql;
            }
        }

        public string InsertIfNotExists(MyEntity entityInfo, string where)
        {
            var sb = new StringBuilder();
            sb.Append("IF NOT EXISTS (SELECT 1 FROM [")
                .Append(entityInfo.TableName)
                .Append("] WHERE ")
                .Append(where)
                .Append(")");
            sb.Append(Insert(entityInfo));
            return sb.ToString();
        }

        public string Insert(MyEntity entityInfo)
        {
            if (entityInfo == null) throw new ArgumentNullException(nameof(entityInfo));

            var sb = new StringBuilder();
            var columns = new List<string>();
            var parameters = new List<string>();

            foreach (var prop in entityInfo.Properties.Where(p => !p.InsertIgnore))
            {
                columns.Add("[" + prop.FieldName + "]");
                parameters.Add(Prefix + prop.Name);
            }

            sb.Append("INSERT INTO [")
                .Append(entityInfo.TableName)
                .Append("] (")
                .Append(string.Join(",", columns))
                .Append(") VALUES (")
                .Append(string.Join(",", parameters))
                .Append(");SELECT SCOPE_IDENTITY();");
            return sb.ToString();
        }

        public string Update(MyEntity entityInfo, string where)
        {
            if (entityInfo == null) throw new ArgumentNullException(nameof(entityInfo));

            var sb = new StringBuilder();
            sb.Append("UPDATE [")
                .Append(entityInfo.TableName)
                .Append("] SET ");

            var clauses = entityInfo.Properties.Where(p => !p.UpdateIgnore)
                .Select(p => $"{p.FieldName}={Prefix}{p.Name}");
            sb.Append(string.Join(",", clauses));

            sb.Append(string.IsNullOrWhiteSpace(where) ? $" WHERE [{entityInfo.KeyColumn}]={Prefix}Id" : where);

            return sb.ToString();
        }

        public string UpdateIgnore(MyEntity entityInfo, string[] propertyList, bool ignoreAttribute, string where)
        {
            if (entityInfo == null) throw new ArgumentNullException(nameof(entityInfo));

            var properties = entityInfo.Properties;

            properties = ignoreAttribute
                ? properties.Where(p => !propertyList.Contains(p.Name)).ToList()
                : properties.Where(p => !p.UpdateIgnore && !propertyList.Contains(p.Name)).ToList();

            if (properties.Count == 0)
            {
                throw new ArgumentNullException(nameof(properties), "要更新的列为空");
            }

            var sb = new StringBuilder();
            sb.Append("UPDATE [")
                .Append(entityInfo.TableName)
                .Append("] SET ");

            var clauses = properties.Select(p => $"{p.FieldName}={Prefix}{p.Name}");
            sb.Append(string.Join(",", clauses));
            sb.Append(string.IsNullOrWhiteSpace(where) ? $" WHERE [{entityInfo.KeyColumn}]={Prefix}Id" : where);
            return sb.ToString();
        }

        public string UpdateInclude(MyEntity entityInfo, string[] propertyList, bool ignoreAttribute, string where)
        {
            var properties = entityInfo.Properties;

            properties = ignoreAttribute
                ? properties.Where(p => propertyList.Contains(p.Name)).ToList()
                : properties.Where(p => !p.UpdateIgnore && propertyList.Contains(p.Name)).ToList();

            if (properties.Count == 0)
            {
                throw new ArgumentNullException(nameof(properties), "要更新的列为空");
            }

            var sb = new StringBuilder();
            sb.Append("UPDATE [")
                .Append(entityInfo.TableName)
                .Append("] SET ");

            var clauses = properties.Select(p => $"{p.FieldName}={Prefix}{p.Name}");
            sb.Append(string.Join(",", clauses));
            sb.Append(string.IsNullOrWhiteSpace(where) ? $" WHERE [{entityInfo.KeyColumn}]={Prefix}Id" : where);
            return sb.ToString();
        }

        public string Update(string table, DbKvs kvs, string where)
        {
            if (kvs == null || kvs.Count == 0) throw new ArgumentNullException(nameof(kvs));
            if (string.IsNullOrWhiteSpace(table)) throw new ArgumentNullException(nameof(table));

            var sb = new StringBuilder();
            sb.Append("UPDATE [")
                .Append(table)
                .Append("] SET ");
            var clauses = kvs.Select(kv => $"[{kv.Key}]={Prefix}{kv.Key}");
            sb.Append(string.Join(",", clauses));
            if (!string.IsNullOrWhiteSpace(where))
            {
                sb.Append(" WHERE ").Append(where);
            }

            return sb.ToString();
        }
    }
}
