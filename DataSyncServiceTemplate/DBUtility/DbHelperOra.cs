using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Configuration;
using System.Data.OracleClient;



namespace DBUtility
{
    /// <summary>
    /// Copyright (C) Maticsoft
    /// 数据访问基础类(基于Oracle)
    /// 可以用户可以修改满足自己项目的需要。
    /// </summary>
    public class DbHelperOra
    {
        //数据库连接字符串(web.config来配置)，可以动态更改connectionString支持多数据库.		
        private string connectionString = null;

        public DbHelperOra()
        {
            connectionString = DESEncrypt.Decrypt(PubConstant.GetConnectionString("TargetDBConn"));
        }

        public DbHelperOra(string ConnName)
        {
            this.connectionString = PubConstant.GetConnectionString(ConnName);
        }

        /// <summary>
        /// 判断服务器数据库是否能成功连接
        /// </summary>
        /// <returns></returns>
        public bool TestConnection(ref string msg)
        {
            bool retValue = false;
            try
            {
                OracleConnection conn = new OracleConnection(connectionString);
                conn.Open();
                if (conn.State == ConnectionState.Open)
                {
                    retValue = true;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                msg = ex.ToString();
                retValue = false;
            }
            return retValue;
        }


        #region  执行简单SQL语句

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteSql(string SQLString)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                using (OracleCommand cmd = new OracleCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (Exception E)
                    {
                        connection.Close();
                        throw new Exception(E.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public DataSet Query(string SQLString)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    OracleDataAdapter command = new OracleDataAdapter(SQLString, connection);
                    command.Fill(ds, "ds");
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                return ds;
            }
        }


        #endregion


        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>		
        public void ExecuteSqlTran(ArrayList SQLStringList)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                conn.Open();
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = conn;
                OracleTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    for (int n = 0; n < SQLStringList.Count; n++)
                    {
                        string strsql = SQLStringList[n].ToString();
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = strsql;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                }
                catch (Exception E)
                {
                    tx.Rollback();
                    Console.WriteLine("执行ExecuteSqlTran出现错误，详细错误为：" + E.ToString());
                    //throw new Exception(E.Message);
                }
                finally
                {
                    if (conn.State != ConnectionState.Closed)
                    {
                        conn.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>		
        public void ExecuteSqlTran(ArrayList SQLStringList, int batch)
        {
            ArrayList sqlList = new ArrayList();
            for (int n = 0; n < SQLStringList.Count; n++)
            {
                sqlList.Add(SQLStringList[n]);
                if ((n + 1) % batch == 0)
                {
                    ExecuteSqlTran(sqlList);
                    sqlList.Clear();
                }
                else if ((n + 1) == SQLStringList.Count)
                {
                    ExecuteSqlTran(sqlList);
                    sqlList.Clear();
                }
            }
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public object GetSingle(string SQLString)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                using (OracleCommand cmd = new OracleCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (Exception e)
                    {
                        connection.Close();
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        if (connection.State != ConnectionState.Closed)
                        {
                            connection.Close();
                        }
                    }
                }
            }
        }

    }
}
