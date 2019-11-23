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
        /// <summary>
        /// Data List
        /// </summary>
        public static string RSSList => "rsslist";
        public static string SearchList => "searchlist";
        public static string TagList => "taglist";
        public static string TopicList => "topiclist";

        /// <summary>
        /// Datebase
        /// </summary>
        public static string CrawlerData => "crawlable.db";
        public static string PaperData => "paper.db";
        public static string FeedData => "feed.db";

        /// <summary>
        /// File List
        /// </summary>
        public static string FileTrace => "filetrace.db";
        public static string FileList => "filelist.db";

        /// <summary>
        /// Log File
        /// </summary>
        public static string StorageLog => "StorageTask.log";
        public static string SearchLog => "SearchTask.log";

    }
}
