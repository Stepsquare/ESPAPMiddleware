using EspapMiddleware.ServiceLayer.Helpers;
using EspapMiddleware.ServiceLayer.Helpers.OutboundMessageInspector;
using EspapMiddleware.Shared.ConfigModels;
using EspapMiddleware.Shared.DataContracts;
using EspapMiddleware.Shared.Entities;
using EspapMiddleware.Shared.Enums;
using EspapMiddleware.Shared.Exceptions;
using EspapMiddleware.Shared.Interfaces.IConfiguration;
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
        private readonly GenericRestRequestManager _webserviceRequest;

        public DocumentService(IUnitOfWorkFactory unitOfWorkFactory)
        {
            var environment = (EnvironmentConfig)ConfigurationManager.GetSection("FaturacaoWebServicesConfig/environment");
            var webservices = (NameValueCollection)ConfigurationManager.GetSection("FaturacaoWebServicesConfig/webServices");

            _webserviceRequest = new GenericRestRequestManager(environment, webservices);
            
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public async Task<GetDocFaturacaoResponse> GetDocFaturacao(string nif = null, string id_doc_feap = null)
        {
            var headers = new Dictionary<string, string>()
            {
                { "id_ano_letivo", _webserviceRequest.Get<GetFaseResponse>("getFase").Result?.id_ano_letivo_atual }
            };

            if (!string.IsNullOrEmpty(nif))
                headers.Add("nif", nif);

            if (!string.IsNullOrEmpty(id_doc_feap))
                headers.Add("id_doc_feap", id_doc_feap);

            return await _webserviceRequest.Get<GetDocFaturacaoResponse>("getDocFaturacao", headers);
        }

        public async Task AddFailedRequestLog(RequestLogTypeEnum type, Exception ex, Guid uniqueId, string documentId = null)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                unitOfWork.RequestLogs.Add(new RequestLog()
                {
                    UniqueId = uniqueId,
                    RequestLogTypeId = type,
                    DocumentId = await unitOfWork.Documents.Any(x => x.DocumentId == documentId) ? documentId : null,
                    Date = DateTime.UtcNow,
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

                contract.SchoolYear = _webserviceRequest.Get<GetFaseResponse>("getFase").Result?.id_ano_letivo_atual;

                documentToInsert = new Document();

                FillWithContract(in contract, ref documentToInsert);

                if (contract.documentType != DocumentTypeEnum.Fatura)
                {
                    var relatedDocument = await unitOfWork.Documents.Find(x => x.ReferenceNumber == contract.RelatedReferenceNumber
                                                                            && x.SupplierFiscalId == contract.supplierFiscalId
                                                                            && x.SchoolYear == contract.SchoolYear
                                                                            && x.TypeId != contract.documentType);

                    if (relatedDocument == null)
                        throw new DatabaseException("Documento relacionado não encontrado na BD. Inserir documento referente à fatura original.");
                    
                    documentToInsert.RelatedDocumentId = relatedDocument.DocumentId;
                }

                var faturaResult = await RequestSetDocFaturacao(contract);

                documentToInsert.MEId = faturaResult.id_me_fatura;

                switch (faturaResult.state_id)
                {
                    case "35":
                        documentToInsert.StateId = DocumentStateEnum.ValidadoConferido;
                        documentToInsert.StateDate = DateTime.UtcNow;
                        break;
                    case "22":
                        documentToInsert.StateId = DocumentStateEnum.Devolvido;
                        documentToInsert.StateDate = DateTime.UtcNow;
                        break;
                    default:
                        break;
                }

                if (documentToInsert.StateId == DocumentStateEnum.ValidadoConferido 
                    || documentToInsert.StateId == DocumentStateEnum.Devolvido)
                {
                    var setDocumentLog = await RequestSetDocument(faturaResult);

                    unitOfWork.RequestLogs.Add(setDocumentLog);
                }

                unitOfWork.Documents.Add(documentToInsert);

                unitOfWork.RequestLogs.Add(new RequestLog()
                {
                    UniqueId = contract.uniqueId,
                    RequestLogTypeId = RequestLogTypeEnum.SendDocument,
                    DocumentId = contract.documentId,
                    Successful = true,
                    Date = DateTime.UtcNow
                });

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
                var documentToUpdate = await unitOfWork.Documents.GetByIdIncludeRelatedDoc(contract.documentId);

                if (documentToUpdate == null)
                    throw new DatabaseException("Documento não encontrado na BD.");

                if (contract.stateId.HasValue)
                {
                    documentToUpdate.StateId = contract.stateId.Value;
                    documentToUpdate.StateDate = contract.stateDate.Value;

                    if (contract.stateId == DocumentStateEnum.EmitidoPagamento)
                    {
                        var faturaResult = await RequestSetDocFaturacao(documentToUpdate);
                    }
                }
                else
                {
                    if (contract.actionId.HasValue)
                    {
                        documentToUpdate.ActionId = contract.actionId;
                        documentToUpdate.ActionDate = contract.actionDate;
                    }
                }

                unitOfWork.Documents.Update(documentToUpdate);

                unitOfWork.RequestLogs.Add(new RequestLog()
                {
                    UniqueId = contract.uniqueId,
                    RequestLogTypeId = RequestLogTypeEnum.SendDocument,
                    DocumentId = documentToUpdate.DocumentId,
                    Successful = true,
                    Date = DateTime.UtcNow
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

                if (contract.messages.Length > 0)
                    documentToUpdate.FEAPMessages = string.Join(Environment.NewLine, contract.messages.Select(x => x.code.ToString() + " - " + x.description).ToArray());

                unitOfWork.Documents.Update(documentToUpdate);

                unitOfWork.RequestLogs.Add(new RequestLog()
                {
                    UniqueId = contract.uniqueId,
                    RequestLogTypeId = RequestLogTypeEnum.SetDocumentResult,
                    DocumentId = documentToUpdate.DocumentId,
                    Successful = true,
                    Date = DateTime.UtcNow
                });

                var result = await unitOfWork.SaveChangesAsync();

                if (result == 0)
                {
                    throw new DatabaseException("Erro ao atualizar Documento na BD.");
                }
            }
        }

        #region Private Methods

        private async Task<SetDocFaturacaoResponse.fatura> RequestSetDocFaturacao(SendDocumentContract contract)
        {
            var setDocFaturacaoObj = new SetDocFaturacao();

            FillWithContract(in contract, ref setDocFaturacaoObj);

            var setDocFaturacaoResult = await _webserviceRequest.Post<SetDocFaturacaoResponse, SetDocFaturacao>("setDocFaturacao", setDocFaturacaoObj);

            if (setDocFaturacaoResult == null)
                throw new WebserviceException("Erro de comunicação com os webservices.");

            if (setDocFaturacaoResult.messages.Any(x => x.cod_msg == "200"))
            {
                return setDocFaturacaoResult.faturas.FirstOrDefault();
            }
            else
            {
                throw new WebserviceException(setDocFaturacaoResult.messages.Select(x => x.msg).ToArray());
            }
        }

        private async Task<SetDocFaturacaoResponse.fatura> RequestSetDocFaturacao(Document document)
        {
            var setDocFaturacaoObj = new SetDocFaturacao();

            FillWithContract(in document, ref setDocFaturacaoObj);

            var setDocFaturacaoResult = await _webserviceRequest.Post<SetDocFaturacaoResponse, SetDocFaturacao>("setDocFaturacao", setDocFaturacaoObj);

            if (setDocFaturacaoResult == null)
                throw new WebserviceException("Erro de comunicação com os webservices.");

            if (setDocFaturacaoResult.messages.Any(x => x.cod_msg == "200"))
            {
                return setDocFaturacaoResult.faturas.FirstOrDefault();
            }
            else
            {
                throw new WebserviceException(setDocFaturacaoResult.messages.Select(x => x.msg).ToArray());
            }
        }

        private async Task<RequestLog> RequestSetDocument(SetDocFaturacaoResponse.fatura response)
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
                        ProcessId = response.id_doc_feap,
                    };

                    var setDocumentRequest = new FEAPServices_PP.SetDocumentRequest()
                    {
                        uniqueId = uniqueId.ToString(),
                        documentId = response.id_doc_feap,
                        stateIdSpecified = true,
                        stateId = int.Parse(response.state_id),
                        reason = response.state_id == ((int)DocumentStateEnum.Devolvido).ToString() ? response.reason : null,
                        documentNumbersSpecified1 = !string.IsNullOrEmpty(response.id_me_fatura),
                        documentNumbers = new string[] { response.id_me_fatura }
                    };

                    await client.SetDocumentAsync(serviceContextHeader, setDocumentRequest);

                    return new RequestLog()
                    {
                        UniqueId = uniqueId,
                        RequestLogTypeId = RequestLogTypeEnum.SetDocument,
                        DocumentId = response.id_doc_feap,
                        Date = DateTime.UtcNow,
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
                    DocumentId = response.id_doc_feap,
                    Date = DateTime.UtcNow,
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

            obj.DocumentLines = new List<DocumentLine>();

            foreach (var line in contract.InvoiceLines)
            {
                obj.DocumentLines.Add(new DocumentLine()
                {
                    LineId = int.Parse(line.Id),
                    DocumentId = contract.documentId,
                    Description = line.Name,
                    StandardItemIdentification = line.StandardItemIdentification,
                    Quantity = int.Parse(line.Quantity),
                    Value = decimal.Parse(line.TotalLineValue, CultureInfo.InvariantCulture),
                    TaxPercentage = decimal.Parse(line.TaxPercent, CultureInfo.InvariantCulture)
                });
            }
        }

        private void FillWithContract(in SendDocumentContract contract, ref SetDocFaturacao obj)
        {
            obj.id_ano_letivo = contract.SchoolYear;

            obj.nif = contract.supplierFiscalId.Substring(2);

            var faturaToSend = new SetDocFaturacao.fatura()
            {
                id_doc_feap = contract.documentId,
                num_fatura = contract.referenceNumber,
                total_fatura = contract.TotalAmount,
                fatura_base64 = contract.pdfFormat,
                tp_doc = contract.documentType == DocumentTypeEnum.Fatura ? "FAT" : "NTC",
                dt_fatura = contract.issueDate.ToString("dd-MM-yyyy"),
                num_doc_rel = contract.documentType == DocumentTypeEnum.NotaCrédito ? contract.RelatedReferenceNumber : null,
                num_compromisso = contract.CompromiseNumber
            };

            faturaToSend.linhas = new List<SetDocFaturacao.fatura.linhaModel>();

            foreach (var line in contract.InvoiceLines)
            {
                faturaToSend.linhas.Add(new SetDocFaturacao.fatura.linhaModel()
                {
                    num_linha = int.Parse(line.Id),
                    id_linha = line.StandardItemIdentification,
                    descricao_linha = line.Name,
                    qtd_linha = int.Parse(line.Quantity),
                    valor_linha = decimal.Parse(line.TotalLineValue, CultureInfo.InvariantCulture),
                    perc_iva_linha = decimal.Parse(line.TaxPercent, CultureInfo.InvariantCulture),
                });
            }

            obj.faturas = new List<SetDocFaturacao.fatura>
            {
                faturaToSend
            };
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
                tp_doc = document.TypeId == DocumentTypeEnum.Fatura ? "FAT" : "NTC",
                dt_fatura = document.IssueDate.ToString("dd-MM-yyyy"),
                num_doc_rel = document.TypeId == DocumentTypeEnum.NotaCrédito ? document.RelatedDocument.ReferenceNumber : null,
                num_compromisso = document.CompromiseNumber,
                estado_doc = "5",
                dt_estado = document.StateDate.ToString("dd-MM-yyyy")
            };

            faturaToSend.linhas = new List<SetDocFaturacao.fatura.linhaModel>();

            foreach (var line in document.DocumentLines)
            {
                faturaToSend.linhas.Add(new SetDocFaturacao.fatura.linhaModel()
                {
                    num_linha = line.LineId,
                    id_linha = line.StandardItemIdentification,
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
