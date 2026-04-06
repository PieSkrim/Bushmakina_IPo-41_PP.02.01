using System.Collections.Generic;

namespace MealDelivery.Models.DTOs.Responses
{
    public class OrderStatistics
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public Dictionary<string, int> OrdersByStatus { get; set; }
        public decimal AverageOrderAmount { get; set; }
        public int OrdersToday { get; set; }
    }
}