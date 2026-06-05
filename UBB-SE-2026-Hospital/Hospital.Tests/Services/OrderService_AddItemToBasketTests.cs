using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Services;
using Hospital.Data.Repositories;
using Hospital.Data.Models;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class OrderService_AddItemToBasketTests
    {
        [TestMethod]
        public async Task AddItemToBasket_NonPositiveQuantity_ThrowsArgument()
        {
            var mockItemsRepo = new Mock<IItemsRepository>();
            var mockUsersRepo = new Mock<IUsersRepository>();
            var mockBasketRepo = new Mock<IBasketRepository>();
            var mockOrdersRepo = new Mock<IOrdersRepository>();
            var mockPresRepo = new Mock<IPrescriptionRepository>();

            var service = new OrderService(mockOrdersRepo.Object, mockItemsRepo.Object, mockUsersRepo.Object, mockBasketRepo.Object, mockPresRepo.Object);

            Assert.ThrowsException<ArgumentException>(() => service.AddItemToBasketAsync(1, 2, 0).GetAwaiter().GetResult());
        }

        [TestMethod]
        public async Task AddItemToBasket_UserNotFound_ThrowsArgument()
        {
            var mockItemsRepo = new Mock<IItemsRepository>();
            var mockUsersRepo = new Mock<IUsersRepository>();
            var mockBasketRepo = new Mock<IBasketRepository>();
            var mockOrdersRepo = new Mock<IOrdersRepository>();
            var mockPresRepo = new Mock<IPrescriptionRepository>();

            mockUsersRepo.Setup(r => r.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync((User?) null);
            var service = new OrderService(mockOrdersRepo.Object, mockItemsRepo.Object, mockUsersRepo.Object, mockBasketRepo.Object, mockPresRepo.Object);

            Assert.ThrowsException<ArgumentException>(() => service.AddItemToBasketAsync(99, 2, 1).GetAwaiter().GetResult());
        }

        [TestMethod]
        public async Task AddItemToBasket_ItemNotFound_ThrowsArgument()
        {
            var mockItemsRepo = new Mock<IItemsRepository>();
            var mockUsersRepo = new Mock<IUsersRepository>();
            var mockBasketRepo = new Mock<IBasketRepository>();
            var mockOrdersRepo = new Mock<IOrdersRepository>();
            var mockPresRepo = new Mock<IPrescriptionRepository>();

            mockUsersRepo.Setup(r => r.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(new User());
            mockItemsRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Item?)null);

            var service = new OrderService(mockOrdersRepo.Object, mockItemsRepo.Object, mockUsersRepo.Object, mockBasketRepo.Object, mockPresRepo.Object);

            Assert.ThrowsException<ArgumentException>(() => service.AddItemToBasketAsync(1, 123, 1).GetAwaiter().GetResult());
        }
    }
}
