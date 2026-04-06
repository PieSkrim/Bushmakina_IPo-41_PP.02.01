using System;
using System.Collections.Generic;
using MealDelivery.Models.Enums;

namespace MealDelivery.Models.DTOs.Responses
{
    public class OrderResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusText { get; set; }
        public string DeliveryAddress { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public string PaymentMethod { get; set; }
        public string Notes { get; set; }
        public List<OrderItemResponse> Items { get; set; } = new List<OrderItemResponse>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class OrderItemResponse
    {
        public int MealId { get; set; }
        public string MealName { get; set; }
        public string CategoryName { get; set; }
        public string ManufacturerName { get; set; }
        public string ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
        public decimal DiscountAtPurchase { get; set; }
        public decimal FinalPrice { get; set; }
    }
}