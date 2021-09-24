using LogicService.Storage;
using System;
using System.Collections.Generic;

namespace LogicService.Data
{
    public class FileList
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

        public static void DBInitializeTrace()
        {
            string sql = @"create table if not exists [FileTrace] (
                    [FilePosition] varchar(150) not null,
                    [FileName] varchar(50) not null,
                    [DateModified] varchar(50) not null,
                    primary key ([FilePosition], [FileName]));";
            DataStorage.FileTrace.ExecuteWrite(sql);
        }

        /// <summary>
        /// Insert or update
        /// </summary>
        /// <param name="position"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int DBInsertTrace(string position, string name)
        {
            if (DBSelectTrace(position, name) == null) // insert a new 
            {
                int affectedRows = 0;
                string sql = @"insert into FileTrace(FilePosition, FileName, DateModified) 
                    values(@FilePosition, @FileName, @DateModified);";
                affectedRows += DataStorage.FileTrace.ExecuteWrite(sql, new Dictionary<string, object>
                {
                    { "@FilePosition", position },
                    { "@FileName", name },
                    { "@DateModified", DateTime.Now.ToString() }
                });

                return affectedRows;
            }
            else // update an existing
            {
                return DBUpdateTrace(position, name);
            }
        }

        public static int DBUpdateTrace(string position, string name, string newname = null, string newposition = null)
        {
            int affectedRows = 0;
            string sql = @"update FileTrace set FilePosition = @NewPosition, FileName = @NewName, DateModified = @DateModified
                where FilePosition = @FilePosition and FileName = @FileName";
            affectedRows += DataStorage.FileTrace.ExecuteWrite(sql, new Dictionary<string, object>
            {
                { "@FilePosition", position },
                { "@FileName", name },
                { "@NewPosition", newposition == null ? position : newposition },
                { "@NewName", newname == null ? name : newname },
                { "@DateModified", DateTime.Now.ToString() }
            });
            return affectedRows;
        }

        public static HashSet<FileList> DBSelectAllTrace()
        {
            string sql = "select * from FileTrace;";
            var reader = DataStorage.FileTrace.ExecuteRead(sql);

            HashSet<FileList> trace = new HashSet<FileList>();
            while (reader.Read())
            {
                trace.Add(new FileList
                {
                    FilePosition = reader.GetString(0),
                    FileName = reader.GetString(1),
                    DateModified = DateTimeOffset.Parse(reader.GetString(2))
                });
            }
            reader.Close();
            return trace;
        }

        /// <summary>
        /// check if there is an existing record
        /// </summary>
        /// <param name="position"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FileList DBSelectTrace(string position, string name)
        {
            string sql = "select * from FileTrace where FilePosition = @FilePosition and FileName = @FileName;";
            var reader = DataStorage.FileTrace.ExecuteRead(sql, new Dictionary<string, object> {
                { "@FilePosition", position }, { "@FileName", name } });

            FileList trace = null;
            if (reader != null)
            {
                while (reader.Read())
                {
                    trace = new FileList();
                    trace.FilePosition = reader.GetString(0);
                    trace.FileName = reader.GetString(1);
                    trace.DateModified = DateTimeOffset.Parse(reader.GetString(2));
                }
                reader.Close();
            }
            return trace;
        }

        /// <summary>
        /// triggered only by sync operation
        /// </summary>
        /// <param name="trace"></param>
        /// <returns></returns>
        public static int DBDeleteTrace(string position, string name)
        {
            int affectedRows = 0;
            string sql = "delete from FileTrace where FilePosition = @FilePosition and FileName = @FileName;";
            affectedRows += DataStorage.FileTrace.ExecuteWrite(sql, new Dictionary<string, object> {
                { "@FilePosition", position }, { "@FileName", name } });
            return affectedRows;
        }

        #endregion

        #region List DB

        public static void DBInitializeList()
        {
            string sql = @"create table if not exists [FileList] (
                    [FilePosition] varchar(150) not null,
                    [FileName] varchar(50) not null,
                    [DateModified] varchar(50) not null,
                    primary key ([FilePosition], [FileName]));";
            DataStorage.FileList.ExecuteWrite(sql);
        }

        /// <summary>
        /// Only insert
        /// </summary>
        /// <param name="postition"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int DBInsertList(string postition, string name)
        {
            if (DBSelectList(postition, name) == null) // insert a new 
            {
                int affectedRows = 0;
                string sql = @"insert into FileList(FilePosition, FileName, DateModified) 
                    values(@FilePosition, @FileName, @DateModified);";
                affectedRows += DataStorage.FileList.ExecuteWrite(sql, new Dictionary<string, object>
                {
                    { "@FilePosition", postition },
                    { "@FileName", name },
                    { "@DateModified", DateTime.Now.ToString() }
                });

                return affectedRows;
            }
            else
            {
                return 0;
            }
        }

        public static int DBUpdateList(string position, string name, string newposition = null, string newname = null)
        {
            int affectedRows = 0;
            string sql = @"update FileList set FilePosition = @NewPosition, FileName = @NewName, DateModified = @DateModified 
                where FilePosition = @FilePosition and FileName = @FileName";
            affectedRows += DataStorage.FileList.ExecuteWrite(sql, new Dictionary<string, object>
            {
                { "@FilePosition", position },
                { "@FileName", name },
                { "@NewPosition", newposition == null ? position : newposition },
                { "@NewName", newname == null ? name : newname },
                { "@DateModified", DateTime.Now.ToString() }
            });
            return affectedRows;
        }

        public static HashSet<FileList> DBSelectAllList()
        {
            string sql = "select * from FileList;";
            var reader = DataStorage.FileList.ExecuteRead(sql);

            HashSet<FileList> list = new HashSet<FileList>();
            if (reader != null)
            {
                while (reader.Read())
                {
                    list.Add(new FileList
                    {
                        FilePosition = reader.GetString(0),
                        FileName = reader.GetString(1),
                        DateModified = DateTimeOffset.Parse(reader.GetString(2))
                    });
                }
                reader.Close();
            }
            return list;
        }

        /// <summary>
        /// check if there is an existing record
        /// </summary>
        /// <param name="position"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FileList DBSelectList(string position, string name)
        {
            string sql = "select * from FileList where FilePosition = @FilePosition and FileName = @FileName;";
            var reader = DataStorage.FileList.ExecuteRead(sql, new Dictionary<string, object> {
                { "@FilePosition", position }, { "@FileName", name } });

            FileList list = null;
            while (reader.Read())
            {
                list = new FileList();
                list.FilePosition = reader.GetString(0);
                list.FileName = reader.GetString(1);
                list.DateModified = DateTimeOffset.Parse(reader.GetString(2));
            }
            reader.Close();
            return list;
        }

        /// <summary>
        /// triggered only by user operation
        /// </summary>
        /// <param name="trace"></param>
        /// <returns></returns>
        public static int DBDeleteList(string position, string name)
        {
            int affectedRows = 0;
            string sql = "delete from FileList where FilePosition = @FilePosition and FileName = @FileName;";
            affectedRows += DataStorage.FileList.ExecuteWrite(sql, new Dictionary<string, object> {
                { "@FilePosition", position }, { "@FileName", name } });
            return affectedRows;
        }

        #endregion

    }
}