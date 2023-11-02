using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PkgCreation.Modals;
using PkgCreation.Repository.Interface;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Cors;

namespace PkgCreation.Controllers
{
    [Route("api/Plan")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    public class PlanController : ControllerBase
    {
        private readonly IManualPlanRepository _ManualPlanRepository;
        private IExceptionErrorLog _exceptionErrorLog;
        public PlanController(IManualPlanRepository objplan, IExceptionErrorLog objexceptionErrorLog)
        {
            _ManualPlanRepository = objplan;
            _exceptionErrorLog = objexceptionErrorLog;
        }

        [HttpPost("CreatePlanManually")]
        [EnableCors("AllowOrigin")]
        public async Task<IActionResult> CreatePlanManually(ManualData objdata)
        {
            try
            {
                var appdata = _ManualPlanRepository.Getappsettingdata("OthECU_QA");

                string FromMailId = objdata.LoginIDEmail;

                GetToken objGetToken = GetToken(appdata).Result;
                if (objGetToken != null)
                {
                    FileUploadDto objresult = UploadFOTAPackage(objGetToken.access_token, appdata, objdata.package.package_name, objdata.package.newversion, objdata.devicetype).Result;
                    if (objresult.statusCode == "201")
                    {
                        return Ok(await _ManualPlanRepository.CreatePlanManually(objdata, FromMailId));
                    }
                    else if(objresult.statusCode == "400" && objresult.message== "Provided file to upload already exist at the CPI.")
                    {
                        return Ok(await _ManualPlanRepository.CreatePlanManually(objdata, FromMailId));
                    }
                    else
                    {
                        return Ok(new { Value = objresult.message });
                    }
                }
                else
                {
                    return Ok(new { Value = "Something went wrong with token API, please try again!" });
                }
            }
            catch(Exception ex)
            {
                ExceptionlogModal logger = new ExceptionlogModal();
                logger.Exception = ex.Message;
                logger.ExceptionDetails = "PlanController";
                logger.StackTrace = ex.StackTrace;
                logger.CreatedBy = ex.Source;
                _exceptionErrorLog.FnExceptionErrorLog(logger);

                return Ok(new { Value = "Something went wrong with token API, please try again!" });
            }
        }

        public async Task<GetToken> GetToken(Appsettings appdata)
        {
            string resultContent = String.Empty;
            HttpResponseMessage result = null;
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            string userNameConfig = appdata.Token_username;
            string passwordConfig = appdata.Token_password;
            string tenantIdConfig = appdata.Token_tenant_id;
            string Url = appdata.TokenUrl;

            using (HttpClient client = new HttpClient(clientHandler))
            {
                client.BaseAddress = new Uri(Url);
                var APIparameters = new
                {
                    username = userNameConfig,
                    password = passwordConfig,
                    tenant_id = tenantIdConfig
                };
                try
                {
                    result = client.PostAsync("t/iot/idm/token", new StringContent(JsonConvert.SerializeObject(APIparameters), Encoding.UTF8, "application/json")).Result;
                    if (result.IsSuccessStatusCode)
                    {
                        resultContent = await result.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        resultContent = await result.Content.ReadAsStringAsync();
                    }
                }
                catch(Exception ex)
                {
                    ExceptionlogModal logger = new ExceptionlogModal();
                    logger.Exception = ex.Message;
                    logger.ExceptionDetails = "PlanController/GetToken";
                    logger.StackTrace = ex.StackTrace;
                    logger.CreatedBy = ex.Source;
                    _exceptionErrorLog.FnExceptionErrorLog(logger);
                }
            }
            return JsonConvert.DeserializeObject<GetToken>(resultContent);
        }

        public async Task<FileUploadDto> UploadFOTAPackage(string AuthToken, Appsettings appdata, string package_name, string newversion, string devicetype)
        {
            try
            {
                string resultContent = String.Empty;
                string AuthorizationToken = AuthToken;
                string result = null;
                await using var stream = System.IO.File.OpenRead(appdata.ZippedPkgFolderPath + "/" + package_name + ".zip");
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                string Url = appdata.PkgUploadUrl;

                fileDetails objfile = new fileDetails();
                objfile.fileName = package_name; //+ ".zip";
                List<fileDetails> lstfile = new List<fileDetails>();
                lstfile.Add(objfile);

                var pardevicetype = new
                {
                    deviceTypeName = devicetype,
                    firmwareVersion = newversion,
                    fileDetails = lstfile
                };

                using (HttpClient client = new HttpClient(clientHandler))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthorizationToken);
                    client.BaseAddress = new Uri(Url);


                    var ur = String.Format("t/iot/cpi/fota/firmware?");

                    var APIparameters = new
                    {
                        category = "DEVICE_TYPE",
                        deviceTypes = pardevicetype,
                    };

                    using var content = new MultipartFormDataContent {

                        { new StreamContent(stream), "files", package_name },
                        { new StringContent(JsonConvert.SerializeObject(APIparameters), Encoding.UTF8, "application/json"),"fileDetails" }
                    };

                    using (HttpResponseMessage response = await client.PostAsync(ur, content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            result = response.Content.ReadAsStringAsync().Result;
                            var result1 = JsonConvert.DeserializeObject<FileUploadDto>(result);
                            return result1;
                        }
                        else if(response.StatusCode.ToString() == "BadRequest")
                        {
                            result = response.Content.ReadAsStringAsync().Result;
                            var result2 = JsonConvert.DeserializeObject<FileUploadDto>(result);

                            ExceptionlogModal logger1 = new ExceptionlogModal();
                            logger1.Exception ="Layer 3 API Response:- StatusCode: "+ Convert.ToString(response.StatusCode)+ " Error: "+ result2.message;
                            logger1.CreatedBy = "PlanController/UploadFOTAPackage";
                            _exceptionErrorLog.FnExceptionErrorLog(logger1);

                            return result2;
                        }
                        else
                        {
                            FileUploadDto result3 = new FileUploadDto();
                            result3.statusCode = Convert.ToString(response.StatusCode);
                            result3.message = "File Upload Failed.";

                            ExceptionlogModal logger2 = new ExceptionlogModal();
                            logger2.Exception = "Layer 3 API Response:- StatusCode: " + Convert.ToString(response.StatusCode) + " Error: "  + response;
                            logger2.CreatedBy = "PlanController/UploadFOTAPackage";
                            _exceptionErrorLog.FnExceptionErrorLog(logger2);

                            return result3;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionlogModal logger = new ExceptionlogModal();
                logger.Exception = ex.Message;
                logger.ExceptionDetails = "PlanController/UploadFOTAPackage";
                logger.StackTrace = ex.StackTrace;
                logger.CreatedBy = ex.Source;
                _exceptionErrorLog.FnExceptionErrorLog(logger);

                FileUploadDto result4 = new FileUploadDto();
                result4.statusCode = "";
                result4.message = ex.Message;
                return result4;
            }
            return null;
        }
    }
}
