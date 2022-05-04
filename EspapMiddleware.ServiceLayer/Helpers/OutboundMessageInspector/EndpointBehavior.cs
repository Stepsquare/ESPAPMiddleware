using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.ServiceLayer.Helpers.OutboundMessageInspector
{
    public class EndpointBehavior : IEndpointBehavior
    {
        private readonly string _uniqueId;

        public EndpointBehavior(string uniqueId)
        {
            _uniqueId = uniqueId;
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { return; }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(new MessageInspector(_uniqueId));
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { return; }

        public void Validate(ServiceEndpoint endpoint) { return; }
    }
}
