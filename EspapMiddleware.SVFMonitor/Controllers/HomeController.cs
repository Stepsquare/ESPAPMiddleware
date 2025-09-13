using EspapMiddleware.Shared.Enums;
using EspapMiddleware.Shared.Interfaces.IServices;
using EspapMiddleware.Shared.MonitorServiceModels;
using EspapMiddleware.Shared.MonitorServiceModels.PaginationModels;
using EspapMiddleware.SVFMonitor.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EspapMiddleware.SVFMonitor.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMonitorServices _monitorServices;
        private readonly string _anoLetivoAtual;

        public HomeController(IMonitorServices monitorServices)
        {
            _monitorServices = monitorServices;
            _anoLetivoAtual = ConfigurationManager.AppSettings["AnoLetivoAtual"];
        }

        #region Views

        public ActionResult Index()
        {
            return View();
        }

        public async Task<PartialViewResult> GetGlobalStatus()
        {
            var model = new HomepageStatusPartialViewModel
            {
                Total = await _monitorServices.GetTotalDocument(),
                TotalUnprocessed = await _monitorServices.GetTotalUnprocessedDocument(),
                TotalMEGA = await _monitorServices.GetTotalMEGADocument(),
                TotalNotMEGA = await _monitorServices.GetTotalNotMEGADocument(),
                TotalNotSyncFeap = await _monitorServices.GetTotalDocumentsToSyncFeap(),
                InvoiceStatus = new HomepageStatusPartialViewModel.InvoiceStatusObject
                {
                    Total = await _monitorServices.GetTotalMEGADocumentsByType(new[] { DocumentTypeEnum.Fatura, DocumentTypeEnum.FaturaSimplificada, DocumentTypeEnum.FaturaRecibo, DocumentTypeEnum.FaturaAdiantamento }),
                    Validated = await _monitorServices.GetTotalMEGADocumentsByType(new[] { DocumentTypeEnum.Fatura, DocumentTypeEnum.FaturaSimplificada, DocumentTypeEnum.FaturaRecibo, DocumentTypeEnum.FaturaAdiantamento }, DocumentStateEnum.ValidadoConferido),
                    ValidatedToSync = await _monitorServices.GetTotalPaidDocsToSync(),
                    Paid = await _monitorServices.GetTotalMEGADocumentsByType(new[] { DocumentTypeEnum.Fatura, DocumentTypeEnum.FaturaSimplificada, DocumentTypeEnum.FaturaRecibo, DocumentTypeEnum.FaturaAdiantamento }, DocumentStateEnum.EmitidoPagamento),
                    PendingRegularization = await _monitorServices.GetTotalMEGADocumentsByType(new[] { DocumentTypeEnum.Fatura, DocumentTypeEnum.FaturaSimplificada, DocumentTypeEnum.FaturaRecibo, DocumentTypeEnum.FaturaAdiantamento }, DocumentStateEnum.Iniciado, DocumentActionEnum.SolicitaçãoDocumentoRegularização),
                    Regularized = await _monitorServices.GetTotalMEGADocumentsByType(new[] { DocumentTypeEnum.Fatura, DocumentTypeEnum.FaturaSimplificada, DocumentTypeEnum.FaturaRecibo, DocumentTypeEnum.FaturaAdiantamento }, DocumentStateEnum.Processado),
                    Returned = await _monitorServices.GetTotalMEGADocumentsByType(new[] { DocumentTypeEnum.Fatura, DocumentTypeEnum.FaturaSimplificada, DocumentTypeEnum.FaturaRecibo, DocumentTypeEnum.FaturaAdiantamento }, DocumentStateEnum.Devolvido)
                },
                CreditNoteStatus = new HomepageStatusPartialViewModel.CreditNoteStatusObject
                {
                    Total = await _monitorServices.GetTotalMEGADocumentsByType(new[] { DocumentTypeEnum.NotaCrédito }),
                    Unprocessed = await _monitorServices.GetTotalCreditNotesToReprocess(),
                    Processed = await _monitorServices.GetTotalMEGADocumentsByType(new[] { DocumentTypeEnum.NotaCrédito }, DocumentStateEnum.Processado),
                    Returned = await _monitorServices.GetTotalMEGADocumentsByType(new[] { DocumentTypeEnum.NotaCrédito }, DocumentStateEnum.Devolvido)
                }
            };

            return PartialView("_homepageStatusPartial", model);
        }

        #endregion

        #region Requests

        [HttpPost]
        public async Task<JsonResult> SyncAllDocumentsFeap()
        {
            try
            {
                await _monitorServices.SyncAllDocumentsFeap();

                return Json(new
                {
                    statusCode = HttpStatusCode.OK,
                    messages = new string[] { "Documentos sincronizados com sucesso." }
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
        public async Task<JsonResult> SyncPaidDocuments()
        {
            try
            {
                await _monitorServices.SyncPaidDocuments();

                return Json(new
                {
                    statusCode = HttpStatusCode.OK,
                    messages = new string[] { "Documentos pagos sincronizados com sucesso." }
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
        public async Task<PartialViewResult> GetDocsToSyncSigefe(PaginatedSearchFilter filters)
        {
            var model = await _monitorServices.GetDocsToSyncSigefe(filters);

            return PartialView("_docsToSyncSigefeResultPartial", model);
        }

        [HttpPost]
        public async Task<JsonResult> SyncDocumentsSigefe(string documentId)
        {
            try
            {
                await _monitorServices.SyncDocumentsSigefe(documentId);

                return Json(new
                {
                    statusCode = HttpStatusCode.OK,
                    messages = new string[] { "Documentos sincronizados com sucesso." }
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
        public async Task<JsonResult> ReprocessCreditNotes()
        {
            try
            {
                await _monitorServices.ReprocessCreditNotes();

                return Json(new
                {
                    statusCode = HttpStatusCode.OK,
                    messages = new string[] { "Notas de crédito reprocessadas com sucesso." }
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