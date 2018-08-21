using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicService.Encapsulates
{
    public class Paper
    {

        public string Title { get; set; }

        public string DOI { get; set; }

        public List<People> Authors { get; set; }

        public string Abstract { get; set; }

        public List<string> Topic { get; set; }

        public List<Paper> References { get; set; }

    }

    public class People
    {

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Tele { get; set; }

        public string Organization { get; set; }

    }

}
