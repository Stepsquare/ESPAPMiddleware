using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EspapMiddleware.ServiceLayer.Helpers
{
    public static class FileManager
    {
        public static void SaveFile(string service,  string content, string uniqueId = null)
        {
            try
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(content);

                if (string.IsNullOrEmpty(uniqueId))
                {
                    var nsmgr = new XmlNamespaceManager(xdoc.NameTable);

                    nsmgr.AddNamespace("soap", "http://www.w3.org/2003/05/soap-envelope");
                    nsmgr.AddNamespace("urnl", "urnl:ElectronicInvoice.B2BClientOperations");

                    uniqueId = xdoc.SelectSingleNode("//soap:Envelope/soap:Body/urnl:SendDocumentMCIn/urnl:uniqueId", nsmgr)?.InnerText;
                }

                var path = ConfigurationManager.AppSettings["LogDirectory"] + $@"\{service}";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                xdoc.Save(path + $@"\{uniqueId}.xml");
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}
