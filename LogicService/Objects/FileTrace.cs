using LogicService.Application;
using LogicService.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace LogicService.Objects
{
    /// <summary>
    /// on local
    /// </summary>
    public class FileTrace
    {
        public string FilePosition { get; set; }

        public string FileName { get; set; }

        public DateTime DateModified { get; set; }

        public bool IsSynced { get; set; }

    }

    /// <summary>
    /// on cloud or roaming
    /// </summary>
    public class RemoveList
    {
        public string FilePosition { get; set; }

        public string FileName { get; set; }

        public int Checked { get; set; }

        public bool AddorDel { get; set; }

    }

    /// <summary>
    /// on cloud or roaming
    /// </summary>
    public class AddList
    {
        public string FilePosition { get; set; }

        public string FileName { get; set; }

        public int Checked { get; set; }

    }
}
