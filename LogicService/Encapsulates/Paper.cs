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

        public List<People> Author { get; set; }

        public StringBuilder Abstract { get; set; }

        public List<string> Topic { get; set; }

        public List<Cite> Reference { get; set; }

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

    public class Cite
    {

        public Paper Article { get; set; }

        public string Expression { get; set; }

        public string Reason { get; set; }

    }
}
