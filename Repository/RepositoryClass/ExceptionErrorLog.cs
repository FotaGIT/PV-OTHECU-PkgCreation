using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PkgCreation.Modals;
using PkgCreation.Repository.Interface;
using Microsoft.EntityFrameworkCore;



namespace PkgCreation.Repository.RepositoryClass
{
    public class ExceptionErrorLog:IExceptionErrorLog
    {
        private readonly CustomContext _customContext = new CustomContext();
        public ExceptionErrorLog()
        {
        }
        public bool FnExceptionErrorLog(ExceptionlogModal objErrorLog)
        {
            try
            {
                var result = _customContext.Set<QueryResult>().FromSqlRaw("call Usp_OAddException({0},{1},{2},{3})", objErrorLog.Exception, objErrorLog.ExceptionDetails, objErrorLog.StackTrace, "PkgAPI").ToList();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
           
        }
    }
}
