using Microsoft.AspNetCore.Mvc;
using MealDelivery.Data.Repositories;
using MealDelivery.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MealDelivery.Models.DTOs.Requests;
using MealDelivery.Models.DTOs.Responses;
using MealDelivery.Models.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MealDelivery.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMealRepository _mealRepository;
        private readonly IUserRepository _userRepository;

        public OrdersController(
            IOrderRepository orderRepository,
            IMealRepository mealRepository,
            IUserRepository userRepository)
        {
            _orderRepository = orderRepository;
            _mealRepository = mealRepository;
            _userRepository = userRepository;
        }

        [HttpGet]
        [Authorize(Roles = "manager, admin")]
        public async Task<IActionResult> GetOrders([FromQuery] string? status = null)
        {
            var orders = await _orderRepository.GetOrdersWithDetailsAsync(status);
            var result = orders.Select(o => new OrderResponse
            {
                Id = o.Id,
                UserId = o.UserId,
                UserName = o.User.FullName,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                StatusText = GetStatusText(o.Status),
                DeliveryAddress = o.DeliveryAddress,
                DeliveryTime = o.DeliveryTime,
                PaymentMethod = o.PaymentMethod,
                Notes = o.Notes,
                Items = o.OrderDetails.Select(od => new OrderItemResponse
                {
                    MealId = od.MealId,
                    MealName = od.Meal.Name,
                    CategoryName = od.Meal.Category.Name,
                    ManufacturerName = od.Meal.Manufacturer.Name,
                    ImageUrl = !string.IsNullOrEmpty(od.Meal.ImagePath) ?
                        $"{Request.Scheme}://{Request.Host}/images/{Path.GetFileName(od.Meal.ImagePath)}" :
                        $"{Request.Scheme}://{Request.Host}/images/picture.png",
                    Quantity = od.Quantity,
                    PriceAtPurchase = od.PriceAtPurchase,
                    DiscountAtPurchase = od.DiscountAtPurchase,
                    FinalPrice = od.FinalPrice
                }).ToList()
            }).ToList();

            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "manager, admin")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _orderRepository.GetByIdWithDetailsAsync(id);
            if (order == null)
            {
                return NotFound(new { message = "Заказ не найден" });
            }

            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (currentUserRole != "admin" && currentUserRole != "manager" && order.UserId != currentUserId)
            {
                return Forbid();
            }

            return Ok(new OrderResponse
            {
                Id = order.Id,
                UserId = order.UserId,
                UserName = order.User.FullName,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                StatusText = GetStatusText(order.Status),
                DeliveryAddress = order.DeliveryAddress,
                DeliveryTime = order.DeliveryTime,
                PaymentMethod = order.PaymentMethod,
                Notes = order.Notes,
                Items = order.OrderDetails.Select(od => new OrderItemResponse
                {
                    MealId = od.MealId,
                    MealName = od.Meal.Name,
                    CategoryName = od.Meal.Category.Name,
                    ManufacturerName = od.Meal.Manufacturer.Name,
                    ImageUrl = !string.IsNullOrEmpty(od.Meal.ImagePath) ?
                        $"{Request.Scheme}://{Request.Host}/images/{Path.GetFileName(od.Meal.ImagePath)}" :
                        $"{Request.Scheme}://{Request.Host}/images/picture.png",
                    Quantity = od.Quantity,
                    PriceAtPurchase = od.PriceAtPurchase,
                    DiscountAtPurchase = od.DiscountAtPurchase,
                    FinalPrice = od.FinalPrice
                }).ToList()
            });
        }

        [HttpGet("my-orders")]
        [Authorize(Roles = "client, manager, admin")]
        public async Task<IActionResult> GetUserOrders()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var orders = await _orderRepository.GetOrdersByUserAsync(userId);
            var result = orders.Select(o => new OrderSummaryResponse
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                StatusText = GetStatusText(o.Status),
                DeliveryAddress = o.DeliveryAddress,
                ItemsCount = o.OrderDetails.Count,
                TotalItems = o.OrderDetails.Sum(od => od.Quantity)
            }).ToList();

            return Ok(new UserOrdersResponse { Orders = result });
        }

        [HttpPost]
        [Authorize(Roles = "client, manager, admin")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var meals = await _mealRepository.GetMealsByIdsAsync(request.Items.Select(i => i.MealId).ToList());
            var foundMeals = meals.ToList();

            if (foundMeals.Count != request.Items.Count)
            {
                return BadRequest("Некоторые блюда из заказа не найдены");
            }

            foreach (var item in request.Items)
            {
                var meal = meals.FirstOrDefault(b => b.Id == item.MealId);
                if (meal == null)
                {
                    return BadRequest($"Блюдо с ID {item.MealId} не найдено");
                }
                if (meal.StockQuantity < item.Quantity)
                {
                    return BadRequest($"Недостаточно блюд \"{meal.Name}\" на складе. Доступно: {meal.StockQuantity}, требуется: {item.Quantity}");
                }
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                DeliveryAddress = request.DeliveryAddress,
                DeliveryTime = request.DeliveryTime,
                PaymentMethod = request.PaymentMethod,
                Notes = request.Notes,
                Status = OrderStatus.Pending
            };

            decimal totalAmount = 0;
            var orderDetails = new List<OrderDetail>();

            foreach (var item in request.Items)
            {
                var meal = meals.First(b => b.Id == item.MealId);
                var priceAtPurchase = meal.Price;
                var discountAtPurchase = meal.Discount;
                var finalPrice = priceAtPurchase * (1 - discountAtPurchase / 100) * item.Quantity;
                totalAmount += finalPrice;

                orderDetails.Add(new OrderDetail
                {
                    MealId = item.MealId,
                    Quantity = item.Quantity,
                    PriceAtPurchase = priceAtPurchase,
                    DiscountAtPurchase = discountAtPurchase
                });
            }

            order.TotalAmount = totalAmount;
            await _orderRepository.AddOrderWithDetailsAsync(order, orderDetails);

            foreach (var item in request.Items)
            {
                var meal = meals.First(b => b.Id == item.MealId);
                meal.StockQuantity -= item.Quantity;
                await _mealRepository.UpdateAsync(meal);
            }

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, new
            {
                order.Id,
                order.OrderDate,
                order.TotalAmount,
                Status = order.Status.ToString(),
                message = "Заказ успешно создан"
            });
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "manager, admin")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatusUpdateRequest request)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound(new { message = "Заказ не найден" });
            }

            if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
            {
                return BadRequest("Неверный статус заказа");
            }

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);

            return Ok(new { message = "Статус заказа успешно обновлен" });
        }

        [HttpGet("statistics")]
        [Authorize(Roles = "manager, admin")]
        public async Task<IActionResult> GetOrderStatistics()
        {
            var stats = await _orderRepository.GetOrderStatisticsAsync();
            return Ok(new OrderStatisticsResponse
            {
                TotalOrders = stats.TotalOrders,
                TotalRevenue = stats.TotalRevenue,
                AverageOrderAmount = stats.AverageOrderAmount,
                OrdersToday = stats.OrdersToday,
                OrdersByStatus = stats.OrdersByStatus.ToDictionary(k => k.Key, v => v.Value)
            });
        }

        private string GetStatusText(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Ожидает обработки",
                OrderStatus.Preparing => "Готовится",
                OrderStatus.Dispatched => "Передан курьеру",
                OrderStatus.Delivered => "Доставлен",
                OrderStatus.Cancelled => "Отменён",
                _ => status.ToString()
            };
        }
    }
}