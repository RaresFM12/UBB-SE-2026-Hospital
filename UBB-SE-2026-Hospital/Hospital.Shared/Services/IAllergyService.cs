using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface IAllergyService
{
    Task<List<Allergy>> GetAllAsync();
    Task<List<Allergy>> GetAllergiesAsync();
}
