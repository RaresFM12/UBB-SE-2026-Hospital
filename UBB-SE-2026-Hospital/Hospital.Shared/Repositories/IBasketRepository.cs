namespace Hospital.Shared.Repositories
{
    using System.Collections.Generic;
    using Hospital.Shared.Models;

    public interface IBasketRepository
    {
        Dictionary<int, BasketEntry> GetBasket(int userId);

        void SaveBasket(int userId, Dictionary<int, BasketEntry> basket);

        void ClearBasket(int userId);
    }
}
