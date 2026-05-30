using System;

namespace Hospital.Data.Models;

public enum ShiftSwapRequestStatus
{
    PENDING,
    ACCEPTED,
    REJECTED,
    CANCELLED,
}

public class ShiftSwapRequest
{
    public int SwapId { get; set; }
    public int? ShiftId { get; set; }
    public int? RequestingStaffId { get; set; }
    public int? TargetStaffId { get; set; }
    public DateTime RequestedAt { get; set; }
    public ShiftSwapRequestStatus Status { get; set; } = ShiftSwapRequestStatus.PENDING;
    public Shift? Shift { get; set; }
    public Staff? Requester { get; set; }
    public Staff? Colleague { get; set; }
}
