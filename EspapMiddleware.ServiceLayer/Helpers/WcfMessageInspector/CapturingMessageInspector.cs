using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.ServiceLayer.Helpers.WcfMessageInspector
{
    public class CapturingMessageInspector : IClientMessageInspector
    {
        public InspectedSOAPMessages SoapMessages { get; set; }

        public CapturingMessageInspector(InspectedSOAPMessages soapMessages)
        {
            this.SoapMessages = soapMessages;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            this.SoapMessages.Response = reply.ToString();
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            this.SoapMessages.Request = request.ToString();
            return null;
        }
    }
}
