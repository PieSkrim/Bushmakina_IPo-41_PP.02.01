using System.ComponentModel.DataAnnotations;

namespace MealDelivery.Models.DTOs.Requests
{
    public class ActivationRequest
    {
        [Required]
        public bool IsActive { get; set; }
    }
}