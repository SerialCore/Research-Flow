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

        private DataStorage(DataType type)
        {
            DBSetting(type);
        }

        /// <summary>
        /// only essential parameters
        /// </summary>
        private void DBSetting(DataType type)
        {
            switch (type)
            {
                case DataType.CrawlData:
                    _dbname = "crawlable.db";
                    break;
                case DataType.PaperData:
                    _dbname = "paper.db";
                    break;
            }
            _dbpath = LocalStorage.TryGetDataPath() + "\\" + _dbname;
        }

        enum DataType { CrawlData, PaperData }

        private static DataStorage _crawldata;
        private static DataStorage _paperdata;

        public static DataStorage CrawlData
        {
            get
            {
                if (_crawldata == null)
                    _crawldata = new DataStorage(DataType.CrawlData);
                return _crawldata;
            }
        }

        public static DataStorage PaperData
        {
            get
            {
                if (_paperdata == null)
                    _paperdata = new DataStorage(DataType.PaperData);
                return _paperdata;
            }
        }

        #endregion

        #region Non-Static

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
        public int ExecuteWrite(string sql, Dictionary<string, object> parameters = null)
        {
            int affectedRows = 0;
            SqliteCommand command = new SqliteCommand(sql, _conn);

            if (parameters != null)
            {
                command.Parameters.AddRange(SetParameters(parameters));
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
            SqliteCommand command = new SqliteCommand(sql, _conn);

            return command.ExecuteReader();
        }

        public Array SetParameters(Dictionary<string, object> parameters = null)
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
