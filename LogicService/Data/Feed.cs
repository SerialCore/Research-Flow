using LogicService.Storage;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace LogicService.Data
{
    public class FeedSource
    {

        public string ID { get; set; }

        public string Name { get; set; }

        public string Uri { get; set; }

        public double Star { get; set; }

        public bool IsRSS { get; set; }

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

        public string ArticleID { get; set; }

        public string Title { get; set; }

        public string Authors { get; set; }

        public string Published { get; set; }

        public string Link { get; set; }

        public string Summary { get; set; }

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
                    [ArticleID] varchar(100),
                    [Title] varchar(100) not null,
                    [Authors] varchar(100),
                    [Published] varchar(50),
                    [Link] varchar(100),
                    [Summary] varchar(500));";
            string sql2 = @"create table if not exists [Bookmark] (
                    [ID] varchar(50) not null primary key,
                    [ParentID] varchar(50) not null,
                    [ArticleID] varchar(100),
                    [Title] varchar(100) not null,
                    [Authors] varchar(100),
                    [Published] varchar(50),
                    [Link] varchar(100),
                    [Summary] varchar(500));";
            DataStorage.FeedData.ExecuteWrite(sql);
            DataStorage.FeedData.ExecuteWrite(sql2);
        }

        public static int DBInsert(List<Feed> feeds)
        {
            int affectedRows = 0;
            string sql = @"insert into Feed(ID, ParentID, ArticleID, Title, Authors, Published, Link, Summary)
                values(@ID, @ParentID, @ArticleID, @Title, @Authors, @Published, @Link, @Summary);";
            foreach (Feed feed in feeds)
            {
                affectedRows += DataStorage.FeedData.ExecuteWrite(sql, new Dictionary<string, object>
                {
                    { "@ID", feed.ID },
                    { "@ParentID", feed.ParentID },
                    { "@ArticleID", feed.ArticleID },
                    { "@Title", feed.Title },
                    { "@Authors", feed.Authors },
                    { "@Published", feed.Published },
                    { "@Link", feed.Link },
                    { "@Summary", feed.Summary }
                }, false);
            }

            FileList.DBInsertList("Data", DataStorage.FeedData.Database);
            FileList.DBInsertTrace("Data", DataStorage.FeedData.Database);

            return affectedRows;
        }

        public static int DBInsertBookmark(Feed feed)
        {
            int affectedRows = 0;
            string sql = @"insert into Bookmark(ID, ParentID, ArticleID, Title, Authors, Published, Link, Summary)
                values(@ID, @ParentID, @ArticleID, @Title, @Authors, @Published, @Link, @Summary);";
            affectedRows += DataStorage.FeedData.ExecuteWrite(sql, new Dictionary<string, object>
                {
                    { "@ID", feed.ID },
                    { "@ParentID", feed.ParentID },
                    { "@ArticleID", feed.ArticleID },
                    { "@Title", feed.Title },
                    { "@Authors", feed.Authors },
                    { "@Published", feed.Published },
                    { "@Link", feed.Link },
                    { "@Summary", feed.Summary }
                }, false);

            FileList.DBInsertList("Data", DataStorage.FeedData.Database);
            FileList.DBInsertTrace("Data", DataStorage.FeedData.Database);

            return affectedRows;
        }

        public static List<Feed> DBSelectByPID(string pid, int limit = 100)
        {
            string sql = "select * from Feed where ParentID = @ParentID order by Published desc limit @Limit;";
            var reader = DataStorage.FeedData.ExecuteRead(sql, new Dictionary<string, object> { { "@Limit", limit }, { "@ParentID", pid } });
            return DBReader(reader);
        }

        public static List<Feed> DBSelectByTitle(string pattern, int limit = 100)
        {
            string sql = "select * from Feed where Title like @Pattern order by Published desc limit @Limit;";
            var reader = DataStorage.FeedData.ExecuteRead(sql, new Dictionary<string, object> { { "@Pattern", '%' + pattern + '%' }, { "@Limit", limit } });
            return DBReader(reader);
        }

        public static List<Feed> DBSelectByText(string pattern, int limit = 100)
        {
            string sql = "select * from Feed where Title like @Pattern or Summary like @Pattern order by Published desc limit @Limit;";
            var reader = DataStorage.FeedData.ExecuteRead(sql, new Dictionary<string, object> { { "@Pattern", '%' + pattern + '%' }, { "@Limit", limit } });
            return DBReader(reader);
        }

        public static List<Feed> DBSelectByAuthor(string pattern, int limit = 100)
        {
            string sql = "select * from Feed where Authors like @Pattern or Summary like @Pattern order by Published desc limit @Limit;";
            var reader = DataStorage.FeedData.ExecuteRead(sql, new Dictionary<string, object> { { "@Pattern", '%' + pattern + '%' }, { "@Limit", limit } });
            return DBReader(reader);
        }

        public static List<Feed> DBSelectByPublished(string pattern, int limit = 100)
        {
            string sql = "select * from Feed where Published like @Pattern or Summary like @Pattern order by Published desc limit @Limit;";
            var reader = DataStorage.FeedData.ExecuteRead(sql, new Dictionary<string, object> { { "@Pattern", '%' + pattern + '%' }, { "@Limit", limit } });
            return DBReader(reader);
        }

        public static List<Feed> DBSelectByIdentifier(string pattern, int limit = 100)
        {
            string sql = "select * from Feed where ArticleID like @Pattern or Summary like @Pattern order by Published desc limit @Limit;";
            var reader = DataStorage.FeedData.ExecuteRead(sql, new Dictionary<string, object> { { "@Pattern", '%' + pattern + '%' }, { "@Limit", limit } });
            return DBReader(reader);
        }

        public static List<Feed> DBSelectBookmark(int limit = 100)
        {
            string sql = "select * from Bookmark order by Published desc limit @Limit;";
            var reader = DataStorage.FeedData.ExecuteRead(sql, new Dictionary<string, object> { { "@Limit", limit } });
            return DBReader(reader);
        }

        private static List<Feed> DBReader(SqliteDataReader reader)
        {
            List<Feed> feeds = new List<Feed>();
            if (reader != null)
            {
                while (reader.Read())
                {
                    feeds.Add(new Feed
                    {
                        ID = reader.GetString(0),
                        ParentID = reader.GetString(1),
                        ArticleID = reader.GetString(2),
                        Title = reader.GetString(3),
                        Authors = reader.GetString(4),
                        Published = reader.GetString(5),
                        Link = reader.GetString(6),
                        Summary = reader.GetString(7)
                    });
                }
                reader.Close();
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

        public static int DBDeleteBookmarkByPID(string pid)
        {
            int affectedRows = 0;
            string sql = "delete from Bookmark where ParentID = @ParentID;";
            affectedRows = DataStorage.FeedData.ExecuteWrite(sql, new Dictionary<string, object> { { "@ParentID", pid } });

            FileList.DBInsertList("Data", DataStorage.FeedData.Database);
            FileList.DBInsertTrace("Data", DataStorage.FeedData.Database);

            return affectedRows;
        }

        #endregion

    }
}
