using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicService.Objects
{
    public class Paper
    {

        public string ID { get; set; }

        public string ParentID { get; set; }

        public string Title { get; set; }

        public string Doi { get; set; }

        public List<string> Authors { get; set; }

        public string Abstract { get; set; }

        public HashSet<string> Tags { get; set; }

    }

}
