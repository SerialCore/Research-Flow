using LogicService.Storage;
using System;
using System.Collections.Generic;

namespace LogicService.Data
{
    public class FileTrace
    {

        public string FilePosition { get; set; }

        public string FileName { get; set; }

        public DateTimeOffset DateModified { get; set; }

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (this.GetType() != obj.GetType())
                return false;

            var one = (FileTrace)obj;
            if (this.FileName == one.FileName && this.FilePosition == one.FilePosition)
                return true;
            else
                return false;
        }

        public static bool operator ==(FileTrace leftHandSide, FileTrace rightHandSide)
        {
            if (ReferenceEquals(leftHandSide, null))
                return ReferenceEquals(rightHandSide, null);
            return (leftHandSide.Equals(rightHandSide));
        }

        public static bool operator !=(FileTrace leftHandSide, FileTrace rightHandSide)
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

        public static void DBInitialize()
        {
            string sql = @"create table if not exists [FileTrace] (
                    [FilePosition] varchar(150) not null,
                    [FileName] varchar(50) not null,
                    [DateModified] varchar(50) not null,
                    primary key ([FilePosition], [FileName]));";
            new DataStorage("filetrace").ExecuteWrite(sql);
        }

        public static int DBInsert(string position, string name)
        {
            if (DBSelect(position, name) == null) // insert a new 
            {
                int affectedRows = 0;
                string sql = @"insert into FileTrace(FilePosition, FileName, DateModified) 
                    values(@FilePosition, @FileName, @DateModified);";
                affectedRows += new DataStorage("filetrace").ExecuteWrite(sql, new Dictionary<string, object>
                {
                    { "@FilePosition", position },
                    { "@FileName", name },
                    { "@DateModified", DateTime.Now.ToString() }
                });

                return affectedRows;
            }
            else // update an existing
            {
                return DBUpdate(position, name);
            }
        }

        public static int DBUpdate(string position, string name, string newname = null, string newposition = null)
        {
            int affectedRows = 0;
            string sql = @"update FileTrace set FilePosition = @NewPosition, FileName = @NewName, DateModified = @DateModified
                where FilePosition = @FilePosition and FileName = @FileName";
            affectedRows += new DataStorage("filetrace").ExecuteWrite(sql, new Dictionary<string, object>
            {
                { "@FilePosition", position },
                { "@FileName", name },
                { "@NewPosition", newposition == null ? position : newposition },
                { "@NewName", newname == null ? name : newname },
                { "@DateModified", DateTime.Now.ToString() }
            });
            return affectedRows;
        }

        public static HashSet<FileTrace> DBSelectAll()
        {
            string sql = "select * from FileTrace;";
            var reader = new DataStorage("filetrace").ExecuteRead(sql);

            HashSet<FileTrace> trace = new HashSet<FileTrace>();
            while (reader.Read())
            {
                trace.Add(new FileTrace
                {
                    FilePosition = reader.GetString(0),
                    FileName = reader.GetString(1),
                    DateModified = DateTimeOffset.Parse(reader.GetString(2))
                });
            }
            reader.Close();
            return trace;
        }

        public static FileTrace DBSelect(string position, string name)
        {
            string sql = "select * from FileTrace where FilePosition = @FilePosition and FileName = @FileName;";
            var reader = new DataStorage("filetrace").ExecuteRead(sql, new Dictionary<string, object> {
                { "@FilePosition", position }, { "@FileName", name } });

            FileTrace trace = null;
            if (reader != null)
            {
                while (reader.Read())
                {
                    trace = new FileTrace();
                    trace.FilePosition = reader.GetString(0);
                    trace.FileName = reader.GetString(1);
                    trace.DateModified = DateTimeOffset.Parse(reader.GetString(2));
                }
                reader.Close();
            }
            return trace;
        }

        public static int DBDelete(string position, string name)
        {
            int affectedRows = 0;
            string sql = "delete from FileTrace where FilePosition = @FilePosition and FileName = @FileName;";
            affectedRows += new DataStorage("filetrace").ExecuteWrite(sql, new Dictionary<string, object> {
                { "@FilePosition", position }, { "@FileName", name } });
            return affectedRows;
        }

        #endregion

    }
}