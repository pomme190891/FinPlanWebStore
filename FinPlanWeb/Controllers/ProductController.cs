using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FinPlanWeb.Database;

using System.Web.Script.Serialization;

namespace FinPlanWeb.Controllers
{
    public class ProductController : Controller
    {
        //
        // GET: /Product/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ProductView()
        {
            var products = ProductManagement.GetProducts(ProductManagement.ProductType.All);
            ViewBag.Products = new JavaScriptSerializer().Serialize(products);
            return View();
        }


        public ActionResult FilterProduct(ProductManagement.ProductType type)
        {
             return Json(ProductManagement.GetProducts(type), JsonRequestBehavior.AllowGet);
        }

    }
}
