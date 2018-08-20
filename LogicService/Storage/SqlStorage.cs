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

        SqlStorage()
        {
            
        }

        private void Operate()
        {
            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                //Use parameterized query to prevent SQL injection attacks
                insertCommand.CommandText = "INSERT INTO MyTable VALUES (NULL, @Entry);";
                //insertCommand.Parameters.AddWithValue("@Entry", Input_Box.Text);

                try
                {
                    insertCommand.ExecuteReader();
                }
                catch (SqliteException)
                {
                    return;
                }
                db.Close();
            }
        }

        private List<string> Grab_Entries()
        {
            List<string> entries = new List<string>();
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
                    entries.Add(query.GetString(0));
                }
                db.Close();
            }
            return entries;
        }

    }

    public class FileQueue
    {
        public string FileName { get; set; }

        public string Path { get; set; }

        public string User { get; set; }

        public bool Writing { get; set; }
    }
}
