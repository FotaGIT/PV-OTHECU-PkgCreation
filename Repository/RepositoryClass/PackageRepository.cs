using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PkgCreation.Repository.Interface;
using PkgCreation.Modals;
using System.Text.Json;

namespace PkgCreation.Repository.RepositoryClass
{
    public class PackageRepository : IPacakgeRepository
    {
        private readonly CustomContext _customContext = new CustomContext();
        public async Task<bool> UpdatePkgCreated(string pkgname, string approvedby, string filesize, string ispkgcreated)
        {
            try
            {
                await Task.Run(() =>
                {
                    _customContext.Set<FotaTblPackageDataDto>().FromSqlRaw("Call OthECU_USP_UpdatePkgCreated({0},{1},{2},{3})", pkgname, approvedby, filesize, ispkgcreated).ToList();
                });
                return true;
            }
            catch(Exception ex)
            {
                ExceptionlogModal logger = new ExceptionlogModal();
                logger.Exception = ex.Message;
                logger.ExceptionDetails = "PackageRepository/UpdatePkgCreated";
                logger.StackTrace = ex.StackTrace;
                logger.CreatedBy = ex.Source;

                ExceptionErrorLog objlog = new ExceptionErrorLog();
                objlog.FnExceptionErrorLog(logger);

                return false;
            }
        }

        public async Task<List<FotaTblPackageDataDto>> SaveECUDetails(List<FotaTblPackageDataDto> objdata)
        {
            try
            {
                var objecus = JsonSerializer.Serialize(objdata);
                var result = new List<FotaTblPackageDataDto>();
                await Task.Run(() =>
                {
                    result = _customContext.Set<FotaTblPackageDataDto>().FromSqlRaw("Call OthECU_USP_SaveECUDetails({0})", objecus).ToList();
                });

                return result;
            }
            catch(Exception ex)
            {
                ExceptionlogModal logger = new ExceptionlogModal();
                logger.Exception = ex.Message;
                logger.ExceptionDetails = "PackageRepository/SaveECUDetails";
                logger.StackTrace = ex.StackTrace;
                logger.CreatedBy = ex.Source;

                ExceptionErrorLog objlog = new ExceptionErrorLog();
                objlog.FnExceptionErrorLog(logger);

                return null;
            }
        }

        public List<FotaTblPackageDataDto> DeleteECUDetails(FotaTblPackageDataDto objdata)
        {
            return _customContext.Set<FotaTblPackageDataDto>().FromSqlRaw("Call OthECU_USP_DeleteECUDetails({0},{1})", objdata.packageid, objdata.deletedby).ToList();
        }
    }
}
