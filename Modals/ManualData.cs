using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PkgCreation.Modals
{
    public class ManualData
    {
        public PackageDetails package { get; set; }
        public SingleStringDto[] vinsunderpkg { get; set; }
        public string devicetype { get; set; }
        public string createdby { get; set; }
        public string LoginIDEmail { get; set; }
    }
    public class SingleStringDto
    {
        public string Value { get; set; }
    }
    public class SingleIdDto
    {
        public int Id { get; set; }
    }
}
