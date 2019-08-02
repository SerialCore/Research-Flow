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

        public List<string> Authors { get; set; }

        public HashSet<string> Tags { get; set; }

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
            DataStorage.PaperData.Connection.Open();

            string sql = @"CREATE TABLE IF NOT EXISTS [Paper] (
                    [ID] VARCHAR(50) NOT NULL PRIMARY KEY,
                    [ParentID] VARCHAR(50),
                    [Title] VARCHAR(100) NOT NULL,
                    [Abstract] VARCHAR(1000),
                    [Authors] VARCHAR(500)),
                    [Tags] VARCHAR(500))";
            DataStorage.PaperData.ExecuteWrite(sql);

            DataStorage.PaperData.Connection.Close();
        }

        #endregion

    }

}
