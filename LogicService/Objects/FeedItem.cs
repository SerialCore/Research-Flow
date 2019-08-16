﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Syndication;
using LogicService.Storage;
using System.Xml;

namespace LogicService.Objects
{
    public class RSSSource
    {

        public string ID { get; set; }

        public string Name { get; set; }

        public string Uri { get; set; }

        public int MaxCount { get; set; }

        public double Star { get; set; }

        public bool IsJournal { get; set; }
        
        // not used
        public bool IsNotificationOn { get; set; }

        public DateTime LastUpdateTime { get; set; }

    }

    public class FeedItem
    {

        public string ID { get; set; }

        public string ParentID { get; set; }

        public string Title { get; set; }

        public string Published { get; set; }

        public string Link { get; set; }

        public string Summary { get; set; }

        public string FullText { get; set; }

        public string Tags { get; set; }

        public string Nodes { get; set; }

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (this.GetType() != obj.GetType())
                return false;

            var one = (FeedItem)obj;
            if (this.ID == one.ID)
                return true;
            else
                return false;
        }

        public static bool operator ==(FeedItem leftHandSide, FeedItem rightHandSide)
        {
            if (ReferenceEquals(leftHandSide, null))
                return ReferenceEquals(rightHandSide, null);
            return (leftHandSide.Equals(rightHandSide));
        }

        public static bool operator !=(FeedItem leftHandSide, FeedItem rightHandSide)
        {
            return !(leftHandSide == rightHandSide);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode(); ;
        }

        #endregion

        #region DB

        public static void DBOpen()
            => DataStorage.FeedData.Connection.Open();

        public static void DBClose()
            => DataStorage.FeedData.Connection.Close();

        public static void DBInitialize()
        {
            DataStorage.FeedData.Connection.Open();

            string sql = @"create table if not exists [Feed] (
                    [ID] varchar(50) not null primary key,
                    [ParentID] varchar(50) not null,
                    [Title] varchar(100) not null,
                    [Published] varchar(50) not null,
                    [Link] varchar(100) not null,
                    [Summary] varchar(500),
                    [FullText] varchar(1000),
                    [Tags] varchar(500),
                    [Nodes] varchar(1000))";
            DataStorage.FeedData.ExecuteWrite(sql);

            DataStorage.FeedData.Connection.Close();
        }

        public static int DBAppend(FeedItem feed, int max)
        {
            int affectedRows = 0;
            string sql = @"insert into Feed(ID, ParentID, Title, Published, Link, Summary, FullText, Tags, Nodes)
                values(@ID, @ParentID, @Title, @Published, @Link, @Summary, @FullText, @Tags, @Nodes)";
            affectedRows = DataStorage.FeedData.ExecuteWrite(sql, new Dictionary<string, object>
            {
                { "ID", feed.ID },
                { "ParentID", feed.ParentID },
                { "Title", feed.Title },
                { "Published", feed.Published },
                { "Link", feed.Link },
                { "Summary", feed.Summary },
                { "FullText", feed.FullText },
                { "Tags", feed.Tags },
                { "Nodes", feed.Nodes },
            });

            // delete

            return affectedRows;
        }

        public static List<FeedItem> DBSelectByPID(string pid)
        {
            string sql = "select * from Feed where ParentID = @ParentID;";
            var reader = DataStorage.FeedData.ExecuteRead(sql, new Dictionary<string, object> { { "ParentID", pid } });

            List<FeedItem> feeds = new List<FeedItem>();
            while (reader.Read())
            {
                feeds.Add(new FeedItem
                {
                    ID = reader.GetString(0),
                    ParentID = reader.GetString(1),
                    Title = reader.GetString(2),
                    Published = reader.GetString(3),
                    Link = reader.GetString(4),
                    Summary = reader.GetString(5),
                    FullText = reader.GetString(6),
                    Tags = reader.GetString(7),
                    Nodes = reader.GetString(8)
                });
            }
            return feeds;
        }

        public static void DBDelete(string id)
        {
            string sql = "delete from Feed";
            DataStorage.FeedData.ExecuteWrite(sql);
        }

        #endregion

        #region Helper

        public static XmlNodeList GetNodes(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc.DocumentElement.ChildNodes;
        }

        #endregion

    }

}
