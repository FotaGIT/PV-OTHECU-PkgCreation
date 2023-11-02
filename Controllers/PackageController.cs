using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PkgCreation.Modals;
using System.Xml;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using PkgCreation.Repository.Interface;
using System.Globalization;
using Microsoft.AspNetCore.Cors;

namespace PkgCreation.Controllers
{
    [Route("api/Package")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    public class PackageController : ControllerBase
    {
        private IConfiguration _configuration;
        private readonly IPacakgeRepository _PackageRepository;
        private IExceptionErrorLog _exceptionErrorLog;

        private string sourcepath = "";
        private string destinationpath = "";
        public PackageController(IConfiguration objconfig,IPacakgeRepository objpackagerepo,IExceptionErrorLog objexceptionErrorLog)
        {
            _configuration = objconfig;
            _PackageRepository = objpackagerepo;
            _exceptionErrorLog = objexceptionErrorLog;
            sourcepath = _configuration.GetValue<string>("MySettings:SourcePath");
            destinationpath = _configuration.GetValue<string>("MySettings:DestinationPath");
        }

        [HttpPost("CreatePackage")]
        [EnableCors("AllowOrigin")]
        public async Task<IActionResult> CreatePackage(List<FotaTblPackageDataDto> objdata)
        {
            string filesize = "";
            string zippedrootPath = Path.Combine(destinationpath, objdata[0].package_name+ ".zip"); //objdata[0].package_name
            string rootPath = Path.Combine(sourcepath, objdata[0].package_name);  // objdata[0].package_name
            string error = "";
            // Testing
            //try
            //{
                //ProcessStartInfo pro = new ProcessStartInfo();
                //pro.FileName = @"java.exe";
                //pro.WorkingDirectory = @"D:\Packaging Tool\";
                ////pro.Arguments = @"/c dir";
                ////pro.Arguments = "/k rename VCU_2_4_8_091220221737 VCU_2_4_8_091220221738";
                //pro.Arguments = "java -jar createFotaPackage.jar D:\\Packaging Tool\\PackagingTool.properties";

                //pro.UseShellExecute = false;
                //pro.RedirectStandardError = true;

                //Process proStart = new Process();
                //proStart.StartInfo = pro;
                //proStart.Start();

                //error = proStart.StandardError.ReadLine();
                //string output = "";


                //ProcessStartInfo pro = new ProcessStartInfo();
                //pro.FileName = @"D:\Backup\ConsoleApp1\bin\Debug\netcoreapp3.1\win10-x64\publish\ConsoleApp1.exe";
                ////pro.WorkingDirectory = @"C:\";
                //Process proStart = new Process();
                //proStart.StartInfo = pro;
                ////proStart.StartInfo.CreateNoWindow = true;
                ////proStart.StartInfo.UseShellExecute = false;
                ////proStart.StartInfo.RedirectStandardOutput = true;
                ////proStart.StartInfo.RedirectStandardError = true;
                ////proStart.OutputDataReceived += (sender, data) => Console.WriteLine(data.Data);
                ////proStart.ErrorDataReceived += (sender, data) => error = data.Data;
                //proStart.Start();
                ////proStart.BeginOutputReadLine();
                ////proStart.BeginErrorReadLine();
                ////var exited = proStart.WaitForExit(1000 * 10);     // (optional) wait up to 10 seconds
                ////Console.WriteLine($"exit {exited}");
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
            //using (var process = new Process())
            //{
            //    process.StartInfo.FileName = @"cmd.exe";
            //    process.StartInfo.Arguments = @"/c dir";      // print the current working directory information
            //    process.StartInfo.CreateNoWindow = true;
            //    process.StartInfo.UseShellExecute = false;
            //    process.StartInfo.RedirectStandardOutput = true;
            //    process.StartInfo.RedirectStandardError = true;

            //    process.OutputDataReceived += (sender, data) => Console.WriteLine(data.Data);
            //    process.ErrorDataReceived += (sender, data) => Console.WriteLine(data.Data);
            //    Console.WriteLine("starting");
            //    process.Start();
            //    process.BeginOutputReadLine();
            //    process.BeginErrorReadLine();
            //    var exited = process.WaitForExit(1000 * 10);     // (optional) wait up to 10 seconds
            //    Console.WriteLine($"exit {exited}");
            //}

            // End Testing



            try
            {
                await _PackageRepository.UpdatePkgCreated(objdata[0].package_name, null, null, "P");  // Updating package creation is inporgress
                CreateIndexfile(objdata, rootPath);

                foreach (var ecuitem in objdata)
                {
                    CreateConfigfiles(ecuitem, Path.Combine(rootPath, ecuitem.ecu_name, "Install"));
                    CreateRollbackConfigfiles(ecuitem, Path.Combine(rootPath, ecuitem.ecu_name, "Rollback"));
                }

                ProcessStartInfo ProcessInfo;
                Process process;

                //ProcessInfo = new ProcessStartInfo("java.exe", "-jar createFotaPackage.jar " + "D:\\FPT\\PackagingTool.properties " + objdata[0].package_name + " " + objdata[0].package_name);
                //ProcessInfo.WorkingDirectory = @"D:/FPT/";

                ProcessInfo = new ProcessStartInfo("java.exe", "-jar createFotaPackage.jar " + "C:\\Tushar\\FPT\\FPT_after_changes\\PackagingTool.properties " + objdata[0].package_name + " " + objdata[0].package_name);
                ProcessInfo.WorkingDirectory = @"C:/Tushar/FPT/FPT_after_changes/";
                ProcessInfo.UseShellExecute = false;
                ProcessInfo.RedirectStandardError = true;

                process = Process.Start(ProcessInfo);
                process.WaitForExit();

                error = process.StandardError.ReadToEnd();
                int exitcode = process.ExitCode;

                process.Close();

                if (exitcode == 1 & (error != null || error != ""))
                {
                    ExceptionlogModal logger = new ExceptionlogModal();
                    logger.Exception = error;
                    logger.ExceptionDetails = "CreatePackage/PackageTool";
                    logger.StackTrace = exitcode.ToString();
                    logger.CreatedBy = "";
                    _exceptionErrorLog.FnExceptionErrorLog(logger);

                    await _PackageRepository.UpdatePkgCreated(objdata[0].package_name, null, null, "N");
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = error });
                }

                decimal sizeinkb = new System.IO.FileInfo(zippedrootPath).Length/1024;
                filesize = Convert.ToString(Math.Round(sizeinkb / 1024,2));  // zip size in mb
                await _PackageRepository.UpdatePkgCreated(objdata[0].package_name, objdata[0].approvedby, filesize,"Y");

                return Ok(new { message = "Package Created Successfully" });

            }
            catch (Exception ex)
            {
                //LogError(ex, rootPath);

                ExceptionlogModal logger = new ExceptionlogModal();
                logger.Exception = ex.Message;
                logger.ExceptionDetails = "CreatePackage";
                logger.StackTrace = ex.StackTrace;
                logger.CreatedBy = ex.Source;
                _exceptionErrorLog.FnExceptionErrorLog(logger);

                await _PackageRepository.UpdatePkgCreated(objdata[0].package_name, null, null, "N");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex });
            }
        }

        public bool CreateIndexfile(List<FotaTblPackageDataDto> objdata, string rootpath)
        {
            var objindex = (from item in objdata where item.isdependentecu == "N" select item).FirstOrDefault();
            var objdependentecus = (from item in objdata where item.isdependentecu == "Y" select item).ToList();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("    ");
            settings.CloseOutput = true;
            settings.OmitXmlDeclaration = true;
            using (XmlWriter writer = XmlWriter.Create(Path.Combine(rootpath, "Index.xml"), settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("package-index");
                writer.WriteAttributeString("format-version", "1.0");
                writer.WriteStartElement("package-info"); //package-info start
                writer.WriteElementString("validity", objindex.validity);
                writer.WriteElementString("release-type", objindex.release_type);
                writer.WriteElementString("update-process-type", objindex.update_process_type);
                writer.WriteElementString("update-type", objindex.update_type);
                writer.WriteElementString("update-size", objindex.update_size);
                writer.WriteElementString("user-confirmation", objindex.user_confirmation);
                writer.WriteElementString("installation_time", objindex.release_type);
                writer.WriteElementString("oem-release-summary", objindex.oem_release_summary);
                writer.WriteElementString("cust-release-summary", objindex.cust_release_summary);
                writer.WriteElementString("release-date", objindex.release_date);
                writer.WriteElementString("success-message", objindex.success_message);
                writer.WriteElementString("background-fota-message", objindex.background_fota_message);
                writer.WriteElementString("campaign-no", objindex.campaign_no);
                writer.WriteElementString("estimated-time", objindex.estimated_time);
                writer.WriteElementString("whats-new-text", objindex.whats_new_text);
                writer.WriteEndElement();                // package-info end

                writer.WriteStartElement("ecus");//ecus start
                foreach (var _ecu in objdata)
                {
                    if (_ecu.ecu_name == "BCM")
                    {
                        // string[] rollbackeculist=null;
                        // if(!string.IsNullOrEmpty(_ecu.rollback_ecu)){
                        //     rollbackeculist = (_ecu.rollback_ecu).Split(',');
                        // }
                        writer.WriteStartElement("ecu"); //ecu start
                        writer.WriteAttributeString("name", _ecu.ecu_name);
                        writer.WriteAttributeString("priority", _ecu.priority);
                        writer.WriteAttributeString("fota-type", _ecu.fota_type);
                        writer.WriteAttributeString("background-fota", _ecu.background_fota);
                        writer.WriteElementString("config-locator", "./" + _ecu.ecu_name);

                        writer.WriteStartElement("commands");//commands start

                        writer.WriteStartElement("command");    //command start
                        writer.WriteAttributeString("block-name", "block-1");
                        writer.WriteAttributeString("priority", _ecu.priority);
                        writer.WriteString("com.bosch.platform.fota.diagsequence.tml.bcm");
                        writer.WriteEndElement();             //command End

                        writer.WriteEndElement();          //commands End

                        writer.WriteStartElement("sequences"); //sequences start

                        writer.WriteStartElement("sequence");   //sequences start
                        writer.WriteAttributeString("lib-nam", _ecu.bcm_lib_name);
                        writer.WriteAttributeString("background-fota", _ecu.background_fota);
                        writer.WriteEndElement();               //sequence END

                        writer.WriteEndElement();            //sequences END

                        if (_ecu.isdependentecu == "N" && objdependentecus.Count > 0)
                        {
                            writer.WriteStartElement("rollback-ecus");//rollback-ecus Start
                            foreach (var ecuitem in objdependentecus)
                            {
                                writer.WriteStartElement("rollback-ecu");//rollback-ecu start
                                writer.WriteAttributeString("name", ecuitem.ecu_name);
                                writer.WriteEndElement();      //rollback-ecu END
                            }
                            writer.WriteEndElement();                //rollback-ecus END
                        }
                        writer.WriteEndElement();      //ecu END
                    }
                    else if (_ecu.ecu_name == "FATC")
                    {
                        writer.WriteStartElement("ecu"); //ecu start
                        writer.WriteAttributeString("name", _ecu.ecu_name);
                        writer.WriteAttributeString("priority", _ecu.priority);
                        writer.WriteAttributeString("fota-type", _ecu.fota_type);
                        writer.WriteAttributeString("background-fota", _ecu.background_fota);
                        writer.WriteElementString("config-locator", "./" + _ecu.ecu_name);

                        writer.WriteStartElement("commands");//commands start

                        writer.WriteStartElement("command");    //command start
                        writer.WriteAttributeString("block-name", "block-1");
                        writer.WriteAttributeString("priority", _ecu.priority);
                        writer.WriteString("com.bosch.platform.fota.diagsequence.tml.fatc");
                        writer.WriteEndElement();             //command End

                        writer.WriteEndElement();          //commands End

                        writer.WriteStartElement("sequences"); //sequences start

                        writer.WriteStartElement("sequence");   //sequences start
                        writer.WriteAttributeString("lib-nam", _ecu.fatc_lib_name);
                        writer.WriteAttributeString("background-fota", _ecu.background_fota);
                        writer.WriteEndElement();               //sequence END

                        writer.WriteEndElement();            //sequences END
                        if (_ecu.isdependentecu == "N" && objdependentecus.Count > 0)
                        {
                            writer.WriteStartElement("rollback-ecus");//rollback-ecus Start
                            foreach (var ecuitem in objdependentecus)
                            {
                                writer.WriteStartElement("rollback-ecu");//rollback-ecu start
                                writer.WriteAttributeString("name", ecuitem.ecu_name);
                                writer.WriteEndElement();      //rollback-ecu END
                            }
                            writer.WriteEndElement();                //rollback-ecus END
                        }
                        writer.WriteEndElement();      //ecu END
                    }
                    else if (_ecu.ecu_name == "VCU")
                    {
                        writer.WriteStartElement("ecu"); //ecu start
                        writer.WriteAttributeString("name", _ecu.ecu_name);
                        writer.WriteAttributeString("priority", _ecu.priority);
                        writer.WriteAttributeString("fota-type", _ecu.fota_type);
                        writer.WriteAttributeString("background-fota", _ecu.background_fota);
                        writer.WriteElementString("config-locator", "./" + _ecu.ecu_name);

                        writer.WriteStartElement("commands");//commands start

                        writer.WriteStartElement("command");    //command start
                        writer.WriteAttributeString("type", "installation");
                        writer.WriteAttributeString("block-name", "block-1");
                        writer.WriteAttributeString("priority", _ecu.priority);
                        writer.WriteString("com.bosch.itrams.fota.tml.pv.fotasequence.vcu.Installation");
                        writer.WriteEndElement();             //command End

                        writer.WriteStartElement("command");    //command start
                        writer.WriteAttributeString("type", "activation");
                        writer.WriteAttributeString("block-name", "block-1");
                        writer.WriteAttributeString("priority", _ecu.priority);
                        writer.WriteString("com.bosch.itrams.fota.tml.pv.fotasequence.vcu.Activation");
                        writer.WriteEndElement();             //command End

                        writer.WriteStartElement("command");    //command start
                        writer.WriteAttributeString("type", "rollback");
                        writer.WriteAttributeString("block-name", "block-1");
                        writer.WriteAttributeString("priority", _ecu.priority);
                        writer.WriteString("com.bosch.itrams.fota.tml.pv.fotasequence.vcu.Rollback");
                        writer.WriteEndElement();             //command End

                        writer.WriteEndElement();          //commands End

                        writer.WriteStartElement("sequences"); //sequences start

                        writer.WriteStartElement("sequence");   //sequences start
                        writer.WriteAttributeString("lib-nam", _ecu.vcu_lib_name);
                        writer.WriteAttributeString("background-fota", _ecu.background_fota);
                        writer.WriteEndElement();               //sequence END

                        writer.WriteEndElement();            //sequences END
                        if (_ecu.isdependentecu == "N" && objdependentecus.Count > 0)
                        {
                            writer.WriteStartElement("rollback-ecus");//rollback-ecus Start
                            foreach (var ecuitem in objdependentecus)
                            {
                                writer.WriteStartElement("rollback-ecu");//rollback-ecu start
                                writer.WriteAttributeString("name", ecuitem.ecu_name);
                                writer.WriteEndElement();      //rollback-ecu END
                            }
                            writer.WriteEndElement();                //rollback-ecus END
                        }
                        writer.WriteEndElement();      //ecu END   
                    }
                    else if (_ecu.ecu_name == "HU")
                    {
                        writer.WriteStartElement("ecu"); //ecu start
                        writer.WriteAttributeString("name", _ecu.ecu_name);
                        writer.WriteAttributeString("priority", _ecu.priority);
                        writer.WriteAttributeString("fota-type", _ecu.fota_type);
                        writer.WriteAttributeString("background-fota", _ecu.background_fota);
                        writer.WriteElementString("config-locator", "./" + _ecu.ecu_name);

                        if (_ecu.isdependentecu == "N" && objdependentecus.Count > 0)
                        {
                            writer.WriteStartElement("rollback-ecus");//rollback-ecus Start
                            foreach (var ecuitem in objdependentecus)
                            {
                                writer.WriteStartElement("rollback-ecu");//rollback-ecu start
                                writer.WriteAttributeString("name", ecuitem.ecu_name);
                                writer.WriteEndElement();      //rollback-ecu END
                            }
                            writer.WriteEndElement();                //rollback-ecus END
                        }
                        writer.WriteEndElement();      //ecu END
                    }
                    else if (_ecu.ecu_name == "TCM")
                    {
                        writer.WriteStartElement("ecu"); //ecu start
                        writer.WriteAttributeString("name", _ecu.ecu_name);
                        writer.WriteAttributeString("priority", _ecu.priority);
                        writer.WriteAttributeString("fota-type", _ecu.fota_type);
                        writer.WriteAttributeString("background-fota", _ecu.background_fota);
                        writer.WriteElementString("config-locator", "./" + _ecu.ecu_name);

                        writer.WriteStartElement("commands");//commands start

                        writer.WriteStartElement("command");    //command start
                        writer.WriteAttributeString("block-name", "block-1");
                        writer.WriteAttributeString("priority", _ecu.priority);
                        writer.WriteString("com.bosch.platform.fota.diagsequence.tml.tcm");
                        writer.WriteEndElement();             //command End

                        writer.WriteEndElement();          //commands End

                        if (_ecu.isdependentecu == "N" && objdependentecus.Count > 0)
                        {
                            writer.WriteStartElement("rollback-ecus");//rollback-ecus Start
                            foreach (var ecuitem in objdependentecus)
                            {
                                writer.WriteStartElement("rollback-ecu");//rollback-ecu start
                                writer.WriteAttributeString("name", ecuitem.ecu_name);
                                writer.WriteEndElement();      //rollback-ecu END
                            }
                            writer.WriteEndElement();                //rollback-ecus END
                        }
                        writer.WriteEndElement();      //ecu END
                    }
                }
                writer.WriteEndElement(); //ecus End

                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
            }
            return true;
        }

        public bool CreateConfigfiles(FotaTblPackageDataDto _ecu, string rootpath)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            using (XmlWriter writer = XmlWriter.Create(Path.Combine(rootpath, "Config.xml"), settings))
            {
                // foreach (var _ecu in objdata)
                // {
                if (_ecu.ecu_name == "VCU")
                {
                    writer.WriteStartDocument();

                    writer.WriteStartElement("blocks"); //blocks start
                    writer.WriteAttributeString("ecu-name", _ecu.ecu_name);
                    writer.WriteAttributeString("format-version", "1.0");

                    writer.WriteStartElement("block"); //block start
                    writer.WriteAttributeString("name", "block-1");
                    writer.WriteElementString("priority", _ecu.priority);
                    writer.WriteStartElement("property"); //property start

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "FW-Data-Source");
                    writer.WriteString(_ecu.vcu_FW_Data_Source);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "BlockSize");
                    writer.WriteString(_ecu.vcu_blocksize);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "NewVersion");
                    writer.WriteString(_ecu.vcu_newversion);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "StMin");
                    writer.WriteString(_ecu.vcu_stmin);
                    writer.WriteEndElement();             //item End

                    writer.WriteEndElement();      //property END
                    writer.WriteEndElement();      //block END
                    writer.WriteEndElement();      //blocks END
                    writer.WriteEndDocument();
                    writer.Flush();
                }
                else if (_ecu.ecu_name == "HU")
                {
                    writer.WriteStartDocument();

                    writer.WriteStartElement("blocks"); //blocks start
                    writer.WriteAttributeString("ecu-name", _ecu.ecu_name);
                    writer.WriteAttributeString("format-version", "1.0");

                    writer.WriteStartElement("block"); //block start
                    writer.WriteAttributeString("name", "block-1");
                    writer.WriteElementString("priority", _ecu.priority);
                    writer.WriteStartElement("property"); //property start

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "metafile-Source");
                    writer.WriteString(_ecu.hu_metafile_Source);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "checksum");
                    writer.WriteString(_ecu.hu_checksum);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "sw_version");
                    writer.WriteString(_ecu.hu_sw_version);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "base-sw-version");
                    writer.WriteString(_ecu.hu_base_sw_version);
                    writer.WriteEndElement();             //item End

                    writer.WriteEndElement();      //property END
                    writer.WriteEndElement();      //block END
                    writer.WriteEndElement();      //blocks END
                    writer.WriteEndDocument();
                    writer.Flush();
                }
                else if (_ecu.ecu_name == "BCM")
                {
                    writer.WriteStartDocument();

                    writer.WriteStartElement("blocks"); //blocks start
                    writer.WriteAttributeString("ecu-name", _ecu.ecu_name);
                    writer.WriteAttributeString("format-version", "1.0");

                    writer.WriteStartElement("block"); //block start
                    writer.WriteAttributeString("name", "block-1");
                    writer.WriteElementString("priority", _ecu.priority);
                    writer.WriteStartElement("property"); //property start

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "BCM_DID_F187");
                    writer.WriteString(_ecu.bcm_vehicle_manspare_partno);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "BCM_DID_7241");
                    writer.WriteString(_ecu.bcm_ecu_app_swno);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "BCM_DID_F181");
                    writer.WriteString(_ecu.bcm_appsw_identification);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "BCM_DID_7242");
                    writer.WriteString(_ecu.bcm_ecu_calib_swno);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "BCM_DID_7244");
                    writer.WriteString(_ecu.bcm_ccid);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "BSW-Data-Source");
                    writer.WriteString(_ecu.bcm_bsw_data_source);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "APP-Data-Source");
                    writer.WriteString(_ecu.bcm_app_data_source);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "Calibration-Data-Source");
                    writer.WriteString(_ecu.bcm_calib_data_source);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "SBL-Data-Source");
                    writer.WriteString(_ecu.bcm_sbl_data_source);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "BCM_BASICSWSRC");
                    writer.WriteString(_ecu.bcm_basicswsrc);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "BCM_APPCRC");
                    writer.WriteString(_ecu.bcm_appcrc);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "BCM_CALCRC");
                    writer.WriteString(_ecu.bcm_calcrc);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "BCM_SBLCRC");
                    writer.WriteString(_ecu.bcm_sblcrc);
                    writer.WriteEndElement();             //item End

                    writer.WriteEndElement();      //property END
                    writer.WriteEndElement();      //block END
                    writer.WriteEndElement();      //blocks END
                    writer.WriteEndDocument();
                    writer.Flush();
                }
                else if (_ecu.ecu_name == "FATC")
                {
                    writer.WriteStartDocument();

                    writer.WriteStartElement("blocks"); //blocks start
                    writer.WriteAttributeString("ecu-name", _ecu.ecu_name);
                    writer.WriteAttributeString("format-version", "1.0");

                    writer.WriteStartElement("block"); //block start
                    writer.WriteAttributeString("name", "block-1");
                    writer.WriteElementString("priority", _ecu.priority);
                    writer.WriteStartElement("property"); //property start

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "FATC_DID_F195");
                    writer.WriteString(_ecu.fatc_application_sw_version);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "FATC_DID_7242");
                    writer.WriteString(_ecu.fatc_calibration_sw_version);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "FATC_DID_7244");
                    writer.WriteString(_ecu.fatc_ccid);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "APP-Data-Source");
                    writer.WriteString(_ecu.fatc_APP_Data_Source);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "Calibration-Data-Source");
                    writer.WriteString(_ecu.fatc_Calibration_Data_Source);
                    writer.WriteEndElement();             //item End          

                    writer.WriteEndElement();      //property END
                    writer.WriteEndElement();      //block END
                    writer.WriteEndElement();      //blocks END
                    writer.WriteEndDocument();
                    writer.Flush();
                }
                else if (_ecu.ecu_name == "TCM")
                {
                    writer.WriteStartDocument();

                    writer.WriteStartElement("blocks"); //blocks start
                    writer.WriteAttributeString("ecu-name", _ecu.ecu_name);
                    writer.WriteAttributeString("format-version", "1.0");

                    writer.WriteStartElement("block"); //block start
                    writer.WriteAttributeString("name", "block-1");
                    writer.WriteElementString("priority", _ecu.priority);
                    writer.WriteStartElement("property"); //property start

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "FW-Data-Source");
                    writer.WriteString(_ecu.tcm_FW_Data_Source);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "sw.version");
                    writer.WriteString(_ecu.tcm_sw_version);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "ValidationTimeout");
                    writer.WriteString(_ecu.tcm_validationtimeout);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "bb.version");
                    writer.WriteString(_ecu.tcm_bb_version);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "updateType");
                    writer.WriteString(_ecu.tcm_updatetype);
                    writer.WriteEndElement();             //item End    

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "isForceUpdate");
                    writer.WriteString(_ecu.tcm_isforceupdate);
                    writer.WriteEndElement();             //item End   

                    if (_ecu.tcm_ishufota == "false")
                    {
                        writer.WriteStartElement("item");    //item start
                        writer.WriteAttributeString("name", "remote.fota.tcm.hu.available");
                        writer.WriteString(_ecu.tcm_ishufota);
                        writer.WriteEndElement();             //item End      
                    }

                    writer.WriteEndElement();      //property END
                    writer.WriteEndElement();      //block END
                    writer.WriteEndElement();      //blocks END
                    writer.WriteEndDocument();
                    writer.Flush();
                }
                // }
            }
            return true;
        }

        public bool CreateRollbackConfigfiles(FotaTblPackageDataDto _ecu, string rootpath)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            using (XmlWriter writer = XmlWriter.Create(Path.Combine(rootpath, "Config.xml"), settings))
            {
                // foreach (var _ecu in objdata)
                // {
                if (_ecu.ecu_name == "VCU")
                {
                    writer.WriteStartDocument();

                    writer.WriteStartElement("blocks"); //blocks start
                    writer.WriteAttributeString("ecu-name", _ecu.ecu_name);
                    writer.WriteAttributeString("format-version", "1.0");

                    writer.WriteStartElement("block"); //block start
                    writer.WriteAttributeString("name", "block-1");
                    writer.WriteElementString("priority", _ecu.priority);
                    writer.WriteStartElement("property"); //property start

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "FW-Data-Source");
                    writer.WriteString(_ecu.rb_vcu_FW_Data_Source);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "BlockSize");
                    writer.WriteString(_ecu.rb_vcu_blocksize);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "NewVersion");
                    writer.WriteString(_ecu.rb_vcu_newversion);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "StMin");
                    writer.WriteString(_ecu.rb_vcu_stmin);
                    writer.WriteEndElement();             //item End

                    writer.WriteEndElement();      //property END
                    writer.WriteEndElement();      //block END
                    writer.WriteEndElement();      //blocks END
                    writer.WriteEndDocument();
                    writer.Flush();
                }
                else if (_ecu.ecu_name == "HU")
                {
                    writer.WriteStartDocument();

                    writer.WriteStartElement("blocks"); //blocks start
                    writer.WriteAttributeString("ecu-name", _ecu.ecu_name);
                    writer.WriteAttributeString("format-version", "1.0");

                    writer.WriteStartElement("block"); //block start
                    writer.WriteAttributeString("name", "block-1");
                    writer.WriteElementString("priority", _ecu.priority);
                    writer.WriteStartElement("property"); //property start

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "metafile-Source");
                    writer.WriteString(_ecu.rb_hu_metafile_Source);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "checksum");
                    writer.WriteString(_ecu.rb_hu_checksum);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "sw_version");
                    writer.WriteString(_ecu.rb_hu_sw_version);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "base-sw-version");
                    writer.WriteString(_ecu.rb_hu_base_sw_version);
                    writer.WriteEndElement();             //item End

                    writer.WriteEndElement();      //property END
                    writer.WriteEndElement();      //block END
                    writer.WriteEndElement();      //blocks END
                    writer.WriteEndDocument();
                    writer.Flush();
                }
                else if (_ecu.ecu_name == "BCM")
                {
                    writer.WriteStartDocument();

                    writer.WriteStartElement("blocks"); //blocks start
                    writer.WriteAttributeString("ecu-name", _ecu.ecu_name);
                    writer.WriteAttributeString("format-version", "1.0");

                    writer.WriteStartElement("block"); //block start
                    writer.WriteAttributeString("name", "block-1");
                    writer.WriteElementString("priority", _ecu.priority);
                    writer.WriteStartElement("property"); //property start

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "BCM_DID_F187");
                    writer.WriteString(_ecu.rb_bcm_vehicle_manspare_partno);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "BCM_DID_7241");
                    writer.WriteString(_ecu.rb_bcm_ecu_app_swno);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "BCM_DID_F181");
                    writer.WriteString(_ecu.rb_bcm_appsw_identification);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "BCM_DID_7242");
                    writer.WriteString(_ecu.rb_bcm_ecu_calib_swno);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "BCM_DID_7244");
                    writer.WriteString(_ecu.rb_bcm_ccid);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "BSW-Data-Source");
                    writer.WriteString(_ecu.rb_bcm_bsw_data_source);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "APP-Data-Source");
                    writer.WriteString(_ecu.rb_bcm_app_data_source);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "Calibration-Data-Source");
                    writer.WriteString(_ecu.rb_bcm_calib_data_source);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "SBL-Data-Source");
                    writer.WriteString(_ecu.rb_bcm_sbl_data_source);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "BCM_BASICSWSRC");
                    writer.WriteString(_ecu.rb_bcm_basicswsrc);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "BCM_APPCRC");
                    writer.WriteString(_ecu.rb_bcm_appcrc);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "BCM_CALCRC");
                    writer.WriteString(_ecu.rb_bcm_calcrc);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "BCM_SBLCRC");
                    writer.WriteString(_ecu.rb_bcm_sblcrc);
                    writer.WriteEndElement();             //item End

                    writer.WriteEndElement();      //property END
                    writer.WriteEndElement();      //block END
                    writer.WriteEndElement();      //blocks END
                    writer.WriteEndDocument();
                    writer.Flush();
                }
                else if (_ecu.ecu_name == "FATC")
                {
                    writer.WriteStartDocument();

                    writer.WriteStartElement("blocks"); //blocks start
                    writer.WriteAttributeString("ecu-name", _ecu.ecu_name);
                    writer.WriteAttributeString("format-version", "1.0");

                    writer.WriteStartElement("block"); //block start
                    writer.WriteAttributeString("name", "block-1");
                    writer.WriteElementString("priority", _ecu.priority);
                    writer.WriteStartElement("property"); //property start

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "FATC_DID_F195");
                    writer.WriteString(_ecu.rb_fatc_application_sw_version);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "FATC_DID_7242");
                    writer.WriteString(_ecu.rb_fatc_calibration_sw_version);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "FATC_DID_7244");
                    writer.WriteString(_ecu.rb_fatc_ccid);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "APP-Data-Source");
                    writer.WriteString(_ecu.rb_fatc_APP_Data_Source);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "Calibration-Data-Source");
                    writer.WriteString(_ecu.rb_fatc_Calibration_Data_Source);
                    writer.WriteEndElement();             //item End          

                    writer.WriteEndElement();      //property END
                    writer.WriteEndElement();      //block END
                    writer.WriteEndElement();      //blocks END
                    writer.WriteEndDocument();
                    writer.Flush();
                }
                else if (_ecu.ecu_name == "TCM")
                {
                    writer.WriteStartDocument();

                    writer.WriteStartElement("blocks"); //blocks start
                    writer.WriteAttributeString("ecu-name", _ecu.ecu_name);
                    writer.WriteAttributeString("format-version", "1.0");

                    writer.WriteStartElement("block"); //block start
                    writer.WriteAttributeString("name", "block-1");
                    writer.WriteElementString("priority", _ecu.priority);
                    writer.WriteStartElement("property"); //property start

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "FW-Data-Source");
                    writer.WriteString(_ecu.rb_tcm_FW_Data_Source);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "sw.version");
                    writer.WriteString(_ecu.rb_tcm_sw_version);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "ValidationTimeout");
                    writer.WriteString(_ecu.rb_tcm_validationtimeout);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "bb.version");
                    writer.WriteString(_ecu.rb_tcm_bb_version);
                    writer.WriteEndElement();             //item End

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "updateType");
                    writer.WriteString(_ecu.rb_tcm_updatetype);
                    writer.WriteEndElement();             //item End    

                    writer.WriteStartElement("item");    //item start
                    writer.WriteAttributeString("name", "isForceUpdate");
                    writer.WriteString(_ecu.rb_tcm_isforceupdate);
                    writer.WriteEndElement();             //item End   

                    if (_ecu.rb_tcm_ishufota == "false")
                    {
                        writer.WriteStartElement("item");    //item start
                        writer.WriteAttributeString("name", "remote.fota.tcm.hu.available");
                        writer.WriteString(_ecu.rb_tcm_ishufota);
                        writer.WriteEndElement();             //item End      
                    }

                    writer.WriteEndElement();      //property END
                    writer.WriteEndElement();      //block END
                    writer.WriteEndElement();      //blocks END
                    writer.WriteEndDocument();
                    writer.Flush();
                }
                // }
            }
            return true;
        }

        [HttpPost("DeleteECUDetails")]
        [EnableCors("AllowOrigin")]
        public List<FotaTblPackageDataDto> DeleteECUDetails(FotaTblPackageDataDto objdata)
        {

            string rootPath = sourcepath; // @"D:\FOTAPackage\";
            var ecufolder = Path.Combine(rootPath, objdata.package_name, objdata.ecu_name);
            if (Directory.Exists(ecufolder))
            {
                var newname = objdata.ecu_name + "_Del_" + DateTime.Now.ToString("MMddyyyyHHmm", CultureInfo.InvariantCulture);
                var newfoldername = Path.Combine(rootPath, objdata.package_name, newname);
                Directory.Move(ecufolder, newfoldername);
            }
            return _PackageRepository.DeleteECUDetails(objdata);
        }

        public void LogError(Exception ex, string rootpath)
        {
            string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            message += string.Format("Message: {0}", ex.Message);
            message += Environment.NewLine;
            message += string.Format("StackTrace: {0}", ex.StackTrace);
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            string errorpath = Path.Combine(rootpath, "ErrorLog.txt");
            using (StreamWriter writer = new StreamWriter(errorpath, true))
            {
                writer.WriteLine(message);
                writer.Close();
            }
        }
    }
}
