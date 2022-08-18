using EspapMiddleware.DataLayer.Context;
using EspapMiddleware.Shared.Entities;
using EspapMiddleware.Shared.Enums;
using EspapMiddleware.Shared.Interfaces.IRepositories;
using EspapMiddleware.Shared.MonitorServiceModels;
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

        public async Task<Document> GetDocumentForSyncSigefe(string documentId)
        {
            return await DbContext.Documents
                .Include(x => x.DocumentMessages)
                .Include(x => x.RequestLogs)
                .FirstOrDefaultAsync(x => x.DocumentId == documentId);
        }

        public async Task<Document> GetRelatedDocument(string relatedReferenceNumber, string supplierFiscalId, string schoolYear, DocumentTypeEnum type)
        {
            return await DbContext.Documents
                                .Where(x => x.ReferenceNumber == relatedReferenceNumber
                                        && x.SupplierFiscalId == supplierFiscalId
                                        && x.SchoolYear == schoolYear
                                        && ((type == DocumentTypeEnum.Fatura && x.TypeId == DocumentTypeEnum.NotaCrédito)
                                            || (type != DocumentTypeEnum.Fatura && (x.TypeId == DocumentTypeEnum.Fatura || x.TypeId == DocumentTypeEnum.NotaCrédito))))
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
                                        && (!filters.FromDate.HasValue || x.IssueDate > filters.FromDate)
                                        && (!filters.UntilDate.HasValue || x.IssueDate < filters.UntilDate)
                                        && (string.IsNullOrEmpty(filters.SupplierFiscalId) || x.SupplierFiscalId.Contains(filters.SupplierFiscalId))
                                        && (string.IsNullOrEmpty(filters.SchoolYear) || x.SchoolYear == filters.SchoolYear)
                                        && (string.IsNullOrEmpty(filters.CompromiseNumber) || x.CompromiseNumber == filters.CompromiseNumber)
                                        && (!filters.State.HasValue || x.StateId == filters.State)
                                        && (!filters.Type.HasValue || x.TypeId == filters.Type)
                                        && (string.IsNullOrEmpty(filters.MeId) || x.MEId == filters.MeId)
                                        && (!filters.SigefeSyncronized.HasValue || x.IsSynchronizedWithSigefe == filters.SigefeSyncronized)
                                        && (!filters.FeapSyncronized.HasValue || x.IsSynchronizedWithFEAP == filters.FeapSyncronized))
                                .OrderByDescending(x => x.IssueDate)
                                .Skip((filters.PageIndex - 1) * filters.PageSize).Take(filters.PageSize)
                                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetSchoolYears()
        {
            return await DbContext.Documents.OrderBy(x => x.SchoolYear).Select(x => x.SchoolYear).Distinct().ToListAsync();
        }
    }
}
