using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Syndication;

namespace LogicService.Objects
{
    public class RSSSource
    {

        public string ID { get; set; }

        public string Name { get; set; }

        public string Uri { get; set; }

        public int MaxCount { get; set; }

        public double DaysforUpdate { get; set; }

        public double Star { get; set; }

        public bool IsJournal { get; set; }

        public DateTime LastUpdateTime { get; set; }

    }

    public class FeedItem
    {

        public string Title { get; set; }

        public string Published { get; set; }

        public string DOI { get; set; }

        public string Link { get; set; }

        public string Xml { get; set; }

        public string Summary { get; set; }

        public string FullText { get; set; }

        public List<ElementLink> PageLinks { get; set; }

    }

    public class ElementLink
    {

        public string Url { get; set; }

        public string Text { get; set; }

    }
}
