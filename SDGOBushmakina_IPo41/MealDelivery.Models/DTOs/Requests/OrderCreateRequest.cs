using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MealDelivery.Models.DTOs.Requests
{
    public class OrderCreateRequest
    {
        [Required]
        public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();

        [Required, StringLength(500)]
        public string DeliveryAddress { get; set; }

        public DateTime? DeliveryTime { get; set; }

        [StringLength(50)]
        public string PaymentMethod { get; set; }

        public string Notes { get; set; }
    }
}