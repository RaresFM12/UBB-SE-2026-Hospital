using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.ViewModels.Pharmacy;

public partial class BasketViewModel : ObservableObject
{
    private readonly IOrderService orderService;
    private readonly ICurrentUserService currentUserService;

    [ObservableProperty] private ObservableCollection<BasketItemViewModel> items = new();
    [ObservableProperty] private float totalBeforeDiscount;
    [ObservableProperty] private float totalAfterDiscount;
    [ObservableProperty] private DateTimeOffset pickUpDate = DateTimeOffset.Now.AddDays(1);
    [ObservableProperty] private string statusMessage = string.Empty;

    public BasketViewModel(IOrderService orderService, ICurrentUserService currentUserService)
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
            List<BasketItemViewModel> result = await orderService.GetBasketItemsAsync(userId);

            Items = new ObservableCollection<BasketItemViewModel>(result);

            Tuple<float, float> totals = orderService.CalculateBasketTotalSum(result);
            TotalBeforeDiscount = totals.Item1;
            TotalAfterDiscount = totals.Item2;

            if (Items.Count == 0)
            {
                StatusMessage = "Your basket is empty.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RemoveAsync(BasketItemViewModel? item)
    {
        if (item is null)
        {
            return;
        }

        StatusMessage = string.Empty;
        try
        {
            await orderService.RemoveFromBasketAsync(currentUserService.UserId, item.ItemId);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error removing item: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task PlaceOrderAsync()
    {
        StatusMessage = string.Empty;
        try
        {
            DateOnly pickup = DateOnly.FromDateTime(PickUpDate.DateTime);
            await orderService.PlaceOrderFromBasketAsync(currentUserService.UserId, pickup);
            StatusMessage = "Order placed successfully.";
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error placing order: {ex.Message}";
        }
    }
}
