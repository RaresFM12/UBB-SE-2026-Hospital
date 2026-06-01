using System.Security.Claims;
using Hospital.Data.Models;
using Hospital.Shared.Models.Orders;
using Hospital.Shared.Services;
using Hospital.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedBasketEntry = Hospital.Shared.Models.StaffPharmacy.BasketEntry;

namespace Hospital.Web.Controllers;

[Authorize(Roles = "Client,Admin")]
public class BasketController : Controller
{
    private const float MaximumDiscount = 1f;
    private const float MinimumDiscount = 0f;

    private readonly IBasketService basketService;
    private readonly IAdminService adminService;
    private readonly IUserAccountService userAccountService;

    public BasketController(
        IBasketService basketService,
        IAdminService adminService,
        IUserAccountService userAccountService)
    {
        this.basketService = basketService;
        this.adminService = adminService;
        this.userAccountService = userAccountService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        User? currentUser = await LoadCurrentUserAsync(cancellationToken);
        if (currentUser is null)
        {
            return Forbid();
        }

        return View(await BuildViewModelAsync(currentUser.Id, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(BasketAddItemViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Choose a valid quantity.";
            return RedirectToBasketOrSource(viewModel);
        }

        User? currentUser = await LoadCurrentUserAsync(cancellationToken);
        if (currentUser is null)
        {
            return Forbid();
        }

        try
        {
            await basketService.AddToBasketAsync(
                currentUser.Id,
                viewModel.ItemId,
                viewModel.Quantity,
                viewModel.ExtraDiscountPercentage,
                cancellationToken);

            TempData["SuccessMessage"] = "Item added to basket.";
        }
        catch (ArgumentException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToBasketOrSource(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(BasketQuantityViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Choose a valid quantity.";
            return RedirectToAction(nameof(Index));
        }

        User? currentUser = await LoadCurrentUserAsync(cancellationToken);
        if (currentUser is null)
        {
            return Forbid();
        }

        Dictionary<int, SharedBasketEntry> basket = await basketService.GetBasketAsync(currentUser.Id, cancellationToken);
        if (basket.TryGetValue(viewModel.ItemId, out SharedBasketEntry? entry))
        {
            if (viewModel.Quantity <= 0)
            {
                basket.Remove(viewModel.ItemId);
            }
            else
            {
                entry.Quantity = viewModel.Quantity;
            }

            await basketService.SaveBasketAsync(currentUser.Id, basket, cancellationToken);
            TempData["SuccessMessage"] = "Basket updated.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int itemId, CancellationToken cancellationToken)
    {
        User? currentUser = await LoadCurrentUserAsync(cancellationToken);
        if (currentUser is null)
        {
            return Forbid();
        }

        Dictionary<int, SharedBasketEntry> basket = await basketService.GetBasketAsync(currentUser.Id, cancellationToken);
        if (basket.Remove(itemId))
        {
            await basketService.SaveBasketAsync(currentUser.Id, basket, cancellationToken);
            TempData["SuccessMessage"] = "Item removed from basket.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ApplyPrescription(BasketViewModel viewModel)
    {
        TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(viewModel.PrescriptionId)
            ? "Enter a prescription ID."
            : "Prescription basket import is waiting for the merged prescription service contract.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<BasketViewModel> BuildViewModelAsync(int userId, CancellationToken cancellationToken)
    {
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

        return new BasketViewModel
        {
            Items = basketItems,
            TotalBeforeDiscount = basketItems.Sum(item => item.FinalPriceBeforeDiscount),
            TotalAfterDiscount = basketItems.Sum(item => item.FinalPriceAfterDiscount),
            SuccessMessage = ReadTempData("SuccessMessage"),
            ErrorMessage = ReadTempData("ErrorMessage"),
        };
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

    private IActionResult RedirectToBasketOrSource(BasketAddItemViewModel viewModel)
    {
        if (!string.IsNullOrWhiteSpace(viewModel.ReturnController) &&
            !string.IsNullOrWhiteSpace(viewModel.ReturnAction))
        {
            return RedirectToAction(viewModel.ReturnAction, viewModel.ReturnController);
        }

        return RedirectToAction(nameof(Index));
    }

    private string? ReadTempData(string key)
        => TempData.TryGetValue(key, out object? value) ? value?.ToString() : null;

    private static float NormalizeDiscount(float discount)
    {
        if (discount > MaximumDiscount)
        {
            discount /= 100f;
        }

        return Math.Clamp(discount, MinimumDiscount, MaximumDiscount);
    }
}
