namespace MealDelivery.Models.DTOs.Requests
{
    public class MealFilterRequest
    {
        public string SearchTerm { get; set; }
        public int? ManufacturerId { get; set; }
        public string SortBy { get; set; } = "name";
        public bool Ascending { get; set; } = true;
    }
}