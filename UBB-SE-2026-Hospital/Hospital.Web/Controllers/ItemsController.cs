using System.Globalization;
using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

[Authorize(Roles = "Admin")]
public class ItemsController : Controller
{
    private const string BatchDateFormat = "yyyy-MM-dd";
    private const string SubstanceLineDelimiter = ":";
    private static readonly string[] LineDelimiters = { "\r\n", "\n" };

    private readonly IAdminService adminService;

    public ItemsController(IAdminService adminService)
    {
        this.adminService = adminService;
    }

    public async Task<IActionResult> Index(
        string searchQuery = "",
        bool showExpiredOnly = false,
        CancellationToken cancellationToken = default)
    {
        List<Item> items = await LoadItemsAsync(searchQuery, showExpiredOnly, cancellationToken);

        var viewModel = new ItemIndexViewModel
        {
            Items = items,
            SearchQuery = searchQuery,
            ShowExpiredOnly = showExpiredOnly,
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        Item? item = await adminService.GetItemByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : View(item);
    }

    [HttpGet]
    public IActionResult Create()
        => View(new ItemViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ItemViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        Dictionary<string, float> activeSubstances = ParseSubstancesText(viewModel.SubstancesText);
        Dictionary<DateOnly, int> batches = ParseBatchesText(viewModel.BatchesText);
        int quantity = batches.Values.Sum();

        try
        {
            await adminService.CreateItemWithQuantityAsync(
                viewModel.Name,
                viewModel.Producer,
                viewModel.Category,
                viewModel.Price,
                viewModel.NumberOfPills,
                quantity,
                activeSubstances,
                batches,
                viewModel.Label,
                viewModel.Description,
                viewModel.ImagePath,
                viewModel.DiscountPercentage,
                cancellationToken);

            TempData["SuccessMessage"] = "Item created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException argumentException)
        {
            ModelState.AddModelError(string.Empty, argumentException.Message);
            return View(viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        Item? item = await adminService.GetItemByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : View(MapItemToViewModel(item));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ItemViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        Dictionary<string, float> activeSubstances = ParseSubstancesText(viewModel.SubstancesText);
        Dictionary<DateOnly, int> batches = ParseBatchesText(viewModel.BatchesText);
        int quantity = batches.Values.Sum();

        var updatedItem = new Item(
            id,
            viewModel.Name,
            viewModel.Producer,
            viewModel.Category,
            viewModel.Price,
            viewModel.NumberOfPills,
            viewModel.Label,
            viewModel.Description,
            viewModel.ImagePath,
            viewModel.DiscountPercentage,
            quantity)
        {
            ActiveSubstances = activeSubstances,
            Batches = batches,
        };

        try
        {
            await adminService.UpdateItemAsync(updatedItem, cancellationToken);
            TempData["SuccessMessage"] = "Item updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException argumentException)
        {
            ModelState.AddModelError(string.Empty, argumentException.Message);
            return View(viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        Item? item = await adminService.GetItemByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        await adminService.DeleteItemAsync(id, cancellationToken);
        TempData["SuccessMessage"] = "Item deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<Item>> LoadItemsAsync(
        string searchQuery,
        bool showExpiredOnly,
        CancellationToken cancellationToken)
    {
        List<Item> items = (await adminService.GetItemsAsync(searchQuery, cancellationToken)).ToList();
        if (!showExpiredOnly)
        {
            return items;
        }

        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        return items
            .Where(item => item.Batches.Keys.Any(expirationDate => expirationDate < today))
            .ToList();
    }

    private static ItemViewModel MapItemToViewModel(Item item)
    {
        string FormatSubstanceEntry(KeyValuePair<string, float> substanceEntry) =>
            $"{substanceEntry.Key}{SubstanceLineDelimiter}{substanceEntry.Value.ToString(CultureInfo.InvariantCulture)}";

        string FormatBatchEntry(KeyValuePair<DateOnly, int> batchEntry) =>
            $"{batchEntry.Key.ToString(BatchDateFormat, CultureInfo.InvariantCulture)}{SubstanceLineDelimiter}{batchEntry.Value}";

        return new ItemViewModel
        {
            Id = item.Id,
            Name = item.Name,
            Producer = item.Producer,
            Price = item.Price,
            Category = item.Category,
            ImagePath = item.ImagePath,
            NumberOfPills = item.NumberOfPills,
            Quantity = item.Quantity,
            Label = item.Label,
            Description = item.Description,
            DiscountPercentage = item.DiscountPercentage,
            SubstancesText = string.Join(Environment.NewLine, item.ActiveSubstances.Select(FormatSubstanceEntry)),
            BatchesText = string.Join(Environment.NewLine, item.Batches.Select(FormatBatchEntry)),
        };
    }

    private static Dictionary<string, float> ParseSubstancesText(string substancesText)
    {
        const int ExpectedPartCount = 2;
        var activeSubstances = new Dictionary<string, float>();

        if (string.IsNullOrWhiteSpace(substancesText))
        {
            return activeSubstances;
        }

        foreach (string line in substancesText.Split(LineDelimiters, StringSplitOptions.RemoveEmptyEntries))
        {
            string[] parts = line.Split(SubstanceLineDelimiter, ExpectedPartCount);
            if (parts.Length != ExpectedPartCount)
            {
                continue;
            }

            string substanceName = parts[0].Trim();
            if (float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float concentration))
            {
                activeSubstances[substanceName] = concentration;
            }
        }

        return activeSubstances;
    }

    private static Dictionary<DateOnly, int> ParseBatchesText(string batchesText)
    {
        const int ExpectedPartCount = 2;
        var batches = new Dictionary<DateOnly, int>();

        if (string.IsNullOrWhiteSpace(batchesText))
        {
            return batches;
        }

        foreach (string line in batchesText.Split(LineDelimiters, StringSplitOptions.RemoveEmptyEntries))
        {
            string[] parts = line.Split(SubstanceLineDelimiter, ExpectedPartCount);
            if (parts.Length != ExpectedPartCount)
            {
                continue;
            }

            bool dateIsValid = DateOnly.TryParseExact(
                parts[0].Trim(),
                BatchDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateOnly expirationDate);

            bool quantityIsValid = int.TryParse(parts[1].Trim(), out int numberOfPacks);

            if (dateIsValid && quantityIsValid)
            {
                batches[expirationDate] = numberOfPacks;
            }
        }

        return batches;
    }
}
