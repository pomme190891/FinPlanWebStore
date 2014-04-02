using System.Collections.Generic;
using System.Web.Mvc;
using FinPlanWeb.Database;

using System.Web.Script.Serialization;
using FinPlanWeb.Models;

namespace FinPlanWeb.Controllers
{
    public class ProductController : BaseController
    {
        //
        // GET: /Product/
        private void InitialiseCart()
        {
            var cart = Session["Cart"];
            if (cart == null)
            {
                Session["Cart"] = new List<CartItem>();
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ProductView()
        {
            InitialiseCart();
            var products = ProductManagement.GetProducts(ProductManagement.ProductType.All);
            ViewBag.Products = new JavaScriptSerializer().Serialize(products);
            ViewBag.ShoppingCart = new JavaScriptSerializer().Serialize(Session["Cart"] as List<CartItem>);
            return View();

            //CTRL + T = Search by class name
            // CTRL + SHIFT + T = Search by file name
            //Alt + SHIFT + T = Search by symbol (method Name, variable, field name, etc)
            // CTRL + R + R = Rename 
            //CTRL + D = Duplicate Row
            // ALT + Enter = show resharper suggestion
        }


        public ActionResult FilterProduct(ProductManagement.ProductType type)
        {
             return Json(ProductManagement.GetProducts(type), JsonRequestBehavior.AllowGet);
        }

    }
}
