using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PkgCreation.Modals
{
    public class PlanningDto
    {
        public string PackageName { get; set; }
        public string FotaType { get; set; }
        public int targetedVehicles { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public int? software_ID { get; set; }
        public int? Buid { get; set; }
        public VinsDto[] PlanVins { get; set; }
        public string DeviceType { get; set; }
        public int PkgInstanceId { get; set; }
    }

    public class VinsDto
    {
        public string vin { get; set; }
        public int Totalcount { get; set; }
    }
}
