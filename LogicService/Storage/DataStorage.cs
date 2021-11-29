using LogicService.Application;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

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
                    _dbpath = LocalStorage.GetLocalCacheFolder().Path + "\\" + _dbname;
                    break;
                case DataType.FeedData:
                    _dbname = "feed.db";
                    _dbpath = LocalStorage.GetLocalCacheFolder().Path + "\\" + _dbname;
                    break;
                case DataType.FileTrace:
                    _dbname = "filetrace.db";
                    _dbpath = LocalStorage.GetLocalCacheFolder().Path + "\\" + _dbname;
                    break;
                case DataType.FileList:
                    _dbname = "filelist.db";
                    _dbpath = LocalStorage.GetRoamingFolder().Path + "\\" + _dbname;
                    break;
            }
        }

        enum DataType { CrawlData, PaperData, FeedData, FileTrace, FileList }

        private static DataStorage _crawldata;
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
        private SqliteConnection _conn
        {
            get
            {
                // always new, if needed, define a private con to store old object
                var con = new SqliteConnection(string.Format("Data Source={0};", this._dbpath));
                con.Open();
                return con;
            }
        }

        public string Database => _dbname;

        public string AbsolutePath => _dbpath;

        #endregion

        #region Operations

        /// <summary>
        /// writing process will be recorded by particular DB operation with DBPath
        /// </summary>
        /// <param name="obj"></param>
        public int ExecuteWrite(string sql, Dictionary<string, object> parameters = null, bool ifnotify = true)
        {
            int affectedRows = 0;
            try
            {
                SqliteCommand command = new SqliteCommand(sql, _conn);
                if (parameters != null)
                {
                    command.Parameters.AddRange(SetParameters(parameters));
                }
                affectedRows = command.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                if (ifnotify)
                    ApplicationMessage.SendMessage(new MessageEventArgs { Title = "DatabaseException", Content = ex.Message, Type = MessageType.InApp, Time = DateTimeOffset.Now });
            }

            _conn.Close();
            _conn.Dispose();

            return affectedRows;
        }

        public SqliteDataReader ExecuteRead(string sql, Dictionary<string, object> parameters = null, bool ifnotify = true)
        {
            try
            {
                SqliteCommand command = new SqliteCommand(sql, _conn);
                if (parameters != null)
                {
                    command.Parameters.AddRange(SetParameters(parameters));
                }
                SqliteDataReader reader = command.ExecuteReader();

                _conn.Close();
                _conn.Dispose();

                return reader;
            }
            catch (SqliteException ex)
            {
                if (ifnotify)
                    ApplicationMessage.SendMessage(new MessageEventArgs { Title = "DatabaseException", Content = ex.Message, Type = MessageType.InApp, Time = DateTimeOffset.Now });

                _conn.Close();
                _conn.Dispose();

                return null;
            }
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

        #endregion

    }
}
