using EspapMiddleware.Shared.Interfaces.IServices;
using EspapMiddleware.Shared.MonitorServiceModels;
using EspapMiddleware.Shared.MonitorServiceModels.PaginationModels;
using EspapMiddleware.SVFMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EspapMiddleware.SVFMonitor.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMonitorService _monitorServices;

        public HomeController(IMonitorService monitorServices)
        {
            _monitorServices = monitorServices;
        }

        #region Views

        public async Task<ActionResult> Index()
        {
            var statistics = await _monitorServices.GetGlobalStatus();

            var model = new HomepageViewModel
            {
                TotalDocuments = statistics.totalDocuments,
                TotalValidDocuments = statistics.totalValidDocuments,
                TotalInvalidDocuments = statistics.totalInvalidDocuments,
                TotalInvalidDocumentsRectified = statistics.totalInvalidDocumentsRectified,
                TotalPaidDocuments = statistics.totalPaidDocuments
            };

            return View(model);
        }

        #endregion

        #region Requests

        [HttpPost]
        public async Task<PartialViewResult> GetPaidDocuments(PaginatedSearchFilter filters)
        {
            var model = await _monitorServices.GetPaidDocsToSync(filters);

            return PartialView("_paidDocumentsResultPartial", model);
        }

        [HttpPost]
        public async Task<JsonResult> SyncPaidDocuments(string documentId)
        {
            try
            {
                await _monitorServices.SyncPaidDocuments(documentId);

                return Json(new
                {
                    statusCode = HttpStatusCode.OK,
                    messages = new string[] { "Documentos pagos sicronizado com sucesso." }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    statusCode = HttpStatusCode.InternalServerError,
                    messages = new string[] { ex.GetBaseException().Message }
                });
            }
        }

        #endregion
    }
}