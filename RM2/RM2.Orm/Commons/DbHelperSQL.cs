using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace MyMiniOrm.Commons
{
    /// <summary>
    /// SqlServer数据访问类
    /// Copyright (C) HZC
    /// </summary> I
    public class DbHelperSqlServer
    {
        private readonly string _connectionString;

        public DbHelperSqlServer(string connectionString)
        {
            _connectionString = connectionString;
        }

        #region 公用方法
        /// <summary>
        /// 判断是否存在某表的某个字段
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="columnName">列名称</param>
        /// <returns>是否存在</returns>
        public bool ColumnExists(string tableName, string columnName)
        {
            var sql = "select count(1) from SysColumns where [id]=object_connectionStringId('" + tableName + "') and [name]='" + columnName + "'";
            var res = GetSingle(sql);
            if (res == null)
            {
                return false;
            }
            return Convert.ToInt32(res) > 0;
        }

        public int GetMaxId(string fieldName, string tableName)
        {
            var sqlString = "select max(" + fieldName + ")+1 from " + tableName;
            var obj = GetSingle(sqlString);
            return obj == null ? 1 : int.Parse(obj.ToString());
        }
        public bool Exists(string sqlString)
        {
            var obj = GetSingle(sqlString);
            int cmdResult;
            if ((Equals(obj, null)) || (Equals(obj, DBNull.Value)))
            {
                cmdResult = 0;
            }
            else
            {
                cmdResult = int.Parse(obj.ToString());
            }
            return cmdResult != 0;
        }

        /// <summary>
        /// 表是否存在
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public bool TabExists(string tableName)
        {
            var sqlString = "select count(*) from SysObjects where id = object_connectionStringId(N'[" + tableName + "]') and OBJECTPROPERTY(id, N'IsUserTable') = 1";
            var obj = GetSingle(sqlString);
            int cmdResult;
            if ((Equals(obj, null)) || (Equals(obj, DBNull.Value)))
            {
                cmdResult = 0;
            }
            else
            {
                cmdResult = int.Parse(obj.ToString());
            }
            return cmdResult != 0;
        }
        public bool Exists(string sqlString, params SqlParameter[] cmdParams)
        {
            var obj = GetSingle(sqlString, cmdParams);
            int cmdResult;
            if ((Equals(obj, null)) || (Equals(obj, DBNull.Value)))
            {
                cmdResult = 0;
            }
            else
            {
                cmdResult = int.Parse(obj.ToString());
            }
            return cmdResult != 0;
        }
        #endregion

        #region  执行简单SQL语句

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteSql(string sqlString)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand(sqlString, connection))
                {
                    try
                    {
                        connection.Open();
                        var rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (SqlException)
                    {
                        connection.Close();
                        throw;
                    }
                }
            }
        }

        public int ExecuteSqlByTime(string sqlString, int times)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand(sqlString, connection))
                {
                    try
                    {
                        connection.Open();
                        cmd.CommandTimeout = times;
                        var rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (SqlException)
                    {
                        connection.Close();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="sqlStringList">多条SQL语句</param>		
        public int ExecuteSqlTran(List<String> sqlStringList)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand { Connection = conn };
                var tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    var count = 0;
                    foreach (var sqlString in sqlStringList)
                    {
                        if (sqlString.Trim().Length <= 1) continue;

                        cmd.CommandText = sqlString;
                        count += cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                    return count;
                }
                catch
                {
                    tx.Rollback();
                    return 0;
                }
            }
        }
        /// <summary>
        /// 执行带一个存储过程参数的的SQL语句。
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteSql(string sqlString, string content)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand(sqlString, connection);
                var myParameter = new SqlParameter("@content", SqlDbType.NText)
                {
                    Value = content
                };
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    var rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }
        /// <summary>
        /// 执行带一个存储过程参数的的SQL语句。
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>
        /// <returns>影响的记录数</returns>
        public object ExecuteSqlGet(string sqlString, string content)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand(sqlString, connection);
                var myParameter = new SqlParameter("@content", SqlDbType.NText)
                {
                    Value = content
                };
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    var obj = cmd.ExecuteScalar();
                    if ((Equals(obj, null)) || (Equals(obj, DBNull.Value)))
                    {
                        return null;
                    }
                    else
                    {
                        return obj;
                    }
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }
        /// <summary>
        /// 向数据库里插入图像格式的字段(和上面情况类似的另一种实例)
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="fs">图像字节,数据库的字段类型为image的情况</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteSqlInsertImg(string sqlString, byte[] fs)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand(sqlString, connection);
                var myParameter = new SqlParameter("@fs", SqlDbType.Image);
                myParameter.Value = fs;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    var rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="sqlString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public object GetSingle(string sqlString)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand(sqlString, connection))
                {
                    try
                    {
                        connection.Open();
                        var obj = cmd.ExecuteScalar();
                        if ((Equals(obj, null)) || (Equals(obj, DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (SqlException)
                    {
                        connection.Close();
                        throw;
                    }
                }
            }
        }
        public object GetSingle(string sqlString, int times)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand(sqlString, connection))
                {
                    try
                    {
                        connection.Open();
                        cmd.CommandTimeout = times;
                        var obj = cmd.ExecuteScalar();
                        if ((Equals(obj, null)) || (Equals(obj, DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (SqlException)
                    {
                        connection.Close();
                        throw;
                    }
                }
            }
        }
        /// <summary>
        /// 执行查询语句，返回SqlDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="sqlString">查询语句</param>
        /// <returns>SqlDataReader</returns>
        public SqlDataReader ExecuteReader(string sqlString)
        {
            var connection = new SqlConnection(_connectionString);
            var cmd = new SqlCommand(sqlString, connection);
            connection.Open();
            var myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            return myReader;
        }
        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="sqlString">查询语句</param>
        /// <returns>DataSet</returns>
        public DataSet Query(string sqlString)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var ds = new DataSet();
                try
                {
                    connection.Open();
                    var command = new SqlDataAdapter(sqlString, connection);
                    command.Fill(ds, "RecordList");
                }
                catch (SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                return ds;
            }
        }
        public DataSet Query(string sqlString, int times)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var ds = new DataSet();
                try
                {
                    connection.Open();
                    var command = new SqlDataAdapter(sqlString, connection);
                    command.SelectCommand.CommandTimeout = times;
                    command.Fill(ds, "ds");
                }
                catch (SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                return ds;
            }
        }



        #endregion

        #region 执行带参数的SQL语句

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="cmdParams"></param>
        /// <returns>影响的记录数</returns>
        public int ExecuteSql(string sqlString, params SqlParameter[] cmdParams)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand())
                {
                    PrepareCommand(cmd, connection, null, sqlString, cmdParams);
                    var rows = cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                    return rows;
                }
            }
        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="sqlStringList">SQL语句的哈希表（key为sql语句，value是该语句的SqlParameter[]）</param>
        public void ExecuteSqlTran(Hashtable sqlStringList)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    var cmd = new SqlCommand();
                    try
                    {
                        //循环
                        foreach (DictionaryEntry myDe in sqlStringList)
                        {
                            var cmdText = myDe.Key.ToString();
                            var cmdParams = (SqlParameter[])myDe.Value;
                            PrepareCommand(cmd, conn, trans, cmdText, cmdParams);
                            cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }
        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="cmdList">SQL语句的哈希表（key为sql语句，value是该语句的SqlParameter[]）</param>
        public int ExecuteSqlTran(List<CommandInfo> cmdList)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    var cmd = new SqlCommand();
                    try
                    {
                        var count = 0;
                        //循环
                        foreach (var myDe in cmdList)
                        {
                            var cmdText = myDe.CommandText;
                            var cmdParams = myDe.Parameters;
                            PrepareCommand(cmd, conn, trans, cmdText, cmdParams);

                            if (myDe.EffectNextType == EffectNextType.WhenHaveContinue || myDe.EffectNextType == EffectNextType.WhenNoHaveContinue)
                            {
                                if (myDe.CommandText.ToLower().IndexOf("count(", StringComparison.Ordinal) == -1)
                                {
                                    trans.Rollback();
                                    return 0;
                                }

                                var obj = cmd.ExecuteScalar();
                                bool isHave;
                                if (obj == null || obj == DBNull.Value)
                                {
                                    isHave = false;
                                }
                                else
                                {
                                    isHave = Convert.ToInt32(obj) > 0;
                                }

                                if (myDe.EffectNextType == EffectNextType.WhenHaveContinue && !isHave)
                                {
                                    trans.Rollback();
                                    return 0;
                                }
                                if (myDe.EffectNextType == EffectNextType.WhenNoHaveContinue && isHave)
                                {
                                    trans.Rollback();
                                    return 0;
                                }
                                continue;
                            }
                            var val = cmd.ExecuteNonQuery();
                            count += val;
                            if (myDe.EffectNextType == EffectNextType.ExecuteEffectRows && val == 0)
                            {
                                trans.Rollback();
                                return 0;
                            }
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                        return count;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }
        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="sqlStringList">SQL语句的哈希表（key为sql语句，value是该语句的SqlParameter[]）</param>
        public void ExecuteSqlTranWithIdentity(List<CommandInfo> sqlStringList)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    var cmd = new SqlCommand();
                    try
                    {
                        var identity = 0;
                        //循环
                        foreach (var myDe in sqlStringList)
                        {
                            var cmdText = myDe.CommandText;
                            var cmdParams = myDe.Parameters;
                            foreach (var q in cmdParams)
                            {
                                if (q.Direction == ParameterDirection.InputOutput)
                                {
                                    q.Value = identity;
                                }
                            }
                            PrepareCommand(cmd, conn, trans, cmdText, cmdParams);
                            cmd.ExecuteNonQuery();
                            foreach (var q in cmdParams)
                            {
                                if (q.Direction == ParameterDirection.Output)
                                {
                                    identity = Convert.ToInt32(q.Value);
                                }
                            }
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }
        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="sqlStringList">SQL语句的哈希表（key为sql语句，value是该语句的SqlParameter[]）</param>
        public void ExecuteSqlTranWithIdentity(Hashtable sqlStringList)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    var cmd = new SqlCommand();
                    try
                    {
                        var identity = 0;
                        //循环
                        foreach (DictionaryEntry myDe in sqlStringList)
                        {
                            var cmdText = myDe.Key.ToString();
                            var cmdParams = (SqlParameter[])myDe.Value;
                            foreach (var q in cmdParams)
                            {
                                if (q.Direction == ParameterDirection.InputOutput)
                                {
                                    q.Value = identity;
                                }
                            }
                            PrepareCommand(cmd, conn, trans, cmdText, cmdParams);
                            cmd.ExecuteNonQuery();
                            foreach (var q in cmdParams)
                            {
                                if (q.Direction == ParameterDirection.Output)
                                {
                                    identity = Convert.ToInt32(q.Value);
                                }
                            }
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="sqlString">计算查询结果语句</param>
        /// <param name="cmdParams"></param>
        /// <returns>查询结果（object）</returns>
        public object GetSingle(string sqlString, params SqlParameter[] cmdParams)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand())
                {
                    PrepareCommand(cmd, connection, null, sqlString, cmdParams);
                    var obj = cmd.ExecuteScalar();
                    cmd.Parameters.Clear();
                    if ((Equals(obj, null)) || (Equals(obj, DBNull.Value)))
                    {
                        return null;
                    }

                    return obj;
                }
            }
        }

        /// <summary>
        /// 执行查询语句，返回SqlDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="sqlString">查询语句</param>
        /// <param name="cmdParams"></param>
        /// <returns>SqlDataReader</returns>
        public SqlDataReader ExecuteReader(string sqlString, params SqlParameter[] cmdParams)
        {
            var connection = new SqlConnection(_connectionString);
            var cmd = new SqlCommand();
            PrepareCommand(cmd, connection, null, sqlString, cmdParams);
            var myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            cmd.Parameters.Clear();
            return myReader;
        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="sqlString">查询语句</param>
        /// <param name="cmdParams"></param>
        /// <returns>DataSet</returns>
        public DataSet Query(string sqlString, params SqlParameter[] cmdParams)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand();
                PrepareCommand(cmd, connection, null, sqlString, cmdParams);
                using (var da = new SqlDataAdapter(cmd))
                {
                    var ds = new DataSet();
                    try
                    {
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    return ds;
                }
            }
        }

        private void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, string cmdText, SqlParameter[] cmdParams)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParams != null)
            {
                foreach (var parameter in cmdParams)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }

        #endregion

        #region 存储过程操作

        /// <summary>
        /// 执行存储过程，返回SqlDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlDataReader</returns>
        public SqlDataReader RunProcedure(string storedProcName, SqlParameter[] parameters)
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            var command = BuildQueryCommand(connection, storedProcName, parameters);
            command.CommandType = CommandType.StoredProcedure;
            var returnReader = command.ExecuteReader(CommandBehavior.CloseConnection);
            return returnReader;

        }


        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="tableName">DataSet结果中的表名</param>
        /// <returns>DataSet</returns>
        public DataSet RunProcedure(string storedProcName, SqlParameter[] parameters, string tableName)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var dataSet = new DataSet();
                connection.Open();
                var sqlDa = new SqlDataAdapter
                {
                    SelectCommand = BuildQueryCommand(connection, storedProcName, parameters)
                };
                sqlDa.Fill(dataSet, tableName);
                connection.Close();
                return dataSet;
            }
        }
        public DataSet RunProcedure(string storedProcName, SqlParameter[] parameters, string tableName, int times)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var dataSet = new DataSet();
                connection.Open();
                var sqlDa = new SqlDataAdapter
                {
                    SelectCommand = BuildQueryCommand(connection, storedProcName, parameters)
                };
                sqlDa.SelectCommand.CommandTimeout = times;
                sqlDa.Fill(dataSet, tableName);
                connection.Close();
                return dataSet;
            }
        }


        /// <summary>
        /// 构建 SqlCommand 对象(用来返回一个结果集，而不是一个整数值)
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlCommand</returns>
        private SqlCommand BuildQueryCommand(SqlConnection connection, string storedProcName, SqlParameter[] parameters)
        {
            var command = new SqlCommand(storedProcName, connection);
            command.CommandType = CommandType.StoredProcedure;
            foreach (var parameter in parameters)
            {
                if (parameter != null)
                {
                    // 检查未分配值的输出参数,将其分配以DBNull.Value.
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    command.Parameters.Add(parameter);
                }
            }

            return command;
        }

        /// <summary>
        /// 执行存储过程，返回影响的行数		
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="rowsAffected">影响的行数</param>
        /// <returns></returns>
        public int RunProcedure(string storedProcName, SqlParameter[] parameters, out int rowsAffected)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = BuildIntCommand(connection, storedProcName, parameters);
                rowsAffected = command.ExecuteNonQuery();
                var result = (int)command.Parameters["ReturnValue"].Value;
                return result;
            }
        }

        /// <summary>
        /// 创建 SqlCommand 对象实例(用来返回一个整数值)	
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlCommand 对象实例</returns>
        private SqlCommand BuildIntCommand(SqlConnection connection, string storedProcName, SqlParameter[] parameters)
        {
            var command = BuildQueryCommand(connection, storedProcName, parameters);
            command.Parameters.Add(new SqlParameter("ReturnValue",
                SqlDbType.Int, 4, ParameterDirection.ReturnValue,
                false, 0, 0, string.Empty, DataRowVersion.Default, null));
            return command;
        }
        #endregion

        #region 存储过程分页
        public DataSet Query(string tableName, string cols, int page, int pageSize, string filter, string orderBy)
        {
            SqlParameter[] param = {
                                        new SqlParameter("@Table",SqlDbType.NVarChar),
                                        new SqlParameter("@Page",SqlDbType.Int),
                                        new SqlParameter("@PageSize",SqlDbType.Int),
                                        new SqlParameter("@Filter",SqlDbType.NVarChar),
                                        new SqlParameter("@OrderBy",SqlDbType.NVarChar)
                                   };
            param[0].Value = tableName;
            param[1].Value = page;
            param[2].Value = pageSize;
            param[3].Value = filter;
            param[4].Value = orderBy;

            return RunProcedure("GetRecordByPage", param, "RecordList");
        }

        public DataSet Query(string tableName, string cols, string filter, string orderBy)
        {
            var sql = "select " + (cols.Trim() == "" ? "*" : cols) + " from " + tableName + (filter.Trim() == "" ? "" : (" where " + filter)) + (orderBy.Trim() == "" ? "" : (" order by " + orderBy));
            return Query(sql);
        }

        public int GetCount(string tableName, string filter)
        {
            var sql = "select count(1) from " + tableName + (filter == "" ? "" : " where " + filter);
            var o = GetSingle(sql);
            return o == null ? 0 : Convert.ToInt32(o);
        } 
        #endregion
    }
}
