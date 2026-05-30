using System;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Data.Models;

public class TriageParameters
{
    public int TriageParametersId { get; set; }
    public Triage Triage { get; set; } = null!;

    [Range(1, 3)]
    public int Consciousness { get; set; }
    [Range(1, 3)]
    public int Breathing { get; set; }
    [Range(1, 3)]
    public int Bleeding { get; set; }
    [Range(1, 3)]
    public int InjuryType { get; set; }
    [Range(1, 3)]
    public int PainLevel { get; set; }

    public void ValidateParameters()
    {
        const int MinParameterValue = 1;
        const int MaxParameterValue = 3;

        if (Consciousness < MinParameterValue || Consciousness > MaxParameterValue)
            throw new ArgumentOutOfRangeException(nameof(Consciousness), $"Consciousness must be between {MinParameterValue} and {MaxParameterValue}.");
        if (Breathing < MinParameterValue || Breathing > MaxParameterValue)
            throw new ArgumentOutOfRangeException(nameof(Breathing), $"Breathing must be between {MinParameterValue} and {MaxParameterValue}.");
        if (Bleeding < MinParameterValue || Bleeding > MaxParameterValue)
            throw new ArgumentOutOfRangeException(nameof(Bleeding), $"Bleeding must be between {MinParameterValue} and {MaxParameterValue}.");
        if (InjuryType < MinParameterValue || InjuryType > MaxParameterValue)
            throw new ArgumentOutOfRangeException(nameof(InjuryType), $"InjuryType must be between {MinParameterValue} and {MaxParameterValue}.");
        if (PainLevel < MinParameterValue || PainLevel > MaxParameterValue)
            throw new ArgumentOutOfRangeException(nameof(PainLevel), $"PainLevel must be between {MinParameterValue} and {MaxParameterValue}.");
    }
}
