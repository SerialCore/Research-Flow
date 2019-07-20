using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicService.Storage
{
    public class DataStorage
    {

        #region Static

        private static DataStorage _crawldata;

        private DataStorage()
        {
            DBSetting();
        }

        /// <summary>
        /// only essential parameters
        /// </summary>
        private void DBSetting()
        {
            _dbname = "crawlable.db";
            _dbpath = LocalStorage.TryGetDataPath() + "\\" + _dbname;
        }

        public static DataStorage CrawlData
        {
            get
            {
                if (_crawldata == null)
                    _crawldata = new DataStorage();
                return _crawldata;
            }
        }

        public static string CMDCreateCrawlable
            => @"CREATE TABLE IF NOT EXISTS [Crawlable] (
                    [ID] VARCHAR(50) NOT NULL PRIMARY KEY,
                    [ParentID] VARCHAR(50),
                    [Text] VARCHAR(50) NOT NULL,
                    [Url] VARCHAR(100) NOT NULL,
                    [LinkTarget] VARCHAR(20),
                    [Content] VARCHAR(1000))";

        public static string CMDInsert
            => @"INSERT INTO Users(Username, Email, Password)
                VALUES('admin', 'testing@gmail.com', 'test')";

        public static string CMDSelect
            => "SELECT * From Users WHERE Id = @UserId;";

        public static string CMDDelete
            => "DELETE FROM Users";

        #endregion

        #region Settings

        private string _dbname;
        private string _dbpath;
        private SqliteConnection _conn;

        /// <summary>
        /// current connection
        /// </summary>
        public SqliteConnection Connection
        {
            get
            {
                if (_conn == null)
                    _conn = new SqliteConnection(string.Format("Data Source={0};", this._dbpath));
                return _conn;
            }
        }

        public string Database
        {
            get
            {
                return _dbname;
            }
        }

        public string AbsolutePath
        {
            get
            {
                return _dbpath;
            }
        }

        #endregion

        #region Operations

        /// <summary>
        /// will be recorded
        /// </summary>
        /// <param name="obj"></param>
        public int ExecuteWrite(string sql, SqliteParameter[] parameters)
        {
            int affectedRows = 0;
            SqliteCommand command = new SqliteCommand(sql, _conn);

            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            DbTransaction transaction = _conn.BeginTransaction();
            affectedRows = command.ExecuteNonQuery();
            transaction.Commit();

            LocalStorage.AddFileList("Data", _dbname);
            LocalStorage.AddFileTrace("Data", _dbname);

            return affectedRows;
        }

        public SqliteDataReader ExecuteRead(string sql)
        {
            List<string> entries = new List<string>();

            SqliteCommand command = new SqliteCommand(sql, _conn);

            return command.ExecuteReader();
        }

        public Array SetParameters(string sql, Dictionary<string, object> parameters = null)
        {
            List<SqliteParameter> sqlite_param = new List<SqliteParameter>();
            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> row in parameters)
                {
                    sqlite_param.Add(new SqliteParameter(row.Key, row.Value));
                }
            }
            return sqlite_param.ToArray();
        }
        
        public DataTable GetSchema()
        {
            return _conn.GetSchema("TABLES");
        }

        #endregion

    }
}
