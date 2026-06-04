using Hospital.Data.Models;

namespace Hospital.Services.PatientEr;

public interface IAllergyService
{
    Task<List<Allergy>> GetAllergiesAsync();
}
