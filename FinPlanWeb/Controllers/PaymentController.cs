using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FinPlanWeb.Database;
using FinPlanWeb.DTOs;
using FinPlanWeb.Models;
using PayPal.PayPalAPIInterfaceService.Model;

namespace FinPlanWeb.Controllers
{
    public class PaymentController : BaseController
    {
        //
        // GET: /Payment/

        public void Index()
        {
            var checkout = TempData["checkoutInfo"] as Checkout;
            var cart = Session["Cart"] as List<CartItem>;
            var paypal = new PayPalManagement
            {
                Checkout = checkout,
                WebResponse = Response,
                CancelUrl = Url.Action("PaymentCancellation", null, null, Request.Url.Scheme),
                CheckoutReturnUrl = Url.Action("PaymentReceived", null, null, Request.Url.Scheme),
                Cart = cart
            };
            TempData["checkoutInfo"] = checkout;
            paypal.SetQuickCheckOut();
        }



        public ActionResult PaymentCancellation()
        {
            return View();
        }

        public ActionResult PaymentReceived(string token, string payerId)
        {
            var checkout = TempData["checkoutInfo"] as Checkout;
            var cart = Session["Cart"] as List<CartItem>;
            var payPalManagement = new PayPalManagement
            {
                Checkout = checkout,
                WebResponse = Response,
                CancelUrl = Url.Action("PaymentCancellation"),
                CheckoutReturnUrl = Url.Action("PaymentReceived"),
                Cart = cart
            };
            TempData["checkoutInfo"] = checkout;
            var response = payPalManagement.DoExpressCheckout(Response, token);
            if (response.Ack.Equals(AckCodeType.FAILURE) || (response.Errors != null && response.Errors.Count > 0))
            {
                return PaymentFailed(response.Errors.Select(x => x.LongMessage));
            }

            return PaymentResponseSuccess(response, checkout, cart);
        }

        private ViewResult PaymentResponseSuccess(DoExpressCheckoutPaymentResponseType response, Checkout checkout, List<CartItem> cart)
        {
            var failure = new List<string>();
            var paymentStatus = response.DoExpressCheckoutPaymentResponseDetails.PaymentInfo[0].PaymentStatus;
            if (paymentStatus == PaymentStatusCodeType.DENIED)
            {
                //change
                failure.Add("Unsuccessful.");
                return PaymentFailed(failure);
            }
            if (paymentStatus == PaymentStatusCodeType.VOIDED || paymentStatus == PaymentStatusCodeType.EXPIRED)
            {
                //changes
                failure.Add("Expired");
                return PaymentFailed(failure);
            }
            if (paymentStatus == PaymentStatusCodeType.COMPLETED || paymentStatus == PaymentStatusCodeType.PENDING ||
                paymentStatus == PaymentStatusCodeType.COMPLETEDFUNDSHELD)
            {
                return PaymentSuccess(response, checkout, cart);
            }
            if (paymentStatus == PaymentStatusCodeType.FAILED)
            {
                failure.Add("Failed");
                return PaymentFailed(failure);
            }
            throw new InvalidOperationException("Invalid payment operation.");
        }

        private ViewResult PaymentFailed(IEnumerable<string> errors)
        {
            return View("PaymentFailed", new TransacDetails { Errors = errors });
        }

        private ViewResult PaymentSuccess(DoExpressCheckoutPaymentResponseType response, Checkout checkout, List<CartItem> cart)
        {
            var details = response.DoExpressCheckoutPaymentResponseDetails;
            var transaction = new TransacDetails
            {
                TransId = details.PaymentInfo[0].TransactionID
            };
            var user = Session["User"] as UserLoginDto;
            OrderManagement.RecordPayPalTransaction(checkout, cart, details.PaymentInfo[0].TransactionID, user.Id);
            return View("PaymentSuccess", transaction);
        }

    }

    public class TransacDetails
    {
        public IEnumerable<string> Errors { get; set; }
        public string TransId { get; set; }
    }
}
