using System.Collections.Generic;

namespace MealDelivery.Models.DTOs.Responses
{
    public class UserOrdersResponse
    {
        public List<OrderSummaryResponse> Orders { get; set; } = new List<OrderSummaryResponse>();
    }

    public class OrderSummaryResponse
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string StatusText { get; set; }
        public string DeliveryAddress { get; set; }
        public int ItemsCount { get; set; }
        public int TotalItems { get; set; }
    }
}