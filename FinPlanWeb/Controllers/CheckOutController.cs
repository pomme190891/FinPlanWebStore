using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Web.WebPages;
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

        public ActionResult ValidateCheckout(string checkout)
        {
            var serializer = new JavaScriptSerializer();
            var checkoutObj = serializer.Deserialize<Checkout>(checkout);
            var validationMessage = string.Join("<br/>",Validate(checkoutObj));
            if (!validationMessage.Any())
            {
                TempData["checkoutInfo"] = checkoutObj;
            }
            return Json(new { validationMessage, passed = !validationMessage.Any() });
        }

        public ActionResult OrderByDirectDebit()
        {
            var checkout = TempData["checkoutInfo"] as Checkout;
            var cart = Session["Cart"] as List<CartItem>;
            var user = Session["User"] as UserLoginDto;
            if (checkout == null)
            {
                throw new InvalidOperationException("You need to validate checkout before ordering by direct debit.");
            }
            
            OrderManagement.RecordDirectDebitTransaction(checkout, cart, user.Id );


            SendEmail(checkout);
            return View();
        }

        private void SendEmail(Checkout checkout, string orderNumber = "")
        {
            MailMessage mail = new MailMessage("you@yourcompany.com", checkout.BillingInfo.Email);
            SmtpClient client = new SmtpClient();
            client.Port = 25;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Host = "localhost";
            mail.Subject = "Order Confirmation:" + orderNumber;
            mail.Body = "this is my test email body";
            client.Send(mail);
        }

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


    public class CheckUserSession : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var userSessoion = filterContext.RequestContext.HttpContext.Session["User"];
            if (userSessoion == null)
            {
                filterContext.Result = new RedirectToRouteResult(
            new RouteValueDictionary {{ "Controller", "Account" },
                                      { "Action", "Login" } });
            }
            base.OnActionExecuting(filterContext);
        }
    }


}
