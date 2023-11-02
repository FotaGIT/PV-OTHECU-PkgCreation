using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PkgCreation.Modals
{
    public class Appsettings
    {
        public int ID { get; set; }
        public string Token_username { get; set; }
        public string Token_password { get; set; }
        public string Token_tenant_id { get; set; }
        public string Layer3URL { get; set; }
        public string Token_Timeout { get; set; }
        public string SMTPHOST { get; set; }
        public string SMTPUSERNAME { get; set; }
        public string SMTPPASSWORD { get; set; }
        public string DB_ConnectionString { get; set; }
        public int NoOfThread { get; set; }
        public int ParallelThread { get; set; }
        public int PreCondition_Minute { get; set; }
        public int PreCondition_EngineRPM { get; set; }
        public int PreCondition_GSMStrength { get; set; }
        public int NoOfRetry { get; set; }
        public int NoOfVins { get; set; }
        public int? TodayRetryCount { get; set; }
        public int TimeIntervalBtwnFota { get; set; }
        public int OtherRetryLimit { get; set; }
        public int CommandTimeOutRetry { get; set; }
        public int TotalCmdTimedOutVin { get; set; }
        public string IsBenchDevice { get; set; }
        public string TokenUrl { get; set; }
        public string FirmVersionUrl { get; set; }
        public string DeviceStateUrl { get; set; }
        public string DownloadUrl { get; set; }
        public string InstallUrl { get; set; }
        public string CommandResUrl { get; set; }
        public string SubscriptionAPIUrl { get; set; }
        public string BalanceAPIUrl { get; set; }
        public string MoveAPIAuthenticationToken { get; set; }
        public string MoveAPIInitiatorID { get; set; }
        public string MoveAPIUserName { get; set; }
        public string MoveAPIPassword { get; set; }
        public string ToMailIds { get; set; }
        public string FromMailId { get; set; }
        public string InCCMailIds { get; set; }
        public string PortalTelemetryUrl { get; set; }
        public string PortalFirmVersionUrl { get; set; }
        public string PortalCommandResUrl { get; set; }
        public string PortalDeviceIdUrl { get; set; }
        public int PVAccountId { get; set; }
        public int EVAccountId { get; set; }
        public string CotaAPIUrl { get; set; }
        public string PortalSetCertStatusUrl { get; set; }
        public string Environment { get; set; }
        public string IsEnvironmentActive { get; set; }
        public string RemoteVSOHUrl { get; set; }
        public string InitiateFOTAUrl { get; set; }
        public string PkgUploadUrl { get; set; }
        public string PkgFolderPath { get; set; }
        public string ZippedPkgFolderPath { get; set; }
    }
}
