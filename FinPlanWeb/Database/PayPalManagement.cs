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

        public HttpResponseBase WebResponse { get; set; }
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
            WebResponse.Redirect(paypalRedirectUrl);
        }


        private void PopulateSetCheckoutRequestObject(SetExpressCheckoutRequestType request)
        {
            var requestedDetails = new SetExpressCheckoutRequestDetailsType();
            requestedDetails.SolutionType = SolutionTypeType.SOLE;
            requestedDetails.NoShipping = "1";
            requestedDetails.BuyerEmail = Checkout.BillingInfo.Email;
            requestedDetails.AddressOverride = "0";
            requestedDetails.BillingAddress = new AddressType
            {
                Name = Checkout.BillingInfo.FirstName + " " + Checkout.BillingInfo.SurName,
                Street1 = Checkout.BillingInfo.FirmName + " " + Checkout.BillingInfo.BuildingName + " " + Checkout.BillingInfo.StreetName,
                CityName = Checkout.BillingInfo.City,
                StateOrProvince = Checkout.BillingInfo.County,
                PostalCode = Checkout.BillingInfo.PostCode
            };
            requestedDetails.BrandName = "Bluecoat Software";
            PopulatePaymentDetails(requestedDetails);
            requestedDetails.ReturnURL = CheckoutReturnUrl; requestedDetails.CancelURL = CancelUrl; request.SetExpressCheckoutRequestDetails = requestedDetails;
        }


        private void PopulatePaymentDetails(SetExpressCheckoutRequestDetailsType ecDetails)
        {

            var paymentInfo = new PaymentDetailsType();
            var total = 0.0;
            var currency = CurrencyCodeType.GBP;
            var address = new AddressType
            {
                Name = Checkout.BillingInfo.FirstName + " " + Checkout.BillingInfo.SurName,
                Street1 = Checkout.BillingInfo.FirmName + " " + Checkout.BillingInfo.BuildingName + " " + Checkout.BillingInfo.StreetName,
                CityName = Checkout.BillingInfo.City,
                StateOrProvince = Checkout.BillingInfo.County,
                PostalCode = Checkout.BillingInfo.PostCode
            };
            paymentInfo.ShipToAddress = address;

            foreach (var item in Cart)
            {
                var itemInformation = new PaymentDetailsItemType();
                itemInformation.Name = string.Format("{0}", item.Name);
                itemInformation.Quantity = item.Quantity;
                itemInformation.Amount = new BasicAmountType(currency, item.UnitPriceInStr);
                total += item.TotalPrice;
                paymentInfo.PaymentDetailsItem.Add(itemInformation);
            }

            var tax = total*20/100;
            paymentInfo.ItemTotal = new BasicAmountType(currency, total.ToString());
            paymentInfo.OrderTotal = new BasicAmountType(currency, (total + tax).ToString());
            paymentInfo.TaxTotal = new BasicAmountType(currency, (total * 20/100).ToString());
            ecDetails.PaymentDetails.Add(paymentInfo);
        }

        public DoExpressCheckoutPaymentResponseType DoExpressCheckout(HttpResponseBase response, string token)
        {
            var getCheckoutRequest = new GetExpressCheckoutDetailsRequestType();
            getCheckoutRequest.Token = token;
            var getCheckOutInfo = new GetExpressCheckoutDetailsReq();
            getCheckOutInfo.GetExpressCheckoutDetailsRequest = getCheckoutRequest;
            var service = new PayPalAPIInterfaceServiceService();
            var getResponse = service.GetExpressCheckoutDetails(getCheckOutInfo);
            var doRequest = new DoExpressCheckoutPaymentRequestType(); 
            var requestInfo = new DoExpressCheckoutPaymentRequestDetailsType(); 
            doRequest.DoExpressCheckoutPaymentRequestDetails = requestInfo; 
            requestInfo.PaymentDetails = getResponse.GetExpressCheckoutDetailsResponseDetails.PaymentDetails; 
            requestInfo.Token = token; 
            requestInfo.PayerID = getResponse.GetExpressCheckoutDetailsResponseDetails.PayerInfo.PayerID; 
            requestInfo.PaymentAction = PaymentActionCodeType.SALE; 
            var wrapper = new DoExpressCheckoutPaymentReq(); 
            wrapper.DoExpressCheckoutPaymentRequest = doRequest; 
            return service.DoExpressCheckoutPayment(wrapper);
        }




    }
}