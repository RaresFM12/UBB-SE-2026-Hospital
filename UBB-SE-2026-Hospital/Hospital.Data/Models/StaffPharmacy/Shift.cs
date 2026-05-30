using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Hospital.Data.Models;

public class Shift
{
    public int Id { get; set; }
    public Staff Staff { get; set; } = null!;

    [NotMapped]
    [JsonIgnore]
    public IStaff AppointedStaff
    {
        get => Staff;
        set => Staff = value as Staff
            ?? throw new ArgumentException("AppointedStaff must be a concrete Staff instance.", nameof(value));
    }

    public string Location { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public ShiftStatus Status { get; set; } = ShiftStatus.SCHEDULED;

    public Shift() { }

    public Shift(int id, IStaff appointedStaff, string location, DateTime startTime, DateTime endTime, ShiftStatus status)
    {
        Id = id;
        AppointedStaff = appointedStaff;
        Location = location;
        StartTime = startTime;
        EndTime = endTime;
        Status = status;
    }
}
