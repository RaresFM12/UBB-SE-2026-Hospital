using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.ViewModels.Admin;

public partial class InventoryItemsViewModel : ObservableObject
{
    private readonly IAdminService adminService;

    [ObservableProperty] private ObservableCollection<Item> items = new();
    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private bool showExpiredOnly;
    [ObservableProperty] private string statusMessage = string.Empty;

    public InventoryItemsViewModel(IAdminService adminService)
    {
        this.adminService = adminService;
    }

    partial void OnShowExpiredOnlyChanged(bool value)
    {
        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        StatusMessage = string.Empty;
        try
        {
            if (ShowExpiredOnly)
            {
                var expired = await Task.Run(() => adminService.GetExpiredItems());
                Items = new ObservableCollection<Item>(expired);
                StatusMessage = $"Showing {Items.Count} expired item(s).";
            }
            else
            {
                var search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();
                var result = await adminService.GetItemsAsync(search);
                Items = new ObservableCollection<Item>(result);
                StatusMessage = $"Showing {Items.Count} item(s).";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading items: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        // A manual search implies the expired filter is off.
        if (ShowExpiredOnly)
        {
            ShowExpiredOnly = false; // triggers reload via OnShowExpiredOnlyChanged
            return;
        }

        await LoadAsync();
    }

    [RelayCommand]
    private async Task DeleteAsync(Item? item)
    {
        StatusMessage = string.Empty;
        if (item is null)
        {
            return;
        }

        try
        {
            await adminService.DeleteItemAsync(item.Id);
            await LoadAsync();
            StatusMessage = $"Deleted '{item.Name}'.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting item: {ex.Message}";
        }
    }
}
