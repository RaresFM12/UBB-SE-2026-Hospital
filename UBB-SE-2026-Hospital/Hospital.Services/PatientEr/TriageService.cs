using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services.PatientEr;

public class TriageService(
    ITriageRepository triageRepository,
    IERVisitRepository erVisitRepository,
    ITriageParametersRepository triageParametersRepository,
    ITriageDecisionService triageDecisionService) : ITriageService
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

    public Task<Triage?> GetByVisitIdAsync(int visitId)
        => triageRepository.GetByVisitIdAsync(visitId);

    public async Task<Triage> CreateTriageAsync(int visitId, PerformTriageDto parameters)
    {
        ERVisit visit = await erVisitRepository.GetByIdAsync(visitId)
            ?? throw new ArgumentException($"ER visit {visitId} was not found.");

        Triage? existingTriage = await triageRepository.GetByVisitIdAsync(visitId);
        if (existingTriage is not null)
        {
            throw new InvalidOperationException($"Visit {visitId} already has a triage record.");
        }

        var triageParameters = new TriageParameters
        {
            Consciousness = parameters.Consciousness,
            Breathing = parameters.Breathing,
            Bleeding = parameters.Bleeding,
            InjuryType = parameters.InjuryType,
            PainLevel = parameters.PainLevel,
        };
        triageParameters.ValidateParameters();

        var triage = new Triage
        {
            Visit = visit,
            TriageLevel = parameters.TriageLevel > 0
                ? parameters.TriageLevel
                : triageDecisionService.CalculateTriageLevel(triageParameters),
            Specialization = string.IsNullOrWhiteSpace(parameters.Specialization)
                ? triageDecisionService.DetermineSpecialization(triageParameters)
                : parameters.Specialization,
            NurseId = parameters.NurseId,
            TriageTime = parameters.TriageTime == default ? DateTime.Now : parameters.TriageTime,
        };

        Triage created = await triageRepository.CreateAsync(triage);
        triageParameters.Triage = created;
        await triageParametersRepository.CreateAsync(triageParameters);

        visit.Status = ERVisit.VisitStatus.TRIAGED;
        await erVisitRepository.UpdateAsync(visit);

        return created;
    }

    public async Task MoveVisitToQueueAsync(int visitId)
    {
        ERVisit visit = await erVisitRepository.GetByIdAsync(visitId)
            ?? throw new ArgumentException($"ER visit {visitId} was not found.");

        visit.Status = ERVisit.VisitStatus.WAITING_FOR_ROOM;
        await erVisitRepository.UpdateAsync(visit);
    }

    public async Task CloseVisitAsync(int visitId)
    {
        ERVisit visit = await erVisitRepository.GetByIdAsync(visitId)
            ?? throw new ArgumentException($"ER visit {visitId} was not found.");

        visit.Status = ERVisit.VisitStatus.CLOSED;
        await erVisitRepository.UpdateAsync(visit);
    }

    public async Task<List<ERVisit>> GetVisitsForTriageAsync()
        => (await erVisitRepository.GetAllAsync())
            .Where(visit =>
                string.Equals(visit.Status, ERVisit.VisitStatus.REGISTERED, StringComparison.OrdinalIgnoreCase)
                || string.Equals(visit.Status, ERVisit.VisitStatus.TRIAGED, StringComparison.OrdinalIgnoreCase))
            .OrderBy(visit => string.Equals(visit.Status, ERVisit.VisitStatus.TRIAGED, StringComparison.OrdinalIgnoreCase))
            .ThenBy(visit => visit.ArrivalDateTime)
            .ToList();
}
