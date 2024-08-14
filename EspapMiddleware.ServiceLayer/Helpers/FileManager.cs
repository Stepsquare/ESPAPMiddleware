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
        public static void SaveFile(string service, string content, string uniqueId = null)
        {
            try
            {
                XmlDocument xdoc = new XmlDocument();

                xdoc.LoadXml(content);

                if (string.IsNullOrEmpty(uniqueId))
                {
                    var nsmgr = new XmlNamespaceManager(xdoc.NameTable);

                    nsmgr.AddNamespace("soap", "http://www.w3.org/2003/05/soap-envelope");
                    nsmgr.AddNamespace("urn", "urn:ElectronicInvoice.B2BClientOperations");

                    switch (service)
                    {
                        case "SendDocument":
                            uniqueId = xdoc.SelectSingleNode("//soap:Envelope/soap:Body/urn:SendDocumentMCIn/urn:uniqueId", nsmgr)?.InnerText;
                            break;
                        case "SetDocumentResult":
                            uniqueId = xdoc.SelectSingleNode("//soap:Envelope/soap:Body/urn:SetDocumentResultMCIn/urn:uniqueId", nsmgr)?.InnerText;
                            break;
                        default:
                            break;
                    }
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

        public static bool FileExists(string service, string uniqueId)
        {
            string path = ConfigurationManager.AppSettings["LogDirectory"] + $@"\{service}\{uniqueId}.xml";

            return File.Exists(path);
        }

        public static byte[] GetFile(string service, string uniqueId)
        {
            string path = ConfigurationManager.AppSettings["LogDirectory"] + $@"\{service}\{uniqueId}.xml";

            try
            {
                return File.ReadAllBytes(path);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
