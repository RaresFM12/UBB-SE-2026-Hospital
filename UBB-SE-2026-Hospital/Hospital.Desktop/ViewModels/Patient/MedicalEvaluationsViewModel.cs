using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.ViewModels.Patient;

public partial class MedicalEvaluationsViewModel : ObservableObject
{
    private readonly IMedicalEvaluationService evaluationService;

    [ObservableProperty] private ObservableCollection<MedicalEvaluation> evaluations = new();
    [ObservableProperty] private string statusMessage = string.Empty;

    public MedicalEvaluationsViewModel(IMedicalEvaluationService evaluationService)
    {
        this.evaluationService = evaluationService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        StatusMessage = string.Empty;
        try
        {
            var items = await evaluationService.GetAllEvaluationsAsync();
            Evaluations = new ObservableCollection<MedicalEvaluation>(items);
            StatusMessage = $"Loaded {Evaluations.Count} evaluation(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(MedicalEvaluation? evaluation)
    {
        if (evaluation is null)
        {
            return;
        }

        StatusMessage = string.Empty;
        try
        {
            await evaluationService.DeleteEvaluationAsync(evaluation.EvaluationID);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}
