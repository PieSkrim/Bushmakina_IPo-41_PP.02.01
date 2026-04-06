using System;
using MealDelivery.Models.Enums;

namespace MealDelivery.Models.DTOs.Requests
{
    public class OrderFilterRequest
    {
        public OrderStatus? Status { get; set; }
        public int? UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SortBy { get; set; } = "orderDate";
        public bool Ascending { get; set; } = false;
    }
}