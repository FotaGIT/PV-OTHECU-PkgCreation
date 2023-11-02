using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PkgCreation.Modals;


namespace PkgCreation.Repository.Interface
{
    public interface IExceptionErrorLog
    {
        bool FnExceptionErrorLog(ExceptionlogModal objErrorLog);
    }
}
