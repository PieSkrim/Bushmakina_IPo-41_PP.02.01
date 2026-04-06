using System.ComponentModel.DataAnnotations;

namespace MealDelivery.Models.DTOs.Requests
{
    public class OrderStatusUpdateRequest
    {
        [Required, StringLength(20)]
        public string Status { get; set; }
    }
}