﻿using EspapMiddleware.Shared.Enums;
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
                SchoolYears = anosLetivos.ToArray(),
                CurrentSchoolYear = await _monitorServices.GetCurrentSchoolYear()
            };

            return View(model);
        }

        #endregion

        #region Requests

        [HttpPost]
        public async Task<PartialViewResult> GetGlobalStatus(string anoLetivo)
        {
            var model = new HomepageStatusPartialViewModel
            {
                Total = await _monitorServices.GetTotalDocument(anoLetivo),
                TotalNotSyncFeap = await _monitorServices.GetTotalDocument(anoLetivo, false),
                IsCurrentSchoolYear = await _monitorServices.GetCurrentSchoolYear() == anoLetivo,
                InvoiceStatus = new HomepageStatusPartialViewModel.InvoiceStatusObject
                {
                    Total = await _monitorServices.GetTotalDocumentsByType(anoLetivo, DocumentTypeEnum.Fatura),
                    Validated = await _monitorServices.GetTotalDocumentsByType(anoLetivo, DocumentTypeEnum.Fatura, DocumentStateEnum.ValidadoConferido),
                    ValidatedToSync = await _monitorServices.GetTotalPaidDocsToSync(),
                    Paid = await _monitorServices.GetTotalDocumentsByType(anoLetivo, DocumentTypeEnum.Fatura, DocumentStateEnum.EmitidoPagamento),
                    PendingRegularization = await _monitorServices.GetTotalDocumentsByType(anoLetivo, DocumentTypeEnum.Fatura, DocumentStateEnum.Iniciado, DocumentActionEnum.SolicitaçãoDocumentoRegularização),
                    Regularized = await _monitorServices.GetTotalDocumentsByType(anoLetivo, DocumentTypeEnum.Fatura, DocumentStateEnum.Processado)
                },
                CreditNoteStatus = new HomepageStatusPartialViewModel.CreditNoteStatusObject
                {
                    Total = await _monitorServices.GetTotalDocumentsByType(anoLetivo, DocumentTypeEnum.NotaCrédito),
                    Unprocessed = await _monitorServices.GetTotalCreditNotesToReprocess(),
                    Processed = await _monitorServices.GetTotalDocumentsByType(anoLetivo, DocumentTypeEnum.NotaCrédito, DocumentStateEnum.Processado),
                    Returned = await _monitorServices.GetTotalDocumentsByType(anoLetivo, DocumentTypeEnum.NotaCrédito, DocumentStateEnum.Devolvido)
                },
                DebitNoteStatus = new HomepageStatusPartialViewModel.DebitNoteStatusObject
                {
                    Total = await _monitorServices.GetTotalDocumentsByType(anoLetivo, DocumentTypeEnum.NotaDébito),
                    Unprocessed = await _monitorServices.GetTotalDocumentsByType(anoLetivo, DocumentTypeEnum.NotaDébito, DocumentStateEnum.Iniciado),
                    Returned = await _monitorServices.GetTotalDocumentsByType(anoLetivo, DocumentTypeEnum.NotaDébito, DocumentStateEnum.Devolvido)
                }
            };

            return PartialView("_homepageStatusPartial", model);
        }

        [HttpPost]
        public async Task<JsonResult> SyncAllDocumentsFeap(string anoLetivo)
        {
            try
            {
                await _monitorServices.SyncAllDocumentsFeap(anoLetivo);

                return Json(new
                {
                    statusCode = HttpStatusCode.OK,
                    messages = new string[] { "Documentos sicronizados com sucesso." }
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
                    messages = new string[] { "Documentos sicronizados com sucesso." }
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

        [HttpPost]
        public async Task<JsonResult> ReturnDebitNotes()
        {
            try
            {
                await _monitorServices.ReturnDebitNotes();

                return Json(new
                {
                    statusCode = HttpStatusCode.OK,
                    messages = new string[] { "Notas de débito devolvidas com sucesso." }
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