using System;

namespace Hospital.Data.Models;

public class MedicalEvaluation
{
    public int EvaluationID { get; set; }
    public int? EvaluatorId { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public string Symptoms { get; set; } = string.Empty;
    public string MedicationsList { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime EvaluationDate { get; set; }
    public Doctor? Evaluator { get; set; }

    public string FormattedDate => EvaluationDate.ToString("dd/MM/yyyy");
    public string FormattedTime => EvaluationDate.ToString("HH:mm");
}
