using System;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Data.Models;

public class TriageParameters
{
    public int TriageParametersId { get; set; }
    public int TriageId { get; set; }

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
        if (Consciousness < 1 || Consciousness > 3)
            throw new ArgumentOutOfRangeException(nameof(Consciousness), "Consciousness must be between 1 and 3.");
        if (Breathing < 1 || Breathing > 3)
            throw new ArgumentOutOfRangeException(nameof(Breathing), "Breathing must be between 1 and 3.");
        if (Bleeding < 1 || Bleeding > 3)
            throw new ArgumentOutOfRangeException(nameof(Bleeding), "Bleeding must be between 1 and 3.");
        if (InjuryType < 1 || InjuryType > 3)
            throw new ArgumentOutOfRangeException(nameof(InjuryType), "InjuryType must be between 1 and 3.");
        if (PainLevel < 1 || PainLevel > 3)
            throw new ArgumentOutOfRangeException(nameof(PainLevel), "PainLevel must be between 1 and 3.");
    }
}
