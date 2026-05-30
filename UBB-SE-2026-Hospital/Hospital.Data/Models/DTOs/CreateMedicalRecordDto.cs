using System;
using Hospital.Data.Models;

namespace Hospital.Data.Models.DTOs;

public class CreateMedicalRecordDto
{
    public SourceType SourceType { get; set; }
    public int SourceId { get; set; }
    public int StaffId { get; set; }
    public string? Symptoms { get; set; }
    public string? Diagnosis { get; set; }
    public DateTime ConsultationDate { get; set; }
    public decimal BasePrice { get; set; }
    public decimal FinalPrice { get; set; }
    public bool PoliceNotified { get; set; }
}
