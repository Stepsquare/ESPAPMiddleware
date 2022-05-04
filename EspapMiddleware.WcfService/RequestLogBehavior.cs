using EspapMiddleware.ServiceLayer.Helpers.InboundMessageInspector;
using System;
using System.ServiceModel.Configuration;

namespace EspapMiddleware.WcfService
{
    public class RequestLogBehavior : BehaviorExtensionElement
    {
        public override Type BehaviorType
        {
            get
            {
                return typeof(ServiceBehavior);
            }
        }

        protected override object CreateBehavior()
        {
            return new ServiceBehavior();
        }
    }
}