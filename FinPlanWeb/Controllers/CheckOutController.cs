using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using FinPlanWeb.Database;
using FinPlanWeb.DTOs;
using FinPlanWeb.Models;

namespace FinPlanWeb.Controllers
{
    public class CheckOutController : BaseController
    {
        //
        // GET: /CheckOut/

        [CheckUserSession]
        public ActionResult CheckOut()
        {
            var cart = Session["Cart"] as List<CartItem>;
            ViewBag.Cart = new JavaScriptSerializer().Serialize(cart);
            ViewBag.Checkout = new JavaScriptSerializer().Serialize(new Checkout());
            return View();
        }

        /// <summary>
        /// Get Promotion Info and return in Json String
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetPromotionInfo(string id)
        {
            var promotion = PromoManagement.GetPromotion(id);
            return Json(new { isValid = promotion != null, promotion });
        }


        /// <summary>
        /// Checkout Validation Method, also store checkout info as a session.
        /// Provides serialization and deserialization functionality for AJAX-enabled applications.
        ///  This method serializes an object and converts it to a JSON string.
        ///  Deserialization from JSON string to any object type
        /// </summary>
        /// <param name="checkout"></param>
        /// <returns></returns>
        public ActionResult ValidateCheckout(string checkout)
        {
            var serializer = new JavaScriptSerializer();
            var checkoutObj = serializer.Deserialize<Checkout>(checkout);
            var validationMessage = string.Join("<br/>", Validate(checkoutObj));
            if (!validationMessage.Any())
            {
                TempData["checkoutInfo"] = checkoutObj;
            }
            return Json(new { validationMessage, passed = !validationMessage.Any() });
        }

        /// <summary>
        /// Direct Debit Order
        /// </summary>
        /// <returns></returns>
        public ActionResult OrderByDirectDebit()
        {
            var checkout = TempData["checkoutInfo"] as Checkout;
            var cart = Session["Cart"] as List<CartItem>;
            var user = Session["User"] as UserLoginDto;
            if (checkout == null)
            {
                throw new InvalidOperationException("You need to validate checkout before ordering by direct debit.");
            }

            int orderId;
            OrderManagement.RecordDirectDebitTransaction(checkout, cart, user.Id, out orderId);

            SendEmail(checkout, cart, orderId);
            return View();
        }


        /// <summary>
        /// Get the system to send email once Direct Debit has been placed.
        /// </summary>
        /// <param name="checkout"></param>
        /// <param name="cart"></param>
        /// <param name="orderNumber"></param>

        private void SendEmail(Checkout checkout, List<CartItem> cart, int orderNumber)

        {
            var mail = new MailMessage("you@yourcompany.com", checkout.BillingInfo.Email);
            var client = new SmtpClient();
            var bodyText = "<html>" +
                           "<head>" +
                               "<style> " +
                                "table{border-collapse:collapse;} " +
                                "table, td, th{border:1px solid black;} " +
                               "</style>" +
                           "</head>" +
                           "<body>" +
                           "<h2>Your order has been confirmed. The order number is : +" + orderNumber + "</h2>" +
                           "<p>Below are a list of items that you have purchased:</p></br></br>" +
                           "<table>" +
                               "<thead>" +
                                   "<tr>" +
                                       "<td>Product Code</td>" +
                                       "<td>Product Name</td>" +
                                       "<td>Quantity</td>" +
                                   "</tr>" +
                               "</thead>" +
                               "<tbody>" +
                               GenerateInnerOrderItem(cart) +
                               "</tbody>" +
                           "</table>" +
                           "</body>" +
                           "</html>";
            client.Port = 25;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Host = "localhost";
            mail.Subject = "Order Confirmation:" + orderNumber;
            mail.IsBodyHtml = true;
            mail.Body = bodyText;
            client.Send(mail);
        }

