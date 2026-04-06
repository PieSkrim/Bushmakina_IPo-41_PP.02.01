using System.ComponentModel.DataAnnotations;

namespace MealDelivery.Models.DTOs.Requests
{
    public class UserUpdateRequest
    {
        [StringLength(150)]
        public string FullName { get; set; }

        [EmailAddress, StringLength(100)]
        public string Email { get; set; }

        [Phone, StringLength(50)]
        public string Phone { get; set; }

        public bool? IsActive { get; set; }

        [StringLength(50)]
        public string RoleName { get; set; }
    }
}