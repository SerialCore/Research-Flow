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

        public string LinkTarget { get; set; }

        public string Content { get; set; }

        public HashSet<string> Tags { get; set; }

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

        private string CMDInsert
            => @"INSERT INTO Users(Username, Email, Password)
                VALUES('admin', 'testing@gmail.com', 'test')";

        private string CMDSelect
            => "SELECT * From Users WHERE Id = @UserId;";

        private string CMDDelete
            => "DELETE FROM Users";

        public static void InitializeTable()
        {
            DataStorage.CrawlData.Connection.Open();
            
            string sql = @"CREATE TABLE IF NOT EXISTS [Crawlable] (
                    [ID] VARCHAR(50) NOT NULL PRIMARY KEY,
                    [ParentID] VARCHAR(50),
                    [Text] VARCHAR(50) NOT NULL,
                    [Url] VARCHAR(100) NOT NULL,
                    [LinkTarget] VARCHAR(20),
                    [Content] VARCHAR(1000))";
            DataStorage.CrawlData.ExecuteWrite(sql);

            DataStorage.CrawlData.Connection.Close();
        }

        #endregion

    }

}
