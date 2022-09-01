using Autofac;
using Autofac.Core;
using EspapMiddleware.ServiceLayer.Helpers;
using EspapMiddleware.ServiceLayer.UnitOfWorkFactory;
using EspapMiddleware.Shared.ConfigModels;
using EspapMiddleware.Shared.Interfaces.IConfiguration;
using EspapMiddleware.Shared.Interfaces.IHelpers;
using EspapMiddleware.Shared.Interfaces.IServices;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;
using Topshelf.Autofac;

namespace EspapMiddleware.SyncService
{
    internal class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            var builder = new ContainerBuilder();
            builder.RegisterType<UnitOfWorkFactory>().As<IUnitOfWorkFactory>();
            builder.RegisterType<GenericRestRequestManager>().As<IGenericRestRequestManager>()
                .WithParameters(new List<Parameter> { 
                    new NamedParameter("environment", (EnvironmentConfig)ConfigurationManager.GetSection("FaturacaoWebServicesConfig/environment")),
                    new NamedParameter("webservices", (NameValueCollection)ConfigurationManager.GetSection("FaturacaoWebServicesConfig/webServices")) 
                });
            builder.RegisterType<ServiceLayer.Services.SyncronizationService>().As<ISyncronizationService>();
            builder.RegisterType<SyncronizationService>();

            var container = builder.Build();

            HostFactory.Run(hostConfigurator =>
            {
                hostConfigurator.SetServiceName("SVF Syncronization Service");
                hostConfigurator.SetDisplayName("SVF Syncronization Service");
                hostConfigurator.SetDescription("Serviço de sincronização de faturas com SIGeFE e FE-AP.");

                hostConfigurator.RunAsLocalSystem();

                hostConfigurator.UseLog4Net();

                hostConfigurator.UseAutofacContainer(container);

                hostConfigurator.Service<SyncronizationService>(serviceConfigurator =>
                {
                    serviceConfigurator.ConstructUsingAutofacContainer();
                    serviceConfigurator.WhenStarted(service => service.OnStart());
                    serviceConfigurator.WhenStopped(service => service.OnStop());
                });
            });
        }
    }
}
