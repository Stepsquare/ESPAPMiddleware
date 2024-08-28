using EspapMiddleware.Shared.Enums;
using EspapMiddleware.Shared.Interfaces.IServices;
using EspapMiddleware.Shared.MonitorServiceModels;
using EspapMiddleware.Shared.WebServiceModels;
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
        private readonly IMonitorServices _monitorServices;

        public DocumentController(IMonitorServices monitorServices)
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

        [HttpPost]
        public async Task<JsonResult> ResetSigefeSync(string id)
        {
            try
            {
                await _monitorServices.ResetSigefeSync(id);

                return Json(new
                {
                    statusCode = HttpStatusCode.OK,
                    messages = new string[] { "Documento desvinculado com sucesso." }
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
        public async Task<JsonResult> ReturnDocument(string id, string reason)
        {
            try
            {
                await _monitorServices.ReturnDocument(id, reason);

                return Json(new
                {
                    statusCode = HttpStatusCode.OK,
                    messages = new string[] { "Documento devolvido com sucesso." }
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

        public async Task<JsonResult> DeleteDocument(string id)
        {
            try
            {
                await _monitorServices.DeleteDocument(id);

                return Json(new
                {
                    statusCode = HttpStatusCode.OK,
                    messages = new string[] { "Documento Apagado com sucesso." }
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
        public async Task<ActionResult> DownloadDocumentFile(int id)
        {
            var file = await _monitorServices.GetFilesForDownload(id);

            if (file == null)
                return HttpNotFound();

            return File(file.Content, file.ContentType);
        }

        [HttpGet]
        public async Task<JsonResult> GetSchoolYears()
        {
            var getFaseResponse = await _monitorServices.GetFase();

            var schoolYears = new Dictionary<string, string>
            {
                { getFaseResponse.id_ano_letivo_atual, getFaseResponse.des_id_ano_letivo_atual },
                { getFaseResponse.id_ano_letivo_anterior, getFaseResponse.des_id_ano_letivo_anterior}
            };

            return Json(schoolYears.Select(year => new ComboBoxViewModel()
            {
                Id = year.Key,
                Description = year.Value
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