        /// <summary>
        /// Generate List of Cart Items to be displayed on the email.
        /// </summary>
        /// <param name="cart"></param>
        /// <returns></returns>
        private string GenerateInnerOrderItem(IEnumerable<CartItem> cart)
        {
            var stringBuilder = new StringBuilder();
            foreach (var item in cart)
            {
                stringBuilder.Append(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", item.Code, item.Name,
                                                   item.Quantity));
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Checkout Validation
        /// </summary>
        /// <param name="checkout"></param>
        /// <returns></returns>
        private IEnumerable<string> Validate(Checkout checkout)
        {
            var validationMessage = new List<string>();

            if (string.IsNullOrEmpty(checkout.BillingInfo.FirstName))
            {
                validationMessage.Add("Firstname cannot be emptied.");
            }

            if (string.IsNullOrEmpty(checkout.BillingInfo.SurName))
            {
                validationMessage.Add("Surname cannot be emptied.");
            }

            if (string.IsNullOrEmpty(checkout.BillingInfo.FirmName))
            {
                validationMessage.Add("First Name is empty.");
            }

            if (string.IsNullOrEmpty(checkout.BillingInfo.BuildingName))
            {
                validationMessage.Add("Building No and Name cannot be emptied.");
            }

            if (string.IsNullOrEmpty(checkout.BillingInfo.StreetName))
            {
                validationMessage.Add("Street Name cannot be emptied.");
            }
            if (string.IsNullOrEmpty(checkout.BillingInfo.City))
            {
                validationMessage.Add("City cannot be emptied.");
            }
            if (string.IsNullOrEmpty(checkout.BillingInfo.County))
            {
                validationMessage.Add("County cannot be emptied.");
            }
            if (string.IsNullOrEmpty(checkout.BillingInfo.PostCode))
            {
                validationMessage.Add("Postcode cannot be emptied.");
            }
            else
            {
                var regex =
                   new Regex(
                       @"(GIR 0AA)|((([A-Z-[QVX]][0-9][0-9]?)|(([A-Z-[QVX]][A-Z-[IJZ]][0-9][0-9]?)|(([A-Z-[QVX]][0-9][A-HJKSTUW])|([A-Z-[QVX]][A-Z-[IJZ]][0-9][ABEHMNPRVWXY])))) [0-9][A-Z-[CIKMOV]]{2})");
                var match = regex.Match(checkout.BillingInfo.PostCode);
                if (!match.Success)
                {
                    validationMessage.Add("The UK postcode entered is not valid.");
                }
            }


            if (string.IsNullOrEmpty(checkout.BillingInfo.TelephoneNo))
            {
                validationMessage.Add("Street Name cannot be emptied.");
            }
            else
            {
                var regex = new Regex(@"^(?:(?:\(?(?:0(?:0|11)\)?[\s-]?\(?|\+)44\)?[\s-]?(?:\(?0\)?[\s-]?)?)|(?:\(?0))(?:(?:\d{5}\)?[\s-]?\d{4,5})|(?:\d{4}\)?[\s-]?(?:\d{5}|\d{3}[\s-]?\d{3}))|(?:\d{3}\)?[\s-]?\d{3}[\s-]?\d{3,4})|(?:\d{2}\)?[\s-]?\d{4}[\s-]?\d{4}))(?:[\s-]?(?:x|ext\.?|\#)\d{3,4})?$ 
");
                var match = regex.Match(checkout.BillingInfo.TelephoneNo);
                if (!match.Success)
                {
                    validationMessage.Add("");
                }
            }


            if (string.IsNullOrEmpty(checkout.BillingInfo.Email))
            {
                validationMessage.Add("Email is empty.");
            }
            else
            {

                var regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                var match = regex.Match(checkout.BillingInfo.Email);
                if (!match.Success)
                    validationMessage.Add("Invalid email format.");
            }

            if (!checkout.PaymentInfo.IsDirectDebit && !checkout.PaymentInfo.IsPayPal)
            {
                validationMessage.Add("Payment method must be set.");
            }

            return validationMessage;
        }
    }

    /// <summary>
    /// Called by the ASP.NET MVC framework before the action method executes.
    /// Store user session. Use this before running the ActionResult CheckOut()
    /// </summary>
    public class CheckUserSession : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext) 
        {
            var userSessoion = filterContext.RequestContext.HttpContext.Session["User"]; //get user session
            if (userSessoion == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary {{ "Controller", "Account" },
                                      { "Action", "Login" } });
                var controller = filterContext.Controller as Controller;
                if (controller != null)
                {
                    if (controller.Request.Url != null)
                        controller.TempData.Add("ReturnUrl", controller.Request.Url.AbsoluteUri); //temp data for return url
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }


}
