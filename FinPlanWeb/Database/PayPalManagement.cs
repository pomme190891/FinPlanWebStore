using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using FinPlanWeb.Models;
using PayPal.Api.Payments;
using PayPal.PayPalAPIInterfaceService;
using PayPal.PayPalAPIInterfaceService.Model;

namespace FinPlanWeb.Database
{
    public class PayPalManagement
    {

        public HttpResponseBase HttpResponse { get; set; }
        public Checkout Checkout { get; set; }
        public string CancelUrl { get; set; }
        public string CheckoutReturnUrl { get; set; }
        public List<CartItem> Cart { get; set; }


        public void SetQuickCheckOut()
        {
            var request = new SetExpressCheckoutRequestType();
            PopulateSetCheckoutRequestObject(request);
            var wrapper = new SetExpressCheckoutReq();
            wrapper.SetExpressCheckoutRequest = request;
            var service = new PayPalAPIInterfaceServiceService();
            var response = service.SetExpressCheckout(wrapper);
            if (response.Ack.Equals(AckCodeType.FAILURE) || (response.Errors != null && response.Errors.Count > 0))
            {
                throw new InvalidOperationException(string.Join("\n", response.Errors.Select(x=>x.LongMessage)));
            }
            var paypalRedirectUrl = string.Format(ConfigurationManager.AppSettings["PayPalRedirectUrl"] + "_express-checkout&token={0}", response.Token);
            HttpResponse.Redirect(paypalRedirectUrl);
        }


        private void PopulateSetCheckoutRequestObject(SetExpressCheckoutRequestType request)
        {
            var ecDetails = new SetExpressCheckoutRequestDetailsType();
            ecDetails.SolutionType = SolutionTypeType.SOLE;
            ecDetails.NoShipping = "1";
            ecDetails.BuyerEmail = Checkout.BillingInfo.Email;
            ecDetails.AddressOverride = "0";
            ecDetails.BillingAddress = new AddressType
            {
                Name = Checkout.BillingInfo.FirstName + " " + Checkout.BillingInfo.SurName,
                Street1 = Checkout.BillingInfo.FirmName + " " + Checkout.BillingInfo.BuildingName + " " + Checkout.BillingInfo.StreetName,
                CityName = Checkout.BillingInfo.City,
                StateOrProvince = Checkout.BillingInfo.County,
                PostalCode = Checkout.BillingInfo.PostCode
            };
            ecDetails.BrandName = "Bluecoat Software";
            PopulatePaymentDetails(ecDetails);
            ecDetails.ReturnURL = CheckoutReturnUrl; ecDetails.CancelURL = CancelUrl; request.SetExpressCheckoutRequestDetails = ecDetails;
        }


        private void PopulatePaymentDetails(SetExpressCheckoutRequestDetailsType ecDetails)
        {

            var paymentDetails = new PaymentDetailsType();
            var itemTotal = 0.0;
            var currency = CurrencyCodeType.GBP;
            var shipAddress = new AddressType
            {
                Name = Checkout.BillingInfo.FirstName + " " + Checkout.BillingInfo.SurName,
                Street1 = Checkout.BillingInfo.FirmName + " " + Checkout.BillingInfo.BuildingName + " " + Checkout.BillingInfo.StreetName,
                CityName = Checkout.BillingInfo.City,
                StateOrProvince = Checkout.BillingInfo.County,
                PostalCode = Checkout.BillingInfo.PostCode
            };
            paymentDetails.ShipToAddress = shipAddress;

            foreach (var item in Cart)
            {
                var itemDetails = new PaymentDetailsItemType();
                itemDetails.Name = string.Format("{0}", item.Name);
                itemDetails.Quantity = item.Quantity;
                itemDetails.Amount = new BasicAmountType(currency, item.UnitPriceInStr);
                itemTotal += item.TotalPrice;
                paymentDetails.PaymentDetailsItem.Add(itemDetails);
            }

            var tax = itemTotal*20/100;
            paymentDetails.ItemTotal = new BasicAmountType(currency, itemTotal.ToString());
            paymentDetails.OrderTotal = new BasicAmountType(currency, (itemTotal + tax).ToString());
            paymentDetails.TaxTotal = new BasicAmountType(currency, (itemTotal * 20/100).ToString());
            ecDetails.PaymentDetails.Add(paymentDetails);
        }

        public DoExpressCheckoutPaymentResponseType DoExpressCheckout(HttpResponseBase response, string token)
        {
            var getCheckoutRequest = new GetExpressCheckoutDetailsRequestType();
            getCheckoutRequest.Token = token;
            var getCheckoutwrapper = new GetExpressCheckoutDetailsReq();
            getCheckoutwrapper.GetExpressCheckoutDetailsRequest = getCheckoutRequest;
            var service = new PayPalAPIInterfaceServiceService();
            var ecResponse = service.GetExpressCheckoutDetails(getCheckoutwrapper);
            var doECRequest = new DoExpressCheckoutPaymentRequestType(); 
            var ecRequestDetails = new DoExpressCheckoutPaymentRequestDetailsType(); 
            doECRequest.DoExpressCheckoutPaymentRequestDetails = ecRequestDetails; 
            ecRequestDetails.PaymentDetails = ecResponse.GetExpressCheckoutDetailsResponseDetails.PaymentDetails; 
            ecRequestDetails.Token = token; 
            ecRequestDetails.PayerID = ecResponse.GetExpressCheckoutDetailsResponseDetails.PayerInfo.PayerID; 
            ecRequestDetails.PaymentAction = PaymentActionCodeType.SALE; 
            var wrapper = new DoExpressCheckoutPaymentReq(); 
            wrapper.DoExpressCheckoutPaymentRequest = doECRequest; 
            return service.DoExpressCheckoutPayment(wrapper);
        }




    }
}