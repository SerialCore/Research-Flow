using LogicService.Service;
using LogicService.Storage;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LogicService.Data
{
    public class Crawlable
    {
        /// <summary>
        /// Resources of links filter, service for GetSpecialLinksByUrl() or GetSpecialLinksByText()
        /// </summary>
        public static Dictionary<string, string> LinkType = new Dictionary<string, string>()
        {
            { "Text: NotEmpty", @"\S" },
            { "Text: Has=", "" },
            { "Text: HasPDF", "pdf" },
            { "Url: Has=", "" },
            { "Url: HasDoi", "doi" },
            { "Url: HasPDF", @"(.pdf\z)|(/pdf/)" },
            { "Url: Insite", "" }, // for tag only, not truely dictionary
        };

        public string ID { get; set; }

        public string ParentID { get; set; }

        public string Text { get; set; }

        public string Url { get; set; }

        public string Content { get; set; }

        public string Tags { get; set; }

        public string Filters { get; set; }

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
                    [Text] varchar(50),
                    [Url] varchar(100) not null,
                    [Content] varchar(1000),
                    [Tags] varchar(500),
                    [Filters] varchar(500))";
            DataStorage.CrawlData.ExecuteWrite(sql);
        }

        public static int DBInsert(List<Crawlable> crawlablelist)
        {
            int affectedRows = 0;
            string sql = @"insert into Crawlable(ID, ParentID, Text, Url, Content, Tags, Filters)
                values(@ID, @ParentID, @Text, @Url, @Content, @Tags, @Filters);";

            foreach (Crawlable crawlable in crawlablelist)
            {
                affectedRows = DataStorage.CrawlData.ExecuteWrite(sql, new Dictionary<string, object>
                {
                    { "@ID", crawlable.ID },
                    { "@ParentID", crawlable.ParentID },
                    { "@Text", crawlable.Text },
                    { "@Url", crawlable.Url },
                    { "@Content", crawlable.Content },
                    { "@Tags", crawlable.Tags },
                    { "@Filters", crawlable.Filters },
                });
            }

            FileList.DBInsertList("Data", DataStorage.CrawlData.Database);
            FileList.DBInsertTrace("Data", DataStorage.CrawlData.Database);

            return affectedRows;
        }

        public static List<Crawlable> DBSelectByLimit(int limit)
        {
            string sql = "select * from Crawlable limit @Limit;";
            var reader = DataStorage.CrawlData.ExecuteRead(sql, new Dictionary<string, object> { { "@Limit", limit } });
            return DBReader(reader);
        }

        public static List<Crawlable> DBSelectByID(string id)
        {
            string sql = "select * from Crawlable where ID = @ID;";
            var reader = DataStorage.CrawlData.ExecuteRead(sql, new Dictionary<string, object> { { "@ID", id } });
            return DBReader(reader);
        }

        public static List<Crawlable> DBSelectByPID(string pid)
        {
            string sql = "select * from Crawlable where ParentID = @ParentID;";
            var reader = DataStorage.CrawlData.ExecuteRead(sql, new Dictionary<string, object> { { "@ParentID", pid } });
            return DBReader(reader);
        }

        public static List<Crawlable> DBSelectByText(string text)
        {
            string sql = "select * from Crawlable where Text = @Text;";
            var reader = DataStorage.CrawlData.ExecuteRead(sql, new Dictionary<string, object> { { "@Text", text } });
            return DBReader(reader);
        }

        public static List<Crawlable> DBSelectByTag(string tag)
        {
            string sql = "select * from Crawlable where Tags like @Tags;";
            var reader = DataStorage.CrawlData.ExecuteRead(sql, new Dictionary<string, object> { { "@Tags", '%' + tag + '%' } });
            return DBReader(reader);
        }

        private static List<Crawlable> DBReader(SqliteDataReader reader)
        {
            List<Crawlable> crawl = new List<Crawlable>();
            while (reader.Read())
            {
                crawl.Add(new Crawlable
                {
                    ID = reader.GetString(0),
                    ParentID = reader.GetString(1),
                    Text = reader.GetString(2),
                    Url = reader.GetString(3),
                    Content = reader.GetString(4),
                    Tags = reader.GetString(5),
                    Filters = reader.GetString(6),
                });
            }
            return crawl;
        }

        public static int DBDeleteByID(string id)
        {
            int affectedRows = 0;
            string sql = "delete from Crawlable where ID = @ID;";
            affectedRows = DataStorage.CrawlData.ExecuteWrite(sql, new Dictionary<string, object> { { "@ID", id } });

            FileList.DBInsertList("Data", DataStorage.CrawlData.Database);
            FileList.DBInsertTrace("Data", DataStorage.CrawlData.Database);

            return affectedRows;
        }

        public static void DBUpdate()
        {

        }

        #endregion

        #region Helper

        public async static void AddtoFavorite(Crawlable crawlable)
        {
            var favorites = await LocalStorage.ReadJsonAsync<List<Crawlable>>(await LocalStorage.GetDataFolderAsync(), "favorite.list");
            favorites.Add(crawlable);
            LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "favorite.list", favorites);

            DBInsert(new List<Crawlable>() { crawlable });
        }

        public static List<Crawlable> LinkFilter(CrawlerService service,string filter)
        {
            Regex regex = new Regex(@"(?<header>^(Text|Url):\s(\w+$|\w+=))(?<param>\w*$)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            Match match = regex.Match(filter);
            if (match.Success && service != null)
            {
                string header = match.Groups["header"].Value;
                string param = match.Groups["param"].Value;
                if (header.StartsWith("Text"))
                {
                    if (header.Equals("Url: Insite"))
                        return service.InsiteLinks;
                    else
                        return service.GetSpecialLinksByText(Crawlable.LinkType.GetValueOrDefault(header) + param);
                }
                if (header.StartsWith("Url"))
                {
                    return service.GetSpecialLinksByUrl(Crawlable.LinkType.GetValueOrDefault(header) + param);
                }
                return null; // cannot reach here
            }
            else if (string.IsNullOrEmpty(filter))
                return service.Links;
            else
                return null; // cannot reach here
        }

        #endregion

        #region Task

        public static void TaskRunCrawler()
        {

        }

        public static void TaskRunSearch()
        {

        }

        #endregion

    }

}
