using EspapMiddleware.DataLayer.UnitOfWork;
using EspapMiddleware.Shared.Interfaces.IConfiguration;

namespace EspapMiddleware.ServiceLayer.UnitOfWorkFactory
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        public IUnitOfWork Create()
        {
            return new UnitOfWork();
        }
    }
}
