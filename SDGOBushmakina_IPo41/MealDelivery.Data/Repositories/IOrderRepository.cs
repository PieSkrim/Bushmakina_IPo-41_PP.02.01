using System.Collections.Generic;
using System.Threading.Tasks;
using MealDelivery.Models.Entities;
using MealDelivery.Models.DTOs.Responses;
using System;

namespace MealDelivery.Data.Repositories
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersWithDetailsAsync(string status = null);
        Task<Order> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId);
        Task AddOrderWithDetailsAsync(Order order, List<OrderDetail> orderDetails);
        Task<OrderStatistics> GetOrderStatisticsAsync();
    }
}