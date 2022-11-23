using EspapMiddleware.Shared.Enums;
using EspapMiddleware.Shared.Interfaces.IServices;
using EspapMiddleware.Shared.MonitorServiceModels;
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
    public class DocumentController : Controller
    {
        private readonly IMonitorService _monitorServices;

        public DocumentController(IMonitorService monitorServices)
        {
            _monitorServices = monitorServices;
        }

        #region Views

        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> Detail(string id)
        {
            var model = await _monitorServices.GetDocumentDetail(id);

            if (model == null)
                return HttpNotFound();

            return View(model);
        }

        #endregion


        #region Requests
        [HttpPost]
        public async Task<PartialViewResult> Search(DocumentSearchFilters filters)
        {
            var model = await _monitorServices.DocumentSearch(filters);

            return PartialView("_searchResultPartial", model);
        }

        [HttpPost]
        public async Task<PartialViewResult> GetLines(DocumentDetailLineFilter filters)
        {
            var model = await _monitorServices.GetDocumentLinesForDetail(filters);

            return PartialView("_linesResultPartial", model);
        }

        [HttpPost]
        public async Task<JsonResult> SyncFeap(string id)
        {
            try
            {
                await _monitorServices.SyncFeap(id);

                return Json(new
                {
                    statusCode = HttpStatusCode.OK,
                    messages = new string[] { "Documento sicronizado com sucesso." }
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
        public async Task<JsonResult> SyncSigefe(string id)
        {
            try
            {
                await _monitorServices.SyncSigefe(id);

                return Json(new
                {
                    statusCode = HttpStatusCode.OK,
                    messages = new string[] { "Documento sicronizado com sucesso." }
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

        [HttpGet]
        public async Task<ActionResult> DownloadUbl(string id)
        {
            var doc = await _monitorServices.GetDocumentDetail(id);

            if (doc == null)
                return HttpNotFound();

            byte[] fileBytes = doc.UblFormat;

            return File(fileBytes, "application/xml", doc.DocumentId + ".xml");
        }

        [HttpGet]
        public async Task<ActionResult> DownloadPdf(string id)
        {
            var doc = await _monitorServices.GetDocumentDetail(id);

            if (doc == null)
                return HttpNotFound();

            byte[] fileBytes = doc.PdfFormat;

            return File(fileBytes, "application/pdf", doc.DocumentId + ".pdf");
        }

        [HttpGet]
        public async Task<JsonResult> GetSchoolYears()
        {
            var schoolYears = await _monitorServices.GetSchoolYears();

            return Json(schoolYears.Select(e => new ComboBoxViewModel()
            {
                Id = e,
                Description = e
            }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetTypes()
        {
            var types = Enum.GetValues(typeof(DocumentTypeEnum)).Cast<DocumentTypeEnum>()
                    .Select(e => new ComboBoxViewModel()
                    {
                        Id = ((int)e).ToString(),
                        Description = e.ToString()
                    });

            return Json(types, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetStates()
        {
            var states = Enum.GetValues(typeof(DocumentStateEnum)).Cast<DocumentStateEnum>()
                    .Select(e => new ComboBoxViewModel()
                    {
                        Id = ((int)e).ToString(),
                        Description = e.ToString()
                    });

            return Json(states, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}