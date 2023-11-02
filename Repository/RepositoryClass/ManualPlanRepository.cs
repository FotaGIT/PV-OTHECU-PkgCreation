using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PkgCreation.Repository.Interface;
using PkgCreation.Modals;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Globalization;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace PkgCreation.Repository.RepositoryClass
{
    public class ManualPlanRepository : IManualPlanRepository
    {
        private readonly CustomContext _customContext = new CustomContext();
        private IConfiguration _configuration;

        public ManualPlanRepository(IConfiguration objconfig)
        {
            _configuration = objconfig;
        }

        public Appsettings Getappsettingdata(string Environment)
        {
            try
            {
                var result = _customContext.Set<Appsettings>().FromSqlRaw("call OthECU_USP_GetAppsettingdata({0})", Environment).ToList();
                return result[0];
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public async Task<SingleStringDto> CreatePlanManually(ManualData manual, string FromMailId)
        {
            SingleStringDto st = new SingleStringDto();
            try
            {
                    List<PackageDetails> p = new List<PackageDetails>();
                    p.Add(manual.package);
                    var obj = JsonSerializer.Serialize(p);
                    var obj1 = JsonSerializer.Serialize(manual.vinsunderpkg);
                    ManualPkgres res = new ManualPkgres();
                    var res1 = _customContext.Set<ManualPkgres>().FromSqlRaw("call USP_OManuallyPkgCreation({0},{1})", obj, obj1).ToList();

                    PlanningDto pl = new PlanningDto();
                    pl.PackageName = res1[0].PackageName;
                    pl.software_ID = res1[0].Software_ID;
                    pl.targetedVehicles = manual.vinsunderpkg.Count();
                    pl.CreatedBy = manual.createdby; //"TEST USER";
                    pl.DeviceType = manual.devicetype;
                    pl.PkgInstanceId = res1[0].PkgInstanceId;
                    int planid = await createplan(pl);
                    if (planid > 0)
                    {
                        VinsDto[] vins = new VinsDto[pl.targetedVehicles];
                        for (int i = 0; i < pl.targetedVehicles; i++)
                        {
                            vins[i] = new VinsDto()
                            {
                                vin = manual.vinsunderpkg[i].Value
                            };
                        }

                        pl.PlanVins = vins;
                        var ret = InsertplanningVin(planid, pl, res1[0].PkgInstanceId);
                    }
                    
                   //EmailTo_Approver(pl,FromMailId,planid);

                    st.Value = "success";

                    return st;
            }
            catch (Exception ex)
            {
                st.Value = ex.InnerException.ToString();
            }
            return st;
        }

        public async Task<int> createplan(PlanningDto plan)
        {
            int resultid = 0;
            await Task.Run(() =>
            {
                string CrDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm tt", CultureInfo.InvariantCulture);
                string dateForplanNumber = DateTime.Now.ToString("ddMMMyyyy", CultureInfo.InvariantCulture);
                // var planobj= new PlanningDto();
                List<object> lst = new List<object>();
                var planobj = new
                {
                    CreatedDate = CrDate,
                    FotaType = "Normal",
                    Buid = 1,
                    vehicleConfiguration = plan.PackageName,
                    software_ID = plan.software_ID,
                    targetedVehicles = plan.targetedVehicles,
                    CreatedBy = plan.CreatedBy,
                    DeviceType = plan.DeviceType,
                    PkgInstanceId = plan.PkgInstanceId
                };
                lst.Add(planobj);
                var obj = JsonSerializer.Serialize(lst);
                var retId = _customContext.Set<SingleIdDto>().FromSqlRaw("call USP_OCreateplan({0})", obj).ToList();
                resultid = retId[0].Id;


            });
            return resultid;
        }

        public int InsertplanningVin(int planid, PlanningDto plan,int pkgid)
        {
            List<object> lst = new List<object>();
            var planvinobj = new
            {
                regionId = 1,
                Buid = 1,
                PackageName = plan.PackageName,
                planId = planid,
                pkginstanceid = pkgid
            };
            lst.Add(planvinobj);
            var obj = JsonSerializer.Serialize(lst);
            var objVin = JsonSerializer.Serialize(plan.PlanVins);
            return _customContext.Database.ExecuteSqlCommand("call USP_OInsertPlanningVins({0},{1})", objVin, obj);
        }

        public async Task<bool> EmailTo_Approver(PlanningDto plan, string FromMailId, int insertedPlanID)
        {
            #region For Email
            string sendmailbody = "";
            string mailbody;
            string FotaPlan_mailbody;
            string Subject = "Fota Planning Approval";
            mailbody = "Dear FOTA Plan Approver,<br><br>";

            FotaPlan_mailbody = mailbody + "The  Plan Id<b> " + insertedPlanID + " </b> for firmware version <b> " + "4.1.1" + " </b>";
            FotaPlan_mailbody = FotaPlan_mailbody + "consisting of " + plan.targetedVehicles + " planned VINs quantity has been submitted";
            FotaPlan_mailbody = FotaPlan_mailbody + " for approval by " + plan.CreatedBy + " <br><br>";
            FotaPlan_mailbody = FotaPlan_mailbody + " Kindly review and approve/reject this FOTA deployment plan.<br><br>";
            FotaPlan_mailbody = FotaPlan_mailbody + " OTH-ECU Portal Link:<br>";
            FotaPlan_mailbody = FotaPlan_mailbody + " <a href='https://cvpfotaportal.tatamotors.com/#/'>https://cvpfotaportal.tatamotors.com</a><br><br>";
            FotaPlan_mailbody = FotaPlan_mailbody + " Thanks & Regards,<br>";
            FotaPlan_mailbody = FotaPlan_mailbody + "FOTA Planner Team<br><br>";
            FotaPlan_mailbody = FotaPlan_mailbody + "This is a system generated Email. Do not reply.";
            sendmailbody = FotaPlan_mailbody;
            #endregion
            await SendMail(sendmailbody, Subject, FromMailId, insertedPlanID);

            return true;
        }

        public async Task<bool> SendMail(string Body, string Subject, string FromMailId, int PlanId)
        {
            try
            {
                await Task.Run(() =>
                {
                    EmailDto objEmailModel = new EmailDto();
                    objEmailModel = GetEmailList(PlanId);
                    var To = objEmailModel.toMail;
                    Subject = Subject.Replace('\r', ' ').Replace('\n', ' ');
                    Subject = Subject.Substring(0, Math.Min(Subject.Length, 78));
                    MailMessage mm = new MailMessage();
                    To = To.Replace(';', ',');
                    if (To != "")
                    {
                        mm.To.Add(To);
                    }
                    string BCCMailId = objEmailModel.bccMail;
                    if (BCCMailId != null && BCCMailId != "")
                    {
                        mm.Bcc.Add(BCCMailId);
                    }
                    mm.From = new MailAddress(FromMailId);
                    mm.Subject = Subject;
                    mm.Body = Body;
                    mm.IsBodyHtml = true;
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = _configuration.GetValue<string>("Emailfields:Host");
                    smtp.EnableSsl = false;
                    NetworkCredential NetworkCred = new NetworkCredential();
                    NetworkCred.UserName = _configuration.GetValue<string>("Emailfields:UserName");
                    NetworkCred.Password = _configuration.GetValue<string>("Emailfields:Password");
                    smtp.UseDefaultCredentials = true;
                    smtp.Credentials = NetworkCred;
                    smtp.Port = 25;
                    smtp.Send(mm);
                });
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public EmailDto GetEmailList(int planId)
        {
            var lst = _customContext.Set<EmailDto>().FromSqlRaw("CALL USP_OGetEmailList('4',{0});", planId).ToList();
            return lst[0];
        }
    }
}
