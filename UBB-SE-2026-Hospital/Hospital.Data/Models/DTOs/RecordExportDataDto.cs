using System.Collections.Generic;
using Hospital.Data.Models;

namespace Hospital.Data.Models.DTOs;

public class RecordExportData
{
    public MedicalRecord Record { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Prescription? Prescription { get; set; }
    public List<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
}
