namespace MealDelivery.Models.Enums
{
    public enum OrderStatus
    {
        Pending,      // Ожидает обработки
        Preparing,    // Готовится
        Dispatched,   // Передан курьеру
        Delivered,    // Доставлен
        Cancelled     // Отменён
    }
}