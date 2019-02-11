using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImportDataWebApi.Domain.Interface;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ImportDataWebApi.Api.Controllers
{
    public class ImportDataController : Controller
    {
        private readonly IImportDataServices _service;

        public ImportDataController(IImportDataServices service) 
        {
            _service = service;
        }

        [Route("api/v1/ImportData/StartService")]
        [HttpGet]
        public bool StartService()
        {
            return _service.StartService();
        }

        [Route("api/v1/ImportData/StopService")]
        [HttpGet]
        public bool StopService()
        {
            return _service.StopService();
        }
    }
}
