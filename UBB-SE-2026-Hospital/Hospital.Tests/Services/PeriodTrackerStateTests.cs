using Hospital.Services;

namespace Hospital.Tests.Services;

[TestClass]
public class PeriodTrackerStateTests
{
    private const int CycleDays = 28;
    private const int PeriodLasts = 5;
    private const int PmsOption = 1;

    [TestMethod]
    public void PeriodTrackerState_RetainsAssignedValues()
    {
        var state = new PeriodTrackerState
        {
            StartPeriodDate = DateTimeOffset.UnixEpoch,
            CycleDays = CycleDays,
            PeriodLasts = PeriodLasts,
            PremenstrualSyndromeOption = PmsOption,
            HasPeriodTracker = true,
        };

        Assert.AreEqual(CycleDays, state.CycleDays);
    }
}
