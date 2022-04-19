using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.WebServiceModels
{
    public class GetDocFaturacaoResponse
    {
        public int id_ano_letivo { get; set; }
        public string nif { get; set; }
        public string id_doc_feap { get; set; }
        public List<documento> documentos { get; set; }

        public class documento
        {
            public string documentId { get; set; }
            public string referenceNumber { get; set; }
            public string documentType { get; set; }
            public string issueDate { get; set; }
            public string supplierFiscalId { get; set; }
            public string stateId { get; set; }
            public string reason { get; set; }
            public int number { get; set; }
        }
    }
}
