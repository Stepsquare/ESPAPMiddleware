using System.Web;
using System.Web.Optimization;

namespace EspapMiddleware.SVFMonitor
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Content/lib/jquery/jquery.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Content/lib/bootstrap/dist/js/bootstrap.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/kendo-ui").Include(
                      "~/Content/lib/kendo-ui/js/kendo.all.min.js",
                      "~/Content/lib/kendo-ui/js/cultures/kendo.culture.pt-PT.min.js",
                      "~/Content/lib/kendo-ui/js/messages/kendo.messages.pt-PT.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/lib/font-awesome/css/all.min.css",
                      "~/Content/lib/bootstrap/dist/css/bootstrap.min.css",
                      "~/Content/css/site.css",
                      "~/Content/lib/kendo-ui/styles/kendo.common-nova.min.css",
                      "~/Content/lib/kendo-ui/styles/kendo.material.min.css"));
        }
    }
}
