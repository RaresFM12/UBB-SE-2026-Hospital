using Hospital.Shared.Proxies;
using Hospital.Shared.Services;
using Hospital.Web.Models.Statistics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

[Authorize(Roles = "Admin")]
public class StatisticsController : Controller
{
    private readonly IStatisticsApiClient statisticsService;

    public StatisticsController(IStatisticsApiClient statisticsService)
    {
        this.statisticsService = statisticsService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? type, CancellationToken cancellationToken = default)
    {
        StatisticsType selectedType = StatisticsViewModel.FromKey(type);
        StatisticsModel model = await BuildModelAsync(selectedType, cancellationToken);
        return View(StatisticsViewModel.FromModel(model));
    }

    private async Task<StatisticsModel> BuildModelAsync(StatisticsType type, CancellationToken cancellationToken)
    {
        var model = new StatisticsModel { SelectedType = type };

        try
        {
            switch (type)
            {
                case StatisticsType.ConsultationSource:
                    model.PrimaryData = await statisticsService.GetConsultationDistributionAsync();
                    break;
                case StatisticsType.TopDiagnoses:
                    model.PrimaryData = await statisticsService.GetTopDiagnosesAsync();
                    break;
                case StatisticsType.TopMedications:
                    model.PrimaryData = await statisticsService.GetMostPrescribedMedsAsync();
                    break;
                case StatisticsType.Demographics:
                    model.PrimaryData = await statisticsService.GetPatientGenderDistributionAsync();
                    model.SecondaryData = await statisticsService.GetAgeDistributionAsync();
                    break;
                default:
                    model.PrimaryData = await statisticsService.GetActiveVsArchivedRatioAsync();
                    break;
            }
        }
        catch (Exception ex)
        {
            model.ErrorMessage = $"Could not load statistics: {ex.Message}";
        }

        return model;
    }
}

