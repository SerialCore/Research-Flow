using LogicService.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicService.Objects
{
    /// <summary>
    /// Trace for all the files in UserFolder
    /// </summary>
    public class FileTrace
    {

        public string FilePosition { get; set; }

        public string FileName { get; set; }

        public DateTime DateModified { get; set; }

        public bool IsSynced { get; set; }

    }

    public class Filesrc
    {
        public static string Tracer => ApplicationSetting.AccountName + ".trace";
        public static string RssList => "rsslist";
        public static string SearchLog => "SearchTask.log";
        public static string FileManage => "FileManagement.log";
    }
}
