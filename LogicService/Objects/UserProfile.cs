using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicService.Objects
{
    public class UserProfile
    {

        public string UserName { get; set; }

        public List<UserTrace> UserTraces { get; set; }

    }

    public class UserTrace
    {

        public string Device { get; set; }

        public string IP { get; set; }

        public DateTime Time { get; set; }

    }

}
