using LogicService.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicService.Objects
{
    public class Resources
    {

        public static string RSSList => "rsslist";

        public static string FileTrace => "filetrace";
        public static string RemoveList => ApplicationSetting.AccountName + ".removelist";
        public static string AddList => ApplicationSetting.AccountName + ".addlist";

    }
}
