using System.ComponentModel.DataAnnotations;

namespace MealDelivery.Models.DTOs.Requests
{
    public class UserCreateRequest
    {
        [Required, StringLength(50)]
        public string Login { get; set; }

        [Required, StringLength(100)]
        public string Password { get; set; }

        [Required, StringLength(50)]
        public string RoleName { get; set; }

        [Required, StringLength(150)]
        public string FullName { get; set; }

        [EmailAddress, StringLength(100)]
        public string Email { get; set; }

        [Phone, StringLength(50)]
        public string Phone { get; set; }
    }
}