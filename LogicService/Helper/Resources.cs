using LogicService.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicService.Helper
{
    public class Resources
    {

        public static string RSSList => "rsslist";
        public static string EngineList => "searchlist";

        public static string FileTrace => "filetrace";
        public static string FileList => ApplicationSetting.AccountName + ".filelist";

        public static string Profile => ApplicationSetting.AccountName + ".profile";

    }
}
