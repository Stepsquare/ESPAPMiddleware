using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.WebServiceModels
{
    public class SetDocFaturacaoResponse : GenericPostResponse
    {
        public List<fatura> faturas { get; set; }

        public class fatura
        {
            public string num_fatura { get; set; }
            public string id_doc_feap { get; set; }
            public string id_me_fatura { get; set; }
            public string cod_msg_fat { get; set; }
            public string msg_fat { get; set; }
            public string state_id { get; set; }
            public string reason { get; set; }
        }
    }
}
