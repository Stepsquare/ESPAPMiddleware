using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.Exceptions
{
    public class ContractValidationException : Exception
    {
        public ContractValidationException(string msg) : base(msg) { }

        public ContractValidationException(string[] errorMessages) : base(string.Join("\r\n", errorMessages)) { }

    }
}
