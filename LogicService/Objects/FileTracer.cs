using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicService.Objects
{
    /// <summary>
    /// Tracer for all the files in UserFolder
    /// </summary>
    public class FileTracer
    {
        public enum SyncAction
        {
            Add,
            Modify,
            Delete
        }

        public string FileName { get; set; }

        public string FilePosition { get; set; }

        public DateTime DateModified { get; set; }

        public bool WillbeSync { get; set; }

        public SyncAction SyncbyWhat { get; set; }

        public bool IsSynced { get; set; }

    }
}
