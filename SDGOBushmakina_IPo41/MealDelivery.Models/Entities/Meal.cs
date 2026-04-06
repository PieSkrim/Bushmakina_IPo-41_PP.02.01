using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealDelivery.Models.Entities
{
    public class Meal
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(255)]
        public string Name { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        [Required]
        public int ManufacturerId { get; set; }

        [ForeignKey("ManufacturerId")]
        public Manufacturer Manufacturer { get; set; }

        public int? SupplierId { get; set; }

        [ForeignKey("SupplierId")]
        public Supplier Supplier { get; set; }

        public string Description { get; set; }

        [Required, Column(TypeName = "numeric(10,2)"), Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required, StringLength(50)]
        public string Unit { get; set; } = "порция";

        [Required, Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Range(0, 100)]
        public decimal Discount { get; set; } = 0.00m;

        public string ImagePath { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public decimal FinalPrice => CalculateFinalPrice();

        [NotMapped]
        public string FormattedPrice => $"{Price:N2} руб.";

        [NotMapped]
        public string FormattedDiscount => Discount > 0 ? $"{Discount}% скидка" : string.Empty;

        [NotMapped]
        public string StockStatus => StockQuantity > 0 ? $"{StockQuantity} {Unit} в наличии" : "Нет на складе";

        public decimal CalculateFinalPrice()
        {
            return Math.Round(Price * (1 - Discount / 100), 2);
        }
    }
}