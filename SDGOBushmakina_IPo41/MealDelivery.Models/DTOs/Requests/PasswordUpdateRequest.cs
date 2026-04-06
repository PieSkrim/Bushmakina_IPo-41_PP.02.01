using System.ComponentModel.DataAnnotations;

namespace MealDelivery.Models.DTOs.Requests
{
    public class PasswordUpdateRequest
    {
        [Required, StringLength(100)]
        public string CurrentPassword { get; set; }

        [Required, StringLength(100)]
        public string NewPassword { get; set; }
    }
}