using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface IWellnessItemsService
{
    IReadOnlyList<Item> GetWellnessItems();
}
