using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Configuration;
using System.Web;

namespace EspapMiddleware.WcfService.RequestLogMessageInspectorBehavior
{
    public class RequestLogBehaviorExtentionElement : BehaviorExtensionElement
    {
        public override Type BehaviorType
        {
            get
            {
                return typeof(RequestLogBehavior);
            }
        }

        protected override object CreateBehavior()
        {
            return new RequestLogBehavior();
        }
    }
}