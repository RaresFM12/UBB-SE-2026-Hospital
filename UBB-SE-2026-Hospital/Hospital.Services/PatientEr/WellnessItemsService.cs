using System;
using System.Collections.Generic;
using System.Linq;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services.PatientEr
{
    public class WellnessItemsService : IWellnessItemsService
    {
        private const string WellnessCategoryName = "wellness";
        private readonly IItemsRepository itemsRepository;

        public WellnessItemsService(IItemsRepository itemsRepository)
        {
            this.itemsRepository = itemsRepository;
        }

        public IReadOnlyList<Item> GetWellnessItems()
        {
            var items = this.itemsRepository.GetAllAsync().GetAwaiter().GetResult();

            return items
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