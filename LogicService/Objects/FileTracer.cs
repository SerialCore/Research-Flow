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
    public class FileTracer
    {

        public string FileName { get; set; }

        public string FilePosition { get; set; }

        public DateTime DateModified { get; set; }

        public bool WillbeSync { get; set; }

        public bool IsSync { get; set; }

    }
}
