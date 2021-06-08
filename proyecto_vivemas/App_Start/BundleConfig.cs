using System.Web;
using System.Web.Optimization;

namespace proyecto_vivemas
{
    public class BundleConfig
    {
        // Para obtener más información sobre las uniones, visite https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                          "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));
            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                        "~/Content/jquery/dist/jquery-3.5.1.min.js",
                        "~/Content/jquery-validate/dist/jquery.validate.min.js",
                        "~/Content/bootstrap-4.5.0/js/bootstrap.bundle.min.js",
                        "~/Content/Admin/js/coreui.bundle.js",
                        "~/Content/toastr/toastr.min.js",
                        "~/Content/tabulator/dist/js/tabulator.min.js",
                        "~/Scripts/util/utilities.js",                        
                        "~/Content/select2/js/select2.full.min.js",
                        "~/Content/html2pdf/html2pdf.bundle.min.js",
                        "~/Content/moment/moment.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/reports").Include(
                        "~/Content/html2pdf/html2pdf.bundle.min.js",
                        "~/Content/table2excel/jquery.table2excel.min.js"
                      ));

            // Utilice la versión de desarrollo de Modernizr para desarrollar y obtener información. De este modo, estará
            // para la producción, use la herramienta de compilación disponible en https://modernizr.com para seleccionar solo las pruebas que necesite.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/login").Include(
                    "~/Scripts/login/login.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap-4.5.0/bootstrap.min.css",
                      "~/Content/Admin/css/coreui.min.css",                     
                      "~/Content/toastr/toastr.min.css",
                      "~/Content/tabulator/dist/css/tabulator.min.css",
                      "~/Content/tabulator/dist/css/bootstrap/tabulator_bootstrap.min.css",                     
                      "~/Content/select2/css/select2.min.css",
                      "~/Content/select2/css/select2-bootstrap4.min.css",
                      "~/Content/Site.css"));
        }
    }
}
