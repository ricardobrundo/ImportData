using System;
using System.Collections.Generic;
using System.Text;

namespace ImportDataWebApi.Domain.Interface
{
    public interface IImportDataServices
    {
        bool StartService();
        bool StopService();
    }
}
