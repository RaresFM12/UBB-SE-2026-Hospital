namespace Hospital.Shared.Services
{
    using System;
    using System.Collections.Generic;
    using Hospital.Shared.Models;
    using Hospital.Shared.Repositories;
    using Hospital.Shared.ViewModels.Orders;

    public interface IOrderService
    {
        ISubstancesRepository SubstancesRepository { get; }

        IItemsRepository ItemsRepository { get; }

        IUsersRepository UsersRepository { get; }

        IOrdersRepository OrdersRepository { get; }

        User ActiveUser { get; }

        void PlaceOrderFromBasket(DateOnly chosenPickUpDate);

        void CompleteOrder(int orderId, Dictionary<int, Tuple<int, float>> updatedQuantities);

        void ModifyIncompleteOrder(int orderIdToModify, Dictionary<int, Tuple<int, float>> updatedQuantities, DateOnly updatedPickUpDate);

        void ResubmitExpiredOrder(int orderIdToResubmit, DateOnly chosenPickUpDate);

        void CancelOrder(int orderId);

        void ExpireOverdueOrders();

        void AddToBasket(int itemId, int quantityToBuy);

        void AddItemToBasket(int itemId, int quantityToBuy, float extraDiscountPercentage = 0f);

        void UpdateBasketItemQuantity(int itemId, int newQuantityToBuy);

        void RemoveFromBasket(int itemIdToRemove);

        Dictionary<int, int> FillBasketFromPrescription(string prescriptionId);

        List<BasketItemViewModel> GetBasketItems();

        void ApplyPrescriptionToBasket(string prescriptionId);

        void RecalculateBasketItemPrices(BasketItemViewModel basketItem);

        Tuple<float, float> CalculateBasketTotalSum(IEnumerable<BasketItemViewModel> basketItems);
    }
}
