using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.Enums
{
    [DataContract]
    public enum DocumentActionEnum
    {
        [EnumMember(Value = "12")]
        RegularizaçãoRececionada = 12,
        [EnumMember(Value = "17")]
        SolicitaçãoDocumentoRegularização = 17,
        [EnumMember(Value = "19")]
        AprovaçãoRececionada = 19,
        [EnumMember(Value = "20")]
        SolicitaçãoAprovação = 20,
        [EnumMember(Value = "22")]
        AceitaçãoNotaCrédito = 22
    }
}
