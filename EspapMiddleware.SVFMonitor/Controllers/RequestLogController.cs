using EspapMiddleware.Shared.Enums;
using EspapMiddleware.Shared.Interfaces.IServices;
using EspapMiddleware.Shared.MonitorServiceModels;
using EspapMiddleware.SVFMonitor.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EspapMiddleware.SVFMonitor.Controllers
{
    public class RequestLogController : Controller
    {
        private readonly IMonitorServices _monitorServices;

        public RequestLogController(IMonitorServices monitorServices)
        {
            _monitorServices = monitorServices;
        }

        #region Views

        public ActionResult Index()
        {
            return View();
        }

        #endregion


        #region Requests
        [HttpPost]
        public async Task<PartialViewResult> Search(RequestLogSearchFilters filters)
        {
            var model = await _monitorServices.RequestLogSearch(filters);

            return PartialView("_searchResultPartial", model);
        }

        [HttpGet]
        public async Task<ActionResult> DownloadRequestLogFile(int id)
        {
            var file = await _monitorServices.GetRequestLogFile(id);

            if (file == null)
                return HttpNotFound();

            return File(file.Content, file.ContentType);
        }

        [HttpGet]
        public JsonResult GetTypes()
        {
            var types = Enum.GetValues(typeof(RequestLogTypeEnum)).Cast<RequestLogTypeEnum>()
                    .Select(e => new ComboBoxViewModel()
                    {
                        Id = ((int)e).ToString(),
                        Description = e.ToString()
                    });

            return Json(types, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}