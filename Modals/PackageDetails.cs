using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PkgCreation.Modals
{
    public class PackageDetails
    {
        public string package_name { get; set; }
        public int PackageID { get; set; }
        public string ecu_name { get; set; }
        public string ecu_app_swno { get; set; }
        public string newversion { get; set; }
        public int TotalCount { get; set; }
        public string createddate { get; set; }
        public string rb_newversion { get; set; }
        public string ApplicableECU { get; set; }
        public string DependentECU { get; set; }
        public string vehicle_type { get; set; }
    }
}
