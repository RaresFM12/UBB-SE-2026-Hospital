using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface IAllergyService
{
    Task<List<Allergy>> GetAllergiesAsync();
}
