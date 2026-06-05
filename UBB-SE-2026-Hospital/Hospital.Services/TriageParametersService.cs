using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services;

public class TriageParametersService(ITriageParametersRepository triageParametersRepository) : ITriageParametersService
{
    public Task<List<TriageParameters>> GetAllAsync()
        => triageParametersRepository.GetAllAsync();

    public Task<TriageParameters?> GetByIdAsync(int id)
        => triageParametersRepository.GetByIdAsync(id);

    public Task<TriageParameters?> GetByTriageIdAsync(int triageId)
        => triageParametersRepository.GetByTriageIdAsync(triageId);

    public Task<TriageParameters> CreateAsync(TriageParameters parameters)
    {
        parameters.ValidateParameters();
        return triageParametersRepository.CreateAsync(parameters);
    }

    public async Task<TriageParameters> UpdateAsync(TriageParameters parameters)
    {
        parameters.ValidateParameters();

        TriageParameters current = await triageParametersRepository.GetByIdAsync(parameters.TriageParametersId)
            ?? throw new ArgumentException($"Triage parameters {parameters.TriageParametersId} were not found.");

        if (parameters.Triage is not null)
        {
            current.Triage = parameters.Triage;
        }

        current.Consciousness = parameters.Consciousness;
        current.Breathing = parameters.Breathing;
        current.Bleeding = parameters.Bleeding;
        current.InjuryType = parameters.InjuryType;
        current.PainLevel = parameters.PainLevel;

        return await triageParametersRepository.UpdateAsync(current);
    }

    public Task DeleteAsync(int id)
        => triageParametersRepository.DeleteAsync(id);
}
