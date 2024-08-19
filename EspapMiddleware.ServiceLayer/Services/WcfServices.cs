using EspapMiddleware.ServiceLayer.FEAPServices_PP;
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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace EspapMiddleware.ServiceLayer.Services
{
    public class WcfServices : IWcfServices
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IGenericRestRequestManager _genericRestRequestManager;

        public WcfServices(IUnitOfWorkFactory unitOfWorkFactory, IGenericRestRequestManager genericRestRequestManager)
        {
            _genericRestRequestManager = genericRestRequestManager;

            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public async Task AddSuccessRequestLog(RequestLogTypeEnum type, Guid uniqueId, string supplierFiscalId, string referenceNumber, string documentId, DateTime receivedOn)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var requestLog = new RequestLog()
                {
                    UniqueId = uniqueId,
                    RequestLogTypeId = type,
                    DocumentId = documentId,
                    SupplierFiscalId = supplierFiscalId,
                    ReferenceNumber = referenceNumber,
                    Date = receivedOn,
                    Successful = true,
                };

                if (FileManager.FileExists(type.ToString(), uniqueId.ToString()))
                {
                    requestLog.RequestLogFile = new RequestLogFile
                    {
                        Content = FileManager.GetFile(type.ToString(), uniqueId.ToString()),
                        ContentType = "application/xml"
                    };
                }

                unitOfWork.RequestLogs.Add(requestLog);

                await unitOfWork.SaveChangesAsync();
            }
        }

        public async Task AddFailedRequestLog(Exception ex, RequestLogTypeEnum type, Guid uniqueId, string supplierFiscalId, string referenceNumber, string documentId, DateTime receivedOn)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var stackTrace = new StackTrace(ex, fNeedFileInfo: true);
                var firstFrame = stackTrace.FrameCount > 0 ? stackTrace.GetFrame(0) : null;

                var requestLog = new RequestLog()
                {
                    UniqueId = uniqueId,
                    RequestLogTypeId = type,
                    DocumentId = await unitOfWork.Documents.Any(x => x.DocumentId == documentId) ? documentId : null,
                    SupplierFiscalId = supplierFiscalId,
                    ReferenceNumber = referenceNumber,
                    Date = DateTime.Now,
                    Successful = false,
                    ExceptionType = ex.GetBaseException().GetType().Name,
                    ExceptionAtFile = firstFrame?.GetFileName(),
                    ExceptionAtLine = firstFrame?.GetFileLineNumber(),
                    ExceptionMessage = ex.GetBaseException().Message
                };

                if (FileManager.FileExists(type.ToString(), uniqueId.ToString()))
                {
                    requestLog.RequestLogFile = new RequestLogFile
                    {
                        Content = FileManager.GetFile(type.ToString(), uniqueId.ToString()),
                        ContentType = "application/xml"
                    };
                }

                unitOfWork.RequestLogs.Add(requestLog);

                await unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<Document> AddDocument(SendDocumentContract contract)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                //Validar se documento já existe na BD...
                if (await unitOfWork.Documents.Any(x => x.DocumentId == contract.documentId))
                    throw new DatabaseException("Documento já inserido na BD. Ativar flag \"isUpdate\" para atualizar.");

                //Criar novo documento...
                var documentToAdd = new Document();

                //Preencher documento com dados do contrato...
                FillWithContract(in contract, ref documentToAdd);

                //Adicionar documento na BD e fazer commit transação...
                unitOfWork.Documents.Add(documentToAdd);

                var result = await unitOfWork.SaveChangesAsync();

                if (result == 0)
                    throw new DatabaseException("Erro ao adicionar Documento na BD.");

                return documentToAdd;
            }
        }

        public async Task ProcessInvoice(Document document)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                //Chamar getFase para ir buscar o ano letivo atual para update ao campo school year do documento inserido...
                document.SchoolYear = _genericRestRequestManager.Get<GetFaseResponse>("getFase").Result?.id_ano_letivo_atual;

                //Verificar resposta getFase (se falhar chamada interrompe o processo e deixa o documento por processar)...
                if (string.IsNullOrEmpty(document.SchoolYear))
                    throw new WebserviceException("Erro na chamada ao webservice getFase. Ano létivo não preenchido.");

                //Chamar setDocFaturacao
                var setDocFaturacaoResponse = await RequestSetDocFaturacao(document);

                //Caso 1 - Indisponibilidade do serviço
                if (setDocFaturacaoResponse == null || setDocFaturacaoResponse.messages == null)
                {
                    unitOfWork.DocumentMessages.Add(new DocumentMessage()
                    {
                        DocumentId = document.DocumentId,
                        MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                        Date = DateTime.Now,
                        MessageCode = "500",
                        MessageContent = "Falha de comunicação. Reenviar pedido mais tarde."
                    });

                    unitOfWork.Documents.Update(document);

                    await unitOfWork.SaveChangesAsync();

                    return;
                }

                //Caso 2 - Validação bem sucedida... atualizar documento com o objecto resultado
                if (setDocFaturacaoResponse.messages.Any(x => x.cod_msg == "200"))
                {
                    var documentResult = setDocFaturacaoResponse.faturas.FirstOrDefault();

                    //Adicionar mensagem de validação do obj fatura...
                    unitOfWork.DocumentMessages.Add(new DocumentMessage()
                    {
                        DocumentId = document.DocumentId,
                        MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                        Date = DateTime.Now,
                        MessageCode = documentResult.cod_msg_fat,
                        MessageContent = documentResult.msg_fat
                    });

                    document.IsMEGA = true;
                    document.IsProcessed = true;

                    document.MEId = documentResult.id_me_fatura;

                    if (!string.IsNullOrEmpty(documentResult.num_compromisso))
                        document.CompromiseNumber = documentResult.num_compromisso;

                    document.IsSynchronizedWithFEAP = false;

                    //Caso 2.1 - Fatura Válida (id 35)
                    if (documentResult.state_id == "35")
                    {
                        document.StateId = DocumentStateEnum.ValidadoConferido;
                        document.StateDate = DateTime.Now;
                    }

                    //Caso 2.2 - Fatura Inválida (id 22)
                    if (documentResult.state_id == "22")
                    {
                        document.ActionId = DocumentActionEnum.SolicitaçãoDocumentoRegularização;
                        document.ActionDate = DateTime.Now;
                    }

                    //Atualizar estado FE-AP com chamada ao SetDocument
                    var setDocumentRequestLog = await RequestSetDocument(document, document.ActionId == DocumentActionEnum.SolicitaçãoDocumentoRegularização ? documentResult.reason : null);
                    
                    unitOfWork.RequestLogs.Add(setDocumentRequestLog);

                    unitOfWork.Documents.Update(document);

                    await unitOfWork.SaveChangesAsync();

                    return;
                }

                //Caso 3 - Restantes respostas...
                //Adicionar message com a resposta...
                unitOfWork.DocumentMessages.Add(new DocumentMessage()
                {
                    DocumentId = document.DocumentId,
                    MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                    Date = DateTime.Now,
                    MessageCode = setDocFaturacaoResponse.messages.FirstOrDefault()?.cod_msg,
                    MessageContent = setDocFaturacaoResponse.messages.FirstOrDefault()?.msg
                });

                // 3.1 - Nif inválido (não é fornecedor MEGA,  status code 422)
                if (setDocFaturacaoResponse.messages.Any(x => x.cod_msg == "422"))
                {
                    document.IsMEGA = false;
                    document.IsProcessed = true;
                }

                unitOfWork.Documents.Update(document);

                await unitOfWork.SaveChangesAsync();
            }
        }

        public async Task ProcessCreditNote(Document document)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                //Chamar getFase para ir buscar o ano letivo atual para update ao campo school year do documento inserido...
                document.SchoolYear = _genericRestRequestManager.Get<GetFaseResponse>("getFase").Result?.id_ano_letivo_atual;

                //Verificar resposta getFase (se falhar chamada interrompe o processo e deixa o documento por processar)...
                if (string.IsNullOrEmpty(document.SchoolYear))
                    throw new WebserviceException("Erro na chamada ao webservice getFase. Ano létivo não preenchido.");

                //Chamar setDocFaturacao 
                var setDocFaturacaoResponse = await RequestSetDocFaturacao(document);

                //Caso 1 - Indisponibilidade do serviço
                if (setDocFaturacaoResponse == null || setDocFaturacaoResponse.messages == null)
                {
                    unitOfWork.DocumentMessages.Add(new DocumentMessage()
                    {
                        DocumentId = document.DocumentId,
                        MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                        Date = DateTime.Now,
                        MessageCode = "500",
                        MessageContent = "Falha de comunicação. Reenviar pedido mais tarde."
                    });

                    unitOfWork.Documents.Update(document);

                    await unitOfWork.SaveChangesAsync();

                    return;
                }

                //Caso 2 - Validação bem sucedida... atualizar documento com o objecto resultado
                if (setDocFaturacaoResponse.messages.Any(x => x.cod_msg == "200"))
                {
                    var documentResult = setDocFaturacaoResponse.faturas.FirstOrDefault();

                    //Adicionar mensagem de validação do obj fatura...
                    unitOfWork.DocumentMessages.Add(new DocumentMessage()
                    {
                        DocumentId = document.DocumentId,
                        MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                        Date = DateTime.Now,
                        MessageCode = documentResult.cod_msg_fat,
                        MessageContent = documentResult.msg_fat
                    });

                    document.IsMEGA = true;
                    document.IsProcessed = true;

                    if (!string.IsNullOrEmpty(documentResult.num_compromisso))
                        document.CompromiseNumber = documentResult.num_compromisso;

                    document.MEId = documentResult.id_me_fatura;

                    document.IsSynchronizedWithFEAP = false;

                    document.RelatedReferenceNumber = documentResult.num_doc_rel;
                    document.RelatedDocumentId = documentResult.id_doc_feap_rel;

                    //Caso 2.1 - Nota de Crédito Válida (id 35)
                    if (documentResult.state_id == "35")
                    {
                        document.StateId = DocumentStateEnum.Processado;
                        document.StateDate = DateTime.Now;

                        unitOfWork.RequestLogs.Add(await RequestSetDocument(document));

                        var relatedDocument = await unitOfWork.Documents.Find(x => x.DocumentId == document.RelatedDocumentId);

                        if (relatedDocument != null)
                        {
                            relatedDocument.StateId = DocumentStateEnum.Processado;
                            relatedDocument.StateDate = DateTime.Now;
                            relatedDocument.IsSynchronizedWithFEAP = false;

                            unitOfWork.Documents.Update(relatedDocument);

                            unitOfWork.RequestLogs.Add(await RequestSetDocument(relatedDocument));
                        }
                    }

                    //Caso 2.2 - Nota de Crédito Inválida (id 22)
                    if (documentResult.state_id == "22")
                    {
                        document.StateId = DocumentStateEnum.Devolvido;
                        document.StateDate = DateTime.Now;

                        unitOfWork.RequestLogs.Add(await RequestSetDocument(document, documentResult.reason));
                    }

                    unitOfWork.Documents.Update(document);

                    await unitOfWork.SaveChangesAsync();

                    return;
                }

                //Caso 3 - Restantes respostas...
                //Adicionar message com a resposta...
                unitOfWork.DocumentMessages.Add(new DocumentMessage()
                {
                    DocumentId = document.DocumentId,
                    MessageTypeId = DocumentMessageTypeEnum.SIGeFE,
                    Date = DateTime.Now,
                    MessageCode = setDocFaturacaoResponse.messages.FirstOrDefault()?.cod_msg,
                    MessageContent = setDocFaturacaoResponse.messages.FirstOrDefault()?.msg
                });

                // 3.1 - Nif inválido (não é fornecedor MEGA,  status code 422)
                if (setDocFaturacaoResponse.messages.Any(x => x.cod_msg == "422"))
                {
                    document.IsMEGA = false;
                    document.IsProcessed = true;
                }

                // 3.3 - Não foi encontrada fatura correspondente (status code 490)
                if (setDocFaturacaoResponse.messages.Any(x => x.cod_msg == "490"))
                {
                    document.IsMEGA = true;
                    document.IsProcessed = false;
                }

                unitOfWork.Documents.Update(document);

                await unitOfWork.SaveChangesAsync();
            }
        }

        public async Task UpdateDocument(SendDocumentContract contract)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var documentToUpdate = await unitOfWork.Documents.Find(x => x.DocumentId == contract.documentId);

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

                await unitOfWork.SaveChangesAsync();
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

                if (contract.messages != null && contract.messages.Any())
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
                else
                {
                    unitOfWork.DocumentMessages.Add(new DocumentMessage()
                    {
                        DocumentId = contract.documentId,
                        MessageTypeId = DocumentMessageTypeEnum.FEAP,
                        Date = DateTime.Now,
                        MessageCode = "MWG6667",
                        MessageContent = "Atualizado com sucesso."
                    });
                }

                unitOfWork.Documents.Update(documentToUpdate);

                await unitOfWork.SaveChangesAsync();
            }
        }

        #region Private Methods

        private async Task<SetDocFaturacaoResponse> RequestSetDocFaturacao(Document document)
        {
            var setDocFaturacaoObj = new SetDocFaturacao();

            FillWithDocument(in document, ref setDocFaturacaoObj);

            var setDocFaturacaoResult = await _genericRestRequestManager.Post<SetDocFaturacaoResponse, SetDocFaturacao>("setDocFaturacao", setDocFaturacaoObj);

            return setDocFaturacaoResult;
        }

        private async Task<RequestLog> RequestSetDocument(Document document, string reason = null)
        {
            var uniqueId = Guid.NewGuid();

            var requestLog = new RequestLog
            {
                UniqueId = uniqueId,
                RequestLogTypeId = RequestLogTypeEnum.SetDocument,
                DocumentId = document.DocumentId,
                SupplierFiscalId = document.SupplierFiscalId,
                ReferenceNumber = document.ReferenceNumber,
                Date = DateTime.Now
            };

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

                    switch (document.StateId)
                    {
                        case DocumentStateEnum.Iniciado:
                            if (document.ActionId == DocumentActionEnum.SolicitaçãoDocumentoRegularização)
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
                            break;
                        case DocumentStateEnum.ValidadoConferido:
                            setDocumentRequest.stateIdSpecified = true;
                            setDocumentRequest.stateId = (int)document.StateId;

                            setDocumentRequest.documentNumbersSpecified1Specified = true;
                            setDocumentRequest.documentNumbersSpecified1 = true;
                            setDocumentRequest.documentNumbers = new string[] { document.MEId };
                            break;
                        case DocumentStateEnum.Processado:
                            setDocumentRequest.stateIdSpecified = true;
                            setDocumentRequest.stateId = (int)document.StateId;

                            setDocumentRequest.documentNumbersSpecified1Specified = true;
                            setDocumentRequest.documentNumbersSpecified1 = true;
                            setDocumentRequest.documentNumbers = new string[] { document.MEId };

                            setDocumentRequest.commitmentSpecified1Specified = true;
                            setDocumentRequest.commitmentSpecified1 = true;
                            setDocumentRequest.commitment = !string.IsNullOrEmpty(document.CompromiseNumber) ? document.CompromiseNumber : "N/A";

                            setDocumentRequest.postingDateSpecified1Specified = true;
                            setDocumentRequest.postingDateSpecified1 = true;
                            setDocumentRequest.postingDateSpecified = true;
                            setDocumentRequest.postingDate = DateTime.Now;
                            break;
                        case DocumentStateEnum.EmitidoPagamento:
                            setDocumentRequest.stateIdSpecified = true;
                            setDocumentRequest.stateId = (int)document.StateId;

                            setDocumentRequest.paymentIssuedReference = document.MEId;
                            setDocumentRequest.paymentIssuedReferenceSpecified1 = true;
                            setDocumentRequest.paymentIssuedReferenceSpecified1Specified = true;

                            setDocumentRequest.paymentIssuedDate = document.IssueDate;
                            setDocumentRequest.paymentIssuedDateSpecified = true;
                            setDocumentRequest.paymentIssuedDateSpecified1 = true;
                            setDocumentRequest.paymentIssuedDateSpecified1Specified = true;

                            setDocumentRequest.paymentReference = document.ReferenceNumber;
                            setDocumentRequest.paymentReferenceSpecified1 = true;
                            setDocumentRequest.paymentReferenceSpecified1Specified = true;
                            break;
                        case DocumentStateEnum.Devolvido:
                            setDocumentRequest.stateIdSpecified = true;
                            setDocumentRequest.stateId = (int)document.StateId;

                            setDocumentRequest.reason = reason;
                            break;
                        default:
                            break;
                    }

                    await client.SetDocumentAsync(serviceContextHeader, setDocumentRequest);

                    requestLog.Successful = true;
                }
            }
            catch (Exception ex)
            {
                var stackTrace = new StackTrace(ex, fNeedFileInfo: true);
                var firstFrame = stackTrace.FrameCount > 0 ? stackTrace.GetFrame(0) : null;

                requestLog.Successful = false;
                requestLog.ExceptionType = ex.GetBaseException().GetType().Name;
                requestLog.ExceptionAtFile = firstFrame?.GetFileName();
                requestLog.ExceptionAtLine = firstFrame?.GetFileLineNumber();
                requestLog.ExceptionMessage = ex.GetBaseException().Message;
            }

            if (FileManager.FileExists(requestLog.RequestLogTypeId.ToString(), uniqueId.ToString()))
            {
                requestLog.RequestLogFile = new RequestLogFile
                {
                    Content = FileManager.GetFile(requestLog.RequestLogTypeId.ToString(), uniqueId.ToString()),
                    ContentType = "application/xml"
                };
            }

            return requestLog;
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

            obj.UblFile = new DocumentFile
            {
                Content = Convert.FromBase64String(contract.ublFormat),
                ContentType = "application/xml",
            };

            obj.PdfFile = new DocumentFile
            {
                Content = Convert.FromBase64String(contract.pdfFormat),
                ContentType = "application/pdf",
            };

            if (!string.IsNullOrEmpty(contract.attachs))
                obj.AttachsFile = new DocumentFile
                {
                    Content = Convert.FromBase64String(contract.attachs),
                    ContentType = "application/zip",
                };

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

        private void FillWithDocument(in Document document, ref SetDocFaturacao obj)
        {
            obj.id_ano_letivo = document.SchoolYear;

            obj.nif = document.SupplierFiscalId.Substring(2);

            var faturaToSend = new SetDocFaturacao.fatura()
            {
                id_doc_feap = document.DocumentId,
                id_me_fatura = document.MEId,
                num_fatura = document.ReferenceNumber,
                total_fatura = document.TotalAmount,
                fatura_base64 = Convert.ToBase64String(document.PdfFile.Content),
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
