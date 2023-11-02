using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PkgCreation.Modals;

namespace PkgCreation.Repository.Interface
{
    public interface IPacakgeRepository
    {
        Task<bool> UpdatePkgCreated(string pkgname, string approvedby, string filesize,string ispkgcreated);
        Task<List<FotaTblPackageDataDto>> SaveECUDetails(List<FotaTblPackageDataDto> objdata);
        List<FotaTblPackageDataDto> DeleteECUDetails(FotaTblPackageDataDto objdata);
    }
}
