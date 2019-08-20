using LogicService.Application;
using LogicService.Helper;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace LogicService.Objects
{
    public class FileList
    {

        public string FilePosition { get; set; }

        public string FileName { get; set; }

        public DateTime DateModified { get; set; }

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (this.GetType() != obj.GetType())
                return false;

            var one = (FileList)obj;
            if (this.FileName == one.FileName && this.FilePosition == one.FilePosition)
                return true;
            else
                return false;
        }

        public static bool operator ==(FileList leftHandSide, FileList rightHandSide)
        {
            if (ReferenceEquals(leftHandSide, null))
                return ReferenceEquals(rightHandSide, null);
            return (leftHandSide.Equals(rightHandSide));
        }

        public static bool operator !=(FileList leftHandSide, FileList rightHandSide)
        {
            return !(leftHandSide == rightHandSide);
        }

        public override int GetHashCode()
        {
            int hashCode = FilePosition.GetHashCode();
            if (FilePosition.GetHashCode() != FileName.GetHashCode())
                hashCode ^= FileName.GetHashCode();
            return hashCode;
        }

        #endregion

        #region Trace DB

        public static void DBOpenTrace()
            => DataStorage.FileTrace.Connection.Open();

        public static void DBCloseTrace()
            => DataStorage.FileTrace.Connection.Close();

        public static void DBInitializeTrace()
        {

        }

        public static void DBInsertTrace()
        {

        }

        public static void DBSelectTrace()
        {

        }

        public static void DBUpdateTrace()
        {

        }

        public static void DBDeleteTrace()
        {

        }

        #endregion

        #region List DB

        public static void DBOpenList()
            => DataStorage.FileList.Connection.Open();

        public static void DBCloseList()
            => DataStorage.FileList.Connection.Close();

        public static void DBInitializeList()
        {

        }

        public static void DBInsertList()
        {

        }

        public static void DBSelectList()
        {

        }

        public static void DBUpdateList()
        {

        }

        public static void DBDeleteList()
        {

        }

        #endregion

    }
}