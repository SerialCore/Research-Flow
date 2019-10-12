using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogicService.Storage;

namespace LogicService.Objects
{
    public class Crawlable
    {
        public string ID { get; set; }

        public string ParentID { get; set; }

        public string Text { get; set; }

        public string Url { get; set; }

        public string Content { get; set; }

        public string Tags { get; set; }

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
                    [Content] varchar(5000),
                    [Tags] varchar(500))";
            DataStorage.CrawlData.ExecuteWrite(sql);
        }

        public static int DBInsert()
        {
            int affectedRows = 0;
            string sql = @"insert into Crawlable(ID, ParentID, Text, Url)
                values(@ID, @ParentID, @Text, @Url);";
            return affectedRows;
        }

        public static List<Crawlable> DBSelectUIByPID(string pid)
        {
            string sql = "select * from Crawlable where ParentID = @ParentID;";
            var reader = DataStorage.CrawlData.ExecuteRead(sql, new Dictionary<string, object> { { "@ParentID", pid } });

            List<Crawlable> crawl = new List<Crawlable>();
            while (reader.Read())
            {
                crawl.Add(new Crawlable
                {
                    ID = reader.GetString(0),
                    ParentID = reader.GetString(1), // if null
                    Text = reader.GetString(2),
                    Url = reader.GetString(3)
                });
            }
            return crawl;
        }

        public static void DBUpdate()
        {

        }

        public static int DBDeleteByID(string id)
        {
            int affectedRows = 0;
            string sql = "delete from Crawlable where ID = @ID;";
            affectedRows = DataStorage.FeedData.ExecuteWrite(sql, new Dictionary<string, object> { { "@ID", id } });
            return affectedRows;
        }

        #endregion

    }

}
