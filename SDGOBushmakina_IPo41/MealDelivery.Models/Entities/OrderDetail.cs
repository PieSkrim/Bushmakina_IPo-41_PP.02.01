using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealDelivery.Models.Entities
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        [Required]
        public int MealId { get; set; }

        [ForeignKey("MealId")]
        public Meal Meal { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required, Column(TypeName = "numeric(10,2)"), Range(0, double.MaxValue)]
        public decimal PriceAtPurchase { get; set; }

        [Required, Range(0, 100)]
        public decimal DiscountAtPurchase { get; set; } = 0.00m;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public decimal FinalPrice => Math.Round(PriceAtPurchase * (1 - DiscountAtPurchase / 100) * Quantity, 2);
    }
}