using System;

namespace MealDelivery.Models.DTOs.Responses
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string FullName { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UsersListResponse
    {
        public List<UserResponse> Users { get; set; } = new List<UserResponse>();
    }

    public class UserCreateResponse
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string FullName { get; set; }
        public string RoleName { get; set; }
        public string Email { get; set; }
    }
}