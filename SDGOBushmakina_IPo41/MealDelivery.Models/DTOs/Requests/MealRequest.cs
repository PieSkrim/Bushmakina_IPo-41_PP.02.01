using System;
using System.ComponentModel.DataAnnotations;

namespace MealDelivery.Models.DTOs.Requests
{
    public class MealRequest
    {
        [Required(ErrorMessage = "Название обязательно"), StringLength(255)]
        public string Name { get; set; }

        [Required, Range(1, int.MaxValue, ErrorMessage = "Выберите категорию")]
        public int CategoryId { get; set; }

        [Required, Range(1, int.MaxValue, ErrorMessage = "Выберите производителя")]
        public int ManufacturerId { get; set; }

        public int? SupplierId { get; set; }

        public string? Description { get; set; } // Добавили ?

        [Required, Range(0.01, 1000000, ErrorMessage = "Цена должна быть больше 0")]
        public decimal Price { get; set; }

        [Required, StringLength(50)]
        public string Unit { get; set; } = "порция";

        [Required, Range(0, int.MaxValue, ErrorMessage = "Количество не может быть отрицательным")]
        public int StockQuantity { get; set; }

        [Range(0, 100, ErrorMessage = "Скидка должна быть от 0 до 100")]
        public decimal Discount { get; set; } = 0;

        public string? ImagePath { get; set; } // Добавили ?

        public bool IsActive { get; set; } = true;
    }
}