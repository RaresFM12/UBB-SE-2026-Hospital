using Hospital.Data.Models;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services;

public class ExaminationService(
    IExaminationRepository examinationRepository,
    IERVisitRepository erVisitRepository,
    IERRoomRepository erRoomRepository,
    ITriageRepository triageRepository,
    ITriageParametersRepository triageParametersRepository,
    IStaffRepository staffRepository) : IExaminationService
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
                || string.Equals(visit.Status, ERVisit.VisitStatus.WAITING_FOR_DOCTOR, StringComparison.OrdinalIgnoreCase)
                || string.Equals(visit.Status, ERVisit.VisitStatus.IN_EXAMINATION, StringComparison.OrdinalIgnoreCase))
            .Where(visit => !string.Equals(visit.Status, ERVisit.VisitStatus.IN_ROOM, StringComparison.OrdinalIgnoreCase)
                || roomLinkedVisitIds.Contains(visit.VisitId))
            .OrderBy(visit => visit.ArrivalDateTime)
            .ToList();
    }

    public async Task<Examination> RequestDoctorAsync(int visitId)
    {
        ERVisit visit = await erVisitRepository.GetByIdAsync(visitId)
            ?? throw new ArgumentException($"ER visit {visitId} was not found.");

        bool isInRoom = string.Equals(
            visit.Status,
            ERVisit.VisitStatus.IN_ROOM,
            StringComparison.OrdinalIgnoreCase);
        bool isWaitingForDoctor = string.Equals(
            visit.Status,
            ERVisit.VisitStatus.WAITING_FOR_DOCTOR,
            StringComparison.OrdinalIgnoreCase);
        if (!isInRoom && !isWaitingForDoctor)
        {
            throw new InvalidOperationException(
                $"ER visit {visitId} must be IN_ROOM or WAITING_FOR_DOCTOR before requesting a doctor.");
        }

        ERRoom? room = (await erRoomRepository.GetAllAsync()).FirstOrDefault(room =>
            ERRoom.StatusEquals(room.AvailabilityStatus, ERRoom.RoomStatus.Occupied)
            && room.CurrentVisit?.VisitId == visitId);
        if (room is null)
        {
            throw new InvalidOperationException(
                $"ER visit {visitId} is not linked to an occupied room.");
        }

        Triage triage = await triageRepository.GetByVisitIdAsync(visitId)
            ?? throw new InvalidOperationException($"Triage was not found for visit {visitId}.");
        if (await triageParametersRepository.GetByTriageIdAsync(triage.TriageId) is null)
        {
            throw new InvalidOperationException(
                $"Triage parameters were not found for visit {visitId}.");
        }

        Examination? existingAssignment = (await examinationRepository.GetByVisitIdAsync(visitId))
            .OrderByDescending(examination => examination.ExaminationDate)
            .FirstOrDefault();
        if (existingAssignment is not null)
        {
            visit.Status = ERVisit.VisitStatus.WAITING_FOR_DOCTOR;
            await erVisitRepository.UpdateAsync(visit);
            return existingAssignment;
        }

        Doctor doctor = SelectDoctor(await staffRepository.GetAllDoctorsAsync(), triage.Specialization)
            ?? throw new InvalidOperationException(
                $"No database doctor is available for {triage.Specialization}.");

        var assignment = new Examination
        {
            Visit = visit,
            Doctor = doctor,
            Room = room,
            ExaminationDate = DateTime.Now,
            Findings = string.Empty,
            Recommendation = string.Empty,
        };
        Examination created = await examinationRepository.CreateAsync(assignment);

        doctor.DoctorStatus = DoctorStatus.InExamination;
        await staffRepository.UpdateAsync(doctor);
        visit.Status = ERVisit.VisitStatus.WAITING_FOR_DOCTOR;
        await erVisitRepository.UpdateAsync(visit);
        return created;
    }

    private static Doctor? SelectDoctor(IEnumerable<Doctor> doctors, string specialization)
    {
        string requested = NormalizeSpecialization(specialization);
        string fallback = requested switch
        {
            "neurology" or "pulmonology" or "general medicine" or "general" => "diagnostician",
            "orthopedics" or "general surgery" or "surgery" => "surgery",
            _ => "diagnostician",
        };

        return doctors
            .Where(doctor => doctor.DoctorStatus == DoctorStatus.Available)
            .OrderBy(doctor =>
                NormalizeSpecialization(doctor.Specialization) == requested ? 0 :
                NormalizeSpecialization(doctor.Specialization) == fallback ? 1 : 2)
            .ThenBy(doctor => doctor.StaffId)
            .FirstOrDefault(doctor =>
                NormalizeSpecialization(doctor.Specialization) == requested
                || NormalizeSpecialization(doctor.Specialization) == fallback);
    }

    private static string NormalizeSpecialization(string? specialization) =>
        (specialization ?? string.Empty).Trim().ToLowerInvariant();

    public async Task<Examination> SaveExaminationAsync(int visitId, string notes)
    {
        ERVisit visit = await erVisitRepository.GetByIdAsync(visitId)
            ?? throw new ArgumentException($"ER visit {visitId} was not found.");

        bool canSave =
            string.Equals(visit.Status, ERVisit.VisitStatus.WAITING_FOR_DOCTOR, StringComparison.OrdinalIgnoreCase)
            || string.Equals(visit.Status, ERVisit.VisitStatus.IN_EXAMINATION, StringComparison.OrdinalIgnoreCase);
        if (!canSave)
        {
            throw new InvalidOperationException(
                $"ER visit {visitId} must be WAITING_FOR_DOCTOR or IN_EXAMINATION before saving.");
        }

        if (string.IsNullOrWhiteSpace(notes))
        {
            throw new ArgumentException("Examination notes are required.");
        }

        Examination examination = (await examinationRepository.GetByVisitIdAsync(visitId))
            .OrderByDescending(item => item.ExaminationDate)
            .FirstOrDefault()
            ?? throw new InvalidOperationException(
                $"No doctor assignment was found for visit {visitId}.");

        examination.ExaminationDate = DateTime.Now;
        examination.Findings = notes.Trim();
        Examination updated = await examinationRepository.UpdateAsync(examination);

        visit.Status = ERVisit.VisitStatus.IN_EXAMINATION;
        await erVisitRepository.UpdateAsync(visit);

        if (examination.Doctor is Doctor doctor &&
            doctor.DoctorStatus == DoctorStatus.InExamination)
        {
            doctor.DoctorStatus = DoctorStatus.Available;
            await staffRepository.UpdateAsync(doctor);
        }

        return updated;
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
            .Where(item => !string.IsNullOrWhiteSpace(item.Findings))
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
