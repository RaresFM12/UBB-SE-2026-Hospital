using Hospital.Shared.Proxies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;
using Hospital.Web.Models;

namespace Hospital.Web.Controllers
{
    public class ProductCatalogueController : Controller
    {
        private readonly IProductCatalogueService _catalogueService;
        private readonly IBasketService _basketService;
        private readonly IUsersRepository _usersRepository;
        private const int ItemsPerPage = 12;
        private const int MinimumQuantity = 1;
        private const int MaximumQuantity = 50;
        private const int CatalogueFirstPageIndex = 0;
        private const int AllItemsPageSize = int.MaxValue;
        private const float FirstPriceRangeMinimum = 0f;
        private const float FirstPriceRangeMaximum = 49.99f;
        private const float SecondPriceRangeMinimum = 50f;
        private const float SecondPriceRangeMaximum = 99.99f;
        private const float ThirdPriceRangeMinimum = 100f;
        private const float ThirdPriceRangeMaximum = 199.99f;
        private const float FourthPriceRangeMinimum = 200f;
        private const float FourthPriceRangeMaximum = 499.99f;
        private const float FifthPriceRangeMinimum = 500f;

        public ProductCatalogueController(
            IProductCatalogueService catalogueService,
            IBasketService basketService,
            IUsersRepository usersRepository)
        {
            _catalogueService = catalogueService;
            _basketService = basketService;
            _usersRepository = usersRepository;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index(
            string searchText,
            List<string> categories,
            List<string> priceRanges,
            string stockFilter = "all",
            string discountFilter = "all",
            string sortBy = "default",
            string sortDirection = "asc",
            int pageIndex = 0)
        {
            var parsedPriceRanges = new List<(float, float)>();
            if (priceRanges != null)
            {
                if (priceRanges.Contains("0-49")) parsedPriceRanges.Add((FirstPriceRangeMinimum, FirstPriceRangeMaximum));
                if (priceRanges.Contains("50-99")) parsedPriceRanges.Add((SecondPriceRangeMinimum, SecondPriceRangeMaximum));
                if (priceRanges.Contains("100-199")) parsedPriceRanges.Add((ThirdPriceRangeMinimum, ThirdPriceRangeMaximum));
                if (priceRanges.Contains("200-499")) parsedPriceRanges.Add((FourthPriceRangeMinimum, FourthPriceRangeMaximum));
                if (priceRanges.Contains("500+")) parsedPriceRanges.Add((FifthPriceRangeMinimum, float.MaxValue));
            }

            string serviceStockFilter = stockFilter == "in_stock" ? Hospital.Shared.Services.IProductCatalogueService.StockFilterInStock :
                                        stockFilter == "low_stock" ? Hospital.Shared.Services.IProductCatalogueService.StockFilterLowStock : null;

            bool? serviceDiscountFilter = discountFilter == "yes" ? true :
                                          discountFilter == "no" ? false : (bool?)null;

            string serviceSortBy = sortBy == "price" ? Hospital.Shared.Services.IProductCatalogueService.SortByPrice :
                                   sortBy == "newest" ? Hospital.Shared.Services.IProductCatalogueService.SortByNewest : null;
            bool isAscending = sortDirection == "asc";

            int pageSize = ItemsPerPage; 

            var rawItems = await _catalogueService.GetItemsAsync(
                searchText,
                categories?.Any() == true ? categories : null,
                parsedPriceRanges.Any() ? parsedPriceRanges : null,
                serviceStockFilter,
                serviceDiscountFilter,
                null,
                isAscending,
                pageIndex,
                pageSize,
                serviceSortBy
            );

            var peekNextPage = await _catalogueService.GetItemsAsync(
                searchText, categories?.Any() == true ? categories : null, parsedPriceRanges.Any() ? parsedPriceRanges : null,
                serviceStockFilter, serviceDiscountFilter, null, isAscending, pageIndex + 1, pageSize, serviceSortBy);

            var viewModel = new CatalogueIndexViewModel
            {
                SearchText = searchText,
                SelectedCategories = categories ?? new List<string>(),
                SelectedPriceRanges = priceRanges ?? new List<string>(),
                StockFilter = stockFilter,
                DiscountFilter = discountFilter,
                SortBy = sortBy,
                SortDirection = sortDirection,
                CurrentPage = pageIndex,
                HasNextPage = peekNextPage.Any(),
                Products = rawItems.Select(MapToViewModel).ToList()
            };

            return View(viewModel);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var item = (await _catalogueService.GetItemsAsync(null, null, null, null, null, null, true, CatalogueFirstPageIndex, AllItemsPageSize))
                                        .FirstOrDefault(item => item.Id == id);

            if (item == null) return NotFound();

            var viewModel = MapToViewModel(item);

            if (item.Quantity == 0 && User.Identity?.IsAuthenticated == true)
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idClaim, out int userId))
                {
                    var currentUser = await _usersRepository.GetUserByIdAsync(userId);
                    if (currentUser != null)
                    {
                        viewModel.ShowStockAlertButton = true;
                        viewModel.IsStockAlertActive = currentUser.StockAlerts.Contains(id);
                    }
                }
            }

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStockAlert(int id)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out int userId))
            {
                return this.Forbid();
            }

            var currentUser = await _usersRepository.GetUserByIdAsync(userId);
            if (currentUser == null)
            {
                return this.Forbid();
            }

            if (currentUser.StockAlerts.Contains(id))
            {
                currentUser.StockAlerts.Remove(id);
            }
            else
            {
                currentUser.StockAlerts.Add(id);
            }

            await _usersRepository.UpdateUserAsync(currentUser);

            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int itemId, int quantity)
        {
            var item = (await _catalogueService.GetItemsAsync(null, null, null, null, null, null, true, CatalogueFirstPageIndex, AllItemsPageSize))
                                        .FirstOrDefault(itemNew => itemNew.Id == itemId);

            if (item == null || quantity < MinimumQuantity || quantity > MaximumQuantity || quantity > item.Quantity)
            {
                TempData["ErrorMessage"] = "Invalid quantity selected.";
                return RedirectToAction(nameof(Details), new { id = itemId });
            }

            try
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(idClaim, out int userId))
                {
                    return Forbid();
                }

                await _basketService.AddToBasketAsync(userId, itemId, quantity);
                TempData["SuccessMessage"] = "Item added to basket successfully!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Item already in basket or stock unavailable.";
            }

            return RedirectToAction(nameof(Details), new { id = itemId });
        }

        private CatalogueItemViewModel MapToViewModel(Hospital.Data.Models.Item item)
        {
            string cleanImage = item.ImagePath?.TrimStart('/') ?? "";
            if (cleanImage.StartsWith("ms-appx:///")) cleanImage = cleanImage.Replace("ms-appx:///", "");

            string substancesText = item.ActiveSubstances != null && item.ActiveSubstances.Any()
                ? string.Join(", ", item.ActiveSubstances.Select(substance => $"{substance.Key} ({substance.Value})"))
                : "None";

            return new CatalogueItemViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Producer = item.Producer,
                Category = item.Category,
                Label = item.Label,
                NumberOfPills = item.NumberOfPills,
                SubstancesText = substancesText,
                OldPrice = (float)item.Price,
                DiscountPercentage = item.DiscountPercentage,
                Quantity = item.Quantity,
                ImagePath = cleanImage,
                Description = item.Description
            };
        }

        [Authorize(Roles = "Admin")][HttpGet] public IActionResult Create() => View();
        [Authorize(Roles = "Admin")][HttpPost][ValidateAntiForgeryToken] public IActionResult Create(object model) => RedirectToAction(nameof(Index));
        [Authorize(Roles = "Admin")][HttpGet] public IActionResult Edit(int id) => View();
        [Authorize(Roles = "Admin")][HttpPost][ValidateAntiForgeryToken] public IActionResult Edit(int id, object model) => RedirectToAction(nameof(Index));
        [Authorize(Roles = "Admin")][HttpGet] public IActionResult Delete(int id) => View();
        [Authorize(Roles = "Admin")][HttpPost, ActionName("Delete")][ValidateAntiForgeryToken] public IActionResult DeleteConfirmed(int id) => RedirectToAction(nameof(Index));
    }
}

