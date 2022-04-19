using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.Interfaces.IConfiguration
{
    public interface IUnitOfWorkFactory
    {
        IUnitOfWork Create();
    }
}
