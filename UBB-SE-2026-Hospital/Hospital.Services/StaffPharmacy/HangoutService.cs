#if false
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services.StaffPharmacy;

public class HangoutService(
    IHangoutRepository hangoutRepository,
    IHangoutParticipantRepository hangoutParticipantRepository,
    IAppointmentRepository appointmentRepository,
    IStaffRepository staffRepository,
    IEvaluationsRepository evaluationsRepository) : IHangoutService
{
    private const int MinimumHangoutTitleLength = 5;
    private const int MaximumHangoutTitleLength = 25;
    private const int MaximumHangoutDescriptionLength = 100;
    private const int MinDaysAheadForHangout = 7;

    public async Task<IReadOnlyList<Hangout>> GetAllHangoutsAsync(CancellationToken cancellationToken = default)
    {
        var hangouts = await hangoutRepository.GetAllAsync();
        var participants = await hangoutParticipantRepository.GetByHangoutIdAsync(0);
        var allParticipants = await GetAllParticipantsAsync(cancellationToken);

        foreach (var hangout in hangouts)
        {
            hangout.ParticipantList = allParticipants
                .Where(participant => participant.Hangout.HangoutID == hangout.HangoutID)
                .Select(participant => (IStaff)participant.Staff)
                .ToList();
        }

        return hangouts;
    }

    public async Task<Hangout?> GetHangoutByIdAsync(int hangoutId, CancellationToken cancellationToken = default)
        => await hangoutRepository.GetByIdAsync(hangoutId);

    public async Task<int> CreateHangoutAsync(string title, string description, DateTime date, int maxParticipants, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length < MinimumHangoutTitleLength || title.Length > MaximumHangoutTitleLength)
        {
            throw new ArgumentException($"Title must be between {MinimumHangoutTitleLength} and {MaximumHangoutTitleLength} characters.");
        }

        if (description.Length > MaximumHangoutDescriptionLength)
        {
            throw new ArgumentException($"Description must be at most {MaximumHangoutDescriptionLength} characters.");
        }

        if (date.Date < DateTime.Now.Date.AddDays(MinDaysAheadForHangout))
        {
            throw new ArgumentException("The hangout date must be at least 1 week away from today.");
        }

        var hangout = await hangoutRepository.CreateAsync(new Hangout
        {
            Title = title,
            Description = description,
            Date = date,
            MaxParticipants = maxParticipants,
        });

        return hangout.HangoutID;
    }

    public async Task<IReadOnlyList<HangoutParticipant>> GetAllParticipantsAsync(CancellationToken cancellationToken = default)
    {
        var hangouts = await hangoutRepository.GetAllAsync();
        var participants = new List<HangoutParticipant>();
        foreach (var hangout in hangouts)
        {
            participants.AddRange(await hangoutParticipantRepository.GetByHangoutIdAsync(hangout.HangoutID));
        }

        return participants;
    }

    public async Task AddParticipantAsync(int hangoutId, int staffId, CancellationToken cancellationToken = default)
    {
        var hangout = await hangoutRepository.GetByIdAsync(hangoutId)
            ?? throw new ArgumentException("Hangout not found.");
        var staff = await staffRepository.GetByIdAsync(staffId)
            ?? throw new ArgumentException("Staff member not found.");

        var existingParticipants = await hangoutParticipantRepository.GetByHangoutIdAsync(hangoutId);
        if (existingParticipants.Count >= hangout.MaxParticipants)
        {
            throw new InvalidOperationException("This hangout is already full.");
        }

        if (existingParticipants.Any(participant => participant.Staff.StaffId == staffId))
        {
            throw new InvalidOperationException("You have already joined this hangout.");
        }

        if (await HasConflictingAppointmentOnDateAsync(staffId, hangout.Date) || await HasMedicalEvaluationOnDateAsync(staffId, hangout.Date))
        {
            throw new InvalidOperationException("You cannot join a hangout on a day where you have active medical work scheduled.");
        }

        await hangoutParticipantRepository.CreateAsync(new HangoutParticipant
        {
            Hangout = hangout,
            Staff = staff,
        });
    }

    private async Task<bool> HasConflictingAppointmentOnDateAsync(int staffId, DateTime date)
        => (await appointmentRepository.GetByDoctorIdAsync(staffId))
            .Any(appointment =>
                appointment.AppointmentDate.Date == date.Date &&
                !string.Equals(appointment.Status, "Finished", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(appointment.Status, "Canceled", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(appointment.Status, "Cancelled", StringComparison.OrdinalIgnoreCase));

    private async Task<bool> HasMedicalEvaluationOnDateAsync(int staffId, DateTime date)
        => (await evaluationsRepository.GetByDoctorIdAsync(staffId))
            .Any(evaluation => evaluation.EvaluationDate.Date == date.Date);
    public int CreateHangout(string title, string description, DateTime date, int maxParticipants, IStaff creator) { throw new System.NotImplementedException(); }
    public void JoinHangout(int hangoutId, IStaff staff) { throw new System.NotImplementedException(); }
    public List<Hangout> GetAllHangouts() { throw new System.NotImplementedException(); }
    public int CreateHangout(string title, string description, DateTime date, int maxParticipants, IStaff creator) { throw new System.NotImplementedException(); }
    public void JoinHangout(int hangoutId, IStaff staff) { throw new System.NotImplementedException(); }
    public int CreateHangout(string title, string description, DateTime date, int maxParticipants, IStaff creator) { throw new System.NotImplementedException(); }
    public void JoinHangout(int hangoutId, IStaff staff) { throw new System.NotImplementedException(); }
    public int CreateHangout(string title, string description, DateTime date, int maxParticipants, IStaff creator) { throw new System.NotImplementedException(); }
    public void JoinHangout(int hangoutId, IStaff staff) { throw new System.NotImplementedException(); }
    public int CreateHangout(string title, string description, DateTime date, int maxParticipants, IStaff creator) { throw new System.NotImplementedException(); }
    public void JoinHangout(int hangoutId, IStaff staff) { throw new System.NotImplementedException(); }
    public int CreateHangout(string title, string description, DateTime date, int maxParticipants, IStaff creator) { throw new System.NotImplementedException(); }
    public void JoinHangout(int hangoutId, IStaff staff) { throw new System.NotImplementedException(); }
    public int CreateHangout(string title, string description, DateTime date, int maxParticipants, IStaff creator) { throw new System.NotImplementedException(); }
    public void JoinHangout(int hangoutId, IStaff staff) { throw new System.NotImplementedException(); }
    public int CreateHangout(string title, string description, DateTime date, int maxParticipants, IStaff creator) { throw new System.NotImplementedException(); }
    public void JoinHangout(int hangoutId, IStaff staff) { throw new System.NotImplementedException(); }
    public int CreateHangout(string title, string description, DateTime date, int maxParticipants, IStaff creator) { throw new System.NotImplementedException(); }
    public void JoinHangout(int hangoutId, IStaff staff) { throw new System.NotImplementedException(); }
    public int CreateHangout(string title, string description, DateTime date, int maxParticipants, IStaff creator) { throw new System.NotImplementedException(); }
    public void JoinHangout(int hangoutId, IStaff staff) { throw new System.NotImplementedException(); }
    public int CreateHangout(string title, string description, DateTime date, int maxParticipants, IStaff creator) { throw new System.NotImplementedException(); }
    public void JoinHangout(int hangoutId, IStaff staff) { throw new System.NotImplementedException(); }
}
#endif
