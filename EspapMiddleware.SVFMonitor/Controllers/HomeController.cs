using EspapMiddleware.Shared.Enums;
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
            var anosLetivos = await _monitorServices.GetSchoolYears();

            var model = new HomepageViewModel
            {
                SchoolYears = anosLetivos.ToArray()
            };

            return View(model);
        }

        #endregion

        #region Requests

        [HttpPost]
        public async Task<PartialViewResult> GetGlobalStatus(string anoLetivo)
        {
            var statistics = await _monitorServices.GetGlobalStatus(anoLetivo);

            var model = new HomepageStatusPartialViewModel
            {
                TotalDocuments = statistics.totalDocuments,
                TotalDocumentsNotSyncFeap = statistics.totalDocumentsNotSyncFeap,
                TotalValidDocuments = statistics.totalValidDocuments,
                TotalValidDocumentsNotSyncFeap = statistics.totalValidDocumentsNotSyncFeap,
                TotalInvalidDocuments = statistics.totalInvalidDocuments,
                TotalInvalidDocumentsNotSyncFeap = statistics.totalInvalidDocumentsNotSyncFeap,
                TotalInvalidDocumentsRectified = statistics.totalInvalidDocumentsRectified,
                TotalInvalidDocumentsRectifiedNotSyncFeap = statistics.totalInvalidDocumentsRectifiedNotSyncFeap,
                TotalPaidDocuments = statistics.totalPaidDocuments,
                TotalPaidDocumentsNotSyncFeap = statistics.totalPaidDocumentsNotSyncFeap
            };

            return PartialView("_homepageStatusPartial", model);
        }

        [HttpPost]
        public async Task<JsonResult> SyncDocuments(string anoLetivo, DocumentStateEnum? stateId, DocumentActionEnum? actionId = null)
        {
            try
            {
                await _monitorServices.SyncAllDocumentsFeap(anoLetivo, stateId, actionId);

                return Json(new
                {
                    statusCode = HttpStatusCode.OK,
                    messages = new string[] { string.Format("Documentos no estado {0} sicronizados com sucesso.", stateId.ToString()) }
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