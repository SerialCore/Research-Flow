using LogicService.Helper;
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

        public static void Initialize()
        {
             List<FeedSource> FeedSources = new List<FeedSource>()
                {
                    new FeedSource{ ID = HashEncode.MakeMD5("http://feeds.aps.org/rss/recent/prl.xml"),
                        Name = "Physical Review Letters", Uri = "http://feeds.aps.org/rss/recent/prl.xml", Star = 5, IsRSS = true },
                    new FeedSource{ ID = HashEncode.MakeMD5("http://feeds.aps.org/rss/recent/prd.xml"),
                        Name = "Physical Review D", Uri = "http://feeds.aps.org/rss/recent/prd.xml", Star = 5, IsRSS = true },

                    new FeedSource{ ID = HashEncode.MakeMD5("http://export.arxiv.org/rss/cs"),
                        Name = "arXiv Computer Science", Uri = "http://export.arxiv.org/rss/cs", Star = 4, IsRSS = true },
                    new FeedSource{ ID = HashEncode.MakeMD5("http://export.arxiv.org/rss/hep-ex"),
                        Name = "arXiv HEP Experiment", Uri = "http://export.arxiv.org/rss/hep-ex", Star = 5, IsRSS = true },
                    new FeedSource{ ID = HashEncode.MakeMD5("http://export.arxiv.org/rss/hep-lat"),
                        Name = "arXiv HEP Lattice", Uri = "http://export.arxiv.org/rss/hep-lat", Star = 5, IsRSS = true },
                    new FeedSource{ ID = HashEncode.MakeMD5("http://export.arxiv.org/rss/hep-ph"),
                        Name = "arXiv HEP Phenomenology", Uri = "http://export.arxiv.org/rss/hep-ph", Star = 5, IsRSS = true },
                    new FeedSource{ ID = HashEncode.MakeMD5("http://export.arxiv.org/rss/hep-th"),
                        Name = "arXiv HEP Theory", Uri = "http://export.arxiv.org/rss/hep-th", Star = 5, IsRSS = true },
                    new FeedSource{ ID = HashEncode.MakeMD5("http://export.arxiv.org/rss/math"),
                        Name = "arXiv Mathematics", Uri = "http://export.arxiv.org/rss/math", Star = 2, IsRSS = true },
                    new FeedSource{ ID = HashEncode.MakeMD5("http://export.arxiv.org/rss/math-ph"),
                        Name = "arXiv Mathematical Physics", Uri = "http://export.arxiv.org/rss/math-ph", Star = 2, IsRSS = true },
                    new FeedSource{ ID = HashEncode.MakeMD5("http://export.arxiv.org/rss/nucl-ex"),
                        Name = "arXiv Nuclear Experiment", Uri = "http://export.arxiv.org/rss/nucl-ex", Star = 2, IsRSS = true },
                    new FeedSource{ ID = HashEncode.MakeMD5("http://export.arxiv.org/rss/nucl-th"),
                        Name = "arXiv Nuclear Theory", Uri = "http://export.arxiv.org/rss/nucl-th", Star = 2, IsRSS = true },
                };
            LocalStorage.WriteJson(LocalStorage.GetLocalCacheFolder(), "rss.list", FeedSources);
            foreach (FeedSource source in FeedSources)
            {
                Feed.DBInitialize(source.ID);
            }
        }

    }

    public class Feed
    {

        public string ID { get; set; }

        public string Title { get; set; }

        public string Authors { get; set; }

        public string ArticleID { get; set; }

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

        public static void DBInitialize(string pid)
        {
            string sql = @"create table if not exists [Feed] (
                    [ID] varchar(50) not null primary key,
                    [Title] varchar(100) not null,
                    [Authors] varchar(100),
                    [ArticleID] varchar(100),
                    [Published] varchar(10),
                    [Link] varchar(100),
                    [Summary] varchar(500));";
            new DataStorage(pid).ExecuteWrite(sql);
        }

        public static void DBInitializeBookmark()
        {
            string sql = @"create table if not exists [Feed] (
                    [ID] varchar(50) not null primary key,
                    [Title] varchar(100) not null,
                    [Authors] varchar(100),
                    [ArticleID] varchar(100),
                    [Published] varchar(10),
                    [Link] varchar(100),
                    [Summary] varchar(500));";
            new DataStorage("bookmark").ExecuteWrite(sql);
        }

        public static int DBInsert(string pid, List<Feed> feeds)
        {
            int affectedRows = 0;
            string sql = @"insert into Feed(ID, Title, Authors, ArticleID, Published, Link, Summary)
                values(@ID, @Title, @Authors, @ArticleID, @Published, @Link, @Summary);";
            DataStorage feeddb = new DataStorage(pid);

            foreach (Feed feed in feeds)
            {
                affectedRows += feeddb.ExecuteWrite(sql, new Dictionary<string, object>
                {
                    { "@ID", feed.ID },
                    { "@Title", feed.Title },
                    { "@Authors", feed.Authors },
                    { "@ArticleID", feed.ArticleID },
                    { "@Published", feed.Published },
                    { "@Link", feed.Link },
                    { "@Summary", feed.Summary }
                }, false);
            }

            FileTrace.DBInsert("Data", feeddb.Database);

            return affectedRows;
        }

        public static int DBInsertBookmark(Feed feed)
        {
            int affectedRows = 0;
            string sql = @"insert into Feed(ID, Title, Authors, ArticleID, Published, Link, Summary)
                values(@ID, @Title, @Authors, @ArticleID, @Published, @Link, @Summary);";
            affectedRows += new DataStorage("bookmark").ExecuteWrite(sql, new Dictionary<string, object>
                {
                    { "@ID", feed.ID },
                    { "@Title", feed.Title },
                    { "@Authors", feed.Authors },
                    { "@ArticleID", feed.ArticleID },
                    { "@Published", feed.Published },
                    { "@Link", feed.Link },
                    { "@Summary", feed.Summary }
                }, false);

            FileTrace.DBInsert("Data", "bookmark.db");

            return affectedRows;
        }

        public static List<Feed> DBSelectByPID(string pid, int limit = 100)
        {
            string sql = "select * from Feed order by Published desc limit @Limit;";
            var reader = new DataStorage(pid).ExecuteRead(sql, new Dictionary<string, object> { { "@Limit", limit } });
            return DBReader(reader);
        }

        public static List<Feed> DBSelectByTitle(string pid, string pattern, int limit = 100)
        {
            string sql = "select * from Feed where Title like @Pattern order by Published desc limit @Limit;";
            var reader = new DataStorage(pid).ExecuteRead(sql, new Dictionary<string, object> { { "@Pattern", '%' + pattern + '%' }, { "@Limit", limit } });
            return DBReader(reader);
        }

        public static List<Feed> DBSelectByText(string pid, string pattern, int limit = 100)
        {
            string sql = "select * from Feed where Title like @Pattern or Summary like @Pattern order by Published desc limit @Limit;";
            var reader = new DataStorage(pid).ExecuteRead(sql, new Dictionary<string, object> { { "@Pattern", '%' + pattern + '%' }, { "@Limit", limit } });
            return DBReader(reader);
        }

        public static List<Feed> DBSelectByAuthor(string pid, string pattern, int limit = 100)
        {
            string sql = "select * from Feed where Authors like @Pattern or Summary like @Pattern order by Published desc limit @Limit;";
            var reader = new DataStorage(pid).ExecuteRead(sql, new Dictionary<string, object> { { "@Pattern", '%' + pattern + '%' }, { "@Limit", limit } });
            return DBReader(reader);
        }

        public static List<Feed> DBSelectByPublished(string pid, string pattern, int limit = 100)
        {
            string sql = "select * from Feed where Published like @Pattern or Summary like @Pattern order by Published desc limit @Limit;";
            var reader = new DataStorage(pid).ExecuteRead(sql, new Dictionary<string, object> { { "@Pattern", '%' + pattern + '%' }, { "@Limit", limit } });
            return DBReader(reader);
        }

        public static List<Feed> DBSelectByIdentifier(string pid, string pattern, int limit = 100)
        {
            string sql = "select * from Feed where ArticleID like @Pattern or Summary like @Pattern order by Published desc limit @Limit;";
            var reader = new DataStorage(pid).ExecuteRead(sql, new Dictionary<string, object> { { "@Pattern", '%' + pattern + '%' }, { "@Limit", limit } });
            return DBReader(reader);
        }

        public static List<Feed> DBSelectBookmark(int limit = 100)
        {
            string sql = "select * from Feed order by Published desc limit @Limit;";
            var reader = new DataStorage("bookmark").ExecuteRead(sql, new Dictionary<string, object> { { "@Limit", limit } });
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
                        Title = reader.GetString(1),
                        Authors = reader.GetString(2),
                        ArticleID = reader.GetString(3),
                        Published = reader.GetString(4),
                        Link = reader.GetString(5),
                        Summary = reader.GetString(6)
                    });
                }
                reader.Close();
            }
            return feeds;
        }

        public static void DBDeleteByPID(string pid)
        {
            LocalStorage.GeneralDeleteAsync(LocalStorage.GetLocalCacheFolder(), pid + ".db");
        }

        public static int DBDeleteBookmarkByID(string id)
        {
            int affectedRows = 0;
            string sql = "delete from Bookmark where ID = @ID;";
            affectedRows = new DataStorage("bookmark").ExecuteWrite(sql, new Dictionary<string, object> { { "@ID", id } });

            FileTrace.DBInsert("Data", id + ".db");

            return affectedRows;
        }

        #endregion

    }
}
