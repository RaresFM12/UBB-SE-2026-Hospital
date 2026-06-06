using Hospital.Data.Models;

namespace Hospital.Shared.Proxies;

public interface IWellnessItemsApiClient
{
    IReadOnlyList<Item> GetWellnessItems();
}
