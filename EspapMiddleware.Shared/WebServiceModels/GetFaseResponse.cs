using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.WebServiceModels
{
    public class GetFaseResponse
    {
        public int id_fase { get; set; }
        public string cod_fase { get; set; }
        public string cod_estado_fase { get; set; }
        public string id_ano_letivo_atual { get; set; }
        public string des_id_ano_letivo_atual { get; set; }
        public string id_ano_letivo_anterior { get; set; }
        public string des_id_ano_letivo_anterior { get; set; }
    }
}
