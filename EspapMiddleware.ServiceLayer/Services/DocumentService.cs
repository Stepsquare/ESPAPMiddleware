using EspapMiddleware.ServiceLayer.Helpers;
using EspapMiddleware.ServiceLayer.Helpers.OutboundMessageInspector;
using EspapMiddleware.Shared.ConfigModels;
using EspapMiddleware.Shared.DataContracts;
using EspapMiddleware.Shared.Entities;
using EspapMiddleware.Shared.Enums;
using EspapMiddleware.Shared.Exceptions;
using EspapMiddleware.Shared.Interfaces.IConfiguration;
using EspapMiddleware.Shared.Interfaces.IHelpers;
using EspapMiddleware.Shared.Interfaces.IServices;
using EspapMiddleware.Shared.WebServiceModels;
using EspapMiddleware.Shared.XmlSerializerModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace EspapMiddleware.ServiceLayer.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IGenericRestRequestManager _genericRestRequestManager;

        public DocumentService(IUnitOfWorkFactory unitOfWorkFactory, IGenericRestRequestManager genericRestRequestManager)
        {
            _genericRestRequestManager = genericRestRequestManager;

            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public async Task AddFailedRequestLog(RequestLogTypeEnum type, Exception ex, Guid uniqueId, string supplierFiscalId, string referenceNumber, string documentId)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                unitOfWork.RequestLogs.Add(new RequestLog()
                {
                    UniqueId = uniqueId,
                    RequestLogTypeId = type,
                    DocumentId = await unitOfWork.Documents.Any(x => x.DocumentId == documentId) ? documentId : null,
                    SupplierFiscalId = supplierFiscalId,
                    ReferenceNumber = referenceNumber,
                    Date = DateTime.Now,
                    Successful = false,
                    ExceptionType = ex.GetBaseException().GetType().Name,
                    ExceptionStackTrace = ex.GetBaseException().StackTrace,
                    ExceptionMessage = ex.GetBaseException().Message
                });

                var result = await unitOfWork.SaveChangesAsync();

                if (result == 0)
                {
                    throw new DatabaseException("Erro ao gravar Log na BD.");
                }
            }
        }

        public async Task AddDocument(SendDocumentContract contract)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var documentToInsert = await unitOfWork.Documents.Find(x => x.DocumentId == contract.documentId);

                if (documentToInsert != null)
                    throw new DatabaseException("Documento já inserido na BD. Ativar flag \"isUpdate\" para atualizar.");

                contract.SchoolYear = _genericRestRequestManager.Get<GetFaseResponse>("getFase").Result?.id_ano_letivo_atual;

                var SendDocumentRequestLog = new RequestLog()
                {
                    UniqueId = contract.uniqueId,
                    RequestLogTypeId = RequestLogTypeEnum.SendDocument,
                    DocumentId = contract.documentId,
                    SupplierFiscalId = contract.supplierFiscalId,
                    ReferenceNumber = contract.referenceNumber,
                    Successful = true,
                    Date = DateTime.Now
                };

                documentToInsert = new Document();

                FillWithContract(in contract, ref documentToInsert);

                if (contract.documentType != DocumentTypeEnum.Fatura)
                {
                    var relatedDocument = await unitOfWork.Documents.GetRelatedDocument(contract.RelatedReferenceNumber,
                                                                                        contract.supplierFiscalId,
                                                                                        contract.SchoolYear,
                                                                                        contract.documentType);

                    if (relatedDocument != null)
                        documentToInsert.RelatedDocumentId = relatedDocument.DocumentId;

                    var setDocFaturacaoResult = await RequestSetDocFaturacao(documentToInsert);

                    //VAlIDAÇÃO DE NIF FORNECEDOR
                    if (setDocFaturacaoResult != null && setDocFaturacaoResult.messages.Any(x => x.cod_msg == "422"))
                        throw new WebserviceException(setDocFaturacaoResult.messages.Select(x => x.msg).ToArray());

                    //VALIDAÇÃO FALHA DE COMUNICAÇÃO DO WEBSERVICE OU ERRO DE CONCORRÊNCIA
                    if (setDocFaturacaoResult == null || setDocFaturacaoResult.messages.Any(x => x.cod_msg == "429"))
                    {
                        unitOfWork.DocumentMessages.Add(new DocumentMessage()
                        {
                            DocumentId = contract.documentId,
                            MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                            Date = DateTime.Now,
                            MessageCode = setDocFaturacaoResult != null ? setDocFaturacaoResult.messages.FirstOrDefault(x => x.cod_msg == "429")?.cod_msg : "500",
                            MessageContent = setDocFaturacaoResult != null ? setDocFaturacaoResult.messages.FirstOrDefault(x => x.cod_msg == "429")?.msg : "Falha de comunicação. Reenviar pedido mais tarde."
                        });
                    }
                    else
                    {
                        var documentToInsertResult = setDocFaturacaoResult.faturas.FirstOrDefault();

                        unitOfWork.DocumentMessages.Add(new DocumentMessage()
                        {
                            DocumentId = contract.documentId,
                            MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                            Date = DateTime.Now,
                            MessageCode = documentToInsertResult.cod_msg_fat,
                            MessageContent = documentToInsertResult.msg_fat
                        });

                        if (documentToInsertResult.cod_msg_fat != "490")
                        {
                            documentToInsert.IsSynchronizedWithSigefe = !string.IsNullOrEmpty(documentToInsertResult.id_me_fatura);
                            documentToInsert.IsSynchronizedWithFEAP = !documentToInsert.IsSynchronizedWithSigefe;

                            documentToInsert.MEId = documentToInsertResult.id_me_fatura;

                            if (documentToInsertResult.state_id == "35")
                            {
                                documentToInsert.StateId = DocumentStateEnum.Processado;
                                documentToInsert.StateDate = DateTime.Now;
                                unitOfWork.RequestLogs.Add(await RequestSetDocument(documentToInsert));

                                if (relatedDocument != null)
                                {
                                    relatedDocument.StateId = DocumentStateEnum.Processado;
                                    relatedDocument.StateDate = DateTime.Now;
                                    unitOfWork.RequestLogs.Add(await RequestSetDocument(relatedDocument));

                                    unitOfWork.Documents.Update(relatedDocument);
                                }
                            }
                            else if (documentToInsertResult.state_id == "22")
                            {
                                documentToInsert.ActionId = DocumentActionEnum.SolicitaçãoDocumentoRegularização;
                                documentToInsert.ActionDate = DateTime.Now;

                                unitOfWork.RequestLogs.Add(await RequestSetDocument(documentToInsert, documentToInsertResult.reason));
                            }
                        }
                    }
                }
                else
                {
                    var setDocFaturacaoResult = await RequestSetDocFaturacao(documentToInsert);

                    //VAlIDAÇÃO DE NIF FORNECEDOR
                    if (setDocFaturacaoResult != null && setDocFaturacaoResult.messages.Any(x => x.cod_msg == "422"))
                        throw new WebserviceException(setDocFaturacaoResult.messages.Select(x => x.msg).ToArray());

                    //VALIDAÇÃO FALHA DE COMUNICAÇÃO DO WEBSERVICE OU ERRO DE CONCORRÊNCIA
                    if (setDocFaturacaoResult == null || setDocFaturacaoResult.messages.Any(x => x.cod_msg == "429"))
                    {
                        unitOfWork.DocumentMessages.Add(new DocumentMessage()
                        {
                            DocumentId = contract.documentId,
                            MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                            Date = DateTime.Now,
                            MessageCode = setDocFaturacaoResult != null ? setDocFaturacaoResult.messages.FirstOrDefault(x => x.cod_msg == "429")?.cod_msg : "500",
                            MessageContent = setDocFaturacaoResult != null ? setDocFaturacaoResult.messages.FirstOrDefault(x => x.cod_msg == "429")?.msg : "Falha de comunicação. Reenviar pedido mais tarde."
                        });
                    }
                    else
                    {
                        var documentToInsertResult = setDocFaturacaoResult.faturas.FirstOrDefault();

                        unitOfWork.DocumentMessages.Add(new DocumentMessage()
                        {
                            DocumentId = contract.documentId,
                            MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                            Date = DateTime.Now,
                            MessageCode = documentToInsertResult.cod_msg_fat,
                            MessageContent = documentToInsertResult.msg_fat
                        });

                        documentToInsert.MEId = documentToInsertResult.id_me_fatura;

                        documentToInsert.IsSynchronizedWithSigefe = !string.IsNullOrEmpty(documentToInsertResult.id_me_fatura);
                        documentToInsert.IsSynchronizedWithFEAP = !documentToInsert.IsSynchronizedWithSigefe;

                        if (documentToInsertResult.state_id == "35")
                        {
                            documentToInsert.StateId = DocumentStateEnum.ValidadoConferido;
                            documentToInsert.StateDate = DateTime.Now;

                            unitOfWork.RequestLogs.Add(await RequestSetDocument(documentToInsert));
                        }
                        else
                        {
                            var relatedDocument = await unitOfWork.Documents.GetRelatedDocument(contract.referenceNumber,
                                                                                        contract.supplierFiscalId,
                                                                                        contract.SchoolYear,
                                                                                        contract.documentType);

                            if (relatedDocument != null)
                            {
                                relatedDocument.RelatedDocumentId = documentToInsert.DocumentId;

                                var relatedSetDocFaturacaoResult = await RequestSetDocFaturacao(relatedDocument);

                                if (relatedSetDocFaturacaoResult != null && relatedSetDocFaturacaoResult.messages.Any(x => x.cod_msg == "200"))
                                {
                                    var relatedDocumentResult = relatedSetDocFaturacaoResult.faturas.FirstOrDefault();

                                    unitOfWork.DocumentMessages.Add(new DocumentMessage()
                                    {
                                        DocumentId = relatedDocument.DocumentId,
                                        MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                                        Date = DateTime.Now,
                                        MessageCode = relatedDocumentResult.cod_msg_fat,
                                        MessageContent = relatedDocumentResult.msg_fat
                                    });

                                    relatedDocument.MEId = relatedDocumentResult.id_me_fatura;

                                    relatedDocument.IsSynchronizedWithSigefe = !string.IsNullOrEmpty(relatedDocumentResult.id_me_fatura);
                                    relatedDocument.IsSynchronizedWithFEAP = !relatedDocument.IsSynchronizedWithSigefe;

                                    if (relatedDocumentResult.state_id == "35")
                                    {
                                        documentToInsert.StateId = DocumentStateEnum.Processado;
                                        documentToInsert.StateDate = DateTime.Now;
                                        unitOfWork.RequestLogs.Add(await RequestSetDocument(documentToInsert));

                                        relatedDocument.StateId = DocumentStateEnum.Processado;
                                        relatedDocument.StateDate = DateTime.Now;
                                        unitOfWork.RequestLogs.Add(await RequestSetDocument(relatedDocument));
                                    }
                                    else
                                    {
                                        relatedDocument.ActionId = DocumentActionEnum.SolicitaçãoDocumentoRegularização;
                                        relatedDocument.ActionDate = DateTime.Now;

                                        unitOfWork.RequestLogs.Add(await RequestSetDocument(relatedDocument, relatedDocumentResult.reason));
                                    }

                                    unitOfWork.Documents.Update(relatedDocument);
                                }
                                else
                                {
                                    unitOfWork.DocumentMessages.Add(new DocumentMessage()
                                    {
                                        DocumentId = relatedDocument.DocumentId,
                                        MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                                        Date = DateTime.Now,
                                        MessageCode = relatedSetDocFaturacaoResult != null ? relatedSetDocFaturacaoResult.messages.FirstOrDefault().cod_msg : "500",
                                        MessageContent = relatedSetDocFaturacaoResult != null ? relatedSetDocFaturacaoResult.messages.FirstOrDefault().msg : "Falha de comunicação. Reenviar pedido mais tarde."
                                    });
                                }
                            }
                            else
                            {
                                documentToInsert.ActionId = DocumentActionEnum.SolicitaçãoDocumentoRegularização;
                                documentToInsert.ActionDate = DateTime.Now;

                                unitOfWork.RequestLogs.Add(await RequestSetDocument(documentToInsert, documentToInsertResult.reason));
                            }
                        }
                    }
                }

                unitOfWork.Documents.Add(documentToInsert);

                unitOfWork.RequestLogs.Add(SendDocumentRequestLog);

                var result = await unitOfWork.SaveChangesAsync();

                if (result == 0)
                {
                    throw new DatabaseException("Erro ao adicionar Documento na BD.");
                }
            }
        }

        public async Task UpdateDocument(SendDocumentContract contract)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var documentToUpdate = await unitOfWork.Documents.GetDocumentForSyncSigefe(contract.documentId);

                if (documentToUpdate == null)
                    throw new DatabaseException("Documento não encontrado na BD.");

                documentToUpdate.IsSynchronizedWithFEAP = true;

                if (contract.stateId.HasValue)
                {
                    documentToUpdate.StateId = contract.stateId.Value;
                    documentToUpdate.StateDate = contract.stateDate.Value;
                }
                else if (contract.actionId.HasValue)
                {
                    documentToUpdate.ActionId = contract.actionId;
                    documentToUpdate.ActionDate = contract.actionDate;
                }

                unitOfWork.Documents.Update(documentToUpdate);

                unitOfWork.RequestLogs.Add(new RequestLog()
                {
                    UniqueId = contract.uniqueId,
                    RequestLogTypeId = RequestLogTypeEnum.SendDocument,
                    DocumentId = documentToUpdate.DocumentId,
                    SupplierFiscalId = documentToUpdate.SupplierFiscalId,
                    ReferenceNumber = documentToUpdate.ReferenceNumber,
                    Successful = true,
                    Date = DateTime.Now
                });

                var result = await unitOfWork.SaveChangesAsync();

                if (result == 0)
                {
                    throw new DatabaseException("Erro ao atualizar Documento na BD.");
                }
            }
        }

        public async Task SyncDocument(SetDocumentResultMCIn contract)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var documentToUpdate = await unitOfWork.Documents.Find(x => x.DocumentId == contract.documentId);

                if (documentToUpdate == null)
                    throw new DatabaseException("Documento não encontrado na BD.");

                documentToUpdate.IsSynchronizedWithFEAP = contract.isASuccess;

                if (contract.messages?.Length > 0)
                {
                    foreach (var message in contract.messages)
                    {
                        unitOfWork.DocumentMessages.Add(new DocumentMessage()
                        {
                            DocumentId = contract.documentId,
                            MessageTypeId = DocumentMessageTypeEnum.FEAP,
                            Date = DateTime.Now,
                            MessageCode = message.code,
                            MessageContent = message.description
                        });
                    }
                }
                else if (contract.isASuccess)
                {
                    unitOfWork.DocumentMessages.Add(new DocumentMessage()
                    {
                        DocumentId = contract.documentId,
                        MessageTypeId = DocumentMessageTypeEnum.FEAP,
                        Date = DateTime.Now,
                        MessageContent = "Documento atualizado com sucesso."
                    });
                }

                unitOfWork.Documents.Update(documentToUpdate);

                unitOfWork.RequestLogs.Add(new RequestLog()
                {
                    UniqueId = contract.uniqueId,
                    RequestLogTypeId = RequestLogTypeEnum.SetDocumentResult,
                    DocumentId = documentToUpdate.DocumentId,
                    SupplierFiscalId = documentToUpdate.SupplierFiscalId,
                    ReferenceNumber = documentToUpdate.ReferenceNumber,
                    Successful = true,
                    Date = DateTime.Now
                });

                var result = await unitOfWork.SaveChangesAsync();

                if (result == 0)
                {
                    throw new DatabaseException("Erro ao atualizar Documento na BD.");
                }
            }
        }

        #region Private Methods

        private async Task<SetDocFaturacaoResponse> RequestSetDocFaturacao(Document document)
        {
            var setDocFaturacaoObj = new SetDocFaturacao();

            FillWithContract(in document, ref setDocFaturacaoObj);

            var setDocFaturacaoResult = await _genericRestRequestManager.Post<SetDocFaturacaoResponse, SetDocFaturacao>("setDocFaturacao", setDocFaturacaoObj);

            return setDocFaturacaoResult;
        }

        private async Task<RequestLog> RequestSetDocument(Document document, string reason = null)
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
                        ProcessId = document.DocumentId,
                    };

                    var setDocumentRequest = new FEAPServices_PP.SetDocumentRequest()
                    {
                        uniqueId = uniqueId.ToString(),
                        documentId = document.DocumentId,
                    };

                    if (document.StateId == DocumentStateEnum.ValidadoConferido || document.StateId == DocumentStateEnum.Processado)
                    {
                        setDocumentRequest.documentNumbersSpecified1Specified = true;
                        setDocumentRequest.documentNumbersSpecified1 = true;
                        setDocumentRequest.documentNumbers = new string[] { document.MEId };
                        setDocumentRequest.stateIdSpecified = true;
                        setDocumentRequest.stateId = (int)document.StateId;

                        if (document.StateId == DocumentStateEnum.Processado)
                        {
                            setDocumentRequest.commitmentSpecified1Specified = true;
                            setDocumentRequest.commitmentSpecified1 = true;
                            setDocumentRequest.commitment = !string.IsNullOrEmpty(document.CompromiseNumber) ? document.CompromiseNumber : "N/A";

                            setDocumentRequest.postingDateSpecified1Specified = true;
                            setDocumentRequest.postingDateSpecified1 = true;
                            setDocumentRequest.postingDateSpecified = true;
                            setDocumentRequest.postingDate = DateTime.Now;
                        }
                    }
                    else if (document.ActionId == DocumentActionEnum.SolicitaçãoDocumentoRegularização)
                    {
                        setDocumentRequest.actionIdSpecified = true;
                        setDocumentRequest.actionId = (int)document.ActionId;
                        setDocumentRequest.regularizationSpecified = true;

                        switch (document.TypeId)
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
                        DocumentId = document.DocumentId,
                        SupplierFiscalId = document.SupplierFiscalId,
                        ReferenceNumber = document.ReferenceNumber,
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
                    DocumentId = document.DocumentId,
                    SupplierFiscalId = document.SupplierFiscalId,
                    ReferenceNumber = document.ReferenceNumber,
                    Date = DateTime.Now,
                    Successful = false,
                    ExceptionType = ex.GetBaseException().GetType().Name,
                    ExceptionStackTrace = ex.GetBaseException().StackTrace,
                    ExceptionMessage = ex.GetBaseException().Message
                };
            }
        }

        private static void FillWithContract(in SendDocumentContract contract, ref Document obj)
        {
            obj.DocumentId = contract.documentId;
            obj.ReferenceNumber = contract.referenceNumber;
            obj.RelatedReferenceNumber = contract.RelatedReferenceNumber;
            obj.TypeId = contract.documentType;
            obj.IssueDate = contract.issueDate;
            obj.SupplierFiscalId = contract.supplierFiscalId;
            obj.CustomerFiscalId = contract.customerFiscalId;
            obj.InternalManagement = contract.internalManagement;

            obj.TotalAmount = contract.TotalAmount;
            obj.CompromiseNumber = contract.CompromiseNumber;
            obj.SchoolYear = contract.SchoolYear;

            obj.UblFormat = Convert.FromBase64String(contract.ublFormat);
            obj.PdfFormat = Convert.FromBase64String(contract.pdfFormat);
            if (!string.IsNullOrEmpty(contract.attachs))
                obj.Attachs = Convert.FromBase64String(contract.attachs);

            obj.StateId = contract.stateId ?? contract.stateId.Value;
            obj.StateDate = contract.stateDate ?? contract.stateDate.Value;

            obj.IsSynchronizedWithFEAP = true;

            obj.DocumentLines = new List<DocumentLine>();

            int currentIteration = 0;

            foreach (var line in contract.InvoiceLines)
            {
                currentIteration++;

                bool success = int.TryParse(line.Id, out int lineId);

                obj.DocumentLines.Add(new DocumentLine()
                {
                    LineId = success ? lineId : currentIteration,
                    DocumentId = contract.documentId,
                    Description = line.Name,
                    StandardItemIdentification = line.StandardItemIdentification,
                    Quantity = int.Parse(line.Quantity),
                    Value = decimal.Parse(line.TotalLineValue, CultureInfo.InvariantCulture),
                    TaxPercentage = decimal.Parse(line.TaxPercent, CultureInfo.InvariantCulture)
                });
            }
        }

        private void FillWithContract(in Document document, ref SetDocFaturacao obj)
        {
            obj.id_ano_letivo = document.SchoolYear;

            obj.nif = document.SupplierFiscalId.Substring(2);

            var faturaToSend = new SetDocFaturacao.fatura()
            {
                id_doc_feap = document.DocumentId,
                id_me_fatura = document.MEId,
                num_fatura = document.ReferenceNumber,
                total_fatura = document.TotalAmount,
                fatura_base64 = Convert.ToBase64String(document.PdfFormat),
                dt_fatura = document.IssueDate.ToString("dd-MM-yyyy"),
                num_compromisso = document.CompromiseNumber,
            };

            switch (document.TypeId)
            {
                case DocumentTypeEnum.Fatura:
                    faturaToSend.tp_doc = "FAT";
                    break;
                case DocumentTypeEnum.NotaCrédito:
                    faturaToSend.tp_doc = "NTC";
                    faturaToSend.num_doc_rel = document.RelatedReferenceNumber;
                    break;
                case DocumentTypeEnum.NotaDébito:
                    faturaToSend.tp_doc = "NTD";
                    faturaToSend.num_doc_rel = document.RelatedReferenceNumber;
                    break;
                default:
                    break;
            }

            if (document.StateId == DocumentStateEnum.EmitidoPagamento)
            {
                faturaToSend.estado_doc = "5";
                faturaToSend.dt_estado = document.StateDate.ToString("dd-MM-yyyy");
            }

            faturaToSend.linhas = new List<SetDocFaturacao.fatura.linhaModel>();

            foreach (var line in document.DocumentLines)
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

            obj.faturas = new List<SetDocFaturacao.fatura>
            {
                faturaToSend
            };
        }

        #endregion
    }
}
