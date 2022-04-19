using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Selectors;
using System.Linq;
using System.Security.Authentication;
using System.Web;

namespace EspapMiddleware.WcfService
{
    public class ServiceAuthenticator : UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))

                throw new InvalidCredentialException("Username and password required");

            if (!(userName == ConfigurationManager.AppSettings["EspapMiddleware_User"] && password == ConfigurationManager.AppSettings["EspapMiddleware_Pass"]))

                throw new InvalidCredentialException(string.Format("Wrong username ({0}) or password ", userName));
        }
    }
}