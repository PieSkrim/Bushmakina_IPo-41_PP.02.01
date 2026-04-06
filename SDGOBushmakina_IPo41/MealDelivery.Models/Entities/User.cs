using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealDelivery.Models.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Login { get; set; }

        [Required, StringLength(255)]
        public string PasswordHash { get; set; }

        [Required]
        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public Role Role { get; set; }

        [Required, StringLength(150)]
        public string FullName { get; set; }

        [StringLength(100), EmailAddress]
        public string? Email { get; set; } // Добавлен знак ? (nullable)

        [StringLength(50), Phone]
        public string? Phone { get; set; } // Добавлен знак ? (nullable)

        public bool IsActive { get; set; } = true;

        public DateTime? LastLogin { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}