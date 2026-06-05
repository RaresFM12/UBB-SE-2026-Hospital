using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.ViewModels.Pharmacy;

public partial class OrdersViewModel : ObservableObject
{
    private readonly IOrderService orderService;
    private readonly ICurrentUserService currentUserService;

    [ObservableProperty] private ObservableCollection<OrderRow> orders = new();
    [ObservableProperty] private string statusMessage = string.Empty;

    public OrdersViewModel(IOrderService orderService, ICurrentUserService currentUserService)
    {
        this.orderService = orderService;
        this.currentUserService = currentUserService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        StatusMessage = string.Empty;
        try
        {
            int userId = currentUserService.UserId;
            IReadOnlyList<Order> result = await orderService.GetOrdersByClientAsync(userId);

            Orders = new ObservableCollection<OrderRow>();
            foreach (Order order in result)
            {
                Orders.Add(new OrderRow(
                    order.Id,
                    order.PickUpDateString,
                    order.IsCompleted,
                    order.IsExpired,
                    order.ItemQuantitiesWithFinalPrice.Count));
            }

            if (Orders.Count == 0)
            {
                StatusMessage = "No orders found.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CancelOrderAsync(OrderRow? row)
    {
        if (row is null)
        {
            return;
        }

        StatusMessage = string.Empty;
        try
        {
            await orderService.CancelOrderAsync(row.Id);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error cancelling order #{row.Id}: {ex.Message}";
        }
    }
}

public sealed record OrderRow(int Id, string PickUpDate, bool IsCompleted, bool IsExpired, int ItemCount);
