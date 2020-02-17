﻿using LogicService.Service;
using LogicService.Storage;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace LogicService.Data
{
    public class FeedSource
    {

        public string ID { get; set; }

        public string Name { get; set; }

        public string Uri { get; set; }

        public double Star { get; set; }

        public bool IsJournal { get; set; }

        //public bool IsNotificationOn { get; set; }

        public DateTime LastUpdateTime { get; set; }

        #region Equals

        // when add a new source
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (this.GetType() != obj.GetType())
                return false;

            var one = (FeedSource)obj;
            if (this.ID == one.ID)
                return true;
            else
                return false;
        }

        public static bool operator ==(FeedSource leftHandSide, FeedSource rightHandSide)
        {
            if (ReferenceEquals(leftHandSide, null))
                return ReferenceEquals(rightHandSide, null);
            return (leftHandSide.Equals(rightHandSide));
        }

        public static bool operator !=(FeedSource leftHandSide, FeedSource rightHandSide)
        {
            return !(leftHandSide == rightHandSide);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode(); ;
        }

        #endregion

    }

    public class Feed
    {

        public string ID { get; set; }

        public string ParentID { get; set; }

        public string Title { get; set; }

        public string Published { get; set; }

        public string Link { get; set; }

        public string Summary { get; set; }

        public string Tags { get; set; }

        public string Nodes { get; set; }

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (this.GetType() != obj.GetType())
                return false;

            var one = (Feed)obj;
            if (this.ID == one.ID)
                return true;
            else
                return false;
        }

        public static bool operator ==(Feed leftHandSide, Feed rightHandSide)
        {
            if (ReferenceEquals(leftHandSide, null))
                return ReferenceEquals(rightHandSide, null);
            return (leftHandSide.Equals(rightHandSide));
        }

        public static bool operator !=(Feed leftHandSide, Feed rightHandSide)
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
            string sql = @"create table if not exists [Feed] (
                    [ID] varchar(50) not null primary key,
                    [ParentID] varchar(50) not null,
                    [Title] varchar(100) not null,
                    [Published] varchar(50) not null,
                    [Link] varchar(100) not null,
                    [Summary] varchar(500),
                    [Tags] varchar(500),
                    [Nodes] varchar(1000));";
            DataStorage.FeedData.ExecuteWrite(sql);
        }

        public static int DBInsert(List<Feed> feeds)
        {
            DBDeleteByPID(feeds[0].ParentID);

            int affectedRows = 0;
            string sql = @"insert into Feed(ID, ParentID, Title, Published, Link, Summary, Tags, Nodes)
                values(@ID, @ParentID, @Title, @Published, @Link, @Summary, @Tags, @Nodes);";
            foreach (Feed feed in feeds)
            {
                affectedRows += DataStorage.FeedData.ExecuteWrite(sql, new Dictionary<string, object>
                {
                    { "@ID", feed.ID },
                    { "@ParentID", feed.ParentID },
                    { "@Title", feed.Title },
                    { "@Published", feed.Published },
                    { "@Link", feed.Link },
                    { "@Summary", feed.Summary },
                    { "@Tags", feed.Tags },
                    { "@Nodes", feed.Nodes },
                });
            }

            FileList.DBInsertList("Data", DataStorage.FeedData.Database);
            FileList.DBInsertTrace("Data", DataStorage.FeedData.Database);

            return affectedRows;
        }

        public static List<Feed> DBSelectByLimit(int limit)
        {
            string sql = "select * from Feed limit @Limit;";
            var reader = DataStorage.FeedData.ExecuteRead(sql, new Dictionary<string, object> { { "@Limit", limit } });
            return DBReader(reader);
        }

        public static List<Feed> DBSelectByPID(string pid)
        {
            string sql = "select * from Feed where ParentID = @ParentID;";
            var reader = DataStorage.FeedData.ExecuteRead(sql, new Dictionary<string, object> { { "@ParentID", pid } });
            return DBReader(reader);
        }

        public static List<Feed> DBSelectByTag(string tag)
        {
            string sql = "select * from Feed where Tags like @Tags;";
            var reader = DataStorage.FeedData.ExecuteRead(sql, new Dictionary<string, object> { { "@Tags", '%' + tag + '%' } });
            return DBReader(reader);
        }

        private static List<Feed> DBReader(SqliteDataReader reader)
        {
            List<Feed> feeds = new List<Feed>();
            while (reader.Read())
            {
                feeds.Add(new Feed
                {
                    ID = reader.GetString(0),
                    ParentID = reader.GetString(1),
                    Title = reader.GetString(2),
                    Published = reader.GetString(3),
                    Link = reader.GetString(4),
                    Summary = reader.GetString(5),
                    Tags = reader.GetString(6),
                    Nodes = reader.GetString(7)
                });
            }
            return feeds;
        }

        public static int DBDeleteByPID(string pid)
        {
            int affectedRows = 0;
            string sql = "delete from Feed where ParentID = @ParentID;";
            affectedRows = DataStorage.FeedData.ExecuteWrite(sql, new Dictionary<string, object> { { "@ParentID", pid } });

            FileList.DBInsertList("Data", DataStorage.FeedData.Database);
            FileList.DBInsertTrace("Data", DataStorage.FeedData.Database);

            return affectedRows;
        }

        public static void DBUpdate()
        {

        }

        #endregion

        #region Helper

        public static XmlNodeList GetNodes(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc.DocumentElement.ChildNodes;
        }

        public static string GetDoi(string xml)
        {
            foreach (XmlNode node in GetNodes(xml))
            {
                if (node.Name.Equals("doi"))
                    return node.Value;
            }
            return "";
        }

        public static string GetAuthor(string xml)
        {
            foreach (XmlNode node in GetNodes(xml))
            {
                if (node.Name.Equals("author"))
                    return node.Value;
            }
            return "";
        }

        public static string GetCategory(string xml)
        {
            foreach (XmlNode node in GetNodes(xml))
            {
                if (node.Name.Equals("category"))
                    return node.Value;
            }
            return "";
        }

        #endregion

        #region Task

        public static void TaskRun()
        {
            // thread security
            List<FeedSource> FeedSources = Task.Run(async () =>
            {
                return await LocalStorage.ReadJsonAsync<List<FeedSource>>(await LocalStorage.GetDataFolderAsync(), "rss.list");
            }).Result;

            foreach (FeedSource source in FeedSources)
            {
                RssService.GetRssItems(
                    source.Uri,
                    (items) =>
                    {
                        List<Feed> feeds = items as List<Feed>;
                        DBInsert(feeds);
                        source.LastUpdateTime = DateTime.Now;
                        Task.Run(async () =>
                        {
                            LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "rss.list", FeedSources);
                        });

                        Task.Run(() =>
                        {
                            LocalStorage.GeneralLogAsync<Feed>("FeedTask.log",
                                "feed updated-" + source.Name);
                        });

                    },
                    (exception) =>
                    {
                        Task.Run(() =>
                        {
                            LocalStorage.GeneralLogAsync<RssService>("FeedTask.log",
                                exception + "-" + source.Name);
                        });
                    }, null);
            }
        }

        #endregion

    }
}