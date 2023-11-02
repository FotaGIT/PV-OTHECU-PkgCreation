using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Configuration;
using PkgCreation.Modals;
using PkgCreation.Repository.Interface;
using System.Text.Json;

namespace PkgCreation.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/FileUpload")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private IConfiguration _configuration;
        private readonly IPacakgeRepository _PackageRepository;
        private IExceptionErrorLog _exceptionErrorLog;
        private string rootpath = "";
        public FileUploadController(IConfiguration objconfig, IPacakgeRepository objpackagerepo, IExceptionErrorLog objexceptionErrorLog)
        {
            _configuration = objconfig;
            _PackageRepository = objpackagerepo;
            _exceptionErrorLog = objexceptionErrorLog;
            rootpath = _configuration.GetValue<string>("MySettings:SourcePath");
        }
        //public static string rootpath = Path.Combine(@"D:\Tushar_share\", "FOTAPackage");

        [EnableCors("AllowOrigin")]
        [HttpPost("Uploadfiles")]
        [RequestFormLimits(MultipartBodyLengthLimit = 2147483648)]
        public async Task<IActionResult> Uploadfiles()  //string packagename, string ecuname
        {
            List<FotaTblPackageDataDto> objPkg = JsonSerializer.Deserialize<List<FotaTblPackageDataDto>>(Request.Form["pkgdata"]);
            
            if (Createfolderstructure(objPkg[0].package_name, objPkg[0].ecu_name))
            {
                try
                {
                    await Task.Run(() =>
                    {
                        string subfolderpath = "";
                        var files = Request.Form.Files;
                        foreach (var file in files)
                        {
                            var postedfile = file;
                            var filename = file.FileName;
                            var filetype = file.Name;
                            var ext = Path.GetExtension(filename);

                            if (filetype == "vcuseqfile" || filetype == "huseqfile" || filetype == "fatcseqfile" || filetype == "bcmseqfile" || filetype == "tcmseqfile")
                                subfolderpath = Path.Combine("Install");
                            else if (filetype == "vcudatfile" || filetype == "humetafile" || filetype == "huchecksumfile" || filetype == "fatcappdatfile" || filetype == "fatccaldatfile" || filetype == "bcmbswfile" || filetype == "bcmappfile" || filetype == "bcmcalibrationfile" || filetype == "bcmsblfile" || filetype == "bcmbasicswsrcfile" || filetype == "bcmsblcrcfile" || filetype == "bcmappcrcfile" || filetype == "bcmcalcrcfile" || filetype == "tcmfwfile")
                                subfolderpath = Path.Combine("Install");
                            else if (filetype == "rbvcuseqfile" || filetype == "rbhuseqfile" || filetype == "rbfatcseqfile" || filetype == "rbbcmseqfile" || filetype == "rbtcmseqfile")
                                subfolderpath = Path.Combine("Rollback");
                            else if (filetype == "rbvcudatfile" || filetype == "rbhumetafile" || filetype == "rbhuchecksumfile" || filetype == "rbfatcappdatfile" || filetype == "rbfatccaldatfile" || filetype == "rbbcmbswfile" || filetype == "rbbcmappfile" || filetype == "rbbcmcalibrationfile" || filetype == "rbbcmsblfile" || filetype == "rbbcmbasicswsrcfile" || filetype == "rbbcmsblcrcfile" || filetype == "rbbcmappcrcfile" || filetype == "rbbcmcalcrcfile" || filetype == "rbtcmfwfile")
                                subfolderpath = Path.Combine("Rollback");

                            var docSavePath = Path.Combine(rootpath, objPkg[0].package_name, objPkg[0].ecu_name, subfolderpath);

                            if (!Directory.Exists(docSavePath))
                            {
                                Directory.CreateDirectory(docSavePath);
                            }

                            string fullPath = Path.Combine(docSavePath, filename);
                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                postedfile.CopyTo(stream);
                            }
                        }

                    });

                    await _PackageRepository.SaveECUDetails(objPkg);

                    return Ok(new { message = "Files Upload Successfully" });
                }
                catch (Exception ex)
                {
                    //LogError(ex, rootpath);

                    ExceptionlogModal logger = new ExceptionlogModal();
                    logger.Exception = ex.Message;
                    logger.ExceptionDetails = "CreatePackage";
                    logger.StackTrace = ex.StackTrace;
                    logger.CreatedBy = ex.Source;
                    _exceptionErrorLog.FnExceptionErrorLog(logger);

                    return BadRequest(new { message = "Files Upload Failed" });
                }
            }
            else
            {
                return BadRequest(new { message = "Files Upload Failed" });
            }

            //if (!Directory.Exists(rootpath))
            //{
            //    Directory.CreateDirectory(rootpath);
            //}
            //var files = Request.Form.Files;
            //foreach (IFormFile postedFile in files)
            //{
            //    string fileName = Path.GetFileName(postedFile.FileName);
            //    using (FileStream stream = new FileStream(Path.Combine(rootpath, fileName), FileMode.Create))
            //    {
            //        postedFile.CopyTo(stream);
            //    }
            //}
            //return Ok(new { message = "Files Upload Success" });
        }

        public bool Createfolderstructure(string packagename, string ecu_name)
        {
            var packagefolder = Path.Combine(rootpath, packagename);
            bool returnvalue = false;

            try
            {
                if (!Directory.Exists(packagefolder))
                {
                    Directory.CreateDirectory(Path.Combine(packagefolder, ecu_name, "Install"));

                    Directory.CreateDirectory(Path.Combine(packagefolder, ecu_name, "Rollback"));
                }
                else
                {
                    if (!Directory.Exists(Path.Combine(packagefolder, ecu_name)))
                    {
                        Directory.CreateDirectory(Path.Combine(packagefolder, ecu_name));
                    }

                    if (!Directory.Exists(Path.Combine(packagefolder, ecu_name, "Install")))
                        Directory.CreateDirectory(Path.Combine(packagefolder, ecu_name, "Install"));

                    if (!Directory.Exists(Path.Combine(packagefolder, ecu_name, "Rollback")))
                        Directory.CreateDirectory(Path.Combine(packagefolder, ecu_name, "Rollback"));

                }
                returnvalue = true;
            }
            catch (Exception ex)
            {
                //LogError(exception, rootpath);

                ExceptionlogModal logger = new ExceptionlogModal();
                logger.Exception = ex.Message;
                logger.ExceptionDetails = "Createfolderstructure";
                logger.StackTrace = ex.StackTrace;
                logger.CreatedBy = ex.Source;
                _exceptionErrorLog.FnExceptionErrorLog(logger);

                returnvalue = false;
            }

            return returnvalue;
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
