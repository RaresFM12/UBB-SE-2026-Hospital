using System.Security.Claims;
using Hospital.Data.Models;
using Hospital.Shared.Models.Orders;
using Hospital.Shared.Services;
using Hospital.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedBasketEntry = Hospital.Shared.Models.StaffPharmacy.BasketEntry;

namespace Hospital.Web.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private const int MinimumCheckoutLeadDays = 1;

    private readonly IOrderService orderService;
    private readonly IUserAccountService userAccountService;
    private readonly IAdminService adminService;
    private readonly IBasketService basketService;

    public OrdersController(
        IOrderService orderService,
        IUserAccountService userAccountService,
        IAdminService adminService,
        IBasketService basketService)
    {
        this.orderService = orderService;
        this.userAccountService = userAccountService;
        this.adminService = adminService;
        this.basketService = basketService;
    }

    [Authorize(Roles = "Client,Admin")]
    [HttpGet]
    public async Task<IActionResult> Index(bool showExpiredOnly = false, CancellationToken cancellationToken = default)
    {
        User? currentUser = await LoadCurrentUserAsync(cancellationToken);
        if (currentUser is null)
        {
            return Forbid();
        }

        await orderService.ExpireOverdueOrdersAsync(cancellationToken);
        List<OrderListItemViewModel> orders = (await orderService.GetOrdersByClientAsync(currentUser.Id, cancellationToken))
            .Where(order => !showExpiredOnly || order.IsExpired)
            .OrderByDescending(order => order.PickUpDate)
            .ThenByDescending(order => order.Id)
            .Select(MapOrderListItem)
            .ToList();

        return View(new OrdersIndexViewModel
        {
            Orders = orders,
            ShowExpiredOnly = showExpiredOnly,
            SuccessMessage = ReadTempData("SuccessMessage"),
            ErrorMessage = ReadTempData("ErrorMessage"),
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Manage(
        string userEmail = "",
        int? orderId = null,
        bool incompleteOnly = false,
        bool expiredOnly = false,
        CancellationToken cancellationToken = default)
    {
        await orderService.ExpireOverdueOrdersAsync(cancellationToken);
        List<Order> orders = (await orderService.GetAllOrdersAsync(cancellationToken)).ToList();

        if (!string.IsNullOrWhiteSpace(userEmail))
        {
            orders = orders
                .Where(order => GetClientEmail(order).Contains(userEmail.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (orderId.HasValue)
        {
            orders = orders.Where(order => order.Id == orderId.Value).ToList();
        }

        if (incompleteOnly)
        {
            orders = orders.Where(order => !order.IsCompleted && !order.IsExpired).ToList();
        }

        if (expiredOnly)
        {
            orders = orders.Where(order => order.IsExpired).ToList();
        }

        return View(new OrderManagementViewModel
        {
            Orders = orders.OrderByDescending(order => order.Id).Select(MapOrderListItem).ToList(),
            UserEmail = userEmail,
            OrderId = orderId,
            IncompleteOnly = incompleteOnly,
            ExpiredOnly = expiredOnly,
            SuccessMessage = ReadTempData("SuccessMessage"),
            ErrorMessage = ReadTempData("ErrorMessage"),
        });
    }

    [Authorize(Roles = "Client,Admin")]
    [HttpGet]
    public async Task<IActionResult> Details(int id, bool adminView = false, CancellationToken cancellationToken = default)
    {
        Order? order = await FindOrderForCurrentUserAsync(id, allowAdmin: true, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        OrderDetailsViewModel viewModel = await MapDetailsAsync(order, adminView, cancellationToken);
        viewModel.SuccessMessage = ReadTempData("SuccessMessage");
        viewModel.ErrorMessage = ReadTempData("ErrorMessage");
        return View(viewModel);
    }

    [Authorize(Roles = "Client,Admin")]
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
        => View(await BuildCheckoutViewModelAsync(cancellationToken: cancellationToken));

    [Authorize(Roles = "Client,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrderCheckoutViewModel viewModel, CancellationToken cancellationToken)
    {
        if (viewModel.PickUpDate <= DateOnly.FromDateTime(DateTime.Today))
        {
            ModelState.AddModelError(string.Empty, "The pick-up date must be at least one day after today.");
        }

        User? currentUser = await LoadCurrentUserAsync(cancellationToken);
        if (currentUser is null)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View(await BuildCheckoutViewModelAsync(viewModel.PickUpDate, cancellationToken));
        }

        try
        {
            await orderService.PlaceOrderFromBasketAsync(currentUser.Id, viewModel.PickUpDate, cancellationToken);
            TempData["SuccessMessage"] = "Order placed successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(await BuildCheckoutViewModelAsync(viewModel.PickUpDate, cancellationToken));
        }
    }

    [Authorize(Roles = "Client,Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        Order? order = await FindOrderForCurrentUserAsync(id, allowAdmin: true, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        if (!CanEdit(order))
        {
            TempData["ErrorMessage"] = "Only incomplete, active orders can be modified.";
            return RedirectToAction(nameof(Details), new { id });
        }

        return View(await MapEditAsync(order, User.IsInRole("Admin"), cancellationToken));
    }

    [Authorize(Roles = "Client,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(OrderEditViewModel viewModel, CancellationToken cancellationToken)
    {
        Order? order = await FindOrderForCurrentUserAsync(viewModel.Id, allowAdmin: true, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        if (!CanEdit(order))
        {
            return Forbid();
        }

        Dictionary<int, Tuple<int, float>> updatedItems = BuildUpdatedItems(viewModel.Items);
        if (updatedItems.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "The order must contain at least one item.");
        }

        if (!ModelState.IsValid)
        {
            viewModel.AdminView = User.IsInRole("Admin");
            viewModel.Total = viewModel.Items.Sum(item => item.FinalPrice);
            return View(viewModel);
        }

        try
        {
            order.PickUpDate = viewModel.PickUpDate;
            order.ItemQuantitiesWithFinalPrice = updatedItems;
            await orderService.UpdateOrderAsync(order, cancellationToken);
            TempData["SuccessMessage"] = "Order changes saved.";
            return RedirectToAction(nameof(Details), new { id = viewModel.Id, adminView = User.IsInRole("Admin") });
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            viewModel.AdminView = User.IsInRole("Admin");
            viewModel.Total = viewModel.Items.Sum(item => item.FinalPrice);
            return View(viewModel);
        }
    }

    [Authorize(Roles = "Client,Admin")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        Order? order = await FindOrderForCurrentUserAsync(id, allowAdmin: true, cancellationToken);
        return order is null ? NotFound() : View(await MapDetailsAsync(order, User.IsInRole("Admin"), cancellationToken));
    }

    [Authorize(Roles = "Client,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        Order? order = await FindOrderForCurrentUserAsync(id, allowAdmin: true, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        if (order.IsCompleted)
        {
            TempData["ErrorMessage"] = "Completed orders cannot be cancelled.";
            return RedirectToAction(nameof(Details), new { id });
        }

        await orderService.CancelOrderAsync(id, cancellationToken);
        TempData["SuccessMessage"] = "Order cancelled.";
        return User.IsInRole("Admin") ? RedirectToAction(nameof(Manage)) : RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Client,Admin")]
    [HttpGet]
    public async Task<IActionResult> Resubmit(int id, CancellationToken cancellationToken)
    {
        Order? order = await FindOrderForCurrentUserAsync(id, allowAdmin: true, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        if (!order.IsExpired)
        {
            TempData["ErrorMessage"] = "Only expired orders can be resubmitted.";
            return RedirectToAction(nameof(Details), new { id });
        }

        return View(await MapResubmitAsync(order, cancellationToken));
    }

    [Authorize(Roles = "Client,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resubmit(OrderResubmitViewModel viewModel, CancellationToken cancellationToken)
    {
        if (viewModel.PickUpDate <= DateOnly.FromDateTime(DateTime.Today))
        {
            ModelState.AddModelError(string.Empty, "The pick-up date must be at least one day after today.");
        }

        Order? order = await FindOrderForCurrentUserAsync(viewModel.Id, allowAdmin: true, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            OrderResubmitViewModel hydratedViewModel = await MapResubmitAsync(order, cancellationToken);
            hydratedViewModel.PickUpDate = viewModel.PickUpDate;
            return View(hydratedViewModel);
        }

        try
        {
            order.PickUpDate = viewModel.PickUpDate;
            order.IsExpired = false;
            await orderService.UpdateOrderAsync(order, cancellationToken);
            TempData["SuccessMessage"] = "Order resubmitted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            OrderResubmitViewModel hydratedViewModel = await MapResubmitAsync(order, cancellationToken);
            hydratedViewModel.PickUpDate = viewModel.PickUpDate;
            return View(hydratedViewModel);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(OrderEditViewModel viewModel, CancellationToken cancellationToken)
    {
        Dictionary<int, (int Quantity, float Discount)> updatedItems = BuildCompletionItems(viewModel.Items);
        if (updatedItems.Count == 0)
        {
            TempData["ErrorMessage"] = "The order must contain at least one item.";
            return RedirectToAction(nameof(Details), new { id = viewModel.Id, adminView = true });
        }

        try
        {
            await orderService.CompleteOrderAsync(viewModel.Id, updatedItems, cancellationToken);
            TempData["SuccessMessage"] = "Order completed and inventory updated.";
        }
        catch (ArgumentException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Details), new { id = viewModel.Id, adminView = true });
    }

    private async Task<User?> LoadCurrentUserAsync(CancellationToken cancellationToken)
    {
        string? idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(idValue, out int userId))
        {
            return await userAccountService.GetUserByIdAsync(userId, cancellationToken);
        }

        string? username = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        return (await userAccountService.GetAllUsersAsync(cancellationToken))
            .FirstOrDefault(user => string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<Order?> FindOrderForCurrentUserAsync(int id, bool allowAdmin, CancellationToken cancellationToken)
    {
        Order? order = await orderService.GetOrderByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return null;
        }

        if (allowAdmin && User.IsInRole("Admin"))
        {
            return order;
        }

        User? currentUser = await LoadCurrentUserAsync(cancellationToken);
        if (currentUser is null)
        {
            return null;
        }

        return order.Client.Id == currentUser.Id ? order : null;
    }

    private async Task<OrderCheckoutViewModel> BuildCheckoutViewModelAsync(
        DateOnly? pickUpDate = null,
        CancellationToken cancellationToken = default)
    {
        User? currentUser = await LoadCurrentUserAsync(cancellationToken);
        var basketItems = new List<BasketItemViewModel>();

        if (currentUser is not null)
        {
            basketItems = await BuildBasketItemsAsync(currentUser.Id, cancellationToken);
        }

        return new OrderCheckoutViewModel
        {
            PickUpDate = pickUpDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(MinimumCheckoutLeadDays)),
            Items = basketItems,
            TotalBeforeDiscount = basketItems.Sum(item => item.FinalPriceBeforeDiscount),
            TotalAfterDiscount = basketItems.Sum(item => item.FinalPriceAfterDiscount),
        };
    }

    private async Task<OrderDetailsViewModel> MapDetailsAsync(Order order, bool adminView, CancellationToken cancellationToken)
    {
        List<OrderLineItemViewModel> items = await MapLineItemsAsync(order, cancellationToken);
        return new OrderDetailsViewModel
        {
            Id = order.Id,
            UserEmail = GetClientEmail(order),
            PickUpDate = order.PickUpDate,
            ExpirationDate = order.PickUpDate.AddDays(Order.OrderExpirationDays),
            IsCompleted = order.IsCompleted,
            IsExpired = order.IsExpired,
            AdminView = adminView,
            Items = items,
            Total = items.Sum(item => item.FinalPrice),
        };
    }

    private async Task<OrderEditViewModel> MapEditAsync(Order order, bool adminView, CancellationToken cancellationToken)
    {
        List<OrderLineItemViewModel> items = await MapLineItemsAsync(order, cancellationToken);
        return new OrderEditViewModel
        {
            Id = order.Id,
            PickUpDate = order.PickUpDate,
            AdminView = adminView,
            Items = items,
            Total = items.Sum(item => item.FinalPrice),
        };
    }

    private async Task<OrderResubmitViewModel> MapResubmitAsync(Order order, CancellationToken cancellationToken)
    {
        List<OrderLineItemViewModel> items = await MapLineItemsAsync(order, cancellationToken);
        return new OrderResubmitViewModel
        {
            Id = order.Id,
            PickUpDate = DateOnly.FromDateTime(DateTime.Today.AddDays(MinimumCheckoutLeadDays)),
            Items = items,
            Total = items.Sum(item => item.FinalPrice),
        };
    }

    private static OrderListItemViewModel MapOrderListItem(Order order)
        => new()
        {
            Id = order.Id,
            UserEmail = GetClientEmail(order),
            PickUpDate = order.PickUpDate,
            ExpirationDate = order.PickUpDate.AddDays(Order.OrderExpirationDays),
            IsCompleted = order.IsCompleted,
            IsExpired = order.IsExpired,
        };

    private async Task<List<OrderLineItemViewModel>> MapLineItemsAsync(Order order, CancellationToken cancellationToken)
    {
        var lineItems = new List<OrderLineItemViewModel>();
        foreach (KeyValuePair<int, Tuple<int, float>> entry in order.ItemQuantitiesWithFinalPrice)
        {
            Item? item = await adminService.GetItemByIdAsync(entry.Key, cancellationToken);
            lineItems.Add(new OrderLineItemViewModel
            {
                ItemId = item?.Id ?? entry.Key,
                Name = item?.Name ?? $"Deleted item #{entry.Key}",
                Producer = item?.Producer ?? "Unavailable",
                Quantity = entry.Value.Item1,
                FinalPrice = entry.Value.Item2,
            });
        }

        return lineItems;
    }

    private async Task<List<BasketItemViewModel>> BuildBasketItemsAsync(int userId, CancellationToken cancellationToken)
    {
        const float MaximumDiscount = 1f;
        const float MinimumDiscount = 0f;

        Dictionary<int, SharedBasketEntry> basket = await basketService.GetBasketAsync(userId, cancellationToken);
        var basketItems = new List<BasketItemViewModel>();

        foreach (SharedBasketEntry entry in basket.Values)
        {
            Item? item = await adminService.GetItemByIdAsync(entry.ItemId, cancellationToken);
            if (item is null)
            {
                continue;
            }

            float baseDiscount = NormalizeDiscount(item.DiscountPercentage);
            float extraDiscount = NormalizeDiscount(entry.ExtraDiscountPercentage);
            float beforeDiscount = item.Price * entry.Quantity;
            float afterDiscount = beforeDiscount * (MaximumDiscount - baseDiscount) * (MaximumDiscount - extraDiscount);

            var basketItem = new BasketItemViewModel(
                item.Id,
                item.ImagePath,
                item.Name,
                item.Producer,
                entry.Quantity,
                baseDiscount,
                extraDiscount,
                MinimumDiscount,
                item.Price);
            basketItem.SetFinalPrices(beforeDiscount, afterDiscount);
            basketItems.Add(basketItem);
        }

        return basketItems;
    }

    private static string GetClientEmail(Order order)
        => order.Client?.Email ?? "Unknown";

    private static bool CanEdit(Order order)
        => !order.IsCompleted && !order.IsExpired;

    private static Dictionary<int, Tuple<int, float>> BuildUpdatedItems(IEnumerable<OrderLineItemViewModel> items)
        => items
            .Where(item => item.Quantity > 0)
            .ToDictionary(item => item.ItemId, item => Tuple.Create(item.Quantity, item.FinalPrice));

    private static Dictionary<int, (int Quantity, float Discount)> BuildCompletionItems(IEnumerable<OrderLineItemViewModel> items)
        => items
            .Where(item => item.Quantity > 0)
            .ToDictionary(item => item.ItemId, item => (item.Quantity, item.FinalPrice));

    private string? ReadTempData(string key)
        => TempData.TryGetValue(key, out object? value) ? value?.ToString() : null;

    private static float NormalizeDiscount(float discount)
    {
        const float MaximumDiscount = 1f;
        const float MinimumDiscount = 0f;

        if (discount > MaximumDiscount)
        {
            discount /= 100f;
        }

        return Math.Clamp(discount, MinimumDiscount, MaximumDiscount);
    }
}
