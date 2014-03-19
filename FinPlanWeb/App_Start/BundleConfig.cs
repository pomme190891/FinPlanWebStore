using System.Web;
using System.Web.Optimization;

namespace FinPlanWeb
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.IgnoreList.Clear();

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-1.10.2.js",
                        "~/Scripts/jquery.easing.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/scrollingnavjs").Include(
                        "~/Scripts/scrolling-nav.js"));

            bundles.Add(new ScriptBundle("~/Content/fontawesomecss").Include(
                        "~/Content/font-awesome.min.css"));

            bundles.Add(new ScriptBundle("~/Content/scrollingnavcss").Include(
                        "~/Content/scrolling-nav.css"));

            bundles.Add(new ScriptBundle("~/Content/stylistcss").Include(
                        "~/Content/stylish-portfolio.css"));
            
            bundles.Add(new ScriptBundle("~/bundles/bootstrapjs").Include(
                        "~/Scripts/bootstrap.min.js",
                        "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/bootstrapcss").Include(
                        "~/Content/bootstrap.css",
                        "~/Content/bootstrap.min.css",
                        "~/Content/bootstrap-theme.min.css"));


        }
    }
}