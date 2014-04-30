using System;

namespace FinPlanWeb.DTOs
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TransactionDate { get; set; }
        public string PaymentType { get; set; }
        public decimal Currency { get; set; }
        public string BuyerUsername { get; set; }
        public string BuyerFirmName { get; set; }
    }
}