using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using EspapMiddleware.ServiceLayer.Helpers;
using EspapMiddleware.ServiceLayer.Services;
using EspapMiddleware.ServiceLayer.UnitOfWorkFactory;
using EspapMiddleware.Shared.ConfigModels;
using EspapMiddleware.Shared.Interfaces.IConfiguration;
using EspapMiddleware.Shared.Interfaces.IHelpers;
using EspapMiddleware.Shared.Interfaces.IServices;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web;

namespace EspapMiddleware.SVFMonitor.Ioc
{
    public class MonitorServiceInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IUnitOfWorkFactory, UnitOfWorkFactory>(),
                Component.For<IGenericRestRequestManager, GenericRestRequestManager>()
                    .DependsOn(new Dependency[] {
                        Dependency.OnValue("environment", (EnvironmentConfig)ConfigurationManager.GetSection("FaturacaoWebServicesConfig/environment")),
                        Dependency.OnValue("webservices", (NameValueCollection)ConfigurationManager.GetSection("FaturacaoWebServicesConfig/webServices"))}
                    ),
                Component.For<IMonitorServices>().ImplementedBy<MonitorService>());
        }
    }
}