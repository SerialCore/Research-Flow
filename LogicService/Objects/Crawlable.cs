using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicService.Objects
{
    public class Crawlable
    {
        [SQLite.PrimaryKey]
        public string ID { get; set; }

        public string Text { get; set; }

        public string Url { get; set; }

        public string Content { get; set; }

        public HashSet<string> Tags { get; set; }

        public List<Crawlable> SubLinks { get; set; }

    }

}
