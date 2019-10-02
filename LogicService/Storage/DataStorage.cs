using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogicService.Application;

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
                    _dbpath = LocalStorage.TryGetDataPath() + "\\" + _dbname;
                    break;
                case DataType.PaperData:
                    _dbname = "paper.db";
                    _dbpath = LocalStorage.TryGetDataPath() + "\\" + _dbname;
                    break;
                case DataType.FeedData:
                    _dbname = "feed.db";
                    _dbpath = LocalStorage.TryGetDataPath() + "\\" + _dbname;
                    break;
                case DataType.FileTrace:
                    _dbname = ApplicationSetting.AccountName + "filetrace.db";
                    _dbpath = LocalStorage.TryGetLogPath() + "\\" + _dbname;
                    break;
                case DataType.FileList:
                    _dbname = ApplicationSetting.AccountName + "filelist.db";
                    _dbpath = LocalStorage.GetRoamingFolder().Path + "\\" + _dbname;
                    break;
            }
        }

        enum DataType { CrawlData, PaperData, FeedData, FileTrace, FileList }

        private static DataStorage _crawldata;
        private static DataStorage _paperdata;
        private static DataStorage _feeddata;
        private static DataStorage _filetrace;
        private static DataStorage _filelist;

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

        public static DataStorage FeedData
        {
            get
            {
                if (_feeddata == null)
                    _feeddata = new DataStorage(DataType.FeedData);
                return _feeddata;
            }
        }

        public static DataStorage FileTrace
        {
            get
            {
                if (_filetrace == null)
                    _filetrace = new DataStorage(DataType.FileTrace);
                return _filetrace;
            }
        }

        public static DataStorage FileList
        {
            get
            {
                if (_filelist == null)
                    _filelist = new DataStorage(DataType.FileList);
                return _filelist;
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
                if (_conn == null) // always new for multithreads supporting
                    _conn = new SqliteConnection(string.Format("Data Source={0};", this._dbpath));
                if (_conn.State != ConnectionState.Open)
                    _conn.Open();
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
            SqliteCommand command = new SqliteCommand(sql, Connection);

            if (parameters != null)
            {
                command.Parameters.AddRange(SetParameters(parameters));
            }

            affectedRows = command.ExecuteNonQuery();
            
            return affectedRows;
        }

        public SqliteDataReader ExecuteRead(string sql, Dictionary<string, object> parameters = null)
        {
            SqliteCommand command = new SqliteCommand(sql, Connection);

            if (parameters != null)
            {
                command.Parameters.AddRange(SetParameters(parameters));
            }

            return command.ExecuteReader();
        }

        public Array SetParameters(Dictionary<string, object> parameters)
        {
            List<SqliteParameter> sqlite_param = new List<SqliteParameter>();

            foreach (KeyValuePair<string, object> row in parameters)
            {
                sqlite_param.Add(new SqliteParameter(row.Key, row.Value));
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
