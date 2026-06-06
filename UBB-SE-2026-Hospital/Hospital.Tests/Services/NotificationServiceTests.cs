using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class NotificationServiceTests
{
    private const int StaffId = 7;
    private const string Title = "Reminder";
    private const string Message = "Your shift starts soon.";

    private static (NotificationService Service, INotificationRepository Notifications, IStaffRepository Staff) CreateService()
    {
        var notifications = Substitute.For<INotificationRepository>();
        var staff = Substitute.For<IStaffRepository>();
        return (new NotificationService(notifications, staff), notifications, staff);
    }

    [TestMethod]
    public async Task CreateNotificationAsync_StaffNotFound_ThrowsArgumentException()
    {
        var (service, _, staff) = CreateService();
        staff.GetByIdAsync(StaffId).Returns((Staff?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.CreateNotificationAsync(StaffId, Title, Message));
    }

    [TestMethod]
    public async Task CreateNotificationAsync_ValidStaff_PersistsNotificationWithTitle()
    {
        var (service, notifications, staff) = CreateService();
        staff.GetByIdAsync(StaffId).Returns(new Staff { StaffId = StaffId });

        await service.CreateNotificationAsync(StaffId, Title, Message);

        await notifications.Received().CreateAsync(Arg.Is<Notification>(notification => notification.Title == Title));
    }

    [TestMethod]
    public async Task GetNotificationsForStaffAsync_ReturnsRepositoryResult()
    {
        var (service, notifications, _) = CreateService();
        notifications.GetByStaffIdAsync(StaffId).Returns(new List<Notification> { new() { Title = Title } });

        var result = await service.GetNotificationsForStaffAsync(StaffId);

        Assert.AreEqual(Title, result[0].Title);
    }
}
