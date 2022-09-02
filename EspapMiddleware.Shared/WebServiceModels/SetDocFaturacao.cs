using EspapMiddleware.Shared.DataContracts;
using EspapMiddleware.Shared.Entities;
using EspapMiddleware.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.WebServiceModels
{
    public class SetDocFaturacao
    {
        public string id_ano_letivo { get; set; }
        public string nif { get; set; }
        public List<fatura> faturas { get; set; }

        public class fatura
        {
            public string num_fatura { get; set; }
            public string total_fatura { get; set; }
            public string fatura_base64 { get; set; }
            public string tp_doc { get; set; }
            public string dt_fatura { get; set; }
            public string num_doc_rel { get; set; }
            public string id_doc_feap { get; set; }
            public string id_me_fatura { get; set; }
            public string estado_doc { get; set; }
            public string dt_estado { get; set; }
            public string num_compromisso { get; set; }
            public List<linhaModel> linhas { get; set; }

            public class linhaModel
            {
                public string num_linha { get; set; }
                public int id_linha { get; set; }
                public string descricao_linha { get; set; }
                public int qtd_linha { get; set; }
                public decimal valor_linha { get; set; }
                public decimal perc_iva_linha { get; set; }
            }
        }
    }
}
