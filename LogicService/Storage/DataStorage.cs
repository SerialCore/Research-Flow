using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicService.Storage
{
    public class DataStorage
    {
        public static string GetCrawlerPath()
        {
            return "";
        }

        public static void InitDatabase<T>() where T : class
        {
            using (SQLiteConnection connection = new SQLiteConnection("sqliteSample.db"))
            {
                connection.CreateTable<T>();
            }
        }

        public static void DataWrite(string inputText)
        {
            using (SQLiteConnection db = new SQLiteConnection("sqliteSample.db"))
            {

            }
        }

        public static List<string> DataRead()
        {
            List<string> entries = new List<string>();

            using (SQLiteConnection db = new SQLiteConnection("sqliteSample.db"))
            {

            }

            return entries;
        }

        public static void DataDelete()
        {

        }
    }
}
