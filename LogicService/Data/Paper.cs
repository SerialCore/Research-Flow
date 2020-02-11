using LogicService.Storage;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace LogicService.Data
{
    public class Paper
    {

        public string ID { get; set; }

        public string ParentID { get; set; }

        public string Title { get; set; }

        public string FileName { get; set; }

        public string Link { get; set; }

        public string Authors { get; set; }

        public string Note { get; set; }

        public string Tags { get; set; }

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (this.GetType() != obj.GetType())
                return false;

            var one = (Paper)obj;
            if (this.ID == one.ID)
                return true;
            else
                return false;
        }

        public static bool operator ==(Paper leftHandSide, Paper rightHandSide)
        {
            if (ReferenceEquals(leftHandSide, null))
                return ReferenceEquals(rightHandSide, null);
            return (leftHandSide.Equals(rightHandSide));
        }

        public static bool operator !=(Paper leftHandSide, Paper rightHandSide)
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
            string sql = @"create table if not exists [Paper] (
                    [ID] varchar(50) not null primary key,
                    [ParentID] varchar(50),
                    [Title] varchar(100),
                    [FileName] varchar(100),
                    [Link] varchar(100),
                    [Authors] varchar(100),
                    [Note] varchar(1000),
                    [Tags] varchar(500))";
            DataStorage.PaperData.ExecuteWrite(sql);
        }

        public static List<Paper> DBSelectByLimit(int limit)
        {
            string sql = "select * from Paper limit @Limit;";
            var reader = DataStorage.PaperData.ExecuteRead(sql, new Dictionary<string, object> { { "@Limit", limit } });
            return DBReader(reader);
        }

        public static List<Paper> DBSelectByFile(string filename)
        {
            string sql = "select * from Paper where FileName = @FileName;";
            var reader = DataStorage.PaperData.ExecuteRead(sql, new Dictionary<string, object> { { "@FileName", filename } });
            return DBReader(reader);
        }

        public static List<Paper> DBSelectByTag(string tag)
        {
            string sql = "select * from Paper where Tags like @Tags;";
            var reader = DataStorage.PaperData.ExecuteRead(sql, new Dictionary<string, object> { { "@Tags", '%' + tag + '%' } });
            return DBReader(reader);
        }

        private static List<Paper> DBReader(SqliteDataReader reader)
        {
            List<Paper> papers = new List<Paper>();
            if (reader != null)
            {
                while (reader.Read())
                {
                    papers.Add(new Paper
                    {
                        ID = reader.GetString(0),
                        ParentID = reader.GetString(1),
                        Title = reader.GetString(2),
                        FileName = reader.GetString(3),
                        Link = reader.GetString(4),
                        Authors = reader.GetString(5),
                        Note = reader.GetString(6),
                        Tags = reader.GetString(7),
                    });
                }
            }
            return papers;
        }

        public static void DBUpdate()
        {

        }

        #endregion

        #region Helper

        public static void ReceiveFlow()
        {

        }

        #endregion

    }

}
