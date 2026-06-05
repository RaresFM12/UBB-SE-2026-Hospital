using Hospital.Data.Repositories;
using Hospital.Services.PatientEr;
using Hospital.Shared.Services;
using System;

namespace Hospital.Services.StaffPharmacy
{
    public class PeriodTrackerServiceFactory : IPeriodTrackerServiceFactory
    {
        private readonly IUsersRepository usersRepository;
        private readonly IItemsRepository itemsRepository;
        private readonly IOrderService orderService;

        public PeriodTrackerServiceFactory(
            IUsersRepository usersRepository,
            IItemsRepository itemsRepository,
            ICurrentUserService currentUserService,
            IOrderService orderService)
        {
            this.usersRepository = usersRepository;
            this.itemsRepository = itemsRepository;
            this.orderService = orderService;
        }

        public IPeriodTrackerService CreatePeriodTrackerService()
        {
            return new PeriodTrackerService(this.usersRepository, CreateWellnessItemsService());
        }

        public IWellnessItemsService CreateWellnessItemsService()
        {
            return new WellnessItemsService(this.itemsRepository);
        }

        public IBasketService CreateBasketService()
        {
            throw new NotImplementedException("BasketService is resolved via DI, not this factory.");
        }
    }
}