using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;

namespace io.borgle.Core.Helper
{
    public class SqliteHelper
    {

        #region Fields

        /// <summary>
        /// 连接串
        /// </summary>
        private string connectionString = "";

        #endregion Fields

        #region Constructors

        /// <summary>     
        /// 构造函数
        /// </summary>     
        /// <param name="path">SQLite数据库文件路径</param>     
        public SqliteHelper(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbPath">SQLite数据库文件路径</param>
        /// <param name="_password">密码</param>
        public SqliteHelper(string dbPath, string password)
        {
            //如果不存在目录，只是文件名，则指定连接到当前应用程序启动目录
            if (!File.Exists(dbPath))
            {
                dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + Path.GetFileName(dbPath));
            }
            this.connectionString = "Data Source=" + dbPath + ";Pooling=true;FailIfMissing=false;Password=" + password;
        }

        #endregion Constructors

        #region Properties

        public string ConnectionString
        {
            get { return this.connectionString; }
            set { this.connectionString = value; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// 预处理SQL参数
        /// </summary>
        /// <param name="cmd">SqlCommand</param>
        /// <param name="conn">SqlConnection</param>
        /// <param name="cmdText">cmdText</param>
        /// <param name="parameters">参数列表</param>
        private void PrepareCommand(SQLiteCommand cmd, SQLiteConnection conn, string cmdText, params object[] parameters)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Parameters.Clear();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 30;
            if (parameters != null)
            {
                if (parameters.Length % 2 == 0)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        SQLiteParameter sp = new SQLiteParameter(parameters[i].ToString(), parameters[i + 1]);
                        cmd.Parameters.Add(sp);
                        i++;
                    }
                }
            }
        }

        /// <summary>     
        /// 对SQLite数据库执行批量增删改操作，返回受影响的总行数。
        /// 该操作包含事务
        /// </summary>     
        /// <param name="batchSqls">批量执行的增删改SQL语句的列表</param>       
        /// <returns></returns>     
        public int ExecuteBatchNonQuery(IList<string> batchSqls)
        {
            return ExecuteBatchNonQuery(batchSqls, null);
        }

