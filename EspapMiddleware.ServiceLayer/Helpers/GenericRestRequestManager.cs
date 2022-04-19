using EspapMiddleware.Shared.ConfigModels;
using EspapMiddleware.Shared.Exceptions;
using EspapMiddleware.Shared.WebServiceModels;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EspapMiddleware.ServiceLayer.Helpers
{
    public class GenericRestRequestManager
    {
        private EnvironmentConfig Environment { get; set; }
        private NameValueCollection Webservices { get; set; }

        public GenericRestRequestManager(EnvironmentConfig environment, NameValueCollection webservices)
        {
            Environment = environment;
            Webservices = webservices;
        }

        public async Task<T> Get<T>(string webservice, IDictionary<string, string> headers = null) where T : class
        {
            var client = new RestClient(WebServiceUrl(webservice));

            var request = new RestRequest();

            AddCredentialHeaders(request);

            if (headers != null && headers.Any())
                foreach (var header in headers)
                    request.AddHeader(header.Key, header.Value);

            RestResponse<T> response = await client.ExecuteGetAsync<T>(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }
            else
            {
                throw new WebserviceException($"{response.StatusCode} - {response.ErrorMessage}");
            }
        }

        public async Task<TOutput> Post<TOutput, TInput>(string webservice, TInput bodyObject, IDictionary<string, string> headers = null) where TOutput : class where TInput : class
        {
            var client = new RestClient(WebServiceUrl(webservice));

            var request = new RestRequest();

            AddCredentialHeaders(request);

            if (headers != null && headers.Any())
                foreach (var header in headers)
                    request.AddHeader(header.Key, header.Value);

            request.AddJsonBody(bodyObject);

            RestResponse<TOutput> response = await client.ExecutePostAsync<TOutput>(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }
            else
            {
                throw new WebserviceException($"{(int)response.StatusCode} - {response.StatusDescription}");
            }
        }

        #region Private methods
        private void AddCredentialHeaders(in RestRequest request)
        {
            request.AddHeader("username", Environment.Username);
            request.AddHeader("password", Environment.Password);
        }

        private string WebServiceUrl(string service)
        {
            return string.Concat(Environment.BaseUrl, Webservices[service]);
        }
        #endregion
    }
}
