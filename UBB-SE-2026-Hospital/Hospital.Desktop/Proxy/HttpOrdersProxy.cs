using Hospital.Shared.Models;
using Hospital.Shared.Services;
using Hospital.Shared.Repositories;
using Hospital.Shared.ViewModels.Orders;

namespace Hospital.Desktop.Proxy;

public class HttpOrdersProxy(HttpClient httpClient) : ProxyBase(httpClient), IOrderService
{
    private const string BaseUri = "api/orders";

    public ISubstancesRepository SubstancesRepository => throw new NotImplementedException();
    public IItemsRepository ItemsRepository => throw new NotImplementedException();
    public IUsersRepository UsersRepository => throw new NotImplementedException();
    public IOrdersRepository OrdersRepository => throw new NotImplementedException();
    public User ActiveUser => throw new NotImplementedException();

    public void PlaceOrderFromBasket(DateOnly chosenPickUpDate)
        => Task.Run(async () => await PostAsync<object, object>($"{BaseUri}/place-from-basket", new { chosenPickUpDate })).GetAwaiter().GetResult();

    public void CompleteOrder(int orderId, Dictionary<int, Tuple<int, float>> updatedQuantities)
        => Task.Run(async () => await PostAsync<object, object>($"{BaseUri}/{orderId}/complete", new { updatedQuantities })).GetAwaiter().GetResult();

    public void ModifyIncompleteOrder(int orderIdToModify, Dictionary<int, Tuple<int, float>> updatedQuantities, DateOnly updatedPickUpDate)
        => Task.Run(async () => await PutAsync<object>($"{BaseUri}/{orderIdToModify}", new { updatedQuantities, updatedPickUpDate })).GetAwaiter().GetResult();

    public void ResubmitExpiredOrder(int orderIdToResubmit, DateOnly chosenPickUpDate)
        => Task.Run(async () => await PostAsync<object, object>($"{BaseUri}/{orderIdToResubmit}/resubmit", new { chosenPickUpDate })).GetAwaiter().GetResult();

    public void CancelOrder(int orderId)
        => Task.Run(async () => await PostAsync<object, object>($"{BaseUri}/{orderId}/cancel", new { })).GetAwaiter().GetResult();

    public void ExpireOverdueOrders()
        => Task.Run(async () => await PostAsync<object, object>($"{BaseUri}/expire-overdue", new { })).GetAwaiter().GetResult();

    public void AddToBasket(int itemId, int quantityToBuy)
        => Task.Run(async () => await PostAsync<object, object>("api/basket/add", new { itemId, quantityToBuy })).GetAwaiter().GetResult();

    public void AddItemToBasket(int itemId, int quantityToBuy, float extraDiscountPercentage = 0f)
        => Task.Run(async () => await PostAsync<object, object>("api/basket/add", new { itemId, quantityToBuy, extraDiscountPercentage })).GetAwaiter().GetResult();

    public void UpdateBasketItemQuantity(int itemId, int newQuantityToBuy)
        => Task.Run(async () => await PutAsync<object>("api/basket/update", new { itemId, newQuantityToBuy })).GetAwaiter().GetResult();

    public void RemoveFromBasket(int itemIdToRemove)
        => Task.Run(async () => await DeleteAsync($"api/basket/{itemIdToRemove}")).GetAwaiter().GetResult();

    public Dictionary<int, int> FillBasketFromPrescription(string prescriptionId)
        => Task.Run(async () => await PostAsync<object, Dictionary<int, int>>($"api/basket/fill-prescription", new { prescriptionId }) ?? []).GetAwaiter().GetResult();

    public List<BasketItemViewModel> GetBasketItems()
        => [];

    public void ApplyPrescriptionToBasket(string prescriptionId)
        => Task.Run(async () => await PostAsync<object, object>("api/basket/apply-prescription", new { prescriptionId })).GetAwaiter().GetResult();

    public void RecalculateBasketItemPrices(BasketItemViewModel basketItem)
    {
    }

    public Tuple<float, float> CalculateBasketTotalSum(IEnumerable<BasketItemViewModel> basketItems)
        => new Tuple<float, float>(0f, 0f);
}
