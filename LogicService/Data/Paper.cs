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

        public string Link { get; set; }

        public string Published { get; set; }

        public string Authors { get; set; }

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
                    [Title] varchar(100) not null,
                    [Link] varchar(100),
                    [Published] varchar(50),
                    [Authors] varchar(100));";
            DataStorage.PaperData.ExecuteWrite(sql);
        }

        public static int DBInsert(List<Paper> paperlist)
        {
            int affectedRows = 0;
            string sql = @"insert into Paper(ID, ParentID, Title, Link, Published, Authors)
                values(@ID, @ParentID, @Title, @Link, @Published, @Authors);";

            foreach (Paper paper in paperlist)
            {
                affectedRows = DataStorage.PaperData.ExecuteWrite(sql, new Dictionary<string, object>
                {
                    { "@ID", paper.ID },
                    { "@ParentID", paper.ParentID },
                    { "@Title", paper.Title },
                    { "@Link", paper.Link },
                    { "@Published", paper.Published },
                    { "@Authors", paper.Authors }
                });
            }

            FileList.DBInsertList("Data", DataStorage.PaperData.Database);
            FileList.DBInsertTrace("Data", DataStorage.PaperData.Database);

            return affectedRows;
        }

        public static List<Paper> DBSelectByLimit(int limit)
        {
            string sql = "select * from Paper limit @Limit;";
            var reader = DataStorage.PaperData.ExecuteRead(sql, new Dictionary<string, object> { { "@Limit", limit } });
            return DBReader(reader);
        }

        public static List<Paper> DBSelectByID(string id)
        {
            string sql = "select * from Paper where ID = @ID;";
            var reader = DataStorage.PaperData.ExecuteRead(sql, new Dictionary<string, object> { { "@ID", id } });
            return DBReader(reader);
        }

        public static List<Paper> DBSelectByPID(string pid)
        {
            string sql = "select * from Paper where ParentID = @ParentID;";
            var reader = DataStorage.PaperData.ExecuteRead(sql, new Dictionary<string, object> { { "@ParentID", pid } });
            return DBReader(reader);
        }

        public static List<Paper> DBSelectByText(string pattern, int limit)
        {
            string sql = "select * from Paper where Title like @Patter order by Published desc limit @Limit;";
            var reader = DataStorage.FeedData.ExecuteRead(sql, new Dictionary<string, object> { { "@Pattern", '%' + pattern + '%' }, { "@Limit", limit } });
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
                        Link = reader.GetString(3),
                        Published = reader.GetString(4),
                        Authors = reader.GetString(5)
                    });
                }
                reader.Close();
            }
            return papers;
        }

        public static int DBDeleteByID(string id)
        {
            int affectedRows = 0;
            string sql = "delete from Paper where ID = @ID;";
            affectedRows = DataStorage.PaperData.ExecuteWrite(sql, new Dictionary<string, object> { { "@ID", id } });

            FileList.DBInsertList("Data", DataStorage.PaperData.Database);
            FileList.DBInsertTrace("Data", DataStorage.PaperData.Database);

            return affectedRows;
        }

        public static void DBUpdateApp()
        {
            PaperFile.DBInitialize();

            string sql1 = @"create table [temp] (
                    [ID] varchar(50) not null primary key,
                    [ParentID] varchar(50),
                    [Title] varchar(100) not null,
                    [Link] varchar(100),
                    [Published] varchar(50),
                    [Authors] varchar(100));";
            DataStorage.PaperData.ExecuteWrite(sql1);

            string sql2 = @"insert into temp(ID, ParentID, Title, Link, Authors) 
                    select ID, ParentID, Title, Link, Authors from Paper;";
            DataStorage.PaperData.ExecuteWrite(sql2);

            string sql21 = "update temp set Published = '';";
            DataStorage.PaperData.ExecuteWrite(sql21);

            string sql22 = "insert into PaperFile select ID, FileName from Paper where FileName != '';";
            DataStorage.PaperData.ExecuteWrite(sql22);

            string sql3 = "drop table if exists Paper;";
            DataStorage.PaperData.ExecuteWrite(sql3);

            string sql4 = "alter table temp rename to Paper;";
            DataStorage.PaperData.ExecuteWrite(sql4);
        }

        #endregion

        #region Helper

        public static void TryDownload(string url)
        {

        }

        public static void TryExtract()
        {

        }

        public static void ReadDocument()
        {

        }

        #endregion

    }

    public class PaperFile
    {

        public string ID { get; set; }

        public string FileName { get; set; }

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (this.GetType() != obj.GetType())
                return false;

            var one = (PaperFile)obj;
            if (this.ID == one.ID)
                return true;
            else
                return false;
        }

        public static bool operator ==(PaperFile leftHandSide, PaperFile rightHandSide)
        {
            if (ReferenceEquals(leftHandSide, null))
                return ReferenceEquals(rightHandSide, null);
            return (leftHandSide.Equals(rightHandSide));
        }

        public static bool operator !=(PaperFile leftHandSide, PaperFile rightHandSide)
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
            string sql = @"create table if not exists [PaperFile] (
                    [ID] varchar(50) not null primary key,
                    [FileName] varchar(100) not null)";
            DataStorage.PaperData.ExecuteWrite(sql);
        }

        public static int DBInsert(List<PaperFile> filelist)
        {
            int affectedRows = 0;
            string sql = @"insert into PaperFile(ID, FileName) values(@ID, @FileName);";

            foreach (PaperFile file in filelist)
            {
                affectedRows = DataStorage.PaperData.ExecuteWrite(sql, new Dictionary<string, object>
                {
                    { "@ID", file.ID },
                    { "@FileName", file.FileName },
                });
            }

            FileList.DBInsertList("Data", DataStorage.PaperData.Database);
            FileList.DBInsertTrace("Data", DataStorage.PaperData.Database);

            return affectedRows;
        }

        public static List<PaperFile> DBSelectByLimit(int limit)
        {
            string sql = "select * from PaperFile limit @Limit;";
            var reader = DataStorage.PaperData.ExecuteRead(sql, new Dictionary<string, object> { { "@Limit", limit } });
            return DBReader(reader);
        }

        public static List<PaperFile> DBSelectByID(string id)
        {
            string sql = "select * from PaperFile where ID = @ID;";
            var reader = DataStorage.PaperData.ExecuteRead(sql, new Dictionary<string, object> { { "@ID", id } });
            return DBReader(reader);
        }

        public static List<PaperFile> DBSelectByFile(string filename)
        {
            string sql = "select * from PaperFile where FileName = @FileName;";
            var reader = DataStorage.PaperData.ExecuteRead(sql, new Dictionary<string, object> { { "@FileName", filename } });
            return DBReader(reader);
        }

        private static List<PaperFile> DBReader(SqliteDataReader reader)
        {
            List<PaperFile> papers = new List<PaperFile>();
            if (reader != null)
            {
                while (reader.Read())
                {
                    papers.Add(new PaperFile
                    {
                        ID = reader.GetString(0),
                        FileName = reader.GetString(1)
                    });
                }
                reader.Close();
            }
            return papers;
        }

        public static int DBDeleteByID(string id)
        {
            int affectedRows = 0;
            string sql = "delete from PaperFile where ID = @ID;";
            affectedRows = DataStorage.PaperData.ExecuteWrite(sql, new Dictionary<string, object> { { "@ID", id } });

            FileList.DBInsertList("Data", DataStorage.PaperData.Database);
            FileList.DBInsertTrace("Data", DataStorage.PaperData.Database);

            return affectedRows;
        }

        #endregion

    }

}
