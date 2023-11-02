using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PkgCreation.Modals
{
    public class ExceptionlogModal
    {
        public int ErrorLogId { get; set; }
        public string Exception { get; set; }
        public string ExceptionDetails { get; set; }
        public string StackTrace { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public class QueryResult
    {
        public string Result { get; set; }
    }
}
