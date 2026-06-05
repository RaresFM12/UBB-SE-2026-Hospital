    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hospital.Shared.Services;
using Hospital.Web.Models;
using BasketItemViewModel = Hospital.Data.Models.BasketItemViewModel;

namespace Hospital.Web.Controllers;

[Authorize(Roles = "Client,Admin")]
public class BasketController : Controller
{
    private readonly IOrderService orderService;

    public BasketController(IOrderService orderService)
    {
        this.orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        int? userId = this.GetCurrentUserId();
        if (!userId.HasValue)
        {
            return this.Forbid();
        }

        return this.View(await this.BuildViewModelAsync(userId.Value));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(BasketAddItemViewModel viewModel)
    {
        if (!this.ModelState.IsValid)
        {
            this.TempData["ErrorMessage"] = "Choose a valid quantity.";
            return this.RedirectToBasketOrSource(viewModel);
        }

        try
        {
            int? userId = this.GetCurrentUserId();
            if (!userId.HasValue)
            {
                return this.Forbid();
            }

            await this.orderService.AddItemToBasketAsync(userId.Value, viewModel.ItemId, viewModel.Quantity, viewModel.ExtraDiscountPercentage);
            this.TempData["SuccessMessage"] = "Item added to basket.";
        }
        catch (ArgumentException exception)
        {
            this.TempData["ErrorMessage"] = exception.Message;
        }

        return this.RedirectToBasketOrSource(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(BasketQuantityViewModel viewModel)
    {
        if (!this.ModelState.IsValid)
        {
            this.TempData["ErrorMessage"] = "Choose a valid quantity.";
            return this.RedirectToAction(nameof(this.Index));
        }

        try
        {
            int? userId = this.GetCurrentUserId();
            if (!userId.HasValue)
            {
                return this.Forbid();
            }

            await this.orderService.UpdateBasketItemQuantityAsync(userId.Value, viewModel.ItemId, viewModel.Quantity);
            this.TempData["SuccessMessage"] = "Basket updated.";
        }
        catch (ArgumentException exception)
        {
            this.TempData["ErrorMessage"] = exception.Message;
        }

        return this.RedirectToAction(nameof(this.Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int itemId)
    {
        try
        {
            int? userId = this.GetCurrentUserId();
            if (!userId.HasValue)
            {
                return this.Forbid();
            }

            await this.orderService.RemoveFromBasketAsync(userId.Value, itemId);
            this.TempData["SuccessMessage"] = "Item removed from basket.";
        }
        catch (ArgumentException exception)
        {
            this.TempData["ErrorMessage"] = exception.Message;
        }

        return this.RedirectToAction(nameof(this.Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyPrescription(BasketViewModel viewModel)
    {
        if (string.IsNullOrWhiteSpace(viewModel.PrescriptionId))
        {
            this.TempData["ErrorMessage"] = "Enter a prescription ID.";
            return this.RedirectToAction(nameof(this.Index));
        }

        try
        {
            int? userId = this.GetCurrentUserId();
            if (!userId.HasValue)
            {
                return this.Forbid();
            }

            await this.orderService.ApplyPrescriptionToBasketAsync(userId.Value, viewModel.PrescriptionId);
            this.TempData["SuccessMessage"] = "Prescription items added to basket.";
        }
        catch (ArgumentException exception)
        {
            this.TempData["ErrorMessage"] = exception.Message;
        }

        return this.RedirectToAction(nameof(this.Index));
    }

    private async Task<BasketViewModel> BuildViewModelAsync(int userId)
    {
        List<BasketItemViewModel> basketItems = await this.orderService.GetBasketItemsAsync(userId);
        Tuple<float, float> totals = this.orderService.CalculateBasketTotalSum(basketItems);

        return new BasketViewModel
        {
            Items = basketItems,
            TotalBeforeDiscount = totals.Item1,
            TotalAfterDiscount = totals.Item2,
            SuccessMessage = this.ReadTempData("SuccessMessage"),
            ErrorMessage = this.ReadTempData("ErrorMessage"),
        };
    }

    private IActionResult RedirectToBasketOrSource(BasketAddItemViewModel viewModel)
    {
        if (!string.IsNullOrWhiteSpace(viewModel.ReturnController) &&
            !string.IsNullOrWhiteSpace(viewModel.ReturnAction))
        {
            return this.RedirectToAction(viewModel.ReturnAction, viewModel.ReturnController);
        }

        return this.RedirectToAction(nameof(this.Index));
    }

    private string? ReadTempData(string key)
    {
        return this.TempData.TryGetValue(key, out object? value) ? value?.ToString() : null;
    }

    private int? GetCurrentUserId()
    {
        string? idValue = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idValue, out int userId) ? userId : null;
    }
}
