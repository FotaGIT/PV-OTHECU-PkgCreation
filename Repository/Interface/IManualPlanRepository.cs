using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PkgCreation.Modals;

namespace PkgCreation.Repository.Interface
{
    public interface IManualPlanRepository
    {
        Appsettings Getappsettingdata(string Environment);
        Task<SingleStringDto> CreatePlanManually(ManualData manual, string FromMailId);

    }
}
