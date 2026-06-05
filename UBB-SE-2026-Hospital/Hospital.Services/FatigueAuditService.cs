using System;
using System.Collections.Generic;
using System.Linq;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services;

public class FatigueAuditService(
    IShiftRepository shiftRepository,
    IStaffRepository staffRepository) : IFatigueAuditService
{
    private const double MaximumWeeklyHours = 60.0;
    private const string DoctorRole = "Doctor";
    private const string PharmacistRole = "Pharmacist";
    private const string DefaultSpecialization = "General";
    private const string CancelledShiftStatus = "CANCELLED";
    private const string InactiveStaffStatus = "INACTIVE";
    private const string AvailableStaffStatus = "AVAILABLE";
    private const string MaximumWeeklyHoursViolationRule = "MAX_60H_PER_WEEK";
    private const string MinimumRestViolationRule = "MIN_12H_REST";
    private const int DaysInWeek = 7;
    private const int MinimumValidId = 1;
    private static readonly TimeSpan MinimumRestGap = TimeSpan.FromHours(12);

    public bool ReassignShift(int shiftId, int newStaffId)
    {
        if (shiftId < MinimumValidId || newStaffId < MinimumValidId)
        {
            return false;
        }

        var shift = shiftRepository.GetByIdAsync(shiftId).GetAwaiter().GetResult();
        var newStaff = staffRepository.GetByIdAsync(newStaffId).GetAwaiter().GetResult();

        if (shift is null || newStaff is null)
        {
            return false;
        }

        shift.Staff = newStaff;
        shiftRepository.UpdateAsync(shift).GetAwaiter().GetResult();
        return true;
    }

    public AutoAuditResult RunAutoAudit(DateTime weekStart)
    {
        var normalizedWeekStart = StartOfWeek(weekStart);
        var normalizedWeekEnd = normalizedWeekStart.AddDays(DaysInWeek);

        var allStaff = staffRepository.GetAllAsync().GetAwaiter().GetResult();
        var staffProfilesById = BuildStaffProfilesById(allStaff);

        var allShifts = shiftRepository.GetAllAsync().GetAwaiter().GetResult();
        var allRosterShifts = BuildAllRosterShifts(allShifts, staffProfilesById)
            .Where(IsAuditableShift)
            .ToList();

        bool OverlapsAuditWindow(RosterShift shift) => OverlapsWindow(shift, normalizedWeekStart, normalizedWeekEnd);
        DateTime ByStart(RosterShift shift) => shift.Start;
        int ByStaffId(RosterShift shift) => shift.StaffId;

        var weeklyShifts = allRosterShifts
            .Where(OverlapsAuditWindow)
            .OrderBy(ByStaffId)
            .ThenBy(ByStart)
            .ToList();

        var eligibleStaffProfiles = staffProfilesById.Values
            .Where(IsEligibleStaff)
            .ToList();

        var violations = new List<AuditViolation>();

        foreach (var staffShiftGroup in weeklyShifts.GroupBy(ByStaffId))
        {
            bool IsForCurrentGroup(RosterShift shift) => shift.StaffId == staffShiftGroup.Key;
            double WeekOverlapHours(RosterShift shift) => GetOverlapHours(shift.Start, shift.End, normalizedWeekStart, normalizedWeekEnd);

            var staffWeeklyShifts = staffShiftGroup.OrderBy(ByStart).ToList();
            var staffAllShifts = allRosterShifts.Where(IsForCurrentGroup).OrderBy(ByStart).ToList();
            var weeklyShiftIds = staffWeeklyShifts.Select(shift => shift.Id).ToHashSet();
            var totalHours = staffWeeklyShifts.Sum(WeekOverlapHours);

            if (totalHours > MaximumWeeklyHours)
            {
                foreach (var shift in staffWeeklyShifts)
                {
                    violations.Add(new AuditViolation
                    {
                        ShiftId = shift.Id,
                        StaffId = shift.StaffId,
                        StaffName = shift.StaffName,
                        ShiftStart = shift.Start,
                        ShiftEnd = shift.End,
                        Rule = MaximumWeeklyHoursViolationRule,
                        Message = $"Weekly total is {totalHours:F1}h (limit {MaximumWeeklyHours:F0}h).",
                    });
                }
            }

            for (int index = 1; index < staffAllShifts.Count; index++)
            {
                var previous = staffAllShifts[index - 1];
                var current = staffAllShifts[index];
                var restGap = current.Start - previous.End;

                if (restGap < MinimumRestGap && weeklyShiftIds.Contains(current.Id))
                {
                    violations.Add(new AuditViolation
                    {
                        ShiftId = current.Id,
                        StaffId = current.StaffId,
                        StaffName = current.StaffName,
                        ShiftStart = current.Start,
                        ShiftEnd = current.End,
                        Rule = MinimumRestViolationRule,
                        Message = $"Rest gap is {restGap.TotalHours:F1}h (minimum {MinimumRestGap.TotalHours:F0}h).",
                    });
                }
            }
        }

        string ByShiftIdAndRule(AuditViolation violation) => $"{violation.ShiftId}:{violation.Rule}";
        AuditViolation FirstInGroup(IGrouping<string, AuditViolation> group) => group.First();
        DateTime ByShiftStart(AuditViolation violation) => violation.ShiftStart;

        var dedupedViolations = violations
            .GroupBy(ByShiftIdAndRule)
            .Select(FirstInGroup)
            .OrderBy(ByShiftStart)
            .ToList();

        var suggestions = BuildSuggestions(dedupedViolations, normalizedWeekStart, weeklyShifts, allRosterShifts, eligibleStaffProfiles);

        bool hasConflicts = dedupedViolations.Count > 0;
        return new AutoAuditResult
        {
            WeekStart = normalizedWeekStart,
            HasConflicts = hasConflicts,
            Violations = dedupedViolations,
            Suggestions = suggestions,
            Summary = hasConflicts
                ? $"Found {dedupedViolations.Count} conflict(s). Publishing is blocked until resolved."
                : "No conflicts found. Roster can be published.",
        };
    }

    private static Dictionary<int, StaffProfile> BuildStaffProfilesById(IEnumerable<Staff> allStaff)
    {
        var profilesById = new Dictionary<int, StaffProfile>();
        foreach (var staffMember in allStaff)
        {
            profilesById[staffMember.StaffId] = new StaffProfile
            {
                StaffId = staffMember.StaffId,
                FullName = $"{staffMember.FirstName} {staffMember.LastName}".Trim(),
                Role = staffMember switch
                {
                    Doctor => DoctorRole,
                    Pharmacyst => PharmacistRole,
                    _ => string.Empty,
                },
                Specialization = staffMember switch
                {
                    Doctor doctor when !string.IsNullOrWhiteSpace(doctor.Specialization) => doctor.Specialization,
                    Pharmacyst pharmacist when !string.IsNullOrWhiteSpace(pharmacist.Certification) => pharmacist.Certification,
                    _ => DefaultSpecialization,
                },
                IsAvailable = staffMember.Available,
                IsActive = true,
                Status = staffMember is Doctor doctorMember ? doctorMember.DoctorStatus.ToString() : AvailableStaffStatus,
            };
        }

        return profilesById;
    }

    private static IReadOnlyList<RosterShift> BuildAllRosterShifts(IEnumerable<Shift> shifts, IReadOnlyDictionary<int, StaffProfile> staffProfilesById)
    {
        var rosterShifts = new List<RosterShift>();
        foreach (var shift in shifts)
        {
            int staffId = shift.Staff.StaffId;
            StaffProfile? profile = staffProfilesById.TryGetValue(staffId, out var found) ? found : null;
            rosterShifts.Add(new RosterShift
            {
                Id = shift.Id,
                StaffId = staffId,
                StaffName = profile?.FullName ?? string.Empty,
                Role = profile?.Role ?? string.Empty,
                Specialization = profile?.Specialization ?? DefaultSpecialization,
                Start = shift.StartTime,
                End = shift.EndTime,
                Status = shift.Status.ToString(),
            });
        }

        return rosterShifts;
    }

    private static List<AutoSuggestRecommendation> BuildSuggestions(
        IReadOnlyList<AuditViolation> violations,
        DateTime weekStart,
        IReadOnlyList<RosterShift> weeklyShifts,
        IReadOnlyList<RosterShift> allShifts,
        IReadOnlyList<StaffProfile> staffProfiles)
    {
        int RosterShiftKey(RosterShift shift) => shift.Id;
        RosterShift Identity(RosterShift shift) => shift;

        var weeklyShiftsById = weeklyShifts.ToDictionary(RosterShiftKey, Identity);
        var effectiveShifts = allShifts.Select(CloneShift).ToList();
        var recommendations = new List<AutoSuggestRecommendation>();

        foreach (var violation in violations)
        {
            if (!weeklyShiftsById.TryGetValue(violation.ShiftId, out var violatingShift))
            {
                continue;
            }

            bool MatchesViolatingId(RosterShift shift) => shift.Id == violatingShift.Id;
            var violatingShiftInPlan = effectiveShifts.FirstOrDefault(MatchesViolatingId) ?? violatingShift;

            bool IsNotCurrentStaff(StaffProfile profile) => profile.StaffId != violatingShiftInPlan.StaffId;
            bool HasMatchingRole(StaffProfile profile) => string.Equals(profile.Role, violatingShiftInPlan.Role, StringComparison.OrdinalIgnoreCase);
            bool HasMatchingSpecialization(StaffProfile profile) => string.Equals(profile.Specialization, violatingShiftInPlan.Specialization, StringComparison.OrdinalIgnoreCase);
            bool CanTakeViolatingShift(StaffProfile profile) => CanTakeShift(profile.StaffId, violatingShiftInPlan, effectiveShifts, weekStart);
            double GetMonthlyHours(StaffProfile profile) => GetMonthlyWorkedHoursFromShifts(profile.StaffId, violatingShiftInPlan.Start.Year, violatingShiftInPlan.Start.Month, effectiveShifts);
            string ByCandidateName(StaffProfile profile) => profile.FullName;

            var specializationCandidates = staffProfiles
                .Where(IsNotCurrentStaff)
                .Where(HasMatchingRole)
                .Where(HasMatchingSpecialization)
                .Where(IsAvailableForReassignment)
                .Where(CanTakeViolatingShift)
                .OrderBy(GetMonthlyHours)
                .ThenBy(ByCandidateName)
                .ToList();

            bool isRoleOnlyFallback = false;
            var candidates = specializationCandidates;
            if (candidates.Count == 0)
            {
                isRoleOnlyFallback = true;
                candidates = staffProfiles
                    .Where(IsNotCurrentStaff)
                    .Where(HasMatchingRole)
                    .Where(IsAvailableForReassignment)
                    .Where(CanTakeViolatingShift)
                    .OrderBy(GetMonthlyHours)
                    .ThenBy(ByCandidateName)
                    .ToList();
            }

            var bestCandidate = candidates.FirstOrDefault();
            if (bestCandidate is null)
            {
                recommendations.Add(new AutoSuggestRecommendation
                {
                    ShiftId = violatingShift.Id,
                    OriginalStaffId = violatingShift.StaffId,
                    OriginalStaffName = violatingShift.StaffName,
                    SuggestedStaffId = null,
                    SuggestedStaffName = string.Empty,
                    Reason = "No valid replacement found for this role under overlap/rest/hour limits.",
                });
                continue;
            }

            double monthlyHours = GetMonthlyWorkedHoursFromShifts(bestCandidate.StaffId, violatingShiftInPlan.Start.Year, violatingShiftInPlan.Start.Month, effectiveShifts);
            recommendations.Add(new AutoSuggestRecommendation
            {
                ShiftId = violatingShiftInPlan.Id,
                OriginalStaffId = violatingShiftInPlan.StaffId,
                OriginalStaffName = violatingShiftInPlan.StaffName,
                SuggestedStaffId = bestCandidate.StaffId,
                SuggestedStaffName = bestCandidate.FullName,
                Reason = isRoleOnlyFallback
                    ? $"Fallback to same role; lowest monthly load in eligible pool ({monthlyHours:F1}h)."
                    : $"Lowest monthly load in matching specialization pool ({monthlyHours:F1}h).",
            });

            ApplyTentativeReassignment(effectiveShifts, violatingShiftInPlan.Id, bestCandidate.StaffId, bestCandidate.FullName);
        }

        return recommendations;
    }

    private static bool CanTakeShift(int candidateStaffId, RosterShift proposed, IReadOnlyList<RosterShift> allShifts, DateTime weekStart)
    {
        if (HasOverlap(candidateStaffId, proposed, allShifts))
        {
            return false;
        }

        bool IsForCandidate(RosterShift shift) => shift.StaffId == candidateStaffId && shift.Id != proposed.Id;
        DateTime ByStart(RosterShift shift) => shift.Start;

        var candidateShifts = allShifts.Where(IsForCandidate).OrderBy(ByStart).ToList();

        bool EndsBeforeProposedStart(RosterShift shift) => shift.End <= proposed.Start;
        var previousShift = candidateShifts.LastOrDefault(EndsBeforeProposedStart);
        if (previousShift != null && (proposed.Start - previousShift.End) < MinimumRestGap)
        {
            return false;
        }

        bool StartsAfterProposedEnd(RosterShift shift) => shift.Start >= proposed.End;
        var nextShift = candidateShifts.FirstOrDefault(StartsAfterProposedEnd);
        if (nextShift != null && (nextShift.Start - proposed.End) < MinimumRestGap)
        {
            return false;
        }

        var weekEnd = weekStart.AddDays(DaysInWeek);
        double WeekOverlapHours(RosterShift shift) => GetOverlapHours(shift.Start, shift.End, weekStart, weekEnd);
        var existingHours = candidateShifts.Sum(WeekOverlapHours);
        var proposedHours = GetOverlapHours(proposed.Start, proposed.End, weekStart, weekEnd);
        return existingHours + proposedHours <= MaximumWeeklyHours;
    }

    private static void ApplyTentativeReassignment(IList<RosterShift> allShifts, int shiftId, int newStaffId, string newStaffName)
    {
        bool HasMatchingId(RosterShift shift) => shift.Id == shiftId;
        var shift = allShifts.FirstOrDefault(HasMatchingId);
        if (shift is null)
        {
            return;
        }

        shift.StaffId = newStaffId;
        shift.StaffName = newStaffName;
    }

    private static RosterShift CloneShift(RosterShift source) => new()
    {
        Id = source.Id,
        StaffId = source.StaffId,
        StaffName = source.StaffName,
        Role = source.Role,
        Specialization = source.Specialization,
        Start = source.Start,
        End = source.End,
        Status = source.Status,
    };

    private static bool IsAuditableShift(RosterShift shift) =>
        !string.Equals(shift.Status, CancelledShiftStatus, StringComparison.OrdinalIgnoreCase);

    private static bool IsEligibleStaff(StaffProfile staff)
    {
        bool isInactiveByStatus = string.Equals(staff.Status, InactiveStaffStatus, StringComparison.OrdinalIgnoreCase);
        return staff.IsActive != false && !isInactiveByStatus;
    }

    private static bool IsAvailableForReassignment(StaffProfile staff) => staff.IsAvailable != false;

    private static bool OverlapsWindow(RosterShift shift, DateTime windowStart, DateTime windowEnd) =>
        shift.Start < windowEnd && shift.End > windowStart;

    private static double GetMonthlyWorkedHoursFromShifts(int staffId, int year, int month, IReadOnlyList<RosterShift> shifts)
    {
        var monthStart = new DateTime(year, month, 1);
        var monthEnd = monthStart.AddMonths(1);
        bool IsForStaff(RosterShift shift) => shift.StaffId == staffId;
        double MonthOverlapHours(RosterShift shift) => GetOverlapHours(shift.Start, shift.End, monthStart, monthEnd);
        return shifts.Where(IsForStaff).Sum(MonthOverlapHours);
    }

    private static double GetOverlapHours(DateTime shiftStart, DateTime shiftEnd, DateTime windowStart, DateTime windowEnd)
    {
        var overlapStart = shiftStart > windowStart ? shiftStart : windowStart;
        var overlapEnd = shiftEnd < windowEnd ? shiftEnd : windowEnd;
        return overlapEnd <= overlapStart ? 0 : (overlapEnd - overlapStart).TotalHours;
    }

    private static bool HasOverlap(int candidateStaffId, RosterShift proposed, IReadOnlyList<RosterShift> allShifts)
    {
        bool OverlapsWithCandidate(RosterShift shift) =>
            shift.StaffId == candidateStaffId
            && shift.Id != proposed.Id
            && shift.Start < proposed.End
            && shift.End > proposed.Start;
        return allShifts.Any(OverlapsWithCandidate);
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        var daysFromMonday = (DaysInWeek + (date.DayOfWeek - DayOfWeek.Monday)) % DaysInWeek;
        return date.Date.AddDays(-daysFromMonday);
    }
}
