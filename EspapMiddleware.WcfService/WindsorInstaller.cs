using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using EspapMiddleware.ServiceLayer.Services;
using EspapMiddleware.ServiceLayer.UnitOfWorkFactory;
using EspapMiddleware.Shared.Interfaces.IConfiguration;
using EspapMiddleware.Shared.Interfaces.IServices;

namespace EspapMiddleware.WcfService
{
    public class WindsorInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IUnitOfWorkFactory, UnitOfWorkFactory>(),
                Component.For<IDocumentService, DocumentService>(),
                Component.For<IService, Service>());
        }
    }
}