using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FinPlanWeb.Database;
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
                Checkout =  checkout,
                HttpResponse = Response,
                CancelUrl = Url.Action("PaymentCancellation",null,null,Request.Url.Scheme),
                CheckoutReturnUrl = Url.Action("PaymentReceived", null, null, Request.Url.Scheme),
                Cart = cart
            };
            TempData["checkoutData"] = checkout;
            paypal.SetQuickCheckOut();
        }

        //public ActionResult PaymentReceived()
        //{
        //    return View();
        //}

        public ActionResult PaymentCancellation()
        {
            return View();
        }

        public ActionResult PaymentReceived(string token, string payerId)
        {
            var checkout = TempData["checkoutInfo"] as Checkout;
            var cart = Session["Cart"] as List<CartItem>;
            var paypalProcess = new PayPalManagement
            {
                Checkout = checkout,
                HttpResponse = Response,
                CancelUrl = Url.Action("PaymentCancellation"),
                CheckoutReturnUrl = Url.Action("PaymentReceived"),
                Cart = cart
            };
            TempData["checkoutInfo"] = checkout;
            var response = paypalProcess.DoExpressCheckout(Response, token);
            if (response.Ack.Equals(AckCodeType.FAILURE) || (response.Errors != null && response.Errors.Count > 0))
            {
                return PaymentFailed(response.Errors.Select(x => x.LongMessage));
            } return PaymentResponseSuccess(response, checkout, payerId);
        }

        private ViewResult PaymentResponseSuccess(DoExpressCheckoutPaymentResponseType response,
            Checkout checkoutInfo, string payerId)
        {
            var errors = new List<string>(); 
            var paymentStatus = response.DoExpressCheckoutPaymentResponseDetails.PaymentInfo[0].PaymentStatus;
            if (paymentStatus == PaymentStatusCodeType.DENIED)
            {
                errors.Add("The payment has been denied by paypal. Please check with your account and try it again.");
                return PaymentFailed(errors);
            }
            if (paymentStatus == PaymentStatusCodeType.VOIDED || paymentStatus == PaymentStatusCodeType.EXPIRED)
            {
                errors.Add("The payment has been voided or expired by paypal. Please try again."); 
                return PaymentFailed(errors);
            }
            if (paymentStatus == PaymentStatusCodeType.COMPLETED || paymentStatus == PaymentStatusCodeType.PENDING ||
                paymentStatus == PaymentStatusCodeType.COMPLETEDFUNDSHELD)
            {
                return PaymentSuccess(response, checkoutInfo, payerId);
            }
            if (paymentStatus == PaymentStatusCodeType.FAILED)
            {
                errors.Add("The payment has been voided or expired by paypal. Please try again.");
                return PaymentFailed(errors);
            }
            throw new InvalidOperationException("Invalid payment operation.");
        }

        private ViewResult PaymentFailed(IEnumerable<string> errors)
        {
            return View("PaymentFailed",new TransacDetails { Errors = errors });
        }

        private ViewResult PaymentSuccess(DoExpressCheckoutPaymentResponseType response,
            Checkout checkoutSummary, string payerId)
        {
            var details = response.DoExpressCheckoutPaymentResponseDetails;
            var transaction = new TransacDetails
            {
                TransId = details.PaymentInfo[0].TransactionID
            };
            //SaveTransaction(checkoutInfo, response, payerId);
            return View("PaymentSuccess", transaction);
        }

    }

    public class TransacDetails
    {
        public IEnumerable<string> Errors { get; set; }
        public string TransId { get; set; }
    }
}
