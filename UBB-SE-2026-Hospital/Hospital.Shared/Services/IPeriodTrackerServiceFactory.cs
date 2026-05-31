namespace Hospital.Shared.Services;

public interface IPeriodTrackerServiceFactory
{
    IPeriodTrackerService CreatePeriodTrackerService();

    IWellnessItemsService CreateWellnessItemsService();

    IBasketService CreateBasketService();
}
