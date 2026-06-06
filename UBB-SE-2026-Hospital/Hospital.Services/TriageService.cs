using Hospital.Data.Models;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services;

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

    public async Task<Triage> CreateTriageAsync(int visitId, PerformTriageRequest request)
    {
        ERVisit visit = await erVisitRepository.GetByIdAsync(visitId)
            ?? throw new ArgumentException($"ER visit {visitId} was not found.");

        if (!string.Equals(visit.Status, ERVisit.VisitStatus.REGISTERED, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"ER visit {visitId} cannot be triaged while it is in {visit.Status} status.");
        }

        Triage? existingTriage = await triageRepository.GetByVisitIdAsync(visitId);
        if (existingTriage is not null)
        {
            // Visit status is authoritative. Seeded databases may contain stale
            // triage data for visits that are still REGISTERED.
            await triageRepository.DeleteAsync(existingTriage.TriageId);
        }

        var triage = new Triage
        {
            Visit = visit,
            NurseId = request.NurseId,
            TriageTime = request.TriageTime,
        };
        TriageParameters parameters = request.ToParameters(triage);
        parameters.ValidateParameters();
        triage.TriageLevel = request.TriageLevel is >= 1 and <= 5
            ? request.TriageLevel
            : triageDecisionService.CalculateTriageLevel(parameters);
        triage.Specialization = string.IsNullOrWhiteSpace(request.Specialization)
            ? triageDecisionService.DetermineSpecialization(parameters)
            : request.Specialization;

        Triage created = await triageRepository.CreateAsync(triage);
        parameters.Triage = created;
        await triageParametersRepository.CreateAsync(parameters);

        visit.Status = ERVisit.VisitStatus.TRIAGED;
        await erVisitRepository.UpdateAsync(visit);
        return created;
    }

    public async Task MoveVisitToQueueAsync(int visitId)
    {
        ERVisit visit = await erVisitRepository.GetByIdAsync(visitId)
            ?? throw new ArgumentException($"ER visit {visitId} was not found.");

        if (!string.Equals(visit.Status, ERVisit.VisitStatus.TRIAGED, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"ER visit {visitId} must be TRIAGED before it can move to the room queue.");
        }

        Triage triage = await triageRepository.GetByVisitIdAsync(visitId)
            ?? throw new InvalidOperationException($"Triage was not found for visit {visitId}.");
        if (await triageParametersRepository.GetByTriageIdAsync(triage.TriageId) is null)
        {
            throw new InvalidOperationException($"Triage parameters were not found for visit {visitId}.");
        }

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
            .Where(visit => string.Equals(
                visit.Status,
                ERVisit.VisitStatus.REGISTERED,
                StringComparison.OrdinalIgnoreCase))
            .OrderBy(visit => visit.ArrivalDateTime)
            .ToList();
}
