using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PkgCreation.Modals
{
    public class EmailDto
    {
        public string fromMail { get; set; }
        public string toMail { get; set; }
        public string ccMail { get; set; }
        public string bccMail { get; set; }
    }
}
