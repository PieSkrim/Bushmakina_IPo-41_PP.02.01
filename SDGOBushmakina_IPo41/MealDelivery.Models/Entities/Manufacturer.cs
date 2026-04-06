using System;
using System.ComponentModel.DataAnnotations;

namespace MealDelivery.Models.Entities
{
    public class Manufacturer
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; }

        public string Address { get; set; }

        [StringLength(50)]
        public string Phone { get; set; }

        [StringLength(100), EmailAddress]
        public string Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}