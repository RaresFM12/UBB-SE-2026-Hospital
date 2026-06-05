using Hospital.Data.Models;

namespace Hospital.Services;

public interface IAllergyService
{
    Task<List<Allergy>> GetAllergiesAsync();
}
