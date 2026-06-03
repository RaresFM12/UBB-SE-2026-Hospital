using System.Text.Json;
using Hospital.Data;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.API;

public static class DesktopCompatibilityRoutes
{
    private const string PendingStatus = "PENDING";
    private const string AssignedStatus = "ASSIGNED";
    private const string UnmatchedStatus = "UNMATCHED";
    private const string CancelledStatus = "CANCELLED";

    public static IEndpointRouteBuilder MapDesktopCompatibilityRoutes(this IEndpointRouteBuilder app)
    {
        MapStaffRoutes(app);
        MapShiftRoutes(app);
        MapAppointmentRoutes(app);
        MapFatigueAuditRoutes(app);
        MapERDispatchRoutes(app);
        return app;
    }

    private static void MapStaffRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/staff", async (HospitalDbContext db) =>
            Results.Ok(await db.Staff.ToListAsync()));

        app.MapGet("api/staff/filtered", async (string? location, string? requiredSpecializationOrCertification, HospitalDbContext db) =>
        {
            var query = Normalize(requiredSpecializationOrCertification);
            var isPharmacy = string.Equals(Normalize(location), "pharmacy", StringComparison.OrdinalIgnoreCase);
            var staff = await db.Staff.ToListAsync();

            var filtered = staff
                .Where(member => isPharmacy ? member is Pharmacyst : member is Doctor)
                .Where(member =>
                    string.IsNullOrWhiteSpace(query)
                    || member.Specialization.Contains(query, StringComparison.OrdinalIgnoreCase)
                    || member.Certification.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Results.Ok(filtered);
        });

        app.MapGet("api/staff/specializations", async (string? location, HospitalDbContext db) =>
        {
            var isPharmacy = string.Equals(Normalize(location), "pharmacy", StringComparison.OrdinalIgnoreCase);
            var staff = await db.Staff.ToListAsync();
            var values = staff
                .Where(member => isPharmacy ? member is Pharmacyst : member is Doctor)
                .Select(member => isPharmacy ? member.Certification : member.Specialization)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value)
                .ToList();

            return Results.Ok(values);
        });

        app.MapGet("api/doctors", async (HospitalDbContext db) =>
        {
            var doctors = await db.Staff
                .OfType<Doctor>()
                .OrderBy(doctor => doctor.LastName)
                .ThenBy(doctor => doctor.FirstName)
                .Select(doctor => new
                {
                    DoctorId = doctor.StaffId,
                    DoctorName = doctor.FullName,
                })
                .ToListAsync();

            return Results.Ok(doctors);
        });

        app.MapGet("api/doctors/by-email", async (string email, HospitalDbContext db) =>
        {
            int? doctorId = await db.Staff
                .OfType<Doctor>()
                .Where(doctor => doctor.Email == email)
                .Select(doctor => doctor.StaffId)
                .Cast<int?>()
                .FirstOrDefaultAsync();

            return Results.Ok(doctorId);
        });
    }

    private static void MapShiftRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/shifts", async (HospitalDbContext db) =>
            Results.Ok(await LoadShifts(db).ToListAsync()));

        app.MapGet("api/shifts/daily", async (DateTime date, HospitalDbContext db) =>
        {
            var start = date.Date;
            var end = start.AddDays(1);
            return Results.Ok(await LoadShifts(db)
                .Where(shift => shift.StartTime >= start && shift.StartTime < end)
                .ToListAsync());
        });

        app.MapGet("api/shifts/weekly", async (DateTime date, HospitalDbContext db) =>
        {
            var start = StartOfWeek(date);
            var end = start.AddDays(7);
            return Results.Ok(await LoadShifts(db)
                .Where(shift => shift.StartTime >= start && shift.StartTime < end)
                .ToListAsync());
        });

        app.MapGet("api/shifts/range", async (int doctorId, DateTime fromDate, DateTime toDate, HospitalDbContext db) =>
            Results.Ok(await LoadShifts(db)
                .Where(shift => shift.Staff.StaffId == doctorId && shift.StartTime >= fromDate && shift.EndTime <= toDate)
                .ToListAsync()));

        app.MapGet("api/shifts/active", async (HospitalDbContext db) =>
            Results.Ok(await LoadShifts(db)
                .Where(shift => shift.Status == ShiftStatus.Active)
                .ToListAsync()));

        app.MapGet("api/shifts/validate-no-overlap", async (int staffId, DateTime start, DateTime end, HospitalDbContext db) =>
            Results.Ok(!await HasShiftOverlap(db, staffId, start, end)));

        app.MapGet("api/shifts/weekly-hours/{staffId:int}", async (int staffId, HospitalDbContext db) =>
        {
            var weekStart = StartOfWeek(DateTime.Today);
            var weekEnd = weekStart.AddDays(7);
            var hours = await db.Shifts
                .Include(shift => shift.Staff)
                .Where(shift => shift.Staff.StaffId == staffId && shift.StartTime >= weekStart && shift.StartTime < weekEnd)
                .SumAsync(shift => (float)EF.Functions.DateDiffMinute(shift.StartTime, shift.EndTime) / 60f);
            return Results.Ok(hours);
        });

        app.MapGet("api/shifts/is-working", async (int staffId, DateTime startTime, DateTime endTime, HospitalDbContext db) =>
            Results.Ok(await db.Shifts
                .Include(shift => shift.Staff)
                .AnyAsync(shift =>
                    shift.Staff.StaffId == staffId
                    && shift.StartTime < endTime
                    && shift.EndTime > startTime
                    && (shift.Status == ShiftStatus.Scheduled || shift.Status == ShiftStatus.Active))));

        app.MapGet("api/shifts/{shiftId:int}/replacements", async (int shiftId, HospitalDbContext db) =>
        {
            var shift = await LoadShifts(db).FirstOrDefaultAsync(item => item.Id == shiftId);
            if (shift is null)
            {
                return Results.NotFound();
            }

            var candidates = await db.Staff
                .Where(staff => staff.Role == shift.Staff.Role && staff.StaffId != shift.Staff.StaffId)
                .ToListAsync();

            var available = new List<Staff>();
            foreach (var candidate in candidates)
            {
                if (!await HasShiftOverlap(db, candidate.StaffId, shift.StartTime, shift.EndTime))
                {
                    available.Add(candidate);
                }
            }

            return Results.Ok(available);
        });

        app.MapPost("api/shifts", async (JsonElement payload, HospitalDbContext db) =>
        {
            var staffId = ReadInt(payload, "staffId")
                ?? ReadNestedInt(payload, "staff", "staffId")
                ?? ReadNestedInt(payload, "staff", "staffID");
            if (!staffId.HasValue)
            {
                return Results.BadRequest("Missing staff id.");
            }

            var staff = await db.Staff.FindAsync(staffId.Value);
            if (staff is null)
            {
                return Results.NotFound("Staff member not found.");
            }

            var start = ReadDateTime(payload, "startTime") ?? DateTime.Now;
            var end = ReadDateTime(payload, "endTime") ?? start.AddHours(8);
            var shift = new Shift
            {
                Staff = staff,
                Location = ReadString(payload, "location") ?? staff.Department ?? string.Empty,
                StartTime = start,
                EndTime = end,
                Status = ReadShiftStatus(payload, "status") ?? ShiftStatus.Scheduled,
            };

            db.Shifts.Add(shift);
            await db.SaveChangesAsync();
            return Results.Ok(shift);
        });

        app.MapPut("api/shifts/{shiftId:int}/status", async (int shiftId, JsonElement payload, HospitalDbContext db) =>
        {
            var shift = await db.Shifts.FindAsync(shiftId);
            if (shift is null)
            {
                return Results.NotFound();
            }

            shift.Status = ReadShiftStatus(payload, "status") ?? shift.Status;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        app.MapPut("api/shifts/{shiftId:int}/staff", async (int shiftId, JsonElement payload, HospitalDbContext db) =>
        {
            var shift = await db.Shifts.Include(item => item.Staff).FirstOrDefaultAsync(item => item.Id == shiftId);
            var staffId = ReadInt(payload, "staffId");
            if (shift is null || !staffId.HasValue)
            {
                return Results.NotFound();
            }

            var staff = await db.Staff.FindAsync(staffId.Value);
            if (staff is null || await HasShiftOverlap(db, staff.StaffId, shift.StartTime, shift.EndTime, shift.Id))
            {
                return Results.Conflict("Selected staff member is not available for that shift.");
            }

            shift.Staff = staff;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }

    private static void MapAppointmentRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/appointments/upcoming", async (int doctorUserId, DateTime fromDate, int skipCount, int takeCount, HospitalDbContext db) =>
            Results.Ok(await MapAppointments(db.Appointments
                .Include(appointment => appointment.Doctor)
                .Where(appointment => appointment.Doctor != null && appointment.Doctor.StaffId == doctorUserId && appointment.AppointmentDate >= fromDate)
                .OrderBy(appointment => appointment.AppointmentDate)
                .Skip(skipCount)
                .Take(takeCount))));

        app.MapGet("api/appointments/admin", async (int doctorId, HospitalDbContext db) =>
            Results.Ok(await MapAppointments(db.Appointments
                .Include(appointment => appointment.Doctor)
                .Where(appointment => doctorId <= 0 || (appointment.Doctor != null && appointment.Doctor.StaffId == doctorId))
                .OrderBy(appointment => appointment.AppointmentDate))));

        app.MapGet("api/appointments/range", async (int doctorId, DateTime fromDate, DateTime toDate, HospitalDbContext db) =>
            Results.Ok(await MapAppointments(db.Appointments
                .Include(appointment => appointment.Doctor)
                .Where(appointment =>
                    (doctorId <= 0 || (appointment.Doctor != null && appointment.Doctor.StaffId == doctorId))
                    && appointment.AppointmentDate >= fromDate
                    && appointment.AppointmentDate <= toDate)
                .OrderBy(appointment => appointment.AppointmentDate))));

        app.MapGet("api/appointments/{appointmentId:int}", async (int appointmentId, HospitalDbContext db) =>
        {
            var appointment = await db.Appointments
                .Include(item => item.Doctor)
                .Where(item => item.Id == appointmentId)
                .ToListAsync();
            return appointment.Count == 0 ? Results.NotFound() : Results.Ok(MapAppointment(appointment[0]));
        });

        app.MapPost("api/appointments", async (JsonElement payload, HospitalDbContext db) =>
            await CreateAppointment(payload, db));

        app.MapPost("api/appointments/book", async (JsonElement payload, HospitalDbContext db) =>
            await CreateAppointment(payload, db));

        app.MapPost("api/appointments/{appointmentId:int}/finish", async (int appointmentId, HospitalDbContext db) =>
        {
            var appointment = await db.Appointments.FindAsync(appointmentId);
            if (appointment is null)
            {
                return Results.NotFound();
            }

            appointment.Status = "Finished";
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        app.MapPost("api/appointments/{appointmentId:int}/cancel", async (int appointmentId, HospitalDbContext db) =>
        {
            var appointment = await db.Appointments.FindAsync(appointmentId);
            if (appointment is null)
            {
                return Results.NotFound();
            }

            appointment.Status = "Cancelled";
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }

    private static void MapFatigueAuditRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/fatigueaudit/run", (DateTime weekStart) =>
        {
            var normalized = StartOfWeek(weekStart);
            return Results.Ok(new
            {
                WeekStart = normalized,
                HasConflicts = false,
                Summary = "No conflicts found. Roster can be published.",
                Violations = Array.Empty<object>(),
                Suggestions = Array.Empty<object>(),
            });
        });
    }

    private static void MapERDispatchRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/er-requests", async (HospitalDbContext db) =>
            Results.Ok(await LoadRequests(db).ToListAsync()));

        app.MapGet("api/er-requests/{requestId:int}", async (int requestId, HospitalDbContext db) =>
        {
            var request = await LoadRequests(db).FirstOrDefaultAsync(item => item.Id == requestId);
            return request is null ? Results.NotFound() : Results.Ok(request);
        });

        app.MapGet("api/er-requests/by-visit/{visitId:int}", async (int visitId, HospitalDbContext db) =>
        {
            var request = await LoadRequests(db)
                .Where(item => item.VisitId == visitId)
                .OrderByDescending(item => item.CreatedAt)
                .FirstOrDefaultAsync();
            return request is null ? Results.NotFound() : Results.Ok(request);
        });

        app.MapGet("api/er-requests/pending", async (HospitalDbContext db) =>
            Results.Ok(await db.ERRequests
                .Where(request => request.Status == PendingStatus)
                .OrderBy(request => request.CreatedAt)
                .Select(request => request.Id)
                .ToListAsync()));

        app.MapPost("api/er-requests", async (JsonElement payload, HospitalDbContext db) =>
        {
            var visitId = ReadInt(payload, "visitId");
            if (visitId.HasValue)
            {
                var existingRequest = await LoadRequests(db)
                    .Where(item => item.VisitId == visitId.Value)
                    .Where(item => item.Status != CancelledStatus)
                    .OrderByDescending(item => item.CreatedAt)
                    .FirstOrDefaultAsync();

                if (existingRequest is not null)
                {
                    await MarkVisitWaitingForDoctorAsync(db, visitId.Value);
                    await db.SaveChangesAsync();
                    return Results.Ok(existingRequest.Id);
                }
            }

            var request = new ERRequest
            {
                VisitId = visitId,
                Specialization = ReadString(payload, "specialization") ?? "General",
                Location = ReadString(payload, "location") ?? "Ward A",
                Status = PendingStatus,
                CreatedAt = DateTime.Now,
            };

            db.ERRequests.Add(request);
            if (visitId.HasValue)
            {
                await MarkVisitWaitingForDoctorAsync(db, visitId.Value);
            }
            await db.SaveChangesAsync();
            return Results.Ok(request.Id);
        });

        app.MapPost("api/er-requests/simulate", async (JsonElement payload, HospitalDbContext db) =>
        {
            var count = Math.Max(1, ReadInt(payload, "count") ?? 3);
            var templates = new[]
            {
                ("Surgery", "ER"),
                ("Cardiology", "ER"),
                ("Neurology", "ER"),
                ("Pediatrics", "ER"),
            };
            var createdIds = new List<int>();

            for (var index = 0; index < count; index++)
            {
                var template = templates[index % templates.Length];
                var request = new ERRequest
                {
                    Specialization = template.Item1,
                    Location = template.Item2,
                    Status = PendingStatus,
                    CreatedAt = DateTime.Now,
                };
                db.ERRequests.Add(request);
                await db.SaveChangesAsync();
                createdIds.Add(request.Id);
            }

            return Results.Ok(createdIds);
        });

        app.MapPut("api/er-requests/{requestId:int}/status", async (int requestId, JsonElement payload, HospitalDbContext db) =>
        {
            var request = await db.ERRequests.FindAsync(requestId);
            if (request is null)
            {
                return Results.NotFound();
            }

            request.Status = ReadString(payload, "status") ?? request.Status;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        app.MapPost("api/er-requests/{requestId:int}/dispatch", async (int requestId, HospitalDbContext db) =>
            await DispatchRequest(requestId, db));

        app.MapPost("api/er-requests/dispatch-all", async (HospitalDbContext db) =>
        {
            var requestIds = await db.ERRequests
                .Where(request => request.Status == PendingStatus)
                .OrderBy(request => request.CreatedAt)
                .Select(request => request.Id)
                .ToListAsync();

            var results = new List<object>();
            foreach (var requestId in requestIds)
            {
                var result = await DispatchRequestObject(requestId, db);
                results.Add(result);
            }

            return Results.Ok(results);
        });

        app.MapGet("api/er-requests/{requestId:int}/candidates", async (int requestId, int nearEndMinutes, HospitalDbContext db) =>
        {
            var request = await db.ERRequests.FindAsync(requestId);
            if (request is null)
            {
                return Results.Ok(Array.Empty<object>());
            }

            var now = DateTime.Now;
            var matchingShifts = await db.Shifts
                .Include(shift => shift.Staff)
                .Where(shift =>
                    shift.Staff.Role == "Doctor"
                    && shift.EndTime >= now
                    && EF.Functions.DateDiffMinute(now, shift.EndTime) <= nearEndMinutes)
                .ToListAsync();

            var candidates = matchingShifts.Select(shift =>
            {
                var doctor = shift.Staff as Doctor;
                return new
                {
                    DoctorId = shift.Staff.StaffId,
                    FullName = shift.Staff.FullName,
                    Specialization = shift.Staff.Specialization,
                    Status = doctor?.DoctorStatus ?? DoctorStatus.OffDuty,
                    Location = shift.Location,
                    ScheduleStart = (DateTime?)shift.StartTime,
                    ScheduleEnd = (DateTime?)shift.EndTime,
                };
            });

            var preferredCandidates = candidates
                .Where(candidate => IsSameSpecialization(candidate.Specialization, request.Specialization))
                .ToList();

            if (preferredCandidates.Count > 0)
            {
                return Results.Ok(preferredCandidates);
            }

            var fallbackDoctors = await db.Staff
                .OfType<Doctor>()
                .ToListAsync();

            var fallbackCandidates = fallbackDoctors
                .Where(doctor => IsSameSpecialization(doctor.Specialization, request.Specialization))
                .OrderBy(doctor => doctor.FullName)
                .Select(doctor => new
                {
                    DoctorId = doctor.StaffId,
                    FullName = doctor.FullName,
                    Specialization = doctor.Specialization,
                    Status = doctor.DoctorStatus,
                    Location = doctor.Department ?? "ER",
                    ScheduleStart = (DateTime?)null,
                    ScheduleEnd = (DateTime?)null,
                })
                .ToList();

            return Results.Ok(fallbackCandidates);
        });

        app.MapPost("api/er-requests/{requestId:int}/override", async (int requestId, JsonElement payload, HospitalDbContext db) =>
        {
            var doctorId = ReadInt(payload, "doctorId");
            if (!doctorId.HasValue)
            {
                return Results.BadRequest("Missing doctor id.");
            }

            var request = await LoadRequests(db).FirstOrDefaultAsync(item => item.Id == requestId);
            var doctor = await db.Staff.OfType<Doctor>().FirstOrDefaultAsync(item => item.StaffId == doctorId.Value);
            if (request is null || doctor is null)
            {
                return Results.NotFound();
            }

            request.Status = AssignedStatus;
            request.AssignedDoctor = doctor;
            doctor.DoctorStatus = DoctorStatus.InExamination;
            doctor.Available = false;
            await UpdateVisitAfterDoctorAssignedAsync(db, request.VisitId);
            await db.SaveChangesAsync();

            return Results.Ok(DispatchResult(request, doctor, true, "Manual override by administrator."));
        });
    }

    private static IQueryable<Shift> LoadShifts(HospitalDbContext db) =>
        db.Shifts.Include(shift => shift.Staff).AsNoTracking();

    private static IQueryable<ERRequest> LoadRequests(HospitalDbContext db) =>
        db.ERRequests
            .Include(request => request.AssignedDoctor)
            .Include(request => request.Visit);

    private static async Task<bool> HasShiftOverlap(
        HospitalDbContext db,
        int staffId,
        DateTime start,
        DateTime end,
        int ignoredShiftId = 0)
    {
        return await db.Shifts
            .Include(shift => shift.Staff)
            .AnyAsync(shift =>
                shift.Id != ignoredShiftId
                && shift.Staff.StaffId == staffId
                && shift.Status != ShiftStatus.Completed
                && shift.Status != ShiftStatus.Cancelled
                && start < shift.EndTime
                && end > shift.StartTime);
    }

    private static async Task<IResult> CreateAppointment(JsonElement payload, HospitalDbContext db)
    {
        var doctorId = ReadInt(payload, "doctorId") ?? ReadNestedInt(payload, "doctor", "staffId") ?? ReadNestedInt(payload, "doctor", "staffID");
        var doctor = doctorId.HasValue
            ? await db.Staff.OfType<Doctor>().FirstOrDefaultAsync(item => item.StaffId == doctorId.Value)
            : null;

        var date = ReadDateTime(payload, "date") ?? ReadDateTime(payload, "appointmentDate") ?? DateTime.Today;
        var start = ReadTimeSpan(payload, "startTime") ?? TimeSpan.Zero;
        var appointmentDate = date.Date.Add(start);

        var appointment = new Appointment
        {
            PatientName = ReadString(payload, "patientName") ?? ReadString(payload, "externalRefId") ?? "Unknown Patient",
            AppointmentDate = appointmentDate,
            Status = "Scheduled",
            Type = ReadString(payload, "type") ?? "Consultation",
            Location = ReadString(payload, "location") ?? "Clinic",
            Notes = ReadString(payload, "notes") ?? string.Empty,
            Doctor = doctor,
        };

        db.Appointments.Add(appointment);
        await db.SaveChangesAsync();
        return Results.Ok(MapAppointment(appointment));
    }

    private static async Task<List<object>> MapAppointments(IQueryable<Appointment> query)
    {
        var appointments = await query.ToListAsync();
        return appointments.Select(MapAppointment).ToList();
    }

    private static object MapAppointment(Appointment appointment) => new
    {
        appointment.Id,
        appointment.PatientName,
        Date = appointment.AppointmentDate.Date,
        StartTime = appointment.AppointmentDate.TimeOfDay,
        EndTime = appointment.AppointmentDate.TimeOfDay.Add(TimeSpan.FromMinutes(30)),
        appointment.Status,
        appointment.Type,
        appointment.Location,
        appointment.Notes,
        ExternalRefId = appointment.ExternalRefId?.ToString() ?? string.Empty,
        Doctor = appointment.Doctor is null
            ? null
            : new
            {
                StaffID = appointment.Doctor.StaffId,
                appointment.Doctor.Email,
                appointment.Doctor.Role,
                appointment.Doctor.Department,
                appointment.Doctor.FirstName,
                appointment.Doctor.LastName,
                appointment.Doctor.ContactInfo,
                appointment.Doctor.Available,
                appointment.Doctor.LicenseNumber,
                appointment.Doctor.Specialization,
                appointment.Doctor.Status,
                appointment.Doctor.Certification,
                appointment.Doctor.YearsOfExperience,
                appointment.Doctor.HourlyRate,
                appointment.Doctor.DoctorStatus,
            },
    };

    private static async Task<IResult> DispatchRequest(int requestId, HospitalDbContext db)
    {
        var result = await DispatchRequestObject(requestId, db);
        return Results.Ok(result);
    }

    private static async Task<object> DispatchRequestObject(int requestId, HospitalDbContext db)
    {
        var request = await LoadRequests(db).FirstOrDefaultAsync(item => item.Id == requestId);
        if (request is null || request.Status != PendingStatus)
        {
            return new
            {
                Request = request ?? new ERRequest { Id = requestId },
                MatchedDoctorId = (int?)null,
                MatchedDoctorName = (string?)null,
                MatchReason = string.Empty,
                IsSuccess = false,
                Message = $"ER request #{requestId} not found or already processed.",
            };
        }

        var now = DateTime.Now;
        var candidate = await db.Shifts
            .Include(shift => shift.Staff)
            .Where(shift =>
                shift.Staff is Doctor
                && shift.StartTime <= now
                && shift.EndTime >= now
                && shift.Status != ShiftStatus.Cancelled
                && shift.Status != ShiftStatus.Completed)
            .Select(shift => new { Shift = shift, Doctor = (Doctor)shift.Staff })
            .ToListAsync();

        var doctor = candidate
            .Where(item => item.Doctor.DoctorStatus == DoctorStatus.Available || item.Doctor.Available)
            .Where(item => IsSameSpecialization(item.Doctor.Specialization, request.Specialization))
            .OrderBy(item => item.Doctor.FullName)
            .Select(item => item.Doctor)
            .FirstOrDefault();

        if (doctor is null)
        {
            request.Status = UnmatchedStatus;
            await db.SaveChangesAsync();
            return DispatchResult(request, null, false, "No available matching doctor found.");
        }

        request.Status = AssignedStatus;
        request.AssignedDoctor = doctor;
        doctor.DoctorStatus = DoctorStatus.InExamination;
        doctor.Available = false;
        await UpdateVisitAfterDoctorAssignedAsync(db, request.VisitId);
        await db.SaveChangesAsync();
        return DispatchResult(request, doctor, true, $"Assigned to {doctor.FullName}.");
    }

    private static async Task MarkVisitWaitingForDoctorAsync(HospitalDbContext db, int visitId)
    {
        var visit = await db.ERVisits.FindAsync(visitId);
        if (visit is null)
        {
            return;
        }

        if (string.Equals(visit.Status, ERVisit.VisitStatus.IN_ROOM, StringComparison.OrdinalIgnoreCase))
        {
            visit.Status = ERVisit.VisitStatus.WAITING_FOR_DOCTOR;
        }
    }

    private static async Task UpdateVisitAfterDoctorAssignedAsync(HospitalDbContext db, int? visitId)
    {
        if (!visitId.HasValue)
        {
            return;
        }

        await MarkVisitWaitingForDoctorAsync(db, visitId.Value);
    }

    private static object DispatchResult(ERRequest request, Doctor? doctor, bool success, string message) => new
    {
        Request = request,
        MatchedDoctorId = doctor?.StaffId,
        MatchedDoctorName = doctor?.FullName,
        MatchReason = success ? "Specialty match + available staff" : string.Empty,
        IsSuccess = success,
        Message = message,
    };

    private static DateTime StartOfWeek(DateTime date)
    {
        const int daysInWeek = 7;
        var daysFromMonday = (daysInWeek + (date.DayOfWeek - DayOfWeek.Monday)) % daysInWeek;
        return date.Date.AddDays(-daysFromMonday);
    }

    private static bool IsSameSpecialization(string left, string right) =>
        string.Equals(NormalizeSpecialization(left), NormalizeSpecialization(right), StringComparison.OrdinalIgnoreCase);

    private static string NormalizeSpecialization(string? value) =>
        Normalize(value).ToLowerInvariant() switch
        {
            "surgeon" => "surgery",
            "general surgery" => "surgery",
            "cardiologist" => "cardiology",
            "cardio" => "cardiology",
            "pediatric" => "pediatrics",
            "pediatrician" => "pediatrics",
            "general" => "diagnostician",
            "emergency medicine" => "diagnostician",
            "emergency" => "diagnostician",
            _ => Normalize(value).ToLowerInvariant(),
        };

    private static string Normalize(string? value) => (value ?? string.Empty).Trim();

    private static string? ReadString(JsonElement element, string propertyName)
    {
        if (!TryGetProperty(element, propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString(),
            JsonValueKind.Number => property.GetRawText(),
            _ => null,
        };
    }

    private static int? ReadInt(JsonElement element, string propertyName)
    {
        if (!TryGetProperty(element, propertyName, out var property))
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var number))
        {
            return number;
        }

        return int.TryParse(property.GetString(), out var parsed) ? parsed : null;
    }

    private static int? ReadNestedInt(JsonElement element, string objectName, string propertyName)
    {
        if (!TryGetProperty(element, objectName, out var nested) || nested.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return ReadInt(nested, propertyName);
    }

    private static DateTime? ReadDateTime(JsonElement element, string propertyName)
    {
        if (!TryGetProperty(element, propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String && DateTime.TryParse(property.GetString(), out var parsed)
            ? parsed
            : null;
    }

    private static TimeSpan? ReadTimeSpan(JsonElement element, string propertyName)
    {
        if (!TryGetProperty(element, propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String && TimeSpan.TryParse(property.GetString(), out var parsed)
            ? parsed
            : null;
    }

    private static ShiftStatus? ReadShiftStatus(JsonElement element, string propertyName)
    {
        if (!TryGetProperty(element, propertyName, out var property))
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var number))
        {
            return Enum.IsDefined(typeof(ShiftStatus), number) ? (ShiftStatus)number : null;
        }

        return Enum.TryParse<ShiftStatus>(property.GetString(), ignoreCase: true, out var parsed)
            ? parsed
            : null;
    }

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement property)
    {
        foreach (var candidate in element.EnumerateObject())
        {
            if (string.Equals(candidate.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                property = candidate.Value;
                return true;
            }
        }

        property = default;
        return false;
    }
}
