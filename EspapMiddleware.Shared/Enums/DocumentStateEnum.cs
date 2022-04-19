using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.Enums
{
    [DataContract]
    public enum DocumentStateEnum
    {
        [EnumMember(Value = "1")]
        Iniciado = 1,
        [EnumMember(Value = "31")]
        AguardaRedigitalização = 31,
        [EnumMember(Value = "7")]
        PréRegistado = 7,
        [EnumMember(Value = "35")]
        ValidadoConferido = 35,
        [EnumMember(Value = "11")]
        Processado = 11,
        [EnumMember(Value = "33")]
        EmitidoPagamento = 33,
        [EnumMember(Value = "22")]
        Devolvido = 22
    }
}
