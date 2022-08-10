using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.Shared.Interfaces.IHelpers
{
    public interface IGenericRestRequestManager
    {
        Task<T> Get<T>(string webservice, IDictionary<string, string> headers = null) where T : class;
        Task<TOutput> Post<TOutput, TInput>(string webservice, TInput bodyObject, IDictionary<string, string> headers = null) where TOutput : class where TInput : class;
    }
}
