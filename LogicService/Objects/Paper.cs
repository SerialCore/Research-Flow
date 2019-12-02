using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicService.Objects
{
    public class Paper
    {

        public string ID { get; set; }

        public string ParentID { get; set; }

        public string Title { get; set; }

        public string Abstract { get; set; }

        public string Authors { get; set; }

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
                    [Title] varchar(100) not null,
                    [Abstract] varchar(1000),
                    [Authors] varchar(500)),
                    [Tags] varchar(500))";
            DataStorage.PaperData.ExecuteWrite(sql);
        }

        public static List<Paper> DBSelectAll()
        {
            string sql = "select * from Paper;";
            var reader = DataStorage.PaperData.ExecuteRead(sql);

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
                        Abstract = reader.GetString(3),
                        Authors = reader.GetString(4),
                        Tags = reader.GetString(5)
                    });
                }
            }
            return papers;
        }

        #endregion

    }

}
