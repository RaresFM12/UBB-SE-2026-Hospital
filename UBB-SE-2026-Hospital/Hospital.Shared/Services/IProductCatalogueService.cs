namespace Hospital.Shared.Services
{
    using System.Collections.Generic;
    using Hospital.Shared.Models;

    public interface IProductCatalogueService
    {
        List<Item> GetItems(
            string search,
            List<string> categories = null,
            List<(float minimum, float maximum)> priceRanges = null,
            string stockFilter = null,
            bool? discounted = null,
            List<string> substances = null,
            bool ascending = true,
            int page = 0,
            int pageSize = ProductCatalogueService.DefaultPageSize,
            string sortBy = null);
    }
}
