using EspapMiddleware.ServiceLayer.Helpers.OutboundMessageInspector;
using EspapMiddleware.Shared.Entities;
using EspapMiddleware.Shared.Enums;
using EspapMiddleware.Shared.Exceptions;
using EspapMiddleware.Shared.Interfaces.IConfiguration;
using EspapMiddleware.Shared.Interfaces.IHelpers;
using EspapMiddleware.Shared.Interfaces.IServices;
using EspapMiddleware.Shared.WebServiceModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.ServiceLayer.Services
{
    public class SyncronizationServices : ISyncronizationServices
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IGenericRestRequestManager _genericRestRequestManager;

        public SyncronizationServices(IUnitOfWorkFactory unitOfWorkFactory, IGenericRestRequestManager genericRestRequestManager)
        {
            _genericRestRequestManager = genericRestRequestManager;

            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public async Task SigefeBulkSyncronization()
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var docsToSync = await unitOfWork.Documents.GetFiltered(x => !x.IsSynchronizedWithSigefe && x.IsSynchronizedWithFEAP);

                foreach (var doc in docsToSync)
                {
                    if (string.IsNullOrEmpty(doc.SchoolYear))
                    {
                        var getFaseResponse = await _genericRestRequestManager.Get<GetFaseResponse>("getFase");
                        doc.SchoolYear = getFaseResponse?.id_ano_letivo_atual;
                    }

                    var documentLines = await unitOfWork.DocumentLines.GetFiltered(x => x.DocumentId == doc.DocumentId);

                    var webserviceResult = await SyncDocumentoWithSigefe(doc, documentLines);

                    if (webserviceResult == null)
                    {
                        unitOfWork.DocumentMessages.Add(new DocumentMessage()
                        {
                            DocumentId = doc.DocumentId,
                            MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                            Date = DateTime.Now,
                            MessageCode = "500",
                            MessageContent = "Falha de comunicação. Reenviar pedido mais tarde."
                        });
                    }
                    else if (webserviceResult.messages.Any(x => x.cod_msg == "422"))
                    {
                        unitOfWork.Documents.Delete(doc);
                    }
                    else if (webserviceResult.messages.Any(x => x.cod_msg == "200"))
                    {
                        var docToSyncResult = webserviceResult.faturas.FirstOrDefault();

                        unitOfWork.DocumentMessages.Add(new DocumentMessage()
                        {
                            DocumentId = doc.DocumentId,
                            MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                            Date = DateTime.Now,
                            MessageCode = docToSyncResult.cod_msg_fat,
                            MessageContent = docToSyncResult.msg_fat
                        });

                        if (docToSyncResult.cod_msg_fat != "490")
                        {
                            doc.MEId = docToSyncResult.id_me_fatura;
                            doc.IsSynchronizedWithSigefe = !string.IsNullOrEmpty(docToSyncResult.id_me_fatura);
                            doc.IsSynchronizedWithFEAP = !doc.IsSynchronizedWithSigefe;

                            if (docToSyncResult.state_id == "35")
                            {
                                if (doc.TypeId != DocumentTypeEnum.Fatura)
                                {
                                    doc.StateId = DocumentStateEnum.Processado;
                                    doc.StateDate = DateTime.Now;
                                }
                                else
                                {
                                    doc.StateId = DocumentStateEnum.ValidadoConferido;
                                    doc.StateDate = DateTime.Now;
                                }
                            }
                            else if (docToSyncResult.state_id == "22")
                            {
                                doc.ActionId = DocumentActionEnum.SolicitaçãoDocumentoRegularização;
                                doc.ActionDate = DateTime.Now;
                            }

                            unitOfWork.Documents.Update(doc);
                        }
                    }
                    else
                    {
                        foreach (var message in webserviceResult.messages)
                        {
                            unitOfWork.DocumentMessages.Add(new DocumentMessage()
                            {
                                DocumentId = doc.DocumentId,
                                MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                                Date = DateTime.Now,
                                MessageCode = message.cod_msg,
                                MessageContent = message.msg
                            });
                        }
                    }

                    await unitOfWork.SaveChangesAsync();
                }
            }
        }

        public async Task FeapBulkSyncronization()
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var docsToSync = await unitOfWork.Documents.GetFiltered(x => !x.IsSynchronizedWithFEAP && x.IsSynchronizedWithSigefe);

                foreach (var doc in docsToSync)
                {
                    if (doc.TypeId == DocumentTypeEnum.Fatura 
                        && await unitOfWork.Documents.Any(x => x.RelatedDocumentId == doc.DocumentId 
                                                            && x.TypeId == DocumentTypeEnum.NotaCrédito 
                                                            && x.StateId == DocumentStateEnum.Processado))
                    {
                        doc.StateId = DocumentStateEnum.Processado;
                        doc.StateDate = DateTime.Now;

                        unitOfWork.Documents.Update(doc);
                    }

                    var lastSigefeMessage = await unitOfWork.DocumentMessages.GetLastSigefeMessage(doc.DocumentId);

                    unitOfWork.RequestLogs.Add(await SyncDocumentoWithFeap(doc, lastSigefeMessage?.MessageContent));

                    await unitOfWork.SaveChangesAsync();
                }
            }
        }

        #region Private sync methods

        private async Task<SetDocFaturacaoResponse> SyncDocumentoWithSigefe(Document docToSync, IEnumerable<DocumentLine> docLinesToSync)
        {
            var setDocFaturacaoObj = new SetDocFaturacao();

            #region setDocFaturacaoObj Contruction

            setDocFaturacaoObj.id_ano_letivo = docToSync.SchoolYear;

            setDocFaturacaoObj.nif = docToSync.SupplierFiscalId.Substring(2);

            var faturaToSend = new SetDocFaturacao.fatura()
            {
                id_doc_feap = docToSync.DocumentId,
                id_me_fatura = docToSync.MEId,
                num_fatura = docToSync.ReferenceNumber,
                total_fatura = docToSync.TotalAmount,
                fatura_base64 = Convert.ToBase64String(docToSync.PdfFormat),
                dt_fatura = docToSync.IssueDate.ToString("dd-MM-yyyy"),
                num_compromisso = docToSync.CompromiseNumber,
            };

            switch (docToSync.TypeId)
            {
                case DocumentTypeEnum.Fatura:
                    faturaToSend.tp_doc = "FAT";
                    break;
                case DocumentTypeEnum.NotaCrédito:
                    faturaToSend.tp_doc = "NTC";
                    faturaToSend.num_doc_rel = docToSync.RelatedReferenceNumber;
                    break;
                case DocumentTypeEnum.NotaDébito:
                    faturaToSend.tp_doc = "NTD";
                    faturaToSend.num_doc_rel = docToSync.RelatedReferenceNumber;
                    break;
                default:
                    break;
            }

            if (docToSync.StateId == DocumentStateEnum.EmitidoPagamento)
            {
                faturaToSend.estado_doc = "5";
                faturaToSend.dt_estado = docToSync.StateDate.ToString("dd-MM-yyyy");
            }

            faturaToSend.linhas = new List<SetDocFaturacao.fatura.linhaModel>();

            foreach (var line in docLinesToSync)
            {
                faturaToSend.linhas.Add(new SetDocFaturacao.fatura.linhaModel()
                {
                    id_linha = line.StandardItemIdentification,
                    num_linha = line.LineId,
                    descricao_linha = line.Description,
                    qtd_linha = line.Quantity,
                    valor_linha = line.Value,
                    perc_iva_linha = line.TaxPercentage,
                });
            }

            setDocFaturacaoObj.faturas = new List<SetDocFaturacao.fatura>
            {
                faturaToSend
            };

            #endregion

            var setDocFaturacaoResult = await _genericRestRequestManager.Post<SetDocFaturacaoResponse, SetDocFaturacao>("setDocFaturacao", setDocFaturacaoObj);

            return setDocFaturacaoResult;
        }

        private async Task<RequestLog> SyncDocumentoWithFeap(Document docToSync, string reason = null)
        {
            var uniqueId = Guid.NewGuid();

            try
            {
                using (var client = new FEAPServices_PP.FEAPServicesClient())
                {
                    client.Endpoint.Behaviors.Add(new EndpointBehavior(uniqueId.ToString()));

                    var feapCredencials = (NameValueCollection)ConfigurationManager.GetSection("FEAPCredencials");

                    client.ClientCredentials.UserName.UserName = feapCredencials["username"];
                    client.ClientCredentials.UserName.Password = feapCredencials["password"];

                    var serviceContextHeader = new FEAPServices_PP.ServiceContextHeader()
                    {
                        Application = "FaturacaoEletronica",
                        ProcessId = docToSync.DocumentId,
                    };

                    var setDocumentRequest = new FEAPServices_PP.SetDocumentRequest()
                    {
                        uniqueId = uniqueId.ToString(),
                        documentId = docToSync.DocumentId,
                    };

                    if (docToSync.StateId == DocumentStateEnum.ValidadoConferido || docToSync.StateId == DocumentStateEnum.Processado)
                    {
                        setDocumentRequest.documentNumbersSpecified1Specified = true;
                        setDocumentRequest.documentNumbersSpecified1 = true;
                        setDocumentRequest.documentNumbers = new string[] { docToSync.MEId };
                        setDocumentRequest.stateIdSpecified = true;
                        setDocumentRequest.stateId = (int)docToSync.StateId;

                        if (docToSync.StateId == DocumentStateEnum.Processado)
                        {
                            setDocumentRequest.commitmentSpecified1Specified = true;
                            setDocumentRequest.commitmentSpecified1 = true;
                            setDocumentRequest.commitment = string.IsNullOrEmpty(docToSync.CompromiseNumber) ? docToSync.CompromiseNumber : "N/A";

                            setDocumentRequest.postingDateSpecified1Specified = true;
                            setDocumentRequest.postingDateSpecified1 = true;
                            setDocumentRequest.postingDateSpecified = true;
                            setDocumentRequest.postingDate = DateTime.Now;
                        }
                    }
                    else if (docToSync.ActionId == DocumentActionEnum.SolicitaçãoDocumentoRegularização)
                    {
                        setDocumentRequest.actionIdSpecified = true;
                        setDocumentRequest.actionId = (int)docToSync.ActionId;
                        setDocumentRequest.regularizationSpecified = true;

                        switch (docToSync.TypeId)
                        {
                            case DocumentTypeEnum.Fatura:
                                setDocumentRequest.regularization = 2;
                                break;
                            case DocumentTypeEnum.NotaCrédito:
                                setDocumentRequest.regularization = 3;
                                break;
                            case DocumentTypeEnum.NotaDébito:
                                setDocumentRequest.regularization = 2;
                                break;
                            default:
                                break;
                        }

                        setDocumentRequest.reason = reason;
                    }

                    await client.SetDocumentAsync(serviceContextHeader, setDocumentRequest);

                    return new RequestLog()
                    {
                        UniqueId = uniqueId,
                        RequestLogTypeId = RequestLogTypeEnum.SetDocument,
                        DocumentId = docToSync.DocumentId,
                        SupplierFiscalId = docToSync.SupplierFiscalId,
                        ReferenceNumber = docToSync.ReferenceNumber,
                        Date = DateTime.Now,
                        Successful = true
                    };
                }
            }
            catch (Exception ex)
            {
                return new RequestLog()
                {
                    UniqueId = uniqueId,
                    RequestLogTypeId = RequestLogTypeEnum.SetDocument,
                    DocumentId = docToSync.DocumentId,
                    SupplierFiscalId = docToSync.SupplierFiscalId,
                    ReferenceNumber = docToSync.ReferenceNumber,
                    Date = DateTime.Now,
                    Successful = false,
                    ExceptionType = ex.GetBaseException().GetType().Name,
                    ExceptionStackTrace = ex.GetBaseException().StackTrace,
                    ExceptionMessage = ex.GetBaseException().Message
                };
            }
        }

        #endregion
    }
}
