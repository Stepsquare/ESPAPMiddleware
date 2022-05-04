using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.ServiceLayer.Helpers.OutboundMessageInspector
{
    public class MessageInspector : IClientMessageInspector
    {
        private readonly string _uniqueId;

        public MessageInspector(string uniqueId)
        {
            _uniqueId = uniqueId;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            return;
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            FileManager.SaveFile("SetDocument", request?.ToString(), _uniqueId);

            return null;
        }
    }
}
