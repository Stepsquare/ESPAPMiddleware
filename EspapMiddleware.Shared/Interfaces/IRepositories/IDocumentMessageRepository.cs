﻿using EspapMiddleware.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.Interfaces.IRepositories
{
    public interface IDocumentMessageRepository : IGenericRepository<DocumentMessage>
    {
        Task<DocumentMessage> GetLastSigefeMessage(string documentId);
    }
}
