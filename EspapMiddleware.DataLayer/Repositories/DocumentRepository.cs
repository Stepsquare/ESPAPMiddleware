using EspapMiddleware.DataLayer.Context;
using EspapMiddleware.Shared.Entities;
using EspapMiddleware.Shared.Enums;
using EspapMiddleware.Shared.Interfaces.IRepositories;
using EspapMiddleware.Shared.MonitorServiceModels;
using EspapMiddleware.Shared.MonitorServiceModels.PaginationModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.DataLayer.Repositories
{
    public class DocumentRepository : GenericRepository<Document>, IDocumentRepository
    {
        public DocumentRepository(EspapMiddlewareDbContext context) : base(context)
        {
        }

        public override void Add(Document entity)
        {
            entity.CreatedOn = DateTime.Now;
            DbContext.Set<Document>().Add(entity);
        }

        public override void Update(Document entity)
        {
            entity.UpdatedOn = DateTime.Now;
            DbContext.Set<Document>().Attach(entity);
            DbContext.Entry(entity).State = EntityState.Modified;
        }

        public async Task<Document> GetDocumentForSyncSigefe(string documentId)
        {
            return await DbContext.Documents
                .Include(x => x.DocumentLines)
                .Include(x => x.RelatedDocument)
                .FirstOrDefaultAsync(x => x.DocumentId == documentId);
        }

        public async Task<Document> GetRelatedDocument(string referenceNumber, string supplierFiscalId, string schoolYear, DocumentTypeEnum type)
        {
            return await DbContext.Documents
                                .Where(x => x.SupplierFiscalId == supplierFiscalId
                                        && x.SchoolYear == schoolYear
                                        && ((type == DocumentTypeEnum.Fatura && x.TypeId == DocumentTypeEnum.NotaCrédito && x.RelatedReferenceNumber == referenceNumber)
                                            || (type == DocumentTypeEnum.NotaCrédito && x.TypeId == DocumentTypeEnum.Fatura && x.ReferenceNumber == referenceNumber)
                                            || (type == DocumentTypeEnum.NotaDébito && x.TypeId == DocumentTypeEnum.NotaCrédito && x.ReferenceNumber == referenceNumber)))
                                .Include(x => x.DocumentLines)
                                .FirstOrDefaultAsync();
        }

        public async Task<Document> GetDocumentForDetail(string documentId)
        {
            return await DbContext.Documents
                .Include(x => x.DocumentMessages)
                .Include(x => x.RequestLogs)
                .FirstOrDefaultAsync(x => x.DocumentId == documentId);
        }

        public async Task<IEnumerable<Document>> GetFilteredPaginated(DocumentSearchFilters filters)
        {
            return await DbContext.Documents
                                .Where(x => (string.IsNullOrEmpty(filters.DocumentId) || x.DocumentId == filters.DocumentId)
                                        && (!filters.FromDate.HasValue || x.CreatedOn >= filters.FromDate)
                                        && (!filters.UntilDate.HasValue || x.CreatedOn <= filters.UntilDate)
                                        && (string.IsNullOrEmpty(filters.SupplierFiscalId) || x.SupplierFiscalId.Contains(filters.SupplierFiscalId))
                                        && (string.IsNullOrEmpty(filters.SchoolYear) || x.SchoolYear == filters.SchoolYear)
                                        && (string.IsNullOrEmpty(filters.CompromiseNumber) || x.CompromiseNumber == filters.CompromiseNumber)
                                        && (string.IsNullOrEmpty(filters.ReferenceNumber) || x.ReferenceNumber == filters.ReferenceNumber)
                                        && (!filters.State.HasValue || x.StateId == filters.State)
                                        && (!filters.Type.HasValue || x.TypeId == filters.Type)
                                        && (string.IsNullOrEmpty(filters.MeId) || x.MEId == filters.MeId)
                                        && (!filters.SigefeSyncronized.HasValue || x.IsSynchronizedWithSigefe == filters.SigefeSyncronized)
                                        && (!filters.FeapSyncronized.HasValue || x.IsSynchronizedWithFEAP == filters.FeapSyncronized))
                                .OrderByDescending(x => x.CreatedOn)
                                .Skip((filters.PageIndex - 1) * filters.PageSize).Take(filters.PageSize)
                                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetSchoolYears()
        {
            return await DbContext.Documents.Where(x => !string.IsNullOrEmpty(x.SchoolYear)).Select(x => x.SchoolYear).Distinct().ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetDocumentsToSyncFeap(string anoLetivo)
        {
            return await DbContext.Documents.Where(x => x.SchoolYear == anoLetivo
                                                    && !x.IsSynchronizedWithFEAP)
                                                .Include(x => x.DocumentMessages)
                                                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetPaidDocsToSync(string[] documentIds)
        {
            return await DbContext.Documents
                                .Where(x => documentIds.Contains(x.DocumentId) && x.StateId == DocumentStateEnum.ValidadoConferido)
                                .ToListAsync();
        }

        public async Task<int> GetPaidDocsToSyncCount(string[] documentIds)
        {
            return await DbContext.Documents
                                .Where(x => documentIds.Contains(x.DocumentId) && x.StateId == DocumentStateEnum.ValidadoConferido)
                                .CountAsync();
        }

        public async Task<IEnumerable<Document>> GetPaginatedDocsToSyncSigefe(string anoLetivo, PaginatedSearchFilter filters)
        {
            return await DbContext.Documents
                                .Where(x => (string.IsNullOrEmpty(x.SchoolYear) || x.SchoolYear == anoLetivo)
                                        && !x.IsSynchronizedWithSigefe
                                        && !x.DocumentMessages.Any(m => m.MessageTypeId == DocumentMessageTypeEnum.SIGeFE
                                                                    && m.MessageCode == "490")
                                        && x.DocumentMessages.Any(m => m.MessageTypeId == DocumentMessageTypeEnum.SIGeFE 
                                                                    && (m.MessageCode == "500" || m.MessageCode == "429")))
                                .OrderByDescending(x => x.CreatedOn)
                                .Include(x => x.DocumentMessages)
                                .Skip((filters.PageIndex - 1) * filters.PageSize)
                                .Take(filters.PageSize)
                                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetDocsToSyncSigefe(string anoLetivo, string documentId = null)
        {
            return await DbContext.Documents
                                .Where(x => (string.IsNullOrEmpty(documentId) || x.DocumentId == documentId)
                                        && (string.IsNullOrEmpty(x.SchoolYear) || x.SchoolYear == anoLetivo)
                                        && !x.IsSynchronizedWithSigefe
                                        && !x.DocumentMessages.Any(m => m.MessageTypeId == DocumentMessageTypeEnum.SIGeFE
                                                                    && m.MessageCode == "490")
                                        && x.DocumentMessages.Any(m => m.MessageTypeId == DocumentMessageTypeEnum.SIGeFE
                                                                    && (m.MessageCode == "500" || m.MessageCode == "429")))
                                .Include(x => x.DocumentLines)
                                .ToListAsync();
        }

        public async Task<int> GetDocsToSyncSigefeCount(string anoLetivo)
        {
            return await DbContext.Documents
                                .Where(x => (string.IsNullOrEmpty(x.SchoolYear) || x.SchoolYear == anoLetivo)
                                        && !x.IsSynchronizedWithSigefe
                                        && !x.DocumentMessages.Any(m => m.MessageTypeId == DocumentMessageTypeEnum.SIGeFE
                                                                    && m.MessageCode == "490")
                                        && x.DocumentMessages.Any(m => m.MessageTypeId == DocumentMessageTypeEnum.SIGeFE
                                                                    && (m.MessageCode == "500" || m.MessageCode == "429")))
                                .CountAsync();
        }

        public async Task<IEnumerable<Document>> GetCreditNotesToReprocess(string anoLetivo)
        {
            return await DbContext.Documents
                                .Where(x => x.SchoolYear == anoLetivo
                                        && !x.IsSynchronizedWithSigefe
                                        && x.TypeId == DocumentTypeEnum.NotaCrédito
                                        && x.StateId == DocumentStateEnum.Iniciado
                                        && !x.ActionId.HasValue
                                        && x.DocumentMessages.Any(m => m.MessageTypeId == DocumentMessageTypeEnum.SIGeFE 
                                                                    && m.MessageCode == "490"))
                                .Include(x => x.DocumentLines)
                                .ToArrayAsync();
        }

        public async Task<int> GetCreditNotesToReprocessCount(string anoLetivo)
        {
            return await DbContext.Documents
                                .Where(x => x.SchoolYear == anoLetivo
                                        && !x.IsSynchronizedWithSigefe
                                        && x.TypeId == DocumentTypeEnum.NotaCrédito
                                        && x.StateId == DocumentStateEnum.Iniciado
                                        && !x.ActionId.HasValue
                                        && x.DocumentMessages.Any(m => m.MessageTypeId == DocumentMessageTypeEnum.SIGeFE
                                                                    && m.MessageCode == "490"))
                                .CountAsync();
        }
    }
}
