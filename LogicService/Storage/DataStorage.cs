using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicService.Storage
{
    public class DataStorage
    {
        /// <summary>
        /// will be recorded
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private static void InitDatabase()
        {
            using (SqliteConnection connection = new SqliteConnection("Data Source=crawlable.db"))
            {
                connection.Open();

                string sql = @"CREATE TABLE IF NOT EXISTS [Crawlable] (
                    [ID] VARCHAR(50) NOT NULL PRIMARY KEY,
                    [ParentID] VARCHAR(50),
                    [Text] VARCHAR(50) NOT NULL,
                    [Url] VARCHAR(100) NOT NULL,
                    [LinkTarget] VARCHAR(20),
                    [Content] VARCHAR(1000))";
                SqliteCommand command = new SqliteCommand(sql, connection);
                command.ExecuteNonQuery();

                connection.Close();
            }
            //LocalStorage.AddFileList("Data", "crawlable.db");
            //LocalStorage.AddFileTrace("Data", "crawlable.db");
        }

        /// <summary>
        /// will be recorded
        /// </summary>
        /// <param name="obj"></param>
        public void DataWrite(object obj)
        {
            using (SqliteConnection connection = new SqliteConnection("Data Source=crawlable.db"))
            {
                connection.Open();

                string sql = "insert into Crawlable values(@ImageList)";
                SqliteCommand command = new SqliteCommand(sql, connection);
                command.Parameters.Add("ImageList", SqliteType.Text);
                command.Parameters["ImageList"].Value = "";
                command.ExecuteNonQuery();

                connection.Close();
            }
        }

        public DataTable DataReadTable<T>() where T : class
        {
            DataTable data = new DataTable();

            using (SqliteConnection connection = new SqliteConnection("Data Source=crawlable.db"))
            {
                connection.Open();

                string sql = "insert into Crawlable values(@ImageList)";
                SqliteCommand command = new SqliteCommand(sql, connection);
                var result = command.ExecuteReader();

                connection.Close();
            }

            return data;
        }

        public static DataStorage CrawlData
        {
            get
            {
                if (_crawldata != null)
                    return _crawldata;
                else
                    return _crawldata = new DataStorage();
            }
        }

        private static DataStorage _crawldata;
        private string _dbpath;
        private SqliteConnection _conn;

        /// <summary>
        /// SQLite连接
        /// </summary>
        private SqliteConnection conn
        {
            get
            {
                if (_conn == null)
                {
                    _conn = new SqliteConnection(
                        string.Format("Data Source={0};Version=3;",
                        this._dbpath));
                    _conn.Open();
                }
                return _conn;
            }
        }

        public DataStorage()
        {

        }
        
        /// <summary>
        /// 获取多行
        /// </summary>
        /// <param name="sql">执行sql</param>
        /// <param name="param">sql参数</param>
        /// <returns>多行结果</returns>
        public DataRow[] getRows(string sql, Dictionary<string, object> param=null)
        {
            List<SqliteParameter> sqlite_param = new List<SqliteParameter>();
            if (param != null)
            {
                foreach (KeyValuePair<string, object> row in param)
                {
                    sqlite_param.Add(new SqliteParameter(row.Key, row.Value.ToString()));
                }
            }
            DataTable dt = this.ExecuteDataTable(sql, sqlite_param.ToArray());
            return dt.Select();
        }
        
        /// <summary>
        /// 获取单行
        /// </summary>
        /// <param name="sql">执行sql</param>
        /// <param name="param">sql参数</param>
        /// <returns>单行数据</returns>
        public DataRow getRow(string sql, Dictionary<string, object> param=null)
        {
            DataRow[] rows = this.getRows(sql, param);
            return rows[0];
        }
        
        /// <summary>
        /// 获取字段
        /// </summary>
        /// <param name="sql">执行sql</param>
        /// <param name="param">sql参数</param>
        /// <returns>字段数据</returns>
        public Object getOne(string sql, Dictionary<string, object> param=null)
        {
            DataRow row = this.getRow(sql, param);
            return row[0];
        }
        
        /// <summary>
        /// SQLite增删改
        /// </summary>
        /// <param name="sql">要执行的sql语句</param>
        /// <param name="parameters">所需参数</param>
        /// <returns>所受影响的行数</returns>
        public int query(string sql, Dictionary<string, object> param = null)
        {
            List<SqliteParameter> sqlite_param = new List<SqliteParameter>();
            if (param != null)
            {
                foreach (KeyValuePair<string, object> row in param)
                {
                    sqlite_param.Add(new SqliteParameter(row.Key, row.Value.ToString()));
                }
            }
            return this.ExecuteNonQuery(sql, sqlite_param.ToArray());
        }
        
        /// <summary> 
        /// SQLite增删改
        /// </summary>
        /// <param name="sql">要执行的sql语句</param>
        /// <param name="parameters">所需参数</param>
        /// <returns>所受影响的行数</returns>
        private int ExecuteNonQuery(string sql, SqliteParameter[] parameters)
        {
            int affectedRows = 0;
            System.Data.Common.DbTransaction transaction = conn.BeginTransaction();
            SqliteCommand command = new SqliteCommand(sql, _conn);
            command.CommandText = sql;
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }
            affectedRows = command.ExecuteNonQuery();
            transaction.Commit();
            return affectedRows;
        }
        
        /// <summary>
        /// SQLite查询
        /// </summary>
        /// <param name="sql">要执行的sql语句</param>
        /// <param name="parameters">所需参数</param>
        /// <returns>结果DataTable</returns>
        private DataTable ExecuteDataTable(string sql, SqliteParameter[] parameters)
        {
            DataTable data = new DataTable();
            SqliteCommand command = new SqliteCommand(sql, conn);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }
            return data;
        }
        
        /// <summary>
        /// 查询数据库表信息
        /// </summary>
        /// <returns>数据库表信息DataTable</returns>
        public DataTable GetSchema()
        {
            DataTable data = new DataTable();
            data = conn.GetSchema("TABLES");
            return data;
        }

    }
}
