using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.ServiceLayer.Helpers.InboundMessageInspector
{
    public class MessageInspector : IDispatchMessageInspector
    {
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            FileManager.SaveFile(request?.Headers?.Action?.Substring(28), request?.ToString());

            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            return;
        }
    }
}
