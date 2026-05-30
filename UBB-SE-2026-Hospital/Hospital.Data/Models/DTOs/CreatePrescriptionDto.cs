using System;
using System.Collections.Generic;

namespace Hospital.Data.Models.DTOs;

public class CreatePrescriptionRequest
{
    public string? DoctorNotes { get; set; }
    public DateTime Date { get; set; }
    public List<CreatePrescriptionItemRequest> Items { get; set; } = new List<CreatePrescriptionItemRequest>();
}
