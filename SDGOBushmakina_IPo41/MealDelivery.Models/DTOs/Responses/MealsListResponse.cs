using System.Collections.Generic;

namespace MealDelivery.Models.DTOs.Responses
{
    public class MealsListResponse
    {
        public List<MealResponse> Meals { get; set; } = new List<MealResponse>();
        public List<FilterOption> Manufacturers { get; set; } = new List<FilterOption>();
        public string UserRole { get; set; }
        public bool CanEdit { get; set; }
        public bool CanFilter { get; set; }
    }

    public class FilterOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}