        /// <summary>     
        /// 对SQLite数据库执行批量增删改操作，返回受影响的总行数。
        /// 该操作包含事务
        /// </summary>     
        /// <param name="batchSqls">批量执行的增删改SQL语句的列表</param>     
        /// <param name="batchParameters">批量执行增删改语句所需要的所有参数，参数必须以它们在SQL语句中的顺序为准；1个SQL语句对应多个参数</param>     
        /// <returns></returns>     
        public int ExecuteBatchNonQuery(IList<string> batchSqls, IList<IList<SQLiteParameter>> batchParameters)
        {
            if (batchSqls == null || batchSqls.Count == 0)
                throw new ArgumentNullException("batchSqls");

            int affectedRows = 0;
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (DbTransaction transaction = connection.BeginTransaction())
                {
                    string sql = null;
                    //每个sql对应的参数列表
                    IList<SQLiteParameter> parameters = null;
                    //循环批量的sql列表
                    for (int i = 0; i < batchSqls.Count; i++)
                    {
                        //每次都情况参数列表
                        parameters = null;

                        //获取sql
                        sql = batchSqls[i];
                        if (string.IsNullOrWhiteSpace(sql))
                        {
                            continue;
                        }

                        //如果有对应的参数列表，就获取
                        if (batchParameters != null && i < batchParameters.Count)
                        {
                            parameters = batchParameters[i];
                        }

                        //执行命令
                        using (SQLiteCommand command = new SQLiteCommand(connection))
                        {
                            command.CommandText = sql;
                            //添加参数
                            if (parameters != null && parameters.Count != 0)
                            {
                                foreach (SQLiteParameter param in parameters)
                                {
                                    if (param == null)
                                    {
                                        continue;
                                    }

                                    command.Parameters.Add(param);
                                }
                            }
                            affectedRows += command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
            }
            return affectedRows;
        }
        /// <summary>     
        /// 对SQLite数据库执行批量操作，返回受影响的总行数。
        /// 该操作包含事务
        /// </summary>     
        /// <param name="oneSql">重复执行的SQL语句</param>     
        /// <param name="batchParameters">重复执行增删改语句所需要的不同参数，参数必须以它们在SQL语句中的顺序为准</param>     
        /// <returns></returns>  
        public int ExecuteBatchNonQuery(string oneSql, IList<IList<SQLiteParameter>> batchParameters)
        {
            if (string.IsNullOrWhiteSpace(oneSql))
                throw new ArgumentNullException("oneSql");

            int affectedRows = 0;
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (DbTransaction transaction = connection.BeginTransaction())
                {
                    //每个sql对应的参数列表
                    IList<SQLiteParameter> parameters = null;
                    //循环批量的batchParameters列表
                    for (int i = 0; i < batchParameters.Count; i++)
                    {
                        //每次都情况参数列表
                        parameters = batchParameters[i];

                        //执行命令
                        using (SQLiteCommand command = new SQLiteCommand(connection))
                        {
                            command.CommandText = oneSql;
                            //添加参数
                            if (parameters != null && parameters.Count != 0)
                            {
                                foreach (SQLiteParameter param in parameters)
                                {
                                    if (param == null)
                                    {
                                        continue;
                                    }

                                    command.Parameters.Add(param);
                                }
                            }
                            affectedRows += command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
            }
            return affectedRows;
        }

        /// <summary>
        /// 查询操作，返回数据行
        /// </summary>
        /// <param name="selectSql">查询sql</param>    
        /// <returns>返回数据行</returns>
        public DataRow ExecuteDataRow(string sql)
        {
            List<SQLiteParameter> parameters = null;
            return ExecuteDataRow(sql, parameters);
        }

        /// <summary>
        /// 查询操作，返回数据行
        /// </summary>
        /// <param name="selectSql">查询sql</param>
        /// <param name="parameters">执行查询sql所需要的参数，参数必须以它们在SQL语句中的顺序为准</param>     
        /// <returns>返回数据行</returns>
        public DataRow ExecuteDataRow(string sql, IList<SQLiteParameter> parameters)
        {
            DataSet ds = ExecuteDataSet(sql, parameters);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                return ds.Tables[0].Rows[0];
            return null;
        }

        /// <summary>
        /// 查询操作，返回数据集
        /// </summary>
        /// <param name="selectSql">查询sql</param>
        /// <returns>返回数据集</returns>
        public DataSet ExecuteDataSet(string sql)
        {
            List<SQLiteParameter> parameters = null;
            return ExecuteDataSet(sql, parameters);
        }

        /// <summary>
        /// 查询操作，返回数据集
        /// </summary>
        /// <param name="selectSql">查询sql</param>
        /// <param name="parameters">执行查询sql所需要的参数，参数必须以它们在SQL语句中的顺序为准</param>     
        /// <returns>返回数据集</returns>
        public DataSet ExecuteDataSet(string sql, IList<SQLiteParameter> parameters)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    if (!(parameters == null || parameters.Count == 0))
                    {
                        foreach (SQLiteParameter parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }
                    }
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                    DataSet data = new DataSet();
                    adapter.Fill(data);
                    return data;
                }
            }
        }

        /// <summary>
        /// 查询操作，返回数据表
        /// </summary>
        /// <param name="selectSql">查询sql</param> 
        /// <returns>返回数据表</returns>
        public DataTable ExecuteDataTable(string sql)
        {
            List<SQLiteParameter> parameters = null;
            return ExecuteDataTable(sql, parameters);
        }

        /// <summary>
        /// 查询操作，返回数据表
        /// </summary>
        /// <param name="selectSql">查询sql</param>
        /// <param name="parameters">执行查询sql所需要的参数，参数必须以它们在SQL语句中的顺序为准</param>     
        /// <returns>返回数据表</returns>
        public DataTable ExecuteDataTable(string sql, IList<SQLiteParameter> parameters)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    if (!(parameters == null || parameters.Count == 0))
                    {
                        foreach (SQLiteParameter parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }
                    }
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                    DataTable data = new DataTable();
                    adapter.Fill(data);
                    return data;
                }
            }
        }

        /// <summary>
        /// 查询操作，返回数据读取器
        /// </summary>
        /// <param name="selectSql">查询sql</param>  
        /// <returns>返回数据读取器</returns>
        public SQLiteDataReader ExecuteReader(string sql)
        {
            return ExecuteReader(sql, null);
        }

        /// <summary>
        /// 查询操作，返回数据读取器
        /// </summary>
        /// <param name="selectSql">查询sql</param>
        /// <param name="parameters">执行查询sql所需要的参数，参数必须以它们在SQL语句中的顺序为准</param>     
        /// <returns>返回数据读取器</returns>
        public SQLiteDataReader ExecuteReader(string sql, IList<SQLiteParameter> parameters)
        {
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            if (!(parameters == null || parameters.Count == 0))
            {
                foreach (SQLiteParameter parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
            }
            connection.Open();
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        /// <summary>     
        /// 对SQLite数据库执行增删改操作，返回受影响的行数。     
        /// </summary>     
        /// <param name="sql">要执行的增删改的SQL语句</param>      
        /// <returns></returns>  
        public int ExecuteNonQuery(string sql)
        {
            return ExecuteNonQuery(sql, null);
        }

        /// <summary>     
        /// 对SQLite数据库执行增删改操作，返回受影响的行数。     
        /// </summary>     
        /// <param name="sql">要执行的增删改的SQL语句</param>     
        /// <param name="parameters">执行增删改语句所需要的参数，参数必须以它们在SQL语句中的顺序为准</param>     
        /// <returns>返回受影响的行数</returns>   
        public int ExecuteNonQuery(string sql, IList<SQLiteParameter> parameters)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException("sql");

            int affectedRows = 0;
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (DbTransaction transaction = connection.BeginTransaction())
                {
                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = sql;
                        if (!(parameters == null || parameters.Count == 0))
                        {
                            foreach (SQLiteParameter parameter in parameters)
                            {
                                command.Parameters.Add(parameter);
                            }
                        }
                        affectedRows = command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
            return affectedRows;
        }

        /// <summary>
        /// 查询操作，返回单个数据值
        /// </summary>
        /// <param name="selectSql">查询sql</param>    
        /// <returns>返回单个数据值</returns>
        public object ExecuteScalar(string sql)
        {
            return ExecuteScalar(sql, null);
        }

        /// <summary>
        /// 查询操作，返回单个数据值
        /// </summary>
        /// <param name="selectSql">查询sql</param>
        /// <param name="parameters">执行查询sql所需要的参数，参数必须以它们在SQL语句中的顺序为准</param>     
        /// <returns>返回单个数据值</returns>
        public object ExecuteScalar(string sql, IList<SQLiteParameter> parameters)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    if (!(parameters == null || parameters.Count == 0))
                    {
                        foreach (SQLiteParameter parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }
                    }
                    return command.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// 获取数据表的结构
        /// </summary>
        /// <returns></returns>
        public DataTable GetSchema()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                DataTable data = connection.GetSchema("TABLES");
                connection.Close();
                return data;
            }
        }

        #endregion Methods
    }
}
