﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PkgCreation.Modals
{
    public class FotaTblPackageDataDto
    {
        public int packageid { get; set; }
        public string package_name { get; set; }
        public string ecu_name { get; set; }
        public string isdependentecu { get; set; }
        public string priority { get; set; }
        public string update_type { get; set; }
        public string rollback_ecu { get; set; }
        public string campaign_no { get; set; }
        public string validity { get; set; }
        public string whats_new_text { get; set; }
        public string release_date { get; set; }
        public string release_type { get; set; }
        public string update_process_type { get; set; }
        public string vehicle_type { get; set; }
        public string fota_type { get; set; }
        public string config_locator { get; set; }
        public string background_fota { get; set; }
        public string background_fota_message { get; set; }
        public string user_confirmation { get; set; }
        public string update_size { get; set; }
        public string estimated_time { get; set; }
        public string oem_release_summary { get; set; }
        public string cust_release_summary { get; set; }
        public string success_message { get; set; }
        public string vcu_newversion { get; set; }
        public string vcu_blocksize { get; set; }
        public string vcu_stmin { get; set; }
        public string vcu_lib_name { get; set; }
        public string vcu_FW_Data_Source { get; set; }
        public string rb_vcu_newversion { get; set; }
        public string rb_vcu_blocksize { get; set; }
        public string rb_vcu_stmin { get; set; }
        public string rb_vcu_lib_name { get; set; }
        public string rb_vcu_FW_Data_Source { get; set; }
        public string hu_metafile_Source { get; set; }
        public string hu_checksum { get; set; }
        public string hu_sw_version { get; set; }
        public string hu_base_sw_version { get; set; }
        public string hu_lib_name { get; set; }
        public string rb_hu_metafile_Source { get; set; }
        public string rb_hu_checksum { get; set; }
        public string rb_hu_sw_version { get; set; }
        public string rb_hu_base_sw_version { get; set; }
        public string rb_hu_lib_name { get; set; }
        public string bcm_vehicle_manspare_partno { get; set; }
        public string bcm_ecu_app_swno { get; set; }
        public string bcm_appsw_identification { get; set; }
        public string bcm_ecu_calib_swno { get; set; }
        public string bcm_ccid { get; set; }
        public string bcm_bsw_data_source { get; set; }
        public string bcm_app_data_source { get; set; }
        public string bcm_calib_data_source { get; set; }
        public string bcm_sbl_data_source { get; set; }
        public string bcm_basicswsrc { get; set; }
        public string bcm_sblcrc { get; set; }
        public string bcm_appcrc { get; set; }
        public string bcm_calcrc { get; set; }
        public string bcm_lib_name { get; set; }
        public string rb_bcm_vehicle_manspare_partno { get; set; }
        public string rb_bcm_ecu_app_swno { get; set; }
        public string rb_bcm_appsw_identification { get; set; }
        public string rb_bcm_ecu_calib_swno { get; set; }
        public string rb_bcm_ccid { get; set; }
        public string rb_bcm_bsw_data_source { get; set; }
        public string rb_bcm_app_data_source { get; set; }
        public string rb_bcm_calib_data_source { get; set; }
        public string rb_bcm_sbl_data_source { get; set; }
        public string rb_bcm_basicswsrc { get; set; }
        public string rb_bcm_sblcrc { get; set; }
        public string rb_bcm_appcrc { get; set; }
        public string rb_bcm_calcrc { get; set; }
        public string rb_bcm_lib_name { get; set; }
        public string fatc_application_sw_version { get; set; }
        public string fatc_calibration_sw_version { get; set; }
        public string fatc_ccid { get; set; }
        public string fatc_APP_Data_Source { get; set; }
        public string fatc_Calibration_Data_Source { get; set; }
        public string fatc_lib_name { get; set; }
        public string rb_fatc_application_sw_version { get; set; }
        public string rb_fatc_calibration_sw_version { get; set; }
        public string rb_fatc_ccid { get; set; }
        public string rb_fatc_APP_Data_Source { get; set; }
        public string rb_fatc_Calibration_Data_Source { get; set; }
        public string rb_fatc_lib_name { get; set; }
        public string tcm_sw_version { get; set; }
        public string tcm_validationtimeout { get; set; }
        public string tcm_bb_version { get; set; }
        public string tcm_updatetype { get; set; }
        public string tcm_isforceupdate { get; set; }
        public string tcm_ishufota { get; set; }
        public string tcm_FW_Data_Source { get; set; }
        public string tcm_lib_name { get; set; }
        public string rb_tcm_sw_version { get; set; }
        public string rb_tcm_validationtimeout { get; set; }
        public string rb_tcm_bb_version { get; set; }
        public string rb_tcm_updatetype { get; set; }
        public string rb_tcm_isforceupdate { get; set; }
        public string rb_tcm_ishufota { get; set; }
        public string rb_tcm_FW_Data_Source { get; set; }
        public string rb_tcm_lib_name { get; set; }
        public string createdby { get; set; }
        public string createddate { get; set; }
        public string modifiedby { get; set; }
        public string modifieddate { get; set; }
        public string approvedby { get; set; }
        public string approveddate { get; set; }
        public string isdeleted { get; set; }
        public string deletedby { get; set; }
        public string deleteddate { get; set; }
        public string ispackagecreated { get; set; }
        public string packagezipsize { get; set; }
    }
}
