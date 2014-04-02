using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using FinPlanWeb.Database;
using FinPlanWeb.Models;

namespace FinPlanWeb.Controllers
{
    public class ShoppingCartController : BaseController
    {
        //
        // GET: /ShoppingCart/


        public ActionResult ShoppingCartView()
        {
            InitialiseCart();
            ViewBag.ShoppingCart = new JavaScriptSerializer().Serialize(Session["Cart"] as List<CartItem>);
            return View();
        }

        public ActionResult UpdateCart(string code, int quantity)
        {
            var cart = Session["Cart"] as List<CartItem>;
            var item = cart.SingleOrDefault(x => x.Code == code);
            if (item != null)
            {
                if (quantity == 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }

            }
            Session["Cart"] = cart;
            return Json(cart, JsonRequestBehavior.AllowGet);
        }


        public ActionResult DeleteCartItem(string code)
        {
            var cart = Session["Cart"] as List<CartItem>;
            var item = cart.SingleOrDefault(x => x.Code == code);
            if (item != null)
            {
                cart.Remove(item);
            }
            Session["Cart"] = cart;
            return Json(cart, JsonRequestBehavior.AllowGet);
        }


        public ActionResult AddToCart(string code, int quantity)
        {
            InitialiseCart();
            var cart = Session["Cart"] as List<CartItem>;
            Product product = null;
            var item = cart.SingleOrDefault(x => x.Code == code);
            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                product = ProductManagement.GetProduct(code);
                cart.Add(new CartItem
                {
                    Quantity = quantity,
                    Name = product.Name,
                    Code = product.Code,
                    UnitPrice = product.Price
                });
                Session["Cart"] = cart;
            }
            return Json(cart, JsonRequestBehavior.AllowGet);
        }

        private void InitialiseCart()
        {
            var cart = Session["Cart"];
            if (cart == null)
            {
                Session["Cart"] = new List<CartItem>();
            }
        }
    }
}
