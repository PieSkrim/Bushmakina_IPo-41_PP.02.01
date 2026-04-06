namespace MealDelivery.Models.DTOs.Requests
{
    public class MealCreateRequest : MealRequest
    {
        public string? Base64Image { get; set; } // Добавили ?
    }
}