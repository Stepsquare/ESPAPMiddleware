using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.Exceptions
{
    public class SyncronizationException : Exception
    {
        public SyncronizationException(string msg) : base(msg) { }

        public SyncronizationException(string[] errorMessages) : base(string.Join("\r\n", errorMessages)) { }

    }
}
