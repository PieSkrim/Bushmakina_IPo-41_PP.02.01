using System.ComponentModel.DataAnnotations;

namespace MealDelivery.Models.DTOs.Requests
{
    public class OrderItemRequest
    {
        [Required, Range(1, int.MaxValue)]
        public int MealId { get; set; }

        [Required, Range(1, 100)]
        public int Quantity { get; set; }
    }
}