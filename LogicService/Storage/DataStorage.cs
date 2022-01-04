using LogicService.Application;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace LogicService.Storage
{
    public class DataStorage
    {

        public DataStorage(string dbname)
        {
            this._dbname = dbname + ".db";
            this._dbpath = LocalStorage.GetLocalCacheFolder().Path + "\\" + this._dbname;
        }

        private string _dbname;
        private string _dbpath;
        private SqliteConnection _conn
        {
            get
            {
                var con = new SqliteConnection(string.Format("Data Source={0};", this._dbpath));
                con.Open();
                return con;
            }
        }

        public string Database => _dbname;

        public string AbsolutePath => _dbpath;

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

    }
}
