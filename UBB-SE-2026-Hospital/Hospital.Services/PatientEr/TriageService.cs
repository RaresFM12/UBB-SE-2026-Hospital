using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services.PatientEr;

public class TriageService(ITriageRepository triageRepository) : ITriageService
{
    public Task<List<Triage>> GetAllAsync()
        => triageRepository.GetAllAsync();

    public Task<Triage?> GetByIdAsync(int id)
        => triageRepository.GetByIdAsync(id);

    public Task<Triage> CreateAsync(Triage triage)
        => triageRepository.CreateAsync(triage);

    public async Task<Triage> UpdateAsync(Triage triage)
    {
        Triage current = await triageRepository.GetByIdAsync(triage.TriageId)
            ?? throw new ArgumentException($"Triage {triage.TriageId} was not found.");

        if (triage.Visit is not null)
        {
            current.Visit = triage.Visit;
        }

        current.TriageLevel = triage.TriageLevel;
        current.Specialization = triage.Specialization;
        current.NurseId = triage.NurseId;
        current.TriageTime = triage.TriageTime;

        return await triageRepository.UpdateAsync(current);
    }

    public Task DeleteAsync(int id)
        => triageRepository.DeleteAsync(id);
}
