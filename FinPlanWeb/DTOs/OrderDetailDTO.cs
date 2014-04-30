using System.Collections.Generic;

namespace FinPlanWeb.DTOs
{
    public class OrderDetailDTO
    {
        public OrderDetailDTO()
        {
            OrderItems = new List<OrderItemDTO>();
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TransactionDate { get; set; }
        public string Email { get; set; }
        public string PaymentType { get; set; }
        public string PaymentStatus { get; set; }
        public decimal Currency { get; set; }
        public decimal Gross { get; set; }
        public string BuyerUsername { get; set; }
        public string BuyerFirmName { get; set; }
        public string PaypalId { get; set; }
        public string DirectDebitId { get; set; }
        public string PromotionCodeId { get; set; }
        public List<OrderItemDTO> OrderItems { get; set; }
    }

    public class OrderItemDTO
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }
}