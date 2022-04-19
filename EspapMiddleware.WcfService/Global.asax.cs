using Castle.Facilities.WcfIntegration;
using Castle.Windsor;
using System;

namespace EspapMiddleware.WcfService
{
    public class Global : System.Web.HttpApplication
    {
        static IWindsorContainer _container;

        protected void Application_Start(object sender, EventArgs e)
        {
            _container = new WindsorContainer();
            _container.AddFacility<WcfFacility>();
            _container.Install(new WindsorInstaller());
        }
    }
}