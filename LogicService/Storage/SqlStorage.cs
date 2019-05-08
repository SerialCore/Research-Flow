using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace LogicService.Storage
{
    public class SqlStorage
    {

        public SqlStorage()
        {

        }

        public void RunCommand(string dbname, string command)
        {
            using (SqliteConnection db = new SqliteConnection("Filename=" + dbname))
            {
                db.Open();

                SqliteCommand sqlcommand = new SqliteCommand(command,db);
                try
                {
                    sqlcommand.ExecuteReader();
                }
                catch (SqliteException)
                {
                    return;
                }

                db.Close();
            }
        }

        public List<T> GetEntries<T>() where T : class
        {
            List<T> entries = new List<T>();
            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand("SELECT Text_Entry from MyTable", db);
                SqliteDataReader query;
                try
                {
                    query = selectCommand.ExecuteReader();
                }
                catch (SqliteException)
                {
                    return entries;
                }
                while (query.Read())
                {
                    //entries.Add(query.GetValue(0));
                }

                db.Close();
            }
            return entries;
        }

    }

}
