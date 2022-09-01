using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.Interfaces.IServices
{
    public interface ISyncronizationService
    {
        Task SigefeBulkSyncronization();
        Task FeapBulkSyncronization();
    }
}
