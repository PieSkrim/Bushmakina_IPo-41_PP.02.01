using Microsoft.EntityFrameworkCore;
using MealDelivery.Models.Entities;
using MealDelivery.Data.Context;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MealDelivery.Models.Enums;
using System;
using MealDelivery.Models.DTOs.Responses;

namespace MealDelivery.Data.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly MealDeliveryDbContext _context;

        public OrderRepository(MealDeliveryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task AddAsync(Order entity)
        {
            await _context.Orders.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Order entity)
        {
            _context.Orders.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var order = await GetByIdAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Orders.AnyAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> FindAsync(Expression<Func<Order, bool>> predicate)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersWithDetailsAsync(string status = null)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Meal)
                .ThenInclude(m => m.Category)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Meal)
                .ThenInclude(m => m.Manufacturer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
                {
                    query = query.Where(o => o.Status == orderStatus);
                }
            }

            return await query
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Meal)
                .ThenInclude(m => m.Category)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Meal)
                .ThenInclude(m => m.Manufacturer)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task AddOrderWithDetailsAsync(Order order, List<OrderDetail> orderDetails)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();

                foreach (var detail in orderDetails)
                {
                    detail.OrderId = order.Id;
                }

                await _context.OrderDetails.AddRangeAsync(orderDetails);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderStatistics> GetOrderStatisticsAsync()
        {
            var now = DateTime.UtcNow;
            var todayStart = now.Date;

            var totalOrders = await _context.Orders.CountAsync();
            var totalRevenue = await _context.Orders.SumAsync(o => o.TotalAmount);
            var averageOrderAmount = totalOrders > 0 ? totalRevenue / totalOrders : 0;
            var ordersToday = await _context.Orders.CountAsync(o => o.OrderDate >= todayStart);

            var ordersByStatus = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status.ToString(), x => x.Count);

            return new OrderStatistics
            {
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                AverageOrderAmount = averageOrderAmount,
                OrdersToday = ordersToday,
                OrdersByStatus = ordersByStatus
            };
        }
    }
}