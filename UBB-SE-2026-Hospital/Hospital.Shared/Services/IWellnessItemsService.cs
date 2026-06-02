namespace Hospital.Shared.Services
{
    using System.Collections.Generic;
    using Hospital.Shared.Models;

    public interface IWellnessItemsService
    {
        List<Item> GetWellnessItems();
    }
}
