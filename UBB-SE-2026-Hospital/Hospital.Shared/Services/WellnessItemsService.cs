namespace Hospital.Shared.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Hospital.Shared.Models;
    using Hospital.Shared.Repositories;

    public class WellnessItemsService : IWellnessItemsService
    {
        private const string WellnessCategoryName = "wellness";

        private readonly IItemsRepository itemsRepository;

        public WellnessItemsService(IItemsRepository itemsRepository)
        {
            this.itemsRepository = itemsRepository;
        }

        public List<Item> GetWellnessItems()
        {
            return this.itemsRepository
                .GetAllItems()
                .Where(IsWellnessItem)
                .OrderBy(item => item.Id)
                .ToList();
        }

        private static bool IsWellnessItem(Item item)
        {
            return item.Category != null &&
                   item.Category.Equals(WellnessCategoryName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
