using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface IOrdersRepository
{
    Task<Order?> GetByIdAsync(int orderId);
    Task<List<Order>> GetAllAsync();
    Task<List<Order>> GetByUserIdAsync(int userId);
    Task<Order> CreateAsync(Order order);
    Task<Order> UpdateAsync(Order order);
    Task DeleteAsync(int orderId);

    Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(int orderId);
    Task<OrderItem> AddOrderItemAsync(OrderItem orderItem);
    Task DeleteOrderItemAsync(int orderItemId);
}
