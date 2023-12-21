using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.WebServiceModels
{
    public class SetEstadoDocFaturacao
    {
        public string id_ano_letivo { get; set; }
        public string nif { get; set; }
        public string id_me_fatura { get; set; }
        public string estado_doc { get; set; }
    }
}
