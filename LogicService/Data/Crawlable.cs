﻿using LogicService.Storage;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace LogicService.Data
{
    public class Crawlable
    {

        public string ID { get; set; }

        public string ParentID { get; set; }

        public string Text { get; set; }

        public string Url { get; set; }

        public string Content { get; set; }

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (this.GetType() != obj.GetType())
                return false;

            var one = (Crawlable)obj;
            if (this.ID == one.ID)
                return true;
            else
                return false;
        }

        public static bool operator ==(Crawlable leftHandSide, Crawlable rightHandSide)
        {
            if (ReferenceEquals(leftHandSide, null))
                return ReferenceEquals(rightHandSide, null);
            return (leftHandSide.Equals(rightHandSide));
        }

        public static bool operator !=(Crawlable leftHandSide, Crawlable rightHandSide)
        {
            return !(leftHandSide == rightHandSide);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode(); ;
        }

        #endregion

        #region DB

        public static void DBInitialize()
        {
            string sql = @"create table if not exists [Crawlable] (
                    [ID] varchar(50) not null primary key,
                    [ParentID] varchar(50),
                    [Text] varchar(100) not null,
                    [Url] varchar(100) not null,
                    [Content] varchar(1000));";
            new DataStorage("crawlable").ExecuteWrite(sql);
        }

        public static int DBInsert(List<Crawlable> crawlablelist)
        {
            int affectedRows = 0;
            string sql = @"insert into Crawlable(ID, ParentID, Text, Url, Content)
                values(@ID, @ParentID, @Text, @Url, @Content);";
            DataStorage crawdb = new DataStorage("crawlable");

            foreach (Crawlable crawlable in crawlablelist)
            {
                if (string.IsNullOrEmpty(crawlable.Text) || string.IsNullOrEmpty(crawlable.Url))
                    continue;
                affectedRows = crawdb.ExecuteWrite(sql, new Dictionary<string, object>
                {
                    { "@ID", crawlable.ID },
                    { "@ParentID", crawlable.ParentID },
                    { "@Text", crawlable.Text },
                    { "@Url", crawlable.Url },
                    { "@Content", crawlable.Content }
                }, false);
            }

            FileTrace.DBInsert("Data", crawdb.Database);

            return affectedRows;
        }

        public static List<Crawlable> DBSelectByLimit(int limit)
        {
            string sql = "select * from Crawlable limit @Limit;";
            var reader = new DataStorage("crawlable").ExecuteRead(sql, new Dictionary<string, object> { { "@Limit", limit } });
            return DBReader(reader);
        }

        public static List<Crawlable> DBSelectByID(string id)
        {
            string sql = "select * from Crawlable where ID = @ID;";
            var reader = new DataStorage("crawlable").ExecuteRead(sql, new Dictionary<string, object> { { "@ID", id } });
            return DBReader(reader);
        }

        public static List<Crawlable> DBSelectByPID(string pid)
        {
            string sql = "select * from Crawlable where ParentID = @ParentID;";
            var reader = new DataStorage("crawlable").ExecuteRead(sql, new Dictionary<string, object> { { "@ParentID", pid } });
            return DBReader(reader);
        }

        public static List<Crawlable> DBSelectByContent(string content)
        {
            string sql = "select * from Crawlable where Content like @Content;";
            var reader = new DataStorage("crawlable").ExecuteRead(sql, new Dictionary<string, object> { { "@Content", '%' + content + '%' } });
            return DBReader(reader);
        }

        public static List<Crawlable> DBSelectByTextContent(string text)
        {
            string sql = "select * from Crawlable where Text like @Text or Content like @Text;";
            var reader = new DataStorage("crawlable").ExecuteRead(sql, new Dictionary<string, object> { { "@Text", '%' + text + '%' } });
            return DBReader(reader);
        }

        private static List<Crawlable> DBReader(SqliteDataReader reader)
        {
            List<Crawlable> crawl = new List<Crawlable>();
            if (reader != null)
            {
                while (reader.Read())
                {
                    crawl.Add(new Crawlable
                    {
                        ID = reader.GetString(0),
                        ParentID = reader.GetString(1),
                        Text = reader.GetString(2),
                        Url = reader.GetString(3),
                        Content = reader.GetString(4)
                    });
                }
                reader.Close();
            }
            return crawl;
        }

        public static int DBDeleteByID(string id)
        {
            int affectedRows = 0;
            string sql = "delete from Crawlable where ID = @ID;";
            affectedRows = new DataStorage("crawlable").ExecuteWrite(sql, new Dictionary<string, object> { { "@ID", id } });

            FileTrace.DBInsert("Data", new DataStorage("crawlable").Database);

            return affectedRows;
        }

        #endregion

    }

}
