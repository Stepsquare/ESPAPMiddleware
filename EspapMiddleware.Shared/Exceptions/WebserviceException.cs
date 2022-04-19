using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.Exceptions
{
    public class WebserviceException : Exception
    {
        public WebserviceException(string msg) : base(msg) { }

        public WebserviceException(string[] errorMessages) : base(string.Join("\r\n", errorMessages)) { }

    }
}
