using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class OrdersRepository(HospitalDbContext context) : IOrdersRepository
{
    public async Task<Order?> GetByIdAsync(int orderId)
        => await context.Orders
            .Include(o => o.Client)
            .Include(o => o.OrderItemEntries)
                .ThenInclude(oi => oi.Item)
            .FirstOrDefaultAsync(o => o.Id == orderId);

    public async Task<List<Order>> GetAllAsync()
        => await context.Orders
            .Include(o => o.Client)
            .Include(o => o.OrderItemEntries)
                .ThenInclude(oi => oi.Item)
            .ToListAsync();

    public async Task<List<Order>> GetByUserIdAsync(int userId)
        => await context.Orders
            .Include(o => o.Client)
            .Include(o => o.OrderItemEntries)
                .ThenInclude(oi => oi.Item)
            .Where(o => o.Client.Id == userId)
            .ToListAsync();

    public async Task<Order> CreateAsync(Order order)
    {
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        return order;
    }

    public async Task<Order> UpdateAsync(Order order)
    {
        context.Orders.Update(order);
        await context.SaveChangesAsync();
        return order;
    }

    public async Task DeleteAsync(int orderId)
    {
        var order = await context.Orders.FindAsync(orderId);
        if (order is not null)
        {
            context.Orders.Remove(order);
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(int orderId)
        => await context.OrderItems
            .Include(oi => oi.Item)
            .Include(oi => oi.Order)
            .Where(oi => oi.Order.Id == orderId)
            .ToListAsync();

    public async Task<OrderItem> AddOrderItemAsync(OrderItem orderItem)
    {
        context.OrderItems.Add(orderItem);
        await context.SaveChangesAsync();
        return orderItem;
    }

    public async Task DeleteOrderItemAsync(int orderItemId)
    {
        var orderItem = await context.OrderItems.FindAsync(orderItemId);
        if (orderItem is not null)
        {
            context.OrderItems.Remove(orderItem);
            await context.SaveChangesAsync();
        }
    }
}
