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

        public async Task UnprocessedSyncronizationSigefe()
        {
            
        }

        public async Task PaidDocsSyncSyncronizationFeap()
        {
            
        }
    }
}
