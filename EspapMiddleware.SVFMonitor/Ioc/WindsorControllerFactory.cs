﻿using Castle.MicroKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EspapMiddleware.SVFMonitor.Ioc
{
    public class WindsorControllerFactory : DefaultControllerFactory
    {
		private readonly IKernel _kernel;

		public WindsorControllerFactory(IKernel kernel)
		{
			this._kernel = kernel;
		}

		public override void ReleaseController(IController controller)
		{
			_kernel.ReleaseComponent(controller);
		}

		protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
		{
			if (controllerType == null)
			{
				throw new HttpException(404, string.Format("The controller for path '{0}' could not be found.", requestContext.HttpContext.Request.Path));
			}
			return (IController)_kernel.Resolve(controllerType);
		}
	}
}