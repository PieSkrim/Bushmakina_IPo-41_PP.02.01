using System.ComponentModel.DataAnnotations;

namespace MealDelivery.Models.DTOs.Requests
{
    public class LoginRequest
    {
        [Required, StringLength(50)]
        public string Login { get; set; }

        [Required, StringLength(100)]
        public string Password { get; set; }
    }
}