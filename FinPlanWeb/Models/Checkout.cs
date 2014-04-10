namespace FinPlanWeb.Models
{
    public class Checkout
    {
        public Checkout()
        {
            BillingInfo = new BillingInfo();
            PaymentInfo = new PaymentInfo();
        }

        public BillingInfo BillingInfo { get; set; }
        public PaymentInfo PaymentInfo { get; set; }

    }

    public class PaymentInfo
    {
        public bool IsPayPal { get; set; }
        public bool IsDirectDebit { get; set; }
    }

    public enum PaymentType
    {
        DirectDebit = 1,
        Paypal
    }

    public class BillingInfo
    {
        public string FirstName { get; set; }
        public string SurName { get; set; }
        public string Email { get; set; }
        public string FirmName { get; set; }
        public string BuildingName { get; set; }
        public string StreetName { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string PostCode { get; set; }
        public string TelephoneNo { get; set; }
        
    }


}