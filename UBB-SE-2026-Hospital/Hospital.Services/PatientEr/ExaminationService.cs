using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services.PatientEr;

public class ExaminationService(
    IExaminationRepository examinationRepository,
    IERVisitRepository erVisitRepository,
    IERRoomRepository erRoomRepository,
    ITriageRepository triageRepository,
    ITriageParametersRepository triageParametersRepository) : IExaminationService
{
    public Task<List<Examination>> GetAllAsync()
        => examinationRepository.GetAllAsync();

    public Task<Examination?> GetByIdAsync(int id)
        => examinationRepository.GetByIdAsync(id);

    public Task<List<Examination>> GetByVisitIdAsync(int visitId)
        => examinationRepository.GetByVisitIdAsync(visitId);

    public Task<Examination> CreateAsync(Examination examination)
        => examinationRepository.CreateAsync(examination);

    public async Task<Examination> UpdateAsync(Examination examination)
    {
        Examination current = await examinationRepository.GetByIdAsync(examination.ExaminationId)
            ?? throw new ArgumentException($"Examination {examination.ExaminationId} was not found.");

        if (examination.Visit is not null)
        {
            current.Visit = examination.Visit;
        }

        if (examination.Doctor is not null)
        {
            current.Doctor = examination.Doctor;
        }

        if (examination.Room is not null)
        {
            current.Room = examination.Room;
        }

        current.ExaminationDate = examination.ExaminationDate;
        current.Findings = examination.Findings;
        current.Recommendation = examination.Recommendation;

        return await examinationRepository.UpdateAsync(current);
    }

    public Task DeleteAsync(int id)
        => examinationRepository.DeleteAsync(id);

    public async Task<List<ERVisit>> GetEligibleVisitsAsync()
    {
        HashSet<int> roomLinkedVisitIds = (await erRoomRepository.GetAllAsync())
            .Where(room => room.CurrentVisit is not null)
            .Select(room => room.CurrentVisit!.VisitId)
            .ToHashSet();

        List<ERVisit> visits = await erVisitRepository.GetAllAsync();

        return visits
            .Where(visit =>
                string.Equals(visit.Status, ERVisit.VisitStatus.IN_ROOM, StringComparison.OrdinalIgnoreCase)
                || string.Equals(visit.Status, ERVisit.VisitStatus.WAITING_FOR_DOCTOR, StringComparison.OrdinalIgnoreCase))
            .Where(visit => !string.Equals(visit.Status, ERVisit.VisitStatus.IN_ROOM, StringComparison.OrdinalIgnoreCase)
                || roomLinkedVisitIds.Contains(visit.VisitId))
            .OrderBy(visit => visit.ArrivalDateTime)
            .ToList();
    }

    public async Task<List<Examination>> GetPatientHistoryAsync(int patientId)
    {
        List<ERVisit> patientVisits = await erVisitRepository.GetByPatientIdAsync(patientId);
        var examinations = new List<Examination>();

        foreach (ERVisit visit in patientVisits)
        {
            examinations.AddRange(await examinationRepository.GetByVisitIdAsync(visit.VisitId));
        }

        return examinations
            .OrderByDescending(examination => examination.ExaminationDate)
            .ToList();
    }

    public async Task<ERExaminationSummary?> GetSummaryByVisitIdAsync(int visitId)
    {
        Examination? examination = (await examinationRepository.GetByVisitIdAsync(visitId))
            .OrderByDescending(item => item.ExaminationDate)
            .FirstOrDefault();
        if (examination is null)
        {
            return null;
        }

        ERVisit? visit = await erVisitRepository.GetByIdAsync(visitId);
        Triage? triage = await triageRepository.GetByVisitIdAsync(visitId);
        if (visit is null || visit.Patient is null || triage is null)
        {
            return null;
        }

        TriageParameters? parameters = await triageParametersRepository.GetByTriageIdAsync(triage.TriageId);
        if (parameters is null)
        {
            return null;
        }

        return new ERExaminationSummary
        {
            FirstName = visit.Patient.FirstName,
            LastName = visit.Patient.LastName,
            ArrivalDateTime = visit.ArrivalDateTime,
            ChiefComplaint = visit.ChiefComplaint,
            TriageLevel = triage.TriageLevel,
            Specialization = triage.Specialization,
            Consciousness = parameters.Consciousness,
            Breathing = parameters.Breathing,
            Bleeding = parameters.Bleeding,
            InjuryType = parameters.InjuryType,
            PainLevel = parameters.PainLevel,
            DoctorId = examination.Doctor?.StaffId ?? 0,
            ExamTime = examination.ExaminationDate,
            Notes = examination.Findings,
            AssignedDoctorName = examination.Doctor?.FullName ?? string.Empty,
        };
    }
}
