using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealDelivery.Models.Enums;

namespace MealDelivery.Models.Entities
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required, Column(TypeName = "numeric(12,2)"), Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required, StringLength(500)]
        public string DeliveryAddress { get; set; }

        public DateTime? DeliveryTime { get; set; }

        [StringLength(50)]
        public string PaymentMethod { get; set; }

        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}