using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.Enums
{
    [DataContract]
    public enum DocumentTypeEnum
    {
        [EnumMember(Value = "1")]
        Fatura = 1,
        [EnumMember(Value = "2")]
        NotaCrédito = 2,
        [EnumMember(Value = "3")]
        NotaDébito = 3
    }
}
