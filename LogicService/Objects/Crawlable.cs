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

        public static void DBOpen()
            => DataStorage.CrawlData.Connection.Open();

        public static void DBClose()
            => DataStorage.CrawlData.Connection.Close();

        public static void DBInitialize()
        {
            DataStorage.CrawlData.Connection.Open();

            string sql = @"create table if not exists [Crawlable] (
                    [ID] varchar(50) not null primary key,
                    [ParentID] varchar(50),
                    [Text] varchar(50),
                    [Url] varchar(100) not null,
                    [Content] varchar(5000),
                    [Tags] varchar(500))";
            DataStorage.CrawlData.ExecuteWrite(sql);

            DataStorage.CrawlData.Connection.Close();
        }

        public static void DBInsert()
        {
            string sql = @"INSERT INTO Users(Username, Email, Password)
                VALUES('admin', 'testing@gmail.com', 'test')";
            DataStorage.CrawlData.ExecuteWrite(sql);
        }

        public static void DBSelect()
        {
            string sql = "SELECT * From Users WHERE Id = @UserId;";
            DataStorage.CrawlData.ExecuteRead(sql);
        }

        public static void DBDelete()
        {
            string sql = "DELETE FROM Users";
            DataStorage.CrawlData.ExecuteWrite(sql);
        }

        #endregion

    }

}
