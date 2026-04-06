using System;

namespace MealDelivery.Models.DTOs.Responses
{
    public class MealResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int ManufacturerId { get; set; }
        public string ManufacturerName { get; set; }
        public int? SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; }
        public int StockQuantity { get; set; }
        public decimal Discount { get; set; }
        public string ImagePath { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public decimal FinalPrice { get; set; }
        public string FormattedPrice { get; set; }
        public string FormattedDiscount { get; set; }
        public string StockStatus { get; set; }
        public string HighlightColor { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}