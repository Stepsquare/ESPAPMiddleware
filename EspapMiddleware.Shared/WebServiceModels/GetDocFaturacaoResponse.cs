using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.WebServiceModels
{
    public class GetDocFaturacaoResponse
    {
        public List<documento> documentos { get; set; }

        public class documento
        {
            public string id_doc_feap { get; set; }
            public int id_me_fatura { get; set; }
        }
    }
}
