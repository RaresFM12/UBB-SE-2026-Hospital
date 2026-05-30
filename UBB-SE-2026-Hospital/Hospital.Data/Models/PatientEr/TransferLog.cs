using System;

namespace Hospital.Data.Models;

public class TransferLog
{
    public int TransferLogId { get; set; }
    public ERVisit Visit { get; set; } = null!;
    public DateTime TransferTime { get; set; }
    public string TargetSystem { get; set; } = string.Empty;
    public string? FilePath { get; set; }

    private string _status = "RETRYING";
    public string Status
    {
        get => _status;
        set
        {
            if (value != "SUCCESS" && value != "FAILED" && value != "RETRYING")
                throw new ArgumentException($"Invalid status '{value}'. Allowed: SUCCESS, FAILED, RETRYING.");
            _status = value;
        }
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(TargetSystem))
            throw new ArgumentException("TargetSystem must not be empty.");
        if (Visit is null)
            throw new ArgumentException("Visit must not be null.");
    }
}
