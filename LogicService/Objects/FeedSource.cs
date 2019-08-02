﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Syndication;
using LogicService.Storage;

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

        public string Title { get; set; }

        public string Published { get; set; }

        public string Link { get; set; }

        public string Summary { get; set; }

        public string FullText { get; set; }

        public HashSet<string> Tags { get; set; }

        public List<ElementNode> Nodes { get; set; }

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

        public static void DBInitialize()
        {
            DataStorage.CrawlData.Connection.Open();

            string sql = @"CREATE TABLE IF NOT EXISTS [Feed] (
                    [ID] VARCHAR(50) NOT NULL PRIMARY KEY,
                    [Title] VARCHAR(100) NOT NULL,
                    [Published] VARCHAR(50) NOT NULL,
                    [Link] VARCHAR(100) NOT NULL,
                    [Summary] VARCHAR(500)),
                    [FullText] VARCHAR(1000)),
                    [Tags] VARCHAR(500)),
                    [Nodes] VARCHAR(500)),
                    [Tags] VARCHAR(500))";
            DataStorage.CrawlData.ExecuteWrite(sql);

            DataStorage.CrawlData.Connection.Close();
        }

        #endregion

    }

    public class ElementNode
    {

        public string Name { get; set; }

        public string Value { get; set; }

    }

}
